using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
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
        bool _finished = true;

        public ChatViewModel(INavigationStack stack, IServerConnection connection)
        {
            ChatInit();
            ThreadSearchInit(stack, connection);


            DispatcherTimer.Run(() =>
            {
                Debug.Assert(Dispatcher.UIThread.CheckAccess());
                if (_finished)
                {
                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        Debug.Assert(Dispatcher.UIThread.CheckAccess());
                        _finished = false;

                        // TODO: make this request only new messages instead of ALL the messages
                        foreach (var (chatMessages, thread) in (await Task.WhenAll(
                            Threads.Select(connection.RequestMessagesForThread))).Zip(Threads))
                        {
                            thread.Messages = chatMessages;
                            if (_currentThread == thread)
                            {
                                SetMessages(chatMessages);
                            }
                        }

                        UpdateThreadList.Execute();

                        _finished = true;
                    });
                }

                return true;
            }, TimeSpan.FromSeconds(0.1));
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
        private ThreadItem? _currentThread;

        public ChatMessages MessagesModel
        {
            get => _messagesModel;
            set => this.RaiseAndSetIfChanged(ref _messagesModel, value);
        }

        public ObservableCollection<ChatMessage> Messages { get; private set; }

        private void ChatInit()
        {
            MessagesModel = new ChatMessages();
            Messages = new ObservableCollection<ChatMessage>();

            var isSendEnabled = this.WhenAnyValue(
                x => x.CurrentMessage,
                x => !string.IsNullOrEmpty(x)
            );
            SendMessage = ReactiveCommand.Create(() =>
                {
                    MessagesModel.Messages.Add(new ChatMessage
                    {
                        Body = CurrentMessage,
                        Time = DateTime.Now
                    });
                    Messages.Add(MessagesModel.Messages[^1]);
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

        public ReactiveCommand<Unit, Unit> CreateNewThread { get; private set; }

        private ThreadSet _threadSet;
        public ObservableCollection<ThreadItem> Threads => _threadSet.Threads;

        public ReactiveCommand<Unit, Unit> UpdateThreadList { get; private set; }

        public ReactiveCommand<ThreadItem, Unit> SelectThread { get; private set; }

        private void ThreadSearchInit(INavigationStack stack, IServerConnection connection)
        {
            _threadSet = new ThreadSet();
            UpdateThreadList = ReactiveCommand.CreateFromTask(async () =>
            {
                Debug.Assert(_finished);
                Threads.Clear();
                var threadSet = await connection.RequestThreadSet();
                Threads.AddRange(threadSet.Threads);
            });
            UpdateThreadList.Execute();
            UpdateThreadList.ThrownExceptions.Subscribe(
                e => Log.Error(Log.Areas.Network, this, e.ToString()));

            CreateNewThread = ReactiveCommand.Create(() =>
            {
                var createThread = new CreateNewThreadViewModel(connection);
                stack.Push(createThread);

                createThread.Create
                    .Merge(createThread.Cancel)
                    .Subscribe(_ =>
                    {
                        UpdateThreadList.Execute();
                        stack.Pop();
                    });
            });
            CreateNewThread.ThrownExceptions.Subscribe(
                e => Log.Error(Log.Areas.Network, this, e.ToString()));

            SelectThread = ReactiveCommand.CreateFromTask<ThreadItem>(async thread =>
            {
                Debug.Assert(_finished);
                Log.Info(Log.Areas.Application, this, $"Selected thread with name {thread.Name}");
                if (thread.Messages == null)
                {
                    thread.Messages = await connection.RequestMessagesForThread(thread);
                }

                SetMessages(thread.Messages);
                _currentThread = thread;
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