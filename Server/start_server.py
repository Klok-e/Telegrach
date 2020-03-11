import asyncio
# import logging
import socket
import sys
import crypto
import controllers as ctrl
import signals_pb2 as signals
from typing import Tuple
from config import *
from database import DataBase

# TODO LOG ALL EXCEPTIONS WITH sys.exc_info

TEST_KEY = b'XP4VTC3mrE-84R4xFVVDBXZFnQo4jf1i'



# logging.basicConfig(filename=LOG_FILE_SERVER,
#                     level=LOG_LEVEL_SERVER,
#                     format=LOG_FORMAT_SERVER)


async def create_user(db, message):
    ''' 
        Returns new_users with code data and commits its data to database
    '''
    # message = crypto.decrypt_message(TEST_KEY, message)
    request = signals.user_creation_request()
    request.ParseFromString(message)

    if request.super_id == -1: # That part of code required if user is using the service for the first time, so super_account does not exists
        select = await db.get_max_super_id()
        super_id = dict(select.items())["max"] + 1
        await db.create_new_super_account(super_id) # creating super_account before user_account

    data: Tuple[Tuple[str, str], Dict] = ctrl.create_user(super_id) # a tuple(User`s data to send, Database data to store)
    await db.create_new_user(data[1])

    return 1, data[0]


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

async def send_users_data(data):
    users_data = signals.send_user_to_client()
    users_data.login = data[0]
    users_data.password = data[1]
    response = users_data.SerializeToString()
    response = b"CODE=1\n\n" + response + b"\n\n\n\n"
    return response

CODES = {
    0: create_user,
    1: send_users_data,
}


async def parse_input(db, data):
    commands, message = data.strip().split(b"\n\n")
    commands = commands.split(b"\n")
    commands = dict(i.split(b"=") for i in commands)
    code = int(commands[b"CODE"])
    return await CODES[code](db, message)


async def make_output(db, data: Tuple[int, bytes]):
    code = data[0]
    response = data[1]
    return await CODES[code](response)


async def handle_read(db, reader: asyncio.StreamReader, sockname: Tuple[str, int]):
    # logging.info(f"Reading from {sockname}...")
    data = await reader.readuntil(SEPARATOR)
    return await parse_input(db, data)


async def handle_write(db, writer: asyncio.StreamWriter, sockname: Tuple[str, int], message: Tuple[int, bytes]):
    # logging.info(f"Writing to {sockname}")
    response = await make_output(db, message)
    writer.write(response)
    await writer.drain()
    # logging.info(f"Message sended to {sockname}...")


async def handler(db, reader: asyncio.StreamReader, writer: asyncio.StreamWriter):
    sockname = writer.get_extra_info('peername')
    # logging.info(f"New connection from {sockname}")
    result = await handle_read(db, reader, sockname)
    await handle_write(db, writer, sockname, result)
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
    server = await asyncio.start_server(lambda r, w: handler(db, r, w), *ADDRESS)

    async with server:
        await server.serve_forever()

    await db.disconnect()


if __name__ == '__main__':
    asyncio.run(main())