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
    /// Контроллер для работы с вещами
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GoodController : ControllerBase
    {
        private readonly IStorage Storage;

        public GoodController(IStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Добавить вещь
        /// </summary>
        /// <param name="good">Вещь</param>
        /// <param name="token">Токен</param>
        /// <returns>Добавленная Вещь</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Good))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Upsert([FromBody] Good good, string token)
        {
            try
            {
                if (good == null)
                {
                    throw new ArgumentException("Good can't be null");
                }

                var usertoken = await Storage.FindUserByToken(token);

                if (good.Id == Guid.Empty)
                {
                    good.Id = Guid.NewGuid();
                }

                var readresponse = await Storage.ReadGood(good.Id);

                if (readresponse != null)
                {
                    if (readresponse.UserId != usertoken.UserId)
                    {
                        throw new ArgumentException("You don't have permission to this good");
                    }
                    good.UserId = readresponse.UserId;
                }
                else
                {
                    good.UserId = usertoken.UserId;
                }

                var response = await Storage.UpsertGood(good);

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
        /// Добавить вещь с привязкой к поездке
        /// </summary>
        /// <param name="good">Вещь</param>
        /// <param name="token">Токен</param>
        /// <returns>Добавленную Вещь</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Good))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> UpsertWithTripId([FromBody] TravelAppModels.ModelsWithTripId.Good good, string token)
        {
            try
            {
                if (good == null)
                {
                    throw new ArgumentException("Good can't be null");
                }

                var usertoken = await Storage.FindUserByToken(token);

                if (good.Id == Guid.Empty)
                {
                    good.Id = Guid.NewGuid();
                }

                var readresponse = await Storage.ReadGood(good.Id);

                if (readresponse != null)
                {
                    if (readresponse.UserId != usertoken.UserId)
                    {
                        throw new ArgumentException("You don't have permission to this good");
                    }
                    good.UserId = readresponse.UserId;
                }
                else
                {
                    good.UserId = usertoken.UserId;
                }

                var responsetrip = await Storage.ReadTrip(good.TripId);

                if (responsetrip == null)
                {
                    throw new ArgumentException("Such trip doesn't exist");
                }

                if (responsetrip.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this trip");
                }


                if (!responsetrip.GoodIds?.Contains(good.Id) ?? true)
                {
                    var goodids = responsetrip.GoodIds?.ToList() ?? new List<Guid>();
                    goodids.Add(good.Id);
                    responsetrip.GoodIds = goodids.ToArray();
                    await Storage.UpsertTrip(responsetrip);
                }

                var response = await Storage.UpsertGood(good);

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
        /// Получить вещь по id
        /// </summary>
        /// <param name="id">id вещи</param>
        /// <param name="token">Токен</param>
        /// <returns>Вещь</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Good))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Read(Guid id, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = await Storage.ReadGood(id);

                if (response == null)
                {
                    throw new ArgumentException("Such good doesn't exist");
                }

                if (response.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this good");
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
        /// Удалить вещь
        /// </summary>
        /// <param name="id">id вещи</param>
        /// <param name="deletefromtrip">true - удаляет вещь из поездки</param>
        /// <param name="token">Токен</param>
        /// <returns>id удаленной вещи</returns>
        [HttpDelete("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Delete(Guid id, bool deletefromtrip, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var readresponse = await Storage.ReadGood(id);

                if (readresponse == null)
                {
                    throw new ArgumentException("Such good doesn't exist");
                }

                if (readresponse.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this good");
                }

                var response = await Storage.DeleteGood(id, deletefromtrip);

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
        /// Получить все вещи
        /// </summary>
        /// <param name="token">Токен</param>
        /// <returns>Список id всех вещей</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid[]))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> GetAll(string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = await Storage.GetAllGoodIds(usertoken.UserId);

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