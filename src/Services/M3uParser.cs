using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace m3u_editor.Services
{
    /// <summary>
    /// 负责解析 IPTV m3u 文件并转换成 DataTable，便于在 UI 层进行动态展示。
    /// </summary>
    public sealed class M3uParser : IM3uParser
    {
        /// <summary>
        /// 解析 EXTINF 属性键值对（key="value"）。
        /// </summary>
        private static readonly Regex AttributeRegex = new("(?<key>[A-Za-z0-9_-]+)=\"(?<value>[^\"]*)\"", RegexOptions.Compiled);

        public async Task<DataTable> ParseAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("文件路径不能为空", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("找不到指定的 m3u 文件", filePath);
            }

            var table = CreateBaseTable();
            var extension = Path.GetExtension(filePath);
            if (string.Equals(extension, ".txt", StringComparison.OrdinalIgnoreCase))
            {
                await ParseTxtAsync(filePath, table, cancellationToken).ConfigureAwait(false);
                return table;
            }

            var lines = await File.ReadAllLinesAsync(filePath, cancellationToken).ConfigureAwait(false);
            Dictionary<string, string>? pendingEntry = null;

            foreach (var rawLine in lines)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var line = rawLine?.Trim();
                if (string.IsNullOrEmpty(line))
                {
                    // 遇到空行时不立即丢弃 pendingEntry，等下一条有效指令处理。
                    continue;
                }

                if (line.StartsWith("#EXTINF", StringComparison.OrdinalIgnoreCase))
                {
                    // 若上一条 EXTINF 尚未匹配 URL，则先补一条空地址记录，避免丢失。
                    if (pendingEntry is not null)
                    {
                        pendingEntry["StreamUrl"] = string.Empty;
                        AddRow(table, pendingEntry);
                    }

                    pendingEntry = ParseExtInfLine(line);
                    continue;
                }

                if (line.StartsWith("#", StringComparison.Ordinal))
                {
                    // 忽略其它扩展指令，例如 #EXTVLCOPT 等。
                    continue;
                }

                if (pendingEntry is null)
                {
                    // 没有配套的 EXTINF，跳过孤立的 URL。
                    continue;
                }

                // URL 行可能携带额外名称，形如 "http://xx.xx.xx 频道名"，只取第一个 token 作为 URL。
                var (streamUrl, tailName) = ParseUrlLine(line);
                pendingEntry["StreamUrl"] = streamUrl;
                if (string.IsNullOrWhiteSpace(pendingEntry["ChannelName"]) && !string.IsNullOrWhiteSpace(tailName))
                {
                    pendingEntry["ChannelName"] = tailName;
                }

                AddRow(table, pendingEntry);
                pendingEntry = null;
            }

            // 文件结束时若仍有 EXTINF 未匹配 URL，则补一条空地址记录。
            if (pendingEntry is not null)
            {
                pendingEntry["StreamUrl"] = string.Empty;
                AddRow(table, pendingEntry);
            }

            return table;
        }

        private static async Task ParseTxtAsync(string filePath, DataTable table, CancellationToken cancellationToken)
        {
            var lines = await File.ReadAllLinesAsync(filePath, cancellationToken).ConfigureAwait(false);
            var isFirstLine = true;

            foreach (var rawLine in lines)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var line = rawLine?.Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split(',', 2, StringSplitOptions.TrimEntries);
                if (parts.Length < 2)
                {
                    continue;
                }

                if (isFirstLine &&
                    parts[0].Equals("ChannelName", StringComparison.OrdinalIgnoreCase) &&
                    parts[1].Equals("StreamUrl", StringComparison.OrdinalIgnoreCase))
                {
                    isFirstLine = false;
                    continue;
                }

                isFirstLine = false;
                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["ChannelName"] = parts[0],
                    ["StreamUrl"] = parts[1]
                };

                AddRow(table, values);
            }
        }

        /// <summary>
        /// 解析 EXTINF 行，拆出时长、属性与频道名。
        /// </summary>
        private static Dictionary<string, string> ParseExtInfLine(string line)
        {
            var payload = line[8..]; // 去掉 "#EXTINF:" 前缀
            var commaIndex = payload.IndexOf(',');
            var metaPart = commaIndex >= 0 ? payload[..commaIndex].Trim() : payload.Trim();
            var channelName = commaIndex >= 0 ? payload[(commaIndex + 1)..].Trim() : string.Empty;

            string duration = string.Empty;
            var spaceIndex = metaPart.IndexOf(' ');
            if (spaceIndex >= 0)
            {
                duration = metaPart[..spaceIndex].Trim();
                metaPart = metaPart[(spaceIndex + 1)..].Trim();
            }
            else
            {
                duration = metaPart;
                metaPart = string.Empty;
            }

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ChannelName"] = channelName,
                ["Duration"] = duration
            };

            foreach (Match match in AttributeRegex.Matches(metaPart))
            {
                var key = match.Groups["key"].Value;
                var value = match.Groups["value"].Value;
                if (!string.IsNullOrWhiteSpace(key))
                {
                    result[key] = value;
                }
            }

            return result;
        }

        /// <summary>
        /// 创建基础表结构。
        /// </summary>
        private static DataTable CreateBaseTable()
        {
            var table = new DataTable("Channels");
            table.Columns.Add("ChannelName");
            table.Columns.Add("StreamUrl");
            table.Columns.Add("Duration");
            return table;
        }

        /// <summary>
        /// 解析 URL 行，支持 "URL 名称" 的形式。
        /// </summary>
        private static (string StreamUrl, string TailName) ParseUrlLine(string line)
        {
            // 兼容 "URL 名称" 的形式，空格后的内容作为备用名称。
            var parts = line.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return (string.Empty, string.Empty);
            }

            var streamUrl = parts[0];
            var tailName = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            return (streamUrl, tailName);
        }

        /// <summary>
        /// 将解析结果写入 DataTable，并动态补齐列。
        /// </summary>
        private static void AddRow(DataTable table, Dictionary<string, string> values)
        {
            foreach (var key in values.Keys)
            {
                if (!table.Columns.Contains(key))
                {
                    table.Columns.Add(key);
                }
            }

            var row = table.NewRow();
            foreach (var pair in values)
            {
                row[pair.Key] = pair.Value;
            }

            table.Rows.Add(row);
        }
    }
}
