using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Documents;

namespace ChristophNieswand.NINA.Astrocolibri.AstrocolibriOptions {

    [Export(typeof(ResourceDictionary))]
    public partial class AstrocolibriOptionsTemplates : ResourceDictionary {

        public AstrocolibriOptionsTemplates() {
            InitializeComponent();
        }

        private void WebPageClick(object sender, RoutedEventArgs e) {
            Hyperlink link = e.OriginalSource as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}