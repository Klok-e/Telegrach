using System;
using System.Collections.Generic;
using System.Text;
using DesktopFrontend.Models;
using System.Text.RegularExpressions;
using ReactiveUI;

namespace DesktopFrontend.ViewModels
{
    class AddServerViewModel : ViewModelBase
    {
        private INavigationStack _stack;
        private readonly Regex _nickName = new Regex(@"[^""{}[]]+||[^ ]+");
        private readonly Regex _ipAdress = new Regex(@"\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}");
        
        private string _nickVal = "";
        private string _ipVal = "";
        private string _portVal = "";

        private string _errMessage = "";

        public string Nick
        {
            get => _nickVal;
            set => this.RaiseAndSetIfChanged(ref _nickVal, value);
        }
        public string Ip
        {
            get => _ipVal;
            set => this.RaiseAndSetIfChanged(ref _ipVal, value);
        }
        public string Port
        {
            get => _portVal;
            set => this.RaiseAndSetIfChanged(ref _portVal, value);
        }
        public string Error
        {
            get => _errMessage;
            private set => this.RaiseAndSetIfChanged(ref _errMessage, value);
        }


        public AddServerViewModel(INavigationStack stack)
        {
            _stack = stack;
        }

        private bool CheckNick() 
        {
            return _nickName.IsMatch(_nickVal);
        }

        private bool CheckIp()
        {
            bool result = _ipAdress.IsMatch(_ipVal);
            string[] ipParts = _ipVal.Split('.');
            foreach (var item in ipParts)
            {
                int temp;
                if(int.TryParse(item, out temp))
                {
                    result &= (-1 < temp && temp < 256);
                }
                else
                {
                    return false;
                }
            }
            return result;
        }

        private bool CheckPort()
        {
            int port;
            if(int.TryParse(_portVal, out port))
            {
                return 0 < port && port < 65535;
            }
            else
            {
                return false;
            }
        }
        public void Add()
        {
            if(CheckIp() && CheckNick() && CheckPort())
            {
                var newSever = new ServerItem { Ip = _ipVal, Nick = _nickVal, Port = int.Parse(_portVal) };
                var list = IpConfig.ReadConfig();
                list.Add(newSever);
                IpConfig.WriteConfig(list);
                DeleteText();
                Error = "Succesful";
            }
            else
            {
                DeleteText();
                Error = "Invalid input. Try again";
            }
        }

        public void Back()
        {
            _stack.Pop();
        }

        private void DeleteText()
        {
            Ip = "";
            Nick = "";
            Port = "";
        }

    }
}
