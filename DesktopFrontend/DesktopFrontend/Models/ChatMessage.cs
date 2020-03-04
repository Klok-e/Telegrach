using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Animation;
using Avalonia.Threading;

namespace DesktopFrontend.Models
{
    public class ChatMessage
    {
        public DateTime Time { get; set; }
        public string Body { get; set; }
    }

    public class ChatMessages
    {
        //public float Interval { get; set; } = 0.5f;
        //private IDisposable timer;

        public ChatMessages()
        {
            Messages = new ObservableCollection<ChatMessage>();
            //var counter = 0;
            //timer = DispatcherTimer.Run(() =>
            //    {
            //        Messages.Add(new ChatMessage {Time = DateTime.Now, Body = $"interval {counter++}"});
            //        return true;
            //    },
            //    TimeSpan.FromSeconds(Interval));
        }

        public ObservableCollection<ChatMessage> Messages { get; }
    }
}