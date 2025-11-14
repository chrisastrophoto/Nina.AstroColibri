using Newtonsoft.Json;
using NINA.Astrometry;
using NINA.Core.Enum;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.Sequencer.Conditions;
using NINA.Sequencer.Container;
using NINA.Sequencer.Interfaces.Mediator;
using NINA.Sequencer.Mediator;
using NINA.Sequencer.SequenceItem;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter.Xml;
using NINA.Sequencer.Utility;

namespace ChristophNieswand.NINA.Astrocolibri.AstrocolibriSequenceItems {

    [ExportMetadata("Name", "AstroColibri Loop Condition")]
    [ExportMetadata("Description", "This condition is true until a new visible transient has been received from Astro-COLIBRI")]
    [ExportMetadata("Icon", "Astrocolibri_SVG")]
    [ExportMetadata("Category", "AstroColibri")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class AstrocolibriCondition : SequenceCondition {

        [ImportingConstructor]
        public AstrocolibriCondition() {
            HasNoTransient = true;
        }

        private bool hasNoTransient;

        [JsonProperty]
        public bool HasNoTransient {
            get => hasNoTransient;
            set {
                hasNoTransient = value;
                RaisePropertyChanged();
            }
        }

        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) {
            HasNoTransient = Astrocolibri.API.HasNoTransient;
            if (previousItem == null) {
                Astrocolibri.API.HasNoTransient = true;
                Application.Current.Dispatcher.BeginInvoke(() => {
                    Notification.CloseAll();
                });
            }

            return HasNoTransient;
        }

        public override object Clone() {
            return new AstrocolibriCondition() {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(AstrocolibriCondition)}, HasNoTransient: {HasNoTransient}";
        }
    }
}