using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Logging;
using Google.Protobuf;

namespace DesktopFrontend
{
    public class LengthPrefixedStreamWrapper
    {
        public const int PrefixSize = 4;

        public readonly NetworkStream Stream;

        public LengthPrefixedStreamWrapper(NetworkStream stream)
        {
            Stream = stream;
        }

        public async Task WriteProtoMessageAsync<T>(T message)
            where T : IMessage<T>
        {
            var size = message.CalculateSize();
            WritePrefix(size);
            await using var mem = new MemoryStream(new byte[size]);
            message.WriteTo(mem);
            await Stream.WriteAsync(mem.GetBuffer());
        }

        public async Task<T> ReadProtoMessageAsync<T>(MessageParser<T> parser)
            where T : IMessage<T>

        {
            var pref = ReadPrefix();
            var bytes = await ReadExactlyAsync(pref);
            return parser.ParseFrom(bytes);
        }

        private async Task<byte[]> ReadExactlyAsync(int count)
        {
            var buffer = new byte[count];
            var offset = 0;
            while (offset < count)
            {
                var read = await Stream.ReadAsync(buffer, offset, count - offset);
                if (read == 0)
                {
                    Logger.Sink.Log(LogEventLevel.Error, "Network", this,
                        $"Tried to read {count} bytes from a network stream and there wasn't enough");
                    throw new EndOfStreamException();
                }

                offset += read;
            }

            System.Diagnostics.Debug.Assert(offset == count);
            return buffer;
        }

        private int ReadPrefix()
        {
            Span<byte> prefix = stackalloc byte[PrefixSize];
            var read = 0;
            while (read < PrefixSize)
            {
                prefix[read] = (byte)Stream.ReadByte();
                read += 1;
            }

            return BitConverter.ToInt32(prefix);
        }

        private void WritePrefix(int value)
        {
            Stream.Write(BitConverter.GetBytes(value));
        }
    }
}