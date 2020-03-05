using System;
using System.Collections.Generic;
using System.Text;
using DesktopFrontend.Models;
using DynamicData.Binding;

namespace DesktopFrontend.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public ChatViewModel Chat { get; }

        public LoginViewModel LoginViewModel { get; }

        public ViewModelBase CurrentContent { get; private set; }

        public MainWindowViewModel(ServerConnection connection)
        {
            LoginViewModel = new LoginViewModel(connection);
            Chat = new ChatViewModel();
            CurrentContent = LoginViewModel;

            LoginViewModel.WhenPropertyChanged(lo => lo.IsConnected)
                .Subscribe(v =>
                {
                    if (v.Value)
                        CurrentContent = Chat;
                });
        }
    }
}