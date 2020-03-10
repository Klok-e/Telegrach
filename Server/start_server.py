import asyncio
import logging
import socket
import sys
from typing import Tuple
from config import *
import controllers as ctrl
from database import DataBase

# TODO LOG ALL EXCEPTIONS WITH sys.exc_info


logging.basicConfig(filename=LOG_FILE_SERVER,
                    level=LOG_LEVEL_SERVER,
                    format=LOG_FORMAT_SERVER)


async def create_user(db, super_id: int=None):
    ''' 
        Returns new_users data and commits its data to database
    '''
    if super_id is None: # That part of code required if user is using the service for the first time, so super_account does not exist
        select = await db.get_max_super_id()
        super_id = dict(select.items())["max"] + 1
        await db.create_new_super_account(super_id) # creating super_account before user_account
    data: Tuple[Tuple[str, str], Dict] = ctrl.create_user(super_id) # a tuple(User`s data to send, Database data to store)
    await db.create_new_user(data[1])
    return data[0]


async def create_message(db, login: str, tred_id: int, message: str):
    values = ctrl.create_message(login, tred_id, message)
    await db.create_new_message(values)


async def create_tred(db, creator_id: int, header: str, body: str):
    values = ctrl.create_tred(creator_id, header, body)
    await db.create_new_tred(values)


async def create_tred_participation(db, tred_id: int, super_id: int):
    values = ctrl.create_tred_participation(tred_id, super_id)
    await db.create_new_tred_participation(values)


async def create_union_request(db, _from: int, to: int):
    values = ctrl.create_union_request(_from, to)
    await db.create_new_union_request(values)


async def create_personal_list(db, list_name: str, owner_id: int):
    values = ctrl.create_personal_list(list_name, owner_id)
    await db.create_new_personal_list(values)


async def create_people_inlist(db, list_id: int, friend_id: int):
    values = ctrl.create_people_inlist(list_id, friend_id)
    await db.create_new_people_inlist(values)


async def handle_read(reader: asyncio.StreamReader, sockname: Tuple[str, int]):
    logging.info(f"Reading from {sockname}...")
    data = await reader.readuntil(SEPARATOR)
    data = data.decode()
    commands, message = data.strip().split("\n\n")
    logging.info(f"Got request from {sockname}")
    logging.info(f"Command is {commands}")
    logging.info(f"Message if {message}")


async def handle_write(writer: asyncio.StreamWriter, sockname: Tuple[str, int]):
    logging.info(f"Writing to {sockname}")
    writer.write("Message from server".encode())
    await writer.drain()
    logging.info(f"Message sended to {sockname}...")


async def handler(reader: asyncio.StreamReader, writer: asyncio.StreamWriter):
    sockname = writer.get_extra_info('peername')
    logging.info(f"New connection from {sockname}")

    try:
        await handle_read(reader, sockname)
    except asyncio.LimitOverrunError as e:
        logging.error(str(e))
        
    except asyncio.IncompleteReadError as e:
        logging.error(str(e))

    except Exception as e:
        logging.error(str(e))

    try:
        await handle_write(writer, sockname)
    except:
        logging.error(f"Got {str(e)}")

    writer.close()


async def main():

    db = DataBase(DB_USER, DB_PW, (DB_HOST, DB_PORT), SCHEMA_NAME, VOCAB)
    await db.connect(DB)

    # await create_message(db, "1691eedc-43f0-4824-8c5c-d80445db99bb", 1, "wpowwowow")
    # await create_tred(db, 1, "test_header_from_code", "test_body_from_code")
    # await create_tred_participation(db, 1, 1)
    # await create_union_request(db, 1, 1)
    # await create_personal_list(db, "test_list_from_code", 1)
    # await create_people_inlist(db, 1, 1)
    # server = await asyncio.start_server(handler, *ADDRESS)

    # async with server:
    #     await server.serve_forever()
    await db.disconnect()

if __name__ == '__main__':
    asyncio.run(main())