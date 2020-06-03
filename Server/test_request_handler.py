import asyncio
from config import connect_string
from database import DataBase
from proto.client_pb2 import ClientMessage
from proto.server_pb2 import ServerMessage
from proto.common_pb2 import UserCredentials
from request_handling_utils import SessionData, request_handler
import request_handlers as rh


async def test_message_send_file(string: str,
                                 filedata: bytes,
                                 filename: str):
    message = ClientMessage.ThreadSendMessageRequest()
    message.body = string
    message.file.filename = filename
    message.file.filedata = filedata
    message.thread_id = 1

    db = DataBase(connect_string())
    await db.connect()
    session = SessionData(db)
    await rh.create_message(message, session)


async def test_message_send_nofile(string: str):
    message = ClientMessage.ThreadSendMessageRequest()
    message.body = string
    message.thread_id = 1

    db = DataBase(connect_string())
    await db.connect()
    session = SessionData(db)
    await rh.create_message(message, session)


async def test_get_messages_with_files():
    message = ClientMessage.ThreadDataRequest()
    db = DataBase(connect_string())
    await db.connect()
    session = SessionData(db, last_message_id=19)
    result = await rh.get_new_messages(message, session)
    for i in result.new_messages_appeared.messages:
        print(i)


async def main() -> None:
    await test_message_send_file("test_request_handler", b"8658567865", "test.exe")
    await test_message_send_nofile("nofile")
    await test_get_messages_with_files()


if __name__ == '__main__':
    asyncio.run(main())