using System;
using System.Collections.Generic;
using System.Text;

namespace DesktopFrontend.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Hello World!";
        
        public LoginWindowViewModel LoginViewModel { get; }

        public MainWindowViewModel()
        {
            LoginViewModel=new LoginWindowViewModel();
        }
    }
}
