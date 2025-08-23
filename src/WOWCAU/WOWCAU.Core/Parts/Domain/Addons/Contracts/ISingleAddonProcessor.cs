using WOWCAU.Core.Parts.Domain.Addons.Types;

namespace WOWCAU.Core.Parts.Domain.Addons.Contracts
{
    public interface ISingleAddonProcessor
    {
        Task ProcessAddonAsync(string addonName, string downloadUrl, string downloadFolder, string unzipFolder,
            IProgress<AddonProgress>? progress = default, CancellationToken cancellationToken = default);
    }
}
