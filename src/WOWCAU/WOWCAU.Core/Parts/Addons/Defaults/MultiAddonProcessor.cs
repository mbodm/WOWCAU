using System.Collections.Concurrent;
using System.Text.Json;
using WOWCAU.Core.Parts.Addons.Contracts;
using WOWCAU.Core.Parts.Addons.Types;
using WOWCAU.Core.Parts.Logging.Contracts;
using WOWCAU.Helper.Parts.Contracts;

namespace WOWCAU.Core.Parts.Addons.Defaults
{
    public sealed class MultiAddonProcessor(ILogger logger, ICurseHelper curseHelper, ISingleAddonProcessor singleAddonProcessor, HttpClient httpClient) : IMultiAddonProcessor
    {
        private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ICurseHelper curseHelper = curseHelper ?? throw new ArgumentNullException(nameof(curseHelper));
        private readonly ISingleAddonProcessor singleAddonProcessor = singleAddonProcessor ?? throw new ArgumentNullException(nameof(singleAddonProcessor));
        private readonly HttpClient httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

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

            var downloadUrlsDict = await GetDownloadUrlsFromWebScraperAsync(cancellationToken).ConfigureAwait(false);
            var foundAll = addonNames.All(downloadUrlsDict.ContainsKey);
            if (!foundAll)
            {
                throw new InvalidOperationException("Received valid response from Deno WOWCAM scraper API, but response not contained all requested addons.");
            }

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

            await Task.WhenAll(tasks);

            logger.LogMethodExit();

            return updatedAddonsCounter;
        }

        private async Task<Dictionary<string, string>> GetDownloadUrlsFromWebScraperAsync(CancellationToken cancellationToken = default)
        {
            var url = "https://mbodm-wowcam.deno.dev/get?token=a983a17f-17f0-4652-bcaf-5f5c29cd99e9";
            using var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var prettyStatusCode = $"HTTP {(int)response.StatusCode} ({response.StatusCode})";
                throw new InvalidOperationException($"Received {prettyStatusCode} response error from Deno WOWCAU scraper API.");
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            if (contentType.ToLower() != "application/json")
            {
                throw new InvalidOperationException("Received invalid response content type from Deno WOWCAU scraper API.");
            }

            var contentLength = response.Content.Headers.ContentLength ?? 0;
            if (contentLength <= 0)
            {
                throw new InvalidOperationException("Received empty response content from Deno WOWCAU scraper API.");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var dict = new Dictionary<string, string>();

                using var doc = JsonDocument.Parse(content);

                foreach (var element in doc.RootElement.GetProperty("addons").EnumerateArray())
                {
                    var addonSlug = element.GetProperty("addonSlug").GetString() ??
                        throw new InvalidOperationException("Could not get 'addonSlug' property value of JSON array element.");
                    var downloadUrl = element.GetProperty("downloadUrlFinal").GetString() ??
                        throw new InvalidOperationException("Could not get 'downloadUrlFinal' property value of JSON array element.");

                    dict.Add(addonSlug, downloadUrl);
                }

                return dict;
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Received invalid JSON response content from Deno WOWCAU scraper API.");
            }
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
