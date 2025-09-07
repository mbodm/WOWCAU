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
            var downloadFolder = Path.Combine(appModule.Settings.WorkFolder, "CurseDownload");
            var targetFolder = appModule.Settings.AddonTargetFolder;

            if (!Directory.Exists(smartUpdateFolder))
            {
                Directory.CreateDirectory(smartUpdateFolder);
            }

            if (!Directory.Exists(downloadFolder))
            {
                Directory.CreateDirectory(downloadFolder);
            }
            else
            {
                await fileSystemHelper.DeleteFolderContentAsync(downloadFolder, cancellationToken).ConfigureAwait(false);
            }

            if (!Directory.Exists(targetFolder))
            {
                // Just to be sure (even when target folder was already handled by config validation)

                throw new InvalidOperationException("Configured target folder not exists.");
            }

            await reliableFileOperations.WaitAsync(cancellationToken).ConfigureAwait(false);

            // Load SmartUpdate

            try
            {
                await smartUpdateFeature.LoadAsync(smartUpdateFolder, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                if (IsCancellationException(e)) throw;
                throw new InvalidOperationException("An error occurred while loading SmartUpdate data (see log file for details).");
            }

            // Process addons

            uint countOfUpdatedAddons;
            try
            {
                countOfUpdatedAddons = await multiAddonProcessor.ProcessAddonsAsync(appModule.Settings.AddonUrls, downloadFolder, appModule.Settings.AddonTargetFolder,
                    progress, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                if (IsCancellationException(e)) throw;
                throw new InvalidOperationException("An error occurred while processing the addons (see log file for details).");
            }

            // Save SmartUpdate

            try
            {
                await smartUpdateFeature.SaveAsync(smartUpdateFolder, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                if (IsCancellationException(e)) throw;
                throw new InvalidOperationException("An error occurred while saving SmartUpdate data (see log file for details).");
            }

            return countOfUpdatedAddons;
        }

        private static bool IsCancellationException(Exception e) => e is TaskCanceledException || e is OperationCanceledException;
    }
}
