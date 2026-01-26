using m3u_editor.Commands;
using m3u_editor.Models;
using m3u_editor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace m3u_editor.ViewModels.Dialogs
{
    /// <summary>
    /// 列编辑器对话框的 ViewModel，负责管理增删改排序等逻辑。
    /// </summary>
    public sealed class ColumnEditorViewModel : ViewModelBase
    {
        private readonly ObservableCollection<ColumnEditorItem> _columns;
        private readonly RelayCommand _removeCommand;
        private readonly RelayCommand _moveUpCommand;
        private readonly RelayCommand _moveDownCommand;
        private ColumnEditorItem? _selectedColumn;
        private string? _errorMessage;

        /// <summary>
        /// 使用已有列构建编辑器。
        /// </summary>
        public ColumnEditorViewModel(IEnumerable<ColumnSchemaEntry> columns)
        {
            if (columns is null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            _columns = new ObservableCollection<ColumnEditorItem>(
                columns.Select(c => new ColumnEditorItem(c.Name, c.IsReserved, c.OriginalName)));

            AddCommand = new RelayCommand(_ => AddColumn());
            _removeCommand = new RelayCommand(_ => RemoveColumn(), _ => SelectedColumn?.CanRemove == true);
            _moveUpCommand = new RelayCommand(_ => MoveColumn(-1), _ => CanMove(-1));
            _moveDownCommand = new RelayCommand(_ => MoveColumn(1), _ => CanMove(1));
            ConfirmCommand = new RelayCommand(_ => ConfirmChanges());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(this, false));

            SelectedColumn = _columns.FirstOrDefault();
        }

        /// <summary>
        /// 请求关闭窗口事件，参数表示是否点击确定。
        /// </summary>
        public event EventHandler<bool>? RequestClose;

        /// <summary>
        /// 当前列集合，支持排序与编辑。
        /// </summary>
        public ObservableCollection<ColumnEditorItem> Columns => _columns;

        /// <summary>
        /// 当前选中的列项。
        /// </summary>
        public ColumnEditorItem? SelectedColumn
        {
            get => _selectedColumn;
            set
            {
                if (SetProperty(ref _selectedColumn, value))
                {
                    _removeCommand.RaiseCanExecuteChanged();
                    _moveUpCommand.RaiseCanExecuteChanged();
                    _moveDownCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 新增列命令。
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// 删除列命令。
        /// </summary>
        public ICommand RemoveCommand => _removeCommand;

        /// <summary>
        /// 上移命令。
        /// </summary>
        public ICommand MoveUpCommand => _moveUpCommand;

        /// <summary>
        /// 下移命令。
        /// </summary>
        public ICommand MoveDownCommand => _moveDownCommand;

        /// <summary>
        /// 确认命令。
        /// </summary>
        public ICommand ConfirmCommand { get; }

        /// <summary>
        /// 取消命令。
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// 用户确认后的列结果。
        /// </summary>
        public IReadOnlyList<ColumnSchemaEntry> ResultColumns { get; private set; } = Array.Empty<ColumnSchemaEntry>();

        /// <summary>
        /// 校验错误提示。
        /// </summary>
        public string? ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// 在当前选中项后插入新列。
        /// </summary>
        private void AddColumn()
        {
            var baseName = "新列";
            var suffix = 1;
            var candidate = baseName;
            var existing = new HashSet<string>(_columns.Select(c => c.Name), StringComparer.OrdinalIgnoreCase);
            while (existing.Contains(candidate))
            {
                candidate = $"{baseName}{suffix++}";
            }

            var newItem = new ColumnEditorItem(candidate, isReserved: false, originalName: null);
            var insertIndex = SelectedColumn is null ? _columns.Count : _columns.IndexOf(SelectedColumn) + 1;
            if (insertIndex < 0 || insertIndex > _columns.Count)
            {
                insertIndex = _columns.Count;
            }

            _columns.Insert(insertIndex, newItem);
            SelectedColumn = newItem;
            ErrorMessage = null;
        }

        /// <summary>
        /// 删除当前选中的列（保留列不允许删除）。
        /// </summary>
        private void RemoveColumn()
        {
            if (SelectedColumn?.CanRemove != true)
            {
                return;
            }

            var removeIndex = _columns.IndexOf(SelectedColumn);
            _columns.RemoveAt(removeIndex);
            if (_columns.Count > 0)
            {
                var nextIndex = Math.Min(removeIndex, _columns.Count - 1);
                SelectedColumn = _columns[nextIndex];
            }
            else
            {
                SelectedColumn = null;
            }
        }

        /// <summary>
        /// 按偏移量移动列顺序。
        /// </summary>
        private void MoveColumn(int offset)
        {
            if (SelectedColumn is null)
            {
                return;
            }

            var currentIndex = _columns.IndexOf(SelectedColumn);
            var targetIndex = currentIndex + offset;
            if (targetIndex < 0 || targetIndex >= _columns.Count)
            {
                return;
            }

            _columns.Move(currentIndex, targetIndex);
        }

        /// <summary>
        /// 判断是否可移动。
        /// </summary>
        private bool CanMove(int offset)
        {
            if (SelectedColumn is null)
            {
                return false;
            }

            var currentIndex = _columns.IndexOf(SelectedColumn);
            var targetIndex = currentIndex + offset;
            return targetIndex >= 0 && targetIndex < _columns.Count;
        }

        /// <summary>
        /// 校验并输出结果。
        /// </summary>
        private void ConfirmChanges()
        {
            if (!Validate())
            {
                return;
            }

            ResultColumns = _columns
                .Select(item => new ColumnSchemaEntry(item.Name, item.IsReserved, item.OriginalName))
                .ToList();

            RequestClose?.Invoke(this, true);
        }

        /// <summary>
        /// 校验列名合法性与唯一性。
        /// </summary>
        private bool Validate()
        {
            if (_columns.Count == 0)
            {
                ErrorMessage = "至少保留一列";
                return false;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var column in _columns)
            {
                if (string.IsNullOrWhiteSpace(column.Name))
                {
                    ErrorMessage = "列名不能为空";
                    return false;
                }

                var normalized = column.Name.Trim();
                if (!column.Name.Equals(normalized, StringComparison.Ordinal))
                {
                    column.Name = normalized;
                }

                if (column.IsReserved && !normalized.Equals(column.OriginalName, StringComparison.Ordinal))
                {
                    ErrorMessage = $"保留列 {column.OriginalName} 不允许重命名";
                    return false;
                }

                if (!seen.Add(normalized))
                {
                    ErrorMessage = "列名必须唯一";
                    return false;
                }
            }

            ErrorMessage = null;
            return true;
        }
    }

    public sealed class ColumnEditorItem : ViewModelBase
    {
        private string _name;

        /// <summary>
        /// 构造列项。
        /// </summary>
        public ColumnEditorItem(string name, bool isReserved, string? originalName)
        {
            _name = name;
            IsReserved = isReserved;
            OriginalName = originalName;
        }

        /// <summary>
        /// 列名（可编辑）。
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 是否保留列。
        /// </summary>
        public bool IsReserved { get; }

        /// <summary>
        /// 原始列名（用于还原/校验）。
        /// </summary>
        public string? OriginalName { get; }

        /// <summary>
        /// 是否允许删除。
        /// </summary>
        public bool CanRemove => !IsReserved;

        /// <summary>
        /// 是否允许编辑。
        /// </summary>
        public bool CanEdit => !IsReserved;
    }
}
