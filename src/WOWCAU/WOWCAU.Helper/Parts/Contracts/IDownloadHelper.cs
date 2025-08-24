using WOWCAU.Helper.Parts.Types;

namespace WOWCAU.Helper.Parts.Contracts
{
    public interface IDownloadHelper
    {
        Task DownloadFileAsync(string downloadUrl, string filePath, IProgress<DownloadProgress>? progress = default, CancellationToken cancellationToken = default);
    }
}
