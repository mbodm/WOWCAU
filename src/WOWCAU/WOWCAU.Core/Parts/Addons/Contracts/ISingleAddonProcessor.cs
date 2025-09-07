using WOWCAU.Core.Parts.Addons.Types;

namespace WOWCAU.Core.Parts.Addons.Contracts
{
    public interface ISingleAddonProcessor
    {
        Task ProcessAddonAsync(string addonName, string downloadUrl, string downloadFolder, string unzipFolder,
            IProgress<AddonProgress>? progress = default, CancellationToken cancellationToken = default);
    }
}
