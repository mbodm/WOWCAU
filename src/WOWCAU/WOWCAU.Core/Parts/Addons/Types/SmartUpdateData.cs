namespace WOWCAU.Core.Parts.Addons.Types
{
    public sealed record SmartUpdateData(
        string AddonName,
        string DownloadUrl,
        string ZipFile,
        string TimeStamp);
}
