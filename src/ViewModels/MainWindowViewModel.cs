using m3u_editor.Common.Models;
using m3u_editor.Common.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace m3u_editor.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "m3u-editor";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string author = "fallssyj";
        public string Author
        {
            get { return author; }
            set { SetProperty(ref author, value); }
        }

        private string compileVersion = string.Empty;

        public string CompileVersion
        {
            get { return compileVersion; }
            set { compileVersion = value; RaisePropertyChanged(); }
        }


        private string filePath = string.Empty;

        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; RaisePropertyChanged(); }
        }

        private string searchText = string.Empty;

        public string SearchText
        {
            get { return searchText; }
            set { searchText = value; RaisePropertyChanged(); }
        }

        private configEntry config = new configEntry();

        public configEntry Config
        {
            get { return config; }
            set { config = value; }
        }

        private ObservableCollection<M3uEntry> m3UEntries = new ObservableCollection<M3uEntry>();
        public ObservableCollection<M3uEntry> M3UEntries
        {
            get { return m3UEntries; }
            set { m3UEntries = value; RaisePropertyChanged(); }
        }


        private ComboBoxItem? selectedOption;

        public ComboBoxItem? SelectedOption
        {
            get { return selectedOption; }
            set { selectedOption = value; RaisePropertyChanged(); }
        }
        private Visibility isAbout = Visibility.Collapsed;

        public Visibility IsAbout
        {
            get { return isAbout; }
            set { isAbout = value; RaisePropertyChanged(); }
        }

        public DelegateCommand MinWindowCommand { get; private set; } = null!;
        public DelegateCommand MaxWindowCommand { get; private set; } = null!;
        public DelegateCommand CloseWindowCommand { get; private set; } = null!;
        public DelegateCommand OpenFileCommand { get; private set; } = null!;
        public DelegateCommand<DragEventArgs> DropCommand { get; private set; } = null!;
        public DelegateCommand SaveFileCommand { get; private set; } = null!;
        public DelegateCommand SaveJsonFileCommand { get; private set; } = null!;
        public DelegateCommand<DataGrid> UpItemMove { get; private set; } = null!;
        public DelegateCommand<DataGrid> DownItemMove { get; private set; } = null!;
        public DelegateCommand<DataGrid> AddItem { get; private set; } = null!;
        public DelegateCommand<DataGrid> DeleteItem { get; private set; } = null!;
        public DelegateCommand RefreshCommand { get; private set; } = null!;
        public DelegateCommand<DataGrid> SearchInputCommand { get; private set; } = null!;
        public DelegateCommand OpenGithub { get; private set; } = null!;
        public DelegateCommand ChangeThemesCommand { get; private set; } = null!;
        public DelegateCommand OpenAboutCommand { get; private set; } = null!;
        public DelegateCommand ShowAboutCommand { get; private set; } = null!;

        private M3u m3U = new M3u();


        public MainWindowViewModel()
        {
            InitializeComponents();

        }
        /// <summary>
        /// 初始化一些操作
        /// </summary>
        private void InitializeComponents()
        {
            Config = m3U.ReadConfig();
            InitCommand();
            M3UEntries = new ObservableCollection<M3uEntry>();
            CompileVersion = $"Version {m3U.GetCompileVersion()}";
            m3U.ChangeThemes(Config.IsDark, Config);
        }


        /// <summary>
        /// 注册一些事件
        /// </summary>
        private void InitCommand()
        {

            OpenFileCommand = new DelegateCommand(openFileAsync);
            DropCommand = new DelegateCommand<DragEventArgs>(dropFileAsync);
            SaveFileCommand = new DelegateCommand(saveFile);
            UpItemMove = new DelegateCommand<DataGrid>(upItemMove);
            DownItemMove = new DelegateCommand<DataGrid>(downItemMove);
            AddItem = new DelegateCommand<DataGrid>(addItem);
            DeleteItem = new DelegateCommand<DataGrid>(deleteItem);
            SaveJsonFileCommand = new DelegateCommand(saveJsonFile);
            RefreshCommand = new DelegateCommand(refreshFile);
            SearchInputCommand = new DelegateCommand<DataGrid>(searchItem);



            MinWindowCommand = new DelegateCommand(() => { Application.Current.MainWindow.WindowState = WindowState.Minimized; });
            MaxWindowCommand = new DelegateCommand(() => { Application.Current.MainWindow.WindowState = Application.Current.MainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; });
            CloseWindowCommand = new DelegateCommand(() => { Application.Current.MainWindow.Close(); });

            ChangeThemesCommand = new DelegateCommand(() => { m3U.ChangeThemes(!Config.IsDark, Config); });

            OpenGithub = new DelegateCommand(() =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/fallssyj/m3u-editor",
                    UseShellExecute = true
                });
            });

            OpenAboutCommand = new DelegateCommand(OpenAbout);
            ShowAboutCommand = new DelegateCommand(OpenAbout);
        }

        /// <summary>
        /// 显示关于窗口
        /// </summary>
        private void OpenAbout()
        {
            IsAbout = IsAbout != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;

        }
        /// <summary>
        /// 打开文件
        /// </summary>
        private async void openFileAsync()
        {
            try
            {
                string? selectedPath = m3U.GetSelectedFilePath("所有文件|*.*");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    FilePath = selectedPath;
                }
                if (string.IsNullOrEmpty(FilePath)) return;
                
                string fileExtension = System.IO.Path.GetExtension(FilePath).ToLower();
                ObservableCollection<M3uEntry>? entries = null;
                
                switch (fileExtension)
                {
                    case ".m3u":
                        entries = await m3U.ParseM3uFileAsync(FilePath);
                        break;
                    case ".json":
                        entries = await m3U.ParseJsonFileAsync(FilePath);
                        break;
                    case ".txt":
                        entries = await m3U.ParseTxtFileAsync(FilePath);
                        break;
                    default:
                        MessageBox.Show("不支持的文件格式");
                        return;
                }
                
                if (entries != null)
                {
                    M3UEntries = entries;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开文件时发生错误: {ex.Message}");
            }
        }
        /// <summary>
        /// 接收到拖放文件
        /// </summary>
        /// <param name="e"></param>
        private async void dropFileAsync(DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files.Length > 0 && !string.IsNullOrEmpty(files[0]))
                    {
                        FilePath = files[0];
                    }
                    if (string.IsNullOrEmpty(FilePath)) return;

                    string fileExtension = System.IO.Path.GetExtension(FilePath).ToLower();
                    ObservableCollection<M3uEntry>? entries = null;

                    switch (fileExtension)
                    {
                        case ".m3u":
                            entries = await m3U.ParseM3uFileAsync(FilePath);
                            break;
                        case ".json":
                            entries = await m3U.ParseJsonFileAsync(FilePath);
                            break;
                        case ".txt":
                            entries = await m3U.ParseTxtFileAsync(FilePath);
                            break;
                        default:
                            MessageBox.Show("不支持的文件格式");
                            return;
                    }

                    if (entries != null)
                    {
                        M3UEntries = entries;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"拖放文件时发生错误: {ex.Message}");
            }
        }
        /// <summary>
        /// 保存为m3u
        /// </summary>
        private void saveFile() { m3U.GenerateM3uString(M3UEntries, FilePath); }
        /// <summary>
        /// 保存为json
        /// </summary>
        private void saveJsonFile() { m3U.GenerateJsonString(M3UEntries, FilePath); }
        /// <summary>
        /// 重载文件
        /// </summary>
        private async void refreshFile()
        {
            if (string.IsNullOrEmpty(FilePath)) return;
            
            try
            {
                string fileExtension = System.IO.Path.GetExtension(FilePath).ToLower();
                ObservableCollection<M3uEntry>? entries = null;

                switch (fileExtension)
                {
                    case ".m3u":
                        entries = await m3U.ParseM3uFileAsync(FilePath);
                        break;
                    case ".json":
                        entries = await m3U.ParseJsonFileAsync(FilePath);
                        break;
                    case ".txt":
                        entries = await m3U.ParseTxtFileAsync(FilePath);
                        break;
                    default:
                        MessageBox.Show("不支持的文件格式");
                        return;
                }

                if (entries != null)
                {
                    M3UEntries = entries;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"重载文件时发生错误: {ex.Message}");
            }
        }

        private void upItemMove(DataGrid dataGrid)
        {

            if (dataGrid == null || dataGrid.SelectedItem == null)
                return;
            int selectedIndex = dataGrid.SelectedIndex;
            if (selectedIndex > 0)
            {
                M3uEntry selectedItem = M3UEntries[selectedIndex];
                M3UEntries.RemoveAt(selectedIndex);
                M3UEntries.Insert(selectedIndex - 1, selectedItem);
                dataGrid.SelectedIndex = selectedIndex - 1;
                dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.SelectedIndex]);
            }

        }
        private void downItemMove(DataGrid dataGrid)
        {
            if (dataGrid == null || dataGrid.SelectedItem == null)
                return;
            int selectedIndex = dataGrid.SelectedIndex;
            if (selectedIndex < M3UEntries.Count - 1)
            {
                M3uEntry selectedItem = M3UEntries[selectedIndex];
                M3UEntries.RemoveAt(selectedIndex);
                M3UEntries.Insert(selectedIndex + 1, selectedItem);
                dataGrid.SelectedIndex = selectedIndex + 1;
                dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.SelectedIndex]);
            }

        }
        private void addItem(DataGrid dataGrid)
        {
            if (dataGrid == null || dataGrid.SelectedItem == null)
                return;
            int selectedIndex = dataGrid.SelectedIndex;
            M3uEntry newItem = new M3uEntry();
            M3UEntries.Insert(selectedIndex + 1, newItem);
            dataGrid.SelectedIndex = selectedIndex + 1;
            dataGrid.ScrollIntoView(dataGrid.Items[dataGrid.SelectedIndex]);
        }
        private void deleteItem(DataGrid dataGrid)
        {
            if (dataGrid == null || dataGrid.SelectedItem == null)
                return;
            int selectedIndex = dataGrid.SelectedIndex;
            M3UEntries.RemoveAt(selectedIndex);
            dataGrid.SelectedIndex = selectedIndex - 1;
        }
        /// <summary>
        /// 搜索列表
        /// </summary>
        /// <param name="dataGrid"></param>
        private async void searchItem(DataGrid dataGrid)
        {
            try
            {
                if (dataGrid == null) return;

                if (SelectedOption == null || SelectedOption.Content == null)
                {
                    // Clear highlights if no option selected
                    foreach (var item in M3UEntries)
                    {
                        item.IsHighlighted = false;
                    }
                    dataGrid.Items.Refresh();
                    return;
                }

                string searchTypeName = SelectedOption.Content?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(searchTypeName))
                {
                    foreach (var item in M3UEntries)
                    {
                        item.IsHighlighted = false;
                    }
                    dataGrid.Items.Refresh();
                    return;
                }

                await Task.Run(() =>
                {
                    if (string.IsNullOrEmpty(SearchText) || SearchText.Length < 1)
                    {
                        foreach (var item in M3UEntries)
                        {
                            item.IsHighlighted = false;
                        }
                    }
                    else
                    {
                        foreach (var item in M3UEntries)
                        {
                            var property = item.GetType().GetProperty(searchTypeName);
                            if (property != null)
                            {
                                var value = property.GetValue(item);
                                if (value != null)
                                {
                                    string valueString = value.ToString() ?? string.Empty;
                                    item.IsHighlighted = valueString.Contains(SearchText ?? string.Empty);
                                }
                                else
                                {
                                    item.IsHighlighted = false;
                                }
                            }
                            else
                            {
                                item.IsHighlighted = false;
                            }
                        }
                    }
                });

                dataGrid.CancelEdit();
                dataGrid.CommitEdit();
                dataGrid.Items.Refresh();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

    }
}
