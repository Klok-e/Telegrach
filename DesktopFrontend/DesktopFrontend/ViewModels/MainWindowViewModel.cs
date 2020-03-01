using System;
using System.Collections.Generic;
using System.Text;

namespace DesktopFrontend.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            Chat = new ChatViewModel();
        }

        public ChatViewModel Chat { get; }
    }
}