using WOWCAU.Core.Parts.Helper.Types;

namespace WOWCAU.Core.Parts.Helper.Contracts
{
    public interface IDownloadHelper
    {
        Task DownloadFileAsync(string downloadUrl, string filePath, IProgress<DownloadProgress>? progress = default, CancellationToken cancellationToken = default);
    }
}
