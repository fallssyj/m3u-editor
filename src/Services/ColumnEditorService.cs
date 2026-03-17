using m3u_editor.Models;
using m3u_editor.ViewModels.Dialogs;
using m3u_editor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace m3u_editor.Services
{
    /// <summary>
    /// 通过弹出 ColumnEditorWindow 来管理列编辑体验。
    /// </summary>
    public sealed class ColumnEditorService : IColumnEditorService
    {
        /// <summary>
        /// 打开列编辑窗口并返回编辑结果。
        /// </summary>
        public IReadOnlyList<ColumnSchemaEntry>? EditColumns(
            IEnumerable<ColumnSchemaEntry> columns,
            Action<IReadOnlyList<ColumnSchemaEntry>>? onChanged = null)
        {
            var columnList = columns?.ToList() ?? throw new ArgumentNullException(nameof(columns));
            var viewModel = new ColumnEditorViewModel(columnList);
            if (onChanged is not null)
            {
                viewModel.ColumnsChanged += (_, updated) => onChanged(updated);
            }
            var window = new ColumnEditorWindow(viewModel)
            {
                Owner = Application.Current?.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.IsActive)
            };

            window.ShowDialog();
            return viewModel.ResultColumns;
        }
    }
}
