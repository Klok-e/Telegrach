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
        public string Name { get; set; }

    }



    public class ThreadSet
    {
        public ThreadSet()
        {
            Threads = new ObservableCollection<ThreadItem>();
        }

        public ObservableCollection<ThreadItem> Threads { get; private set; }
    }
}
