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

        public string Head { get; set; }
        public string Body { get; set; }
        public string Name => Head.PadRight(NameCharCount)[..NameCharCount].Trim();
        public ulong Id { get; set; }
    }

    public class ThreadMessages
    {
        public ThreadItem Thread { get; set; }
        public ChatMessages Messages { get; set; }
    }

    public class ThreadSet
    {
        public ThreadSet()
        {
            Threads = new ObservableCollection<ThreadMessages>();
        }

        public ObservableCollection<ThreadMessages> Threads { get; }
    }
}