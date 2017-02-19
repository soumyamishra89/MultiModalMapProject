using BingMapsRESTToolkit;
using MultiModalMapProject.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiModalMapProject.SpeechUtil
{
    // this class contains the parameters required for getting route.
    public class RouteParameters
    {
        public static RouteParameters INSTANCE = new RouteParameters();

        // to location in coordinate
        public Coordinate toCLocation { get; set; }

        // from location in coordinate
        public Coordinate fromCLocation { get; set; }

        // the to location
        public string toLocation { get; set; }

        // the from location
        public string fromLocation { get; set; }

        // routing paramaeters. These parameters can be changed basedon user preference
        public DistanceUnitType distanceUnits { get; set; }
        public TravelModeType travelMode { get; set; }
        public RouteOptimizationType optimize { get; set; }
        private RouteParameters()
        {
            this.toLocation = "";
            this.fromLocation = "";
            this.distanceUnits = DistanceUnitType.KM;
            this.travelMode = TravelModeType.Driving;
            this.optimize = RouteOptimizationType.TimeWithTraffic;
        }

        // sets the default values of the parameters
        internal void clear()
        {
            this.toCLocation = null;
            this.fromCLocation = null;
            this.toLocation = "";
            this.fromLocation = "";
            this.distanceUnits = DistanceUnitType.KM;
            this.travelMode = TravelModeType.Driving;
            this.optimize = RouteOptimizationType.TimeWithTraffic;
        }

        // creates a route option to be used in route request REST api call
        internal RouteOptions getRouteOptions()
        {
            return new RouteOptions()
            {
                DistanceUnits = this.distanceUnits,
                TravelMode = this.travelMode,
                Optimize = this.optimize,
                RouteAttributes = new List<RouteAttributeType>() { RouteAttributeType.All }
            };

        }
        
        internal string getMissingInfoMessage()
        {
            // if only one of the location is avaliable, then the system can prompt the user to provide the other location
            if (isRouteInformationInComplete())
            {
                if (string.IsNullOrEmpty(toLocation) || toCLocation == null)
                {
                    return string.Format(SystemMessages.MISSING_LOCATION, "To");
                }
                else if (string.IsNullOrEmpty(fromLocation) || fromCLocation == null)
                {
                    return string.Format(SystemMessages.MISSING_LOCATION, "From");
                }
            }
            return "";
        }

        // this method tells if the address is available. 
        // this method is used when isRouteInformationInComplete() returns true. 
        // this helps in knowing if to and from location is available as string or as coordinates
        internal Boolean isAddressAvailable()
        {
            return !string.IsNullOrEmpty(fromLocation) && !string.IsNullOrEmpty(toLocation);
        }

        internal string getTravelingModeChangeMessage()
        {
            return string.Format(SystemMessages.CHANGE_TRAVELMODE, StaticVariables.travelModeTypes);
        }

        // checks if all the required information for route finding is avalaible. It returns true when one of them is available only. If both to and from location is empty, then it does not consider the information to be incomplete.
        internal bool isRouteInformationInComplete()
        {
            return ((string.IsNullOrEmpty(toLocation)  && (!string.IsNullOrEmpty(fromLocation)) || (fromCLocation != null && toCLocation == null)) || ((!string.IsNullOrEmpty(toLocation) && string.IsNullOrEmpty(fromLocation)) || (fromCLocation == null && toCLocation != null)));
        }

        // this tells if all the required field is available for finding route info. 
        // doing a negation on isRouteInformationInComplete() would not give same result as isRouteInformationInComplete() gives false when both to and from location are unavailable. 
        // In that case the condition would fail as negating isRouteInformationInComplete() would give true value meaning Route Information is Complete but in actual nothing is available.
        internal bool isRouteInformationComplete()
        {
            return (!string.IsNullOrEmpty(toLocation) || toCLocation != null) && (!string.IsNullOrEmpty(fromLocation) || fromCLocation != null);
        }
    }
}
