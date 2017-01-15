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
using MultiModalMapProject.Util;

// This class file contains related to Bing Maps
namespace MultiModalMapProject
{
    //author: @soumyamishra89
    public partial class MainWindow
    {
         // position of the kinect hand (preferrably right hand)  on the screen relative to the UIElement on which it is drawn
        System.Windows.Point kinectHandPositionOnScreen = new System.Windows.Point();

        
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
            myMap.ZoomLevel = myMap.ZoomLevel + StaticVariables.zoominFactor;
        }

        // zooms out the map based on a defined zoom factor
        private void zoomoutMap()
        {
            myMap.ZoomLevel = myMap.ZoomLevel - StaticVariables.zoomoutFactor;
        }

        // returns a BingMapsRESTToolkit.Coordinate for a address from Bing Geocode. 
        // Can return empty coordinates
        private async Task<Coordinate> getLocationFromAddress(string address)
        {
            Coordinate addressCoordinates = new Coordinate();
            // geocode request helps in coordinates of an address
            var geocodeRequest = new GeocodeRequest()
            {
                Query = address,
                BingMapsKey = StaticVariables.bingMapSessionKey,
                IncludeIso2 = true,
                IncludeNeighborhood = true,
            };

            // BingMapsRestToolkit service manager for rest api call as per request. The response contains the desired information
            var geocodeLoc = await getBingRESTResponseRequest<BingMapsRESTToolkit.Location>(geocodeRequest);
            if (geocodeLoc != null)
            {
                addressCoordinates = new Coordinate(geocodeLoc.Point.Coordinates[0], geocodeLoc.Point.Coordinates[1]);
            }
            else
            {
                //TODO error message to display when the coordinates could not be found
            }
            return addressCoordinates;
        }

        // point of interest nearby a location
        // queryFilter can be any BingQueryFilters.BingQueryFilter which helps in fecthing specific POI from bing query api
        private async Task<NavteqPoiSchema.Response> getPOIForLocation(double latitude, double longitude, params BingQueryFilters.BingQueryFilter[] queryFilters)
        {
            List<BingQueryFilters.BingQueryFilter> filters = queryFilters.ToList();
            // the spatial filter is required
            filters.Insert(0, new BingQueryFilters.SpatialFilter(latitude, longitude, StaticVariables.defaultSearchRadius));

            string query = BingPOIQueryBuilder.buildPOIQuery(longitude, filters);          

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
                DistanceUnits = StaticVariables.distanceUnits,
                TravelMode = StaticVariables.travelMode,
                Optimize = StaticVariables.optimize,
                RouteAttributes = new List<RouteAttributeType>() { RouteAttributeType.All }
            };

        }

        // creates a route between two points based on the coordinates of the points
        private void getRouteFromCoordinates(Coordinate from, Coordinate to)
        {
            var routeOption = getRouteOptions();
            var routeRequest = new RouteRequest()
            {
                BingMapsKey = StaticVariables.bingMapSessionKey,
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
                BingMapsKey = StaticVariables.bingMapSessionKey,
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
                        Stroke = new SolidColorBrush(StaticVariables.routePathColor),
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

        // the method adds a pushpin at the location of the kinect hand position (preferrably right hand)
        // the kinectHandPositionOnScreen has a default value of 0,0. It gets updated based on hand position. 
        private void pointToALocation()
        {
            // The kinectHandPositionOnScreen gives coordinates relative to the UIElement on which kinect hands are drawn which in this case is 'KinectCanvas'.
            // This coordinates needs to translated relative to the Map UIElement to find out the location.
            System.Windows.Point mapPoint = KinectCanvas.TranslatePoint(kinectHandPositionOnScreen, myMap);
            // Due to some alignment problem, the Y coordinate is shifted (Open Issue: need to figure out the problem). Hence this offset tries to place the point exactly on the kinect hand position
            mapPoint.Y = mapPoint.Y - StaticVariables.handPointOffsetY;
            Microsoft.Maps.MapControl.WPF.Location l = myMap.ViewportPointToLocation(mapPoint);
            Pushpin pushpin = new Pushpin();
            pushpin.Location = l;
            myMap.Children.Add(pushpin);

        }

        // the method gets the location(latitude and longitude) on map based on where the kinect hand (preferrably right hand) is pointing at the map.
        private Microsoft.Maps.MapControl.WPF.Location getLocationFromScreenPoint()
        {
            System.Windows.Point mapPoint = KinectCanvas.TranslatePoint(kinectHandPositionOnScreen, myMap);
            // Due to some alignment problem, the Y coordinate is shifted (Open Issue: need to figure out the problem). Hence this offset tries to place the point exactly on the kinect hand position
            mapPoint.Y = mapPoint.Y - StaticVariables.handPointOffsetY;
            Microsoft.Maps.MapControl.WPF.Location l = myMap.ViewportPointToLocation(mapPoint);
            return l;
        }

        // the method gets and adds POI based on the point on application where the kinect hand (preferrably right hand) points. 
        // In case of no POI is found it displays a no result message. The number of POI is limited by the user. 
        private async void addPOIToMapFromKinectHandPosition()
        {
            Microsoft.Maps.MapControl.WPF.Location handLocation = getLocationFromScreenPoint();

            // returns a NavteqPoiSchema.Response containing details of the POI nearby the location. 
            var pois = await getPOIForLocation(handLocation.Latitude, handLocation.Longitude);
            // in case of no result is found, a no result message needs to be displayed
            if (pois != null && pois.ResultSet != null &&
                                       pois.ResultSet.Results != null &&
                                       pois.ResultSet.Results.Length > 0)
            {
                myMap.Children.Clear();
                foreach (var poi in pois.ResultSet.Results)
                {
                    var loc = new Microsoft.Maps.MapControl.WPF.Location(poi.Latitude, poi.Longitude);
                    var pin = new Pushpin();
                    pin.Tag = poi;
                    pin.Location = loc;                      
                    myMap.Children.Add(pin);
                    // TODO add levels of POI to the screen
                }
            }
            else
            {
                // TODO no result message
            }
            }
    }
}