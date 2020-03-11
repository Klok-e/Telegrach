import asyncio
import signals_pb2 as signals


async def tcp_echo_client():
    s = signals.user_creation_request()
    s.super_id = -1
    l = s.SerializeToString()
    reader, writer = await asyncio.open_connection(
        '127.0.0.1', 9999)

    print(f'Send: {l!r}')
    writer.write(b"CODE=0\n\n" + l + b"\n\n\n\n")

    data = await reader.read(100)
    print(f'Received: {data.decode()!r}')

    print('Close the connection')
    writer.close()

asyncio.run(tcp_echo_client())