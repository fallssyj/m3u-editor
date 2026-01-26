using HandyControl.Controls;
using m3u_editor.ViewModels.Dialogs;

namespace m3u_editor.Views
{
    /// <summary>
    /// 列编辑对话框窗口。
    /// </summary>
    public partial class ColumnEditorWindow : Window
    {
        /// <summary>
        /// 绑定 ViewModel 并监听关闭请求。
        /// </summary>
        public ColumnEditorWindow(ColumnEditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.RequestClose += (_, result) =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}
