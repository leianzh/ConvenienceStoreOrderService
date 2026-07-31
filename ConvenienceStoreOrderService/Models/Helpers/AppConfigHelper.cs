using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ConvenienceStoreOrderService.Models.Helpers
{
    public class AppConfigHelper
    {
        /// <summary>
        /// 優先讀環境變數，讀不到才讀 Web.config 的 appSettings。
        /// </summary>
        public static string GetRequiredSetting(string environmentVariableName, string appSettingsKey)
        {
            var value = GetEnvironmentValue(environmentVariableName);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            value = ConfigurationManager.AppSettings[appSettingsKey];

            if (!string.IsNullOrWhiteSpace(value) &&
                !value.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            throw new InvalidOperationException(
                $"缺少必要設定：請設定 Windows 環境變數 {environmentVariableName}，或 Web.config appSettings[{appSettingsKey}]。"
            );
        }

        /// <summary>
        /// 讀取資料庫連線字串。      
        /// </summary>
        public static string GetDbConnectionString()
        {
            var value = GetEnvironmentValue("COS_DB_CONNECTION_STRING");

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["AppDbContext"]?.ConnectionString;

            if (!string.IsNullOrWhiteSpace(connectionString) &&
                !connectionString.Contains("YOUR_PASSWORD"))
            {
                return connectionString;
            }

            throw new InvalidOperationException(
                "缺少資料庫連線字串：請設定 Windows 環境變數 COS_DB_CONNECTION_STRING，或 Web.config connectionStrings[AppDbContext]。"
            );
        }

        /// <summary>
        /// 依序讀取 Process、User、Machine 層級的環境變數。
        /// </summary>
        private static string GetEnvironmentValue(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process)
                ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)
                ?? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
        }
    }
}
