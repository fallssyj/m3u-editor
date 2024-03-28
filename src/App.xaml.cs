using m3u_editor.Views;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Prism.Ioc;
using System;
using System.Windows;

namespace m3u_editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key != null)
            {
                int theme = (int)key.GetValue("AppsUseLightTheme", -1);
                if (theme == 0) //暗黑模式
                {
                    ChangeDarkTheme(true);
                }
                else if (theme == 1) //亮色模式
                {
                    ChangeDarkTheme(false);
                }
                else //调用失败后兜底
                {
                    ChangeDarkTheme(false);
                }
                key.Close();
            }
            base.OnStartup(e);
        }
        private void ChangeDarkTheme(bool isDark)
        {
            var path = isDark ? new Uri("Themes/DarkTheme.xaml", UriKind.RelativeOrAbsolute) : new Uri("Themes/LightTheme.xaml", UriKind.RelativeOrAbsolute);
            foreach (var res in Resources.MergedDictionaries)
            {
                if (res.Source != null && res.Source.ToString() == "Themes/LightTheme.xaml")
                {
                    res.Source = path;
                }
            }

            ModifyTheme(isDark);
        }
        private void ModifyTheme(bool isDarkTheme)
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(isDarkTheme ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);

        }
    }
}
