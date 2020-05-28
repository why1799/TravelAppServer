using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using TravelAppModels.Models;
using TravelAppModels.Templates;
using TravelAppServer.Controllers;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Pages.Auth
{
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        private readonly AuthController Auth;

        [BindProperty(Name = "ReturnUrl", SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public LoginModel(IStorage storage)
        {
            ReturnUrl = "/trips";
            Auth = new AuthController(storage);
        }

        public IActionResult OnGet()
        {
            ViewData["Login"] = true;
            if (User.Identity.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme)
            {
                return Redirect(ReturnUrl);
            }
            else
            {
                return Page();
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostLogin([FromBody] LoginTemplate login)
        {
            var result = await Auth.Login(login) as ObjectResult;
            if (result.StatusCode == 200)
            {
                var token = result.Value as UserToken;

                var options = new CookieOptions
                {
                    Expires = new DateTimeOffset(DateTime.Now.AddMonths(6))
                };

                HttpContext.Response.Cookies.Append("TraverlApp.fun.UserId", token.UserId.ToString(), options);
                HttpContext.Response.Cookies.Append("TraverlApp.fun.Token", token.Token, options);

                return StatusCode(StatusCodes.Status200OK, "Login success");
            }
            else
            {
                return result;
            }
        }
    }
}