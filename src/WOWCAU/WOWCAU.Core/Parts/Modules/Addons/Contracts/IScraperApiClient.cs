namespace WOWCAU.Core.Parts.Modules.Addons.Contracts
{
    public interface IScraperApiClient
    {
        Task<bool> HasDownloadUrlsOnWebScraperApiAsync(IEnumerable<string> addonNames, CancellationToken cancellationToken = default);
        Task<Dictionary<string, string>> GetDownloadUrlsFromWebScraperApiAsync(IEnumerable<string> addonNames, CancellationToken cancellationToken = default);
        Task AddAddonsToWebScrapeApiAsync(IEnumerable<string> addonNames, CancellationToken cancellationToken = default);
        Task ScrapeAddonsWithWebScrapeApiAsync(IEnumerable<string> addonNames, CancellationToken cancellationToken = default);
    }
}
