namespace WOWCAU.Core.Parts.Addons.Types
{
    public sealed record AddonProgress(
        AddonState AddonState,
        string AddonName,
        byte DownloadPercent);
}
