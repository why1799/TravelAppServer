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
    [IgnoreAntiforgeryToken]
    public class RegisterModel : PageModel
    {
        private readonly AuthController Auth;
        private readonly IStorage _storage;

        public RegisterModel(IStorage storage)
        {
            Auth = new AuthController(storage);
            _storage = storage;
        }

        public async Task<IActionResult> OnGet()
        {
            ViewData["Login"] = true;
            var token = HttpContext.Request.Cookies["TraverlApp.fun.Token"];
            var userId = HttpContext.Request.Cookies["TraverlApp.fun.UserId"];

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var useridguid) || !(await _storage.CheckToken(token, useridguid)))
            {
                HttpContext.Response.Cookies.Delete("TraverlApp.fun.UserId");
                HttpContext.Response.Cookies.Delete("TraverlApp.fun.Token");
                return Page();
            }
            else
            {
                return Redirect("/trips");
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostRegister([FromBody] RegisterTemplate register)
        {
            var result = await Auth.Register(register) as ObjectResult;
            if (result.StatusCode == 200)
            {
                var token = result.Value as UserToken;

                HttpContext.Response.Cookies.Append("TraverlApp.fun.UserId", token.UserId.ToString());
                HttpContext.Response.Cookies.Append("TraverlApp.fun.Token", token.Token);

                return StatusCode(StatusCodes.Status200OK, "Login success");
            }
            else
            {
                return result;
            }
        }
    }
}