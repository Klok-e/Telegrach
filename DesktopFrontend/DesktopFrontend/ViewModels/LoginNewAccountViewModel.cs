using System.Drawing;
using System.Reactive;
using System.Threading.Tasks;
using System;
using DesktopFrontend.Models;
using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    public class LoginNewAccountViewModel : ViewModelBase
    {
        private bool _captchaPassed;

        public bool CaptchaPassed
        {
            get => _captchaPassed;
            set => this.RaiseAndSetIfChanged(ref _captchaPassed, value);
        }

        private ViewModelBase _captcha;

        public ViewModelBase Captcha
        {
            get => _captcha;
            set => this.RaiseAndSetIfChanged(ref _captcha, value);
        }

        public ReactiveCommand<Unit, Unit> Back { get; }

        public ReactiveCommand<Unit, Unit> SignIn { get; }

        public LoginNewAccountViewModel(INavigationStack stack, IServerConnection connection)
        {
            var c = new CaptchaViewModel(connection);
            Captcha = c;
            c.CaptchaPassed.Subscribe(pl =>
            {
                stack.Push(new ChatViewModel(connection));
                return true;
            });
            Back = ReactiveCommand.Create(() => { stack.Pop(); });
            var canExec = this.WhenAny(x => x.CaptchaPassed,
                s => s.Value);
            SignIn = ReactiveCommand.CreateFromTask(async () => { stack.Push(new ChatViewModel(connection)); }, canExec);
        }
    }
}