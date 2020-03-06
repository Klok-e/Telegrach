using System.Threading.Tasks;

namespace DesktopFrontend.Models
{
    public interface IServerConnection
    {
        bool IsConnected { get; }

        Task<bool> Connect();

        Task<bool> LogInWithCredentials(string user, string pass);
        
        Task<bool> RequestANewAccount();
    }
}