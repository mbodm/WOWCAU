using WOWCAU.Core.Parts.Helper.Contracts;

namespace WOWCAU.Core.Parts.Helper.Defaults
{
    public sealed class CurseHelper : ICurseHelper
    {
        public bool IsAddonPageUrl(string url)
        {
            // Example -> https://www.curseforge.com/wow/addons/deadly-boss-mods
            url = Normalize(url);
            return url.StartsWith("https://www.curseforge.com/wow/addons/") && !url.EndsWith("/addons");
        }

        public bool IsDownloadUrl(string url)
        {
            // Example -> https://mediafilez.forgecdn.net/files/4485/146/DBM-10.0.35.zip
            url = Normalize(url);
            return url.StartsWith("https://mediafilez.forgecdn.net/files/") && url.EndsWith(".zip");
        }

        public string GetAddonSlugNameFromAddonPageUrl(string url)
        {
            // Example -> https://www.curseforge.com/wow/addons/deadly-boss-mods
            url = Normalize(url);
            return IsAddonPageUrl(url) ? url.Split("https://www.curseforge.com/wow/addons/").Last().ToLower() : string.Empty;
        }

        public string GetZipFileNameFromDownloadUrl(string url)
        {
            // Example -> https://mediafilez.forgecdn.net/files/4485/146/DBM-10.0.35.zip
            url = Normalize(url);
            return IsDownloadUrl(url) ? url.Split('/').Last().ToLower() : string.Empty;
        }

        private static string Normalize(string url) => url?.Trim().TrimEnd('/') ?? string.Empty;
    }
}
