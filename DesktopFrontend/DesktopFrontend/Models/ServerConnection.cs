using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
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
        private TcpClient _client;

        public bool IsConnected => _client.Connected;

        private readonly Subject<ThreadItem> _newThreadArrived;
        private readonly Subject<ChatMessageInThread> _newMessageArrived;
        public IObservable<ThreadItem> NewThreadArrived => _newThreadArrived;

        public IObservable<ChatMessageInThread> NewMessageArrived => _newMessageArrived;

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
                await _client.ConnectAsync(_connectString, _port);
            }
            catch (SocketException e)
            {
                Logger.Sink.Log(LogEventLevel.Error, "Network", this, e.Message);
                return false;
            }

            StartQuerying();

            return IsConnected;
        }

        private void StartQuerying()
        {
            _queryTask = Task.Run(async () =>
            {
                while (true)
                {
                    await _querySema.WaitAsync();
                    try
                    {
                        // execute OnNext on the UI thread so that subscribers can call UI commands
                        var threads = await RequestNewThreads();
                        Dispatcher.UIThread.Post(() =>
                        {
                            foreach (var thread in threads)
                            {
                                _newThreadArrived.OnNext(thread);
                            }
                        });

                        var messages = await RequestNewMessages();
                        Dispatcher.UIThread.Post(() =>
                        {
                            foreach (var message in messages)
                            {
                                _newMessageArrived.OnNext(message);
                            }
                        });
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

                    await Task.Delay(TimeSpan.FromSeconds(0.1), _cancelServerQuerying.Token);
                    if (_cancelServerQuerying.Token.IsCancellationRequested)
                        break;
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
                    Logger.Sink.Log(LogEventLevel.Error, "Network", this,
                        $"Response to the login request was given an unexpected answer: {responseUnion}");
                    return false;
                }

                var response = responseUnion.ServerResponse;

                Logger.Sink.Log(LogEventLevel.Information, "Network", this,
                    $"Response to the login request is {response}");
                return response.IsOk;
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
                Bitmap b;
                await using (var s = assets.Open(new Uri("avares://DesktopFrontend/Assets/mock-captcha.jpg")))
                    b = new Bitmap(s);
                return b;
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
                    Logger.Sink.Log(LogEventLevel.Information, "Network", this,
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