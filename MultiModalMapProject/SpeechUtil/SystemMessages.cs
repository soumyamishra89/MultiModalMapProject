using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiModalMapProject.SpeechUtil
{
    // this class contains the messages that the system shows to user in case of warnings or errors or asking for extra information
    public sealed class SystemMessages
    {
        // message to be displayed when asking for additional location information while showing route {0}= To or From
        public const string MISSING_LOCATION = "Provide {0} Location with syntax \"{0} Place name\"";

        // message to be displayed when asking for change of Travel Mode
        public const string CHANGE_TRAVELMODE = "You can choose a travel mode among {0}";

        // message when the location in SHOW Location is not found
        public const string NOLOCATION_MESSAGE = "Could not find the location \"{0}\"";

        // message when the Bing Entity for a place could not be found
        public const string NOENTITY_MESSAGE = "Could not understand the entity \"{0}\"";

        // message when the number is identified
        public const string UNIDENTIFIEDNUMBER_MESSAGE = "\"{0}\" is not a number. Please use a number";

        // empty message 
        public const string NOINTENT_MESSAGE = "Say something like \"show me Berlin\"";

        // no route message
        public const string NOROUTE_MESSAGE = "Could not find the route between \"{0}\" and \"{1}\"";

        // no poi message
        public const string NOPOI_MESSAGE = "No Places to show. Try a different location";
    }
}
