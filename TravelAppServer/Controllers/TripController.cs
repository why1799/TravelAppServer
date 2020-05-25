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
    }
}