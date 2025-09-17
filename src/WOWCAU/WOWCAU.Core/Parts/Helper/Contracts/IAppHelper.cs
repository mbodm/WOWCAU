namespace WOWCAU.Core.Parts.Helper.Contracts
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
