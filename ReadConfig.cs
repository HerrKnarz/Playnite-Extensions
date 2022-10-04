using System;
using System.Configuration;

namespace LinkManager
{
    internal class ReadConfig
    {
        public string ItchApiKey { get; set; }

        public ReadConfig()
        {
            Configuration config = null;
            string exeConfigPath = this.GetType().Assembly.Location;
            try
            {
                config = ConfigurationManager.OpenExeConfiguration(exeConfigPath);
            }
            catch (Exception)
            {
            }

            if (config != null)
            {
                ItchApiKey = GetAppSetting(config, "ItchApiKey");
            }
        }
        public string GetAppSetting(Configuration config, string key)
        {
            KeyValueConfigurationElement element = config.AppSettings.Settings[key];
            if (element != null)
            {
                string value = element.Value;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return string.Empty;
        }
    }
}
