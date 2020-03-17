import asyncio
import config


async def read_proto_message(reader: asyncio.StreamReader, message_type):
    # read prefix
    prefix = int.from_bytes(await reader.readexactly(config.MESSAGE_PREFIX_SIZE), byteorder='little')
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
