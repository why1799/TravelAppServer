using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelAppModels.Models;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Controllers
{
    /// <summary>
    /// Контроллер для работы с фотографиями
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly IStorage Storage;

        public PhotoController(IStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Загрузка фотографии
        /// </summary>
        /// <param name="photo">Фотография</param>
        /// <param name="token">Токен</param>
        /// <returns>Загруженная фотография</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Photo))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Upload(IFormFile photo, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);
                string base64;
                using (var ms = new MemoryStream())
                {
                    photo.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    base64 = Convert.ToBase64String(fileBytes);
                }

                DirectoryCreater(@$"wwwroot/userfiles");
                DirectoryCreater(@$"wwwroot/userfiles/{usertoken.UserId}");
                var newid = Guid.NewGuid();
                System.IO.File.WriteAllBytes(@$"wwwroot/userfiles/{usertoken.UserId}/{newid}.png", Convert.FromBase64String(base64));
                //var response = await Storage.UploadPhoto(base64, usertoken.UserId);
                return StatusCode(StatusCodes.Status200OK, new Photo { Location = @$"userfiles/{usertoken.UserId}/{newid}", Id = newid, UserId = usertoken.UserId });
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
        /// Загрузка фотографии
        /// </summary>
        /// <param name="base64">Фотография</param>
        /// <param name="token">Токен</param>
        /// <returns>Загруженная фотография</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Photo))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> UploadBase64(string base64, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                DirectoryCreater(@$"wwwroot/userfiles");
                DirectoryCreater(@$"wwwroot/userfiles/{usertoken.UserId}");
                var newid = Guid.NewGuid(); 
                System.IO.File.WriteAllBytes(@$"wwwroot/userfiles/{usertoken.UserId}/{newid}.png", Convert.FromBase64String(base64));
                //var response = await Storage.UploadPhoto(base64, usertoken.UserId);
                return StatusCode(StatusCodes.Status200OK, new Photo { Location = @$"userfiles/{usertoken.UserId}/{newid}.png", Id = newid, UserId = usertoken.UserId });
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
        /// Получение фотографии по id
        /// </summary>
        /// <param name="Id">id фотографии</param>
        /// <param name="token">Токен</param>
        /// <returns>Фотография</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Photo))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Get(Guid Id, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                /*var response = await Storage.GetPhoto(Id);
                if(response.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this photo");
                }*/

                return StatusCode(StatusCodes.Status200OK, new Photo { Location = @$"userfiles/{usertoken.UserId}/{Id}.png", Id = Id, UserId = usertoken.UserId });
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

        private void DirectoryCreater(string path)
        {
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}