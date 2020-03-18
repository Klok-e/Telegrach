import asyncio
# import logging
import controllers as ctrl
from proto.client_pb2 import ClientMessage
from proto.server_pb2 import ServerMessage
from proto.common_pb2 import UserCredentials
import typing
from typing import Tuple, Callable, Awaitable, Any, Dict
from config import *
from database import DataBase
import utils

# TODO LOG ALL EXCEPTIONS WITH sys.exc_info

TEST_KEY = b'XP4VTC3mrE-84R4xFVVDBXZFnQo4jf1i'


# logging.basicConfig(filename=LOG_FILE_SERVER,
#                     level=LOG_LEVEL_SERVER,
#                     format=LOG_FORMAT_SERVER)

class SessionData:
    def __init__(self, database):
        self.logged_in = False
        self.db = database


REQUEST_HANDLERS = {}


def request_handler(accept_variant):
    '''
        A decorator which associates fynction with specified code recieved from clients
    '''

    def inner(func: Callable[[Any, SessionData], Awaitable[Any]]):
        if accept_variant in REQUEST_HANDLERS:
            raise RuntimeError
        REQUEST_HANDLERS[accept_variant] = func
        return func

    return inner


# TODO: fix me
@request_handler(ClientMessage.user_create_request)
async def create_user(db, message):
    '''
        Returns new_users with code data and commits its data to database
        Works lil bit different from other similar creation functions
        I super_id is provided it will link users together
        Either it will create new super_account and link new user with it
    '''
    request = signals.user_creation_request()
    request.ParseFromString(message)

    if request.super_id == 1:
        # That part of code required if user is using the service for the first
        # time, so super_account does not exists
        select = await db.get_max_super_id()
        super_id = dict(select.items())["max"] + 1
        # creating super_account before user_account
        await db.create_new_super_account(super_id)

    # a tuple(User`s data to send, Database data to store)
    data: Tuple[Tuple[str, str], Dict] = ctrl.create_user(super_id)
    await db.create_new_user(data[1])

    return 1, data[0]


# TODO: fix me
# @request_handler(client_pb2.ClientMessage.user_create_request)
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


# TODO: fix me
@request_handler(ClientMessage.thread_data_request)
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


# TODO: fix me
# @request_handler(client_pb2.ClientMessage.user_create_request)
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


# TODO: fix me
@request_handler(ClientMessage.create_thread_request)
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


# TODO: fix me
# @request_handler(client_pb2.ClientMessage.user_create_request)
async def response_creation(data):
    return b"CODE=5\n\n" + data[0].encode() + b"\n\n\n\n"


# TODO: fix me
@request_handler(ClientMessage.send_msg_to_thread_request)
async def create_message(db, message):
    request = signals.message_to_create()
    request.ParseFromString(message)
    values = ctrl.create_message(
        request.login,
        request.tred_id,
        request.message)
    await db.create_new_message(values)
    return (5, ("#TODO. SEND NORMAL RESPONSE",))


# TODO: fix me
# @request_handler(client_pb2.ClientMessage.user_create_request)
async def create_tred_participation(db, message):
    request = signals.tred_participation_to_create()
    request.ParseFromString(message)
    values = ctrl.create_tred_participation(request.tred_id, request.super_id)
    await db.create_new_tred_participation(values)
    return (5, ("#TODO. SEND NORMAL RESPONSE",))


# TODO: fix me
@request_handler(ClientMessage.union_another_request)
async def create_union_request(db, message):
    request = signals.union_request_to_create()
    request.ParseFromString(message)
    values = ctrl.create_union_request(request._from, request.to)
    await db.create_new_union_request(values)
    return (5, ("#TODO. SEND NORMAL RESPONSE",))


# TODO: fix me
# @request_handler(client_pb2.ClientMessage.user_create_request)
async def create_personal_list(db, message):
    request = signals.personal_list_to_create()
    request.ParseFromString(message)
    values = ctrl.create_personal_list(request.list_name, request.owner_id)
    await db.create_new_personal_list(values)
    return (5, ("#TODO. SEND NORMAL RESPONSE",))


# TODO: fix me
# @request_handler(client_pb2.ClientMessage.user_create_request)
async def create_people_inlist(db, message):
    request = signals.people_inlist_to_create()
    request.ParseFromString(message)
    values = ctrl.create_people_inlist(request.list_id, request.friend_id)
    await db.create_new_people_inlist(values)
    return (5, ("#TODO. SEND NORMAL RESPONSE",))


# TODO: obsolete?
async def make_output(db, data: Tuple[int, bytes]):
    code = data[0]
    response = data[1]
    result = await REQUEST_HANDLERS[code](response)
    return result


# TODO: obsolete?
async def handle_write(db, writer: asyncio.StreamWriter, sockname: Tuple[str, int], message: Tuple[int, bytes]):
    response = await make_output(db, message)
    writer.write(response)
    await writer.drain()


# TODO: obsolete?
async def parse_input(db, data):
    commands, message = data.strip().split(b"\n\n")
    commands = commands.split(b"\n")
    commands = dict(i.split(b"=") for i in commands)
    code = int(commands[b"CODE"])
    return await REQUEST_HANDLERS[code](db, message)


@request_handler(ClientMessage.login_request)
async def login(message: UserCredentials, session: SessionData):
    print(f"Sign in request: {message}")
    if message.login == "rwerwer" and message.password == "564756868":
        print(f"Sign in successful")
        ok = True
    else:
        print(f"Sign in failed")
        ok = False
    # create response
    response = ServerMessage()
    response.server_response.is_ok = ok
    return response


async def handle_request(message: ClientMessage, session_data: SessionData) -> Any:
    msg_type_str: str = message.WhichOneof('inner')
    msg_type: Any = getattr(ClientMessage, msg_type_str)
    handler = REQUEST_HANDLERS[msg_type]

    # invoke handler with a given variant
    variant = getattr(message, msg_type_str)
    return await handler(variant, session_data)


async def handler(db, reader: asyncio.StreamReader, writer: asyncio.StreamWriter):
    sockname = writer.get_extra_info('peername')
    print(f"new connection! {sockname}")

    session_data = SessionData(db)
    is_closed = False
    while not is_closed:
        # read request
        request = await utils.read_proto_message(reader, ClientMessage)

        # calculate response
        response = await handle_request(request, session_data)

        # send response
        await utils.write_proto_message(writer, response)

        is_closed = True

    writer.close()


def main():
    db = DataBase(connect_string())
    with db as db:
        loop = asyncio.get_event_loop()
        server = asyncio.start_server(lambda r, w: handler(db, r, w), *ADDRESS)
        server = loop.run_until_complete(server)

        # serve until CTRL + C
        print(f'Serving on {server.sockets[0].getsockname()}')
        try:
            loop.run_forever()
        except KeyboardInterrupt:
            pass

        print(f'Stopped serving on {server.sockets[0].getsockname()}')
        server.close()

    loop.run_until_complete(server.wait_closed())
    loop.close()


if __name__ == '__main__':
    main()
