namespace WOWCAU.Helper.Parts.Contracts
{
    public interface IAppHelper
    {
        string GetApplicationName();
        string GetApplicationVersion();
        string GetApplicationExecutableFolder();
        string GetApplicationExecutableFileName();
        string GetApplicationExecutableFilePath();
    }
}
