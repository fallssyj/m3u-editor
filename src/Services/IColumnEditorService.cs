using m3u_editor.Models;
using System.Collections.Generic;

namespace m3u_editor.Services
{
    /// <summary>
    /// 抽象列编辑器弹窗的行为，便于在 MVVM 中解耦视图与逻辑。
    /// </summary>
    public interface IColumnEditorService
    {
        /// <summary>
        /// 打开列编辑器，返回经过排序、重命名后的列定义列表。
        /// </summary>
        /// <param name="columns">现有列的描述集合，包含保留列信息。</param>
        /// <returns>若用户点击确定则返回新列表，否则返回 null。</returns>
        IReadOnlyList<ColumnSchemaEntry>? EditColumns(
            IEnumerable<ColumnSchemaEntry> columns,
            Action<IReadOnlyList<ColumnSchemaEntry>>? onChanged = null);
    }
}
