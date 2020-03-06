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
        public ReactiveCommand<Unit, bool> ConneсtWithCredentials { get; }

        public ReactiveCommand<Unit, bool> CreateNewAccount { get; }

        public LoginViewModel(IServerConnection connection)
        {
            // TODO: load credentials from disk
            ConneсtWithCredentials =
                ReactiveCommand.CreateFromTask(async () => await connection.LogInWithCredentials("blah", "blah"));
            CreateNewAccount = ReactiveCommand.CreateFromTask(async () => await connection.RequestANewAccount());
        }
    }
}