using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelAppModels.Models;
using TravelAppModels.Templates;
using TravelAppServer.Controllers;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Pages.Auth
{
    [IgnoreAntiforgeryToken(Order = 2000)]
    public class RegisterModel : PageModel
    {
        private readonly AuthController Auth;

        public RegisterModel(IStorage storage)
        {
            Auth = new AuthController(storage);
        }

        public IActionResult OnGet()
        {
            if (User.Identity.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme)
            {
                return Redirect("/trips");
            }
            else
            {
                return Page();
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostRegister([FromBody] RegisterTemplate register)
        {
            var result = await Auth.Register(register) as ObjectResult;
            if (result.StatusCode == 200)
            {
                var token = result.Value as UserToken;

                var claims = new List<Claim>
                {
                    new Claim("UserId", token.UserId.ToString()),
                    new Claim("Token", token.Token),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    //AllowRefresh = <bool>,
                    // Refreshing the authentication session should be allowed.

                    //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    // The time at which the authentication ticket expires. A 
                    // value set here overrides the ExpireTimeSpan option of 
                    // CookieAuthenticationOptions set with AddCookie.

                    IsPersistent = true,
                    // Whether the authentication session is persisted across 
                    // multiple requests. When used with cookies, controls
                    // whether the cookie's lifetime is absolute (matching the
                    // lifetime of the authentication ticket) or session-based.

                    //IssuedUtc = <DateTimeOffset>,
                    // The time at which the authentication ticket was issued.

                    //RedirectUri = <string>
                    // The full path or absolute URI to be used as an http 
                    // redirect response value.
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                var a = User.Identity.AuthenticationType;

                return StatusCode(StatusCodes.Status200OK, "Login success");
            }
            else
            {
                return result;
            }
        }
    }
}