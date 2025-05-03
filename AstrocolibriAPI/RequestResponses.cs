using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChristophNieswand.NINA.Astrocolibri.AstrocolibriAPI {

    internal class RequestResponses {
    }

    public class LatestTransientsResponse {
        public IList<EventResponse> voevents { get; set; }
    }

    public class VoeventResponse {

        [JsonProperty("voe:VOEvent")]
        public Voevent voevent { get; set; }
    }

    public class Voevent {
        public string ivorn { get; set; }
        public What What { get; set; }
        public WhereWhen WhereWhen { get; set; }
    }

    public class What {
        public IList<NameValue> Param { get; set; }
    }

    public class NameValue {
        public string name { get; set; }
        public string value { get; set; }
        public string unit { get; set; }
        public string Description { get; set; }
    }

    public class WhereWhen {
        public ObsDataLocation ObsDataLocation { get; set; }
    }

    public class ObsDataLocation {
        public ObservationLocation ObservationLocation { get; set; }
    }

    public class ObservationLocation {
        public AstroCoords AstroCoords { get; set; }
    }

    public class AstroCoords {
        public Position2D Position2D { get; set; }
    }

    public class Position2D {
        public string unit { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public Value2 Value2 { get; set; }
        public string Error2Radius { get; set; }
    }

    public class Value2 {
        public string C1 { get; set; }
        public string C2 { get; set; }
    }

    public class EventResponse {
        public string trigger_id { get; set; }

        public float dec { get; set; }

        public float ra { get; set; }

        public string classification { get; set; }

        public string type { get; set; }

        public string alert_type { get; set; }

        public string url { get; set; }

        public string event_url { get; set; }

        public string time { get; set; }

        public string source_name { get; set; }

        public string transient_flux { get; set; }

        public string transient_flux_units { get; set; }

        public string err { get; set; }
    }

    public class VisibilityPlotResponse {
        public string img_url { get; set; }

        public string storage_url { get; set; }

        public string url { get; set; }
    }

    public class VisibilityPlotDetailedResponse {
        public string img_url { get; set; }

        public string results_url { get; set; }
    }
}