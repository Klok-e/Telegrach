using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using Avalonia.Interactivity;
using DesktopFrontend.Models;
using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    public class LoginCredentialsViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> Back { get; }

        public ReactiveCommand<Unit, bool> SignIn { get; }
        
        private string _login = "";

        public string Login
        {
            get => _login;
            set => this.RaiseAndSetIfChanged(ref _login, value);
        }
        
        private string _password = "";

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public LoginCredentialsViewModel(INavigationStack stack, IServerConnection connection)
        {
            Back = ReactiveCommand.Create(() => { stack.Pop(); });
            // TODO: make this into a real sign in
            SignIn = ReactiveCommand.Create(() =>
            {
                stack.Push(new ChatViewModel());
                return true;
            });
        }
    }
}