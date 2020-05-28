using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
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
            var token = context.HttpContext.Request.Cookies["TraverlApp.fun.Token"];
            var userId = context.HttpContext.Request.Cookies["TraverlApp.fun.UserId"];

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var useridguid) || !(_storage.CheckToken(token, useridguid).Result))
            {
                context.HttpContext.Response.Cookies.Delete("TraverlApp.fun.UserId");
                context.HttpContext.Response.Cookies.Delete("TraverlApp.fun.Token");
                context.Result = new ForbidResult();
            }
        }
    }
}
