using System.Windows;
using WOWCAU.Core.Parts.Domain.Contracts;

namespace WOWCAU
{
    public partial class MainWindow : Window
    {
        private readonly IAppModule appModule;
        private readonly IUpdateModule updateModule;
        private readonly IAddonsModule addonsModule;

        public MainWindow(IAppModule appModule, IUpdateModule updateModule, IAddonsModule addonsModule)
        {
            this.appModule = appModule ?? throw new ArgumentNullException(nameof(appModule));
            this.updateModule = updateModule ?? throw new ArgumentNullException(nameof(updateModule));
            this.addonsModule = addonsModule ?? throw new ArgumentNullException(nameof(addonsModule));

            InitializeComponent();

            MinWidth = Width;
            MinHeight = Height;
            Title = $"WOWCAM {appModule.GetApplicationVersion()}";

            textBlockConfigFolder.Visibility = Visibility.Hidden;
            textBlockCheckUpdates.Visibility = Visibility.Hidden;

            SetProgress(false, string.Empty, 0, 100);
            SetControls(false);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            // Register a hook which reacts to a specific custom window message

            base.OnSourceInitialized(e);

            SingleInstance.RegisterHook(this);
        }
    }
}
