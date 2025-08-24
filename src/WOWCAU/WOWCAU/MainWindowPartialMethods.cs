using System.Windows;
using System.Windows.Documents;

namespace WOWCAU
{
    public partial class MainWindow : Window
    {
        private void SetControls(bool enabled)
        {
            SetLink(0, enabled);
            SetLink(1, enabled);
            SetProgress(enabled, null, null, null);
            button.IsEnabled = enabled;
        }

        private void SetLink(uint target, bool enabled)
        {
            // target == 0 -> config folder link
            // target == 1 -> check updates link

            if (target == 0 && linksPanel.Children.Contains(textBlockConfigFolder))
            {
                hyperlinkConfigFolder.IsEnabled = enabled;
                DisableHyperlinkHoverEffect(hyperlinkConfigFolder);
            }

            if (target == 1 && linksPanel.Children.Contains(textBlockCheckUpdates))
            {
                hyperlinkCheckUpdates.IsEnabled = enabled;
                DisableHyperlinkHoverEffect(hyperlinkCheckUpdates);
            }
        }

        private void RemoveLink(uint target)
        {
            // target == 0 -> config folder link
            // target == 1 -> check updates link

            if (target == 0 && linksPanel.Children.Contains(textBlockConfigFolder))
            {
                linksPanel.Children.Remove(textBlockConfigFolder);
            }

            if (target == 1 && linksPanel.Children.Contains(textBlockCheckUpdates))
            {
                linksPanel.Children.Remove(textBlockCheckUpdates);
            }
        }

        private void SetProgress(bool? enabled, string? text, double? value, double? maximum)
        {
            if (text != null) labelProgressBar.Content = text;
            if (value != null) progressBar.Value = value.Value;
            if (maximum != null) progressBar.Maximum = maximum.Value;

            if (enabled != null)
            {
                labelProgressBar.IsEnabled = enabled.Value;
                progressBar.IsEnabled = enabled.Value;
            }
        }

        // Hyperlink control in WPF has some default hover effect which sets foreground color to red.
        // Since i don´t want that behaviour and since Hyperlink is somewhat painful to style in WPF,
        // i just set the correct default system colors manually, when Hyperlink is enabled/disabled.
        // Note: This is just some temporary fix anyway, cause of the upcoming theme-support changes.
        private static void DisableHyperlinkHoverEffect(Hyperlink hyperlink) =>
            hyperlink.Foreground = hyperlink.IsEnabled ? SystemColors.HotTrackBrush : SystemColors.GrayTextBrush;

        private static void ShowInfo(string message) =>
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);

        private static void ShowError(string message) =>
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
