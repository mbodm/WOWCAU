namespace WOWCAU.Core.Parts.Modules.Config.Types
{
    public sealed record ConfigData(
        string ActiveProfile,
        IEnumerable<string> ActiveOptions,
        string TargetFolder,
        IEnumerable<string> AddonUrls)
    {
        public static ConfigData Empty() => new(
            string.Empty,
            [],
            string.Empty,
            []);
    }
}
