using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace DesktopFrontend.Models
{
    public interface IServerConnection
    {
        bool IsConnected { get; }

        /// <summary>
        /// Returns new threads
        /// </summary>
        //IObservable<ThreadItem[]> NewThreadArrived { get; }
        
        /// <summary>
        /// Returns new messages
        /// </summary>
        //IObservable<ChatMessage[]> NewMessageArrived { get; }

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

        Task SendMessage(string body, ulong threadId);

        Task<ChatMessages> RequestMessagesForThread(ThreadItem thread);
    }
}