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
        public string Body { get; set; } = "";

        public MediaFile? File { get; set; }
    }

    public class ChatMessageInThread
    {
        public ulong ThreadId { get; set; }
        public ChatMessage Message { get; set; } = new ChatMessage();
    }

    public class ChatMessages
    {
        public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();
    }
}