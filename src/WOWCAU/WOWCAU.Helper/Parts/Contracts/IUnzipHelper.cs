namespace WOWCAU.Helper.Parts.Contracts
{
    public interface IUnzipHelper
    {
        Task<bool> ValidateZipFileAsync(string zipFile, CancellationToken cancellationToken = default);
        Task ExtractZipFileAsync(string zipFile, string destFolder, CancellationToken cancellationToken = default);
    }
}
