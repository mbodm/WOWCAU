namespace WOWCAU.Helper.Parts.Contracts
{
    public interface ICurseHelper
    {
        bool IsAddonPageUrl(string url);
        bool IsDownloadUrl(string url);
        string GetAddonSlugNameFromAddonPageUrl(string url);
        string GetZipFileNameFromDownloadUrl(string url);
    }
}
