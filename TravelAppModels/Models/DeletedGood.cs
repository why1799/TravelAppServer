using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAppModels.Models
{
    public class DeletedGood
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TripId { get; set; }
        public long LastUpdate { get; set; }
    }
}
