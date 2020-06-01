using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAppModels.Models
{
    public class File
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TripId { get; set; }
        public string Location { get; set; }
        public string Name { get; set; }
    }
}
