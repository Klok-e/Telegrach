using System;
using System.Collections.ObjectModel;
using System.Reactive;
using DesktopFrontend.Models;
using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private ChatMessages _model;

        public ChatViewModel(IServerConnection connection)
        {
            ChatInit();
            ThreadSearchInit(connection);
        }

        #region Chat

        public ReactiveCommand<Unit, Unit> SendMessage { get; private set; }

        private string _currentMessage = "";

        public string CurrentMessage
        {
            get => _currentMessage;
            set => this.RaiseAndSetIfChanged(ref _currentMessage, value);
        }

        public ObservableCollection<ChatMessage> Messages => _model.Messages;

        private void ChatInit()
        {
            _model = new ChatMessages();
            var isSendEnabled = this.WhenAnyValue(
                x => x.CurrentMessage,
                x => !string.IsNullOrEmpty(x)
            );
            SendMessage = ReactiveCommand.Create(() =>
                {
                    _model.Messages.Add(new ChatMessage
                    {
                        Body = CurrentMessage,
                        Time = DateTime.Now
                    });
                },
                isSendEnabled);
            SendMessage.Subscribe(_ => CurrentMessage = string.Empty);
        }

        #endregion

        #region Threads

        private string _threadSearch = string.Empty;

        public string ThreadSearch
        {
            get => _threadSearch;
            set => this.RaiseAndSetIfChanged(ref _threadSearch, value);
        }

        public ReactiveCommand<Unit, Unit> GetTreadList { get; private set; }

        private ThreadSet _threadSet;
        public ObservableCollection<ThreadItem> Threads => _threadSet.Threads;

        private void ThreadSearchInit(IServerConnection connection)
        {
            _threadSet = new ThreadSet();
            var isSendEnabled = this.WhenAnyValue(
                x => x.ThreadSearch,
                x => !string.IsNullOrEmpty(x)
            );

            GetTreadList = ReactiveCommand.Create(() =>
            {
#if DEBUG
                Threads.Add(new ThreadItem {Name = _threadSearch});
#else
                Threads.Clear();
                foreach (var item in connection.RequestThreadSet(_threadSearch).Result)
                     _threadSet.Threads.Add(new ThreadItem { Name = item });
#endif
            }, isSendEnabled);
            GetTreadList.Subscribe(_ => ThreadSearch = string.Empty);
        }

        #endregion
    }
}