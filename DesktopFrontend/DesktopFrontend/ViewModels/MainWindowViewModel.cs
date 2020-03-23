using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using Avalonia.Logging;
using Avalonia.Logging.Serilog;
using DesktopFrontend.Models;
using DynamicData.Binding;
using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    class MainWindowViewModel : ViewModelBase, INavigationStack
    {
        private ViewModelBase _currentContent;

        public ViewModelBase CurrentContent
        {
            get => _currentContent;
            private set => this.RaiseAndSetIfChanged(ref _currentContent, value);
        }

        public MainWindowViewModel(IServerConnection connection)
        {
            connection.Connect()
                .ToObservable()
                .Subscribe(async connected =>
                {
                    if (connected)
                    {
                        Logger.Sink.Log(LogEventLevel.Information, "Network", this,
                            "Connected to the server");

                        var storage = new CredentialsStorage();
                        var cred = storage.Retrieve();
                        if (cred != null)
                        {
                            if (await connection.LogInWithCredentials(cred.Value.login, cred.Value.password))
                            {
                                Push(new ChatViewModel(this, connection));
                                Log.Info(Log.Areas.Network, this,
                                    $"Logged in successfully as {cred.Value.login}");
                                return;
                            }
                        }

                        storage.ResetConfig();
                        Push(new LoginViewModel(this, connection));
                    }
                    else
                    {
                        Logger.Sink.Log(LogEventLevel.Error, "Network", this,
                            "Could not connect to the server");
                    }
                });
        }

        #region INavigationStack

        private Stack<ViewModelBase> _navigation = new Stack<ViewModelBase>();

        public ViewModelBase Pop()
        {
            var t = _navigation.Pop();
            CurrentContent = _navigation.Peek();
            return t;
        }

        public void Push(ViewModelBase vm)
        {
            _navigation.Push(vm);
            CurrentContent = vm;
        }

        public ViewModelBase ReplaceTop(ViewModelBase vm)
        {
            var t = _navigation.Pop();
            _navigation.Push(vm);
            CurrentContent = vm;
            return t;
        }

        #endregion
    }
}