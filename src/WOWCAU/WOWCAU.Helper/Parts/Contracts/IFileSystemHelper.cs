namespace WOWCAU.Helper.Parts.Contracts
{
    public interface IFileSystemHelper
    {
        bool IsValidAbsolutePath(string path);
        bool CopyFile(string sourceFilePath, string destFilePath);
        Task MoveFolderContentAsync(string sourceFolder, string destFolder, CancellationToken cancellationToken = default);
        Task DeleteFolderContentAsync(string folder, CancellationToken cancellationToken = default);
        Version GetExeFileVersion(string exeFilePath);
    }
}
