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
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChristophNieswand.NINA.Astrocolibri.AstrocolibriSequenceItems {

    /// <summary>
    /// This Class shows the basic principle on how to add a new Sequence Instruction to the N.I.N.A. sequencer via the plugin interface
    /// For ease of use this class inherits the abstract SequenceItem which already handles most of the running logic, like logging, exception handling etc.
    /// A complete custom implementation by just implementing ISequenceItem is possible too
    /// The following MetaData can be set to drive the initial values
    /// --> Name - The name that will be displayed for the item
    /// --> Description - a brief summary of what the item is doing. It will be displayed as a tooltip on mouseover in the application
    /// --> Icon - a string to the key value of a Geometry inside N.I.N.A.'s geometry resources
    ///
    /// If the item has some preconditions that should be validated, it shall also extend the IValidatable interface and add the validation logic accordingly.
    /// </summary>
    [ExportMetadata("Name", "AstroColibri Instruction")]
    [ExportMetadata("Description", "This instruction will add a DSO Sequence Template for the latest visible transient to the sequence and will continue there")]
    [ExportMetadata("Icon", "Astrocolibri_SVG")]
    [ExportMetadata("Category", "AstroColibri")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class AstrocolibriInstruction : SequenceItem {

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
        public AstrocolibriInstruction(IProfileService profileService, IApplicationMediator applicationMediator, ISequenceMediator sequenceMediator) {
            ProfileService = profileService;
            ApplicationMediator = applicationMediator;
            SequenceMediator = sequenceMediator;
        }

        public AstrocolibriInstruction(IProfileService profileService, IApplicationMediator applicationMediator, ISequenceMediator sequenceMediator, AstrocolibriInstruction copyMe) : this(profileService, applicationMediator, sequenceMediator) {
            CopyMetaData(copyMe);
        }

        /// <summary>
        /// An example property that can be set from the user interface via the Datatemplate specified in PluginTestItem.Template.xaml
        /// </summary>
        /// <remarks>
        /// If the property changes from the code itself, remember to call RaisePropertyChanged() on it for the User Interface to notice the change
        /// </remarks>
        [JsonProperty]
        public IApplicationMediator ApplicationMediator { get; set; }

        public ISequenceMediator SequenceMediator { get; set; }

        public IProfileService ProfileService { get; set; }

        /// <summary>
        /// The core logic when the sequence item is running resides here
        /// Add whatever action is necessary
        /// </summary>
        /// <param name="progress">The application status progress that can be sent back during execution</param>
        /// <param name="token">When a cancel signal is triggered from outside, this token can be used to register to it or check if it is cancelled</param>
        /// <returns></returns>
        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            // Add logic to run the item here

            if (!Astrocolibri.API.HasNoTransient)
                AddDSOSequence(Astrocolibri.API.LatestTransient);

            Astrocolibri.API.HasNoTransient = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// When items are put into the sequence via the factory, the factory will call the clone method. Make sure all the relevant fields are cloned with the object.
        /// </summary>
        /// <returns></returns>
        public override object Clone() {
            return new AstrocolibriInstruction(ProfileService, ApplicationMediator, SequenceMediator, this);
        }

        /// <summary>
        /// This string will be used for logging
        /// </summary>
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
                string dsoTemplateName = Astrocolibri.AstroColibriOptions.DsoTemplate == "" ? "AstroColibri" : Astrocolibri.AstroColibriOptions.DsoTemplate;
                foreach (IDeepSkyObjectContainer c in DSOTemplates)
                    if (c.Name == dsoTemplateName) {
                        container = c;
                        break;
                    }
                if (container != null) {
                    InputTarget it = new InputTarget(Angle.ByDegree(ProfileService.ActiveProfile.AstrometrySettings.Latitude), Angle.ByDegree(ProfileService.ActiveProfile.AstrometrySettings.Longitude), ProfileService.ActiveProfile.AstrometrySettings.Horizon) {
                        DeepSkyObject = dso,
                        InputCoordinates = new InputCoordinates(dso.Coordinates),
                        TargetName = dso.Name == null || dso.Name == "" ? "NoName" : dso.Name
                    };
                    container.Target = it;
                    container.Name = dso.Name == null || dso.Name == "" ? "NoName" : dso.Name;
                    await Application.Current.Dispatcher.BeginInvoke(() => {
                        Logger.Info($"Adding target to advanced sequencer: {container.Target.DeepSkyObject.Name} - {container.Target.DeepSkyObject.Coordinates}");
                        SequenceMediator.AddAdvancedTarget(container);
                    });
                    await Task.Delay(100);
                } else {
                    Notification.ShowInformation("DSO Template " + dsoTemplateName + " not found");
                    Logger.Info("DSO Template " + dsoTemplateName + " not found");
                }
            });
        }
    }
}