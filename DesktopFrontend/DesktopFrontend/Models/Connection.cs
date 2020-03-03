using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace DesktopFrontend.Models
{
    public static class Connection
    {
        private static Socket socket;

        public static bool Connect(byte[] server_ip, int port)
        {
#if DEBUG
            socket = null;
            Console.WriteLine(Dns.GetHostName());
            return true; // delete this
#else
            socket = new Socket(new IPAddress(server_ip).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            return socket.Connected;
#endif            
        }
    }
}
