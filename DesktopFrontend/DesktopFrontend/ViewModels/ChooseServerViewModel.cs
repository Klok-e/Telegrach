using System;
using System.Collections.Generic;
using System.Text;
using DesktopFrontend.Models;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using ReactiveUI;
using System.Runtime.InteropServices;

namespace DesktopFrontend.ViewModels
{
    public class ChooseServerViewModel : ViewModelBase
    {
        private INavigationStack _stack;
        private DataStorage _storage;
        private IServerConnection _connection;
        private List<ServerItem> _items;
        private ServerItem _selectedServer =  new ServerItem();
        

        public ReactiveCommand<ServerItem, Unit> SelectServer { get; private set; }
        public ChooseServerViewModel(INavigationStack stack, ref IServerConnection connection, DataStorage storage)
        {
            _stack = stack;
            _storage = storage;
            _connection = connection;
            SelectServer = ReactiveCommand.Create<ServerItem>(ServerItem =>
            {                
                Log.Info(Log.Areas.Application, this, $"Selected server : {ServerItem}");
                _selectedServer =  ServerItem ;
                
            });

        }

        public List<ServerItem> Items
        {
            get
            {
                _items = IpConfig.ReadConfig();
                return _items;
            }
        }     

        public string SelectedText => "The selected server:";
        

        public void Connect() 
        {
            var connection = new ServerConnection(_selectedServer.Ip, _selectedServer.Port, _storage);
            connection.Connect()
                .ToObservable()
                .SelectMany(async connected =>
                {
                    if (connected)
                    {
                        await Login(connection, _storage);
                    }
                    else
                    {
                        Log.Warn(Log.Areas.Network, this,
                            "Could not connect to the server");
                        var retry = new RetryConnectViewModel();
                        _stack.Push(retry);
                        retry.RetryAttempt.SelectMany(async _ => await connection.Connect())
                            .Where(didConnect => didConnect)
                            .Take(1)
                            // async subscribe bad, idk why
                            // https://stackoverflow.com/questions/24843000/reactive-extensions-subscribe-calling-await
                            .SelectMany(async _ =>
                            {
                                _stack.Pop();
                                await Login(connection, _storage);
                                return default(Unit);
                            })
                            .Subscribe();
                    }

                    return default(Unit);
                })
                .Subscribe();
        }

        private async Task Login(IServerConnection connection, DataStorage storage)
        {
            var cred = storage.RetrieveCredentials();
            if (cred != null &&
                await connection.LogInWithCredentials(cred.Value.login, cred.Value.password))
            {
                _stack.Push(new ChatViewModel(_stack, connection));
                Log.Info(Log.Areas.Network, this,
                    $"Logged in successfully as {cred.Value.login}");
            }
            else
            {
                storage.ResetCredentials();
                _stack.Push(new LoginViewModel(_stack, connection));
            }
        }

        public void AddServer()
        {
            _stack.Push(new AddServerViewModel(_stack));
        }

    }
}
