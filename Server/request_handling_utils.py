from dataclasses import dataclass, field
from typing import Callable, Awaitable, Any, Optional, Dict
from database import DataBase
from proto.client_pb2 import ClientMessage
from proto.server_pb2 import ServerMessage
from proto.common_pb2 import UserCredentials


@dataclass
class SessionData:
    db: DataBase
    logged_in: bool = False
    login: Optional[str] = None
    last_thread_id: int = 0
    last_message_id: int = 0


class Handlers:
    def __init__(self, *handlers):
        self._handlers = {h.accept_variant: h for h in handlers}

    async def handle_request(self, message: ClientMessage, session_data: SessionData) -> Any:
        msg_type_str: str = message.WhichOneof('inner')
        msg_type: Any = getattr(ClientMessage, msg_type_str)
        if msg_type not in self._handlers:
            raise RuntimeError
        handler = self._handlers[msg_type]

        # invoke handler with a given variant
        variant = getattr(message, msg_type_str)
        return await handler(variant, session_data)


def request_handler(accept_variant):
    """
        A decorator which associates a function with the client message variant
    """

    def inner(func: Callable[[Any, SessionData], Awaitable[Any]]):
        func.accept_variant = accept_variant
        return func

    return inner
