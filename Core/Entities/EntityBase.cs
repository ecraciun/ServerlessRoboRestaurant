using Newtonsoft.Json;

namespace Core.Entities
{
    public class EntityBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}