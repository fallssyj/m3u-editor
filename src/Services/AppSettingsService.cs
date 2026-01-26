using m3u_editor.Models;
using System;
using System.IO;
using System.Text.Json;

namespace m3u_editor.Services
{
    /// <summary>
    /// 使用 JSON 文件持久化应用配置。
    /// </summary>
    public sealed class AppSettingsService : IAppSettingsService
    {
        private const string SettingsFileName = "settings.json";

        /// <summary>
        /// 读取配置文件，失败时返回默认配置。
        /// </summary>
        public AppSettings Load()
        {
            var path = GetSettingsPath();
            if (!File.Exists(path))
            {
                return new AppSettings();
            }

            try
            {
                var json = File.ReadAllText(path);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        /// <summary>
        /// 将配置序列化到本地文件。
        /// </summary>
        public void Save(AppSettings settings)
        {
            if (settings is null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var path = GetSettingsPath();
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }

        /// <summary>
        /// 获取配置文件路径（位于程序目录）。
        /// </summary>
        private static string GetSettingsPath()
        {
            // 将配置文件放在程序目录下，便于随程序一起移动。
            var baseDirectory = AppContext.BaseDirectory;
            return Path.Combine(baseDirectory, SettingsFileName);
        }
    }
}
