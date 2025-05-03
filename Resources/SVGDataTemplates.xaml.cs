using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChristophNieswand.NINA.Astrocolibri.Resources {

    [Export(typeof(ResourceDictionary))]
    public partial class SVGDataTemplates : ResourceDictionary {

        public SVGDataTemplates() {
            InitializeComponent();
        }
    }
}