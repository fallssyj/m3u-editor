using m3u_editor.ViewModels;
using System.Windows;
using System.Windows.Controls;
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

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                e.Data.GetData(DataFormats.FileDrop) is string[] files &&
                files.Any(MainViewModel.IsSupportedPlaylistFile))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                e.Data.GetData(DataFormats.FileDrop) is string[] files)
            {
                var target = files.FirstOrDefault(MainViewModel.IsSupportedPlaylistFile);
                if (string.IsNullOrWhiteSpace(target))
                {
                    return;
                }

                if (DataContext is MainViewModel viewModel)
                {
                    await viewModel.LoadPlaylistFromPathAsync(target);
                }
            }
        }

        private void ChannelGrid_AutoGeneratingColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            //if (e.Column is not null)
            //{
            //    
            //}
            if (string.Equals(e.PropertyName, "tvg-logo", StringComparison.OrdinalIgnoreCase))
            {
                e.Column.Width = new DataGridLength(200);
            }
            if (string.Equals(e.PropertyName, "StreamUrlCandidates", StringComparison.OrdinalIgnoreCase))
            {
                e.Cancel = true;
                return;
            }

            if (string.Equals(e.PropertyName, "StreamUrl", StringComparison.OrdinalIgnoreCase))
            {
                if (FindResource("StreamUrlTextTemplate") is DataTemplate textTemplate &&
                    FindResource("StreamUrlComboTemplate") is DataTemplate comboTemplate)
                {
                    e.Column = new DataGridTemplateColumn
                    {
                        Header = e.Column?.Header ?? "StreamUrl",
                        CellTemplate = textTemplate,
                        CellEditingTemplate = comboTemplate,
                        SortMemberPath = "StreamUrl"
                    };
                }
            }

            if (string.Equals(e.PropertyName, "group-title", StringComparison.OrdinalIgnoreCase))
            {
                if (FindResource("GroupTitleTextTemplate") is DataTemplate textTemplate &&
                    FindResource("GroupTitleComboTemplate") is DataTemplate comboTemplate)
                {
                    e.Column = new DataGridTemplateColumn
                    {
                        Header = e.Column?.Header ?? "group-title",
                        CellTemplate = textTemplate,
                        CellEditingTemplate = comboTemplate,
                        SortMemberPath = "group-title"
                    };
                }
            }
        }
    }
}