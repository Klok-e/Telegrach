using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Avalonia.Animation;
using Avalonia.Threading;

namespace DesktopFrontend.Models
{
    public class ThreadItem
    {
        public const int NameCharCount = 15;

        public string Head { get; }
        public string Body { get; }
        public string Name => Head.PadRight(NameCharCount)[..NameCharCount].Trim();
        public ulong Id { get; }

        public ChatMessages? Messages { get; set; }

        public ThreadItem(string head, string body, ulong id)
        {
            Head = head;
            Body = body;
            Id = id;
        }
    }

    public class ThreadSet
    {
        public ThreadSet()
        {
            Threads = new ObservableCollection<ThreadItem>();
        }

        public ObservableCollection<ThreadItem> Threads { get; }
    }
}