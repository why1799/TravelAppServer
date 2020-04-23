using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TravelAppServer.Pages
{
    public class TripModel : PageModel
    {
        [BindProperty(Name = "id", SupportsGet = true)]
        public Guid Id { get; set; }
        
        public void OnGet()
        {

        }

        public void CreateModel(string token)
        {
            
        }
    }
}