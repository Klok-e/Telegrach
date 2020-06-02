from proto.client_pb2 import ClientMessage
from proto.server_pb2 import ServerMessage
from proto.common_pb2 import UserCredentials
from request_handling_utils import SessionData, request_handler
from typing import Tuple, Callable, Awaitable, Any, Dict
import controllers as ctrl
from crypto import validate_password
from models import SuperAccount


@request_handler(ClientMessage.login_request)
async def login(message: UserCredentials, session: SessionData):
    print(f"Sign in request: {message}")
    user = None
    if 32 <= len(message.login) <= 36:
        user = await session.db.get_user(message.login)
    ok = False
    if user is None:
        print(f"Sign in failed")
    elif validate_password(user.salt, user.pword, message.password):
        print(f"Sign in successful")
        session.login = user.login
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


# TODO: handle the case where there's already a lot of messages and they all can't be sent in one protobuf message
# (maybe send fetch only a portion on get_new_messages_from_tred request that fits in one protobuf
# message and wait till client sends another get_new_messages_from_tred
# request)
@request_handler(ClientMessage.thread_data_request)
async def get_new_messages(message: ClientMessage.ThreadDataRequest, session: SessionData):
    new_messages = list(await session.db.messages_with_id_above(session.last_message_id))
    if len(new_messages) > 0:
        session.last_message_id = new_messages[-1].message_id

    response = ServerMessage()
    response.new_messages_appeared.messages.extend([])
    for msg in new_messages:
        appendable = response.new_messages_appeared.messages.add()
        appendable.id = msg.message_id
        appendable.thread_id = msg.tred_id
        appendable.body = msg.body
        appendable.time.FromDatetime(msg.timestamp)

    return response


# TODO: handle the case where there's already a lot of threads and they all can't be sent in one protobuf message
# (maybe fetch only a portion on get_new_threads request that fits in one protobuf
# message and wait till client sends another get_new_threads request)
@request_handler(ClientMessage.get_all_joined_threads_request)
async def get_new_threads(message: ClientMessage.GetAllJoinedThreadsRequest, session: SessionData):
    new_threads = list(await session.db.threads_with_id_above(session.last_thread_id))
    if len(new_threads) > 0:
        session.last_thread_id = new_threads[-1].tred_id

    response = ServerMessage()
    response.all_the_threads.threads.extend([])
    for thread in new_threads:
        r_th = response.all_the_threads.threads.add()
        r_th.id = thread.tred_id
        r_th.head = thread.header
        r_th.body = thread.body
    return response


@request_handler(ClientMessage.create_thread_request)
async def thread_creation(message: ClientMessage.ThreadCreateRequest, session: SessionData):
    user = await session.db.get_user(session.login)
    await session.db.create_new_tred(user.super_id, message.head, message.body)

    response = ServerMessage()
    response.server_response.is_ok = True

    return response


@request_handler(ClientMessage.send_msg_to_thread_request)
async def create_message(message: ClientMessage.ThreadSendMessageRequest, session: SessionData):
    file: UserCredentials.File = message.file
    file_id = None
    if not file.IsInitialized():
        filename, _, extension = file.filename.rpartition('.')
        file_id = await session.db.create_new_file(extension, filename, file.filedata)

    await session.db.create_new_message(session.login, message.thread_id, message.body, file_id)

    response = ServerMessage()
    response.server_response.is_ok = True

    return response


# @request_handler(ClientMessage.create_thread_request)
async def create_tred(db, message):
    """
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
    """
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
# @request_handler(ClientMessage.user_create_request)
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
