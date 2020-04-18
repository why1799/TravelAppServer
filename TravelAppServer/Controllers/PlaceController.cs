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
    /// Контроллер для работы с местами
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PlaceController : ControllerBase
    {
        private readonly IStorage Storage;

        public PlaceController(IStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Добавить место
        /// </summary>
        /// <param name="place">Место</param>
        /// <param name="token">Токен</param>
        /// <returns>Добавленная место</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Place))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Upsert([FromBody] Place place, string token)
        {
            try
            {
                if (place == null)
                {
                    throw new ArgumentException("Place can't be null");
                }

                var usertoken = await Storage.FindUserByToken(token);

                if (place.Id == Guid.Empty)
                {
                    place.Id = Guid.NewGuid();
                }

                var readresponse = await Storage.ReadPlace(place.Id);

                if (readresponse != null)
                {
                    if (readresponse.UserId != usertoken.UserId)
                    {
                        throw new ArgumentException("You don't have permission to this place");
                    }
                    place.UserId = readresponse.UserId;
                }
                else
                {
                    place.UserId = usertoken.UserId;
                }

                var response = await Storage.UpsertPlace(place);

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
        /// Добавить место с привязкой к поездке
        /// </summary>
        /// <param name="place">Места</param>
        /// <param name="token">Токен</param>
        /// <returns>Добавленную место</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Place))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> UpsertWithTripId([FromBody] TravelAppModels.ModelsWithTripId.Place place, string token)
        {
            try
            {
                if (place == null)
                {
                    throw new ArgumentException("Place can't be null");
                }

                var usertoken = await Storage.FindUserByToken(token);

                if (place.Id == Guid.Empty)
                {
                    place.Id = Guid.NewGuid();
                }

                var readresponse = await Storage.ReadPlace(place.Id);

                if (readresponse != null)
                {
                    if (readresponse.UserId != usertoken.UserId)
                    {
                        throw new ArgumentException("You don't have permission to this place");
                    }
                    place.UserId = readresponse.UserId;
                }
                else
                {
                    place.UserId = usertoken.UserId;
                }

                var responsetrip = await Storage.ReadTrip(place.TripId);

                if (responsetrip == null)
                {
                    throw new ArgumentException("Such trip doesn't exist");
                }

                if (responsetrip.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this trip");
                }

                if(!responsetrip.PlaceIds?.Contains(place.Id) ?? true)
                {
                    var placeids = responsetrip.PlaceIds?.ToList() ?? new List<Guid>();
                    placeids.Add(place.Id);
                    responsetrip.PlaceIds = placeids.ToArray();
                    await Storage.UpsertTrip(responsetrip);
                }

                var response = await Storage.UpsertPlace(place);

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
        /// Получить место по id
        /// </summary>
        /// <param name="id">id места</param>
        /// <param name="token">Токен</param>
        /// <returns>Место</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Place))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Read(Guid id, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = await Storage.ReadPlace(id);

                if (response == null)
                {
                    throw new ArgumentException("Such place doesn't exist");
                }

                if (response.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this place");
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
        /// Удалить место
        /// </summary>
        /// <param name="id">id места</param>
        /// <param name="deletefromtrip">true - удаляет место из поездки</param>
        /// <param name="token">Токен</param>
        /// <returns>id удаленной места</returns>
        [HttpDelete("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Delete(Guid id, bool deletefromtrip, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var readresponse = await Storage.ReadPlace(id);

                if (readresponse == null)
                {
                    throw new ArgumentException("Such place doesn't exist");
                }

                if (readresponse.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this place");
                }

                var response = await Storage.DeletePlace(id, deletefromtrip);

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
        /// Получить все места
        /// </summary>
        /// <param name="token">Токен</param>
        /// <returns>Список id всех мест</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid[]))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> GetAll(string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = await Storage.GetAllPlaces(usertoken.UserId);

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