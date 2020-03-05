import asyncio
import socket
from typing import Tuple
from config import *


async def handle_read(reader: asyncio.StreamReader, sockname: Tuple[str, int]):
    try:
        data = await reader.readuntil(SEPARATOR)
        print('read')
        data = data.decode()
        print(data)
        commands, message = data.strip().split("\n\n")
        print(f"Got request from {sockname}")
        print(f"Command is {commands}")
        print(f"Message if {message}")
    except asyncio.LimitOverrunError:
        print("LimitOverrunError")

    except asyncio.IncompleteReadError:
        print("IncompleteReadError")

    except Exception as e:
        print(str(e))
        raise e


async def handle_write(writer: asyncio.StreamWriter, sockname: Tuple[str, int]):
    print(f"Writing to {sockname}")
    try:
        writer.write("Message from server".encode())
        await writer.drain()
        print("Message sended...")
    except:
        print("handle_write Error")


async def handler(reader: asyncio.StreamReader, writer: asyncio.StreamWriter):
    sockname = writer.get_extra_info('peername')
    print(f"New connection from {sockname}")

    try:
        await handle_read(reader, sockname)
    except asyncio.LimitOverrunError:
        print("LimitOverrunError")
        
    except asyncio.IncompleteReadError:
        print("IncompleteReadError")

    except Exception as e:
        print(str(e))

    try:
        await handle_write(writer, sockname)
    except:
        print("TroubleShouting with writing")

    writer.close()


async def main():
    server = await asyncio.start_server(handler, *ADDRESS)

    async with server:
        await server.serve_forever()

if __name__ == '__main__':
    asyncio.run(main())