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

async def create_user(db, super_id=None):
    ''' 
        Returns new_users data and commits its data to database
    '''
    if super_id is None: # That part of code required if user is using the service for the first time, so super_account does not exist
        select = await db.get_max_super_id()
        super_id = dict(select.items())["max"] + 1
        await db.create_new_super_account(super_id) # creating super_account before user_account
    data: Tuple[Tuple[str, str], Dict] = ctrl.create_user(super_id)
    await db.create_new_user(data[1])
    return data[0]


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

    await create_user(db, 3)
    # server = await asyncio.start_server(handler, *ADDRESS)

    # async with server:
    #     await server.serve_forever()
    await db.disconnect()

if __name__ == '__main__':
    asyncio.run(main())