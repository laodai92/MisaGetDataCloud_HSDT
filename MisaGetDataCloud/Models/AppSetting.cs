using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MisaGetDataCloud.Models
{
    public class AppSetting
    {
        /// <summary>
        /// Đọc appsetting
        /// </summary>
        /// <param name="appSettingKey"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetAppSettingWithDefaultValue(string appSettingKey, string defaultValue = "")
        {
            string strConfigValue = ConfigurationManager.AppSettings.Get(appSettingKey);
            //Nếu appsetting ko có và có giá trị ngầm định
            if (string.IsNullOrWhiteSpace(strConfigValue) && defaultValue != null)
            {
                strConfigValue = defaultValue;
            }
            return strConfigValue;
        }

        /// <summary>
        /// Lưu appsetting
        /// </summary>
        /// <param name="appSettingKey"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static void SaveAppSetting(string appSettingKey, string value = "")
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            AppSettingsSection appSettingSection = config.AppSettings;
            if (appSettingSection.Settings.AllKeys.Contains(appSettingKey))
            {
                appSettingSection.Settings.Remove(appSettingKey);
            }

            appSettingSection.Settings.Add(appSettingKey, value);
            ConfigurationManager.AppSettings[appSettingKey] = value;

            config.Save();
        }
    }
}
