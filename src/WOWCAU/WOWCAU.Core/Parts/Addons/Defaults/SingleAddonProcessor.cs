using System.Diagnostics;
using WOWCAU.Core.Parts.Addons.Contracts;
using WOWCAU.Core.Parts.Addons.Types;
using WOWCAU.Core.Parts.Logging.Contracts;
using WOWCAU.Helper.Parts.Contracts;
using WOWCAU.Helper.Parts.Types;

namespace WOWCAU.Core.Parts.Addons.Defaults
{
    public sealed class SingleAddonProcessor(
        ILogger logger, ICurseHelper curseHelper, ISmartUpdateFeature smartUpdateFeature, IDownloadHelper downloadHelper, IUnzipHelper unzipHelper) : ISingleAddonProcessor
    {
        private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ICurseHelper curseHelper = curseHelper ?? throw new ArgumentNullException(nameof(curseHelper));
        private readonly ISmartUpdateFeature smartUpdateFeature = smartUpdateFeature ?? throw new ArgumentNullException(nameof(smartUpdateFeature));
        private readonly IDownloadHelper downloadHelper = downloadHelper ?? throw new ArgumentNullException(nameof(downloadHelper));
        private readonly IUnzipHelper unzipHelper = unzipHelper ?? throw new ArgumentNullException(nameof(unzipHelper));

        public async Task ProcessAddonAsync(string addonName, string downloadUrl, string downloadFolder, string unzipFolder,
            IProgress<AddonProgress>? progress = default, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(addonName);
            ArgumentException.ThrowIfNullOrWhiteSpace(downloadUrl);
            ArgumentException.ThrowIfNullOrWhiteSpace(downloadFolder);
            ArgumentException.ThrowIfNullOrWhiteSpace(unzipFolder);

            logger.LogMethodEntry();

            // Get zip file name

            var zipFile = downloadUrl.Split('/').LastOrDefault();
            if (!curseHelper.IsDownloadUrl(downloadUrl) || zipFile == null)
            {
                throw new InvalidOperationException("Given addon download URL is not a valid Curse CDN download URL.");
            }

            // Handle SmartUpdate feature

            if (smartUpdateFeature.AddonVersionAlreadyExists(addonName, downloadUrl, zipFile))
            {
                progress?.Report(new AddonProgress(AddonState.NoDownloadNeeded, addonName, 100));
            }
            else
            {
                // Download zip file

                var filePath = Path.Combine(downloadFolder, zipFile);

                logger.Log($"Start downloading zip file ({zipFile}).");
                var swDownload = Stopwatch.StartNew();

                await downloadHelper.DownloadFileAsync(downloadUrl, filePath, new Progress<DownloadProgress>(p =>
                {
                    var percent = CalcDownloadPercent(p.ReceivedBytes, p.TotalBytes);
                    progress?.Report(new AddonProgress(AddonState.DownloadProgress, addonName, percent));
                }),
                cancellationToken).ConfigureAwait(false);

                swDownload.Stop();
                logger.Log($"Finished downloading zip file ({zipFile}), after {swDownload.ElapsedMilliseconds} ms.");

                progress?.Report(new AddonProgress(AddonState.DownloadFinished, addonName, 100));

                // Extract zip file

                cancellationToken.ThrowIfCancellationRequested();

                var zipFilePath = Path.Combine(downloadFolder, zipFile);

                var valid = await unzipHelper.ValidateZipFileAsync(zipFilePath, cancellationToken).ConfigureAwait(false);
                if (!valid)
                {
                    throw new InvalidOperationException($"It seems the addon zip file ('{zipFile}') is corrupted, cause zip file validation failed.");
                }

                logger.Log($"Start extracting zip file ({zipFile}).");
                var swExtract = Stopwatch.StartNew();

                await unzipHelper.ExtractZipFileAsync(zipFilePath, unzipFolder, cancellationToken).ConfigureAwait(false);

                swExtract.Stop();
                logger.Log($"Finished extracting zip file ({zipFile}), after {swExtract.ElapsedMilliseconds} ms.");

                progress?.Report(new AddonProgress(AddonState.UnzipFinished, addonName, 100));

                // Add to SmartUpdate

                smartUpdateFeature.AddOrUpdateAddonVersion(addonName, downloadUrl, zipFile);
            }

            logger.LogMethodExit();
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
