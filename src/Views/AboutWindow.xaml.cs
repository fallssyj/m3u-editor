using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;

namespace m3u_editor.Views
{
    /// <summary>
    /// 关于窗口
    /// </summary>
    public partial class AboutWindow
    {
        /// <summary>
        /// 应用名称（用于展示）。
        /// </summary>
        public string AppTitle { get; set; } = "m3u-editor";

        /// <summary>
        /// 关于窗口标题（绑定到界面）。
        /// </summary>
        public string _Title { get; set; } = "关于 ";


        /// <summary>
        /// 依赖库展示列表。
        /// </summary>
        public ObservableCollection<string> Libraries { get; } =
        [
            "MiSans",
            "HandyControl"
        ];

        /// <summary>
        /// 许可证文本内容。
        /// </summary>
        public string LicenseText { get; set; } = "";

        public AboutWindow()
        {
            InitializeComponent();
            DataContext = this;
            // 启动时拼接版本号与许可证文本。
            GetVersion();
            LicenseText = LoadLicenseText();
        }

        /// <summary>
        /// 读取程序集版本并拼接标题。
        /// </summary>
        private void GetVersion()
        {
            var (_, currentVersionText) = GetCurrentVersionInfo();
            if (currentVersionText != "未知")
            {
                _Title += $"{AppTitle} v{currentVersionText}";
            }
        }

        /// <summary>
        /// 获取当前程序集版本及其显示文本。
        /// </summary>
        private static (Version? Version, string Text) GetCurrentVersionInfo()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var text = version is null
                ? "未知"
                : $"{version.Major}.{version.Minor}.{version.Build}";
            return (version, text);
        }

        /// <summary>
        /// 从资源中读取许可证文本。
        /// </summary>
        private static string LoadLicenseText()
        {
            try
            {
                var resourceUri = new Uri("Assets/LICENSE/LICENSE", UriKind.Relative);
                var streamInfo = System.Windows.Application.GetResourceStream(resourceUri);
                if (streamInfo?.Stream == null)
                {
                    return "MIT License";
                }

                using var reader = new StreamReader(streamInfo.Stream, Encoding.UTF8, true);
                return reader.ReadToEnd();
            }
            catch
            {
                return "MIT License";
            }
        }

    }

}
