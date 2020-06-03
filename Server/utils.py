import asyncio
import config
from typing import Optional, Any, TypeVar
from proto.client_pb2 import ClientMessage
from proto.server_pb2 import ServerMessage


T_MSG = TypeVar('T_MSG', ClientMessage, ServerMessage)


async def read_proto_message(reader: asyncio.StreamReader, message_type: T_MSG) -> Optional[T_MSG]:
    # read prefix
    prefix = await reader.read(config.MESSAGE_PREFIX_SIZE)

    # if EOF is reached then nothing to read here
    # reader.read returns empty bytes object if EOF is reached
    if prefix == b'':
        return None

    # parse prefix
    prefix = int.from_bytes(prefix, byteorder='little')

    # read message
    msg = await reader.readexactly(prefix)

    # parse message
    return message_type.FromString(msg)


async def write_proto_message(writer: asyncio.StreamWriter, message):
    # write prefix
    prefix = message.ByteSize().to_bytes(4, byteorder='little')
    writer.write(prefix)

    # write message
    writer.write(message.SerializeToString())

    await writer.drain()
