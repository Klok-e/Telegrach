'''
Simple TCP listener
Things to discuss: how to split the messages?
My assumption: the message consists of two part:
1 - commands
2 - data or values (depending on commands)
Message must be ended by \n\n\n\n. Commands and data are separated by \n\n
The inner of both commads must be separated just by \n
In data we have no separates'''

import asyncio
import asyncore # https://docs.python.org/3.0/library/asyncore.html
import asynchat # https://docs.python.org/3.0/library/asynchat.html
import socket
from typing import Tuple
from config import *

class Server(asyncore.dispatcher):
    '''Receives connections and establishes handlers for each client.'''

    def __init__(self, address: Tuple[str, int]):
        asyncore.dispatcher.__init__(self)
        self.create_socket(socket.AF_INET, socket.SOCK_STREAM)
        self.bind(ADDRESS)
        self.addr = self.socket.getsockname()
        self.listen(256)

    def handle_accept(self):
        '''Called when a client connects to server socket '''
        client = self.accept() # Accepting new connetion. Returns a tuple (conn_socket, conn_address)
        return Handler(sock=client[0]) # Creates a Handler that handles the client streams
    
    def handle_close(self):
        '''Just closing the server2'''
        self.close()


class Handler(asynchat.async_chat):
    '''Handles clients Streams.'''
    
    ac_in_buffer_size = BUFFSIZE
    ac_out_buffer_size = BUFFSIZE

    def __init__(self, sock: socket.Socket):
        self.buffer = []
        asynchat.async_chat.__init__(self, sock)
        print(f"New connection from {sock.getsockname()}")
        self.set_terminator(b'\n\n\n\n') # the message must ends with this. Described in 2nd line

    def collect_incoming_data(self, data: bytes):
        '''Read an incoming message from the client and write it to buffer.'''
        print(f"Collecting {data}")
        self.buffer.append(data)

    def found_terminator(self):
        '''The end of a command or message has been seen.'''
        self.process_incoming_data()

    def process_incoming_data(self):
        data = b''.join(self.buffer)
        print(data)
        commands, message = data.decode().split('\n\n')
        print(f"Commands: {commands}")
        print(f"Message: {message}")
        self.push("Message from server".encode())


# To test out run file and try to establish some tcp connections via commands like netcat

server = Server(ADDRESS)

asyncore.loop()