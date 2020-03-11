using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace DesktopFrontend.Models
{
    public class MockServerConnection : IServerConnection
    {
        public bool IsConnected { get; private set; }

        public async Task<bool> Connect()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return IsConnected = true;
        }

        public async Task<bool> LogInWithCredentials(string user, string pass)
        {
            throw new NotImplementedException();
        }

        public async Task<Bitmap> RequestCaptcha()
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            Bitmap b;
            await using (var s = assets.Open(new Uri("/Assets/mock-captcha.jpg")))
                b = new Bitmap(s);

            return b;
        }

        public async Task<(string login, string pass)?> TryRequestAccount(string tryText)
        {
            if (tryText == "hey /b/")
                return ("rwerwer", "564756868");
            return null;
        }
    }
}