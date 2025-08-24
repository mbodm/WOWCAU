namespace WOWCAU.Helper.Parts.Types
{
    public sealed record GitHubReleaseData(
        Version Version,
        string DownloadUrl,
        string FileName);
}
