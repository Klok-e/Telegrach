using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using DesktopFrontend.Models;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        public ChatViewModel(INavigationStack stack, IServerConnection connection)
        {
            ChatInit(connection);
            ThreadSearchInit(stack, connection);
        }

        ~ChatViewModel()
        {
            //_cancelServerQuerying.Cancel();
        }

        #region Chat

        public ReactiveCommand<Unit, Unit> SendMessage { get; private set; }

        private string _currentMessage = "";

        public string CurrentMessage
        {
            get => _currentMessage;
            set => this.RaiseAndSetIfChanged(ref _currentMessage, value);
        }

        private ChatMessages _messagesModel;
        private ThreadMessages? _currentThread;

        public ThreadMessages? CurrentThread
        {
            get => _currentThread;
            set
            {
                if (_currentThread != value)
                {
                    _currentThread = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(CurrThreadNotNull));
                }
            }
        }

        public bool CurrThreadNotNull => CurrentThread != null;

        public ChatMessages MessagesModel
        {
            get => _messagesModel;
            set => this.RaiseAndSetIfChanged(ref _messagesModel, value);
        }

        public ObservableCollection<ChatMessage> Messages { get; private set; }

        private void ChatInit(IServerConnection connection)
        {
            MessagesModel = new ChatMessages();
            Messages = new ObservableCollection<ChatMessage>();

            var canSend = this.WhenAnyValue(
                x => x.CurrentMessage,
                x => x.CurrentThread,
                (msg, th) => !string.IsNullOrEmpty(msg) && th != null
            );
            SendMessage = ReactiveCommand.CreateFromTask(
                async () => { await connection.SendMessage(CurrentMessage, CurrentThread.Thread.Id); },
                canSend);
            SendMessage.Subscribe(_ => CurrentMessage = string.Empty);
            SendMessage.ThrownExceptions.Subscribe(
                e => Log.Error(Log.Areas.Network, this, e.ToString()));
        }

        #endregion

        #region Threads

        private string _threadSearch = string.Empty;

        public string ThreadSearch
        {
            get => _threadSearch;
            set => this.RaiseAndSetIfChanged(ref _threadSearch, value);
        }

        public ReactiveCommand<Unit, Unit> CreateNewThread { get; private set; }

        private ThreadSet _threadSet;
        public ObservableCollection<ThreadMessages> Threads => _threadSet.Threads;
        public ReactiveCommand<ThreadMessages, Unit> SelectThread { get; private set; }

        private void ThreadSearchInit(INavigationStack stack, IServerConnection connection)
        {
            _threadSet = new ThreadSet();
            connection.NewThreadArrived.Subscribe(newThread =>
            {
                Threads.Add(new ThreadMessages
                {
                    Thread = newThread,
                    Messages = new ChatMessages()
                });
            });

            CreateNewThread = ReactiveCommand.Create(() =>
            {
                var createThread = new CreateNewThreadViewModel(connection);
                stack.Push(createThread);

                createThread.Create
                    .Merge(createThread.Cancel)
                    .Subscribe(_ => { stack.Pop(); });
            });
            CreateNewThread.ThrownExceptions.Subscribe(
                e => Log.Error(Log.Areas.Network, this, e.ToString()));

            SelectThread = ReactiveCommand.Create<ThreadMessages>(thread =>
            {
                Log.Info(Log.Areas.Application, this, $"Selected thread with name {thread.Thread.Name}");
                SetMessages(thread.Messages);
                CurrentThread = thread;
            });
            SelectThread.ThrownExceptions.Subscribe(
                e => Log.Error(Log.Areas.Network, this, e.ToString()));
        }

        #endregion

        private void SetMessages(ChatMessages messages)
        {
            MessagesModel = messages;
            Messages.Clear();
            Messages.AddRange(MessagesModel.Messages);
        }
    }
}