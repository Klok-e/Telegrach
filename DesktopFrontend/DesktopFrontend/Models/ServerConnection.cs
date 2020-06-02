using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Logging;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using DynamicData;
using Google.Protobuf;
using Proto;

namespace DesktopFrontend.Models
{
    public class ServerConnection : IServerConnection
    {
        public const int BufferSize = 1024 * 64;

        private readonly string _connectString;
        private readonly int _port;
        private readonly TcpClient _client;
        private bool _isLoggedIn = false;

        public bool IsConnected => _client.Connected;

        private readonly ReplaySubject<ThreadItem> _newThreadArrived = new ReplaySubject<ThreadItem>();

        private readonly ReplaySubject<ChatMessageInThread> _newMessageArrived =
            new ReplaySubject<ChatMessageInThread>();

        public IObservable<ThreadItem> NewThreadArrived => _newThreadArrived.AsObservable();
        public IObservable<ChatMessageInThread> NewMessageArrived => _newMessageArrived.AsObservable();

        /// <summary>
        /// A lock that doesn't allow to simultaneously execute async methods (buttons) and query server
        /// </summary>
        private SemaphoreSlim _querySema;

        private Task? _queryTask;
        private CancellationTokenSource _cancelServerQuerying;


        public ServerConnection(string connectString, int port)
        {
            _connectString = connectString;
            _port = port;
            _client = new TcpClient();

            _cancelServerQuerying = new CancellationTokenSource();
            _querySema = new SemaphoreSlim(1, 1);
        }

        public void Dispose()
        {
            // finish current requests first
            _querySema.Wait();
            try
            {
                _cancelServerQuerying.Cancel();
                if (IsConnected)
                {
                    _client.Close();
                }

                if (_queryTask?.Exception != null)
                {
                    Log.Error(Log.Areas.Network, this, $"Query task threw an exception: {_queryTask.Exception}");
                }
            }
            finally
            {
                _querySema.Release();
            }
        }

        public async Task<bool> Connect()
        {
            try
            {
                var connectTask = _client.ConnectAsync(_connectString, _port);
                if (await Task.WhenAny(connectTask,
                        Task.Delay(TimeSpan.FromSeconds(0.5))) == connectTask)
                    // connected successfully
                {
                    // propagate exceptions
                    await connectTask;
                    Logger.Sink.Log(LogEventLevel.Information, "Network", this,
                        "Connected to the server");

                    StartQuerying();
                }
            }
            catch (SocketException e)
            {
                Log.Warn(Log.Areas.Network, this, $"Error while trying to connect to the server: {e.Message}");
            }

            return IsConnected;
        }

        private void StartQuerying()
        {
            Log.Info(Log.Areas.Network, this, "Started querying the server");
            _queryTask = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(0.1), _cancelServerQuerying.Token);
                    if (_cancelServerQuerying.Token.IsCancellationRequested)
                        break;

                    if (!_isLoggedIn)
                        continue;

                    await _querySema.WaitAsync();
                    try
                    {
                        foreach (var thread in await RequestNewThreads())
                            _newThreadArrived.OnNext(thread);

                        foreach (var message in await RequestNewMessages())
                            _newMessageArrived.OnNext(message);
                    }
                    catch (Exception e)
                    {
                        Log.Error(Log.Areas.Application, this, $"{e}");
                        throw;
                    }
                    finally
                    {
                        _querySema.Release();
                    }
                }
            }, _cancelServerQuerying.Token);
        }

        public async Task<bool> LogInWithCredentials(string user, string pass)
        {
            await _querySema.WaitAsync();
            try
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
                    Log.Error(Log.Areas.Network, this,
                        $"Response to the login request was given an unexpected answer: {responseUnion}");
                    return false;
                }

                var response = responseUnion.ServerResponse;

                Log.Info(Log.Areas.Network, this,
                    $"Response to the login request is {response}");
                return _isLoggedIn = response.IsOk;
            }
            finally
            {
                _querySema.Release();
            }
        }

        public async Task<Bitmap> RequestCaptcha()
        {
            await _querySema.WaitAsync();
            try
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                await using var s = assets.Open(new Uri("avares://DesktopFrontend/Assets/mock-captcha.jpg"));
                return new Bitmap(s);
            }
            finally
            {
                _querySema.Release();
            }
        }

        public async Task<(string login, string pass)?> TryRequestAccount(string tryText)
        {
            await _querySema.WaitAsync();
            try
            {
                var stream = new LengthPrefixedStreamWrapper(_client.GetStream());

                var msg = new ClientMessage
                    {UserCreateRequest = new ClientMessage.Types.UserCreationRequest {Link = false}};

                await stream.WriteProtoMessageAsync(msg);

                var response = await stream.ReadProtoMessageAsync(ServerMessage.Parser);
                if (response.InnerCase != ServerMessage.InnerOneofCase.NewAccountData)
                {
                    Log.Info(Log.Areas.Network, this,
                        $"Response to the account creation request was unexpected: {response}");
                    return null;
                }

                var accData = response.NewAccountData;
                return (accData.Login, accData.Password);
            }
            finally
            {
                _querySema.Release();
            }
        }

        public async Task CreateThread(string head, string body)
        {
            await _querySema.WaitAsync();
            try
            {
                var stream = new LengthPrefixedStreamWrapper(_client.GetStream());

                var msg = new ClientMessage
                {
                    CreateThreadRequest = new ClientMessage.Types.ThreadCreateRequest
                    {
                        Head = head,
                        Body = body,
                    }
                };

                await stream.WriteProtoMessageAsync(msg);

                var response = await stream.ReadProtoMessageAsync(ServerMessage.Parser);
                if (response.InnerCase != ServerMessage.InnerOneofCase.ServerResponse)
                {
                    Log.Error("Network", this,
                        $"Response to the thread creation request was unexpected: {response}");
                    throw new Exception();
                }

                if (!response.ServerResponse.IsOk)
                {
                    Log.Warn(Log.Areas.Network, this, "Server didn't allow the creation of a thread");
                    // TODO: show a popup explaining the situation
                    throw new Exception();
                }
            }
            finally
            {
                _querySema.Release();
            }
        }

        public async Task SendMessage(string body, ulong threadId)
        {
            await _querySema.WaitAsync();
            try
            {
                var stream = new LengthPrefixedStreamWrapper(_client.GetStream());

                var msg = new ClientMessage
                {
                    SendMsgToThreadRequest = new ClientMessage.Types.ThreadSendMessageRequest
                    {
                        Body = body,
                        ThreadId = threadId
                    }
                };
                await stream.WriteProtoMessageAsync(msg);

                var response = await stream.ReadProtoMessageAsync(ServerMessage.Parser);
                if (response.InnerCase != ServerMessage.InnerOneofCase.ServerResponse)
                {
                    Log.Error("Network", this,
                        $"Response to the send message request was unexpected: {response}");
                    throw new Exception();
                }

                if (!response.ServerResponse.IsOk)
                {
                    Log.Warn(Log.Areas.Network, this, "Server didn't allow to send a message");
                    // TODO: show a popup explaining the situation
                    throw new Exception();
                }
            }
            finally
            {
                _querySema.Release();
            }
        }

        public Task<IEnumerable<UserData>> RequestUsersOnline(ulong threadId)
        {
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<ThreadItem>> RequestNewThreads()
        {
            var stream = new LengthPrefixedStreamWrapper(_client.GetStream());

            var msg = new ClientMessage
            {
                GetAllJoinedThreadsRequest = new ClientMessage.Types.GetAllJoinedThreadsRequest()
            };

            await stream.WriteProtoMessageAsync(msg);

            var response = await stream.ReadProtoMessageAsync(ServerMessage.Parser);
            if (response.InnerCase != ServerMessage.InnerOneofCase.AllTheThreads)
            {
                Log.Error("Network", this,
                    $"Response to the threads request was unexpected: {response}");
                throw new Exception();
            }

            return response.AllTheThreads.Threads.Select(t => new ThreadItem
            {
                Body = t.Body,
                Head = t.Head,
                Id = t.Id,
            });
        }

        private async Task<IEnumerable<ChatMessageInThread>> RequestNewMessages()
        {
            var stream = new LengthPrefixedStreamWrapper(_client.GetStream());

            var msg = new ClientMessage
            {
                ThreadDataRequest = new ClientMessage.Types.ThreadDataRequest()
            };

            await stream.WriteProtoMessageAsync(msg);

            var response = await stream.ReadProtoMessageAsync(ServerMessage.Parser);
            if (response.InnerCase != ServerMessage.InnerOneofCase.NewMessagesAppeared)
            {
                Log.Error("Network", this,
                    $"Response to the messages request was unexpected: {response}");
                throw new Exception();
            }

            return response.NewMessagesAppeared.Messages.Select(m => new ChatMessageInThread
            {
                Message = new ChatMessage
                {
                    Body = m.Body,
                    Time = m.Time.ToDateTime(),
                },
                ThreadId = m.ThreadId,
            });
        }
    }
}