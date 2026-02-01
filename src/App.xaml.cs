using m3u_editor.ViewModels;
using System.Linq;
using System.Windows;

namespace m3u_editor
{
    /// <summary>
    /// 应用程序入口，负责启动 WPF 生命周期。
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var window = Current?.MainWindow as MainWindow;
            if (window is null)
            {
                return;
            }

            var target = e.Args.FirstOrDefault(MainViewModel.IsSupportedPlaylistFile);
            if (!string.IsNullOrWhiteSpace(target) && window.DataContext is MainViewModel viewModel)
            {
                await viewModel.LoadPlaylistFromPathAsync(target);
            }
        }
    }

}
