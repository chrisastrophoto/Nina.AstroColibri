using Newtonsoft.Json;
using NINA.Astrometry;
using NINA.Core.Enum;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Profile;
using NINA.Sequencer.Container;
using NINA.Sequencer.Mediator;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Trigger;
using NINA.Sequencer.Interfaces.Mediator;
using NINA.WPF.Base.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using System.Security.Cryptography;
using NINA.Sequencer.Utility;

namespace ChristophNieswand.NINA.Astrocolibri.AstrocolibriSequenceItems {

    [ExportMetadata("Name", "AstroColibri Trigger")]
    [ExportMetadata("Description", "This trigger will check for new transients on Astro-COLIBRI")]
    [ExportMetadata("Icon", "Astrocolibri_SVG")]
    [ExportMetadata("Category", "AstroColibri")]
    [Export(typeof(ISequenceTrigger))]
    [JsonObject(MemberSerialization.OptIn)]
    public class AstrocolibriTrigger : SequenceTrigger {

        [ImportingConstructor]
        public AstrocolibriTrigger() : base() {
        }

        public override object Clone() {
            return new AstrocolibriTrigger() {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description
            };
        }

        public override Task Execute(ISequenceContainer context, IProgress<ApplicationStatus> progress, CancellationToken token) {
            Astrocolibri.API.LatestTransients();

            return Task.CompletedTask;
        }

        public override bool ShouldTrigger(ISequenceItem previousItem, ISequenceItem nextItem) {
            return false;
        }

        public override bool ShouldTriggerAfter(ISequenceItem previousItem, ISequenceItem nextItem) {
            string mes = "After >> " + (previousItem == null ? "" : (previousItem.Name + ":" + previousItem.Category)) + " --> " + (nextItem == null ? "" : (nextItem.Name + ":" + nextItem.Category)) + " <<";
            Logger.Info(mes);
            if (previousItem != null && previousItem.Name != null && previousItem.Category != null && previousItem.Name.Equals("Take Exposure") && previousItem.Category.Equals("Camera")) {
                return true;
            }
            return false;
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(AstrocolibriTrigger)}";
        }
    }
}