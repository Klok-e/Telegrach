using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DesktopFrontend.Models
{
    public class ServerConnection : IServerConnection
    {
        private readonly string _connectString;
        private readonly int _port;
        private TcpClient _client;

        public bool IsConnected => _client.Connected;

        public ServerConnection(string connectString, int port)
        {
            _connectString = connectString;
            _port = port;
            _client = new TcpClient();
        }

        public async Task<bool> Connect()
        {
            try
            {
                await _client.ConnectAsync(_connectString, _port);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                return false;
            }

            var stream = _client.GetStream();
            // TODO: send login request witch credentials
            // TODO: or request a new account

            return IsConnected;
        }
    }
}