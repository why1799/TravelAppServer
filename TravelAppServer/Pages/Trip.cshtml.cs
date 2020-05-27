using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelAppModels.FullModels;
using TravelAppModels.Models;
using TravelAppServer.Controllers;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Pages
{
    [Authorize]
    public class TripModel : PageModel
    {
        private readonly TripController _trips;
        private readonly PhotoController _photos;
        private readonly GoodController _goods;
        private readonly PlaceController _places;
        private readonly GoalController _goals;
        private readonly PurchaseController _purchases;
        private readonly CategoryController _categories;


        [BindProperty(Name = "id", SupportsGet = true)]
        public Guid Id { get; set; }

        public TravelAppModels.Models.Trip Trip { get; private set; }

        public Category[] Categories { get; private set; }

        public TripModel(IStorage storage)
        {

            _trips = new TripController(storage);
            _places = new PlaceController(storage);
            _goals = new GoalController(storage);
            _goods = new GoodController(storage);
            _photos = new PhotoController(storage);
            _purchases = new PurchaseController(storage);
            _categories = new CategoryController(storage);

            Trip = null;
        }

        public async Task OnGet()
        {
            var token = User.Claims.Where(c => c.Type == "Token").Select(c => c.Value).FirstOrDefault();
            var fullTrip = ((await _trips.ReadWithData(Id, token)) as ObjectResult).Value as FullTrip;
            Trip = fullTrip;
            Trip.Goals = fullTrip.Goals;
            Trip.Places = fullTrip.Places;
            Trip.Goods = fullTrip.Goods;
            Trip.Purchases = fullTrip.Purchases;
            Trip.Photos = await GetElements<PhotoController, Photo>(_photos, Trip.PhotoIds, token, "Get");
            
            foreach(var place in Trip.Places)
            {
                place.Photos = await GetElements<PhotoController, Photo>(_photos, place.PhotoIds, token, "Get");
            }

            Categories = ((await _categories.GetAll()) as ObjectResult).Value as Category[];
        }

        private async Task<Element[]> GetElements<Controller, Element>(Controller controller, Guid[] ids, string token, string method = "Read") 
        {
            MethodInfo read = controller.GetType().GetMethod(method,
            new Type[] { typeof(Guid), typeof(string) });

            List<Element> elements = new List<Element>();

            foreach(var id in ids ?? new Guid[0])
            {
                var result = (await (Task<ActionResult>)read.Invoke(controller, new object[] { id, token })) as ObjectResult;
                elements.Add((Element)Convert.ChangeType(result.Value, typeof(Element)));
            }

            return elements.ToArray();
        }

        [HttpDelete]
        public async Task<IActionResult> OnDeleteDelete(Guid Id)
        {
            var token = User.Claims.Where(c => c.Type == "Token").Select(c => c.Value).FirstOrDefault();
            //Trip = ((await _trips.Read(Id, token)) as ObjectResult).Value as TravelAppModels.Models.Trip;

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
    }
}