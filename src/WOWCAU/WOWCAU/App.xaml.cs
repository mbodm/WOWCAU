using System.Net.Http;
using System.Windows;
using WOWCAU.Core.Parts.Domain.Defaults;

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

            var domainLogic = new DomainLogic(httpClient);

            MainWindow = new MainWindow(domainLogic);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow.Show();
        }
    }
}
