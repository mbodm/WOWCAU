using System.Net.Http;
using System.Windows;
using WOWCAU.Core.Parts.Domain.Outer.Defaults;
using WOWCAU.Helper.Parts.Defaults;

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

            var domainLogic = new DomainLogic(
                httpClient,
                new AppHelper(),
                new PluralizeHelper(),
                new CurseHelper(),
                new FileSystemHelper(),
                new DownloadHelper(httpClient),
                new UnzipHelper(),
                new GitHubHelper(httpClient));

            MainWindow = new MainWindow(domainLogic);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow.Show();
        }
    }
}
