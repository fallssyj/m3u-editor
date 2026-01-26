using System.Windows.Input;

namespace m3u_editor
{
    /// <summary>
    /// 主窗口代码隐藏，仅保留必要的窗口级交互。
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                // 允许在窗口空白区域拖动窗口。
                DragMove();
            }
        }
    }
}