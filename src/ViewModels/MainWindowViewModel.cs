using m3u_editor.Common.Models;
using m3u_editor.Common.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
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

        private string compileVersion;

        public string CompileVersion
        {
            get { return compileVersion; }
            set { compileVersion = value; RaisePropertyChanged(); }
        }


        private string filePath;

        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; RaisePropertyChanged(); }
        }

        private string searchText;

        public string SearchText
        {
            get { return searchText; }
            set { searchText = value; RaisePropertyChanged(); }
        }




        private ObservableCollection<M3uEntry> m3UEntries;
        public ObservableCollection<M3uEntry> M3UEntries
        {
            get { return m3UEntries; }
            set { m3UEntries = value; RaisePropertyChanged(); }
        }


        private ComboBoxItem selectedOption;

        public ComboBoxItem SelectedOption
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

        public DelegateCommand MinWindowCommand { get; private set; }
        public DelegateCommand MaxWindowCommand { get; private set; }
        public DelegateCommand CloseWindowCommand { get; private set; }
        public DelegateCommand OpenFileCommand { get; private set; }
        public DelegateCommand<DragEventArgs> DropCommand { get; private set; }
        public DelegateCommand SaveFileCommand { get; private set; }
        public DelegateCommand SaveJsonFileCommand { get; private set; }
        public DelegateCommand<DataGrid> UpItemMove { get; private set; }
        public DelegateCommand<DataGrid> DownItemMove { get; private set; }
        public DelegateCommand<DataGrid> AddItem { get; private set; }
        public DelegateCommand<DataGrid> DeleteItem { get; private set; }
        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand<DataGrid> SearchInputCommand { get; private set; }
        public DelegateCommand OpenGithub { get; private set; }
        public DelegateCommand OpenAboutCommand { get; private set; }
        public DelegateCommand ShowAboutCommand { get; private set; }

        M3u m3U = new M3u();


        public MainWindowViewModel()
        {
            InitializeComponents();

        }
        /// <summary>
        /// 初始化一些操作
        /// </summary
        private void InitializeComponents()
        {

            InitCommand();
            M3UEntries = new ObservableCollection<M3uEntry>();
            CompileVersion = $"Version {m3U.GetCompileVersion()}";
        }

        /// <summary>
        /// 注册一些事件
        /// </summary>
        private void InitCommand()
        {

            OpenFileCommand = new DelegateCommand(openFile);
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

        private void OpenAbout()
        {
            IsAbout = IsAbout != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;

        }

        private async void openFile()
        {

            await Task.Run(() =>
            {
                FilePath = m3U.GetSelectedFilePath("所有文件|*.*") ?? FilePath;
                if (string.IsNullOrEmpty(FilePath)) return;
                string FileExtension = System.IO.Path.GetExtension(FilePath);
                if (FileExtension.ToLower() == ".m3u")
                    M3UEntries = m3U.ParseM3uFile(FilePath);
                if (FileExtension.ToLower() == ".json")
                    M3UEntries = m3U.ParseJsonFile(FilePath);
                if (FileExtension.ToLower() == ".txt")
                    M3UEntries = m3U.ParseTxtFile(FilePath);
            });
        }
        private async void dropFileAsync(DragEventArgs e)
        {
            await Task.Run(() =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    FilePath = files[0];
                    if (string.IsNullOrEmpty(FilePath)) return;

                    string FileExtension = System.IO.Path.GetExtension(FilePath);
                    if (FileExtension.ToLower() == ".m3u")
                        M3UEntries = m3U.ParseM3uFile(FilePath);
                    if (FileExtension.ToLower() == ".json")
                        M3UEntries = m3U.ParseJsonFile(FilePath);
                    if (FileExtension.ToLower() == ".txt")
                        M3UEntries = m3U.ParseTxtFile(FilePath);
                }
            });

        }
        private void saveFile() { m3U.GenerateM3uString(M3UEntries, FilePath); }
        private void saveJsonFile() { m3U.GenerateJsonString(M3UEntries, FilePath); }
        private void refreshFile() { M3UEntries = m3U.ParseM3uFile(FilePath); }

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
        private async void searchItem(DataGrid dataGrid)
        {
            if (dataGrid == null) return;

            string searchTypeName = SelectedOption.Content.ToString();
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
                        item.IsHighlighted = item.GetType().GetProperty(searchTypeName).GetValue(item).ToString().Contains(SearchText) ? true : false;
                    }
                }
            });


            dataGrid.CancelEdit();
            dataGrid.CommitEdit();
            dataGrid.Items.Refresh();

        }

    }
}
