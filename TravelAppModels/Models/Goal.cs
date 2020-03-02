using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAppModels.Models
{
    public class Goal
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDone { get; set; }
    }
}
