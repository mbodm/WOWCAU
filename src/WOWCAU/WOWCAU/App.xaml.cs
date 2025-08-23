using System.Net.Http;
using System.Windows;
using WOWCAU.Core.Parts.Addons.Defaults;
using WOWCAU.Core.Parts.Config.Defaults;
using WOWCAU.Core.Parts.Domain.Defaults;
using WOWCAU.Core.Parts.Logging.Defaults;
using WOWCAU.Core.Parts.System.Defaults;
using WOWCAU.Core.Parts.Update.Defaults;

namespace WOWCAU
{
    public partial class App : Application
    {
        private static readonly HttpClient httpClient = new();

        public App()
        {
            if (SingleInstance.AnotherInstanceIsAlreadyRunning)
            {
                SingleInstance.BroadcastMessage();
                Shutdown();
                return;
            }

            // Logger
            var logger = new TextFileLogger(AppModule.LogFile);

            // App
            var reliableFileOperations = new ReliableFileOperations();
            var configStorage = new XmlConfigStorage(logger, reliableFileOperations);
            var configReader = new XmlConfigReader(logger, configStorage);
            var configValidator = new XmlConfigValidator(logger);
            var appModule = new AppModule(logger, configStorage, configReader, configValidator);

            // Update
            var gitHubClient = new GitHubClient(httpClient);
            var updateManager = new UpdateManager(httpClient, reliableFileOperations, gitHubClient);
            var updateModule = new UpdateModule(logger, appModule, updateManager);

            // Addons
            var smartUpdateFeature = new SmartUpdateFeature(logger, reliableFileOperations);
            var singleAddonProcessor = new SingleAddonProcessor(httpClient, smartUpdateFeature);
            var multiAddonProcessor = new MultiAddonProcessor(logger, httpClient, singleAddonProcessor);
            var addonsModule = new AddonsModule(logger, appModule, smartUpdateFeature, multiAddonProcessor, reliableFileOperations);

            MainWindow = new MainWindow(appModule, updateModule, addonsModule);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow.Show();
        }
    }
}
