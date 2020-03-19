using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAppModels.ModelsWithTripId
{
    public class Purchase : Models.Purchase
    {
        public Guid TripId { get; set; }
    }
}
