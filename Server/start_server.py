import asyncio
import socket
from typing import Tuple
from config import *


class Handler:
    '''Handler for incoming connection'''
    def __init__(self):
        self._r = None
        self._w = None
        self._w_addr = None

    async def create(self, reader, writer):
        self._r = reader
        self._w = writer
        self._w_addr = self._w.get_extra_info("peername")
        await self.handle_read()
        await self.handle_write()

    async def handle_read(self):
        print(f"Reading from {self._w_addr}...")
        data = await self._r.readuntil(SEPARATOR)
        data = data.decode().strip()
        comm, message = data.split("\n\n")
        print(f"Got new command {comm} message {message} from {self._w_addr}")
        # TODO: Handle asyncio.LimitOverrunError & asyncio.IncompleteReadError

    async def handle_write(self):
        print(f"Writing to {self._w_addr}...")
        self._w.write("message from server".encode())
        await self._w.drain()
        self._w.close()


async def main():
    server = await asyncio.start_server(Handler().create, *ADDRESS)

    print(f"Serving on {ADDRESS}")

    async with server:
        await server.serve_forever()

if __name__ == '__main__':
    asyncio.run(main())