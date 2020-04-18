using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelAppModels;
using TravelAppModels.Models;
using TravelAppModels.Templates;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Controllers
{
    /// <summary>
    /// Контроллер для авторизации
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IStorage Storage;
 
        public AuthController(IStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Авторизация в профиль 
        /// </summary>
        /// <param name="loginTemplate">Email и пароль</param>
        /// <returns>Возвращает токен и id пользователя</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserToken))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Login([FromBody] LoginTemplate loginTemplate)
        {
            var email = loginTemplate.Email;
            var password = loginTemplate.Password;
            try
            {
                if (email == null || password == null || email == "" || password == "")
                {
                    return StatusCode(StatusCodes.Status404NotFound, "Invalid arguments");
                }

                var token = await Storage.FindUser(email, password);
                return StatusCode(StatusCodes.Status200OK, token);
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
        /// Регистрация
        /// </summary>
        /// <param name="registerTemplate">Email, Имя пользователя и пароль</param>
        /// <returns>Возвращает токен и id пользователя</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserToken))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Register([FromBody] RegisterTemplate registerTemplate)
        {
            var username = registerTemplate.Username;
            var email = registerTemplate.Email;
            var password = registerTemplate.Password;

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
                return StatusCode(StatusCodes.Status404NotFound, exeption.Message);
            }
            catch(Exception exeption)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, exeption.Message);
            }
        }

        /// <summary>
        /// Проверка валидности токена
        /// </summary>
        /// <param name="token">Токен</param>
        /// <param name="UserId">Id пользователя</param>
        /// <returns>true - если соответствует, иначе - false</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult<bool>> CheckToken(string token, Guid UserId)
        {
            try
            {
                var result = await Storage.CheckToken(token, UserId);
                return StatusCode(StatusCodes.Status200OK, result);
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