using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TravelAppStorage.Settings;

namespace TravelAppServer.Controllers
{
    /// <summary>
    /// Контроллер для определения статуса сервера
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        DBConnection options;

        public StatusController(IOptions<DBConnection> options)
        {
            this.options = options.Value;
        }

        /// <summary>
        /// Проверка подключения к серверу
        /// </summary>
        /// <returns>True, если есть подключение</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public ActionResult<bool> Connection()
        {
            return true;
        }

        /// <summary>
        /// Проверка на наличие подключения к бд
        /// </summary>
        /// <returns>True, если есть подключение. Иначе - False</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public ActionResult<bool> ConnectionToDB()
        {
            try
            {
                var test = new MongoClient(options.StringDBConnection);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}