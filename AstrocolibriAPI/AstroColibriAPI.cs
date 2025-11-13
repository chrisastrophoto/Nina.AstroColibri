using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NINA.Core.Utility.Http;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Astrometry;
using NINA.Astrometry.RiseAndSet;
using System.Windows.Input;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Diagnostics.Eventing.Reader;
using OxyPlot.Axes;

namespace ChristophNieswand.NINA.Astrocolibri.AstrocolibriAPI {

    public class AstroColibriAPI {

        public AstroColibriAPI() {
            LastTransientCheck = DateTime.Now.Subtract(new TimeSpan(0, Astrocolibri.AstroColibriOptions.WaitMinMinutes, 0, 0));
            CoreUtil.DirectoryCleanup(Astrocolibri.AstroColibriOptions.JSONFilePath, new TimeSpan(-Astrocolibri.AstroColibriOptions.KeepFilesDays, 0, 0, 0));
            ACEvents = new AstroColibriEvents();
            HasNoTransient = true;
            RunLatestTransientsCommand = new GalaSoft.MvvmLight.Command.RelayCommand(DoLatestTransients);
        }

        public DateTime LastTransientCheck { get; private set; }

        public AstroColibriEvents ACEvents { get; private set; }

        public Boolean HasNoTransient { get; set; }

        public DeepSkyObject LatestTransient { get; private set; }

        public ICommand RunLatestTransientsCommand { get; }

        //public CustomHorizon Horizon { get; private set; }

        private async Task<string> CallAPIGetAsync(string endpoint, params object[] pars) {
            string resp = null;
            string ep = Astrocolibri.AstroColibriOptions.ApiUrl + (Astrocolibri.AstroColibriOptions.ApiUrl.EndsWith("/") ? "" : "/") + endpoint;
            HttpGetRequest req = new HttpGetRequest(ep, pars);
            System.Threading.CancellationToken ct = new System.Threading.CancellationToken();
            resp = (await Task.Run(() => req.Request(ct)));

            return resp;
        }

        private async Task<string> CallAPIPostAsync(string endpoint, string body) {
            string resp = null;
            string ep = Astrocolibri.AstroColibriOptions.ApiUrl + (Astrocolibri.AstroColibriOptions.ApiUrl.EndsWith("/") ? "" : "/") + endpoint;
            HttpPostRequest req = new HttpPostRequest(ep, body, "application/json");
            System.Threading.CancellationToken ct = new System.Threading.CancellationToken();
            resp = (await Task.Run(() => req.Request(ct)));

            return resp;
        }

        public string KnownSources() {
            string resp = null;

            try {
                resp = (Task.Run(() => CallAPIGetAsync("known_sources"))).Result;
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            return resp;
        }

        public EventResponse EventCall(string trigger_id, string now) {
            EventResponse evr = null;

            try {
                object[] pars = new object[1];
                pars[0] = trigger_id;
                string resp = (Task.Run(() => CallAPIGetAsync("event?trigger_id={0}", pars))).Result;
                SaveEventToFile("Event_" + now + "_" + trigger_id, resp);
                evr = JsonConvert.DeserializeObject<EventResponse>(resp);
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            return evr;
        }

        public VoeventResponse VoeventCall(string trigger_id, string now) {
            VoeventResponse evr = null;

            try {
                object[] pars = new object[1];
                pars[0] = trigger_id;
                string resp = (Task.Run(() => CallAPIGetAsync("voevent?trigger_id={0}&json=1", pars))).Result;
                SaveEventToFile("Voevent_" + now + "_" + trigger_id, resp);
                evr = JsonConvert.DeserializeObject<VoeventResponse>(resp);
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            return evr;
        }

        public VisibilityPlotResponse VisibilityPlot(EventResponse evr, string now) {
            VisibilityPlotResponse vis = null;

            try {
                object[] pars = new object[6];
                pars[0] = evr.ra;
                pars[1] = evr.dec;
                pars[2] = Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Latitude;
                pars[3] = Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Longitude;
                pars[4] = Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Elevation;
                pars[5] = DateTime.Now.Add(new TimeSpan(0, -12, 0, 0)).ToString("yyyy-MM-dd");
                string resp = (Task.Run(() => CallAPIGetAsync("visibility_plots?pos=[{0},{1}]&pos_obs=[{2},{3},{4}]&date={5}", pars))).Result;
                vis = JsonConvert.DeserializeObject<VisibilityPlotResponse>(resp);
                SaveEventToFile("Visibility_" + now + "_" + evr.trigger_id, resp);
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            return vis;
        }

        public VisibilityPlotDetailedResponse VisibilityPlotDetailed(EventResponse evr, string now) {
            VisibilityPlotDetailedResponse vis = null;

            try {
                object[] pars = new object[7];
                pars[0] = evr.ra;
                pars[1] = evr.dec;

                pars[2] = Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Latitude;
                pars[3] = Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Longitude;
                pars[4] = Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Elevation;
                pars[5] = DateTime.Now.Add(new TimeSpan(0, -12, 0, 0)).ToString("yyyy-MM-dd");
                pars[6] = evr.source_name;
                string resp = (Task.Run(() => CallAPIGetAsync("visibility_plots_detailed?pos_src=[{0},{1}]&pos_obs=[{2},{3},{4}]&twilight_level=2&date={5}&source_name={6}", pars))).Result;
                vis = JsonConvert.DeserializeObject<VisibilityPlotDetailedResponse>(resp);
                SaveEventToFile("VisibilityDetail_" + now + "_" + evr.trigger_id, resp);
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            return vis;
        }

        /*
        public void LatestTransients() {
            string resp = null;

            int diff = (int)DateTime.Now.Subtract(LastTransientCheck).TotalMinutes;
            int soll = Astrocolibri.AstroColibriOptions.WaitMinMinutes;

            if (Astrocolibri.AstroColibriOptions.TestMode)
                diff = 10000;

            if (diff >= soll) {
                Logger.Info("Checking for latest Events from Astro-Colibri");
                LatestTransientsBody eb = new LatestTransientsBody {
                    uid = Astrocolibri.AstroColibriOptions.Uid,
                    return_format = "json",
                    time_range = new LatestTransientsBodyMaxMin {
                        max = DateTime.Now,
                        min = LastTransientCheck
                    }
                };
                // Reset last check for transients
                LastTransientCheck = DateTime.Now;
                LatestTransient = null;
                HasNoTransient = true;

                JsonSerializerSettings SerSet = new JsonSerializerSettings();
                SerSet.Formatting = Formatting.Indented;
                SerSet.NullValueHandling = NullValueHandling.Ignore;

                string body = JsonConvert.SerializeObject(eb, SerSet);

                try {
                    resp = Astrocolibri.AstroColibriOptions.TestMode
                        ? ReadEventFromResource("LatestEvents_TestSet.json")
                        : (Task.Run(() => CallAPIPostAsync("latest_transients", body))).Result;

                    string now = DateTime.Now.ToString("yyyyMMddHHmmss");

                    LatestTransientsResponse ltr = JsonConvert.DeserializeObject<LatestTransientsResponse>(resp);
                    if (ltr != null && ltr.voevents.Count > 0) {
                        SaveEventToFile("LatestEvents_" + now, resp);
                        Notification.ShowSuccess("Got Events from Astro-Colibri");
                        Logger.Info("Got Events from Astro-Colibri");
                        List<EventResponse> verl = (List<EventResponse>)ltr.voevents;
                        int i = 0;
                        foreach (EventResponse er in verl) {
                            VoeventResponse ver = null;
                            if (!Astrocolibri.AstroColibriOptions.TestMode)
                                ver = VoeventCall(er.trigger_id, now);
                            string err = er.err + " " + "deg";
                            if (err == "None deg" || err == "-1.0 deg")
                                if (ver != null) {
                                    try {
                                        string error = ver.voevent.WhereWhen.ObsDataLocation.ObservationLocation.AstroCoords.Position2D.Error2Radius;
                                        string unit = ver.voevent.WhereWhen.ObsDataLocation.ObservationLocation.AstroCoords.Position2D.unit;
                                        if (error != null && error != "" && unit != null && unit != "")
                                            err = error + " " + unit;
                                    } catch (Exception ex) {
                                    }
                                }
                            if (err == "None deg" || err == "-1.0 deg")
                                err = "";
                            double dec = er.dec;
                            double ra = er.ra;
                            //VisibilityPlotResponse vis = VisibilityPlot(er, now);

                            VisibilityPlotDetailedResponse visDet = null;
                            if (!Astrocolibri.AstroColibriOptions.TestMode)
                                visDet = VisibilityPlotDetailed(er, now);
                            DeepSkyObject dso = new DeepSkyObject(er.trigger_id, new Coordinates(er.ra, er.dec, Epoch.J2000, Coordinates.RAType.Degrees), "", Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Horizon);
                            dso.Name = er.source_name;
                            if (Astrocolibri.AstroColibriOptions.TestMode) {
                                switch (er.source_name) {
                                    case "Visible Target":
                                        MakeVisibleNow(dso);
                                        break;

                                    case "Invisible Target":
                                        MakeInvisible(dso);
                                        break;

                                    case "Never visible Target":
                                        MakeNeverVisible(dso);
                                        break;

                                    default:
                                        break;
                                }
                                dec = dso.Coordinates.Dec;
                                ra = dso.Coordinates.RADegrees;
                            }
                            double[] altaz = GetAltitude(ra, dec);
                            double alt = altaz[0];
                            if (IsNeverVisible(dec)) {
                                Notification.ShowSuccess("Event " + er.source_name + " is NEVER visible and will be ignored! Declination: " + dec);
                                Logger.Info("Event " + er.source_name + " is NEVER visible and will be ignored! Declination: " + dec);
                            } else {
                                if (IsVisibleNow(altaz)) {
                                    Notification.ShowWarning("Event " + er.source_name + " is visible! Altidude now: " + alt, new TimeSpan(0, 10, 0, 0));
                                    Logger.Info("Event " + er.source_name + " is visible! Altitude now:" + alt);
                                    InsertEvent(i, dso, er, visDet, err, "Visible now");
                                    i++;
                                    LatestTransient = dso;
                                    HasNoTransient = false;
                                } else {
                                    if (IsVisibleAtNight(dso)) {
                                        Notification.ShowInformation("Event " + er.source_name + " is currently not visible, but becomes visible during nighttime: Altidude now: " + alt, new TimeSpan(0, 10, 0, 0));
                                        Logger.Info("Event " + er.source_name + " is currently not visible, but becomes visible during nighttime: Altitude now:" + alt);
                                        InsertEvent(i, dso, er, visDet, err, "Visible at night");
                                        i++;
                                    } else {
                                        Notification.ShowSuccess("Event " + er.source_name + " is currently not visible and is visible only during daytime: Altidude now: " + alt);
                                        Logger.Info("Event " + er.source_name + " is currently not visible and is visible only during daytime: Altitude now:" + alt);
                                    }
                                }
                            }
                        }
                        Astrocolibri.AstroColibriDockable.UpdateTargetInfo();
                    } else {
                        Notification.ShowInformation("Got NO Events from Astro-Colibri");
                        Logger.Info("Got NO Events from Astro-Colibri");
                    }
                } catch (Exception ex) {
                    Logger.Error(ex);
                }
            }
            return;
        }
        */

        public void LatestTransients() {
            int diff = (int)DateTime.Now.Subtract(LastTransientCheck).TotalMinutes;
            int soll = Astrocolibri.AstroColibriOptions.WaitMinMinutes;

            if (Astrocolibri.AstroColibriOptions.TestMode)
                diff = 10000;

            if (diff >= soll) {
                LatestTransients(LastTransientCheck);

                LastTransientCheck = DateTime.Now;
                LatestTransient = null;
                HasNoTransient = true;
            }
            return;
        }

        public void LatestTransientsManual() {
            int latestMinutes = Astrocolibri.AstroColibriOptions.CheckMinutes;
            DateTime fromDate = DateTime.Now.AddMinutes(-latestMinutes);
            LatestTransients(fromDate);

            return;
        }

        public void LatestTransients(DateTime fromDate) {
            string resp = null;

            try {
                string body = null;

                if (!Astrocolibri.AstroColibriOptions.TestMode) {
                    Logger.Info("Checking for latest Events from Astro-Colibri since " + fromDate.ToString());
                    LatestTransientsBody eb = new LatestTransientsBody {
                        uid = Astrocolibri.AstroColibriOptions.Uid,
                        return_format = "json",
                        time_range = new LatestTransientsBodyMaxMin {
                            max = DateTime.Now,
                            min = fromDate
                        }
                    };

                    JsonSerializerSettings SerSet = new JsonSerializerSettings();
                    SerSet.Formatting = Formatting.Indented;
                    SerSet.NullValueHandling = NullValueHandling.Ignore;

                    body = JsonConvert.SerializeObject(eb, SerSet);

                    resp = (Task.Run(() => CallAPIPostAsync("latest_transients", body))).Result;
                } else {
                    resp = ReadEventFromResource("LatestEvents_TestSet.json");
                }
                string now = DateTime.Now.ToString("yyyyMMddHHmmss");

                LatestTransientsResponse ltr = JsonConvert.DeserializeObject<LatestTransientsResponse>(resp);
                if (ltr != null && ltr.voevents.Count > 0) {
                    SaveEventToFile("LatestEvents_" + now, resp);
                    Notification.ShowSuccess("Got Events from Astro-Colibri");
                    Logger.Info("Got Events from Astro-Colibri");
                    List<EventResponse> verl = (List<EventResponse>)ltr.voevents;
                    int i = 0;
                    foreach (EventResponse er in verl) {
                        VoeventResponse ver = null;
                        if (!Astrocolibri.AstroColibriOptions.TestMode)
                            ver = VoeventCall(er.trigger_id, now);
                        string err = er.err + " " + "deg";
                        if (err == "None deg" || err == "-1.0 deg")
                            if (ver != null) {
                                try {
                                    string error = ver.voevent.WhereWhen.ObsDataLocation.ObservationLocation.AstroCoords.Position2D.Error2Radius;
                                    string unit = ver.voevent.WhereWhen.ObsDataLocation.ObservationLocation.AstroCoords.Position2D.unit;
                                    if (error != null && error != "" && unit != null && unit != "")
                                        err = error + " " + unit;
                                } catch (Exception ex) {
                                }
                            }
                        if (err == "None deg" || err == "-1.0 deg")
                            err = "";
                        double dec = er.dec;
                        double ra = er.ra;
                        //VisibilityPlotResponse vis = VisibilityPlot(er, now);

                        VisibilityPlotDetailedResponse visDet = null;
                        if (!Astrocolibri.AstroColibriOptions.TestMode)
                            visDet = VisibilityPlotDetailed(er, now);
                        DeepSkyObject dso = new DeepSkyObject(er.trigger_id, new Coordinates(er.ra, er.dec, Epoch.J2000, Coordinates.RAType.Degrees), "", Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Horizon);
                        dso.Name = er.source_name;
                        if (Astrocolibri.AstroColibriOptions.TestMode) {
                            switch (er.source_name) {
                                case "Visible Target":
                                    MakeVisibleNow(dso);
                                    break;

                                case "Invisible Target":
                                    MakeInvisible(dso);
                                    break;

                                case "Never visible Target":
                                    MakeNeverVisible(dso);
                                    break;

                                default:
                                    break;
                            }
                            dec = dso.Coordinates.Dec;
                            ra = dso.Coordinates.RADegrees;
                        }
                        double[] altaz = GetAltitude(ra, dec);
                        double alt = altaz[0];
                        if (IsNeverVisible(dec)) {
                            Notification.ShowSuccess("Event " + er.source_name + " is NEVER visible and will be ignored! Declination: " + dec);
                            Logger.Info("Event " + er.source_name + " is NEVER visible and will be ignored! Declination: " + dec);
                        } else {
                            if (IsVisibleNow(altaz)) {
                                Notification.ShowWarning("Event " + er.source_name + " is visible! Altidude now: " + alt, new TimeSpan(0, 10, 0, 0));
                                Logger.Info("Event " + er.source_name + " is visible! Altitude now:" + alt);
                                InsertEvent(i, dso, er, visDet, err, "Visible now");
                                i++;
                                LatestTransient = dso;
                                HasNoTransient = false;
                            } else {
                                if (IsVisibleAtNight(dso)) {
                                    Notification.ShowInformation("Event " + er.source_name + " is currently not visible, but becomes visible during nighttime: Altidude now: " + alt, new TimeSpan(0, 10, 0, 0));
                                    Logger.Info("Event " + er.source_name + " is currently not visible, but becomes visible during nighttime: Altitude now:" + alt);
                                    InsertEvent(i, dso, er, visDet, err, "Visible at night");
                                    i++;
                                } else {
                                    Notification.ShowSuccess("Event " + er.source_name + " is currently not visible and is visible only during daytime: Altidude now: " + alt);
                                    Logger.Info("Event " + er.source_name + " is currently not visible and is visible only during daytime: Altitude now:" + alt);
                                }
                            }
                        }
                    }
                    Astrocolibri.AstroColibriDockable.UpdateTargetInfo();
                } else {
                    Notification.ShowInformation("Got NO Events from Astro-Colibri");
                    Logger.Info("Got NO Events from Astro-Colibri");
                }
            } catch (Exception ex) {
                Logger.Error(ex);
            }

            return;
        }

        private void InsertEvent(int i, DeepSkyObject dso, EventResponse er, VisibilityPlotDetailedResponse visDet, string err, string visibility) {
            if (Astrocolibri.AstroColibriOptions.TestMode)
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                    ACEvents.Insert(i, new AstroColibriEvent(dso, er.trigger_id, Astrocolibri.AstroColibriOptions.WebUrl, Astrocolibri.AstroColibriOptions.WebUrl, Astrocolibri.AstroColibriOptions.WebUrl, Astrocolibri.AstroColibriOptions.WebUrl, er.classification, er.type, er.alert_type, er.time, er.transient_flux, er.transient_flux_units, err, visibility));
                });
            else
                System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                    ACEvents.Insert(i, new AstroColibriEvent(dso, er.trigger_id, visDet.img_url, visDet.results_url, er.url, er.event_url, er.classification, er.type, er.alert_type, er.time, er.transient_flux, er.transient_flux_units, err, visibility));
                });
        }

        public void RemoveEvent(DeepSkyObject dso) {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                AstroColibriEvent toRemove = null;
                foreach (AstroColibriEvent a in ACEvents) {
                    if (a.DSO == dso) { toRemove = a; break; }
                }
                if (toRemove != null) { ACEvents.Remove(toRemove); }
            });
        }

        public void RemoveAllEvents() {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                ACEvents = new AstroColibriEvents();
            });
        }

        private void DoLatestTransients() {
            Task.Run(() => CallLatestTransients());
        }

        private async Task CallLatestTransients() {
            await Task.Run(() => LatestTransientsManual());
        }

        private void SaveEventToFile(string name, string resp) {
            Directory.CreateDirectory(Astrocolibri.AstroColibriOptions.JSONFilePath);
            File.WriteAllText(Path.Combine(Astrocolibri.AstroColibriOptions.JSONFilePath, name + ".json"), resp);
        }

        private static string ReadEventFromFile(string name) {
            return File.ReadAllText(Path.Combine(Astrocolibri.AstroColibriOptions.JSONFilePath, name));
        }

        private string ReadEventFromResource(string resource) {
            string json = "";

            string resourceName = Assembly.GetAssembly(this.GetType()).GetManifestResourceNames().Single(str => str.EndsWith(resource));

            using (Stream stream = Assembly.GetAssembly(this.GetType()).GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream)) {
                json = reader.ReadToEnd();
            }
            return json;
        }

        private static double[] GetAltitude(double ra, double dec) {
            return GetAltitude(ra, dec, null);
        }

        private static double[] GetAltitude(double ra, double dec, DateTime? dt) {
            double LST;
            if (dt != null) {
                LST = AstroUtil.GetLocalSiderealTime((DateTime)dt, Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Longitude);
            } else {
                LST = AstroUtil.GetLocalSiderealTimeNow(Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Longitude);
            }
            double HA = AstroUtil.GetHourAngle(LST, AstroUtil.DegreesToHours(ra));

            double HADeg = AstroUtil.HoursToDegrees(HA);

            double alt = AstroUtil.GetAltitude(HADeg, Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Latitude, dec);
            double az = AstroUtil.GetAzimuth(HADeg, Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Longitude, Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Latitude, dec);

            double[] altaz = new double[2];
            altaz[0] = alt;
            altaz[1] = az;

            return altaz;
        }

        private static bool IsNeverVisible(double dec) {
            bool nv = false;
            double lat = Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Latitude;
            double minalt = Astrocolibri.AstroColibriOptions.MinAltitude;
            double za = 90.0 - minalt;
            if (lat >= 0) {
                nv = (dec <= lat - za) || (dec >= lat + za && minalt >= 2 * lat);
            } else {
                nv = (dec >= lat + za) || (dec <= lat - za && minalt >= -2 * lat);
            }
            return nv;
        }

        private static bool IsVisibleNow(double[] altaz) {
            double halt = Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Horizon.GetAltitude(altaz[1]);

            return altaz[0] > Astrocolibri.AstroColibriOptions.MinAltitude && altaz[0] > halt;
        }

        private static bool IsVisibleAtNight(DeepSkyObject dso) {
            // This should ideally return true, when the target is above horizon and above minalt at some time between start and end of nautical twilight

            RiseAndSetEvent ev = AstroUtil.GetNauticalNightTimes(DateTime.Now, Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Latitude, Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Longitude);

            DateTime? start = ev.Set;
            DateTime? end = ev.Rise;
            if (start != null && end != null) {
                double startd = DateTimeAxis.ToDouble(start);
                double endd = DateTimeAxis.ToDouble(end);

                List<OxyPlot.DataPoint> alts = dso.Altitudes;
                List<OxyPlot.DataPoint> hor = dso.Horizon;
                double minalt = Astrocolibri.AstroColibriOptions.MinAltitude;

                List<OxyPlot.DataPoint>.Enumerator altenum = alts.GetEnumerator();
                List<OxyPlot.DataPoint>.Enumerator horenum = hor.GetEnumerator();
                OxyPlot.DataPoint altdp = altenum.Current;
                OxyPlot.DataPoint hordp = horenum.Current;
                // We ignore the first datapoint
                while (altenum.MoveNext()) {
                    horenum.MoveNext();
                    altdp = altenum.Current;
                    hordp = horenum.Current;
                    if (altdp.X > startd && altdp.X < endd && altdp.Y > minalt && altdp.Y > hordp.Y) {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void MakeVisibleNow(DeepSkyObject dso) {
            double LST = AstroUtil.GetLocalSiderealTimeNow(Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Longitude);
            double ra = AstroUtil.HoursToDegrees(LST);
            double dec = 0.0; // mus be better
            dso.Coordinates = new Coordinates(ra, dec, Epoch.J2000, Coordinates.RAType.Degrees);
        }

        private static void MakeInvisible(DeepSkyObject dso) {
            // Might become visible during daytime or nighttime, depending on when you run the test
            double LST = AstroUtil.GetLocalSiderealTimeNow(Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Longitude);
            double ra = AstroUtil.EuclidianModulus(AstroUtil.HoursToDegrees(LST) + 180, 360);
            double dec = 0.0;
            dso.Coordinates = new Coordinates(ra, dec, Epoch.J2000, Coordinates.RAType.Degrees);
        }

        private static void MakeNeverVisible(DeepSkyObject dso) {
            double lat = Astrocolibri.AstroColibriOptions.profileService.ActiveProfile.AstrometrySettings.Latitude;
            double minalt = Astrocolibri.AstroColibriOptions.MinAltitude;
            double dec = 0.0;
            double ra = dso.Coordinates.RADegrees;

            if (lat > 0)
                dec = lat - 90.0 + minalt - 1.0;
            else
                dec = lat + 90 - minalt + 1.0;

            dso.Coordinates = new Coordinates(ra, dec, Epoch.J2000, Coordinates.RAType.Degrees);
        }
    }
}