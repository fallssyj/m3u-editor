using m3u_editor.Commands;
using m3u_editor.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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

            _columns.CollectionChanged += Columns_CollectionChanged;
            foreach (var item in _columns)
            {
                RegisterItem(item);
            }

            AddCommand = new RelayCommand(_ => AddColumn());
            _removeCommand = new RelayCommand(_ => RemoveColumn(), _ => SelectedColumn?.CanRemove == true);
            _moveUpCommand = new RelayCommand(_ => MoveColumn(-1), _ => CanMove(-1));
            _moveDownCommand = new RelayCommand(_ => MoveColumn(1), _ => CanMove(1));
            SelectedColumn = _columns.FirstOrDefault();
            NotifyColumnsChanged();
        }

        /// <summary>
        /// 列发生变化时触发（校验通过后）。
        /// </summary>
        public event EventHandler<IReadOnlyList<ColumnSchemaEntry>>? ColumnsChanged;

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
        /// 用户确认后的列结果。
        /// </summary>
        public IReadOnlyList<ColumnSchemaEntry> ResultColumns { get; private set; } = Array.Empty<ColumnSchemaEntry>();

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
            NotifyColumnsChanged();
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

            NotifyColumnsChanged();
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
            NotifyColumnsChanged();
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



        private void Columns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
            {
                foreach (var item in e.OldItems.OfType<ColumnEditorItem>())
                {
                    UnregisterItem(item);
                }
            }

            if (e.NewItems is not null)
            {
                foreach (var item in e.NewItems.OfType<ColumnEditorItem>())
                {
                    RegisterItem(item);
                }
            }

            NotifyColumnsChanged();
        }

        private void RegisterItem(ColumnEditorItem item)
        {
            item.PropertyChanged += ColumnItem_PropertyChanged;
        }

        private void UnregisterItem(ColumnEditorItem item)
        {
            item.PropertyChanged -= ColumnItem_PropertyChanged;
        }

        private void ColumnItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ColumnEditorItem.Name))
            {
                NotifyColumnsChanged();
            }
        }

        private void NotifyColumnsChanged()
        {
            if (!Validate())
            {
                return;
            }

            ResultColumns = BuildResult();
            ColumnsChanged?.Invoke(this, ResultColumns);
        }

        private IReadOnlyList<ColumnSchemaEntry> BuildResult()
        {
            return _columns
                .Select(item => new ColumnSchemaEntry(item.Name, item.IsReserved, item.OriginalName))
                .ToList();
        }

        /// <summary>
        /// 校验列名合法性与唯一性。
        /// </summary>
        private bool Validate()
        {
            if (_columns.Count == 0)
            {
                return false;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var column in _columns)
            {
                if (string.IsNullOrWhiteSpace(column.Name))
                {
                    return false;
                }

                var normalized = column.Name.Trim();
                if (!column.Name.Equals(normalized, StringComparison.Ordinal))
                {
                    column.Name = normalized;
                }

                if (column.IsReserved && !normalized.Equals(column.OriginalName, StringComparison.Ordinal))
                {
                    return false;
                }

                if (!seen.Add(normalized))
                {
                    return false;
                }
            }

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
