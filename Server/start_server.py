import asyncio
# import logging
import socket
import sys
import os
import crypto
import controllers as ctrl
import proto.signals_pb2 as signals
from typing import Tuple
from config import *
from database import DataBase

# TODO LOG ALL EXCEPTIONS WITH sys.exc_info

TEST_KEY = b'XP4VTC3mrE-84R4xFVVDBXZFnQo4jf1i'
CODES = {}


# logging.basicConfig(filename=LOG_FILE_SERVER,
#                     level=LOG_LEVEL_SERVER,
#                     format=LOG_FORMAT_SERVER)


def code(code, *args, **kwargs):
    '''
        A decorator which associates fynction with specified code recieved from clients
    '''

    def inner(f):
        CODES[code] = f
        return f

    return inner


@code(0)
async def create_user(db, message):
    '''
        Returns new_users with code data and commits its data to database
        Works lil bit different from other similar creation functions
        I super_id is provided it will link users together
        Either it will create new super_account and link new user with it
    '''
    request = signals.user_creation_request()
    request.ParseFromString(message)

    if request.super_id == - \
            1:  # That part of code required if user is using the service for the first time, so super_account does not exists
        select = await db.get_max_super_id()
        super_id = dict(select.items())["max"] + 1
        # creating super_account before user_account
        await db.create_new_super_account(super_id)

    # a tuple(User`s data to send, Database data to store)
    data: Tuple[Tuple[str, str], Dict] = ctrl.create_user(super_id)
    await db.create_new_user(data[1])

    return 1, data[0]


@code(1)
async def send_users_data(data):
    '''
        This function sends user`s generated data.
    '''
    response = signals.send_user_to_client()
    response.login = data[0]
    response.password = data[1]
    response = response.SerializeToString()
    result = b"CODE=1\n\n" + response + b"\n\n\n\n"
    return result


@code(2)
async def get_all_messages_from_tred(db, message):
    '''
        Function returns all message in tred by one query.
        It assumes the
    '''
    request = signals.get_tred_data()
    request.ParseFromString(message)
    result = await db.all_messages_in_tred(request.tred_id)
    result = [dict(i.items()) for i in result]
    tred_creator, tred_time = result[0]["creator_id"], str(
        result[0]["tred_time"])
    tred_head, tred_body = result[0]["head_tred"], result[0]["tred_body"]
    for i in result:
        i.pop("creator_id")
        i.pop("tred_time")
        i.pop("head_tred")
        i.pop("tred_body")
        i["message_time"] = str(i["message_time"])
        i["author_login"] = str(i["author_login"])
    return (3, ((tred_creator, tred_time, tred_head, tred_body), result))


@code(3)
async def send_all_messages_from_tred(data):
    """
        Recieves a pair:
         1 - Tuple with the tred info
         2 - List of Dicts with specified field for each message
         For polymorfical reasons it`s need to be a coroutine
    """
    response = signals.messages_in_tred()
    response.tred_creator = data[0][0]
    response.tred_time = data[0][1]
    response.tred_head = data[0][2]
    response.tred_body = data[0][3]
    for i in data[1]:
        message = response.messages.add()
        message.author_login = i["author_login"]
        message.message_time = i["message_time"]
        message.message_body = i["message_body"]
    response = response.SerializeToString()
    result = b"CODE=3\n\n" + response + b"\n\n\n\n"
    return result


@code(4)
async def create_tred(db, message):
    '''
        This fucntion is similar to the other ones, so ill explain everything common here
        Every function, that processes input and generates output must recieve 2 args:
            1 - Database instance to work with
            2 - Bytes data that ready to be parsed using protobuffers
        This is required because all of them are encoded similary in CODES
        All of fucntions decorated by @code must return a Tuple[code_number, other_data] where
            code_number - the code of a function, that will process response
            other_data - data that must be processed by the response function
        Also for the creation and other functions that are not assume the specified output
        (database response for example): must be created a single function that will process all of them
        AAAAND Also the code numbers even for the requests and odd for the responses
    '''
    request = signals.tred_to_create()
    request.ParseFromString(message)
    values = ctrl.create_tred(request.creator_id, request.header, request.body)
    await db.create_new_tred(values)
    return (5, ("#TODO. SEND NORMAL RESPONSE",))


@code(5)
async def response_creation(data):
    return b"CODE=5\n\n" + data[0].encode() + b"\n\n\n\n"


@code(6)
async def create_message(db, message):
    request = signals.message_to_create()
    request.ParseFromString(message)
    values = ctrl.create_message(
        request.login,
        request.tred_id,
        request.message)
    await db.create_new_message(values)
    return (5, ("#TODO. SEND NORMAL RESPONSE",))


@code(8)
async def create_tred_participation(db, message):
    request = signals.tred_participation_to_create()
    request.ParseFromString(message)
    values = ctrl.create_tred_participation(request.tred_id, request.super_id)
    await db.create_new_tred_participation(values)
    return (5, ("#TODO. SEND NORMAL RESPONSE",))


@code(10)
async def create_union_request(db, message):
    request = signals.union_request_to_create()
    request.ParseFromString(message)
    values = ctrl.create_union_request(request._from, request.to)
    await db.create_new_union_request(values)
    return (5, ("#TODO. SEND NORMAL RESPONSE",))


@code(12)
async def create_personal_list(db, message):
    request = signals.personal_list_to_create()
    request.ParseFromString(message)
    values = ctrl.create_personal_list(request.list_name, request.owner_id)
    await db.create_new_personal_list(values)
    return (5, ("#TODO. SEND NORMAL RESPONSE",))


@code(14)
async def create_people_inlist(db, message):
    request = signals.people_inlist_to_create()
    request.ParseFromString(message)
    values = ctrl.create_people_inlist(request.list_id, request.friend_id)
    await db.create_new_people_inlist(values)
    return (5, ("#TODO. SEND NORMAL RESPONSE",))


async def make_output(db, data: Tuple[int, bytes]):
    code = data[0]
    response = data[1]
    result = await CODES[code](response)
    return result


async def send_users_data(data):
    users_data = signals.send_user_to_client()
    users_data.login = data[0]
    users_data.password = data[1]
    response = users_data.SerializeToString()
    response = b"CODE=1\n\n" + response + b"\n\n\n\n"
    return response


async def handle_write(db, writer: asyncio.StreamWriter, sockname: Tuple[str, int], message: Tuple[int, bytes]):
    response = await make_output(db, message)
    writer.write(response)
    await writer.drain()


async def parse_input(db, data):
    commands, message = data.strip().split(b"\n\n")
    commands = commands.split(b"\n")
    commands = dict(i.split(b"=") for i in commands)
    code = int(commands[b"CODE"])
    return await CODES[code](db, message)


async def handle_read(db, reader: asyncio.StreamReader, sockname: Tuple[str, int]):
    data = await reader.readuntil(SEPARATOR)
    return await parse_input(db, data)


async def handler(db, reader: asyncio.StreamReader, writer: asyncio.StreamWriter):
    sockname = writer.get_extra_info('peername')
    result = await handle_read(db, reader, sockname)
    await handle_write(db, writer, sockname, result)
    writer.close()


async def main():
    db = DataBase(get_connect_string())
    await db.connect(DB)

    server = await asyncio.start_server(lambda r, w: handler(db, r, w), *ADDRESS)
    async with server:
        await server.serve_forever()

    await db.disconnect()


if __name__ == '__main__':
    asyncio.run(main())
