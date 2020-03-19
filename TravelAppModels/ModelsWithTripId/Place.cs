using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAppModels.ModelsWithTripId
{
    public class Place : Models.Place
    {
        public Guid TripId { get; set; }
    }
}
