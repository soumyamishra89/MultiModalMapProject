
using System.Runtime.Serialization;

namespace MultiModalMapProject.JsonSchemas.LUISJson
{
    // This class is the C# JSON object equivalent of the json result from microsoft LUIS
    // Contains the intents and entities of the recognised speech.
    [DataContract]
    class LUISJsonObject
    {
        [DataMember(Name ="query", EmitDefaultValue =false)]
        public string Query { get; set; }
        [DataMember(Name ="intents", EmitDefaultValue =false)]
        public Intent[] Intents { get; set; }

        [DataMember(Name = "entities", EmitDefaultValue = false)]
        public Entity[] Entities { get; set; }
    }

    [DataContract]
    public class Intent
    {
        [DataMember(Name ="intent",EmitDefaultValue =false)]
        public string IntentValue { get; set; }

        [DataMember(Name ="score", EmitDefaultValue =false)]
        public double Score { get; set; }
    }

    [DataContract]
    public class Entity
    {
        [DataMember(Name ="entity", EmitDefaultValue =false)]
        public string EntityValue { get; set; }

        [DataMember(Name ="type", EmitDefaultValue =false)]
        public string Type { get; set; }

        [DataMember(Name ="startIndex",EmitDefaultValue =false)]
        public int StartIndex { get; set; }

        [DataMember(Name ="endIndex", EmitDefaultValue =false)]
        public int EndIndex { get; set; }

        [DataMember(Name ="score",EmitDefaultValue =false)]
        public double Score { get; set; }
    }
}
