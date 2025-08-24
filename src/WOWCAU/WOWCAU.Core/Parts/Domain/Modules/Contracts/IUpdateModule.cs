using WOWCAU.Core.Parts.Domain.Update.Types;
using WOWCAU.Core.Parts.Helper.Types;

namespace WOWCAU.Core.Parts.Domain.Modules.Contracts
{
    public interface IUpdateModule
    {
        Task<UpdateData> CheckForUpdateAsync(CancellationToken cancellationToken = default);
        Task DownloadUpdateAsync(UpdateData updateData, IProgress<DownloadProgress>? downloadProgress = default, CancellationToken cancellationToken = default);
        Task ApplyUpdateAndRestartApplicationAsync(CancellationToken cancellationToken = default);
        Task RemoveBakFileIfExistsAsync(CancellationToken cancellationToken = default);
    }
}
