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

        public IObservable<ThreadItem> NewThreadArrived { get; }
        public IObservable<ChatMessageInThread> NewMessageArrived { get; }

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
                new ThreadItem
                {
                    Head = "мозкоподібні структури",
                    Body = "fuck you",
                    Id = 1,
                    //Messages = new ChatMessages()
                    //{
                    //    Messages = new ObservableCollection<ChatMessage>
                    //    {
                    //        new[]
                    //        {
                    //            new ChatMessage
                    //            {
                    //                Body = "i like potatoes",
                    //                Time = DateTime.Now,
                    //            },
                    //            new ChatMessage
                    //            {
                    //                Body = "me too",
                    //                Time = DateTime.Now,
                    //            },
                    //        }
                    //    }
                    //}
                },
                new ThreadItem
                {
                    Head = "блаблабла",
                    Body = "fuck me",
                    Id = 2,
                    //Messages = new ChatMessages
                    //{
                    //    Messages = new ObservableCollection<ChatMessage>
                    //    {
                    //        new[]
                    //        {
                    //            new ChatMessage
                    //            {
                    //                Body = "anyone here",
                    //                Time = DateTime.Now,
                    //            },
                    //        }
                    //    }
                    //},
                },
            }
        };

        public async Task<ThreadSet> RequestThreadSet()
        {
            if (!_loggedIn)
                throw new Exception();
            var threadSet = new ThreadSet();
            //threadSet.Threads.AddRange(threads.Select(t => new ThreadItem
            //{
            //    Head = t.Head,
            //    Body = t.Body,
            //    Id = t.Id,
            //}));
            return threadSet;
        }

        public async Task CreateThread(string head, string body)
        {
            if (!_loggedIn)
                throw new Exception();
            // uncomment to test throwing
            // throw new Exception();

            threads.Add(new ThreadItem
            {
                Head = head,
                Body = body,
                Id = (ulong)threads.Count + 1,
            });
        }

        public async Task SendMessage(string body, ulong threadId)
        {
            if (!_loggedIn)
                throw new Exception();
            var thread = threads.Find(t => t.Id == threadId) ?? throw new Exception();
            //thread.Messages ??= new ChatMessages();
            //thread.Messages.Messages.Add(new ChatMessage
            //{
            //    Body = body,
            //    Time = DateTime.Now,
            //});
        }

        public Task<IEnumerable<UserData>> RequestUsersOnline(ulong threadId)
        {
            throw new NotImplementedException();
        }

        public async Task<ChatMessages> RequestMessagesForThread(ThreadItem thread)
        {
            if (!_loggedIn)
                throw new Exception();
            //var xxx = threads.Find(t => t.Id == thread.Id)?.Messages;
            //if (xxx != null)
            //{
            //    return new ChatMessages
            //    {
            //        Messages = new ObservableCollection<ChatMessage>(xxx.Messages)
            //    };
            //}

            return new ChatMessages();
        }

        public void Dispose()
        {
        }
    }
}