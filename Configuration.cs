using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace Capybara
{
    public class Configuration
    {
        public static string TfsUrl
        {
            get
            {
                return GetSettings()["TfsUrl"].Value;
            }
        }

        public static string TfsUsername
        {
            get
            {
                return GetSettings()["TfsUsername"].Value;
            }
        }

        public static string TfsPassword
        {
            get
            {
                return GetSettings()["TfsPassword"].Value;
            }
        }

        public static string EwsUrl
        {
            get
            {
                return GetSettings()["EwsUrl"].Value;
            }
        }

        public static string EmailUsername
        {
            get
            {
                return GetSettings()["EmailUsername"].Value;
            }
        }

        public static string EmailPassword
        {
            get
            {
                return GetSettings()["EmailPassword"].Value;
            }
        }

        public static ICollection<string> Projects
        {
            get
            {
                var setting = GetSettings()["Projects"].Value;
                return setting.Split(',').Select(each => each.Trim()).ToArray();
            }
        }

        public static TimeSpan PollingInterval
        {
            get
            {
                var setting = GetSettings()["PollingInterval"].Value;
                return TimeSpan.FromSeconds(Convert.ToInt32(setting));
            }
        }

        public static DateTime LastCheckDateTime
        {
            get
            {
                var setting = GetSettings()["LastCheckDateTime"].Value;

                var ticks = Convert.ToInt64(setting);
                return ticks == 0 ? DateTime.Now : new DateTime(ticks);
            }
            set
            {
                var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configuration.AppSettings.Settings;
                settings["LastCheckDateTime"].Value = value.Ticks.ToString(CultureInfo.InvariantCulture);
                configuration.Save();
            }
        }

        private static KeyValueConfigurationCollection GetSettings()
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return configuration.AppSettings.Settings;
        }
    }
}