using System.Diagnostics;
using WOWCAU.Core.Parts.Domain.Contracts;
using WOWCAU.Core.Parts.Domain.Types;
using WOWCAU.Core.Parts.Helper.Contracts;
using WOWCAU.Core.Parts.Helper.Defaults;
using WOWCAU.Core.Parts.Helper.Types;
using WOWCAU.Core.Parts.Modules.Addons.Contracts;
using WOWCAU.Core.Parts.Modules.Addons.Defaults;
using WOWCAU.Core.Parts.Modules.Config.Contracts;
using WOWCAU.Core.Parts.Modules.Config.Defaults;
using WOWCAU.Core.Parts.Modules.Config.Types;
using WOWCAU.Core.Parts.Modules.System.Contracts;
using WOWCAU.Core.Parts.Modules.System.Defaults;
using WOWCAU.Core.Parts.Modules.Update.Contracts;
using WOWCAU.Core.Parts.Modules.Update.Defaults;
using WOWCAU.Core.Parts.Modules.Update.Types;

namespace WOWCAU.Core.Parts.Domain.Defaults
{
    public sealed class DomainLogic : IDomainLogic
    {
        // Helper
        private readonly IAppHelper appHelper;
        private readonly ICurseHelper curseHelper;
        private readonly IDownloadHelper downloadHelper;
        private readonly IFileSystemHelper fileSystemHelper;
        private readonly IGitHubHelper gitHubHelper;
        private readonly IPluralizeHelper pluralizeHelper;
        private readonly IUnzipHelper unzipHelper;

        // Modules
        private readonly ILogger logger;
        private readonly IReliableFileOperations reliableFileOperations;
        private readonly IConfigStorage configStorage;
        private readonly IConfigReader configReader;
        private readonly IConfigValidator configValidator;
        private readonly ISmartUpdateFeature smartUpdateFeature;
        private readonly IScraperApiClient scraperApiClient;
        private readonly ISingleAddonProcessor singleAddonProcessor;
        private readonly IMultiAddonProcessor multiAddonProcessor;
        private readonly IUpdateManager updateManager;
        private readonly IAppRestarter appRestarter;

        public DomainLogic(HttpClient httpClient)
        {
            // Use this constructor if you don't want to build the dependency graph on your own (when you want to use the default implementations)

            ArgumentNullException.ThrowIfNull(httpClient);

            // Increasing the HttpClient's timeout to 5min (since the "manually trigger scraping"-addons-part could take some time)

            httpClient.Timeout = TimeSpan.FromMinutes(5);

            // Build dependency-graph

            appHelper = new AppHelper();
            curseHelper = new CurseHelper();
            downloadHelper = new DownloadHelper(httpClient);
            fileSystemHelper = new FileSystemHelper();
            gitHubHelper = new GitHubHelper(httpClient);
            pluralizeHelper = new PluralizeHelper();
            unzipHelper = new UnzipHelper();

            var logFile = Path.Combine(appHelper.GetApplicationExecutableFolder(), $"{appHelper.GetApplicationName()}.log");
            logger = new TextFileLogger(logFile);
            reliableFileOperations = new ReliableFileOperations();

            configStorage = new XmlConfigStorage(logger, reliableFileOperations);
            configReader = new XmlConfigReader(logger, configStorage);
            configValidator = new XmlConfigValidator(logger, curseHelper, fileSystemHelper);

            smartUpdateFeature = new SmartUpdateFeature(logger);
            scraperApiClient = new ScraperApiClient(logger, httpClient);
            singleAddonProcessor = new SingleAddonProcessor(logger, curseHelper, smartUpdateFeature, downloadHelper, unzipHelper);
            multiAddonProcessor = new MultiAddonProcessor(logger, curseHelper, scraperApiClient, singleAddonProcessor);

            updateManager = new UpdateManager(reliableFileOperations, gitHubHelper, fileSystemHelper, downloadHelper, unzipHelper, appHelper);
            appRestarter = new AppRestarter(appHelper, reliableFileOperations);
        }

        public DomainLogic(
            IAppHelper appHelper,
            ICurseHelper curseHelper,
            IDownloadHelper downloadHelper,
            IFileSystemHelper fileSystemHelper,
            IGitHubHelper gitHubHelper,
            IPluralizeHelper pluralizeHelper,
            IUnzipHelper unzipHelper,
            ILogger logger,
            IReliableFileOperations reliableFileOperations,
            IConfigStorage configStorage,
            IConfigReader configReader,
            IConfigValidator configValidator,
            ISmartUpdateFeature smartUpdateFeature,
            IScraperApiClient scraperApiClient,
            ISingleAddonProcessor singleAddonProcessor,
            IMultiAddonProcessor multiAddonProcessor,
            IUpdateManager updateManager,
            IAppRestarter appRestarter)
        {
            // Use this constructor if you want to build the dependency graph on your own (when you want to use non-default implementations)

            this.appHelper = appHelper ?? throw new ArgumentNullException(nameof(appHelper));
            this.curseHelper = curseHelper ?? throw new ArgumentNullException(nameof(curseHelper));
            this.downloadHelper = downloadHelper ?? throw new ArgumentNullException(nameof(downloadHelper));
            this.fileSystemHelper = fileSystemHelper ?? throw new ArgumentNullException(nameof(fileSystemHelper));
            this.gitHubHelper = gitHubHelper ?? throw new ArgumentNullException(nameof(gitHubHelper));
            this.pluralizeHelper = pluralizeHelper ?? throw new ArgumentNullException(nameof(pluralizeHelper));
            this.unzipHelper = unzipHelper ?? throw new ArgumentNullException(nameof(unzipHelper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.reliableFileOperations = reliableFileOperations ?? throw new ArgumentNullException(nameof(reliableFileOperations));
            this.configStorage = configStorage ?? throw new ArgumentNullException(nameof(configStorage));
            this.configReader = configReader ?? throw new ArgumentNullException(nameof(configReader));
            this.configValidator = configValidator ?? throw new ArgumentNullException(nameof(configValidator));
            this.smartUpdateFeature = smartUpdateFeature ?? throw new ArgumentNullException(nameof(smartUpdateFeature));
            this.scraperApiClient = scraperApiClient ?? throw new ArgumentNullException(nameof(scraperApiClient));
            this.singleAddonProcessor = singleAddonProcessor ?? throw new ArgumentNullException(nameof(singleAddonProcessor));
            this.multiAddonProcessor = multiAddonProcessor ?? throw new ArgumentNullException(nameof(multiAddonProcessor));
            this.updateManager = updateManager ?? throw new ArgumentNullException(nameof(updateManager));
            this.appRestarter = appRestarter ?? throw new ArgumentNullException(nameof(appRestarter));
        }

        public SettingsData Settings { get; private set; } = SettingsData.Empty();
        public string ConfigStorageInformation => configStorage.StorageInformation;

        #region Application

        public void LogApplicationStart()
        {
            logger.ClearLog();
            logger.Log("Application started and log file was cleared.");
        }

        public async Task LoadSettingsAsync(CancellationToken cancellationToken = default)
        {
            if (!configStorage.StorageExists)
            {
                try
                {
                    await configStorage.CreateStorageWithDefaultsAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    logger.Log(e);
                    throw new InvalidOperationException("Could not create empty default config (see log file for details).", e);
                }
            }

            ConfigData configData;
            try
            {
                configData = await configReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Could not load config (see log file for details).", e);
            }

            try
            {
                configValidator.Validate(configData);
            }
            catch (Exception e)
            {
                if (e is ConfigValidationException)
                {
                    throw;
                }
                else
                {
                    logger.Log(e);
                    throw new InvalidOperationException("Data validation of loaded config failed (see log file for details).", e);
                }
            }

            var settingsData = new SettingsData(appHelper.GetApplicationExecutableFolder(), configData.ActiveOptions, configData.AddonUrls, configData.TargetFolder);
            var optionsAsString = settingsData.Options.Any() ? string.Join(", ", settingsData.Options) : "NONE";
            logger.Log(
            [
                "Application settings loaded",
                $" => {nameof(Settings.ApplicationFolder)} = {settingsData.ApplicationFolder}",
                $" => {nameof(Settings.Options)}           = {optionsAsString}",
                $" => {nameof(Settings.AddonUrls)}         = {settingsData.AddonUrls.Count()}",
                $" => {nameof(Settings.AddonTargetFolder)} = {settingsData.AddonTargetFolder}",
            ]);

            Settings = settingsData;
        }

        public string GetApplicationVersion()
        {
            return appHelper.GetApplicationVersion();
        }

        public void OpenProgramFolderInExplorer()
        {
            try
            {
                var folder = appHelper.GetApplicationExecutableFolder();
                Process.Start("explorer", folder);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Could not start Explorer.exe process to open program folder (see log file for details).", e);
            }
        }

        public void OpenAddonsFolderInExplorer()
        {
            try
            {
                var folder = Settings.AddonTargetFolder;
                Process.Start("explorer", folder);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Could not start Explorer.exe process to open addons folder (see log file for details).", e);
            }
        }

        public void OpenConfigFolderInExplorer()
        {
            try
            {
                var folder = Path.GetDirectoryName(ConfigStorageInformation) ?? throw new InvalidOperationException("Could not get directory name from config storage information.");
                Process.Start("explorer", folder);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Could not start Explorer.exe process to open config folder (see log file for details).", e);
            }
        }

        public void ShowLogFileInNotepad()
        {
            try
            {
                Process.Start("notepad", logger.StorageInformation);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Could not start Notepad.exe process to show log file (see log file for details).", e);
            }
        }

        public string PluralizeWordByCount(string singular, uint count)
        {
            return pluralizeHelper.PluralizeWord(singular, () => count != 1);
        }

        #endregion

        #region Addons

        public async Task<uint> ProcessAddonsAsync(IProgress<byte>? progress = null, CancellationToken cancellationToken = default)
        {
            // Prepare folders

            var smartUpdateFolder = Path.Combine(Settings.ApplicationFolder, "SmartUpdate");
            var downloadFolder = Path.Combine(Settings.ApplicationFolder, "CurseDownload");
            var targetFolder = Settings.AddonTargetFolder;

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
                countOfUpdatedAddons = await multiAddonProcessor.ProcessAddonsAsync(Settings.AddonUrls, downloadFolder, Settings.AddonTargetFolder,
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

        #endregion

        #region Update

        public async Task<UpdateData> CheckForUpdateAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var updateData = await updateManager.CheckForUpdateAsync(cancellationToken).ConfigureAwait(false);

                return updateData;
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException($"Could not determine the latest {appHelper.GetApplicationName()} version (see log file for details).", e);
            }
        }

        public async Task DownloadUpdateAsync(UpdateData updateData, IProgress<DownloadProgress>? downloadProgress = null, CancellationToken cancellationToken = default)
        {
            var updateFolder = Path.Combine(Settings.ApplicationFolder, "AppUpdate");

            try
            {
                await updateManager.DownloadUpdateAsync(updateData, updateFolder, downloadProgress, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException($"Error while downloading latest {appHelper.GetApplicationName()} release (see log file for details).", e);
            }
        }

        public async Task ApplyUpdateAndRestartApplicationAsync(CancellationToken cancellationToken = default)
        {
            var updateFolder = Path.Combine(Settings.ApplicationFolder, "AppUpdate");

            try
            {
                await updateManager.ApplyUpdateAsync(updateFolder, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Could not apply update (see log file for details).", e);
            }

            try
            {
                appRestarter.RestartApplication(2);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Error while restarting application (see log file for details).", e);
            }
        }

        public async Task RemoveBakFileIfExistsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await appRestarter.RemoveBakFileIfExistsAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Log(e);
                throw new InvalidOperationException("Error while removing .bak file of application update (see log file for details).", e);
            }
        }

        #endregion

        private static bool IsCancellationException(Exception e) => e is TaskCanceledException || e is OperationCanceledException;
    }
}
