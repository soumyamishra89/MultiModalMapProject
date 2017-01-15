using System;

namespace MultiModalMapProject.BingQueryFilters
{
    // This interface defines a method which the bing query filters would implement in order to query places of interest based on some criteria
    interface BingQueryFilter
    {
        // creates a query filter criteria. e.g. EntityTypeId in ('9537','9565',)
        String buildFilter();
    }
}
