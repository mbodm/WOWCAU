using WOWCAU.Core.Parts.Update.Types;
using WOWCAU.Helper.Parts.Types;

namespace WOWCAU.Core.Parts.Modules.Contracts
{
    public interface IUpdateModule
    {
        Task<UpdateData> CheckForUpdateAsync(CancellationToken cancellationToken = default);
        Task DownloadUpdateAsync(UpdateData updateData, IProgress<DownloadProgress>? downloadProgress = default, CancellationToken cancellationToken = default);
        Task ApplyUpdateAndRestartApplicationAsync(CancellationToken cancellationToken = default);
        Task RemoveBakFileIfExistsAsync(CancellationToken cancellationToken = default);
    }
}
