namespace WOWCAU.Core.Parts.Modules.Types
{
    public sealed record SettingsData(
        string WorkFolder,
        string TempFolder,
        IEnumerable<string> Options,
        IEnumerable<string> AddonUrls,
        string AddonTargetFolder)
    {
        public static SettingsData Empty()
        {
            return new(
                string.Empty,
                string.Empty,
                [],
                [],
                string.Empty);
        }
    }
}
