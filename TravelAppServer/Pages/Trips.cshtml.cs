using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelAppModels.Models;
using TravelAppServer.Authorize;
using TravelAppServer.Controllers;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Pages
{
    [MyClaimRequirement]
    [IgnoreAntiforgeryToken]
    public class TripsModel : PageModel
    {
        private readonly TripController _trips;
        private readonly PhotoController _photos;
        private readonly GoodController _goods;
        private readonly PlaceController _places;
        private readonly GoalController _goals;
        private readonly PurchaseController _purchases;

        public List<TravelAppModels.Models.Trip> Trips;

        public TripsModel(IStorage storage)
        {
            _trips = new TripController(storage);
            _photos = new PhotoController(storage);
            _places = new PlaceController(storage);
            _goals = new GoalController(storage);
            _goods = new GoodController(storage);
            _purchases = new PurchaseController(storage);
        }

        public async Task<IActionResult> OnGet()
        {
            var token = User.Claims.Where(c => c.Type == "Token").Select(c => c.Value).FirstOrDefault();
            Trips = new List<TravelAppModels.Models.Trip>();
            var alltrips = ((await _trips.GetAll(token)) as ObjectResult).Value as TravelAppModels.Models.Trip[];

            foreach(var trip in alltrips)
            {
                trip.Photos = new Photo[trip.PhotoIds?.Length ?? 0] ;
                for (int i = 0; i < (trip.PhotoIds?.Length ?? 0); i++)
                {
                    trip.Photos[i] = ((await _photos.Get(trip.PhotoIds[i], token)) as ObjectResult).Value as Photo;
                }
                Trips.Add(trip);
            }

            return Page();
        }

        [HttpDelete]
        public async Task<IActionResult> OnDeleteDelete(Guid Id)
        {
            var token = User.Claims.Where(c => c.Type == "Token").Select(c => c.Value).FirstOrDefault();
            //var Trip = ((await _trips.Read(Id, token)) as ObjectResult).Value as TravelAppModels.Models.Trip;

            

            //foreach (var id in Trip.PlaceIds ?? new Guid[0])
            //{
            //    await _places.Delete(id, false, token);
            //}
            //foreach (var id in Trip.GoalIds ?? new Guid[0])
            //{
            //    await _goals.Delete(id, false, token);
            //}
            //foreach (var id in Trip.GoodIds ?? new Guid[0])
            //{
            //    await _goods.Delete(id, false, token);
            //}
            //foreach (var id in Trip.PurchaseIds ?? new Guid[0])
            //{
            //    await _purchases.Delete(id, false, token);
            //}


            await _trips.Delete(Id, token);
            return StatusCode(StatusCodes.Status200OK, "Ok");
        }

        [HttpGet]
        public async Task<IActionResult> OnGetAddTrip()
        {
            var token = User.Claims.Where(c => c.Type == "Token").Select(c => c.Value).FirstOrDefault();
            var Trip = new TravelAppModels.Models.Trip
            {
                Name = "Новая поездка"
            };

            Trip = ((await _trips.Upsert(Trip, token)) as ObjectResult).Value as TravelAppModels.Models.Trip;

            return StatusCode(StatusCodes.Status200OK, Trip.Id);
        }
    }
}