using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelAppServer.Controllers;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        private readonly AuthController Auth;

        [BindProperty(Name = "ReturnUrl", SupportsGet = true)]
        public string ReturnUrl { get; set; }

        public AccessDeniedModel(IStorage storage)
        {
            ReturnUrl = "/trips";
            Auth = new AuthController(storage);
        }

        public IActionResult OnGet()
        {
            //return Page();
            return Redirect($"/Account/Login?ReturnUrl={ReturnUrl}");
        }
    }
}