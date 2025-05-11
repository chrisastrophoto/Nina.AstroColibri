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

    [ExportMetadata("Name", "Astrocolibri Trigger")]
    [ExportMetadata("Description", "This trigger will check for new transients on Astro-COLIBRI")]
    [ExportMetadata("Icon", "Astrocolibri_SVG")]
    [ExportMetadata("Category", "AstroColibri")]
    [Export(typeof(ISequenceTrigger))]
    [JsonObject(MemberSerialization.OptIn)]
    public class AstrocolibriTrigger : SequenceTrigger {

        [ImportingConstructor]
        public AstrocolibriTrigger(IProfileService profileService, IApplicationMediator applicationMediator, ISequenceMediator sequenceMediator) : base() {
            SequenceMediator = sequenceMediator;
            ProfileService = profileService;
            ApplicationMediator = applicationMediator;
        }

        public override object Clone() {
            return new AstrocolibriTrigger(ProfileService, ApplicationMediator, SequenceMediator) {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description
            };
        }

        public ISequenceMediator SequenceMediator { get; set; }
        public IProfileService ProfileService { get; set; }
        public IApplicationMediator ApplicationMediator { get; set; }

        public override Task Execute(ISequenceContainer context, IProgress<ApplicationStatus> progress, CancellationToken token) {
            Astrocolibri.API.LatestTransients();
            //if (!Astrocolibri.API.HasNoTransient && Astrocolibri.API.LatestTransient != null) {
            //   AddDSOSequence(Astrocolibri.API.LatestTransient);
            //}
            return Task.CompletedTask;
        }

        public override bool ShouldTrigger(ISequenceItem previousItem, ISequenceItem nextItem) {
            return false;
        }

        public override bool ShouldTriggerAfter(ISequenceItem previousItem, ISequenceItem nextItem) {
            string mes = "After >> " + (previousItem == null ? "" : (previousItem.Name + ":" + previousItem.Category)) + " --> " + (nextItem == null ? "" : (nextItem.Name + ":" + nextItem.Category)) + " <<";
            Logger.Info(mes);
            if (previousItem != null && previousItem.Name.Equals("Take Exposure") && previousItem.Category.Equals("Camera")) {
                return true;
            }
            return false;
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(AstrocolibriTrigger)}";
        }

        private void AddDSOSequence(DeepSkyObject dso) {
            ApplicationMediator.ChangeTab(ApplicationTab.SEQUENCE);
            Task.Run(async () => {
                // This is needed for the tab to start loading and the virtualizing stack panel to allocate proper space. otherwise we run into problems
                await Task.Delay(100);

                IList<IDeepSkyObjectContainer> DSOTemplates = SequenceMediator.GetDeepSkyObjectContainerTemplates();
                IDeepSkyObjectContainer container = null;
                string dsoTemplateName = Astrocolibri.AstroColibriOptions.DsoTemplate;
                foreach (IDeepSkyObjectContainer c in DSOTemplates)
                    if (c.Name == dsoTemplateName) {
                        container = (IDeepSkyObjectContainer)c.Clone();
                        break;
                    }
                if (container != null) {
                    InputTarget it = new InputTarget(Angle.ByDegree(ProfileService.ActiveProfile.AstrometrySettings.Latitude), Angle.ByDegree(ProfileService.ActiveProfile.AstrometrySettings.Longitude), ProfileService.ActiveProfile.AstrometrySettings.Horizon) {
                        InputCoordinates = new InputCoordinates(dso.Coordinates),
                        TargetName = (dso.Name == null || dso.Name == "") ? "NoName" : dso.Name
                    };
                    container.Target = it;
                    container.Name = (dso.Name == null || dso.Name == "") ? "NoName" : dso.Name;
                    await Application.Current.Dispatcher.BeginInvoke(() => {
                        Logger.Info($"Adding target " + dso.Name + "to advanced sequencer: {container.Target.DeepSkyObject.Name} - {container.Target.DeepSkyObject.Coordinates}");
                        SequenceMediator.AddAdvancedTarget(container);
                    });
                } else {
                    Notification.ShowInformation("DSO Template " + dsoTemplateName + " not found");
                    Logger.Info("DSO Template " + dsoTemplateName + " not found");
                }
            });
        }
    }
}