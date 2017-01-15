using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiModalMapProject.BingQueryFilters
{
    // Entity filter creates a Bing query filter over the entity types provided by user
    class EntityTypeFilter : BingQueryFilter
    {
        List<string> entityIds;
        // constructor takes in a number of entity ids with atleast one entity id
        public EntityTypeFilter(string entityId, params string[] entityIds)
        {
            this.entityIds=entityIds.ToList();
            this.entityIds.Add(entityId);
        }
        public EntityTypeFilter(List<string> entityIds)
        {
            this.entityIds = entityIds.ToList();
           
        }
        public string buildFilter()
        {
            // %20 is used in place of space for url query
            StringBuilder filterQuery = new StringBuilder("EntityTypeId%20");
            if (entityIds.Count > 1)
            {
                filterQuery.Append("in%20(");
                foreach(string entityId in entityIds)
                {
                    filterQuery.Append("'").Append(entityId).Append("',");
                }
                // removes the last , in the query
                filterQuery.Remove(filterQuery.Length - 1, 1);
                filterQuery.Append(")");
            }
            else
            {
                filterQuery.Append("Eq%20").Append("'").Append(entityIds[0]).Append("'");
            }
            filterQuery.Append("&");

            return filterQuery.ToString();
        }
    }
}
