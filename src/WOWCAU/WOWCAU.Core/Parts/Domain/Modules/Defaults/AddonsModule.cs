using WOWCAU.Core.Parts.Addons.Contracts;
using WOWCAU.Core.Parts.Domain.Modules.Contracts;
using WOWCAU.Core.Parts.Logging.Contracts;
using WOWCAU.Core.Parts.System.Contracts;
using WOWCAU.Helper.Parts.Contracts;

namespace WOWCAU.Core.Parts.Domain.Modules.Defaults
{
    public sealed class AddonsModule(
        ILogger logger,
        IAppModule appModule,
        ISmartUpdateFeature smartUpdateFeature,
        IMultiAddonProcessor multiAddonProcessor,
        IFileSystemHelper fileSystemHelper,
        IReliableFileOperations reliableFileOperations) : IAddonsModule
    {
        private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAppModule appModule = appModule ?? throw new ArgumentNullException(nameof(appModule));
        private readonly ISmartUpdateFeature smartUpdateFeature = smartUpdateFeature ?? throw new ArgumentNullException(nameof(smartUpdateFeature));
        private readonly IMultiAddonProcessor multiAddonProcessor = multiAddonProcessor ?? throw new ArgumentNullException(nameof(multiAddonProcessor));
        private readonly IFileSystemHelper fileSystemHelper = fileSystemHelper ?? throw new ArgumentNullException(nameof(fileSystemHelper));
        private readonly IReliableFileOperations reliableFileOperations = reliableFileOperations ?? throw new ArgumentNullException(nameof(reliableFileOperations));

        public async Task<uint> ProcessAddonsAsync(IProgress<byte>? progress = null, CancellationToken cancellationToken = default)
        {
            // Prepare folders

            var smartUpdateFolder = Path.Combine(appModule.Settings.WorkFolder, "SmartUpdate");
            var downloadFolder = Path.Combine(appModule.Settings.TempFolder, "Curse-Download");
            var unzipFolder = Path.Combine(appModule.Settings.TempFolder, "Curse-Unzip");
            var targetFolder = appModule.Settings.AddonTargetFolder;

            await PrepareFoldersAsync(downloadFolder, unzipFolder, targetFolder, cancellationToken).ConfigureAwait(false);

            // Prepare SmartUpdate

            await smartUpdateFeature.InitAsync(smartUpdateFolder, downloadFolder, cancellationToken).ConfigureAwait(false);

            // Process addons

            await SmartUpdateLoadAsync(cancellationToken).ConfigureAwait(false);

            uint countOfUpdatedAddons;
            try
            {
                countOfUpdatedAddons = await multiAddonProcessor.ProcessAddonsAsync(appModule.Settings.AddonUrls, downloadFolder, unzipFolder,
                    progress, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                if (IsCancellationException(e)) throw;
                throw new InvalidOperationException("An error occurred while processing the addons (see log file for details).");
            }

            await SmartUpdateSaveAsync(cancellationToken).ConfigureAwait(false);

            //await MoveContentAsync(unzipFolder, targetFolder, cancellationToken).ConfigureAwait(false);
            await CopyContentAsync(unzipFolder, targetFolder, cancellationToken).ConfigureAwait(false);
            await CleanUpAsync(downloadFolder, unzipFolder, cancellationToken).ConfigureAwait(false);

            return countOfUpdatedAddons;
        }

        private async Task PrepareFoldersAsync(string downloadFolder, string unzipFolder, string targetFolder, CancellationToken cancellationToken = default)
        {
            if (Directory.Exists(downloadFolder))
            {
                await fileSystemHelper.DeleteFolderContentAsync(downloadFolder, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                Directory.CreateDirectory(downloadFolder);
            }

            if (Directory.Exists(unzipFolder))
            {
                await fileSystemHelper.DeleteFolderContentAsync(unzipFolder, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                Directory.CreateDirectory(unzipFolder);
            }

            await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);

            // Just to be sure (even when target folder was already handled by config validation)

            if (!Directory.Exists(targetFolder))
            {
                throw new InvalidOperationException("Configured target folder not exists.");
            }
        }

        private async Task SmartUpdateLoadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await smartUpdateFeature.LoadAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                if (IsCancellationException(e)) throw;
                throw new InvalidOperationException("An error occurred while loading SmartUpdate data (see log file for details).");
            }
        }

        private async Task SmartUpdateSaveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await smartUpdateFeature.SaveAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                if (IsCancellationException(e)) throw;
                throw new InvalidOperationException("An error occurred while saving SmartUpdate data (see log file for details).");
            }
        }

        private async Task MoveContentAsync(string unzipFolder, string targetFolder, CancellationToken cancellationToken = default)
        {
            await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);

            // Clear the target folder

            try
            {
                await fileSystemHelper.DeleteFolderContentAsync(targetFolder, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                if (IsCancellationException(e)) throw;
                throw new InvalidOperationException("An error occurred while deleting the content of target folder (see log file for details).");
            }

            await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);

            // Move to target folder

            try
            {
                await fileSystemHelper.MoveFolderContentAsync(unzipFolder, targetFolder, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                if (IsCancellationException(e)) throw;
                throw new InvalidOperationException("An error occurred while moving the unzipped addons to target folder (see log file for details).");
            }
        }

        private async Task CopyContentAsync(string unzipFolder, string targetFolder, CancellationToken cancellationToken = default)
        {
            await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);

            // Copy everything in unzip folder to target folder

            try
            {
                await fileSystemHelper.CopyFolderContentAsync(unzipFolder, targetFolder, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                if (IsCancellationException(e)) throw;
                throw new InvalidOperationException("An error occurred while copying the unzipped addons to target folder (see log file for details).");
            }
        }

        private async Task CleanUpAsync(string downloadFolder, string unzipFolder, CancellationToken cancellationToken = default)
        {
            await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);

            // Clean up temporary folders

            try
            {
                await fileSystemHelper.DeleteFolderContentAsync(downloadFolder, cancellationToken).ConfigureAwait(false);
                await fileSystemHelper.DeleteFolderContentAsync(unzipFolder, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                if (IsCancellationException(e)) throw;
                throw new InvalidOperationException("An error occurred while deleting the content of temporary folders (see log file for details).");
            }
        }

        private static bool IsCancellationException(Exception e) => e is TaskCanceledException || e is OperationCanceledException;
    }
}
