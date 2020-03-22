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
        private bool _finished = true;
        private TaskCompletionSource<Unit> _finish;

        public ChatViewModel(INavigationStack stack, IServerConnection connection)
        {
            ChatInit();
            ThreadSearchInit(stack, connection);

            DispatcherTimer.Run(() =>
            {
                Debug.Assert(Dispatcher.UIThread.CheckAccess());
                if (_finished)
                {
                    _finish = new TaskCompletionSource<Unit>();
                    _finished = false;
                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        try
                        {
                            Debug.Assert(Dispatcher.UIThread.CheckAccess());

                            // TODO: make this request only new messages instead of ALL the messages
                            foreach (var (chatMessages, thread) in (await Task.WhenAll(
                                Threads.Select(connection.RequestMessagesForThread))).Zip(Threads))
                            {
                                // if no changes then continue (if anyMessages is null or false)
                                var threadMessages = thread.Messages?.Messages;
                                var anyMessages = threadMessages == null
                                    ? null
                                    : chatMessages?.Messages?.Except(threadMessages)?.Any();
                                if (anyMessages != true)
                                    continue;

                                thread.Messages = chatMessages;
                                if (_currentThread == thread)
                                {
                                    SetMessages(chatMessages);
                                }
                            }

                            await UpdateThreads(connection);

                            _finished = true;
                            _finish.SetResult(default);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
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
                if (!_finished)
                    await _finish.Task;

                Debug.Assert(_finished);
                await UpdateThreads(connection);
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
                if (!_finished)
                    await _finish.Task;

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

        private async Task UpdateThreads(IServerConnection connection)
        {
            var threadSet = await connection.RequestThreadSet();

            // if any changes
            if (threadSet.Threads.Except(Threads).Any())
            {
                Threads.Clear();
                Threads.AddRange(threadSet.Threads);
            }
        }
    }
}