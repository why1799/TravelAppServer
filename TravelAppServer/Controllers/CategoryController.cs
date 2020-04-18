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
    /// Контроллер для работы с категориями
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IStorage Storage;

        public CategoryController(IStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Добавить категорию (В данный момент не работает)
        /// </summary>
        /// <param name="category">Категория</param>
        /// <returns>Возвращает добавленную категорию</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Upsert([FromBody] Category category)
        {
            try
            {
                throw new NotImplementedException();

                if (category == null)
                {
                    throw new ArgumentException("Category can't be null");
                }

                //var usertoken = await Storage.FindUserByToken(token);

                if (category.Id == Guid.Empty)
                {
                    category.Id = Guid.NewGuid();
                }

                var readresponse = await Storage.ReadCategory(category.Id);

                //if (readresponse != null)
                //{
                //    if (readresponse.UserId != usertoken.UserId)
                //    {
                //        throw new ArgumentException("You don't have permission to this goal");
                //    }
                //    goal.UserId = readresponse.UserId;
                //}
                //else
                //{
                //    goal.UserId = usertoken.UserId;
                //}

                var response = await Storage.UpsertCategory(category);

                return StatusCode(StatusCodes.Status200OK, response);
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
        /// Получить категорию по id
        /// </summary>
        /// <param name="id">id категории</param>
        /// <returns>Категорию</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Read(Guid id)
        {
            try
            {
                var response = await Storage.ReadCategory(id);

                if (response == null)
                {
                    throw new ArgumentException("Such category doesn't exist");
                }

                return StatusCode(StatusCodes.Status200OK, response);
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
        /// Получить id всех категорий
        /// </summary>
        /// <returns>Список id категорий</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAllIds()
        {
            try
            {
                var response = await Storage.GetAllCategoryIds();

                return StatusCode(StatusCodes.Status200OK, response);
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
        /// Получить все категории
        /// </summary>
        /// <returns>Список всех категорий</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                var response = await Storage.GetAllCategories();

                return StatusCode(StatusCodes.Status200OK, response);
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