namespace WOWCAU.Core.Parts.Modules.Addons.Types
{
    public sealed record SmartUpdateData(
        string AddonName,
        string DownloadUrl,
        string ZipFile,
        string TimeStamp);
}
