using WOWCAU.Core.Parts.Addons.Contracts;
using WOWCAU.Core.Parts.Addons.Types;
using WOWCAU.Helper.Parts.Contracts;
using WOWCAU.Helper.Parts.Types;

namespace WOWCAU.Core.Parts.Addons.Defaults
{
    public sealed class SingleAddonProcessor(
        ICurseHelper curseHelper, IDownloadHelper downloadHelper, IUnzipHelper unzipHelper, ISmartUpdateFeature smartUpdateFeature) : ISingleAddonProcessor
    {
        private readonly ICurseHelper curseHelper = curseHelper ?? throw new ArgumentNullException(nameof(curseHelper));
        private readonly IDownloadHelper downloadHelper = downloadHelper ?? throw new ArgumentNullException(nameof(downloadHelper));
        private readonly IUnzipHelper unzipHelper = unzipHelper ?? throw new ArgumentNullException(nameof(unzipHelper));
        private readonly ISmartUpdateFeature smartUpdateFeature = smartUpdateFeature ?? throw new ArgumentNullException(nameof(smartUpdateFeature));

        public async Task ProcessAddonAsync(string addonName, string downloadUrl, string downloadFolder, string unzipFolder,
            IProgress<AddonProgress>? progress = default, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(addonName);
            ArgumentException.ThrowIfNullOrWhiteSpace(downloadUrl);
            ArgumentException.ThrowIfNullOrWhiteSpace(downloadFolder);
            ArgumentException.ThrowIfNullOrWhiteSpace(unzipFolder);

            // Get zip file name

            var zipFile = downloadUrl.Split('/').LastOrDefault();
            if (!curseHelper.IsDownloadUrl(downloadUrl) || zipFile == null)
            {
                throw new InvalidOperationException("Given addon download URL is not a valid Curse CDN download URL.");
            }

            // Handle SmartUpdate feature

            if (smartUpdateFeature.AddonExists(addonName, downloadUrl, zipFile))
            {
                // Copy zip file

                cancellationToken.ThrowIfCancellationRequested();

                smartUpdateFeature.DeployZipFile(addonName);

                progress?.Report(new AddonProgress(AddonState.DownloadFinishedBySmartUpdate, addonName, 100));
            }
            else
            {
                // Download zip file

                var filePath = Path.Combine(downloadFolder, zipFile);

                await downloadHelper.DownloadFileAsync(downloadUrl, filePath, new Progress<DownloadProgress>(p =>
                {
                    var percent = CalcDownloadPercent(p.ReceivedBytes, p.TotalBytes);
                    progress?.Report(new AddonProgress(AddonState.DownloadProgress, addonName, percent));
                }),
                cancellationToken).ConfigureAwait(false);

                progress?.Report(new AddonProgress(AddonState.DownloadFinished, addonName, 100));

                smartUpdateFeature.AddOrUpdateAddon(addonName, downloadUrl, zipFile);
            }

            // Extract zip file

            cancellationToken.ThrowIfCancellationRequested();

            var zipFilePath = Path.Combine(downloadFolder, zipFile);

            var valid = await unzipHelper.ValidateZipFileAsync(zipFilePath, cancellationToken).ConfigureAwait(false);
            if (!valid)
            {
                throw new InvalidOperationException($"It seems the addon zip file ('{zipFile}') is corrupted, cause zip file validation failed.");
            }

            await unzipHelper.ExtractZipFileAsync(zipFilePath, unzipFolder, cancellationToken).ConfigureAwait(false);

            progress?.Report(new AddonProgress(AddonState.UnzipFinished, addonName, 100));
        }

        private static byte CalcDownloadPercent(uint bytesReceived, uint bytesTotal)
        {
            // Doing casts inside try/catch block (just to be sure)

            try
            {
                var exact = (double)bytesReceived / bytesTotal;
                var exactPercent = exact * 100;
                var roundedPercent = (byte)Math.Round(exactPercent);
                var cappedPercent = roundedPercent > 100 ? (byte)100 : roundedPercent; // Cap it (just to be sure)

                return cappedPercent;
            }
            catch
            {
                return 0;
            }
        }
    }
}
