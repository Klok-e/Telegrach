using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace DesktopFrontend.Models
{
    public interface IServerConnection
    {
        bool IsConnected { get; }

        Task<bool> Connect();

        Task<bool> LogInWithCredentials(string user, string pass);

        Task<Bitmap> RequestCaptcha();

        Task<(string login, string pass)?> TryRequestAccount(string tryText);

        Task<ThreadSet> RequestThreadSet();

        /// <summary>
        /// Creates a thread on the server. Throws an exception if unsuccessful.
        /// </summary>
        /// <param name="head"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        Task CreateThread(string head, string body);
    }
}