from proto.client_pb2 import ClientMessage
from proto.server_pb2 import ServerMessage
from proto.common_pb2 import UserCredentials
from request_handling_utils import SessionData, request_handler
from typing import Tuple, Callable, Awaitable, Any, Dict
from crypto import validate_password
from models import SuperAccount


@request_handler(ClientMessage.login_request)
async def login(message: UserCredentials,
                session: SessionData) -> ServerMessage:
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
async def create_user(message: ClientMessage.UserCreationRequest,
                      session: SessionData) -> ServerMessage:
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
async def get_new_messages(message: ClientMessage.ThreadDataRequest,
                           session: SessionData) -> ServerMessage:
    new_messages = await session.db.messages_with_id_above(session.last_message_id)
    if len(new_messages) > 0:
        session.last_message_id = new_messages[-1]["message_id"]

    response = ServerMessage()
    response.new_messages_appeared.messages.extend([])
    for msg in new_messages:
        appendable = response.new_messages_appeared.messages.add()
        appendable.id = msg["message_id"]
        appendable.thread_id = msg["tred_id"]
        appendable.body = msg["body"]
        appendable.time.FromDatetime(msg["timestamp"])
        if msg["data"] is not None:
            filename = msg["filename"] + "." + msg["extension"]
            appendable.file.filename = filename
            appendable.file.filedata = msg["data"]

    return response


# TODO: handle the case where there's already a lot of threads and they all can't be sent in one protobuf message
# (maybe fetch only a portion on get_new_threads request that fits in one protobuf
# message and wait till client sends another get_new_threads request)
@request_handler(ClientMessage.get_all_joined_threads_request)
async def get_new_threads(message: ClientMessage.GetAllJoinedThreadsRequest,
                          session: SessionData) -> ServerMessage:
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
async def thread_creation(message: ClientMessage.ThreadCreateRequest,
                          session: SessionData) -> ServerMessage:
    user = await session.db.get_user(session.login)
    await session.db.create_new_tred(user.super_id, message.head, message.body)

    response = ServerMessage()
    response.server_response.is_ok = True

    return response


@request_handler(ClientMessage.send_msg_to_thread_request)
async def create_message(message: ClientMessage.ThreadSendMessageRequest,
                         session: SessionData) -> ServerMessage:
    file: UserCredentials.File = message.file
    file_id = None
    if file.filename != "":
        filename, _, extension = file.filename.rpartition('.')
        file_id = await session.db.create_new_file(extension, filename, file.filedata)

    await session.db.create_new_message(session.login, message.thread_id, message.body, file_id)

    response = ServerMessage()
    response.server_response.is_ok = True

    return response