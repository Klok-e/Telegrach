using System;
using System.Reactive;
using Avalonia.Logging;
using DesktopFrontend.Models;
using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    public class CreateNewThreadViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, ThreadItem> Create { get; }

        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public string Head
        {
            get => _head;
            set => this.RaiseAndSetIfChanged(ref _head, value);
        }

        public string Body
        {
            get => _body;
            set => this.RaiseAndSetIfChanged(ref _body, value);
        }

        private string _head = string.Empty;
        private string _body = string.Empty;

        public CreateNewThreadViewModel(IServerConnection connection)
        {
            var canOk = this.WhenAny(a => a.Head,
                h => !string.IsNullOrEmpty(h.GetValue()));

            Create = ReactiveCommand.CreateFromTask(async () => await connection.CreateThread(Head, Body),
                canOk);
            // Create thread throws if the thread wasn't created successfully
            Create.ThrownExceptions.Subscribe(e =>
            {
                Logger.Sink.Log(LogEventLevel.Warning, "Network", this,
                    $"Exception while creating a thread: {e.Message}; {e.StackTrace}");
                // TODO: add a nice pop up explaining the situation
            });
            Cancel = ReactiveCommand.Create(() => { });
        }
    }
}