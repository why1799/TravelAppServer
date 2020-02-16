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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserToken>> Login([FromQuery] string email, [FromQuery] string password)
        {
            try
            {
                if (email == null || password == null || email == "" || password == "")
                {
                    return StatusCode(StatusCodes.Status404NotFound, "Invalid arguments");
                }

                var token = await Storage.FindUser(email, password);
                return token;
            }
            catch (ArgumentException exeption)
            {
                return StatusCode(StatusCodes.Status404NotFound, exeption);
            }
            catch (Exception exeption)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exeption);
            }
        }

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserToken>> Register([FromQuery] string username, [FromQuery] string email, [FromQuery] string password)
        {
            try
            {
                if(username == null || email == null || password == null || username == "" || email == "" || password == "")
                {
                    return StatusCode(StatusCodes.Status404NotFound, "Invalid arguments");
                }

                var token = await Storage.AddUser(username, email, password);
                return StatusCode(StatusCodes.Status200OK, token);
            }
            catch(ArgumentException exeption)
            {
                return StatusCode(StatusCodes.Status404NotFound, exeption);
            }
            catch(Exception exeption)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exeption);
            }
        }
    }
}