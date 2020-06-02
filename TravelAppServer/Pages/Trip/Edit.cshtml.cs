using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelAppModels.FullModels;
using TravelAppModels.Models;
using TravelAppServer.Authorize;
using TravelAppServer.Controllers;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Pages.Trip
{
    [MyClaimRequirement]
    [IgnoreAntiforgeryToken]
    public class EditModel : PageModel
    {
        private readonly TripController _trips;
        private readonly PhotoController _photos;
        private readonly FileController _files;
        private readonly CategoryController _categories;

        private readonly SynchronizationController _sync;


        [BindProperty(Name = "id", SupportsGet = true)]
        public Guid Id { get; set; }

        public TravelAppModels.Models.Trip Trip { get; private set; }

        public Category[] Categories { get; private set; }

        public EditModel(IStorage storage)
        {

            _trips = new TripController(storage);
            _photos = new PhotoController(storage);
            _files = new FileController(storage);
            _categories = new CategoryController(storage);

            _sync = new SynchronizationController(storage);

            Trip = null;
        }

        public async Task OnGet()
        {
            var token = HttpContext.Request.Cookies["TraverlApp.fun.Token"];
            var fullTrip = ((await _trips.ReadWithData(Id, token)) as ObjectResult).Value as FullTrip;
            Trip = fullTrip;
            Trip.Goals = fullTrip.Goals;
            Trip.Places = fullTrip.Places;
            Trip.Goods = fullTrip.Goods;
            Trip.Purchases = fullTrip.Purchases;
            Trip.Photos = await GetElements<PhotoController, Photo>(_photos, Trip.PhotoIds, token, "Get");
            Trip.Files = await GetElements<FileController, TravelAppModels.Models.File>(_files, Trip.FileIds, token, "Get", Trip.Id);

            foreach (var place in Trip.Places)
            {
                place.Photos = await GetElements<PhotoController, Photo>(_photos, place.PhotoIds, token, "Get");
            }

            Categories = ((await _categories.GetAll()) as ObjectResult).Value as Category[];
        }

        private async Task<Element[]> GetElements<Controller, Element>(Controller controller, Guid[] ids, string token, string method = "Read", Guid TripId =  new Guid())
        {
            MethodInfo read;
            if (TripId == Guid.Empty)
            {
                read = controller.GetType().GetMethod(method,
                new Type[] { typeof(Guid), typeof(string) });
            }
            else
            {
                read = controller.GetType().GetMethod(method,
                new Type[] { typeof(Guid), typeof(Guid), typeof(string) });
            }

            List<Element> elements = new List<Element>();

            foreach (var id in ids ?? new Guid[0])
            {
                try
                {
                    var result = (await (Task<ActionResult>)read.Invoke(controller, TripId == Guid.Empty ? new object[] { id, token } : new object[] { id, TripId, token })) as ObjectResult;
                    elements.Add((Element)Convert.ChangeType(result.Value, typeof(Element)));
                }
                catch(Exception ex)
                {
                    elements.Add((Element)Convert.ChangeType((Element)Activator.CreateInstance(typeof(Element)), typeof(Element)));
                }
            }

            return elements.ToArray();
        }

        [HttpPost]
        public async Task<IActionResult> OnPostSave(Guid Id, string name, string description, string fromdate, string todate, string photo, string newphoto, string[][] files, string[][] notes, string[][] places, string[][] goods, string[][] goals, string[][] purchases)
        {
            var token = HttpContext.Request.Cookies["TraverlApp.fun.Token"];
            Trip = ((await _trips.Read(Id, token)) as ObjectResult).Value as TravelAppModels.Models.Trip;

            Trip.Name = name;
            Trip.TextField = description;
            Trip.FromDate = fromdate == null ? null : (long?)Convert.ToDateTime(fromdate).Ticks;
            Trip.ToDate = todate == null ? null : (long?)Convert.ToDateTime(todate).Ticks;

            switch (newphoto)
            {
                case "yes":
                    photo = photo.Substring(photo.IndexOf("base64,") + 7);
                    var newguid = Guid.NewGuid();
                    Task.Run(async () => await _photos.UploadBase64(photo, token, newguid));
                    Trip.PhotoIds = new Guid[] { newguid };
                    break;
                case "nophoto":
                    Trip.PhotoIds = null;
                    break;
                default:
                    break;
            }

            var fileid = new List<Guid>();
            foreach (var file in files)
            {
                if (!Guid.TryParse(file[0], out var id))
                {
                    id = Guid.NewGuid();
                }

                if(bool.Parse(file[1]))
                {
                    Task.Run(async () => await _files.UploadBase64(file[2].Substring(file[2].IndexOf("base64,") + 7), file[3], Trip.Id, token, id));
                }

                fileid.Add(id);
            }
            Trip.FileIds = fileid.ToArray();

            var notestoadd = new List<Note>();
            foreach(var note in notes)
            {
                var n = new Note
                {
                    Name = note[0],
                    Description = note[1]
                };

                notestoadd.Add(n);
            }
            Trip.Notes = notestoadd.ToArray();

            List<Place> newplaces = new List<Place>();
            foreach (var place in places)
            {
                var Place = new Place();
                if (Guid.TryParse(place[0], out var id))
                {
                    Place.Id = id;
                }
                else
                {
                    Place.Id = Guid.NewGuid();
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
                        var newguid = Guid.NewGuid();
                        Task.Run(async () => await _photos.UploadBase64(place[6], token, newguid));
                        Place.PhotoIds = new Guid[] { newguid };
                        break;
                    case "nophoto":
                        Place.PhotoIds = null;
                        break;
                    case "no":
                        Place.PhotoIds = new Guid[] { new Guid(Path.ChangeExtension(Path.GetFileName(place[6]), null)) } ;
                        break;
                    case "url":
                        var newguidurl = Guid.NewGuid();
                        Task.Run(async () => await _photos.UploadURL(place[6], token, newguidurl));
                        Place.PhotoIds = new Guid[] { newguidurl };
                        break;
                    default:
                        break;
                }

                newplaces.Add(Place);
            }

            List<Good> newgoods = new List<Good>();
            foreach (var good in goods)
            {
                var Good = new Good();
                if (Guid.TryParse(good[0], out var id))
                {
                    Good.Id = id;
                }
                else
                {
                    Good.Id = Guid.NewGuid();
                }
                Good.Name = good[1];
                Good.IsTook = bool.Parse(good[2]);
                Good.Count = int.Parse(good[3]);

                newgoods.Add(Good);
            }

            List<Goal> newgoals = new List<Goal>();
            foreach (var goal in goals)
            {
                var Goal = new Goal();
                if (Guid.TryParse(goal[0], out var id))
                {
                    Goal.Id = id;
                }
                else
                {
                    Goal.Id = Guid.NewGuid();
                }
                Goal.Name = goal[1];
                Goal.IsDone = bool.Parse(goal[2]);

                newgoals.Add(Goal);
            }

            Categories = ((await _categories.GetAll()) as ObjectResult).Value as Category[];

            List<Purchase> newpurchases = new List<Purchase>();
            foreach (var purchase in purchases)
            {
                var Purchase = new Purchase();
                if (Guid.TryParse(purchase[0], out var id))
                {
                    Purchase.Id = id;
                }
                else
                {
                    Purchase.Id = Guid.NewGuid();
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

                newpurchases.Add(Purchase);
            }


            List<Guid> removeplaces = new List<Guid>();
            foreach (var id in Trip.PlaceIds ?? new Guid[0])
            {
                var found = newplaces.FirstOrDefault(x => x.Id == id);
                if (found == null)
                {
                    removeplaces.Add(id);
                }
            }

            List<Guid> removegoals = new List<Guid>();
            foreach (var id in Trip.GoalIds ?? new Guid[0])
            {
                var found = newgoals.FirstOrDefault(x => x.Id == id);
                if (found == null)
                {
                    removegoals.Add(id);
                }
            }

            List<Guid> removegoods = new List<Guid>();
            foreach (var id in Trip.GoodIds ?? new Guid[0])
            {
                var found = newgoods.FirstOrDefault(x => x.Id == id);
                if (found == null)
                {
                    removegoods.Add(id);
                }
            }

            List<Guid> removepurchases = new List<Guid>();
            foreach (var id in Trip.PurchaseIds ?? new Guid[0])
            {
                var found = newpurchases.FirstOrDefault(x => x.Id == id);
                if (found == null)
                {
                    removepurchases.Add(id);
                }
            }

            Trip.PlaceIds = newplaces.Select(x => x.Id).ToArray();
            Trip.GoalIds = newgoals.Select(x => x.Id).ToArray();
            Trip.GoodIds = newgoods.Select(x => x.Id).ToArray();
            Trip.PurchaseIds = newpurchases.Select(x => x.Id).ToArray();

            SynchronizationController.Data data = new SynchronizationController.Data
            {
                Update = new SynchronizationController.Update
                {
                    Trips = new TravelAppModels.Models.Trip[] { Trip },
                    Places = newplaces.ToArray(),
                    Goods = newgoods.ToArray(),
                    Goals = newgoals.ToArray(),
                    Purchases = newpurchases.ToArray()
                },
                Delete = new SynchronizationController.Delete
                {
                    PlaceIds = removeplaces.ToArray(),
                    GoodIds = removegoods.ToArray(),
                    GoalIds = removegoals.ToArray(),
                    PurchaseIds = removepurchases.ToArray()
                }
            };


            await _sync.SetData(data, token);

            return StatusCode(StatusCodes.Status200OK, "Ok");
        }

        [HttpDelete]
        public async Task<IActionResult> OnDeleteDelete(Guid Id)
        {
            var token = HttpContext.Request.Cookies["TraverlApp.fun.Token"];
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