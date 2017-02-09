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
    }
}
