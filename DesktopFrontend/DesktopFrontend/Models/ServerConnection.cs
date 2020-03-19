using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Google.Protobuf;
using Proto;

namespace DesktopFrontend.Models
{
    public class ServerConnection : IServerConnection
    {
        public const int BufferSize = 1024 * 64;

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

        ~ServerConnection()
        {
            if (IsConnected)
            {
                var stream = new LengthPrefixedStreamWrapper(_client.GetStream());
                stream.WriteProtoMessageAsync(new ClientMessage
                {
                    TerminateSessionRequest = new ClientMessage.Types.TerminateSessionRequest()
                }).Wait();
                _client.Close();
            }
        }

        public async Task<bool> Connect()
        {
            try
            {
                await _client.ConnectAsync(_connectString, _port);
            }
            catch (SocketException e)
            {
                Logger.Sink.Log(LogEventLevel.Error, "Network", this, e.Message);
                return false;
            }

            return IsConnected;
        }

        public async Task<bool> LogInWithCredentials(string user, string pass)
        {
            var stream = new LengthPrefixedStreamWrapper(_client.GetStream());
            var request = new ClientMessage()
            {
                LoginRequest = new UserCredentials
                {
                    Login = user,
                    Password = pass
                }
            };
            await stream.WriteProtoMessageAsync(request);

            var responseUnion = await stream.ReadProtoMessageAsync(ServerMessage.Parser);
            if (responseUnion.InnerCase != ServerMessage.InnerOneofCase.ServerResponse)
            {
                Logger.Sink.Log(LogEventLevel.Error, "Network", this,
                    $"Response to the login request was given an unexpected answer: {responseUnion}");
                return false;
            }

            var response = responseUnion.ServerResponse;

            Logger.Sink.Log(LogEventLevel.Information, "Network", this,
                $"Response to the login request is {response}");
            return response.IsOk;
        }

        public async Task<Bitmap> RequestCaptcha()
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            Bitmap b;
            await using (var s = assets.Open(new Uri("avares://DesktopFrontend/Assets/mock-captcha.jpg")))
                b = new Bitmap(s);
            return b;
        }

        public async Task<(string login, string pass)?> TryRequestAccount(string tryText)
        {
            var stream = new LengthPrefixedStreamWrapper(_client.GetStream());

            var msg = new ClientMessage
                {UserCreateRequest = new ClientMessage.Types.UserCreationRequest {Link = false}};

            await stream.WriteProtoMessageAsync(msg);

            var response = await stream.ReadProtoMessageAsync(ServerMessage.Parser);
            if (response.InnerCase != ServerMessage.InnerOneofCase.NewAccountData)
            {
                Logger.Sink.Log(LogEventLevel.Information, "Network", this,
                    $"Response to the account creation request was unexpected: {response}");
                return null;
            }

            var accData = response.NewAccountData;
            return (accData.Login, accData.Password);
        }
    }
}