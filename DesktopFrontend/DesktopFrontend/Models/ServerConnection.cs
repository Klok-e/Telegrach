using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DesktopFrontend.Models
{
    public class ServerConnection
    {
        private readonly string _connectString;
        private readonly int _port;
        private TcpClient _client;

        public ServerConnection(string connectString, int port)
        {
            _connectString = connectString;
            _port = port;
            _client = new TcpClient();
        }

        public async Task Connect()
        {
            // maybe idk
            await _client.ConnectAsync(_connectString, _port);
        }
    }
}