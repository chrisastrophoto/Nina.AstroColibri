using System;

namespace ChristophNieswand.NINA.Astrocolibri.AstrocolibriAPI {

    internal class RequestBodies {
    }

    public class LatestTransientsBody {
        public LatestTransientsBodyMaxMin time_range { get; set; }

        public string uid { get; set; }

        public string filter { get; set; }

        public string return_format { get; set; }
    }

    public class LatestTransientsBodyMaxMin {
        public DateTime max { get; set; }

        public DateTime min { get; set; }
    }
}