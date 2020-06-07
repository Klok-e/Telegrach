using System;
using System.IO;
using Avalonia;
using System.Text.Json;
using System.Collections.Generic;

namespace DesktopFrontend.Models
{
    public class DataStorage
    {
        public static readonly string DataFolderPath = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "telegrach");

        public static readonly string CacheFolderPath = Path.Join(DataFolderPath, "cache");

        public static readonly string ConfigFilePath = Path.Join(DataFolderPath, "config");

        private static readonly string IpConfigPath = Path.Join(DataFolderPath, "ipConfig.json");

        private (string login, string password)? _credsCache;

        public DataStorage()
        {
            Log.Info(Log.Areas.Storage, this, $"Credentials path is {ConfigFilePath}");

            if (!Directory.Exists(DataFolderPath))
                Directory.CreateDirectory(DataFolderPath);
            if (!Directory.Exists(CacheFolderPath))
                Directory.CreateDirectory(CacheFolderPath);
            if (!File.Exists(IpConfigPath))
            {
                var json = JsonSerializer.Serialize<List<ServerItem>>(new List<ServerItem> { new ServerItem() });
                File.WriteAllTextAsync(IpConfigPath, json).Wait();

            }
        }

        public void StoreCredentials(string login, string password)
        {
            Contract.Requires<ArgumentNullException>(login != null && password != null);

            _credsCache = null;
            try
            {
                // store in a totally secure way
                File.WriteAllText(ConfigFilePath, login + " " + password);
                Log.Info(Log.Areas.Storage, this, $"Config file written successfully");
            }
            catch (Exception e)
            {
                Log.Error(Log.Areas.Storage, this, $"Couldn't store config: {e}");
            }
        }

        public (string login, string password)? RetrieveCredentials()
        {
            if (_credsCache != null)
                return _credsCache;
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    // retrieve in a totally secure way
                    var secret = File.ReadAllText(ConfigFilePath).Split(' ');
                    Log.Info(Log.Areas.Storage, this, $"Config file retrieved successfully");
                    return (secret[0], secret[1]);
                }

                Log.Info(Log.Areas.Storage, this, $"Config file not found");
                return null;
            }
            catch (Exception e)
            {
                Log.Error(Log.Areas.Storage, this, $"Couldn't retrieve config: {e}");
            }

            return null;
        }

        public void ResetCredentials()
        {
            _credsCache = null;
            try
            {
                if (File.Exists(ConfigFilePath))
                    File.Delete(ConfigFilePath);
            }
            catch (Exception e)
            {
                Log.Error(Log.Areas.Storage, this, $"Couldn't save file: {e}");
            }
        }

        public string? SaveFile(string name, byte[] bytes)
        {
            try
            {
                var filePath = Path.Join(CacheFolderPath, name);

                File.WriteAllBytes(filePath, bytes);
                return filePath;
            }
            catch (Exception e)
            {
                Log.Error(Log.Areas.Storage, this, $"Couldn't save file: {e}");
            }

            return null;
        }

        public List<ServerItem> ReadIpConfig()
        {
            var json = File.ReadAllText(IpConfigPath);
            var serverList = JsonSerializer.Deserialize<List<ServerItem>>(json);
            return serverList;
        }

        public void WriteIpConfig(List<ServerItem> servers)
        {
            var json = JsonSerializer.Serialize(servers);
            File.WriteAllTextAsync(IpConfigPath, json).Wait();
        }
    }
}