using WOWCAU.Helper.Parts.Types;

namespace WOWCAU.Helper.Parts.Contracts
{
    public interface IGitHubHelper
    {
        Task<GitHubReleaseData> GetLatestReleaseDataAsync(string user, string repo, CancellationToken cancellationToken = default);
    }
}
