using WOWCAU.Core.Parts.Helper.Types;

namespace WOWCAU.Core.Parts.Helper.Contracts
{
    public interface IGitHubHelper
    {
        Task<GitHubReleaseData> GetLatestReleaseDataAsync(string user, string repo, CancellationToken cancellationToken = default);
    }
}
