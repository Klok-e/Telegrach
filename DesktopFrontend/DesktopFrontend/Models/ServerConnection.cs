﻿using System;
using System.Collections.Generic;
using System.Drawing;
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

            return IsConnected;
        }

        public Task<bool> LogInWithCredentials(string user, string pass)
        {
            var stream = _client.GetStream();
            // TODO: send login request witch credentials
            throw new NotImplementedException();
        }

        public Task<Image> RequestCaptcha()
        {
            throw new NotImplementedException();
        }

        public Task<bool> TryPassCaptcha(string tryText)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RequestANewAccount()
        {
            var stream = _client.GetStream();
            // TODO: request a new account
            throw new NotImplementedException();
        }
    }
}