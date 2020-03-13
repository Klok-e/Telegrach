using System.Reactive;
using DesktopFrontend.Models;
using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    public class LoginNewAccountViewModel : ViewModelBase
    {
        private string _receivedPassword = "";

        public string ReceivedPassword
        {
            get => _receivedPassword;
            private set => this.RaiseAndSetIfChanged(ref _receivedPassword, value);
        }

        private string _receivedLogin = "";

        public string ReceivedLogin
        {
            get => _receivedLogin;
            private set => this.RaiseAndSetIfChanged(ref _receivedLogin, value);
        }

        public ReactiveCommand<Unit, Unit> Back { get; }

        public ReactiveCommand<Unit, bool> SignIn { get; }

        public LoginNewAccountViewModel(INavigationStack stack, IServerConnection connection)
        {
            Back = ReactiveCommand.Create(() => { stack.Pop(); });
            // TODO: make this into a real sign in
            SignIn = ReactiveCommand.Create(() =>
            {
                stack.Push(new ChatViewModel(connection));
                return true;
            });
        }
    }
}