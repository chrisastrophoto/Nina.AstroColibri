using Newtonsoft.Json;
using NINA.Astrometry;
using NINA.Core.Enum;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Profile;
using NINA.Profile.Interfaces;
using NINA.Sequencer.Container;
using NINA.Sequencer.Interfaces.Mediator;
using NINA.Sequencer.Mediator;
using NINA.Sequencer.SequenceItem;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChristophNieswand.NINA.Astrocolibri.AstrocolibriSequenceItems {

    [ExportMetadata("Name", "AstroColibri Instruction")]
    [ExportMetadata("Description", "This instruction will add a DSO Sequence Template for the latest visible transient to the sequence and will continue there")]
    [ExportMetadata("Icon", "Astrocolibri_SVG")]
    [ExportMetadata("Category", "AstroColibri")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class AstrocolibriInstruction : SequenceItem {

        [JsonProperty]
        private string dsoTemplate { get; set; }

        public string DSOTemplate {
            get => dsoTemplate; set {
                dsoTemplate = value;
                RaisePropertyChanged();
            }
        }

        [ImportingConstructor]
        public AstrocolibriInstruction(IProfileService profileService, IApplicationMediator applicationMediator, ISequenceMediator sequenceMediator) {
            ProfileService = profileService;
            ApplicationMediator = applicationMediator;
            SequenceMediator = sequenceMediator;
            DSOTemplate ??= Astrocolibri.AstroColibriOptions.DsoTemplate;
        }

        public AstrocolibriInstruction(IProfileService profileService, IApplicationMediator applicationMediator, ISequenceMediator sequenceMediator, AstrocolibriInstruction copyMe) : this(profileService, applicationMediator, sequenceMediator) {
            CopyMetaData(copyMe);
        }

        [JsonProperty]
        public IApplicationMediator ApplicationMediator { get; set; }

        public ISequenceMediator SequenceMediator { get; set; }

        public IProfileService ProfileService { get; set; }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            // Add logic to run the item here

            if (!Astrocolibri.API.HasNoTransient)
                AddDSOSequence(Astrocolibri.API.LatestTransient);

            Astrocolibri.API.HasNoTransient = true;
            return Task.CompletedTask;
        }

        public override object Clone() {
            return new AstrocolibriInstruction(ProfileService, ApplicationMediator, SequenceMediator, this);
        }

        /// <returns></returns>
        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(AstrocolibriInstruction)}";
        }

        private void AddDSOSequence(DeepSkyObject dso) {
            ApplicationMediator.ChangeTab(ApplicationTab.SEQUENCE);
            Task.Run(async () => {
                // This is needed for the tab to start loading and the virtualizing stack panel to allocate proper space. otherwise we run into problems
                await Task.Delay(100);

                IList<IDeepSkyObjectContainer> DSOTemplates = SequenceMediator.GetDeepSkyObjectContainerTemplates();
                IDeepSkyObjectContainer container = null;
                foreach (IDeepSkyObjectContainer c in DSOTemplates)
                    if (c.Name == DSOTemplate) {
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
                        Logger.Info("Adding target " + dso.Name + "to advanced sequencer: {container.Target.DeepSkyObject.Name} - {container.Target.DeepSkyObject.Coordinates}");
                        SequenceMediator.AddAdvancedTarget(container);
                    });
                    SequenceMediator.CancelAdvancedSequence();

                    while (SequenceMediator.IsAdvancedSequenceRunning()) {
                        await Task.Delay(100);
                    }
                    await Application.Current.Dispatcher.BeginInvoke(async () => {
                        Logger.Info("Relaunching Sequencer");
                        await SequenceMediator.StartAdvancedSequence(true);
                    });
                } else {
                    Notification.ShowInformation("DSO Template " + DSOTemplate + " not found");
                    Logger.Info("DSO Template " + DSOTemplate + " not found");
                }
            });
        }
    }
}