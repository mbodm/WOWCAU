namespace WOWCAU.Core.Parts.Domain.Types
{
    public sealed record SettingsData(
        string ApplicationFolder,
        IEnumerable<string> Options,
        IEnumerable<string> AddonUrls,
        string AddonTargetFolder)
    {
        public static SettingsData Empty()
        {
            return new(
                string.Empty,
                [],
                [],
                string.Empty);
        }
    }
}
