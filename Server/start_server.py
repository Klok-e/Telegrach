import asyncio
# import logging
from proto.client_pb2 import ClientMessage
from proto.server_pb2 import ServerMessage
from proto.common_pb2 import UserCredentials
import typing
from typing import Tuple, Callable, Awaitable, Any, Dict, Union, NewType, List, TypeVar
from config import *
from database import DataBase
import utils
from request_handling_utils import SessionData, request_handler, Handlers
from request_handlers import *
import signal

# TODO LOG ALL EXCEPTIONS WITH sys.exc_info

TEST_KEY = b'XP4VTC3mrE-84R4xFVVDBXZFnQo4jf1i'

# logging.basicConfig(filename=LOG_FILE_SERVER,
#                     level=LOG_LEVEL_SERVER,
#                     format=LOG_FORMAT_SERVER)


# T = TypeVar("T")
# Handler = NewType("Handler", Callable[[T, SessionData], ServerMessage])
# HandlersList = NewType("HandlersList", List[Handler])

SHUTDOWN = False


async def shutdown(signal, loop):
    """Cleanup tasks tied to the service's shutdown."""
    global SHUTDOWN
    # logging.info(f"Received exit signal {signal.name}...")
    # logging.info("Closing database connections")
    # logging.info("Nacking outstanding messages")
    print(f"Received exit signal {signal.name}...")
    SHUTDOWN = True
    await asyncio.sleep(1)

    tasks = [t for t in asyncio.all_tasks() if t is not
             asyncio.current_task()]

    [task.cancel() for task in tasks]

    # logging.info(f"Cancelling {len(tasks)} outstanding tasks")
    await asyncio.gather(*tasks, return_exceptions=True)
    # logging.info(f"Flushing metrics")
    loop.stop()


async def server_handler(handlers: Handlers, db: DataBase,
                         reader: asyncio.StreamReader,
                         writer: asyncio.StreamWriter):
    sockname = writer.get_extra_info('peername')
    print(f"new connection! {sockname}")

    session_data = SessionData(db)
    while not SHUTDOWN:
        # read request
        request = await utils.read_proto_message(reader, ClientMessage)

        # EOF is reached
        if request is None:
            print(f"connection {sockname} ended communications")
            break

        if session_data.logged_in:
            await db.set_user_last_action_time_to_now(session_data.login)

        # calculate response
        response = await handlers.handle_request(request, session_data)

        # send response
        await utils.write_proto_message(writer, response)

    print(f"connection {sockname} closed")
    writer.close()
    await writer.wait_closed()


def main():
    handlers = Handlers(
        login,
        create_user,
        get_new_messages,
        get_new_threads,
        thread_creation,
        create_message,
        users_online)
    db = DataBase(connect_string())
    with db as db:
        loop = asyncio.get_event_loop()

        signals = (signal.SIGTERM, signal.SIGINT)
        for s in signals:
            loop.add_signal_handler(
                s, lambda s=s: asyncio.create_task(shutdown(s, loop)))

        server = asyncio.start_server(
            lambda r, w: server_handler(handlers,
                                        db, r, w), *ADDRESS)
        server = loop.run_until_complete(server)

        try:
            # serve until CTRL + C
            print(f'Serving on {server.sockets[0].getsockname()}')
            loop.run_forever()
        finally:
            print(f'Stopped serving on {server.sockets[0].getsockname()}')
            server.close()

    loop.run_until_complete(server.wait_closed())
    loop.close()


if __name__ == '__main__':
    main()
