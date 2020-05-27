using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelAppModels.FullModels;
using TravelAppModels.Models;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Controllers
{
    /// <summary>
    /// Контроллер для работы с поездками
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly IStorage Storage;

        public TripController(IStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Добавить поездку
        /// </summary>
        /// <param name="trip">Поездка</param>
        /// <param name="token">Токен</param>
        /// <returns>Добавленная поездка</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Trip))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Upsert([FromBody] Trip trip, string token)
        {
            try
            {
                if(trip == null)
                {
                    throw new ArgumentException("Trip can't be null");
                }

                if(trip.FromDate != null && trip.ToDate != null)
                {
                    if(trip.FromDate.Value > trip.ToDate.Value)
                    {
                        throw new ArgumentException("From date can't be after to date");
                    }
                }

                var usertoken = await Storage.FindUserByToken(token);
                
                if(trip.Id == Guid.Empty)
                {
                    trip.Id = Guid.NewGuid();
                }

                var response = await Storage.UpsertTrip(trip, usertoken.UserId);

                return StatusCode(StatusCodes.Status200OK, response);
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
        /// Добавить поездки
        /// </summary>
        /// <param name="trips">Поездки</param>
        /// <param name="token">Токен</param>
        /// <returns>Добавленные поездки</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Trip))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> UpsertMany([FromBody] Trip[] trips, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                if (trips == null)
                {
                    throw new ArgumentException("Trips can't be null");
                }

                var response = new List<Trip>();

                foreach (var trip in trips)
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

                    response.Add(await Storage.UpsertTrip(trip, usertoken.UserId));
                }

                return StatusCode(StatusCodes.Status200OK, response);
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
        /// Получить поездку по id
        /// </summary>
        /// <param name="id">id поездки</param>
        /// <param name="token">Токен</param>
        /// <returns>Поездка</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Trip))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Read(Guid id, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = await Storage.ReadTrip(id, usertoken.UserId);

                if(response == null)
                {
                    throw new ArgumentException("Such trip doesn't exist");
                }

                return StatusCode(StatusCodes.Status200OK, response);
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
        /// Получить поездку по id с заполненными даными
        /// </summary>
        /// <param name="id">id поездки</param>
        /// <param name="token">Токен</param>
        /// <returns>Поездка</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FullTrip))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> ReadWithData(Guid id, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = new FullTrip(await Storage.ReadTrip(id, usertoken.UserId));

                if (response == null)
                {
                    throw new ArgumentException("Such trip doesn't exist");
                }

                response.Places = await Storage.ReadManyPlaces(response.PlaceIds ?? new Guid[0], usertoken.UserId);
                response.Goods = await Storage.ReadManyGoods(response.GoodIds ?? new Guid[0], usertoken.UserId);
                response.Goals = await Storage.ReadManyGoals(response.GoalIds ?? new Guid[0], usertoken.UserId);
                response.Purchases = await Storage.ReadManyPurchases(response.PurchaseIds ?? new Guid[0], usertoken.UserId);

                return StatusCode(StatusCodes.Status200OK, response);
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

        ///// <summary>
        ///// Получить поездку по id
        ///// </summary>
        ///// <param name="ids">id поездки</param>
        ///// <param name="token">Токен</param>
        ///// <returns>Поездка</returns>
        //[HttpGet("[action]")]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Trip))]
        //[ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        //public async Task<ActionResult> ReadMany([FromForm]Guid[] ids, string token)
        //{
        //    try
        //    {
        //        var usertoken = await Storage.FindUserByToken(token);

        //        var response = await Storage.ReadManyTrips(ids, usertoken.UserId);

        //        if (response == null || response.Length == 0)
        //        {
        //            throw new ArgumentException("Such trip doesn't exist");
        //        }

        //        return StatusCode(StatusCodes.Status200OK, response);
        //    }
        //    catch (ArgumentException exeption)
        //    {
        //        return StatusCode(StatusCodes.Status404NotFound, exeption.Message);
        //    }
        //    catch (Exception exeption)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, exeption.Message);
        //    }
        //}

        /// <summary>
        /// Удалить поездку
        /// </summary>
        /// <param name="id">id поездки</param>
        /// <param name="token">Токен</param>
        /// <returns>id удаленной поездки</returns>
        [HttpDelete("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Delete(Guid id, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = await Storage.DeleteTrip(id, usertoken.UserId);

                Task.Run(async () =>
                {
                    var trip = (await Storage.GetAllTrips(usertoken.UserId, 0, true)).FirstOrDefault(x => x.Id == id);
                    await Storage.DeleteManyPlaces(trip.PlaceIds ?? new Guid[0], false, usertoken.UserId);
                    await Storage.DeleteManyGoals(trip.GoalIds ?? new Guid[0], false, usertoken.UserId);
                    await Storage.DeleteManyGoods(trip.GoodIds ?? new Guid[0], false, usertoken.UserId);
                    await Storage.DeleteManyPurchases(trip.PurchaseIds ?? new Guid[0], false, usertoken.UserId);
                    return;
                });

                return StatusCode(StatusCodes.Status200OK, response);
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
        /// Удалить поездки
        /// </summary>
        /// <param name="ids">id поездкок</param>
        /// <param name="token">Токен</param>
        /// <returns>id удаленных поездок</returns>
        [HttpDelete("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid[]))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> DeleteMany([FromBody] Guid[] ids, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var responses = await Storage.DeleteManyTrips(ids, usertoken.UserId);

                Task.Run(async () =>
                {
                    var trips = (await Storage.GetAllTrips(usertoken.UserId, 0, true)).Where(x => ids.Contains(x.Id));
                    foreach (var trip in trips)
                    {
                        await Storage.DeleteManyPlaces(trip.PlaceIds ?? new Guid[0], false, usertoken.UserId);
                        await Storage.DeleteManyGoals(trip.GoalIds ?? new Guid[0], false, usertoken.UserId);
                        await Storage.DeleteManyGoods(trip.GoodIds ?? new Guid[0], false, usertoken.UserId);
                        await Storage.DeleteManyPurchases(trip.PurchaseIds ?? new Guid[0], false, usertoken.UserId);
                    }
                    return;
                });

                return StatusCode(StatusCodes.Status200OK, responses);
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
        /// Получить id всех поездок
        /// </summary>
        /// <param name="token">Токен</param>
        /// <returns>Список id всех поездок</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid[]))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> GetAllIds(string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = await Storage.GetAllTripIds(usertoken.UserId);

                return StatusCode(StatusCodes.Status200OK, response);
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
        /// Получить все поездки
        /// </summary>
        /// <param name="token">Токен</param>
        /// <returns>Список id всех поездок</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid[]))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> GetAll(string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = await Storage.GetAllTrips(usertoken.UserId);

                return StatusCode(StatusCodes.Status200OK, response);
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
    }
}