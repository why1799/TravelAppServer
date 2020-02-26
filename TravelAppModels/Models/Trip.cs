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
        public Guid[] Photos {get;set;}
    }
}
