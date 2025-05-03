using NINA.Astrometry;
using System;

namespace ChristophNieswand.NINA.Astrocolibri.AstrocolibriAPI {

    public class AstroColibriEvent {

        public AstroColibriEvent(DeepSkyObject _dso) {
            DSO = _dso;
        }

        public AstroColibriEvent(DeepSkyObject _dso, string trigger_id, string img_url, string results_url, string url, string event_url, string classification, string type, string alert_type, string time, string transient_flux, string transient_flux_units, string error) {
            DSO = _dso;
            Trigger_id = trigger_id;
            Img_url = img_url == "" || img_url == "" ? null : new Uri(img_url);
            Results_url = results_url == null || results_url == "" ? null : new Uri(results_url);
            Url = url == null || url == "" ? null : new Uri(url);
            Event_url = event_url == "" || event_url == "" ? null : new Uri(event_url);
            Classification = classification;
            Type = type;
            Alert_type = alert_type;
            Time = time;
            Transient_flux_units = transient_flux_units;
            Transient_flux = transient_flux_units == "" ? "" : transient_flux + " " + transient_flux_units;
            Error = error;
        }

        private string trigger_id;

        public string Trigger_id {
            get => trigger_id; set {
                trigger_id = value;
            }
        }

        private DeepSkyObject dso;

        public DeepSkyObject DSO {
            get => dso; set {
                dso = value;
            }
        }

        private Uri img_url;

        public Uri Img_url {
            get => img_url; set {
                img_url = value;
            }
        }

        private Uri event_url;

        public Uri Event_url {
            get => event_url; set {
                event_url = value;
            }
        }

        private Uri url;

        public Uri Url {
            get => url; set {
                url = value;
            }
        }

        private Uri results_url;

        public Uri Results_url {
            get => results_url; set {
                results_url = value;
            }
        }

        private string classification;

        public string Classification {
            get => classification; set {
                classification = value;
            }
        }

        private string type;

        public string Type {
            get => type; set {
                type = value;
            }
        }

        private string alert_type;

        public string Alert_type {
            get => alert_type; set {
                alert_type = value;
            }
        }

        private string time;

        public string Time {
            get => time; set {
                time = value;
            }
        }

        private string transient_flux;

        public string Transient_flux {
            get => transient_flux; set {
                transient_flux = value;
            }
        }

        private string transient_flux_units;

        public string Transient_flux_units {
            get => transient_flux_units; set {
                transient_flux_units = value == "" ? null : value;
            }
        }

        private string error;

        public string Error {
            get => error; set {
                error = value;
            }
        }
    }
}