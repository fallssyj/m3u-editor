namespace m3u_editor.Models
{
    /// <summary>
    /// 表示列编辑器中的一项，用于描述列名、是否保留以及原始名称。
    /// </summary>
    public sealed class ColumnSchemaEntry
    {
        public ColumnSchemaEntry(string name, bool isReserved, string? originalName = null)
        {
            Name = name;
            IsReserved = isReserved;
            OriginalName = originalName;
        }

        /// <summary>
        /// 显示给用户的列名，也是最终写入 DataTable 的列名。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 是否为内置保留列（如 ChannelName 等），用于限制删除/重命名操作。
        /// </summary>
        public bool IsReserved { get; }

        /// <summary>
        /// 原始列名，便于在保存时定位原始 DataColumn。
        /// 新增列则为 null。
        /// </summary>
        public string? OriginalName { get; }
    }
}
