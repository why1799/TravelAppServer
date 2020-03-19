using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAppModels.ModelsWithTripId
{
    public class Goal : Models.Goal
    {
        public Guid TripId { get; set; }
    }
}
