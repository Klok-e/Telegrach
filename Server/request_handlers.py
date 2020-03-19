from proto.client_pb2 import ClientMessage
from proto.server_pb2 import ServerMessage
from proto.common_pb2 import UserCredentials
from request_handling_utils import SessionData, request_handler, handle_request
from typing import Tuple, Callable, Awaitable, Any, Dict
import controllers as ctrl
from crypto import validate_password
from models import SuperAccount


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


@request_handler(ClientMessage.login_request)
async def login(message: UserCredentials, session: SessionData):
    print(f"Sign in request: {message}")
    user = await session.db.get_user(message.login)
    ok = False
    if user is None:
        print(f"Sign in failed")
    elif validate_password(user.salt, user.pword, message.password):
        print(f"Sign in successful")
        ok = True

    session.logged_in = ok

    # create response
    response = ServerMessage()
    response.server_response.is_ok = ok
    return response


@request_handler(ClientMessage.user_create_request)
async def create_user(message: ClientMessage.UserCreationRequest, session: SessionData):
    """
        Returns new_users with code data and commits its data to database
        Works lil bit different from other similar creation functions
        I super_id is provided it will link users together
        Either it will create new super_account and link new user with it
    """

    super_acc = None
    if session.logged_in and message.link:
        user = await session.db.get_user(session.login)
        super_acc = SuperAccount(super_id=user.super_id)

    new_acc, password = await session.db.create_new_user(super_acc)

    response = ServerMessage()
    response.new_account_data.login = new_acc.login
    response.new_account_data.password = password

    return response
