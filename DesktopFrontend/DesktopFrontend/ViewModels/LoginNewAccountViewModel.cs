using System.Drawing;
using System.Reactive;
using System.Threading.Tasks;
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

        private bool _showCaptha;

        public bool ShowCaptha
        {
            get => _showCaptha;
            set => this.RaiseAndSetIfChanged(ref _showCaptha, value);
        }

        private Image _captchaImage;

        public Image CaptchaImage
        {
            get => _captchaImage;
            set => this.RaiseAndSetIfChanged(ref _captchaImage, value);
        }

        private string _capthaAttemptText = "";

        public string CapthaAttemptText
        {
            get => _capthaAttemptText;
            private set => this.RaiseAndSetIfChanged(ref _capthaAttemptText, value);
        }

        public ReactiveCommand<Unit, Unit> TryPassCaptha { get; }

        public ReactiveCommand<Unit, Unit> Back { get; }

        public ReactiveCommand<Unit, Unit> SignIn { get; }

        public LoginNewAccountViewModel(INavigationStack stack, IServerConnection connection)
        {
            var capthaPassed = new TaskCompletionSource<bool>();

            TryPassCaptha = ReactiveCommand.CreateFromTask(async () =>
            {
                if (await connection.TryPassCaptcha(CapthaAttemptText))
                {
                    capthaPassed.SetResult(true);
                }
                else
                {
                    capthaPassed.SetResult(false);
                }
            });

            Back = ReactiveCommand.Create(() => { stack.Pop(); });
            SignIn = ReactiveCommand.CreateFromTask(async () =>
            {
                ShowCaptha = true;
                bool notPassed;
                do
                {
                    CaptchaImage = await connection.RequestCaptcha();
                    notPassed = !await capthaPassed.Task;
                    if (notPassed)
                        capthaPassed = new TaskCompletionSource<bool>();
                } while (notPassed);

                stack.Push(new ChatViewModel());
            });
        }
    }
}