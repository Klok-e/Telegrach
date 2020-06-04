using System;
using System.IO;
using Avalonia;

namespace DesktopFrontend.Models
{
    public class DataStorage
    {
        public static readonly string DataFolderPath = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "telegrach");

        public static readonly string CacheFolderPath = Path.Join(DataFolderPath, "cache");

        public static readonly string ConfigFilePath = Path.Join(DataFolderPath, "config");

        private (string login, string password)? _credsCache;

        public DataStorage()
        {
            Log.Info(Log.Areas.Storage, this, $"Credentials path is {ConfigFilePath}");
        }

        public void StoreCredentials(string login, string password)
        {
            Contract.Requires<ArgumentNullException>(login != null && password != null);

            _credsCache = null;
            try
            {
                // store in a totally secure way
                Directory.CreateDirectory(DataFolderPath);
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

        public string? SaveFile(string name, byte[] stream)
        {
            try
            {
                var filePath = Path.Join(CacheFolderPath, name);

                // create dir if it doesn't exist
                Directory.CreateDirectory(filePath);

                File.WriteAllBytes(filePath, stream);
                return filePath;
            }
            catch (Exception e)
            {
                Log.Error(Log.Areas.Storage, this, $"Couldn't save file: {e}");
            }

            return null;
        }
    }
}