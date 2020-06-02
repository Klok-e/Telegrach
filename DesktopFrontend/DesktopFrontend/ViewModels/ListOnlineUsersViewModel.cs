using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using Avalonia.Collections;
using DesktopFrontend.Models;
using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    public class ListOnlineUsersViewModel : ViewModelBase
    {
        // ReSharper disable once MemberCanBePrivate.Global UnusedAutoPropertyAccessor.Global
        public string ThreadHead { get; }
        public ReactiveCommand<Unit, Unit> Back { get; }

        // ReSharper disable once MemberCanBePrivate.Global UnusedAutoPropertyAccessor.Global
        public AvaloniaList<UserData> UsersOnline { get; }

        public ListOnlineUsersViewModel(string threadHead, IEnumerable<UserData> users)
        {
            ThreadHead = threadHead;
            UsersOnline = new AvaloniaList<UserData>(users);
            Back = ReactiveCommand.Create(() => { });
            Back.ThrownExceptions.Subscribe(
                e => Log.Error(Log.Areas.Network, this, e.ToString()));
        }
    }
}