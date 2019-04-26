﻿using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Core.Entities
{
    [ExcludeFromCodeCoverage]
    public class EntityBase
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}