using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace ChristophNieswand.NINA.Astrocolibri.AstrocolibriDockables {

    [Export(typeof(ResourceDictionary))]
    public partial class MyPluginDockableTemplates : ResourceDictionary {

        public MyPluginDockableTemplates() {
            InitializeComponent();
        }

        private void WebPageClick(object sender, RoutedEventArgs e) {
            Hyperlink link = e.OriginalSource as Hyperlink;
            Process.Start(new ProcessStartInfo(link.NavigateUri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}