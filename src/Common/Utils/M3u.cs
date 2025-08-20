using m3u_editor.Common.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace m3u_editor.Common.Utils
{
    public class M3u
    {

        /// <summary>
        /// 解析M3U文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>M3U条目集合</returns>
        public async Task<ObservableCollection<M3uEntry>> ParseM3uFileAsync(string filePath)
        {
            if (!File.Exists(filePath)) return new ObservableCollection<M3uEntry>();
            
            var entries = new ObservableCollection<M3uEntry>();
            try
            {
                using var reader = new StreamReader(filePath);
                M3uEntry? currentEntry = null;
                string? line;
                
                while ((line = await reader.ReadLineAsync()) is not null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    if (line.StartsWith("#EXTINF:"))
                    {
                        line = line["#EXTINF:".Length..].Trim();
                        int commaIndex = line.IndexOf(',');
                        if (commaIndex > 0)
                        {
                            currentEntry = new M3uEntry
                            {
                                Tvgname = ExtractM3uInfo(line[..commaIndex], "tvg-name"),
                                Tvgid = ExtractM3uInfo(line[..commaIndex], "tvg-id"),
                                Tvglogo = ExtractM3uInfo(line[..commaIndex], "tvg-logo"),
                                Name2 = line[(commaIndex + 1)..].Trim(),
                                Grouptitle = ExtractM3uInfo(line[..commaIndex], "group-title"),
                                IsHighlighted = false
                            };
                            entries.Add(currentEntry);
                        }
                    }
                    else if (currentEntry is not null && !line.StartsWith("#"))
                    {
                        currentEntry.Link = line;
                    }
                }
                return entries;
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"读取M3U文件时发生IO错误: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"解析M3U文件时发生错误: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// 解析JSON文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>M3U条目集合</returns>
        public async Task<ObservableCollection<M3uEntry>> ParseJsonFileAsync(string filePath)
        {
            if (!File.Exists(filePath)) return new ObservableCollection<M3uEntry>();
            
            try
            {
                var jsonContent = await File.ReadAllTextAsync(filePath);
                return JsonConvert.DeserializeObject<ObservableCollection<M3uEntry>>(jsonContent) ?? new ObservableCollection<M3uEntry>();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"JSON文件格式错误: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"读取JSON文件时发生IO错误: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"解析JSON文件时发生错误: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// 解析TXT文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>M3U条目集合</returns>
        public async Task<ObservableCollection<M3uEntry>> ParseTxtFileAsync(string filePath)
        {
            if (!File.Exists(filePath)) return new ObservableCollection<M3uEntry>();
            
            var entries = new ObservableCollection<M3uEntry>();
            try
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (line.StartsWith("#")) continue;
                    
                    var parts = line.Split(',');
                    if (parts.Length >= 2)
                    {
                        entries.Add(new M3uEntry
                        {
                            Name2 = parts[0].Trim(),
                            Tvgname = parts[0].Trim(),
                            Link = parts[1].Trim(),
                            IsHighlighted = false
                        });
                    }
                }
                return entries;
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"读取TXT文件时发生IO错误: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"解析TXT文件时发生错误: {ex.Message}", ex);
            }
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

            return string.Empty;
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
            // 获取入口程序集
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                // 首先尝试获取信息版本（对应.csproj中的Version）
                var informationalVersionAttr = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false).FirstOrDefault() as System.Reflection.AssemblyInformationalVersionAttribute;
                if (informationalVersionAttr != null && !string.IsNullOrEmpty(informationalVersionAttr.InformationalVersion))
                {
                    // 移除可能附加的提交哈希（以'+'开头的内容）
                    var infoVersion = informationalVersionAttr.InformationalVersion;
                    int plusIndex = infoVersion.IndexOf('+');
                    if (plusIndex >= 0)
                    {
                        infoVersion = infoVersion.Substring(0, plusIndex);
                    }
                    return infoVersion;
                }

                // 如果没有信息版本，则使用程序集版本
                var asmVersion = assembly.GetName().Version;
                if (asmVersion != null)
                {
                    return asmVersion.ToString();
                }
            }

            // 备用方案：返回文件版本
            var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (!string.IsNullOrEmpty(fileVersionInfo.ProductVersion))
            {
                // 同样处理文件版本中的提交哈希
                var fileVersion = fileVersionInfo.ProductVersion;
                int plusIndex = fileVersion.IndexOf('+');
                if (plusIndex >= 0)
                {
                    fileVersion = fileVersion.Substring(0, plusIndex);
                }
                return fileVersion;
            }

            return "1.0.0"; // 默认版本
        }
        private readonly string ConfigPath = $"{AppDomain.CurrentDomain.BaseDirectory}/config.json";
        public configEntry ReadConfig()
        {
            configEntry? config = null;
            try
            {
                config = JsonConvert.DeserializeObject<configEntry>(File.ReadAllText(ConfigPath));
            }
            catch
            {
                // Ignore and create new config
            }

            if (config == null)
            {
                config = new configEntry();
                config.IsDark = false;
                WriteConfig(config);
            }

            return config;


        }
        public void WriteConfig(configEntry Config)
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
            WriteConfig(Config);
        }
    }
    class ExportedM3uEntry
    {
        public string Tvgname { get; set; } = string.Empty;
        public string Tvgid { get; set; } = string.Empty;
        public string Tvglogo { get; set; } = string.Empty;
        public string Grouptitle { get; set; } = string.Empty;
        public string Name2 { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
    }
}
