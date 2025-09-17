namespace WOWCAU.Core.Parts.Modules.Addons.Types
{
    public sealed record AddonProgress(
        AddonState AddonState,
        string AddonName,
        byte DownloadPercent);
}
