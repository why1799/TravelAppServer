using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelAppModels.Models;
using TravelAppServer.Controllers;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Pages
{
    [Authorize]
    public class TripsModel : PageModel
    {
        private readonly TripController _trips;
        private readonly PhotoController _photos;

        public List<(Trip, Photo)> Trips;

        public TripsModel(IStorage storage)
        {
            _trips = new TripController(storage);
            _photos = new PhotoController(storage);
        }

        public async Task<IActionResult> OnGet()
        {
            var token = User.Claims.Where(c => c.Type == "Token").Select(c => c.Value).FirstOrDefault();
            Trips = new List<(Trip, Photo)>();
            var allids = ((await _trips.GetAll(token)) as ObjectResult).Value as Guid[];

            foreach(var id in allids)
            {
                var trip = ((await _trips.Read(id, token)) as ObjectResult).Value as Trip;
                Photo photo = null;
                if(trip.PhotoIds != null && trip.PhotoIds.Length > 0)
                {
                    photo = ((await _photos.Get(trip.PhotoIds[0], token)) as ObjectResult).Value as Photo;
                }
                Trips.Add((trip, photo));
            }

            return Page();
        }

        [HttpGet]
        public async Task<IActionResult> OnGetLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //LogOut
            return StatusCode(StatusCodes.Status200OK, "Logout success");
        }
    }
}