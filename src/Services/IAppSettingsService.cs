using m3u_editor.Models;

namespace m3u_editor.Services
{
    /// <summary>
    /// 负责加载与保存应用配置。
    /// </summary>
    public interface IAppSettingsService
    {
        /// <summary>
        /// 读取配置文件，不存在则返回默认配置。
        /// </summary>
        AppSettings Load();

        /// <summary>
        /// 保存配置到本地文件。
        /// </summary>
        void Save(AppSettings settings);
    }
}
