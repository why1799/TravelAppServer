using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelAppModels.FullModels;
using TravelAppModels.Models;
using TravelAppServer.Controllers;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Pages.Trip
{
    [Authorize]
    public class EditModel : PageModel
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

        public EditModel(IStorage storage)
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

            foreach (var place in Trip.Places)
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

            foreach (var id in ids ?? new Guid[0])
            {
                var result = (await (Task<ActionResult>)read.Invoke(controller, new object[] { id, token })) as ObjectResult;
                elements.Add((Element)Convert.ChangeType(result.Value, typeof(Element)));
            }

            return elements.ToArray();
        }

        [HttpPost]
        public async Task<IActionResult> OnPostSave(Guid Id, string name, string description, string fromdate, string todate, string photo, string newphoto, string[][] places, string[][] goods, string[][] goals, string[][] purchases)
        {
            var token = User.Claims.Where(c => c.Type == "Token").Select(c => c.Value).FirstOrDefault();
            Trip = ((await _trips.Read(Id, token)) as ObjectResult).Value as TravelAppModels.Models.Trip;

            Trip.Name = name;
            Trip.TextField = description;
            Trip.FromDate = fromdate == null ? null : (long?)Convert.ToDateTime(fromdate).Ticks;
            Trip.ToDate = todate == null ? null : (long?)Convert.ToDateTime(todate).Ticks;

            switch (newphoto)
            {
                case "yes":
                    photo = photo.Substring(photo.IndexOf("base64,") + 7);
                    var addedphoto = ((await _photos.UploadBase64(photo, token)) as ObjectResult).Value as Photo;
                    Trip.PhotoIds = new Guid[] { addedphoto.Id };
                    break;
                case "nophoto":
                    Trip.PhotoIds = null;
                    break;
                default:
                    break;
            }

            List<Guid> placeIds = new List<Guid>();
            foreach (var place in places)
            {
                var Place = new Place();
                if (Guid.TryParse(place[0], out var id))
                {
                    Place = ((await _places.Read(id, token)) as ObjectResult).Value as TravelAppModels.Models.Place;
                }
                Place.Name = place[1];
                Place.Description = place[2];
                Place.Adress = place[3];
                Place.IsVisited = bool.Parse(place[4]);
                Place.Date = place[5] == null ? null : (long?)Convert.ToDateTime(place[5]).Ticks;
                switch (place[7])
                {
                    case "yes":
                        place[6] = place[6].Substring(place[6].IndexOf("base64,") + 7);
                        var addedphoto = ((await _photos.UploadBase64(place[6], token)) as ObjectResult).Value as Photo;
                        Place.PhotoIds = new Guid[] { addedphoto.Id };
                        break;
                    case "nophoto":
                        Place.PhotoIds = null;
                        break;
                    default:
                        break;
                }

                Place = ((await _places.Upsert(Place, token)) as ObjectResult).Value as TravelAppModels.Models.Place;
                placeIds.Add(Place.Id);
            }

            List<Guid> goodids = new List<Guid>();
            foreach (var good in goods)
            {
                var Good = new Good();
                if (Guid.TryParse(good[0], out var id))
                {
                    Good = ((await _goods.Read(id, token)) as ObjectResult).Value as TravelAppModels.Models.Good;
                }
                Good.Name = good[1];
                Good.IsTook = bool.Parse(good[2]);
                Good.Count = int.Parse(good[3]);

                Good = ((await _goods.Upsert(Good, token)) as ObjectResult).Value as TravelAppModels.Models.Good;
                goodids.Add(Good.Id);
            }

            List<Guid> goalids = new List<Guid>();
            foreach (var goal in goals)
            {
                var Goal = new Goal();
                if (Guid.TryParse(goal[0], out var id))
                {
                    Goal = ((await _goals.Read(id, token)) as ObjectResult).Value as TravelAppModels.Models.Goal;
                }
                Goal.Name = goal[1];
                Goal.IsDone = bool.Parse(goal[2]);


                Goal = ((await _goals.Upsert(Goal, token)) as ObjectResult).Value as TravelAppModels.Models.Goal;
                goalids.Add(Goal.Id);
            }

            Categories = ((await _categories.GetAll()) as ObjectResult).Value as Category[];

            List<Guid> purchaseids = new List<Guid>();
            foreach (var purchase in purchases)
            {
                var Purchase = new Purchase();
                if (Guid.TryParse(purchase[0], out var id))
                {
                    Purchase = ((await _purchases.Read(id, token)) as ObjectResult).Value as TravelAppModels.Models.Purchase;
                }
                Purchase.Name = purchase[1];
                Purchase.CategoryId = Guid.Empty;
                foreach (var cat in Categories)
                {
                    if (cat.Name == purchase[2])
                    {
                        Purchase.CategoryId = cat.Id;
                    }
                }

                Purchase.Price = double.Parse((purchase[3].Replace('.', ',')));
                Purchase.IsBought = bool.Parse(purchase[4]);


                Purchase = ((await _purchases.Upsert(Purchase, token)) as ObjectResult).Value as TravelAppModels.Models.Purchase;
                purchaseids.Add(Purchase.Id);
            }

            foreach (var id in Trip.PlaceIds ?? new Guid[0])
            {
                var found = placeIds.FirstOrDefault(x => x == id);
                if (found == Guid.Empty)
                {
                    await _places.Delete(id, false, token);
                }
            }

            foreach (var id in Trip.GoalIds ?? new Guid[0])
            {
                var found = goalids.FirstOrDefault(x => x == id);
                if (found == Guid.Empty)
                {
                    await _goals.Delete(id, false, token);
                }
            }

            foreach (var id in Trip.GoodIds ?? new Guid[0])
            {
                var found = goodids.FirstOrDefault(x => x == id);
                if (found == Guid.Empty)
                {
                    await _goods.Delete(id, false, token);
                }
            }

            foreach (var id in Trip.PurchaseIds ?? new Guid[0])
            {
                var found = purchaseids.FirstOrDefault(x => x == id);
                if (found == Guid.Empty)
                {
                    await _purchases.Delete(id, false, token);
                }
            }


            Trip.PlaceIds = placeIds.ToArray();
            Trip.GoalIds = goalids.ToArray();
            Trip.GoodIds = goodids.ToArray();
            Trip.PurchaseIds = purchaseids.ToArray();

            await _trips.Upsert(Trip, token);

            return StatusCode(StatusCodes.Status200OK, "Ok");
        }

        [HttpDelete]
        public async Task<IActionResult> OnDeleteDelete(Guid Id)
        {
            var token = User.Claims.Where(c => c.Type == "Token").Select(c => c.Value).FirstOrDefault();
            //Trip = ((await _trips.Read(Id, token)) as ObjectResult).Value as TravelAppModels.Models.Trip;

            //foreach(var id in Trip.PlaceIds ?? new Guid[0])
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