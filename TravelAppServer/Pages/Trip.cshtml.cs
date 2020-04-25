using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        public Photo Photo { get; private set; }

        public List<Place> Places { get; private set; }
        public List<Goal> Goals { get; private set; }
        public List<Good> Goods { get; private set; }
        public List<Purchase> Purchases { get; private set; }
        
        public TripModel(IStorage storage)
        {

            _trips = new TripController(storage);
            _places = new PlaceController(storage);
            _goals = new GoalController(storage);
            _goods = new GoodController(storage);
            _photos = new PhotoController(storage);
            _purchases = new PurchaseController(storage);


            Trip = null;
            Photo = null;
            Places = new List<Place>();
            Goals = new List<Goal>();
            Goods = new List<Good>();
            Purchases = new List<Purchase>();
        }

        public async Task OnGet()
        {
            var token = User.Claims.Where(c => c.Type == "Token").Select(c => c.Value).FirstOrDefault();
            Trip = ((await _trips.Read(Id, token)) as ObjectResult).Value as Trip;
            Goals = await GetElements<GoalController, Goal>(_goals, Trip.GoalIds, token);
            Places = await GetElements<PlaceController, Place>(_places, Trip.PlaceIds, token);
            Goods = await GetElements<GoodController, Good>(_goods, Trip.GoodIds, token);
            Purchases = await GetElements<PurchaseController, Purchase>(_purchases, Trip.PurchaseIds, token);
        }

        private async Task<List<Element>> GetElements<Controller, Element>(Controller controller, Guid[] ids, string token) 
        {
            MethodInfo read = controller.GetType().GetMethod("Read",
            new Type[] { typeof(Guid), typeof(string) });

            List<Element> elements = new List<Element>();

            foreach(var id in ids ?? new Guid[0])
            {
                var result = (await (Task<ActionResult>)read.Invoke(controller, new object[] { id, token })) as ObjectResult;
                elements.Add((Element)Convert.ChangeType(result.Value, typeof(Element)));
            }

            return elements;
        }

        public async Task CreateModel(Guid id, string token)
        {
            var a = this.GetType();
            MethodInfo method = _goals.GetType().GetMethod("Read",
            new Type[] { typeof(Guid), typeof(string) });
            var result = (Task<ActionResult>)method.Invoke(_goals, new object[] { id, token });
            //var b = await result;
        }

        public async Task<int> GetCar()
        {
            return 5;
        }

        public async Task<IActionResult> Read(Guid id, string token)
        {
            return StatusCode(StatusCodes.Status200OK, 5);
        }
    }
}