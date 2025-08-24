namespace WOWCAU.Core.Parts.Update.Types
{
    public sealed record UpdateData(
        Version InstalledVersion,
        Version AvailableVersion,
        bool UpdateAvailable,
        string UpdateDownloadUrl,
        string UpdateFileName);
}
