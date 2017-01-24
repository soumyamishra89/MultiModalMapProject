namespace MultiModalMapProject.BingQueryFilters
{
    // Creates a spatial filter with latitude, longitude and search radius for bing query of places of interests
    class SpatialFilter : BingQueryFilter
    {
        private double latitude;
        private double longitude;
        private double searchRadius;

        public SpatialFilter(double latitude, double longitude, double searchRadius)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.searchRadius = searchRadius;
        }
        string BingQueryFilter.buildFilter()
        {
            System.Diagnostics.Trace.WriteLine(latitude);
            return string.Format("spatialfilter=nearby({0},{1},{2})&", latitude, longitude, searchRadius);
        }
    }
}
