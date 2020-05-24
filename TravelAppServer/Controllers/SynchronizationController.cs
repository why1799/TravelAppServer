using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TravelAppServer.Controllers
{
    /// <summary>
    /// Контроллер для синхронизации
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SynchronizationController : ControllerBase
    {


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
    }
}