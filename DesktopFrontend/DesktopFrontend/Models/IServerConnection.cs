using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace DesktopFrontend.Models
{
    public interface IServerConnection
    {
        bool IsConnected { get; }

        Task<bool> Connect();

        Task<bool> LogInWithCredentials(string user, string pass);

        Task<Bitmap> RequestCaptcha();

        Task<(string login, string pass)?> TryRequestAccount(string tryText);
    }
}