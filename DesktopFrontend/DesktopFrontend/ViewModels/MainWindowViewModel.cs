using System;
using System.Collections.Generic;
using System.Text;

namespace DesktopFrontend.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public ChatViewModel Chat { get; }

        public LoginViewModel LoginViewModel { get; }
        
        public ViewModelBase CurrentContent { get; private set; }

        public MainWindowViewModel()
        {
            LoginViewModel = new LoginViewModel();
            Chat = new ChatViewModel();
            CurrentContent = LoginViewModel;
        }
    }
}