using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TravelAppServer.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGet()
        {
            if (User.Identity.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //LogOut
                return Redirect("/");
            }
            else
            {
                return Redirect("/");
            }
        }
    }
}