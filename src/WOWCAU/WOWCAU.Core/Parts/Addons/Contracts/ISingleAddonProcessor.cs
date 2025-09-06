using WOWCAU.Core.Parts.Addons.Types;

namespace WOWCAU.Core.Parts.Addons.Contracts
{
    public interface ISingleAddonProcessor
    {
        Task ProcessAddonAsync(string addonName, string downloadUrl, string downloadFolder, string unzipFolder,
            bool extractAlways = false, IProgress<AddonProgress>? progress = default, CancellationToken cancellationToken = default);
    }
}
