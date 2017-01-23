using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiModalMapProject.SpeechUtil
{
    // contains the different types of entities in the recognised speech from LUIS.
    // LUIS returns a json result with list of intents and entities for the recognised speech
    public class LuisEntityTypes
    {
        // the predefined entity 'NUMBER' in LUIS system has the this type
        public const string NUMBER = "builtin.number";

        // the type for geographic cities
        public const string CITY = "builtin.geography.city";

        // for famous architectural structures the type is this
        public const string ARCHSTRUCTURE = "builtin.encyclopedia.architecture.structure";

        public const string BINGENTITYTYPE = "bingentity";

        // here, there is classified as abstract location
        public const string ABSTRACTLOCATION = "abstractlocation";
    }
}
