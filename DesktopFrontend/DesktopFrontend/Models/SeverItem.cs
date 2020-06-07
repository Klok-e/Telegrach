using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;

namespace DesktopFrontend.Models
{
    public class ServerItem
    {
        public string Nick { get; set; } = "Default Server";
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 9999;

        public override string ToString()
        {
            return Nick + "\n" + Ip + $":{Port}";
        }

        public string String => ToString();

    }

}
