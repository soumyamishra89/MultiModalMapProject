using Microsoft.Kinect;
using Microsoft.Maps.MapControl.WPF.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using Microsoft.Maps.MapControl.WPF;
using BingMapsRESTToolkit;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Windows.Media;
// This class file contains related to Bing Maps
namespace MultiModalMapProject
{
    //author: @soumyamishra89
    public partial class MainWindow
    {
        private readonly string bingMapsKey = "0JbzNXxaqJam9uOChVzc~OL4ubOxhmU4QXRlhrkhI3g~Aj-KxTfYO5Xfec0xQ0nXJVLPJeYUeFSIRckkxz6CC0v-A9x_hTobSKLks9Rb6g3F";
        private string bingMapSessionKey;
        // search radius in KM for nearby places
        private double defaultSearchRadius = 1;
        int zoominFactor = 2;
        int zoomoutFactor = 2;

        /*********************************************************************************************/
        // routing paramaeters. These parameters can be changed basedon user preference
        private DistanceUnitType distanceUnits = DistanceUnitType.KM;
        private TravelModeType travelMode = TravelModeType.Driving;
        private RouteOptimizationType optimize = RouteOptimizationType.TimeWithTraffic;
        /**********************************************************************************************/
        LocationConverter locConv = new LocationConverter();
        
        // TODO to be removed
        private int zoomScalingValue = 19;
        // TODO to be removed
        // scales the increasing or decreasing distance between the hands to zoom in and zoom out respectively
        private void zoomInZoomOutMap(Joint leftHand, Joint rightHand)
        {
            double distance_between_hands = Math.Sqrt(Math.Pow(leftHand.Position.X - rightHand.Position.X, 2) + Math.Pow(leftHand.Position.Y - rightHand.Position.Y, 2));
            // leftHandY.Add(distance_between_hands * 19);

            int distance_in_integer = Convert.ToInt32(distance_between_hands * zoomScalingValue);
            Console.WriteLine("Distance between left and right hand: " + distance_in_integer);// if(distance_between_hands)
                                                                                              //if (distance_in_integer != referenceDistanceBetweenHands)
                                                                                              //{
                                                                                              //  myMap.ZoomLevel = distance_between_hands * zoomScalingValue;
                                                                                              //}

        }

        // zooms in the map based on a defined zoom factor
        private void zoominMap()
        {
            myMap.ZoomLevel = myMap.ZoomLevel + zoominFactor;
        }

        // zooms out the map based on a defined zoom factor
        private void zoomoutMap()
        {
            myMap.ZoomLevel = myMap.ZoomLevel - zoomoutFactor;
        }

        // returns a BingMapsRESTToolkit.Coordinate for a address from Bing Geocode. 
        // Can return empty coordinates
        private async Task<Coordinate> getLocationForAddress(string address)
        {
            Coordinate addressCoordinates = new Coordinate();
            // geocode request helps in coordinates of an address
            var geocodeRequest = new GeocodeRequest()
            {
                Query = address,
                BingMapsKey = bingMapSessionKey,
                IncludeIso2 = true,
                IncludeNeighborhood = true,
            };

            // BingMapsRestToolkit service manager for rest api call as per request. The response contains the desired information
            var geocodeLoc = await getBingRESTResponseRequest<BingMapsRESTToolkit.Location>(geocodeRequest);
            if (geocodeLoc != null)
            {
                addressCoordinates = new Coordinate(geocodeLoc.Point.Coordinates[0], geocodeLoc.Point.Coordinates[1]);
            }
            return addressCoordinates;
        }

        private async Task<NavteqPoiSchema.Response> getPOIForLocation(double latitude, double longitude)
        {
            // point of interest nearby a location
            string baseUrl;
            //Switch between the NAVTEQ POI data sets for NA and EU based on the longitude value. 
            if (longitude < -30)
            {
                baseUrl = "http://spatial.virtualearth.net/REST/v1/data/f22876ec257b474b82fe2ffcb8393150/NavteqNA/NavteqPOIs";
            }
            else
            {
                baseUrl = "http://spatial.virtualearth.net/REST/v1/data/c2ae584bbccc4916a0acf75d1e6947b4/NavteqEU/NavteqPOIs";
            }

            string query = string.Format("{0}?spatialfilter=nearby({1},{2},{3})&$format=json&key={4}",
                baseUrl, latitude, longitude, defaultSearchRadius, bingMapSessionKey);

            return await GetResponse<NavteqPoiSchema.Response>(new Uri(query));
        }

        // gets a http response(json) and formats into a object of type T
        private async Task<T> GetResponse<T>(Uri uri)
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            var response = await client.GetAsync(uri);

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                System.Diagnostics.Trace.WriteLine(stream.ToString());
                return (T)ser.ReadObject(stream);
            }
        }

        // adds pushpins for the POI locations. The pusphins can be customised depending on the place
        private void addPushPinAtPOI(NavteqPoiSchema.Response response)
        {
            if (response != null && response.ResultSet != null &&
                                       response.ResultSet.Results != null &&
                                       response.ResultSet.Results.Length > 0)
            {
                
                myMap.Children.Clear();
                foreach (var poi in response.ResultSet.Results)
                {
                    var loc = new Microsoft.Maps.MapControl.WPF.Location(poi.Latitude, poi.Longitude);
                    var pin = new Pushpin();
                    pin.Tag = poi;
                    pin.Location = loc;
                    myMap.Children.Add(pin);
                }
            }
            else
            {
                // TODO meesage to display on map
                System.Diagnostics.Trace.WriteLine("No result");
            }
        }

        // creates a route option to be used in route request REST api call
        private RouteOptions getRouteOptions()
        {
            return new RouteOptions()
            {
                DistanceUnits = distanceUnits,
                TravelMode = travelMode,
                Optimize = optimize,
                RouteAttributes = new List<RouteAttributeType>() { RouteAttributeType.All }
            };

        }

        // creates a route between two points based on the coordinates of the points
        private void getRouteFromCoordinates(Coordinate from, Coordinate to)
        {
            var routeOption = getRouteOptions();
            var routeRequest = new RouteRequest()
            {
                BingMapsKey = bingMapSessionKey,
                Waypoints = new List<SimpleWaypoint>() { new SimpleWaypoint(from), new SimpleWaypoint(to) },
                RouteOptions = routeOption,
            };

            // processes the request and creates route accordingly
            getRoute(routeRequest);
        }

        // creates a route between two points based on the address of the points
        private void getRouteFromAddress(string from, string to)
        {
            var routeOption = getRouteOptions();
            var routeRequest = new RouteRequest()
            {
                BingMapsKey = bingMapSessionKey,
                Waypoints = new List<SimpleWaypoint>() { new SimpleWaypoint(from), new SimpleWaypoint(to) },
                RouteOptions = routeOption,
            };
            // processes the request and creates route accordingly
            getRoute(routeRequest);
        }

        // creates a route between two points 
        private async void getRoute(RouteRequest routeRequest)
        {
            // result from Route Request is contained in BingMapsRESTToolkit.Route
            var routeResult = await getBingRESTResponseRequest<BingMapsRESTToolkit.Route>(routeRequest);
            if (routeResult != null)
            {
                // the route path contains the coordinates to construct a route on the map
                if (routeResult.RoutePath != null && routeResult.RoutePath.Line != null && routeResult.RoutePath.Line.Coordinates != null)
                {
                    double[][] routePath = routeResult.RoutePath.Line.Coordinates;

                    LocationCollection locs = new LocationCollection();

                    for (int i = 0; i < routePath.Length; i++)
                    {
                        if (routePath[i].Length >= 2)
                        {
                            locs.Add(new Microsoft.Maps.MapControl.WPF.Location(routePath[i][0], routePath[i][1]));
                        }
                    }

                    MapPolyline routeLine = new MapPolyline()
                    {
                        Locations = locs,
                        Stroke = new SolidColorBrush(Colors.Blue),
                        StrokeThickness = 5
                    };
                    myMap.Children.Add(routeLine);
                    myMap.SetView(locs, new Thickness(5), 0);
                }
                else
                {
                    // TODO message to be displayed for not finding any route between the two points
                }
            }
            else
            {
                //TODO message to be displayed on map for not finding any route between the two points
            }
        }

        // common method for all BingREST request. Can return null value
        // takes parameter of type BaseRestRequest, gets the response and extracts the BingMapsRESTToolkit.Resource from the response. The method takes generic T : BingMapsRESTToolkit.Resource, to evaluate the resource according to request
        private async Task<T> getBingRESTResponseRequest<T>(BingMapsRESTToolkit.BaseRestRequest request) where T : BingMapsRESTToolkit.Resource
        {
            T result = null;
            var response = await ServiceManager.GetResponseAsync(request);
            if (response != null && response.ResourceSets != null && response.ResourceSets.Length > 0 
                && response.ResourceSets[0].Resources != null && response.ResourceSets[0].Resources.Length > 0)
            {
                // result from Route Request is contained in BingMapsRESTToolkit.Route
                result = (T)Convert.ChangeType(response.ResourceSets[0].Resources[0], typeof(T));
            }
            return result;
        }
    }
}