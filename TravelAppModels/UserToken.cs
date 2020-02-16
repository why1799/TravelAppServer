using System;

namespace TravelAppModels
{
    public class UserToken
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}
