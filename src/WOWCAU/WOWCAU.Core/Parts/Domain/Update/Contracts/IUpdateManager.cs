using WOWCAU.Core.Parts.Helper.Types;
using WOWCAU.Core.Parts.Domain.Update.Types;

namespace WOWCAU.Core.Parts.Domain.Update.Contracts
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
