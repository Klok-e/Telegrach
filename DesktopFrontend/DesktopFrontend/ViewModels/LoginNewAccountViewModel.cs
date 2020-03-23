using System.Drawing;
using System.Reactive;
using System.Threading.Tasks;
using System;
using Avalonia.Logging;
using Avalonia.Threading;
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

        public ReactiveCommand<Unit, bool> SignIn { get; }

        public LoginNewAccountViewModel(INavigationStack stack, IServerConnection connection)
        {
            var c = new CaptchaViewModel(connection);
            Captcha = c;
            string login = null;
            string pass = null;
            c.CaptchaPassed.Subscribe(pl =>
            {
                CaptchaPassed = true;
                login = pl.login;
                pass = pl.pass;
                Captcha = new LoginShowPasswdViewModel(login, pass);
            });
            Back = ReactiveCommand.Create(() => { stack.Pop(); });
            var canExec = this.WhenAny(x => x.CaptchaPassed,
                s => s.Value);
            SignIn = ReactiveCommand.CreateFromTask(async () =>
            {
                if (await connection.LogInWithCredentials(login!, pass!))
                {
                    Log.Info(Log.Areas.Network, this,
                        $"Logged in successfully as {login}");
                    stack.Push(new ChatViewModel(stack, connection));

                    new CredentialsStorage().Store(login, pass);
                    return true;
                }

                Logger.Sink.Log(LogEventLevel.Warning, "Network", this,
                    $"Could not log in as {login}");
                return false;
            }, canExec);
        }
    }
}