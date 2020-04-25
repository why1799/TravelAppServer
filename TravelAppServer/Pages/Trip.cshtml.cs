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


        [BindProperty(Name = "id", SupportsGet = true)]
        public Guid Id { get; set; }

        public Trip Trip { get; private set; }
        
        public TripModel(IStorage storage)
        {

            _trips = new TripController(storage);
            _places = new PlaceController(storage);
            _goals = new GoalController(storage);
            _goods = new GoodController(storage);
            _photos = new PhotoController(storage);
            _purchases = new PurchaseController(storage);

            Trip = null;
        }

        public async Task OnGet()
        {
            var token = User.Claims.Where(c => c.Type == "Token").Select(c => c.Value).FirstOrDefault();
            Trip = ((await _trips.Read(Id, token)) as ObjectResult).Value as Trip;
            Trip.Goals = await GetElements<GoalController, Goal>(_goals, Trip.GoalIds, token);
            Trip.Places = await GetElements<PlaceController, Place>(_places, Trip.PlaceIds, token);
            Trip.Goods = await GetElements<GoodController, Good>(_goods, Trip.GoodIds, token);
            Trip.Purchases = await GetElements<PurchaseController, Purchase>(_purchases, Trip.PurchaseIds, token);
            Trip.Photos = await GetElements<PhotoController, Photo>(_photos, Trip.PhotoIds, token, "Get");
            
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
    }
}