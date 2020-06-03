using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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

        #region Chat

        // ReSharper disable once MemberCanBePrivate.Global
        public ReactiveCommand<Unit, Unit> SendMessage { get; private set; }

        private string _currentMessage = "";

        // ReSharper disable once MemberCanBePrivate.Global
        public string CurrentMessage
        {
            get => _currentMessage;
            set => this.RaiseAndSetIfChanged(ref _currentMessage, value);
        }

        private ChatMessages _messagesModel;
        private ThreadMessages? _currentThread;

        // ReSharper disable once MemberCanBePrivate.Global
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

        // ReSharper disable once MemberCanBePrivate.Global
        public ChatMessages MessagesModel
        {
            get => _messagesModel;
            set => this.RaiseAndSetIfChanged(ref _messagesModel, value);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public ObservableCollection<ChatMessage> Messages { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public ReactiveCommand<ChatMessage, Unit> ActivateMediaMessage { get; private set; }

        public ReactiveCommand<Unit, Unit> DeactivateMediaMessage { get; private set; }

        private bool _isMediaActive;

        // ReSharper disable once MemberCanBePrivate.Global UnusedMember.Global
        public bool IsMediaActive
        {
            get => _isMediaActive;
            private set => this.RaiseAndSetIfChanged(ref _isMediaActive, value);
        }

        private Bitmap _activeImage;

        // ReSharper disable once MemberCanBePrivate.Global UnusedMember.Global
        public Bitmap ActiveImage
        {
            get => _activeImage;
            private set => this.RaiseAndSetIfChanged(ref _activeImage, value);
        }

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
                async () => { await connection.SendMessage(CurrentMessage, CurrentThread!.Thread.Id); },
                canSend);
            SendMessage.Subscribe(_ => CurrentMessage = string.Empty);
            SendMessage.LogErrors(Log.Areas.Network, this);

            ActivateMediaMessage = ReactiveCommand.Create<ChatMessage>(message =>
            {
                Log.Info(Log.Areas.Application, this, $"Activate image for message {message.Time}");
                if (message.File == null)
                {
                    Log.Warn(Log.Areas.Application, this,
                        $"Activate image for message {message.Time} failed: file not present in the message");
                    var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                    var bitmap = new Bitmap(assets.Open(new Uri("avares://DesktopFrontend/Assets/generic_image.png")));
                    ActiveImage = bitmap;
                    IsMediaActive = true;
                    return;
                }

                switch (message.File.Type)
                {
                    case FileType.Image:
                        ActiveImage = message.File.Bitmap();
                        IsMediaActive = true;
                        break;
                    default:
                        //Process.Start(@"c:\myPDF.pdf");
                        break;
                }
            });
            ActivateMediaMessage.LogErrors(Log.Areas.Network, this);

            DeactivateMediaMessage = ReactiveCommand.Create(() => { IsMediaActive = false; });
            DeactivateMediaMessage.LogErrors(Log.Areas.Network, this);
        }

        #endregion

        #region Threads

        private string _threadSearch = string.Empty;

        // ReSharper disable once UnusedMember.Global
        public string ThreadSearch
        {
            get => _threadSearch;
            set => this.RaiseAndSetIfChanged(ref _threadSearch, value);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public ReactiveCommand<Unit, Unit> CreateNewThread { get; private set; }

        private ThreadSet _threadSet;

        // ReSharper disable once MemberCanBePrivate.Global
        public ObservableCollection<ThreadMessages> Threads => _threadSet.Threads;

        // ReSharper disable once MemberCanBePrivate.Global
        public ReactiveCommand<ThreadMessages, Unit> SelectThread { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public ReactiveCommand<Unit, Unit> ShowOnline { get; private set; }

        private void ThreadSearchInit(INavigationStack stack, IServerConnection connection)
        {
            _threadSet = new ThreadSet();

            connection.NewThreadArrived
                // observe on the UI thread
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(newThread =>
                {
                    Threads.Add(new ThreadMessages
                    {
                        Thread = newThread,
                        Messages = new ChatMessages()
                    });
                });

            connection.NewMessageArrived
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(newMessage =>
                {
                    Threads
                        .First(msg => msg.Thread.Id == newMessage.ThreadId).Messages.Messages
                        .Add(newMessage.Message);
                    if (CurrentThread != null && newMessage.ThreadId == CurrentThread.Thread.Id)
                        Messages.Add(newMessage.Message);
                });

            CreateNewThread = ReactiveCommand.Create(() =>
            {
                var createThread = new CreateNewThreadViewModel(connection);
                stack.Push(createThread);

                createThread.Create
                    .Merge(createThread.Cancel)
                    .Subscribe(_ => { stack.Pop(); });
            });
            CreateNewThread.LogErrors(Log.Areas.Network, this);

            SelectThread = ReactiveCommand.Create<ThreadMessages>(thread =>
            {
                Log.Info(Log.Areas.Application, this, $"Selected thread with name {thread.Thread.Name}");
                SetMessages(thread.Messages);
                CurrentThread = thread;
            });
            SelectThread.LogErrors(Log.Areas.Network, this);

            ShowOnline = ReactiveCommand.CreateFromTask(async () =>
            {
                Log.Info(Log.Areas.Application, this,
                    $"Showing online users in thread with name {CurrentThread!.Thread.Name}");
                var online = await connection.RequestUsersOnline(CurrentThread!.Thread.Id);
                var listUsers = new ListOnlineUsersViewModel(CurrentThread.Thread.Head, online);
                stack.Push(listUsers);
                listUsers.Back.Subscribe(_ => { stack.Pop(); });
            });
            ShowOnline.LogErrors(Log.Areas.Network, this);
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