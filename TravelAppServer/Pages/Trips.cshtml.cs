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

        public List<Trip> Trips;

        public TripsModel(IStorage storage)
        {
            _trips = new TripController(storage);
            _photos = new PhotoController(storage);
        }

        public async Task<IActionResult> OnGet()
        {
            var token = User.Claims.Where(c => c.Type == "Token").Select(c => c.Value).FirstOrDefault();
            Trips = new List<Trip>();
            var allids = ((await _trips.GetAll(token)) as ObjectResult).Value as Guid[];

            foreach(var id in allids)
            {
                var trip = ((await _trips.Read(id, token)) as ObjectResult).Value as Trip;
                trip.Photos = new Photo[trip.PhotoIds?.Length ?? 0] ;
                for (int i = 0; i < (trip.PhotoIds?.Length ?? 0); i++)
                {
                    trip.Photos[i] = ((await _photos.Get(trip.PhotoIds[i], token)) as ObjectResult).Value as Photo;
                }
                Trips.Add(trip);
            }

            return Page();
        }
    }
}