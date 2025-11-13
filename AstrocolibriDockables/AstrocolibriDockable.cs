using NINA.Astrometry;
using NINA.Astrometry.Interfaces;
using NINA.Core.Enum;
using NINA.Equipment.Interfaces.ViewModel;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NINA.WPF.Base.Interfaces.ViewModel;
using NINA.WPF.Base.ViewModel;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChristophNieswand.NINA.Astrocolibri.AstrocolibriDockables {

    [Export(typeof(IDockableVM))]
    public class AstrocolibriDockable : DockableVM {
        private INighttimeCalculator nighttimeCalculator;

        public NighttimeData NighttimeData { get; private set; }

        public AstrocolibriAPI.AstroColibriEvents Targets { get; private set; }

        private readonly IApplicationMediator applicationMediator;
        private readonly IFramingAssistantVM framingAssistantVM;

        public ICommand CoordsToFramingCommand { get; set; }
        public ICommand RemoveCommand { get; set; }

        public ICommand RemoveAllCommand { get; set; }

        [ImportingConstructor]
        public AstrocolibriDockable(
            IProfileService profileService,
            INighttimeCalculator nighttimeCalculator,
            IApplicationMediator applicationMediator,
            IFramingAssistantVM framingAssistantVM) : base(profileService) {
            var dict = new ResourceDictionary();
            dict.Source = new Uri("Astrocolibri.NINAPlugin;component/Resources/SVGDataTemplates.xaml", UriKind.RelativeOrAbsolute);
            ImageGeometry = (System.Windows.Media.GeometryGroup)dict["Astrocolibri_SVG"];
            ImageGeometry.Freeze();

            this.nighttimeCalculator = nighttimeCalculator;
            this.applicationMediator = applicationMediator;
            this.framingAssistantVM = framingAssistantVM;

            Title = "Altitude Charts for Astro-COLIBRI Events";
            Targets = null;

            Task.Run(() => {
                NighttimeData = nighttimeCalculator.Calculate();
                nighttimeCalculator.OnReferenceDayChanged += NighttimeCalculator_OnReferenceDayChanged;
            });

            profileService.LocationChanged += (object sender, EventArgs e) => {
                if (Targets != null) {
                    foreach (AstrocolibriAPI.AstroColibriEvent Target in Targets)
                        Target.DSO?.SetDateAndPosition(NighttimeCalculator.GetReferenceDate(DateTime.Now), profileService.ActiveProfile.AstrometrySettings.Latitude, profileService.ActiveProfile.AstrometrySettings.Longitude);
                    UpdateTargetInfo();
                }
            };

            profileService.HorizonChanged += (object sender, EventArgs e) => {
                if (Targets != null) {
                    foreach (AstrocolibriAPI.AstroColibriEvent Target in Targets)
                        Target.DSO?.SetCustomHorizon(profileService.ActiveProfile.AstrometrySettings.Horizon);
                    UpdateTargetInfo();
                }
            };

            CoordsToFramingCommand = new GalaSoft.MvvmLight.Command.RelayCommand<object>(SendCoordinatesToFraming);
            RemoveCommand = new GalaSoft.MvvmLight.Command.RelayCommand<object>(RemoveDSO);
            RemoveAllCommand = new GalaSoft.MvvmLight.Command.RelayCommand(RemoveAll);

            Astrocolibri.AstroColibriDockable = this;
        }

        private void SendCoordinatesToFraming(object dso) {
            _ = CoordsToFraming(dso);
        }

        private void RemoveDSO(object dso) {
            Astrocolibri.API.RemoveEvent((DeepSkyObject)dso);
            UpdateTargetInfo();
        }

        private void RemoveAll() {
            Astrocolibri.API.RemoveAllEvents();
            UpdateTargetInfo();
        }

        private async Task<bool> CoordsToFraming(object dso) {
            if (((DeepSkyObject)dso)?.Coordinates != null) {
                applicationMediator.ChangeTab(ApplicationTab.FRAMINGASSISTANT);
                return await framingAssistantVM.SetCoordinates((DeepSkyObject)dso);
            }
            return false;
        }

        private void NighttimeCalculator_OnReferenceDayChanged(object sender, EventArgs e) {
            NighttimeData = nighttimeCalculator.Calculate();
            RaisePropertyChanged(nameof(NighttimeData));
        }

        public void UpdateTargetInfo() {
            if (IsVisible) {
                if (NighttimeData != null) {
                    if (Astrocolibri.API != null && Astrocolibri.API.ACEvents != null) {
                        Targets = Astrocolibri.API.ACEvents;
                        foreach (AstrocolibriAPI.AstroColibriEvent Target in Targets) {
                            Target.DSO.SetDateAndPosition(NighttimeCalculator.GetReferenceDate(DateTime.Now), profileService.ActiveProfile.AstrometrySettings.Latitude, profileService.ActiveProfile.AstrometrySettings.Longitude);
                            var showMoon = Target.DSO != null ? Target.DSO.Moon.DisplayMoon : false;
                            if (showMoon) {
                                Target.DSO.Refresh();
                                Target.DSO.Moon.DisplayMoon = true;
                            }
                        }
                        RaisePropertyChanged(nameof(Targets));
                    }
                } else {
                    Targets = null;
                    RaisePropertyChanged(nameof(Targets));
                }
            }
        }
    }
}