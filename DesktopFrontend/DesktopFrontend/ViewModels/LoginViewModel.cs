using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using DesktopFrontend.Models;
using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit,Unit> Login { get; }
        
        public LoginViewModel()
        {
            Login = ReactiveCommand.Create(() => { Connection.Connect(null, 0); });
        }

        private void GetConnection()
        {
            Connection.Connect(null, 0);
        }
    }
}
