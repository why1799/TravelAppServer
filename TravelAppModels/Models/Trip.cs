using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAppModels.Models
{
    public class Trip
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string TextField { get; set; }
        public Guid[] PhotoIds {get;set;}
        public Guid[] PlaceIds { get; set; }
        public Guid[] GoodIds { get; set; }
        public Guid[] GoalIds { get; set; }
        public Guid[] PurchaseIds { get; set; }
        public long? FromDate { get; set; }
        public long? ToDate { get; set; }
    }
}
