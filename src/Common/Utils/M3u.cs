using m3u_editor.Common.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace m3u_editor.Common.Utils
{
    public class M3u
    {

        /// <summary>
        /// 解析M3U
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public ObservableCollection<M3uEntry> ParseM3uFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                ObservableCollection<M3uEntry> entries = new ObservableCollection<M3uEntry>();
                using (StreamReader reader = new StreamReader(filePath))
                {
                    M3uEntry currentEntry = null;
                    string line = null;
                    while ((line = reader.ReadLine()!) != null)
                    {
                        line = line.Trim();
                        if (line.StartsWith("#EXTINF:"))
                        {
                            line = line.Substring("#EXTINF:".Length).Trim();
                            int commaIndex = line.IndexOf(',');
                            if (commaIndex > 0)
                            {
                                string[] parts = line.Split(',');
                                currentEntry = new M3uEntry();
                                currentEntry.Tvgname = ExtractM3uInfo(parts[0], "tvg-name");
                                currentEntry.Tvgid = ExtractM3uInfo(parts[0], "tvg-id");
                                currentEntry.Tvglogo = ExtractM3uInfo(parts[0], "tvg-logo");
                                currentEntry.Name2 = parts[1].Trim();
                                currentEntry.Grouptitle = ExtractM3uInfo(parts[0], "group-title");

                                currentEntry.IsHighlighted = false;
                                entries.Add(currentEntry);
                            }
                        }
                        else if (currentEntry != null)
                        {
                            currentEntry.Link = line;
                        }
                    }
                }
                return entries;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return null;
        }
        /// <summary>
        /// 解析JSON
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public ObservableCollection<M3uEntry> ParseJsonFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                return JsonConvert.DeserializeObject<ObservableCollection<M3uEntry>>(File.ReadAllText(filePath));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return null;
        }
        /// <summary>
        /// 解析txt
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public ObservableCollection<M3uEntry> ParseTxtFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                ObservableCollection<M3uEntry> entries = new ObservableCollection<M3uEntry>();
                string[] vars = File.ReadAllLines(filePath);
                foreach (string var in vars)
                {
                    if (string.IsNullOrWhiteSpace(var)) continue;
                    if (var.Substring(0, 1) == "#") continue;
                    entries.Add(new M3uEntry() { Name2 = var.Split(',')[0], Tvgname = var.Split(',')[0], Link = var.Split(',')[1] });

                }
                return entries;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return null;
        }

        private string ExtractM3uInfo(string m3u, string tagInfo)
        {
            int startIndex = m3u.IndexOf(tagInfo);
            if (startIndex == -1)
                return "";
            startIndex += tagInfo.Length + 2;

            int endIndex = m3u.IndexOf("\"", startIndex);
            if (endIndex == -1)
                return "";


            return m3u.Substring(startIndex, endIndex - startIndex);
        }


        /// <summary>
        /// 打开OpenFileDialog 返回选择路径
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public string GetSelectedFilePath(string filter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = filter;

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// 保存M3u文件
        /// </summary>
        /// <returns></returns>
        public void GenerateM3uString(ObservableCollection<M3uEntry> m3Uentries, string path)
        {
            try
            {
                if (m3Uentries == null) return;
                StringBuilder m3uStr = new StringBuilder("#EXTM3U\r\n");

                foreach (M3uEntry Item in m3Uentries)
                {
                    m3uStr.AppendLine($"#EXTINF:-1 tvg-name=\"{Item.Tvgname}\" tvg-id=\"{Item.Tvgid}\" tvg-logo=\"{Item.Tvglogo}\" group-title=\"{Item.Grouptitle}\",{Item.Name2}");
                    m3uStr.AppendLine(Item.Link);
                }
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "m3u(*.m3u)|*.m3u";
                saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(path);
                saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (saveFileDialog.ShowDialog() == true && m3Uentries.Count > 0)
                {
                    File.WriteAllText(saveFileDialog.FileName, m3uStr.ToString(), Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// 保存Json文件
        /// </summary>
        /// <returns></returns>
        public void GenerateJsonString(ObservableCollection<M3uEntry> m3Uentries, string path)
        {
            try
            {
                if (m3Uentries == null) return;
                string jsonStr = JsonConvert.SerializeObject(
                    m3Uentries.Select(entry => new ExportedM3uEntry
                    {
                        Tvgname = entry.Tvgname,
                        Tvgid = entry.Tvgid,
                        Tvglogo = entry.Tvglogo,
                        Grouptitle = entry.Grouptitle,
                        Name2 = entry.Name2,
                        Link = entry.Link
                    }).ToList()
                    , Formatting.Indented);
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "json(*.json)|*.json";
                saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(path);
                saveFileDialog.FileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (saveFileDialog.ShowDialog() == true && m3Uentries.Count > 0)
                {
                    File.WriteAllText(saveFileDialog.FileName, jsonStr, Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public string GetCompileVersion()
        {
            string OriginVersion = "" + File.GetLastWriteTime(this.GetType().Assembly.Location);
            string formattedDate = "";

            foreach (char ch in OriginVersion)
            {
                if (char.IsDigit(ch))
                {
                    formattedDate += ch;
                }
            }

            return formattedDate.Length >= 8 ? formattedDate.Substring(0, 8) : "";

        }
        private readonly string ConfigPath = $"{AppDomain.CurrentDomain.BaseDirectory}/config.json";
        public configEntry ReadConfig()
        {
            configEntry Config;
            try
            {
                Config = JsonConvert.DeserializeObject<configEntry>(File.ReadAllText(ConfigPath));
                return Config;
            }
            catch
            {
                Config = new configEntry();
                Config.IsDark = false;
                WirteConfig(Config);
                return Config;
            }


        }
        public void WirteConfig(configEntry Config)
        {
            string jsonStr = JsonConvert.SerializeObject(Config);
            File.WriteAllText(ConfigPath, jsonStr);
        }

        public void ChangeThemes(bool IsThemeDark, configEntry Config)
        {
            var path = IsThemeDark ? new Uri("Themes/DarkTheme.xaml", UriKind.RelativeOrAbsolute) : new Uri("Themes/LightTheme.xaml", UriKind.RelativeOrAbsolute);

            foreach (var res in Application.Current.Resources.MergedDictionaries)
            {
                if (res.Source != null && (res.Source.ToString() == "Themes/LightTheme.xaml" || res.Source.ToString() == "Themes/DarkTheme.xaml"))
                {
                    res.Source = path;
                }
            }
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(IsThemeDark ? BaseTheme.Dark : BaseTheme.Light);
            theme.SetPrimaryColor(theme.GetBaseTheme() == BaseTheme.Dark ? Colors.OrangeRed : Colors.Blue);
            theme.SetSecondaryColor(Colors.Lime);
            paletteHelper.SetTheme(theme);
            Config.IsDark = IsThemeDark;
            WirteConfig(Config);
        }
    }
    class ExportedM3uEntry
    {
        public string Tvgname { get; set; }
        public string Tvgid { get; set; }
        public string Tvglogo { get; set; }
        public string Grouptitle { get; set; }
        public string Name2 { get; set; }
        public string Link { get; set; }
    }
}