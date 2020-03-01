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

        public ChatViewModel()
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

        public ReactiveCommand<Unit, Unit> SendMessage { get; }

        private string _description;

        public string CurrentMessage
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        public ObservableCollection<ChatMessage> Messages => _model.Messages;
    }
}