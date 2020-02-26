using System.Collections.ObjectModel;
using DesktopFrontend.Models;

namespace DesktopFrontend.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private ChatMessages model;

        public ChatViewModel()
        {
            model = new ChatMessages();
        }

        public ObservableCollection<ChatMessage> Messages => model.Messages;
    }
}