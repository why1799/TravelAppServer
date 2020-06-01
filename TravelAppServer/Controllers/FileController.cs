using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TravelAppStorage.Interfaces;

namespace TravelAppServer.Controllers
{
    /// <summary>
    /// Контроллер для работы с файлами
    /// </summary>
    public class FileController : Controller
    {
        private readonly IStorage Storage;

        public FileController(IStorage storage)
        {
            Storage = storage;
        }
        /// <summary>
        /// Загрузка файла
        /// </summary>
        /// <param name="file">Файл</param>
        /// <param name="TripId">Id поездки, к которой относится файл</param>
        /// <param name="token">Токен</param>
        /// <param name="Id">Id (необязательно)</param>
        /// <returns>Загруженный файл</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TravelAppModels.Models.File))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Upload(IFormFile file, Guid TripId, string token, Guid Id = new Guid())
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);
                string base64;
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    base64 = Convert.ToBase64String(fileBytes);
                }
                DirectoryCreater(@$"wwwroot/userfiles");
                DirectoryCreater(@$"wwwroot/userfiles/{usertoken.UserId}");
                DirectoryCreater(@$"wwwroot/userfiles/{usertoken.UserId}/{TripId}");
                var newid = Id == Guid.Empty ? Guid.NewGuid() : Id;
                await System.IO.File.WriteAllBytesAsync(@$"wwwroot/userfiles/{usertoken.UserId}/{TripId}/{newid}_{file.FileName}", Convert.FromBase64String(base64));
                return StatusCode(StatusCodes.Status200OK, new TravelAppModels.Models.File { Location = @$"userfiles/{usertoken.UserId}/{TripId}/{newid}_{file.FileName}", Id = newid, UserId = usertoken.UserId, Name = file.FileName, TripId = TripId });
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
        /// Загрузка файла
        /// </summary>
        /// <param name="base64">Файл</param>
        /// <param name="name">Название файла (С расширением)</param>
        /// <param name="TripId">Id поездки, к которой относится файл</param>
        /// <param name="token">Токен</param>
        /// <param name="Id">Id (необязательно)</param>
        /// <returns>Загруженный файл</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TravelAppModels.Models.File))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> UploadBase64(string base64, string name, Guid TripId, string token, Guid Id = new Guid())
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new NullReferenceException(nameof(name));
                }

                var usertoken = await Storage.FindUserByToken(token);

                DirectoryCreater(@$"wwwroot/userfiles");
                DirectoryCreater(@$"wwwroot/userfiles/{usertoken.UserId}");
                DirectoryCreater(@$"wwwroot/userfiles/{usertoken.UserId}/{TripId}");
                var newid = Id == Guid.Empty ? Guid.NewGuid() : Id;
                await System.IO.File.WriteAllBytesAsync(@$"wwwroot/userfiles/{usertoken.UserId}/{TripId}/{newid}_{name}", Convert.FromBase64String(base64));
                return StatusCode(StatusCodes.Status200OK, new TravelAppModels.Models.File { Location = @$"userfiles/{usertoken.UserId}/{TripId}/{newid}_{name}", Id = newid, UserId = usertoken.UserId, Name = name, TripId = TripId });
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
        /// Получение файла
        /// </summary>
        /// <param name="Id">id файла</param>
        /// <param name="TripId">id поездки, к которой относится файл</param>
        /// <param name="token">Токен</param>
        /// <returns>Файл</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TravelAppModels.Models.File))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Get(Guid Id, Guid TripId, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var files = Directory.GetFiles(@$"wwwroot/userfiles/{usertoken.UserId}/{TripId}", $"{Id}_*");

                if(files.Length != 1)
                {
                    throw new FileNotFoundException("File doesn't exist");
                }

                var sep = files[0].Split($"{Id}_");

                var name = sep[1];

                return StatusCode(StatusCodes.Status200OK, new TravelAppModels.Models.File { Location = @$"userfiles/{usertoken.UserId}/{TripId}/{Id}_{name}", Id = Id, UserId = usertoken.UserId, TripId = TripId, Name = name });
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
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

    }
}