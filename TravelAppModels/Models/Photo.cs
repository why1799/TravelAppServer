using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAppModels.Models
{
    public class Photo
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Base64 { get; set; }
    }
}
