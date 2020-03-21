using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData;


namespace DesktopFrontend.Models
{
    public class MockServerConnection : IServerConnection
    {
        public bool IsConnected { get; private set; }

        public async Task<bool> Connect()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return IsConnected = true;
        }

        public async Task<bool> LogInWithCredentials(string user, string pass)
        {
            return user == "rwerwer" && pass == "564756868";
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
                new ThreadItem("мозкоподібні структури", "dasdasdasd", 1),
                new ThreadItem("блаблабла", "dasdasdasd", 2),
            }
        };

        public async Task<ThreadSet> RequestThreadSet()
        {
            var threadSet = new ThreadSet();
            threadSet.Threads.AddRange(threads);
            return threadSet;
        }

        public async Task CreateThread(string head, string body)
        {
            // uncomment to test throwing
            // throw new Exception();

            threads.Add(new ThreadItem(head, body, (ulong)threads.Count + 1));
        }

        public async Task<ChatMessages> RequestMessagesForThread(ThreadItem thread)
        {
            if (thread.Id == 1)
            {
                var msgs = new ChatMessages();
                msgs.Messages.AddRange(new[]
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
                });
                return msgs;
            }

            if (thread.Id == 2)
            {
                var msgs = new ChatMessages();
                msgs.Messages.AddRange(new[]
                {
                    new ChatMessage
                    {
                        Body = "anyone here",
                        Time = DateTime.Now,
                    },
                });
                return msgs;
            }

            return new ChatMessages();
        }
    }
}