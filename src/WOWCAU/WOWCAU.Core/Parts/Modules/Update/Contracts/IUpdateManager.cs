using WOWCAU.Core.Parts.Helper.Types;
using WOWCAU.Core.Parts.Modules.Update.Types;

namespace WOWCAU.Core.Parts.Modules.Update.Contracts
{
    public interface IUpdateManager
    {
        Task<UpdateData> CheckForUpdateAsync(CancellationToken cancellationToken = default);
        Task DownloadUpdateAsync(UpdateData updateData, string updateFolder, IProgress<DownloadProgress>? downloadProgress = default, CancellationToken cancellationToken = default);
        Task ApplyUpdateAsync(string updateFolder, CancellationToken cancellationToken = default);
    }
}
