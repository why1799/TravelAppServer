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
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        DBConnection options;

        public StatusController(IOptions<DBConnection> options)
        {
            this.options = options.Value;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<bool> Connection()
        {
            return true;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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