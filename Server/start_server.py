import asyncio
import logging
import socket
import sys
from typing import Tuple
from config import *

# TODO LOG ALL EXCEPTIONS WITH sys.exc_info


logging.basicConfig(filename=LOG_FILE_SERVER,
                    level=LOG_LEVEL_SERVER,
                    format=LOG_FORMAT_SERVER)



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
    server = await asyncio.start_server(handler, *ADDRESS)

    async with server:
        await server.serve_forever()

if __name__ == '__main__':
    asyncio.run(main())