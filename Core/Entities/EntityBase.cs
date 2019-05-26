using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Core.Entities
{
    [ExcludeFromCodeCoverage]
    public class EntityBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("_etag")]
        public string ETag { get; set; }

        public string Partition => Constants.DefaultPartitionName;
    }
}