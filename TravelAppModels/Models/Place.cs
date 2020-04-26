using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TravelAppModels.Models
{
    public class Place
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Adress { get; set; }
        public string Description { get; set; }
        public bool IsVisited { get; set; }
        public long? Date { get; set; }
        public long? LastUpdate { get; set; }
        public Guid[] PhotoIds { get; set; }

        [JsonIgnore]
        public Photo[] Photos { get; set; }
    }
}
