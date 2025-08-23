namespace WOWCAU.Core.Parts.Domain.Addons.Types
{
    public sealed record AddonProgress(
        AddonState AddonState,
        string AddonName,
        byte DownloadPercent);
}
