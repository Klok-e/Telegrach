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
        /// <summary>
        /// A lock that doesn't allow to simultaneously execute async methods (buttons) and query server
        /// </summary>
        private SemaphoreSlim _semaphore;

        private CancellationTokenSource _cancelServerQuerying;

        public ChatViewModel(INavigationStack stack, IServerConnection connection)
        {
            ChatInit(connection);
            ThreadSearchInit(stack, connection);
            _cancelServerQuerying = new CancellationTokenSource();
            _semaphore = new SemaphoreSlim(1, 1);
            Task.Run(async () =>
            {
                while (true)
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        await UpdateThreads(connection);

                        // TODO: make this request only new messages instead of ALL the messages
                        // copy because doesn't work otherwise
                        var threads = new ObservableCollection<ThreadItem>(Threads);
                        foreach (var (chatMessages, thread) in (await Task.WhenAll(
                            threads.Select(connection.RequestMessagesForThread))).Zip(threads))
                        {
                            // if no changes then continue (if anyNew is null or false)
                            var threadMessages = thread.Messages?.Messages;
                            if (threadMessages != null && chatMessages.Messages.Count == threadMessages.Count)
                                continue;

                            thread.Messages = chatMessages;
                            if (ReferenceEquals(CurrentThread, thread))
                            {
                                SetMessages(chatMessages);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(Log.Areas.Application, this, $"{e}");
                        throw;
                    }
                    finally
                    {
                        _semaphore.Release();
                    }

                    await Task.Delay(TimeSpan.FromSeconds(0.4), _cancelServerQuerying.Token);
                    if (_cancelServerQuerying.Token.IsCancellationRequested)
                        break;
                }
            }, _cancelServerQuerying.Token);
        }

        ~ChatViewModel()
        {
            _cancelServerQuerying.Cancel();
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

        public ThreadItem? CurrentThread
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
            SendMessage = ReactiveCommand.CreateFromTask(async () =>
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        await connection.SendMessage(CurrentMessage, CurrentThread.Id);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                },
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
        public ObservableCollection<ThreadItem> Threads => _threadSet.Threads;

        public ReactiveCommand<Unit, Unit> UpdateThreadList { get; private set; }

        public ReactiveCommand<ThreadItem, Unit> SelectThread { get; private set; }

        private void ThreadSearchInit(INavigationStack stack, IServerConnection connection)
        {
            _threadSet = new ThreadSet();
            UpdateThreadList = ReactiveCommand.CreateFromTask(async () =>
            {
                await _semaphore.WaitAsync();
                try
                {
                    await UpdateThreads(connection);
                    if (CurrentThread != null)
                    {
                        if (CurrentThread.Messages == null)
                        {
                            CurrentThread.Messages = await connection.RequestMessagesForThread(CurrentThread);
                        }

                        SetMessages(CurrentThread.Messages);
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            });
            UpdateThreadList.Execute();
            UpdateThreadList.ThrownExceptions.Subscribe(
                e => Log.Error(Log.Areas.Network, this, e.ToString()));

            CreateNewThread = ReactiveCommand.Create(() =>
            {
                var createThread = new CreateNewThreadViewModel(connection, _semaphore);
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
                await _semaphore.WaitAsync();
                try
                {
                    Log.Info(Log.Areas.Application, this, $"Selected thread with name {thread.Name}");
                    if (thread.Messages == null)
                    {
                        thread.Messages = await connection.RequestMessagesForThread(thread);
                    }

                    SetMessages(thread.Messages);
                    CurrentThread = thread;
                }
                finally
                {
                    _semaphore.Release();
                }
            });
            SelectThread.ThrownExceptions.Subscribe(
                e => Log.Error(Log.Areas.Network, this, e.ToString()));
        }

        #endregion

        private void SetMessages(ChatMessages messages)
        {
            // TODO: doesn't work without this, fix maybe
            Dispatcher.UIThread.Post(() =>
            {
                MessagesModel = messages;
                Messages.Clear();
                Messages.AddRange(MessagesModel.Messages);
            });
        }

        private async Task UpdateThreads(IServerConnection connection)
        {
            var threadSet = await connection.RequestThreadSet();

            Dispatcher.UIThread.Post(() =>
            {
                // if any changes
                if (threadSet.Threads.Count != Threads.Count)
                {
                    Threads.Clear();
                    Threads.AddRange(threadSet.Threads);
                    if (CurrentThread != null)
                    {
                        CurrentThread = Threads.First(t => t.Id == CurrentThread.Id);
                    }
                }
            });
        }
    }
}