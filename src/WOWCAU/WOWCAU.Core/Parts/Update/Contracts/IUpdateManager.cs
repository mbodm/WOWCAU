using WOWCAU.Core.Parts.Update.Types;
using WOWCAU.Helper.Parts.Types;

namespace WOWCAU.Core.Parts.Update.Contracts
{
    public interface IUpdateManager
    {
        Task InitAsync(string pathToApplicationTempFolder, CancellationToken cancellationToken = default);
        Task<UpdateData> CheckForUpdateAsync(CancellationToken cancellationToken = default);
        Task DownloadUpdateAsync(UpdateData updateData, IProgress<DownloadProgress>? downloadProgress = default, CancellationToken cancellationToken = default);
        Task ApplyUpdateAsync(CancellationToken cancellationToken = default);
        void RestartApplication(uint delayInSeconds);
        Task RemoveBakFileIfExistsAsync(CancellationToken cancellationToken = default);
    }
}
