﻿using System.Runtime.Serialization;

namespace MultiModalMapProject.JsonSchemas.NavteqPoiSchema
{
    [DataContract]
    public class Response
    {
        [DataMember(Name = "d", EmitDefaultValue = false)]
        public ResultSet ResultSet { get; set; }
    }

    [DataContract]
    public class ResultSet
    {
        [DataMember(Name = "__copyright", EmitDefaultValue = false)]
        public string Copyright { get; set; }

        [DataMember(Name = "results", EmitDefaultValue = false)]
        public Result[] Results { get; set; }
    }

    [DataContract]
    public class Result
    {
        [DataMember(Name = "Latitude", EmitDefaultValue = false)]
        public double Latitude { get; set; }

        [DataMember(Name = "Longitude", EmitDefaultValue = false)]
        public double Longitude { get; set; }

        [DataMember(Name = "LanguageCode", EmitDefaultValue = false)]
        public string LanguageCode { get; set; }

        [DataMember(Name = "Name", EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "DisplayName", EmitDefaultValue = false)]
        public string DisplayName { get; set; }

        [DataMember(Name = "AddressLine", EmitDefaultValue = false)]
        public string AddressLine { get; set; }

        [DataMember(Name = "Locality", EmitDefaultValue = false)]
        public string Locality { get; set; }

        [DataMember(Name = "AdminDistrict", EmitDefaultValue = false)]
        public string AdminDistrict { get; set; }

        [DataMember(Name = "AdminDistrict2", EmitDefaultValue = false)]
        public string AdminDistrict2 { get; set; }

        [DataMember(Name = "PostalCode", EmitDefaultValue = false)]
        public string PostalCode { get; set; }

        [DataMember(Name = "CountryRegion", EmitDefaultValue = false)]
        public string CountryRegion { get; set; }

        [DataMember(Name = "Phone", EmitDefaultValue = false)]
        public string Phone { get; set; }

        [DataMember(Name = "EntityTypeID", EmitDefaultValue = false)]
        public string EntityTypeID { get; set; }
    }
}
