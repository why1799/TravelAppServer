using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Authorize
{
    public class MyClaimRequirementAttribute : TypeFilterAttribute
    {
        public MyClaimRequirementAttribute() : base(typeof(MyClaimRequirementFilter))
        {
        }
    }

    public class MyClaimRequirementFilter : IAuthorizationFilter
    { 
        private readonly IStorage _storage;

        public MyClaimRequirementFilter(IStorage storage)
        {
            _storage = storage;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            var token = (from c in user.Claims
                         where c.Type == "Token"
                         select c.Value).FirstOrDefault();

            var userId = (from c in user.Claims
                          where c.Type == "UserId"
                          select c.Value).FirstOrDefault();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var useridguid) || !(_storage.CheckToken(token, useridguid).Result))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
