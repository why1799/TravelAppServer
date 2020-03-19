using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAppModels.ModelsWithTripId
{
    public class Good : Models.Good
    {
        public Guid TripId { get; set; }
    }
}
