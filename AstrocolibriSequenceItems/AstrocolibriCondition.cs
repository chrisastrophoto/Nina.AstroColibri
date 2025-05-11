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

    /// <summary>
    /// This Class shows the basic principle on how to add a new Sequence Trigger to the N.I.N.A. sequencer via the plugin interface
    /// For ease of use this class inherits the abstract SequenceTrigger which already handles most of the running logic, like logging, exception handling etc.
    /// A complete custom implementation by just implementing ISequenceTrigger is possible too
    /// The following MetaData can be set to drive the initial values
    /// --> Name - The name that will be displayed for the item
    /// --> Description - a brief summary of what the item is doing. It will be displayed as a tooltip on mouseover in the application
    /// --> Icon - a string to the key value of a Geometry inside N.I.N.A.'s geometry resources
    ///
    /// If the item has some preconditions that should be validated, it shall also extend the IValidatable interface and add the validation logic accordingly.
    /// </summary>
    [ExportMetadata("Name", "AstroColibri Loop Condition")]
    [ExportMetadata("Description", "This condition is true until a new visible transient has been received from Astro-COLIBRI")]
    [ExportMetadata("Icon", "Astrocolibri_SVG")]
    [ExportMetadata("Category", "AstroColibri")]
    [Export(typeof(ISequenceCondition))]
    [JsonObject(MemberSerialization.OptIn)]
    public class AstrocolibriCondition : SequenceCondition {

        /// <summary>
        /// The constructor marked with [ImportingConstructor] will be used to import and construct the object
        /// General device interfaces can be added to the constructor parameters and will be automatically injected on instantiation by the plugin loader
        /// </summary>
        /// <remarks>
        /// Available interfaces to be injected:
        ///     - IProfileService,
        ///     - ICameraMediator,
        ///     - ITelescopeMediator,
        ///     - IFocuserMediator,
        ///     - IFilterWheelMediator,
        ///     - IGuiderMediator,
        ///     - IRotatorMediator,
        ///     - IFlatDeviceMediator,
        ///     - IWeatherDataMediator,
        ///     - IImagingMediator,
        ///     - IApplicationStatusMediator,
        ///     - INighttimeCalculator,
        ///     - IPlanetariumFactory,
        ///     - IImageHistoryVM,
        ///     - IDeepSkyObjectSearchVM,
        ///     - IDomeMediator,
        ///     - IImageSaveMediator,
        ///     - ISwitchMediator,
        ///     - ISafetyMonitorMediator,
        ///     - IApplicationMediator
        ///     - IApplicationResourceDictionary
        ///     - IFramingAssistantVM
        ///     - IList<IDateTimeProvider>
        /// </remarks>
        [ImportingConstructor]
        public AstrocolibriCondition(IProfileService profileService, IApplicationMediator applicationMediator, ISequenceMediator sequenceMediator) {
            HasNoTransient = true;
            SequenceMediator = sequenceMediator;
            ProfileService = profileService;
            ApplicationMediator = applicationMediator;
        }

        public IProfileService ProfileService { get; set; }
        public IApplicationMediator ApplicationMediator { get; set; }
        public ISequenceMediator SequenceMediator { get; set; }
        private bool hasNoTransient;

        [JsonProperty]
        public bool HasNoTransient {
            get => hasNoTransient;
            set {
                hasNoTransient = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Once this check returns false, the condition will cause its parent instruction set to skip the rest and proceed with the next set
        /// </summary>
        /// <param name="previousItem"></param>
        /// <param name="nextItem"></param>
        /// <returns></returns>
        public override bool Check(ISequenceItem previousItem, ISequenceItem nextItem) {
            HasNoTransient = Astrocolibri.API.HasNoTransient;
            if (previousItem == null) {
                Astrocolibri.API.HasNoTransient = true;
                Application.Current.Dispatcher.BeginInvoke(() => {
                    Notification.CloseAll();
                });
            }
            if (!hasNoTransient) {
                AddDSOSequence(Astrocolibri.API.LatestTransient);
                Astrocolibri.API.HasNoTransient = true;

                // This is not solvable: This works with only one smart exposure in a loop or with normal take exposure in Loops
                // If there are more than one smart exposure and the trigger was trigegred in the first snmart exposure, the relauch always continues on the second smart exposure which is not yet marked as done. So it triggers again!
                // This problem arises from the fact, that the inserted DSO template comes after all smart exposures in the 1 loop which is necessary to prevent the astrocolibri condition let the loop run indefinively ...

                // When implementing the AddDSOSequence in the trigger. It seemed to work somehow. But then I had trouble on restarting after stopping the seqence manually.
            }
            return HasNoTransient;
        }

        public override object Clone() {
            return new AstrocolibriCondition(ProfileService, ApplicationMediator, SequenceMediator) {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description
            };
        }

        /// <summary>
        /// This string will be used for logging
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(AstrocolibriCondition)}, HasNoTransient: {HasNoTransient}";
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
                    Notification.ShowInformation("DSO Template " + dsoTemplateName + " not found");
                    Logger.Info("DSO Template " + dsoTemplateName + " not found");
                }
            });
        }
    }
}