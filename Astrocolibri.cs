using ChristophNieswand.NINA.Astrocolibri.AstrocolibriAPI;
using NINA.Core.Utility;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile.Interfaces;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Settings = ChristophNieswand.NINA.Astrocolibri.Properties.Settings;

namespace ChristophNieswand.NINA.Astrocolibri {

    [Export(typeof(IPluginManifest))]
    public class Astrocolibri : PluginBase, INotifyPropertyChanged {
        public readonly IPluginOptionsAccessor pluginSettings;
        private readonly IProfileService profileService;

        [ImportingConstructor]
        public Astrocolibri(IProfileService profileService) {
            if (Settings.Default.UpdateSettings) {
                Settings.Default.Upgrade();
                Settings.Default.UpdateSettings = false;
                CoreUtil.SaveSettings(Settings.Default);
            }

            this.profileService = profileService;

            AstroColibriOptions = new AstrocolibriOptions.AstrocolibriOptions(this, this.profileService);

            API = new AstroColibriAPI();

            this.profileService.ProfileChanged += ProfileService_ProfileChanged;

            OpenJSONFolderDiagCommand = new GalaSoft.MvvmLight.Command.RelayCommand(OpenJSONFolderDiag);
        }

        public override Task Teardown() {
            profileService.ProfileChanged -= ProfileService_ProfileChanged;

            return base.Teardown();
        }

        public static AstroColibriAPI API { get; set; } = null;

        public static AstrocolibriOptions.AstrocolibriOptions AstroColibriOptions { get; private set; }

        public static AstrocolibriDockables.AstrocolibriDockable AstroColibriDockable { get; set; } = null;

        public void ProfileService_ProfileChanged(object sender, EventArgs e) {
            AstroColibriOptions.InitializeOptions();
            RaisePropertyChanged(nameof(AstroColibriOptions.Uid));
            RaisePropertyChanged(nameof(AstroColibriOptions.WaitMinMinutes));
            RaisePropertyChanged(nameof(AstroColibriOptions.CheckMinutes));
            RaisePropertyChanged(nameof(AstroColibriOptions.JSONFilePath));
            RaisePropertyChanged(nameof(AstroColibriOptions.KeepFilesDays));
            RaisePropertyChanged(nameof(AstroColibriOptions.MinAltitude));
            RaisePropertyChanged(nameof(AstroColibriOptions.TestMode));
            RaisePropertyChanged(nameof(AstroColibriOptions.SaveSequence));
            RaisePropertyChanged(nameof(AstroColibriOptions.DsoTemplate));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(AstroColibriOptions, new PropertyChangedEventArgs(propertyName));
        }

        private void OpenJSONFolderDiag() {
            using (var diag = new System.Windows.Forms.FolderBrowserDialog()) {
                if (Directory.Exists(AstroColibriOptions.JSONFilePath)) {
                    diag.SelectedPath = AstroColibriOptions.JSONFilePath;
                }

                System.Windows.Forms.DialogResult result = diag.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK) {
                    if (Directory.Exists(diag.SelectedPath)) {
                        AstroColibriOptions.JSONFilePath = diag.SelectedPath;
                    }
                }
            }
        }

        public ICommand OpenJSONFolderDiagCommand { get; }
    }
}