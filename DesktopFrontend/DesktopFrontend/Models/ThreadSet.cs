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
        public string Head { get; }
        public string Body { get; }
        public string Name { get; }
        public ulong Id { get; }

        public ThreadItem(string head, string body, string name, ulong id)
        {
            Head = head;
            Body = body;
            Name = name;
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