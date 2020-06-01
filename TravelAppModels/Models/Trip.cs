using System;
using System.Text.Json.Serialization;

namespace TravelAppModels.Models
{
    public class Trip
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string TextField { get; set; }
        public Guid[] PhotoIds {get;set;}
        public Guid[] FileIds { get; set; }
        public Guid[] PlaceIds { get; set; }
        public Guid[] GoodIds { get; set; }
        public Guid[] GoalIds { get; set; }
        public Guid[] PurchaseIds { get; set; }
        public long? FromDate { get; set; }
        public long? ToDate { get; set; }
        public long? LastUpdate { get; set; }

        [JsonIgnore]
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public Photo[] Photos { get; set; }
        [JsonIgnore]
        public File[] Files { get; set; }
        [JsonIgnore]
        public Place[] Places { get; set; }
        [JsonIgnore]
        public Good[] Goods { get; set; }
        [JsonIgnore]
        public Goal[] Goals { get; set; }
        [JsonIgnore]
        public Purchase[] Purchases { get; set; }
    }
}
