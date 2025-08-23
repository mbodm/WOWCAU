namespace WOWCAU.Core.Parts.Helper.Types
{
    public sealed record GitHubReleaseData(
        Version Version,
        string DownloadUrl,
        string FileName);
}
