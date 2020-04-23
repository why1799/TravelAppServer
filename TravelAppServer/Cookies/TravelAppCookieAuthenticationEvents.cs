using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Cookies
{
    public class TravelAppCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly IStorage _storage;

        public TravelAppCookieAuthenticationEvents(IStorage storage)
        {
            // Get the database from registered DI services.
            _storage = storage;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipal = context.Principal;
            
            var token = (from c in userPrincipal.Claims
                               where c.Type == "Token"
                         select c.Value).FirstOrDefault();

            var userId = (from c in userPrincipal.Claims
                         where c.Type == "UserId"
                          select c.Value).FirstOrDefault();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var useridguid) || !(await _storage.CheckToken(token, useridguid)))
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}
