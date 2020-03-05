using System;
using System.Collections.Generic;
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
        public ReactiveCommand<Unit, Unit> TryConneсt { get; }

        private bool _isConnected;

        public bool IsConnected
        {
            get => _isConnected;
            set => this.RaiseAndSetIfChanged(ref _isConnected, value);
        }
        
        public LoginViewModel(ServerConnection connection)
        {
            TryConneсt = ReactiveCommand.CreateFromTask(async () =>
            {
                var task = connection.Connect();
                task.ToObservable().Subscribe(_ => IsConnected = true);
                await task;
            });
        }
    }
}