using System.Collections.Concurrent;
using System.Diagnostics;
using WOWCAU.Core.Parts.Addons.Contracts;
using WOWCAU.Core.Parts.Addons.Types;
using WOWCAU.Core.Parts.Logging.Contracts;
using WOWCAU.Helper.Parts.Contracts;

namespace WOWCAU.Core.Parts.Addons.Defaults
{
    public sealed class MultiAddonProcessor(
        ILogger logger, ICurseHelper curseHelper, IScraperApiClient scraperApiClient, ISingleAddonProcessor singleAddonProcessor) : IMultiAddonProcessor
    {
        private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ICurseHelper curseHelper = curseHelper ?? throw new ArgumentNullException(nameof(curseHelper));
        private readonly IScraperApiClient scraperApiClient = scraperApiClient ?? throw new ArgumentNullException(nameof(scraperApiClient));
        private readonly ISingleAddonProcessor singleAddonProcessor = singleAddonProcessor ?? throw new ArgumentNullException(nameof(singleAddonProcessor));

        private readonly ConcurrentDictionary<string, uint> progressData = new();
        private readonly ConcurrentDictionary<string, string> downloadData = new();

        public async Task<uint> ProcessAddonsAsync(IEnumerable<string> addonUrls, string downloadFolder, string unzipFolder,
            IProgress<byte>? progress = default, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(addonUrls);
            ArgumentException.ThrowIfNullOrWhiteSpace(downloadFolder);
            ArgumentException.ThrowIfNullOrWhiteSpace(unzipFolder);

            logger.LogMethodEntry();

            var addonNames = addonUrls.Select(curseHelper.GetAddonSlugNameFromAddonPageUrl);

            // Get download URLs

            var hasAll = await scraperApiClient.HasDownloadUrlsOnWebScraperApiAsync(addonNames, cancellationToken).ConfigureAwait(false);
            if (!hasAll)
            {
                await scraperApiClient.AddAddonsToWebScrapeApiAsync(addonNames, cancellationToken).ConfigureAwait(false);
                await scraperApiClient.ScrapeAddonsWithWebScrapeApiAsync(addonNames, cancellationToken).ConfigureAwait(false);
            }

            var downloadUrlsDict = await scraperApiClient.GetDownloadUrlsFromWebScraperApiAsync(addonNames, cancellationToken).ConfigureAwait(false);

            // Prepare concurrent dictionaries

            progressData.Clear();
            downloadData.Clear();
            foreach (var addonName in addonNames)
            {
                progressData.TryAdd(addonName, 0);
                var downloadUrl = downloadUrlsDict[addonName];
                downloadData.TryAdd(addonName, downloadUrl);
            }

            // Concurrently do for every addon "download -> unzip" (download part may be skipped/faked internally by SmartUpdate)

            uint updatedAddonsCounter = 0;

            var tasks = downloadData.Select(kvp =>
            {
                var addonName = kvp.Key;
                var downloadUrl = kvp.Value;
                var addonProgress = new Progress<AddonProgress>(p =>
                {
                    switch (p.AddonState)
                    {
                        case AddonState.DownloadProgress:
                            progressData[p.AddonName] = p.DownloadPercent;
                            break;
                        case AddonState.DownloadFinished:
                            progressData[p.AddonName] = 100; // Just to make sure download is 100%
                            Interlocked.Increment(ref updatedAddonsCounter);
                            break;
                        case AddonState.DownloadFinishedBySmartUpdate:
                            progressData[p.AddonName] = 100;
                            break;
                        case AddonState.UnzipFinished:
                            progressData[p.AddonName] = 200;
                            break;
                    }

                    progress?.Report(CalcTotalPercent());
                });

                return singleAddonProcessor.ProcessAddonAsync(addonName, downloadUrl, downloadFolder, unzipFolder, addonProgress, cancellationToken);
            });

            logger.Log("Start parallel processing of all addons.");
            var sw = Stopwatch.StartNew();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            sw.Stop();
            logger.Log($"Finished parallel processing of all addons,  after {sw.ElapsedMilliseconds} ms.");

            logger.LogMethodExit();

            return updatedAddonsCounter;
        }

        private byte CalcTotalPercent()
        {
            // Doing casts inside try/catch block (just to be sure)

            try
            {
                var sumOfAllAddons = (ulong)progressData.Sum(kvp => kvp.Value);
                var hundredPercent = (ulong)progressData.Count * 200;

                var exact = (double)sumOfAllAddons / hundredPercent;
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
