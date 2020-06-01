using System;
using System.Drawing;
using System.IO;
using System.Net;
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
        /// <param name="Id">Id (необязательно)</param>
        /// <returns>Загруженная фотография</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Photo))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Upload(IFormFile photo, string token, Guid Id = new Guid())
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
                var newid = Id == Guid.Empty ? Guid.NewGuid() : Id;
                await System.IO.File.WriteAllBytesAsync(@$"wwwroot/userfiles/{usertoken.UserId}/{newid}.png", Convert.FromBase64String(base64));
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
        /// <param name="Id">Id (необязательно)</param>
        /// <returns>Загруженная фотография</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Photo))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> UploadBase64(string base64, string token, Guid Id = new Guid())
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                DirectoryCreater(@$"wwwroot/userfiles");
                DirectoryCreater(@$"wwwroot/userfiles/{usertoken.UserId}");
                var newid = Id == Guid.Empty ? Guid.NewGuid() : Id; 
                await System.IO.File.WriteAllBytesAsync(@$"wwwroot/userfiles/{usertoken.UserId}/{newid}.png", Convert.FromBase64String(base64));
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
        /// Загрузка фотографии по URL
        /// </summary>
        /// <param name="url">Ссылка на фотографию</param>
        /// <param name="token">Токен</param>
        /// <param name="Id">Id (необязательно)</param>
        /// <returns>Загруженная фотография</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Photo))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> UploadURL(string url, string token, Guid Id = new Guid())
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                DirectoryCreater(@$"wwwroot/userfiles");
                DirectoryCreater(@$"wwwroot/userfiles/{usertoken.UserId}");
                var newid = Id == Guid.Empty ? Guid.NewGuid() : Id;
                var path = @$"wwwroot/userfiles/{usertoken.UserId}/{newid}.png";

                using var client = new WebClient();
                var data = await client.DownloadDataTaskAsync(url);

                // Example use:
                Bitmap source = System.Drawing.Image.FromStream(new MemoryStream(data)) as Bitmap;

                int x, y, width, height;

                if (source.Width > source.Height)
                {
                    height = width = source.Height;
                    y = 0;
                    x = (source.Width - source.Height) / 2;
                }
                else
                {
                    height = width = source.Width;
                    y = (source.Height - source.Width) / 2;
                    x = 0;
                }

                Rectangle section = new Rectangle(new Point(x, y), new Size(width, height));
                Bitmap CroppedImage = CropImage(source, section);
                CroppedImage.Save(path);

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

        private Bitmap CropImage(Bitmap source, Rectangle section)
        {
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
                return bitmap;
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