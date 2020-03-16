using System;
using System.Collections.Generic;
using System.Net.Sockets;

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

        public int ReadPrefix()
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

        public void WritePrefix(int value)
        {
            Stream.Write(BitConverter.GetBytes(value));
        }
    }
}