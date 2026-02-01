using Microsoft.Win32;

namespace m3u_editor.Services
{
    /// <summary>
    /// 默认的文件对话框实现，基于系统 OpenFileDialog。
    /// </summary>
    public sealed class FileDialogService : IFileDialogService
    {
        /// <summary>
        /// 选择 m3u 文件。
        /// </summary>
        public string? PickM3uFile(string? initialDirectory = null)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "播放列表 (*.m3u;*.m3u8;*.txt)|*.m3u;*.m3u8;*.txt|M3U Playlist (*.m3u;*.m3u8)|*.m3u;*.m3u8|TXT (*.txt)|*.txt|All Files (*.*)|*.*",
                Multiselect = false,
                InitialDirectory = initialDirectory,
                Title = "选择播放列表"
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        /// <summary>
        /// 保存 m3u 文件。
        /// </summary>
        public string? SaveM3uFile(string? initialDirectory = null, string? defaultFileName = null)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "M3U Playlist (*.m3u)|*.m3u|UTF-8 M3U (*.m3u8)|*.m3u8",
                InitialDirectory = initialDirectory,
                FileName = defaultFileName,
                AddExtension = true,
                DefaultExt = ".m3u8",
                Title = "保存 IPTV 播放列表"
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        /// <summary>
        /// 选择外部播放器程序。
        /// </summary>
        public string? PickPlayerExecutable()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "播放器 (*.exe)|*.exe|All Files (*.*)|*.*",
                Multiselect = false,
                Title = "选择外部播放器"
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        /// <summary>
        /// 保存 JSON 文件。
        /// </summary>
        public string? SaveJsonFile(string? initialDirectory = null, string? defaultFileName = null)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON 文件 (*.json)|*.json|All Files (*.*)|*.*",
                InitialDirectory = initialDirectory,
                FileName = defaultFileName,
                AddExtension = true,
                DefaultExt = ".json",
                Title = "导出 JSON"
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
