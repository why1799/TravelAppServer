using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using TravelAppServer.Configs;

namespace TravelAppServer.Pages
{
    public class IndexModel : PageModel
    {
        public string Host { get; set; }

        public IndexModel(IOptions<Settings> options)
        {
            Host = options.Value.Host;
        }
    
        public string Message { get; private set; } = "PageModel in C#";

        public void OnGet()
        {
            Message += $" Server time is { DateTime.Now }";
        }
    }
}