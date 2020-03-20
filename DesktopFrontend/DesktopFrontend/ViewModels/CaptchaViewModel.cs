using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using DesktopFrontend.Models;
using ReactiveUI;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace DesktopFrontend.ViewModels
{
    public class CaptchaViewModel : ViewModelBase
    {
        private Bitmap _captchaImage;

        public Bitmap CaptchaImage
        {
            get => _captchaImage;
            private set => this.RaiseAndSetIfChanged(ref _captchaImage, value);
        }

        private string _capthaAttemptText = "";

        public string CaptchaAttemptText
        {
            get => _capthaAttemptText;
            set => this.RaiseAndSetIfChanged(ref _capthaAttemptText, value);
        }

        public ReactiveCommand<Unit, Unit> TryPassCaptha { get; }

        private Subject<(string login, string pass)> _captchaPassed;
        public IObservable<(string login, string pass)> CaptchaPassed => _captchaPassed;

        public CaptchaViewModel(IServerConnection connection)
        {
            _captchaPassed = new Subject<(string login, string pass)>();
            connection.RequestCaptcha()
                .ToObservable()
                .Subscribe(b => CaptchaImage = b);
            TryPassCaptha = ReactiveCommand.CreateFromTask(async () =>
            {
                var r = await connection.TryRequestAccount(CaptchaAttemptText);
                if (r != null)
                {
                    _captchaPassed.OnNext(r.Value);
                    _captchaPassed.OnCompleted();
                }
                else
                {
                    CaptchaImage = await connection.RequestCaptcha();
                }
            });
            // TODO: remove this to enable captcha requests
            DispatcherTimer.RunOnce(() =>
            {
                var credentials = connection.TryRequestAccount("").Result;
                if (credentials != null)
                    _captchaPassed.OnNext(credentials.Value);
                _captchaPassed.OnCompleted();
            }, TimeSpan.FromMilliseconds(0.1));
        }
    }
}