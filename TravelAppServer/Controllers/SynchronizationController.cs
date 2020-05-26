using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelAppModels.Models;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Controllers
{
    /// <summary>
    /// Контроллер для синхронизации
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SynchronizationController : ControllerBase
    {
        private readonly IStorage Storage;

        public SynchronizationController(IStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Получение всех данных
        /// </summary>
        /// <param name="token">Токен</param>
        /// <param name="time">Время изменения</param>
        /// <returns>Все данные после изменения</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SendData))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> GetData(string token, long time = 0)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var Data = new SendData();
                Data.Updated = new Updated();
                Data.Deleted = new Deleted();

                Data.Updated.Trips = await Storage.GetAllTrips(usertoken.UserId, time);
                Data.Updated.Places = await Storage.GetAllPlaces(usertoken.UserId, time);
                Data.Updated.Goods = await Storage.GetAllGoods(usertoken.UserId, time);
                Data.Updated.Goals = await Storage.GetAllGoals(usertoken.UserId, time);
                Data.Updated.Purchases = await Storage.GetAllPurchases(usertoken.UserId, time);

                Data.Deleted.Trips = await Storage.GetAllTrips(usertoken.UserId, time, true);
                Data.Deleted.Places = await Storage.GetAllPlaces(usertoken.UserId, time, true);
                Data.Deleted.Goods = await Storage.GetAllGoods(usertoken.UserId, time, true);
                Data.Deleted.Goals = await Storage.GetAllGoals(usertoken.UserId, time, true);
                Data.Deleted.Purchases = await Storage.GetAllPurchases(usertoken.UserId, time, true);

                return StatusCode(StatusCodes.Status200OK, Data);
            }
            catch (ArgumentException exeption)
            {
                return StatusCode(StatusCodes.Status404NotFound, exeption.Message);
            }
            catch (Exception exeption)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exeption.Message);
            }
        }

        /// <summary>
        /// Загрузка данных
        /// </summary>
        /// <param name="Data">Данные</param>
        /// <param name="token">токен</param>
        /// <returns>загруженные данные</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SendData))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> SetData([FromBody] Data Data, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                Data.Update = await CreateUpdate(Data, usertoken.UserId);

                Data.Delete.TripIds = await Storage.DeleteManyTrips(Data.Delete.TripIds, usertoken.UserId);
                Data.Delete.PlaceIds = await Storage.DeleteManyPlaces(Data.Delete.PlaceIds, false, usertoken.UserId);
                Data.Delete.GoodIds = await Storage.DeleteManyGoods(Data.Delete.GoodIds, false, usertoken.UserId);
                Data.Delete.GoalIds = await Storage.DeleteManyGoals(Data.Delete.GoalIds, false, usertoken.UserId);
                Data.Delete.PurchaseIds = await Storage.DeleteManyPurchases(Data.Delete.PurchaseIds, false, usertoken.UserId);

                return StatusCode(StatusCodes.Status200OK, Data);
            }
            catch (ArgumentException exeption)
            {
                return StatusCode(StatusCodes.Status404NotFound, exeption.Message);
            }
            catch (Exception exeption)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exeption.Message);
            }
        }

        private async Task<Update> CreateUpdate(Data Data, Guid UserId)
        {
            var UpdatedTrips = new List<Trip>();
            var UpdatedPlaces = new List<Place>();
            var UpdatedGoods = new List<Good>();
            var UpdatedGoals = new List<Goal>();
            var UpdatedPurchases = new List<Purchase>();

            foreach (var trip in Data.Update.Trips ?? new Trip[0])
            {
                if (trip.FromDate != null && trip.ToDate != null)
                {
                    if (trip.FromDate.Value > trip.ToDate.Value)
                    {
                        throw new ArgumentException("From date can't be after to date");
                    }
                }

                if (trip.Id == Guid.Empty)
                {
                    trip.Id = Guid.NewGuid();
                }
                UpdatedTrips.Add(await Storage.UpsertTrip(trip, UserId));
            }

            foreach (var place in Data.Update.Places ?? new Place[0])
            {
                if (place.Id == Guid.Empty)
                {
                    place.Id = Guid.NewGuid();
                }
                UpdatedPlaces.Add(await Storage.UpsertPlace(place, UserId));
            }

            foreach (var good in Data.Update.Goods ?? new Good[0])
            {
                if (good.Id == Guid.Empty)
                {
                    good.Id = Guid.NewGuid();
                }
                UpdatedGoods.Add(await Storage.UpsertGood(good, UserId));
            }

            foreach (var goal in Data.Update.Goals ?? new Goal[0])
            {
                if (goal.Id == Guid.Empty)
                {
                    goal.Id = Guid.NewGuid();
                }
                UpdatedGoals.Add(await Storage.UpsertGoal(goal, UserId));
            }

            foreach (var purchase in Data.Update.Purchases ?? new Purchase[0])
            {
                if (purchase.Id == Guid.Empty)
                {
                    purchase.Id = Guid.NewGuid();
                }
                UpdatedPurchases.Add(await Storage.UpsertPurchase(purchase, UserId));
            }

            return new Update
            {
                Trips = UpdatedTrips.ToArray(),
                Places = UpdatedPlaces.ToArray(),
                Goods = UpdatedGoods.ToArray(),
                Goals = UpdatedGoals.ToArray(),
                Purchases = UpdatedPurchases.ToArray(),
            };
        }

        /// <summary>
        /// Получение текущего времени на сервере
        /// </summary>
        /// <returns>Текущее время на сервере</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(long))]
        public ActionResult GetServerTime()
        {
            return StatusCode(StatusCodes.Status200OK, DateTime.UtcNow.Ticks);
        }

        private class SendData
        {
            public Updated Updated { get; set; }
            public Deleted Deleted { get; set; }
        }

        private class Updated
        {
            public Trip[] Trips { get; set; }
            public Place[] Places { get; set; }
            public Good[] Goods { get; set; }
            public Goal[] Goals { get; set; }
            public Purchase[] Purchases { get; set; }
        }
        private class Deleted
        {
            public Trip[] Trips { get; set; }
            public Place[] Places { get; set; }
            public Good[] Goods { get; set; }
            public Goal[] Goals { get; set; }
            public Purchase[] Purchases { get; set; }
        }

        public class Data
        {
            public Update Update { get; set; }
            public Delete Delete { get; set; }
        }

        public class Update
        {
            public Trip[] Trips { get; set; }
            public Place[] Places { get; set; }
            public Good[] Goods { get; set; }
            public Goal[] Goals { get; set; }
            public Purchase[] Purchases { get; set; }
        }
        public class Delete
        {
            public Guid[] TripIds { get; set; }
            public Guid[] PlaceIds { get; set; }
            public Guid[] GoodIds { get; set; }
            public Guid[] GoalIds { get; set; }
            public Guid[] PurchaseIds { get; set; }
        }
    }
}