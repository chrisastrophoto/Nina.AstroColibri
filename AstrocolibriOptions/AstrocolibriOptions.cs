using NINA.Core.Utility;
using NINA.Profile;
using NINA.Profile.Interfaces;
using System;
using System.ComponentModel;

namespace ChristophNieswand.NINA.Astrocolibri.AstrocolibriOptions {

    /// <summary>
    /// This class helps to maintain profile changes
    /// </summary>
    public class AstrocolibriOptions : BaseINPC, INotifyPropertyChanged {
        private readonly PluginOptionsAccessor optionsAccessor;
        public readonly IProfileService profileService;
        private readonly Astrocolibri ac;

        public AstrocolibriOptions(Astrocolibri AC, IProfileService profileService) {
            var guid = PluginOptionsAccessor.GetAssemblyGuid(typeof(AstrocolibriOptions));
            if (guid == null) {
                throw new Exception($"Guid not found in assembly metadata");
            }
            this.profileService = profileService;
            ac = AC;

            optionsAccessor = new PluginOptionsAccessor(profileService, guid.Value);

            InitializeOptions();
        }

        public void InitializeOptions() {
            waitMinMinutes = optionsAccessor.GetValueInt32(nameof(WaitMinMinutes), Properties.Settings.Default.WaitMinMinutes);
            uid = optionsAccessor.GetValueString(nameof(Uid), Properties.Settings.Default.AstroColibriUID);
            apiUrl = optionsAccessor.GetValueString(nameof(ApiUrl), Properties.Settings.Default.AstroColibriAPIUrl);
            jsonFilePath = optionsAccessor.GetValueString(nameof(JSONFilePath), Environment.ExpandEnvironmentVariables(Properties.Settings.Default.JSONFilePath));
            keepFilesDays = optionsAccessor.GetValueInt32(nameof(KeepFilesDays), Properties.Settings.Default.KeepFilesDays);
            minAltitude = optionsAccessor.GetValueDouble(nameof(MinAltitude), Properties.Settings.Default.MinAltitude);
            testMode = optionsAccessor.GetValueBoolean(nameof(TestMode), Properties.Settings.Default.Testmode);
            dsoTemplate = optionsAccessor.GetValueString(nameof(DsoTemplate), Properties.Settings.Default.DSOTemplate);
        }

        private string uid;

        public string Uid {
            get => uid;
            set {
                if (uid != value) {
                    Logger.Debug($"Set User ID={value}");
                    uid = value;
                    optionsAccessor.SetValueString(nameof(Uid), uid);
                    ac.RaisePropertyChanged();
                }
            }
        }

        private int waitMinMinutes;

        public int WaitMinMinutes {
            get => waitMinMinutes;
            set {
                if (value < 10)
                    value = 10;
                if (waitMinMinutes != value) {
                    Logger.Debug($"Set Wait minimum minutes={value}");
                    waitMinMinutes = value;
                    optionsAccessor.SetValueInt32(nameof(WaitMinMinutes), waitMinMinutes);
                    ac.RaisePropertyChanged();
                }
            }
        }

        private string apiUrl;

        public string ApiUrl {
            get {
                return Properties.Settings.Default.AstroColibriAPIUrl;
            }
            set {
                if (apiUrl != value) {
                    Logger.Debug($"Set Astro-Colibri API URL={value}");
                    apiUrl = value;
                    Properties.Settings.Default.AstroColibriAPIUrl = apiUrl;
                    CoreUtil.SaveSettings(Properties.Settings.Default);
                    ac.RaisePropertyChanged();
                }
            }
        }

        private string webUrl;

        public string WebUrl {
            get {
                return Properties.Settings.Default.AstroColibriWebUrl;
            }
            set {
                if (webUrl != value) {
                    Logger.Debug($"Set Astro-Colibri Web URL={value}");
                    webUrl = value;
                    Properties.Settings.Default.AstroColibriWebUrl = webUrl;
                    CoreUtil.SaveSettings(Properties.Settings.Default);
                    ac.RaisePropertyChanged();
                }
            }
        }

        private string jsonFilePath;

        public string JSONFilePath {
            get => jsonFilePath;
            set {
                if (value == "")
                    value = Environment.ExpandEnvironmentVariables(Properties.Settings.Default.JSONFilePath);
                if (jsonFilePath != value) {
                    Logger.Debug($"Set JSON File Path={value}");
                    jsonFilePath = value;
                    optionsAccessor.SetValueString(nameof(JSONFilePath), jsonFilePath);
                    ac.RaisePropertyChanged();
                }
            }
        }

        private int keepFilesDays;

        public int KeepFilesDays {
            get => keepFilesDays;
            set {
                if (value < 1)
                    value = 1;
                if (keepFilesDays != value) {
                    Logger.Debug($"Set Keep Files Days={value}");
                    keepFilesDays = value;
                    optionsAccessor.SetValueInt32(nameof(KeepFilesDays), keepFilesDays);
                    ac.RaisePropertyChanged();
                }
            }
        }

        private double minAltitude;

        public double MinAltitude {
            get => minAltitude;
            set {
                if (value < 0)
                    value = 0;
                if (minAltitude != value) {
                    Logger.Debug($"Set Minimum Altitude={value}");
                    minAltitude = value;
                    optionsAccessor.SetValueDouble(nameof(MinAltitude), minAltitude);
                    ac.RaisePropertyChanged();
                }
            }
        }

        private bool testMode;

        public bool TestMode {
            get => testMode;
            set {
                if (testMode != value) {
                    Logger.Debug($"Set Testmode={value}");
                    testMode = value;
                    optionsAccessor.SetValueBoolean(nameof(TestMode), testMode);
                    ac.RaisePropertyChanged();
                }
            }
        }

        private string dsoTemplate;

        public string DsoTemplate {
            get => dsoTemplate;
            set {
                if (dsoTemplate != value) {
                    Logger.Debug($"Set DSO Template={value}");
                    dsoTemplate = value;
                    optionsAccessor.SetValueString(nameof(DsoTemplate), dsoTemplate);
                    ac.RaisePropertyChanged();
                }
            }
        }
    }
}