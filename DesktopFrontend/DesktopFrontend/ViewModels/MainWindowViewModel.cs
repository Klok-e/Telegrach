using System;
using System.Collections.Generic;
using System.Text;
using DesktopFrontend.Models;
using DynamicData.Binding;
using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public ChatViewModel Chat { get; }

        public LoginViewModel LoginViewModel { get; }

        private ViewModelBase _currentContent;

        public ViewModelBase CurrentContent
        {
            get => _currentContent;
            private set => this.RaiseAndSetIfChanged(ref _currentContent, value);
        }

        public MainWindowViewModel(IServerConnection connection)
        {
            LoginViewModel = new LoginViewModel(connection);
            Chat = new ChatViewModel();
            CurrentContent = LoginViewModel;

            LoginViewModel.TryConneсt
                .Subscribe(v =>
                {
                    if (v)
                        CurrentContent = Chat;
                });
        }
    }
}