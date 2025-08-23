namespace WOWCAU.Core.Parts.Helper.Types
{
    public sealed record DownloadProgress(
        string Url,
        bool PreTransfer,
        uint ReceivedBytes,
        uint TotalBytes,
        bool TransferFinished);
}
