using System.Diagnostics;
using WOWCAU.Core.Parts.Config.Contracts;
using WOWCAU.Core.Parts.Config.Types;
using WOWCAU.Core.Parts.Domain.Modules.Contracts;
using WOWCAU.Core.Parts.Domain.Modules.Types;
using WOWCAU.Core.Parts.Logging.Contracts;
using WOWCAU.Helper.Parts.Contracts;

namespace WOWCAU.Core.Parts.Domain.Modules.Defaults
{
    public sealed class AppModule(
        ILogger logger, IAppHelper appHelper, IPluralizeHelper pluralizeHelper, IConfigStorage configStorage, IConfigReader configReader, IConfigValidator configValidator) : IAppModule
    {
        private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAppHelper appHelper = appHelper ?? throw new ArgumentNullException(nameof(appHelper));
        private readonly IPluralizeHelper pluralizeHelper = pluralizeHelper ?? throw new ArgumentNullException(nameof(pluralizeHelper));
        private readonly IConfigStorage configStorage = configStorage ?? throw new ArgumentNullException(nameof(configStorage));
        private readonly IConfigReader configReader = configReader ?? throw new ArgumentNullException(nameof(configReader));
        private readonly IConfigValidator configValidator = configValidator ?? throw new ArgumentNullException(nameof(AppModule.configValidator));

        public string LogFile => Path.Combine(appHelper.GetApplicationExecutableFolder(), $"{appHelper.GetApplicationName()}.log");
        public SettingsData Settings { get; private set; } = SettingsData.Empty();
        public string ConfigStorageInformation => configStorage.StorageInformation;

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

            var workFolder = appHelper.GetApplicationExecutableFolder();
            var settingsData = new SettingsData(
                Options: configData.ActiveOptions,
                WorkFolder: workFolder,
                TempFolder: Path.Combine(workFolder, "TempFolder"),
                AddonUrls: configData.AddonUrls,
                AddonTargetFolder: configData.TargetFolder);

            var optionsAsString = settingsData.Options.Any() ? string.Join(", ", settingsData.Options) : "NONE";
            logger.Log(
            [
                "Application settings loaded",
                    $" => {nameof(Settings.Options)}           = {optionsAsString}",
                    $" => {nameof(Settings.WorkFolder)}        = {settingsData.WorkFolder}",
                    $" => {nameof(Settings.TempFolder)}        = {settingsData.TempFolder}",
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

        public string PluralizeAddonWord(uint count)
        {
            return pluralizeHelper.PluralizeWord("addon", () => count != 1);
        }
    }
}
