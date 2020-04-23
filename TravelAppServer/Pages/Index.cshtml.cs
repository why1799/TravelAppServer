using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using TravelAppServer.Configs;

namespace TravelAppServer.Pages
{
    public class IndexModel : PageModel
    {

        public IActionResult OnGet()
        {
            if(User.Identity.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme)
            {
                return Redirect("/trips");
            }
            else
            {
                return Redirect("/Account/Login");
            }
        }
    }
}