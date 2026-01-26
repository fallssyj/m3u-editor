using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace m3u_editor.Services
{
    /// <summary>
    /// IPTV m3u 解析器契约，方便在不同 ViewModel 中注入使用。
    /// </summary>
    public interface IM3uParser
    {
        /// <summary>
        /// 解析指定路径的 m3u 文件并生成 DataTable，后续可直接绑定到 DataGrid。
        /// </summary>
        /// <param name="filePath">m3u 文件的绝对路径。</param>
        /// <param name="cancellationToken">用于取消耗时操作的标记。</param>
        /// <returns>包含频道信息的 DataTable。</returns>
        Task<DataTable> ParseAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
