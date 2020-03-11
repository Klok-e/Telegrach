using System.Drawing;
using System.Threading.Tasks;

namespace DesktopFrontend.Models
{
    public interface IServerConnection
    {
        bool IsConnected { get; }

        Task<bool> Connect();

        Task<bool> LogInWithCredentials(string user, string pass);

        Task<Image> RequestCaptcha();

        Task<bool> TryPassCaptcha(string tryText);

        Task<bool> RequestANewAccount();
    }
}