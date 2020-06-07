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
            this.Push(new ChooseServerViewModel(this, connection, storage));
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