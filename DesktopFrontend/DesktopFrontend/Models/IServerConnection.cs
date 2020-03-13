using System.Threading.Tasks;
using System.Collections.Generic;

namespace DesktopFrontend.Models
{
    public interface IServerConnection
    {
        bool IsConnected { get; }

        Task<bool> Connect();

        Task<bool> LogInWithCredentials(string user, string pass);

        Task<bool> RequestANewAccount();

        Task<List<string>> RequestThreadSet(string name);
    }
}