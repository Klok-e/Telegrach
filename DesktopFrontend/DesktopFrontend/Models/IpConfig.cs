using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;

namespace DesktopFrontend.Models
{
    public class ServerItem {
        public string Nick { get; set; } = "Default Server";
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 9999;

        public override string ToString()
        {
            return Nick + "\n" + Ip + $":{Port}";
        }

        public string String => ToString();

        public string Color { get; set; } = "White"; //kostil
    }

    public static class IpConfig
    {
        private static string _path;
        
        static IpConfig()
        {
            _path = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName + @"\Assets\serverConfig.json";
            if (!File.Exists(_path))
            {
                var json = JsonSerializer.Serialize<List<ServerItem>>(new List<ServerItem> { new ServerItem() });
                File.WriteAllTextAsync(_path, json).Wait();
                
            }
        }

        public static List<ServerItem> ReadConfig()
        {
            var json = File.ReadAllText(_path);
            var serverList = JsonSerializer.Deserialize<List<ServerItem>>(json);
            return serverList;            
        }

        public static void WriteConfig(List<ServerItem> servers)
        {
            var json = JsonSerializer.Serialize(servers);
            File.WriteAllTextAsync(_path, json).Wait();
        }



        
    }
}
