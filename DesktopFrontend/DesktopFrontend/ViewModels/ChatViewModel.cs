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
            ChatInit(connection);
            ThreadSearchInit(stack, connection);

            DispatcherTimer.Run(() =>
            {
                Debug.Assert(Dispatcher.UIThread.CheckAccess());
                if (!_finished)
                    return true;

                _finish = new TaskCompletionSource<Unit>();
                _finished = false;
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    try
                    {
                        Debug.Assert(Dispatcher.UIThread.CheckAccess());

                        await UpdateThreads(connection);
                        
                        // TODO: make this request only new messages instead of ALL the messages
                        foreach (var (chatMessages, thread) in (await Task.WhenAll(
                            Threads.Select(connection.RequestMessagesForThread))).Zip(Threads))
                        {
                            // if no changes then continue (if anyNew is null or false)
                            var threadMessages = thread.Messages?.Messages;
                            if (threadMessages != null && chatMessages.Messages.Count == threadMessages.Count)
                                continue;

                            thread.Messages = chatMessages;
                            if (ReferenceEquals(_currentThread, thread))
                            {
                                SetMessages(chatMessages);
                            }
                        }

                        _finished = true;
                        _finish.SetResult(default);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });

                return true;
            }, TimeSpan.FromSeconds(0.4));
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

        private void ChatInit(IServerConnection connection)
        {
            MessagesModel = new ChatMessages();
            Messages = new ObservableCollection<ChatMessage>();

            var isSendEnabled = this.WhenAnyValue(
                x => x.CurrentMessage,
                x => !string.IsNullOrEmpty(x)
            );
            SendMessage = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    if (!_finished)
                        await _finish.Task;

                    Debug.Assert(_finished);
                    await connection.SendMessage(CurrentMessage, _currentThread.Id);
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
                //if (_currentThread != null)
                //{
                //    if (_currentThread.Messages == null)
                //    {
                //        _currentThread.Messages = await connection.RequestMessagesForThread(_currentThread);
                //    }
//
                //    SetMessages(_currentThread.Messages);
                //}
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
            if (threadSet.Threads.Count != Threads.Count)
            {
                Threads.Clear();
                Threads.AddRange(threadSet.Threads);
                if (_currentThread != null)
                {
                    _currentThread = Threads.First(t => t.Id == _currentThread.Id);
                }
            }
        }
    }
}