using System.Diagnostics;
using WOWCAU.Core.Parts.Domain.System.Contracts;
using WOWCAU.Core.Parts.Domain.Update.Contracts;
using WOWCAU.Core.Parts.Domain.Update.Types;
using WOWCAU.Core.Parts.Helper.Contracts;
using WOWCAU.Core.Parts.Helper.Types;

namespace WOWCAU.Core.Parts.Domain.Update.Defaults
{
    public sealed class UpdateManager(
        IReliableFileOperations reliableFileOperations,
        IGitHubHelper gitHubHelper, IFileSystemHelper fileSystemHelper, IDownloadHelper downloadHelper, IUnzipHelper unzipHelper, IAppHelper appHelper) : IUpdateManager
    {
        private readonly IReliableFileOperations reliableFileOperations = reliableFileOperations ?? throw new ArgumentNullException(nameof(reliableFileOperations));
        private readonly IGitHubHelper gitHubHelper = gitHubHelper ?? throw new ArgumentNullException(nameof(gitHubHelper));
        private readonly IFileSystemHelper fileSystemHelper = fileSystemHelper ?? throw new ArgumentNullException(nameof(fileSystemHelper));
        private readonly IDownloadHelper downloadHelper = downloadHelper ?? throw new ArgumentNullException(nameof(downloadHelper));
        private readonly IUnzipHelper unzipHelper = unzipHelper ?? throw new ArgumentNullException(nameof(unzipHelper));
        private readonly IAppHelper appHelper = appHelper ?? throw new ArgumentNullException(nameof(appHelper));

        private string updateFolder = string.Empty;

        public async Task InitAsync(string pathToApplicationTempFolder, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pathToApplicationTempFolder))
            {
                throw new ArgumentException($"'{nameof(pathToApplicationTempFolder)}' cannot be null or whitespace.", nameof(pathToApplicationTempFolder));
            }

            if (!Directory.Exists(pathToApplicationTempFolder))
            {
                throw new InvalidOperationException("Given application temp folder not exists.");
            }

            updateFolder = Path.Combine(Path.GetFullPath(pathToApplicationTempFolder), "App-Update");

            if (!Directory.Exists(updateFolder))
            {
                Directory.CreateDirectory(updateFolder);
                await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<UpdateData> CheckForUpdateAsync(CancellationToken cancellationToken = default)
        {
            var installedVersion = GetInstalledVersion();
            var latestReleaseData = await gitHubHelper.GetLatestReleaseDataAsync("mbodm", "wowcau", cancellationToken).ConfigureAwait(false);
            var updateAvailable = installedVersion < latestReleaseData.Version;

            return new UpdateData(installedVersion, latestReleaseData.Version, updateAvailable, latestReleaseData.DownloadUrl, latestReleaseData.FileName);
        }

        public async Task DownloadUpdateAsync(UpdateData updateData, IProgress<DownloadProgress>? downloadProgress = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(updateData);

            CheckInitialization();

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

        public async Task ApplyUpdateAsync(CancellationToken cancellationToken = default)
        {
            CheckInitialization();

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

        public void RestartApplication(uint delayInSeconds)
        {
            if (delayInSeconds > 10)
            {
                delayInSeconds = 10;
            }

            // To decouple our .exe call from the cmd.exe process, we also need to use "start" here.
            // Since we could have spaces in our .exe path, the path has to be surrounded by quotes.
            // Doing this properly, together with "start", its fist argument has to be empty quotes.
            // See here -> https://stackoverflow.com/questions/2937569/how-to-start-an-application-without-waiting-in-a-batch-file

            var psi = new ProcessStartInfo
            {
                Arguments = $"/C ping 127.0.0.1 -n {delayInSeconds} && start \"\" \"{appHelper.GetApplicationExecutableFilePath()}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };

            var process = Process.Start(psi) ?? throw new InvalidOperationException("The 'Process.Start()' call returned null.");
        }

        public async Task RemoveBakFileIfExistsAsync(CancellationToken cancellationToken = default)
        {
            var exeFilePath = appHelper.GetApplicationExecutableFilePath();
            var bakFilePath = Path.ChangeExtension(exeFilePath, ".bak");

            if (File.Exists(bakFilePath))
            {
                File.Delete(bakFilePath);
                await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private void CheckInitialization()
        {
            if (updateFolder == string.Empty)
            {
                throw new InvalidOperationException("UpdateManager is not initialized (please initialize first, by calling the appropriate method.");
            }
        }

        private Version GetInstalledVersion()
        {
            var installedExeFile = appHelper.GetApplicationExecutableFilePath();
            var installedVersion = fileSystemHelper.GetExeFileVersion(installedExeFile);

            return installedVersion;
        }
    }
}
