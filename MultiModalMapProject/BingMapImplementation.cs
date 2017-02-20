using Microsoft.Kinect;
using Microsoft.Maps.MapControl.WPF.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Maps.MapControl.WPF;
using BingMapsRESTToolkit;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Windows.Media;
using MultiModalMapProject.Util;
using System.Threading;
using MultiModalMapProject.SpeechUtil;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using MultiModalMapProject.JsonSchemas.NavteqPoiSchema;

// This class file contains related to Bing Maps
namespace MultiModalMapProject
{
    //author: @soumyamishra89
    public partial class MainWindow
    {

        // position of the kinect hand (preferrably right hand)  on the screen relative to the UIElement on which it is drawn
        System.Windows.Point kinectHandPositionOnScreen = new System.Windows.Point();


        LocationConverter locConv = new LocationConverter();

        // sets default parameters to map and initialises map components if necessary
        private void InitialiseMapComponent()
        {
            myMap.Center = StaticVariables.defaultCenter;
            myMap.ZoomLevel = 2;
            // getting a session key from bing maps for using non-billable call to bing rest api
            myMap.CredentialsProvider.GetCredentials(c =>
            {
                StaticVariables.bingMapSessionKey = c.ApplicationId;
            });
        }
        
        // Summary:
        //      zooms in the map by the zoominFactor.
        //      zoomInFactor is nullable. In case a null value is sent, then the zoomin is done by a default value.
        private void zoominMap(int? zoominFactor)
        {
            if (zoominFactor.HasValue)
                myMap.ZoomLevel = myMap.ZoomLevel + zoominFactor.Value;
            else
                myMap.ZoomLevel = myMap.ZoomLevel + StaticVariables.zoominFactor;
        }

        // zooms out the map by the zoomoutFactor.
        // zoomoutFactor is nullable. In case a null value is sent, then the zoomout is done by a default value.
        private void zoomoutMap(int? zoomoutFactor)
        {
            if (zoomoutFactor.HasValue)
                myMap.ZoomLevel = myMap.ZoomLevel - zoomoutFactor.Value;
            else
                myMap.ZoomLevel = myMap.ZoomLevel - StaticVariables.zoomoutFactor;
        }

        // returns a BingMapsRESTToolkit.Coordinate for a address from Bing Geocode. 
        // if no coordinates is found, it returns null
        private async Task<Coordinate> getLocationFromAddress(string address)
        {
            Coordinate addressCoordinates = null;
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
            if (geocodeLoc != null && geocodeLoc.Point != null && geocodeLoc.Point.Coordinates != null && geocodeLoc.Point.Coordinates.Length > 0)
            {
                addressCoordinates = new Coordinate(geocodeLoc.Point.Coordinates[0], geocodeLoc.Point.Coordinates[1]);
            }
            else
            {
                setSystemWarningMessagesToSpeechLabel(string.Format(SystemMessages.NOLOCATION_MESSAGE, address));
            }
            return addressCoordinates;
        }

        // point of interest nearby a location
        // queryFilter can be any BingQueryFilters.BingQueryFilter which helps in fecthing specific POI from bing query api
        private async Task<JsonSchemas.NavteqPoiSchema.Response> getPOIForLocation(double latitude, double longitude, params BingQueryFilters.BingQueryFilter[] queryFilters)
        {
            List<BingQueryFilters.BingQueryFilter> filters = queryFilters.ToList();
            // the spatial filter is required
            filters.Insert(0, new BingQueryFilters.SpatialFilter(latitude, longitude, StaticVariables.defaultSearchRadius));

            string query = BingPOIQueryBuilder.buildPOIQuery(longitude, filters);
            Trace.WriteLine(query);
            return await GetResponse<JsonSchemas.NavteqPoiSchema.Response>(new Uri(query));
        }

        // gets a http response(json) and formats into a object of type T
        private async Task<T> GetResponse<T>(Uri uri)
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            var response = await client.GetAsync(uri);

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));

                return (T)ser.ReadObject(stream);
            }
        }

        // adds pushpins for the POI locations. The pusphins can be customised depending on the place.
        // the method also adds poi info to the listview
        private async void addPushPinAtPOIAndAddtoList(JsonSchemas.NavteqPoiSchema.Response response)
        {
            if (response != null && response.ResultSet != null &&
                                       response.ResultSet.Results != null &&
                                       response.ResultSet.Results.Length > 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    myMap.Children.Clear();
                    nearbyPlacesList.Visibility = Visibility.Visible;
                });
                foreach (var poi in response.ResultSet.Results)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        var loc = new Microsoft.Maps.MapControl.WPF.Location(poi.Latitude, poi.Longitude);
                        var pin = new Pushpin();
                        pin.Tag = poi;
                        pin.Content = "" + StaticVariables.POINumber;
                        pin.Location = loc;
                        myMap.Children.Add(pin);
                       
                    });
                    BitmapImage poiImage = await getImageryOfPOI(new Coordinate(poi.Latitude, poi.Longitude));

                    addPOIToListView(poiImage, poi, StaticVariables.POINumber++);
                }
               
            }

            else
            {
                setSystemWarningMessagesToSpeechLabel(SystemMessages.NOPOI_MESSAGE);
                System.Diagnostics.Trace.WriteLine("No result");
            }
        }


        // creates a route between two points based on the coordinates of the points
        private async Task<Boolean> getRouteFromCoordinates(Coordinate from, Coordinate to)
        {
            var routeOption = RouteParameters.INSTANCE.getRouteOptions();
            var routeRequest = new RouteRequest()
            {
                BingMapsKey = StaticVariables.bingMapSessionKey,
                Waypoints = new List<SimpleWaypoint>() { new SimpleWaypoint(from), new SimpleWaypoint(to) },
                RouteOptions = routeOption,
            };

            // processes the request and creates route accordingly
            return await getRoute(routeRequest);
        }

        // creates a route between two points based on the address of the points
        private async Task<Boolean> getRouteFromAddress(string from, string to)
        {
            var routeOption = RouteParameters.INSTANCE.getRouteOptions();
            var routeRequest = new RouteRequest()
            {
                BingMapsKey = StaticVariables.bingMapSessionKey,
                Waypoints = new List<SimpleWaypoint>() { new SimpleWaypoint(from), new SimpleWaypoint(to) },
                RouteOptions = routeOption,
            };
            // processes the request and creates route accordingly
            return await getRoute(routeRequest);
        }

        // creates a route between two points 
        private async Task<Boolean> getRoute(RouteRequest routeRequest)
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
                    Dispatcher.Invoke(() =>
                    {
                        MapPolyline routeLine = new MapPolyline()
                        {
                            Locations = locs,
                            Stroke = new SolidColorBrush(StaticVariables.routePathColor),
                            StrokeThickness = 5
                        };
                        // adding the route to the map on UI Thread

                        myMap.Children.Add(routeLine);
                        myMap.SetView(locs, new Thickness(5), 0);
                    });
                    return true;
                }
                else
                {
                    if (RouteParameters.INSTANCE.isAddressAvailable())
                        setSystemWarningMessagesToSpeechLabel(string.Format(SystemMessages.NOROUTE_MESSAGE, RouteParameters.INSTANCE.fromLocation, RouteParameters.INSTANCE.toLocation));
                    else
                        setSystemWarningMessagesToSpeechLabel(string.Format(SystemMessages.NOROUTE_MESSAGE, RouteParameters.INSTANCE.fromCLocation.Latitude + ", " + RouteParameters.INSTANCE.fromCLocation.Longitude, RouteParameters.INSTANCE.toCLocation.Latitude + ", " + RouteParameters.INSTANCE.toCLocation.Longitude));
                    return false;
                }
            }
            else
            {
                if (RouteParameters.INSTANCE.isAddressAvailable())
                    setSystemWarningMessagesToSpeechLabel(string.Format(SystemMessages.NOROUTE_MESSAGE, RouteParameters.INSTANCE.fromLocation, RouteParameters.INSTANCE.toLocation));
                else
                    setSystemWarningMessagesToSpeechLabel(string.Format(SystemMessages.NOROUTE_MESSAGE, RouteParameters.INSTANCE.fromCLocation.Latitude + ", " + RouteParameters.INSTANCE.fromCLocation.Longitude, RouteParameters.INSTANCE.toCLocation.Latitude+", "+ RouteParameters.INSTANCE.toCLocation.Longitude));

                return false;
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
        private void ShowLocationOfKinectHandPoint()
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


        // adds a pushpin to a location sent to this method
        private void addPushpinToLocation(double latitude, double longitude)
        {
            this.Dispatcher.Invoke(() =>
            {
                Pushpin pushpin = new Pushpin();
                pushpin.Location = new Microsoft.Maps.MapControl.WPF.Location(latitude, longitude);
                myMap.Children.Add(pushpin);
            });
        }

        // the method gets the location(latitude and longitude) on map based on where the kinect hand (preferrably right hand) is pointing at the map.
        private Microsoft.Maps.MapControl.WPF.Location getLocationFromScreenPoint()
        {
            Microsoft.Maps.MapControl.WPF.Location l = null;
            this.Dispatcher.Invoke(() =>
            {
                System.Windows.Point mapPoint = KinectCanvas.TranslatePoint(kinectHandPositionOnScreen, myMap);
                // Due to some alignment problem, the Y coordinate is shifted (Open Issue: need to figure out the problem). Hence this offset tries to place the point exactly on the kinect hand position
                mapPoint.Y = mapPoint.Y - StaticVariables.handPointOffsetY;
                l = myMap.ViewportPointToLocation(mapPoint);
            });
            return l;
        }

        // the method gets and adds POI(Specific places if BingQueryFilters are specified) based on the point on application where the kinect hand (preferrably right hand) points.
        // In case of no POI is found it displays a no result message. The number of POI is limited by the user. 
        private async void addPOIToMapFromKinectHandPosition(params BingQueryFilters.BingQueryFilter[] queryFilters)
        {
            Microsoft.Maps.MapControl.WPF.Location handLocation = getLocationFromScreenPoint();
            if (null != handLocation)
            {
                // returns a NavteqPoiSchema.Response containing details of the POI nearby the location. 
                var pois = await getPOIForLocation(handLocation.Latitude, handLocation.Longitude, queryFilters);

                // in case of no result is found, a no result message needs to be displayed
                if (pois != null && pois.ResultSet != null &&
                                           pois.ResultSet.Results != null &&
                                           pois.ResultSet.Results.Length > 0)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        myMap.Children.Clear();
                        nearbyPlacesList.Visibility = Visibility.Visible;
                    });
                    foreach (var poi in pois.ResultSet.Results)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            var loc = new Microsoft.Maps.MapControl.WPF.Location(poi.Latitude, poi.Longitude);
                            var pin = new Pushpin();
                            pin.Tag = poi;
                            pin.Location = loc;
                            pin.Content = "" + StaticVariables.POINumber;
                            myMap.Children.Add(pin);
                        });
                        BitmapImage poiImage = await getImageryOfPOI(new Coordinate(poi.Latitude, poi.Longitude));

                        addPOIToListView(poiImage, poi, StaticVariables.POINumber++);
                    }
                }
                else
                {
                    setSystemWarningMessagesToSpeechLabel(SystemMessages.NOPOI_MESSAGE);
                }
            }
        }
        // resets the map to original setting and removes all children from map.
        private void resetMap()
        {
            clearMap();
            //sets the zoom level to 0 and center of the map to defaultCenter for resetting the map
            myMap.SetView(StaticVariables.defaultCenter, 2);
            
        }

        // removes all the child elements in the map
        private void clearMap()
        {
            // removes all child elements in the map
            myMap.Children.Clear();
        }

        // sets the center of the map to this location and zooms in the map by the zoomlevel. If zoomlevel is null then it zooms in by a default value
        private void setCenterAndZoominLevelOfMap(double latitude, double longitude, int? zoominLevel)
        {
            // default value of zoom in level in case zoominLevel is null
            int zoomLevel = StaticVariables.zoominFactor;
            if (zoominLevel.HasValue)
                zoomLevel = zoominLevel.Value;

            myMap.SetView(new Microsoft.Maps.MapControl.WPF.Location(latitude, longitude), myMap.ZoomLevel + zoomLevel);
        }

        // sets the center of the map to this location and zooms out the map by the zoomlevel. If zoomlevel is null then it zooms out by a default value
        private void setCenterAndZoomoutLevelOfMap(double latitude, double longitude, int? zoomoutLevel)
        {
            // default value of zoom in level in case zoominLevel is null
            int zoomLevel = StaticVariables.zoominFactor;
            if (zoomoutLevel.HasValue)
                zoomLevel = zoomoutLevel.Value;

            myMap.SetView(new Microsoft.Maps.MapControl.WPF.Location(latitude, longitude), myMap.ZoomLevel - zoomLevel);
        }

        // sets the center of the map to this location
        private void setCenterOfMap(double latitude, double longitude, double zoomLevel)
        {
            myMap.Center = new Microsoft.Maps.MapControl.WPF.Location(latitude, longitude);
            myMap.ZoomLevel = zoomLevel;
        }

        // zoom in on the map at the place where the kinect is pointing
        private void zoominMapAtKinectHandLocation(int? zoominFactor)
        {
            double zoomLevel = myMap.ZoomLevel + StaticVariables.zoominFactor;
            if (zoominFactor.HasValue)
                zoomLevel = myMap.ZoomLevel + zoominFactor.Value;

            myMap.SetView(getLocationFromScreenPoint(), zoomLevel);
        }

        // zoom out on the map at the place where the kinect is pointing
        private void zoomoutMapAtKinectHandLocation(int? zoomoutFactor)
        {
            double zoomLevel = myMap.ZoomLevel - StaticVariables.zoomoutFactor;
            if (zoomoutFactor.HasValue)
                zoomLevel = myMap.ZoomLevel - zoomoutFactor.Value;

            myMap.SetView(getLocationFromScreenPoint(), zoomLevel);
        }

        // this method gets the image of a location at a particular zoom level
        private async Task<BitmapImage> getImageryOfPOI(Coordinate centerPoint)
        {
            var imageRequest = new ImageryRequest()
            {
                CenterPoint = centerPoint,
                ZoomLevel = 21,
                ImagerySet = ImageryType.Aerial,
                BingMapsKey = StaticVariables.bingMapSessionKey,
            };
            
            var imageStream = await ServiceManager.GetImageAsync(imageRequest);
            var bitmapImage = new BitmapImage();
            try
            {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.DecodePixelHeight = StaticVariables.imageHeight;
                bitmapImage.DecodePixelWidth = StaticVariables.imageWidth;
                bitmapImage.StreamSource = imageStream;
                bitmapImage.EndInit();
            }
            catch (Exception)
            {
                // in case of no image available from bing map, a default image is loaded
                FileStream fileStream = new FileStream("../../Images/nopreview.png", FileMode.Open, FileAccess.Read);
                
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = fileStream;
                bitmapImage.EndInit();
            }
            bitmapImage.Freeze();
            return bitmapImage;
        }
        
    }
    
}