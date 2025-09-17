namespace WOWCAU.Core.Parts.Helper.Contracts
{
    public interface IFileSystemHelper
    {
        bool IsValidAbsolutePath(string path);
        Task DeleteFolderContentAsync(string folder, CancellationToken cancellationToken = default);
        Version GetExeFileVersion(string exeFilePath);
    }
}
