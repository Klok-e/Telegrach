using System;
using System.Collections.Generic;
using System.Text;

namespace DesktopFrontend.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public ChatViewModel Chat { get; }
        
        public LoginWindowViewModel LoginViewModel { get; }

        public MainWindowViewModel()
        {
            LoginViewModel = new LoginWindowViewModel();
            Chat = new ChatViewModel();
        }
    }
}