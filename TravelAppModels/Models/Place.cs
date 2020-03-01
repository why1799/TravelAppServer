using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAppModels.Models
{
    public class Place
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Adress { get; set; }
        public string Description { get; set; }
        public Guid[] Photos { get; set; }
    }
}
