using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiModalMapProject.Util
{
    // creates a bing query url for POI search based on the filters provided by user
    class BingPOIQueryBuilder
    {

        // the filter should atleast contain BingQueryFilters.SpatialFilter
        public static string buildPOIQuery(double longitude, List<BingQueryFilters.BingQueryFilter> filters)
        {
            string baseUrl;
            //Switch between the NAVTEQ POI data sets for NA and EU based on the longitude value. 
            if (longitude < -30)
            {
                baseUrl = "http://spatial.virtualearth.net/REST/v1/data/f22876ec257b474b82fe2ffcb8393150/NavteqNA/NavteqPOIs?";
            }
            else
            {
                baseUrl = "http://spatial.virtualearth.net/REST/v1/data/c2ae584bbccc4916a0acf75d1e6947b4/NavteqEU/NavteqPOIs?";
            }

            StringBuilder urlBuilder = new StringBuilder(baseUrl);
            foreach (BingQueryFilters.BingQueryFilter filter in filters)
            {
                urlBuilder.Append(filter.buildFilter());
            }
            urlBuilder.Append("$format=json&key=").Append(StaticVariables.bingMapSessionKey);

            return urlBuilder.ToString();
        }
    }
}
