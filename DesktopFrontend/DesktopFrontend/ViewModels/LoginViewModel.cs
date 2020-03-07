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
    public class LoginViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> Credentials { get; }

        public ReactiveCommand<Unit, Unit> NewAccount { get; }

        public LoginViewModel(INavigationStack stack, IServerConnection connection)
        {
            Credentials = ReactiveCommand.Create(() => stack.Push(new LoginCredentialsViewModel(stack, connection)));
            NewAccount = ReactiveCommand.Create(() => stack.Push(new LoginNewAccountView(stack, connection)));
        }
    }
}