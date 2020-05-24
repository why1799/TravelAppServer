using System;

namespace TravelAppModels.Models
{
    public class DeletedTrip
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public long LastUpdate { get; set; }
    }
}
