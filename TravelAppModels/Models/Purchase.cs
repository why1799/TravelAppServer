using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TravelAppModels.Models
{
    public class Purchase
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public bool IsBought { get; set; }
        public long? LastUpdate { get; set; }

        [JsonIgnore]
        public bool IsDeleted { get; set; }
    }
}
