using System.Threading.Tasks;

namespace DesktopFrontend.Models
{
    public interface IServerConnection
    {
        bool IsConnected { get; }

        Task<bool> Connect();
    }
}