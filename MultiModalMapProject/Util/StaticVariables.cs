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
        public static double handPointOffsetY = 30;

        public static Microsoft.Maps.MapControl.WPF.Location defaultCenter = new Microsoft.Maps.MapControl.WPF.Location(52.50141, 13.40233);

        /*********************************************************************************************/
        // routing paramaeters. These parameters can be changed basedon user preference
        public static DistanceUnitType distanceUnits = DistanceUnitType.KM;
        public static TravelModeType travelMode = TravelModeType.Driving;
        public static RouteOptimizationType optimize = RouteOptimizationType.TimeWithTraffic;
        /**********************************************************************************************/
        // all the travel mode type available on the map
        public static readonly string travelModeTypes = TravelModeType.Driving.ToString() + ", " + TravelModeType.Transit.ToString() + " or " + TravelModeType.Walking.ToString();
        // specifies the color to be used to draw route path between two points
        public static Color routePathColor = Colors.Blue;
        

        public static Dictionary<string, int> bingPOISearchEntityNameToEntityId = new Dictionary<string, int>();
        internal static int imageHeight = 60;
        internal static int imageWidth = 60;

        public static int POINumber = 1;

        static StaticVariables()
        {
            
            bingPOISearchEntityNameToEntityId.Add("atm", 3578);
            bingPOISearchEntityNameToEntityId.Add("train station", 4013);
            bingPOISearchEntityNameToEntityId.Add("bus station", 4170);
            bingPOISearchEntityNameToEntityId.Add("airport", 4581);
            bingPOISearchEntityNameToEntityId.Add("grocery store", 5400);
            bingPOISearchEntityNameToEntityId.Add("restaurant", 5800);
            bingPOISearchEntityNameToEntityId.Add("restaurants", 5800);
            bingPOISearchEntityNameToEntityId.Add("nightlife", 5813);
            bingPOISearchEntityNameToEntityId.Add("shopping", 6512);
            bingPOISearchEntityNameToEntityId.Add("hotel", 7011);
            bingPOISearchEntityNameToEntityId.Add("hotels", 7011);
            bingPOISearchEntityNameToEntityId.Add("bank", 6000);
            bingPOISearchEntityNameToEntityId.Add("banks", 6000);
            bingPOISearchEntityNameToEntityId.Add("hospital", 8060);
            bingPOISearchEntityNameToEntityId.Add("hospitals", 8060);
            bingPOISearchEntityNameToEntityId.Add("school", 8211);
            bingPOISearchEntityNameToEntityId.Add("schools", 8211);
            bingPOISearchEntityNameToEntityId.Add("library", 8231);
            bingPOISearchEntityNameToEntityId.Add("libraries", 8231);
            bingPOISearchEntityNameToEntityId.Add("museum", 8410);
            bingPOISearchEntityNameToEntityId.Add("museums", 8410);
            bingPOISearchEntityNameToEntityId.Add("pharmacy", 9565);
            bingPOISearchEntityNameToEntityId.Add("coffee", 9996);

        }
    }
}
