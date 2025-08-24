namespace WOWCAU.Core.Parts.Addons.Contracts
{
    public interface IMultiAddonProcessor
    {
        Task<uint> ProcessAddonsAsync(IEnumerable<string> addonUrls, string downloadFolder, string unzipFolder,
            IProgress<byte>? progress = default, CancellationToken cancellationToken = default);
    }
}
