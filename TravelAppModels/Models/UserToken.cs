using System;

namespace TravelAppModels.Models
{
    public class UserToken
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}
