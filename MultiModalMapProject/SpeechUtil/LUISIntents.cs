using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiModalMapProject.SpeechUtil
{
    // contains all the intents from LUIS speech intent recognition.
    // The recognised speech from LUIS contains one of the intents from this enum class
    public static class LUISIntents
    {
        public const string SHOW_LOCATION = "SHOW LOCATION";
        public const string SHOW_NEARBY = "SHOW NEARBY";
        public const string ZOOM_IN = "ZOOM IN";
        public const string ZOOM_OUT = "ZOOM OUT";
        public const string SHOW_ROUTE = "ROUTE";
        public const string RESET = "RESET";
        public const string NONE = "NONE";
        public const string PAN = "PAN";
        public const string INSTRUCTIONS = "INSTRUCTIONS";
        public const string TRAVELMODE = "TRAVELMODE";
    }
}
