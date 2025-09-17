using WOWCAU.Core.Parts.Helper.Contracts;
using WOWCAU.Core.Parts.Helper.Types;
using WOWCAU.Core.Parts.Modules.System.Contracts;
using WOWCAU.Core.Parts.Modules.Update.Contracts;
using WOWCAU.Core.Parts.Modules.Update.Types;

namespace WOWCAU.Core.Parts.Modules.Update.Defaults
{
    public sealed class UpdateManager(
        IReliableFileOperations reliableFileOperations,
        IGitHubHelper gitHubHelper,
        IFileSystemHelper fileSystemHelper,
        IDownloadHelper downloadHelper,
        IUnzipHelper unzipHelper,
        IAppHelper appHelper) : IUpdateManager
    {
        private readonly IReliableFileOperations reliableFileOperations = reliableFileOperations ?? throw new ArgumentNullException(nameof(reliableFileOperations));
        private readonly IGitHubHelper gitHubHelper = gitHubHelper ?? throw new ArgumentNullException(nameof(gitHubHelper));
        private readonly IFileSystemHelper fileSystemHelper = fileSystemHelper ?? throw new ArgumentNullException(nameof(fileSystemHelper));
        private readonly IDownloadHelper downloadHelper = downloadHelper ?? throw new ArgumentNullException(nameof(downloadHelper));
        private readonly IUnzipHelper unzipHelper = unzipHelper ?? throw new ArgumentNullException(nameof(unzipHelper));
        private readonly IAppHelper appHelper = appHelper ?? throw new ArgumentNullException(nameof(appHelper));

        public async Task<UpdateData> CheckForUpdateAsync(CancellationToken cancellationToken = default)
        {
            var installedVersion = GetInstalledVersion();
            var latestReleaseData = await gitHubHelper.GetLatestReleaseDataAsync("mbodm", "wowcau", cancellationToken).ConfigureAwait(false);
            var updateAvailable = installedVersion < latestReleaseData.Version;

            return new UpdateData(installedVersion, latestReleaseData.Version, updateAvailable, latestReleaseData.DownloadUrl, latestReleaseData.FileName);
        }

        public async Task DownloadUpdateAsync(UpdateData updateData, string updateFolder,
            IProgress<DownloadProgress>? downloadProgress = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(updateData);
            ArgumentException.ThrowIfNullOrWhiteSpace(updateFolder);

            if (!Directory.Exists(updateFolder))
            {
                Directory.CreateDirectory(updateFolder);
            }
            else
            {
                await fileSystemHelper.DeleteFolderContentAsync(updateFolder, cancellationToken).ConfigureAwait(false);
            }

            await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);

            var zipFilePath = Path.Combine(updateFolder, updateData.UpdateFileName);
            await downloadHelper.DownloadFileAsync(updateData.UpdateDownloadUrl, zipFilePath, downloadProgress, cancellationToken).ConfigureAwait(false);
            if (!File.Exists(zipFilePath))
            {
                throw new InvalidOperationException("Downloaded latest release, but update folder not contains zip file.");
            }

            await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);

            await unzipHelper.ExtractZipFileAsync(zipFilePath, updateFolder, cancellationToken).ConfigureAwait(false);

            var appFileName = appHelper.GetApplicationExecutableFileName();
            var newExeFilePath = Path.Combine(updateFolder, appFileName);
            if (!File.Exists(newExeFilePath))
            {
                throw new InvalidOperationException($"Extracted zip file, but update folder not contains {appFileName} file.");
            }
        }

        public async Task ApplyUpdateAsync(string updateFolder, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(updateFolder);

            if (!Directory.Exists(updateFolder))
            {
                throw new InvalidOperationException("Update folder not exists.");
            }

            var appFileName = appHelper.GetApplicationExecutableFileName();
            var newExeFilePath = Path.Combine(updateFolder, appFileName);
            if (!File.Exists(newExeFilePath))
            {
                throw new InvalidOperationException($"Update folder not contains {appFileName} file.");
            }

            var newExeVersion = fileSystemHelper.GetExeFileVersion(newExeFilePath);
            var installedVersion = GetInstalledVersion();
            if (newExeVersion < installedVersion)
            {
                throw new InvalidOperationException($"{appFileName} in update folder is older than existing {appFileName} in application folder.");
            }

            var exeFilePath = appHelper.GetApplicationExecutableFilePath();
            var bakFilePath = Path.ChangeExtension(exeFilePath, ".bak");

            File.Move(exeFilePath, bakFilePath, true);
            await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);

            File.Copy(newExeFilePath, exeFilePath, true);
            await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);

            await fileSystemHelper.DeleteFolderContentAsync(updateFolder, cancellationToken).ConfigureAwait(false);
            await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        private Version GetInstalledVersion()
        {
            var installedExeFile = appHelper.GetApplicationExecutableFilePath();
            var installedVersion = fileSystemHelper.GetExeFileVersion(installedExeFile);

            return installedVersion;
        }
    }
}
