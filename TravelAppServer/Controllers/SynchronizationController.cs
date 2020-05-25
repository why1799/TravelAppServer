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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Data))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> GetData(string token, long time = 0)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var Data = new Data();
                Data.Updated = new Update();
                Data.Deleted = new Delete();

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

        
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Data))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> SetData([FromBody] Data Data, string token, long time = 0)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                

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
        /// Получение текущего времени на сервере
        /// </summary>
        /// <returns>Текущее время на сервере</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(long))]
        public ActionResult GetServerTime()
        {
            return StatusCode(StatusCodes.Status200OK, DateTime.UtcNow.Ticks);
        }

        public class Data
        {
            public Update Updated { get; set; }
            public Delete Deleted { get; set; }
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
            public Trip[] Trips { get; set; }
            public Place[] Places { get; set; }
            public Good[] Goods { get; set; }
            public Goal[] Goals { get; set; }
            public Purchase[] Purchases { get; set; }

        }
    }
}