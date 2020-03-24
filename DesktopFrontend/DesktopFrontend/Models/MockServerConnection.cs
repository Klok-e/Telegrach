using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData;


namespace DesktopFrontend.Models
{
    public class MockServerConnection : IServerConnection
    {
        public bool IsConnected { get; private set; }
        private bool _loggedIn;

        public async Task<bool> Connect()
        {
            await Task.Delay(TimeSpan.FromSeconds(0.4));
            return IsConnected = true;
        }

        public async Task<bool> LogInWithCredentials(string user, string pass)
        {
            return _loggedIn = user == "rwerwer" && pass == "564756868";
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
            return ("rwerwer", "564756868");
        }

        private List<ThreadItem> threads = new List<ThreadItem>
        {
            new[]
            {
                new ThreadItem("мозкоподібні структури", "fuck you", 1)
                {
                    Messages = new ChatMessages()
                    {
                        Messages = new ObservableCollection<ChatMessage>
                        {
                            new[]
                            {
                                new ChatMessage
                                {
                                    Body = "i like potatoes",
                                    Time = DateTime.Now,
                                },
                                new ChatMessage
                                {
                                    Body = "me too",
                                    Time = DateTime.Now,
                                },
                            }
                        }
                    }
                },
                new ThreadItem("блаблабла", "fuck me", 2)
                {
                    Messages = new ChatMessages
                    {
                        Messages = new ObservableCollection<ChatMessage>
                        {
                            new[]
                            {
                                new ChatMessage
                                {
                                    Body = "anyone here",
                                    Time = DateTime.Now,
                                },
                            }
                        }
                    },
                },
            }
        };

        public async Task<ThreadSet> RequestThreadSet()
        {
            if (!_loggedIn)
                throw new Exception();
            var threadSet = new ThreadSet();
            threadSet.Threads.AddRange(threads.Select(t => new ThreadItem(t.Head, t.Body, t.Id)));
            return threadSet;
        }

        public async Task CreateThread(string head, string body)
        {
            if (!_loggedIn)
                throw new Exception();
            // uncomment to test throwing
            // throw new Exception();

            threads.Add(new ThreadItem(head, body, (ulong)threads.Count + 1));
        }

        public async Task SendMessage(string body, ulong threadId)
        {
            if (!_loggedIn)
                throw new Exception();
            var thread = threads.Find(t => t.Id == threadId) ?? throw new Exception();
            thread.Messages ??= new ChatMessages();
            thread.Messages.Messages.Add(new ChatMessage
            {
                Body = body,
                Time = DateTime.Now,
            });
        }

        public async Task<ChatMessages> RequestMessagesForThread(ThreadItem thread)
        {
            if (!_loggedIn)
                throw new Exception();
            var xxx = threads.Find(t => t.Id == thread.Id)?.Messages;
            if (xxx != null)
            {
                return new ChatMessages
                {
                    Messages = new ObservableCollection<ChatMessage>(xxx.Messages)
                };
            }

            return new ChatMessages();
        }
    }
}