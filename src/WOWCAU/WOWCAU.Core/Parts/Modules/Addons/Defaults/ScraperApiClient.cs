using System.Diagnostics;
using System.Text.Json;
using WOWCAU.Core.Parts.Modules.Addons.Contracts;
using WOWCAU.Core.Parts.Modules.System.Contracts;

namespace WOWCAU.Core.Parts.Modules.Addons.Defaults
{
    public sealed class ScraperApiClient(ILogger logger, HttpClient httpClient) : IScraperApiClient
    {
        private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly HttpClient httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<bool> HasDownloadUrlsOnWebScraperApiAsync(IEnumerable<string> addonNames, CancellationToken cancellationToken = default)
        {
            // Check if the API already has all download URLs (for given addon names)

            logger.LogMethodEntry();

            var allDownloadUrlsDict = await GetAllDownloadUrlsFromWebScraperApiAsync(cancellationToken).ConfigureAwait(false);
            var hasAllSearchedNames = addonNames.All(allDownloadUrlsDict.ContainsKey);

            return hasAllSearchedNames;
        }

        public async Task<Dictionary<string, string>> GetDownloadUrlsFromWebScraperApiAsync(IEnumerable<string> addonNames, CancellationToken cancellationToken = default)
        {
            // Get all download URLs from API (for given addon names)

            logger.LogMethodEntry();

            var allDownloadUrlsDict = await GetAllDownloadUrlsFromWebScraperApiAsync(cancellationToken).ConfigureAwait(false);
            var searchedEntriesOnly = allDownloadUrlsDict.Where(kvp => addonNames.Contains(kvp.Key));

            return searchedEntriesOnly.ToDictionary();
        }

        public Task AddAddonsToWebScrapeApiAsync(IEnumerable<string> addonNames, CancellationToken cancellationToken = default)
        {
            // Add the given addons to API

            logger.LogMethodEntry();

            var addonsQueryParam = string.Join(',', addonNames);
            var url = $"https://mbodm-wowcam.deno.dev/add?token=a983a17f-17f0-4652-bcaf-5f5c29cd99e9&addons={addonsQueryParam}";

            logger.Log($"Start Deno web scraper API request, to add addons.");

            return SendAddOrScrapeRequestToWebScraperApiAsync(url, addonNames, cancellationToken);
        }

        public Task ScrapeAddonsWithWebScrapeApiAsync(IEnumerable<string> addonNames, CancellationToken cancellationToken = default)
        {
            // Manually trigger the API to scrape all addons immediately

            logger.LogMethodEntry();

            var url = $"https://mbodm-wowcam.deno.dev/scrape?token=a983a17f-17f0-4652-bcaf-5f5c29cd99e9";

            logger.Log($"Start Deno web scraper API request, to scrape all addons.");

            return SendAddOrScrapeRequestToWebScraperApiAsync(url, addonNames, cancellationToken);
        }

        private async Task<Dictionary<string, string>> GetAllDownloadUrlsFromWebScraperApiAsync(CancellationToken cancellationToken = default)
        {
            logger.Log($"Start Deno web scraper API request, to get all download urls.");
            var sw = Stopwatch.StartNew();

            var url = "https://mbodm-wowcam.deno.dev/get?token=a983a17f-17f0-4652-bcaf-5f5c29cd99e9";
            using var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

            ThrowOnBadWebScraperApiResponse(response);

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            sw.Stop();
            logger.Log($"Finished Deno web scraper API request, after {sw.ElapsedMilliseconds} ms.");

            try
            {
                using var doc = JsonDocument.Parse(content);

                var dict = new Dictionary<string, string>();

                foreach (var element in doc.RootElement.GetProperty("addons").EnumerateArray())
                {
                    var addonSlug = element.GetProperty("addonSlug").GetString()?.Trim() ??
                        throw new InvalidOperationException("The 'addonSlug' property value (of JSON array element) was null.");

                    var hadScrape = element.GetProperty("hadScrape").GetBoolean();

                    if (addonSlug != string.Empty && hadScrape)
                    {
                        var downloadUrl = element.GetProperty("downloadUrlFinal").GetString()?.Trim() ??
                            throw new InvalidOperationException("The 'downloadUrlFinal' property value (of JSON array element) was null.");

                        if (downloadUrl != string.Empty)
                        {
                            dict.Add(addonSlug, downloadUrl);
                        }
                    }
                }

                return dict;
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Received invalid JSON response content from Deno web scraper API.");
            }
        }

        private async Task SendAddOrScrapeRequestToWebScraperApiAsync(string url, IEnumerable<string> addonNames, CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();

            using var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

            ThrowOnBadWebScraperApiResponse(response);

            var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            sw.Stop();

            logger.Log($"Finished Deno web scraper API request, after {sw.ElapsedMilliseconds} ms.");

            try
            {
                using var doc = JsonDocument.Parse(content);

                var addonSlugs = new List<string>();

                foreach (var element in doc.RootElement.GetProperty("addons").EnumerateArray())
                {
                    var addonSlug = element.GetProperty("addonSlug").GetString()?.Trim() ??
                        throw new InvalidOperationException("Could not get 'addonSlug' property value of JSON array element.");

                    if (addonSlug != string.Empty)
                    {
                        addonSlugs.Add(addonSlug);
                    }
                }

                var allAddonsAdded = addonSlugs.All(addonSlug => addonNames.Contains(addonSlug));
                if (!allAddonsAdded)
                {
                    throw new InvalidOperationException("Received valid response from Deno web scraper API, but response not contained all of the given addons.");
                }
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Received invalid JSON response content from Deno web scraper API.");
            }
        }

        private static void ThrowOnBadWebScraperApiResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var prettyStatusCode = $"HTTP {(int)response.StatusCode} ({response.StatusCode})";
                throw new InvalidOperationException($"Received {prettyStatusCode} response error from Deno web scraper API.");
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
            if (contentType.ToLower() != "application/json")
            {
                throw new InvalidOperationException("Received invalid response content type from Deno web scraper API.");
            }

            var contentLength = response.Content.Headers.ContentLength ?? 0;
            if (contentLength <= 0)
            {
                throw new InvalidOperationException("Received empty response content from Deno web scraper API.");
            }
        }
    }
}
