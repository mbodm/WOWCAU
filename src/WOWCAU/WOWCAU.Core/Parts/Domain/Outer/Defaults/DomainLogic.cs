using WOWCAU.Core.Parts.Addons.Defaults;
using WOWCAU.Core.Parts.Config.Defaults;
using WOWCAU.Core.Parts.Domain.Modules.Contracts;
using WOWCAU.Core.Parts.Domain.Modules.Defaults;
using WOWCAU.Core.Parts.Domain.Outer.Contracts;
using WOWCAU.Core.Parts.Logging.Contracts;
using WOWCAU.Core.Parts.Logging.Defaults;
using WOWCAU.Core.Parts.System.Defaults;
using WOWCAU.Core.Parts.Update.Defaults;
using WOWCAU.Helper.Parts.Contracts;

namespace WOWCAU.Core.Parts.Domain.Outer.Defaults
{
    public sealed class DomainLogic : IDomainLogic
    {
        private readonly ILogger logger;
        private readonly IAppModule appModule;
        private readonly IAddonsModule addonsModule;
        private readonly IUpdateModule updateModule;

        public DomainLogic(
            HttpClient httpClient,
            IAppHelper appHelper,
            IPluralizeHelper pluralizeHelper,
            ICurseHelper curseHelper,
            IFileSystemHelper fileSystemHelper,
            IDownloadHelper downloadHelper,
            IUnzipHelper unzipHelper,
            IGitHubHelper gitHubHelper)
        {
            // Dependency-Graph

            var logFile = Path.Combine(appHelper.GetApplicationExecutableFolder(), $"{appHelper.GetApplicationName()}.log");
            logger = new TextFileLogger(logFile);

            var reliableFileOperations = new ReliableFileOperations();
            var configStorage = new XmlConfigStorage(logger, reliableFileOperations);
            var configReader = new XmlConfigReader(logger, configStorage);
            var configValidator = new XmlConfigValidator(logger, curseHelper, fileSystemHelper);
            appModule = new AppModule(logger, appHelper, pluralizeHelper, configStorage, configReader, configValidator);

            var smartUpdateFeature = new SmartUpdateFeature(logger, reliableFileOperations);
            var singleAddonProcessor = new SingleAddonProcessor(curseHelper, downloadHelper, unzipHelper, smartUpdateFeature);
            var multiAddonProcessor = new MultiAddonProcessor(logger, curseHelper, singleAddonProcessor, httpClient);
            addonsModule = new AddonsModule(logger, appModule, smartUpdateFeature, multiAddonProcessor, fileSystemHelper, reliableFileOperations);

            var updateManager = new UpdateManager(reliableFileOperations, gitHubHelper, fileSystemHelper, downloadHelper, unzipHelper, appHelper);
            updateModule = new UpdateModule(logger, appHelper, appModule, updateManager);
        }

        public ILogger Logger => logger;
        public IAppModule App => appModule;
        public IAddonsModule Addons => addonsModule;
        public IUpdateModule Update => updateModule;
    }
}
