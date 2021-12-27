using System;
using System.Configuration;

namespace LocalDatabase_Client.Client
{

    public sealed class SettingsManager
    {

        private static readonly Lazy<SettingsManager> lazy = new Lazy<SettingsManager>(() => new SettingsManager());

        public static SettingsManager Instance { get { return lazy.Value; } }

        /// <summary>
        /// Sets server Ip. Default is 127.0.0.1
        /// </summary>
        /// <param name="serverIp"></param>
        public void SetServerIp(string serverIp)
        {
            SetSetting(ServerIp, serverIp);
        }

        public string GetServerIp()
        {
            return GetSetting(ServerIp);
        }

        private const string ServerIp = "ServerIp";

        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        private static void SetSetting(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Full, true);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}