namespace WOWCAU.Core.Parts.Domain.Addons.Types
{
    public sealed record SmartUpdateData(
        string AddonName,
        string DownloadUrl,
        string ZipFile,
        string TimeStamp);
}
