using System;
using BingMapsRESTToolkit;
using System.Collections.Generic;
using System.Windows.Media;

namespace MultiModalMapProject.Util
{
    public static class StaticVariables
    {
        public static string bingMapSessionKey;
        // search radius in KM for nearby places
        public static double defaultSearchRadius = 1;
        // default values for zooming in the map based on hand gesture and speech command
        public static int zoominFactor = 2;
        // default values for zooming out the map based on hand gesture and speech command
        public static int zoomoutFactor = 2;
        // Due to some alignment problem, the Y coordinate is shifted. Hence this offset tries to place the point exactly on the kinect hand position
        public static double handPointOffsetY = 300;

        public static Microsoft.Maps.MapControl.WPF.Location defaultCenter = new Microsoft.Maps.MapControl.WPF.Location(52.50141, 13.40233);

        /*********************************************************************************************/
        // routing paramaeters. These parameters can be changed basedon user preference
        public static DistanceUnitType distanceUnits = DistanceUnitType.KM;
        public static TravelModeType travelMode = TravelModeType.Driving;
        public static RouteOptimizationType optimize = RouteOptimizationType.TimeWithTraffic;
        /**********************************************************************************************/
        // all the travel mode type available on the map
        public static readonly string[] travelModeTypes = new string[]{ TravelModeType.Driving.ToString(), TravelModeType.Transit.ToString(), TravelModeType.Walking.ToString() };
        // specifies the color to be used to draw route path between two points
        public static Color routePathColor = Colors.Blue;
        

        public static Dictionary<string, int> bingPOISearchEntityNameToEntityId = new Dictionary<string, int>();
        static StaticVariables()
        {
            
            bingPOISearchEntityNameToEntityId.Add("ATM", 3578);
            bingPOISearchEntityNameToEntityId.Add("Train Station", 4013);
            bingPOISearchEntityNameToEntityId.Add("Bus Station", 4170);
            bingPOISearchEntityNameToEntityId.Add("Airport", 4581);
            bingPOISearchEntityNameToEntityId.Add("Grocery Store", 5400);
            bingPOISearchEntityNameToEntityId.Add("Restaurant", 5800);
            bingPOISearchEntityNameToEntityId.Add("Nightlife", 5813);
            bingPOISearchEntityNameToEntityId.Add("Shopping", 6512);
            bingPOISearchEntityNameToEntityId.Add("Hotel", 7011);
            bingPOISearchEntityNameToEntityId.Add("Bank", 6000);
            bingPOISearchEntityNameToEntityId.Add("Hospital", 8060);
            bingPOISearchEntityNameToEntityId.Add("School", 8211);
            bingPOISearchEntityNameToEntityId.Add("Library", 8231);
            bingPOISearchEntityNameToEntityId.Add("Museum", 8410);
            bingPOISearchEntityNameToEntityId.Add("Pharmacy", 9565);
            bingPOISearchEntityNameToEntityId.Add("Coffee", 9996);

        }
    }
}
