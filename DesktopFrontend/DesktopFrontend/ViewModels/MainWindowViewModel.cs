using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using DesktopFrontend.Models;
using DynamicData.Binding;
using ReactiveUI;
using Serilog;
using Serilog.Core;

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
            Push(new LoginViewModel(this, connection));
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