using System;
using System.IO;
using System.Net.Mime;
using Avalonia;

namespace DesktopFrontend.Models
{
    public class CredentialsStorage
    {
        public static readonly string ConfigFolderPath = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "telegrach");

        public string StorageFilePath { get; }

        public CredentialsStorage()
        {
            StorageFilePath = Path.Join(ConfigFolderPath, "config");
            Log.Info(Log.Areas.Storage, this, $"Credentials path is {StorageFilePath}");
        }

        public void Store(string login, string password)
        {
            Contract.Requires<ArgumentNullException>(login != null && password != null);
            try
            {
                // store in a totally secure way
                Directory.CreateDirectory(ConfigFolderPath);
                File.WriteAllText(StorageFilePath, login + " " + password);
                Log.Info(Log.Areas.Storage, this, $"Config file written successfully");
            }
            catch (Exception e)
            {
                Log.Error(Log.Areas.Storage, this, $"Couldn't store config: {e}");
            }
        }

        public (string login, string password)? Retrieve()
        {
            try
            {
                if (File.Exists(StorageFilePath))
                {
                    // retrieve in a totally secure way
                    var secret = File.ReadAllText(StorageFilePath).Split(' ');
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

        public void ResetConfig()
        {
            if (File.Exists(StorageFilePath))
                File.Delete(StorageFilePath);
        }
    }
}