using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelAppModels;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IStorage Storage;
 
        public AuthController(IStorage storage)
        {
            Storage = storage;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserToken>> Login([FromQuery] string email, [FromQuery] string password)
        {
            var token = await Storage.FindUser(email, password);
            return token;
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserToken>> Register([FromQuery] string username, [FromQuery] string email, [FromQuery] string password)
        {
            var token = await Storage.AddUser(username, email, password);
            return token;
        }
    }
}