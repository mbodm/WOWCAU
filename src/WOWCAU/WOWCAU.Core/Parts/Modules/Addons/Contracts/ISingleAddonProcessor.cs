using WOWCAU.Core.Parts.Modules.Addons.Types;

namespace WOWCAU.Core.Parts.Modules.Addons.Contracts
{
    public interface ISingleAddonProcessor
    {
        Task ProcessAddonAsync(string addonName, string downloadUrl, string downloadFolder, string unzipFolder,
            IProgress<AddonProgress>? progress = default, CancellationToken cancellationToken = default);
    }
}
