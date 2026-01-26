namespace m3u_editor.Services
{
    /// <summary>
    /// 抽象文件对话框行为，避免 ViewModel 直接依赖 UI 组件。
    /// </summary>
    public interface IFileDialogService
    {
        /// <summary>
        /// 打开文件选择对话框，返回用户选中的 m3u 路径。
        /// </summary>
        /// <param name="initialDirectory">首选打开目录。</param>
        /// <returns>若用户取消，则返回 null。</returns>
        string? PickM3uFile(string? initialDirectory = null);

        /// <summary>
        /// 打开保存对话框，返回用户指定的 m3u 输出路径。
        /// </summary>
        /// <param name="initialDirectory">建议的初始目录。</param>
        /// <param name="defaultFileName">默认文件名，通常沿用当前播放列表。</param>
        /// <returns>若用户取消，则返回 null。</returns>
        string? SaveM3uFile(string? initialDirectory = null, string? defaultFileName = null);

        /// <summary>
        /// 打开播放器选择对话框，返回用户选中的可执行文件路径。
        /// </summary>
        /// <returns>若用户取消，则返回 null。</returns>
        string? PickPlayerExecutable();

        /// <summary>
        /// 打开保存 JSON 对话框，返回用户指定的输出路径。
        /// </summary>
        /// <param name="initialDirectory">建议的初始目录。</param>
        /// <param name="defaultFileName">默认文件名。</param>
        /// <returns>若用户取消，则返回 null。</returns>
        string? SaveJsonFile(string? initialDirectory = null, string? defaultFileName = null);
    }
}
