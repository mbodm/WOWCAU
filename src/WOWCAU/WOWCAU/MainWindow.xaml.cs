using System.Windows;
using WOWCAU.Core.Parts.Domain.Contracts;

namespace WOWCAU
{
    public partial class MainWindow : Window
    {
        private readonly IDomainLogic domainLogic;

        public MainWindow(IDomainLogic domainLogic)
        {
            this.domainLogic = domainLogic ?? throw new ArgumentNullException(nameof(domainLogic));

            InitializeComponent();

            MinWidth = Width;
            MinHeight = Height;
            Title = $"WOWCAU {domainLogic.GetApplicationVersion()}";

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
