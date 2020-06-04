using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Logging;
using Avalonia.Logging.Serilog;
using Avalonia.Threading;
using DesktopFrontend.Models;
using DynamicData.Binding;
using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INavigationStack
    {
        private ViewModelBase? _currentContent;

        // ReSharper disable once MemberCanBePrivate.Global
        public ViewModelBase? CurrentContent
        {
            // ReSharper disable once UnusedMember.Global
            get => _currentContent;
            private set => this.RaiseAndSetIfChanged(ref _currentContent, value);
        }

        public MainWindowViewModel(IServerConnection connection, DataStorage storage)
        {
            connection.Connect()
                .ToObservable()
                .SelectMany(async connected =>
                {
                    if (connected)
                    {
                        await Login(connection, storage);
                    }
                    else
                    {
                        Log.Warn(Log.Areas.Network, this,
                            "Could not connect to the server");
                        var retry = new RetryConnectViewModel();
                        Push(retry);
                        retry.RetryAttempt.SelectMany(async _ => await connection.Connect())
                            .Where(didConnect => didConnect)
                            .Take(1)
                            // async subscribe bad, idk why
                            // https://stackoverflow.com/questions/24843000/reactive-extensions-subscribe-calling-await
                            .SelectMany(async _ =>
                            {
                                Pop();
                                await Login(connection, storage);
                                return default(Unit);
                            })
                            .Subscribe();
                    }

                    return default(Unit);
                })
                .Subscribe();
        }

        private async Task Login(IServerConnection connection, DataStorage storage)
        {
            var cred = storage.RetrieveCredentials();
            if (cred != null &&
                await connection.LogInWithCredentials(cred.Value.login, cred.Value.password))
            {
                Push(new ChatViewModel(this, connection));
                Log.Info(Log.Areas.Network, this,
                    $"Logged in successfully as {cred.Value.login}");
            }
            else
            {
                storage.ResetCredentials();
                Push(new LoginViewModel(this, connection));
            }
        }

        #region INavigationStack

        private readonly Stack<ViewModelBase> _navigation = new Stack<ViewModelBase>();

        public ViewModelBase Pop()
        {
            var t = _navigation.Pop();
            CurrentContent = _navigation.Count > 0 ? _navigation.Peek() : null;
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