using HandyControl.Themes;
using HandyControl.Tools;
using m3u_editor.Commands;
using m3u_editor.Models;
using m3u_editor.Services;
using m3u_editor.Views;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace m3u_editor.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private const string GithubUrl = "https://github.com/fallssyj/m3u-editor";
        private string _appTitle = "m3u-editor";
        private readonly IM3uParser _m3uParser;
        private readonly IFileDialogService _fileDialogService;
        private readonly IColumnEditorService _columnEditorService;
        private readonly IAppSettingsService _settingsService;
        private readonly AppSettings _settings;
        private readonly RelayCommand _loadM3uCommand;
        private readonly RelayCommand _saveM3uCommand;
        private readonly RelayCommand _loadDataFileCommand;
        private readonly RelayCommand _reloadM3uCommand;
        private readonly RelayCommand _reloadJsonCommand;
        private readonly RelayCommand _playCommand;
        private readonly RelayCommand _moveUpCommand;
        private readonly RelayCommand _moveDownCommand;
        private readonly RelayCommand _insertAfterCommand;
        private readonly RelayCommand _deleteCommand;
        private readonly RelayCommand _editColumnsCommand;
        private readonly RelayCommand _openAboutCommand;
        private DataView? _channelTable;
        private DataRowView? _selectedRow;
        private string? _selectedSearchColumn;
        private string _searchText = string.Empty;
        private readonly ObservableCollection<string> _availableColumns = new();
        private readonly ObservableCollection<string> _groupTitleCandidates = new();
        private string? _selectedFilePath;
        private string _statusMessage = "尚未加载播放列表";
        private bool _isBusy;

        private const string StreamUrlCandidatesColumn = "StreamUrlCandidates";

        // 默认构造函数使用内置服务，方便在设计器中预览。
        public MainViewModel()
            : this(new M3uParser(), new FileDialogService(), new ColumnEditorService(), new AppSettingsService())
        {
        }

        public MainViewModel(IM3uParser m3uParser, IFileDialogService fileDialogService)
            : this(m3uParser, fileDialogService, new ColumnEditorService(), new AppSettingsService())
        {
        }

        public MainViewModel(
            IM3uParser m3uParser,
            IFileDialogService fileDialogService,
            IColumnEditorService columnEditorService,
            IAppSettingsService settingsService)
        {
            _m3uParser = m3uParser ?? throw new ArgumentNullException(nameof(m3uParser));
            _fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
            _columnEditorService = columnEditorService ?? throw new ArgumentNullException(nameof(columnEditorService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            // 启动时加载配置，并应用主题。
            _settings = _settingsService.Load();
            ApplyTheme(_settings.ThemeMode);

            ChangeThemeCommand = new RelayCommand(_ => ToggleTheme());
            OpenGithubCommand = new RelayCommand(_ => OpenGithub());
            _loadM3uCommand = new RelayCommand(async _ => await LoadM3uAsync(), _ => !IsBusy);
            _saveM3uCommand = new RelayCommand(async _ => await SaveM3uAsync(), _ => !IsBusy && HasChannelData);
            _loadDataFileCommand = new RelayCommand(async _ => await LoadDataFileAsync(), _ => !IsBusy && HasChannelData);
            _reloadM3uCommand = new RelayCommand(async _ => await ReloadM3uAsync(), _ => !IsBusy && !string.IsNullOrWhiteSpace(SelectedFilePath));
            _reloadJsonCommand = new RelayCommand(async _ => await ExportJsonAsync(), _ => !IsBusy && HasChannelData);
            _playCommand = new RelayCommand(async parameter => await PlayAsync(parameter), _ => !IsBusy && HasChannelData);
            _moveUpCommand = new RelayCommand(_ => MoveSelectedRow(-1), _ => CanMoveSelectedRow(-1));
            _moveDownCommand = new RelayCommand(_ => MoveSelectedRow(1), _ => CanMoveSelectedRow(1));
            _insertAfterCommand = new RelayCommand(_ => InsertAfterSelected(), _ => !IsBusy);
            _deleteCommand = new RelayCommand(_ => DeleteSelectedRow(), _ => !IsBusy && SelectedRow is not null);
            _editColumnsCommand = new RelayCommand(_ => EditColumns(), _ => !IsBusy);
            _openAboutCommand = new RelayCommand(_ => OpenAbout(), _ => !IsBusy);
        }

        public string AppTitle
        {
            get => _appTitle;
            set => SetProperty(ref _appTitle, value);
        }

        public ICommand ChangeThemeCommand { get; }

        public ICommand OpenGithubCommand { get; }

        public ICommand LoadM3uCommand => _loadM3uCommand;

        public ICommand SaveM3uCommand => _saveM3uCommand;

        public ICommand LoadDataFileCommand => _loadDataFileCommand;

        public ICommand ReloadM3uCommand => _reloadM3uCommand;

        public ICommand ReloadJsonCommand => _reloadJsonCommand;

        public ICommand PlayCommand => _playCommand;

        public ICommand MoveUpCommand => _moveUpCommand;

        public ICommand MoveDownCommand => _moveDownCommand;

        public ICommand InsertAfterCommand => _insertAfterCommand;

        public ICommand DeleteCommand => _deleteCommand;

        public ICommand EditColumnsCommand => _editColumnsCommand;

        public ICommand OpenAboutCommand => _openAboutCommand;

        public ObservableCollection<string> AvailableColumns => _availableColumns;

        public ObservableCollection<string> GroupTitleCandidates => _groupTitleCandidates;

        public DataView? ChannelTable
        {
            get => _channelTable;
            private set
            {
                if (SetProperty(ref _channelTable, value))
                {
                    UpdateSearchColumns();
                    UpdateGroupTitleCandidates();
                    RefreshCommandStates();
                }
            }
        }

        public DataRowView? SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (SetProperty(ref _selectedRow, value))
                {
                    RefreshCommandStates();
                }
            }
        }

        public string? SelectedSearchColumn
        {
            get => _selectedSearchColumn;
            set
            {
                if (SetProperty(ref _selectedSearchColumn, value))
                {
                    ApplySearchFilter();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplySearchFilter();
                }
            }
        }

        public string? SelectedFilePath
        {
            get => _selectedFilePath;
            private set
            {
                if (SetProperty(ref _selectedFilePath, value))
                {
                    RefreshCommandStates();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    RefreshCommandStates();
                }
            }
        }

        private bool HasChannelData => ChannelTable is { Count: > 0 };

        /// <summary>
        /// 切换主题并持久化到配置文件。
        /// </summary>
        private void ToggleTheme()
        {

            var window = Application.Current?.MainWindow;
            if (window != null)
            {
                ThemeAnimationHelper.AnimateTheme(window, ThemeAnimationHelper.SlideDirection.Top, 0.3, 1, 0.5);
            }

            var currentTheme = ThemeManager.Current.ApplicationTheme;

            ThemeManager.Current.ApplicationTheme = currentTheme == ApplicationTheme.Light
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light;
            ThemeAnimationHelper.AnimateTheme(window, ThemeAnimationHelper.SlideDirection.Bottom, 0.3, 0.5, 1);

            // 保存到设置
            _settings.ThemeMode = ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light
                ? ThemeMode.Light
                : ThemeMode.Dark;
            _settingsService.Save(_settings);

        }

        /// <summary>
        /// 应用指定主题（启动时调用）。
        /// </summary>
        private void ApplyTheme(ThemeMode theme)
        {

            ThemeManager.Current.ApplicationTheme = theme == ThemeMode.Light
                ? ApplicationTheme.Light
                : ApplicationTheme.Dark;

        }

        private static void OpenGithub()
        {
            Process.Start(new ProcessStartInfo(GithubUrl)
            {
                UseShellExecute = true
            });
        }

        private void OpenAbout()
        {
            var window = new AboutWindow
            {
                Owner = Application.Current?.Windows
                    .OfType<System.Windows.Window>()
                    .FirstOrDefault(w => w.IsActive)
            };

            window.ShowDialog();
        }

        /// <summary>
        /// 打开并解析 m3u 文件。
        /// </summary>
        private async Task LoadM3uAsync()
        {
            // 记住上次选择的目录，提升用户体验。
            var initialDirectory = string.IsNullOrWhiteSpace(SelectedFilePath) ? null : Path.GetDirectoryName(SelectedFilePath);
            var filePath = _fileDialogService.PickM3uFile(initialDirectory);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                StatusMessage = "已取消选择播放列表";
                return;
            }

            await LoadPlaylistFromPathAsync(filePath);
        }

        public static bool IsSupportedPlaylistFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return false;
            }

            var extension = Path.GetExtension(filePath);
            return extension.Equals(".m3u", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".m3u8", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".txt", StringComparison.OrdinalIgnoreCase);
        }

        public async Task LoadPlaylistFromPathAsync(string filePath)
        {
            if (!IsSupportedPlaylistFile(filePath) || !File.Exists(filePath))
            {
                return;
            }

            SelectedFilePath = filePath;

            try
            {
                IsBusy = true;
                StatusMessage = "正在解析播放列表...";

                var table = await _m3uParser.ParseAsync(filePath);
                EnsureCandidateColumn(table);
                ChannelTable = table.DefaultView;
                StatusMessage = $"成功加载 {table.Rows.Count} 个频道";
            }
            catch (Exception ex)
            {
                StatusMessage = $"解析失败：{ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// 保存当前 DataGrid 数据为 m3u。
        /// </summary>
        private async Task SaveM3uAsync()
        {
            if (!HasChannelData || ChannelTable is null)
            {
                StatusMessage = "当前没有可保存的频道数据";
                return;
            }

            var initialDirectory = string.IsNullOrWhiteSpace(SelectedFilePath) ? null : Path.GetDirectoryName(SelectedFilePath);
            var defaultFileName = string.IsNullOrWhiteSpace(SelectedFilePath) ? "playlist.m3u8" : Path.GetFileName(SelectedFilePath);
            var targetPath = _fileDialogService.SaveM3uFile(initialDirectory, defaultFileName);
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                StatusMessage = "已取消保存播放列表";
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "正在保存 m3u 文件...";

                var content = await Task.Run(() => BuildM3uContent(ChannelTable));
                await File.WriteAllTextAsync(targetPath, content, Encoding.UTF8);

                SelectedFilePath = targetPath;
                StatusMessage = $"播放列表已保存到 {targetPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存失败：{ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// 重新加载当前 m3u 文件。
        /// </summary>
        private async Task ReloadM3uAsync()
        {
            // 直接使用当前路径重新加载，不弹出对话框。
            if (string.IsNullOrWhiteSpace(SelectedFilePath))
            {
                StatusMessage = "尚未选择播放列表";
                return;
            }

            if (!File.Exists(SelectedFilePath))
            {
                StatusMessage = "播放列表文件不存在";
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "正在重载 m3u 文件...";

                var table = await _m3uParser.ParseAsync(SelectedFilePath);
                EnsureCandidateColumn(table);
                ChannelTable = table.DefaultView;
                StatusMessage = $"重载成功：{table.Rows.Count} 个频道";
            }
            catch (Exception ex)
            {
                StatusMessage = $"重载失败：{ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void MoveSelectedRow(int offset)
        {
            if (ChannelTable?.Table is not { } table || SelectedRow is null)
            {
                StatusMessage = "请选择需要移动的频道";
                return;
            }

            var row = SelectedRow.Row;
            var currentIndex = table.Rows.IndexOf(row);
            var targetIndex = currentIndex + offset;
            if (currentIndex < 0 || targetIndex < 0 || targetIndex >= table.Rows.Count)
            {
                StatusMessage = offset < 0 ? "已位于列表顶部" : "已位于列表底部";
                return;
            }

            var rowData = row.ItemArray.Clone() as object[] ?? Array.Empty<object>();
            table.Rows.Remove(row);
            var newRow = table.NewRow();
            newRow.ItemArray = rowData;
            table.Rows.InsertAt(newRow, targetIndex);
            SelectedRow = table.DefaultView[targetIndex];
            StatusMessage = offset < 0 ? "已上移频道" : "已下移频道";
        }

        private bool CanMoveSelectedRow(int offset)
        {
            if (IsBusy || ChannelTable?.Table is not { } table || SelectedRow is null)
            {
                return false;
            }

            var currentIndex = table.Rows.IndexOf(SelectedRow.Row);
            var targetIndex = currentIndex + offset;
            return currentIndex >= 0 && targetIndex >= 0 && targetIndex < table.Rows.Count;
        }

        private void InsertAfterSelected()
        {
            var table = EnsureChannelTable();
            if (table is null)
            {
                StatusMessage = "请先加载或创建播放列表";
                return;
            }

            EnsureCandidateColumn(table);

            var newRow = table.NewRow();
            if (table.Columns.Contains("ChannelName"))
            {
                newRow["ChannelName"] = "新频道";
            }

            if (table.Columns.Contains("Duration"))
            {
                newRow["Duration"] = "-1";
            }

            if (table.Columns.Contains(StreamUrlCandidatesColumn))
            {
                newRow[StreamUrlCandidatesColumn] = new ObservableCollection<string>();
            }

            var insertIndex = SelectedRow is null
                ? table.Rows.Count
                : table.Rows.IndexOf(SelectedRow.Row) + 1;
            if (insertIndex < 0)
            {
                insertIndex = 0;
            }
            else if (insertIndex > table.Rows.Count)
            {
                insertIndex = table.Rows.Count;
            }

            table.Rows.InsertAt(newRow, insertIndex);
            SelectedRow = table.DefaultView[insertIndex];
            StatusMessage = "已插入新的频道";
        }

        private void DeleteSelectedRow()
        {
            if (ChannelTable?.Table is not { } table || SelectedRow is null)
            {
                StatusMessage = "请先选择需要删除的频道";
                return;
            }

            var row = SelectedRow.Row;
            var removeIndex = table.Rows.IndexOf(row);
            if (removeIndex < 0)
            {
                return;
            }

            table.Rows.Remove(row);
            if (table.DefaultView.Count > 0)
            {
                var nextIndex = System.Math.Min(removeIndex, table.DefaultView.Count - 1);
                SelectedRow = table.DefaultView[nextIndex];
            }
            else
            {
                SelectedRow = null;
            }

            StatusMessage = "已删除选中频道";
        }

        /// <summary>
        /// 打开列编辑器，支持改名/排序/增删列。
        /// </summary>
        private void EditColumns()
        {
            var table = EnsureChannelTable();
            if (table is null)
            {
                StatusMessage = "无法获取数据表";
                return;
            }

            var schema = table.Columns.Cast<DataColumn>()
                .Select(column => new ColumnSchemaEntry(column.ColumnName, IsReservedColumn(column.ColumnName), column.ColumnName))
                .ToList();

            void ApplySchema(IReadOnlyList<ColumnSchemaEntry> result)
            {
                ApplyColumnChanges(table, result);
                var preferredIndex = SelectedRow is null ? -1 : table.Rows.IndexOf(SelectedRow.Row);
                RebindDataView(table, preferredIndex);
                StatusMessage = "列信息已更新";
            }

            var result = _columnEditorService.EditColumns(schema, ApplySchema);
            if (result is null)
            {
                StatusMessage = "已关闭列编辑";
                return;
            }

            ApplySchema(result);
        }

        private DataTable? EnsureChannelTable()
        {
            if (ChannelTable?.Table is { } table)
            {
                return table;
            }

            var newTable = new DataTable("Channels");
            newTable.Columns.Add("ChannelName");
            newTable.Columns.Add("StreamUrl");
            newTable.Columns.Add("Duration");
            newTable.Columns.Add(StreamUrlCandidatesColumn, typeof(ObservableCollection<string>));
            ChannelTable = newTable.DefaultView;
            return newTable;
        }

        private static void EnsureCandidateColumn(DataTable table)
        {
            if (!table.Columns.Contains(StreamUrlCandidatesColumn))
            {
                table.Columns.Add(StreamUrlCandidatesColumn, typeof(ObservableCollection<string>));
            }

            foreach (DataRow row in table.Rows)
            {
                if (row[StreamUrlCandidatesColumn] is not ObservableCollection<string>)
                {
                    row[StreamUrlCandidatesColumn] = new ObservableCollection<string>();
                }
            }
        }

        private static void ApplyColumnChanges(DataTable table, IReadOnlyList<ColumnSchemaEntry> schema)
        {
            if (schema.Count == 0)
            {
                return;
            }

            var ordinal = 0;
            foreach (var entry in schema)
            {
                if (string.IsNullOrWhiteSpace(entry.Name))
                {
                    continue;
                }

                var targetName = entry.Name.Trim();
                DataColumn? column = null;

                if (!string.IsNullOrWhiteSpace(entry.OriginalName) && table.Columns.Contains(entry.OriginalName))
                {
                    column = table.Columns[entry.OriginalName];
                }
                else if (table.Columns.Contains(targetName))
                {
                    column = table.Columns[targetName];
                }

                if (column is null)
                {
                    column = table.Columns.Add(targetName);
                    foreach (DataRow row in table.Rows)
                    {
                        row[column] = string.Empty;
                    }
                }
                else if (!column.ColumnName.Equals(targetName, StringComparison.Ordinal))
                {
                    column.ColumnName = targetName;
                }

                column.SetOrdinal(ordinal++);
            }

            var desiredNames = new HashSet<string>(schema.Select(entry => entry.Name.Trim()), StringComparer.OrdinalIgnoreCase);
            var removableColumns = table.Columns.Cast<DataColumn>()
                .Where(column => !desiredNames.Contains(column.ColumnName) && !IsReservedColumn(column.ColumnName))
                .ToList();

            foreach (var column in removableColumns)
            {
                table.Columns.Remove(column);
            }
        }

        private void RefreshCommandStates()
        {
            _loadM3uCommand?.RaiseCanExecuteChanged();
            _saveM3uCommand?.RaiseCanExecuteChanged();
            _loadDataFileCommand?.RaiseCanExecuteChanged();
            _reloadM3uCommand?.RaiseCanExecuteChanged();
            _reloadJsonCommand?.RaiseCanExecuteChanged();
            _playCommand?.RaiseCanExecuteChanged();
            _moveUpCommand?.RaiseCanExecuteChanged();
            _moveDownCommand?.RaiseCanExecuteChanged();
            _insertAfterCommand?.RaiseCanExecuteChanged();
            _deleteCommand?.RaiseCanExecuteChanged();
            _editColumnsCommand?.RaiseCanExecuteChanged();
            _openAboutCommand?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 导出当前数据为 JSON 文件。
        /// </summary>
        private async Task ExportJsonAsync()
        {
            if (!HasChannelData || ChannelTable?.Table is not { } table)
            {
                StatusMessage = "当前没有可导出的数据";
                return;
            }

            var initialDirectory = string.IsNullOrWhiteSpace(SelectedFilePath) ? null : Path.GetDirectoryName(SelectedFilePath);
            var defaultFileName = string.IsNullOrWhiteSpace(SelectedFilePath)
                ? "playlist.json"
                : Path.ChangeExtension(Path.GetFileName(SelectedFilePath), ".json");

            var targetPath = _fileDialogService.SaveJsonFile(initialDirectory, defaultFileName);
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                StatusMessage = "已取消导出 JSON";
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "正在导出 JSON...";

                var payload = table.Rows.Cast<DataRow>()
                    .Select(row => table.Columns.Cast<DataColumn>()
                        .ToDictionary(
                            column => column.ColumnName,
                            column => row.IsNull(column) ? null : row[column]))
                    .ToList();

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(targetPath, json, Encoding.UTF8);
                StatusMessage = $"JSON 已导出到 {targetPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"导出失败：{ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// 调用外部播放器播放当前频道。
        /// </summary>
        private async Task PlayAsync(object? parameter)
        {
            if (ChannelTable?.Table is not { } table)
            {
                StatusMessage = "尚未加载播放列表";
                return;
            }

            var rowView = parameter as DataRowView ?? SelectedRow;
            if (rowView is null)
            {
                StatusMessage = "请先选择需要播放的频道";
                return;
            }

            var streamUrl = table.Columns.Contains("StreamUrl") ? rowView["StreamUrl"]?.ToString() : null;
            if (string.IsNullOrWhiteSpace(streamUrl))
            {
                StatusMessage = "当前频道缺少播放地址";
                return;
            }

            // 若配置中的播放器不存在，提示用户重新选择。
            var playerPath = _settings.PlayerPath;
            if (string.IsNullOrWhiteSpace(playerPath) || !File.Exists(playerPath))
            {
                playerPath = _fileDialogService.PickPlayerExecutable();
                if (string.IsNullOrWhiteSpace(playerPath))
                {
                    StatusMessage = "已取消选择播放器";
                    return;
                }

                _settings.PlayerPath = playerPath;
                _settingsService.Save(_settings);
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = playerPath,
                    Arguments = streamUrl,
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                StatusMessage = "已调用外部播放器";
            }
            catch (Exception ex)
            {
                StatusMessage = $"启动播放器失败：{ex.Message}";
            }

            await Task.CompletedTask;
        }

        private void RebindDataView(DataTable table, int preferredIndex = -1)
        {
            ChannelTable = null;
            SelectedRow = null;
            ChannelTable = table.DefaultView;

            if (preferredIndex >= 0 && ChannelTable is { Count: > 0 } view && preferredIndex < view.Count)
            {
                SelectedRow = view[preferredIndex];
            }
        }

        private void UpdateSearchColumns()
        {
            _availableColumns.Clear();
            if (ChannelTable?.Table is not { } table)
            {
                SelectedSearchColumn = null;
                return;
            }

            foreach (DataColumn column in table.Columns)
            {
                _availableColumns.Add(column.ColumnName);
            }

            if (!string.IsNullOrWhiteSpace(SelectedSearchColumn) && _availableColumns.Contains(SelectedSearchColumn))
            {
                return;
            }

            SelectedSearchColumn = _availableColumns.Count > 0 ? _availableColumns[0] : null;
        }

        private void UpdateGroupTitleCandidates()
        {
            _groupTitleCandidates.Clear();
            if (ChannelTable?.Table is not { } table)
            {
                return;
            }

            var groupColumn = table.Columns
                .Cast<DataColumn>()
                .FirstOrDefault(column => column.ColumnName.Equals("group-title", StringComparison.OrdinalIgnoreCase));
            if (groupColumn is null)
            {
                return;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in table.Rows)
            {
                var value = row[groupColumn]?.ToString();
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                var trimmed = value.Trim();
                if (seen.Add(trimmed))
                {
                    _groupTitleCandidates.Add(trimmed);
                }
            }
        }

        /// <summary>
        /// 根据列名与关键字过滤 DataView。
        /// </summary>
        private void ApplySearchFilter()
        {
            if (ChannelTable is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(SearchText) || string.IsNullOrWhiteSpace(SelectedSearchColumn))
            {
                ChannelTable.RowFilter = string.Empty;
                return;
            }

            var escapedText = SearchText.Replace("'", "''");
            var escapedColumn = SelectedSearchColumn.Replace("]", "]]");
            ChannelTable.RowFilter = $"CONVERT([{escapedColumn}], 'System.String') LIKE '%{escapedText}%'";
        }

        private static string BuildM3uContent(DataView view)
        {
            // 将 DataView 中的每行恢复成 #EXTINF + URL 的格式，保证用户编辑后的内容可以重新写回文件。
            var builder = new StringBuilder();
            builder.AppendLine("#EXTM3U");

            var table = view.Table ?? view.ToTable();
            foreach (DataRow row in table.Rows)
            {
                var streamUrl = row.Table.Columns.Contains("StreamUrl") ? row["StreamUrl"]?.ToString() : string.Empty;

                var duration = row.Table.Columns.Contains("Duration") ? row["Duration"]?.ToString() : null;
                builder.Append("#EXTINF:");
                builder.Append(string.IsNullOrWhiteSpace(duration) ? "-1" : duration);

                foreach (DataColumn column in table.Columns)
                {
                    var columnName = column.ColumnName;
                    if (IsReservedColumn(columnName))
                    {
                        continue;
                    }

                    var value = row[column]?.ToString();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    builder.Append(' ');
                    builder.Append(columnName);
                    builder.Append("=\"");
                    builder.Append(value.Replace("\"", "\\\""));
                    builder.Append('\"');
                }

                var channelName = row.Table.Columns.Contains("ChannelName") ? row["ChannelName"]?.ToString() : string.Empty;
                builder.Append(',');
                builder.AppendLine(string.IsNullOrWhiteSpace(channelName) ? "未知频道" : channelName);
                builder.AppendLine(streamUrl ?? string.Empty);
            }

            return builder.ToString();
        }

        private static bool IsReservedColumn(string columnName)
        {
            return columnName.Equals("ChannelName", StringComparison.OrdinalIgnoreCase) ||
                   columnName.Equals("StreamUrl", StringComparison.OrdinalIgnoreCase) ||
                   columnName.Equals("Duration", StringComparison.OrdinalIgnoreCase) ||
                   columnName.Equals(StreamUrlCandidatesColumn, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 加载数据文件（m3u），根据 ChannelName 生成 StreamUrl 候选列表。
        /// </summary>
        private async Task LoadDataFileAsync()
        {
            if (ChannelTable?.Table is not { } table)
            {
                StatusMessage = "请先加载播放列表";
                return;
            }

            var initialDirectory = string.IsNullOrWhiteSpace(SelectedFilePath) ? null : Path.GetDirectoryName(SelectedFilePath);
            var filePath = _fileDialogService.PickM3uFile(initialDirectory);
            if (string.IsNullOrWhiteSpace(filePath))
            {
                StatusMessage = "已取消选择数据文件";
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "正在解析数据文件...";

                var dataTable = await _m3uParser.ParseAsync(filePath);
                EnsureCandidateColumn(table);

                var candidates = dataTable.Rows.Cast<DataRow>()
                    .Select(row => new
                    {
                        Name = dataTable.Columns.Contains("ChannelName") ? row["ChannelName"]?.ToString() : null,
                        Url = dataTable.Columns.Contains("StreamUrl") ? row["StreamUrl"]?.ToString() : null
                    })
                    .Where(item => !string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.Url))
                    .ToList();

                var matchedRows = 0;
                foreach (DataRow row in table.Rows)
                {
                    var channelName = table.Columns.Contains("ChannelName") ? row["ChannelName"]?.ToString() : null;
                    var currentStreamUrl = table.Columns.Contains("StreamUrl") ? row["StreamUrl"]?.ToString() : null;
                    var list = new ObservableCollection<string>();

                    if (!string.IsNullOrWhiteSpace(channelName))
                    {
                        foreach (var item in candidates)
                        {
                            if (item.Name != null && item.Name.IndexOf(channelName, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                if (!string.IsNullOrWhiteSpace(currentStreamUrl) && string.Equals(item.Url, currentStreamUrl, StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }

                                if (!list.Contains(item.Url!))
                                {
                                    list.Add(item.Url!);
                                }
                            }
                        }
                    }

                    if (list.Count > 0)
                    {
                        matchedRows++;
                    }

                    row[StreamUrlCandidatesColumn] = list;
                }

                StatusMessage = $"数据文件已加载：{matchedRows} 行生成候选地址";
            }
            catch (Exception ex)
            {
                StatusMessage = $"加载数据文件失败：{ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
