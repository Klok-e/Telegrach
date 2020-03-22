from dataclasses import dataclass
from typing import Callable, Awaitable, Any, Optional
from database import DataBase
from proto.client_pb2 import ClientMessage
from proto.server_pb2 import ServerMessage
from proto.common_pb2 import UserCredentials

REQUEST_HANDLERS = {}


@dataclass
class SessionData:
    db: DataBase
    logged_in: bool = False
    login: Optional[str] = None


async def handle_request(message: ClientMessage, session_data: SessionData) -> Any:
    # to initialize all handlers before this function is run
    import request_handlers

    msg_type_str: str = message.WhichOneof('inner')
    msg_type: Any = getattr(ClientMessage, msg_type_str)
    handler = REQUEST_HANDLERS[msg_type]

    # invoke handler with a given variant
    variant = getattr(message, msg_type_str)
    return await handler(variant, session_data)


def request_handler(accept_variant):
    """
        A decorator which associates a function with the client message variant
    """

    def inner(func: Callable[[Any, SessionData], Awaitable[Any]]):
        if accept_variant in REQUEST_HANDLERS:
            raise RuntimeError
        REQUEST_HANDLERS[accept_variant] = func
        return func

    return inner
