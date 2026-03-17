namespace m3u_editor.Models
{
    /// <summary>
    /// 应用配置模型，用于持久化主题与外部播放器路径。
    /// </summary>
    public sealed class AppSettings
    {
        /// <summary>
        /// 主题模式：Light 或 Dark。
        /// </summary>
        public ThemeMode ThemeMode { get; set; } = ThemeMode.Dark;

        /// <summary>
        /// 外部播放器可执行文件路径。
        /// </summary>
        public string? PlayerPath { get; set; }
    }

    public enum ThemeMode
    {
        Light,
        Dark
    }
}
