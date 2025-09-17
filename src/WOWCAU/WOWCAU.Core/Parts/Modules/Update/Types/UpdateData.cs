namespace WOWCAU.Core.Parts.Modules.Update.Types
{
    public sealed record UpdateData(
        Version InstalledVersion,
        Version AvailableVersion,
        bool UpdateAvailable,
        string UpdateDownloadUrl,
        string UpdateFileName);
}
