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
    /// Контроллер для работы с целями
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GoalController : ControllerBase
    {
        private readonly IStorage Storage;

        public GoalController(IStorage storage)
        {
            Storage = storage;
        }

        /// <summary>
        /// Добавить цель
        /// </summary>
        /// <param name="goal">Цель</param>
        /// <param name="token">Токен</param>
        /// <returns>Добавленная цель</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Goal))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Upsert([FromBody] Goal goal, string token)
        {
            try
            {
                if (goal == null)
                {
                    throw new ArgumentException("Goal can't be null");
                }

                var usertoken = await Storage.FindUserByToken(token);

                if (goal.Id == Guid.Empty)
                {
                    goal.Id = Guid.NewGuid();
                }

                var readresponse = await Storage.ReadGoal(goal.Id);

                if (readresponse != null)
                {
                    if (readresponse.UserId != usertoken.UserId)
                    {
                        throw new ArgumentException("You don't have permission to this goal");
                    }
                    goal.UserId = readresponse.UserId;
                }
                else
                {
                    goal.UserId = usertoken.UserId;
                }

                var response = await Storage.UpsertGoal(goal);

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
        /// Добавить цель с привязкой к поездке
        /// </summary>
        /// <param name="goal">Цель</param>
        /// <param name="token">Токен</param>
        /// <returns>Добавленную цель</returns>
        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Goal))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> UpsertWithTripId([FromBody] TravelAppModels.ModelsWithTripId.Goal goal, string token)
        {
            try
            {
                if (goal == null)
                {
                    throw new ArgumentException("Goal can't be null");
                }

                var usertoken = await Storage.FindUserByToken(token);

                if (goal.Id == Guid.Empty)
                {
                    goal.Id = Guid.NewGuid();
                }

                var readresponse = await Storage.ReadGoal(goal.Id);

                if (readresponse != null)
                {
                    if (readresponse.UserId != usertoken.UserId)
                    {
                        throw new ArgumentException("You don't have permission to this goal");
                    }
                    goal.UserId = readresponse.UserId;
                }
                else
                {
                    goal.UserId = usertoken.UserId;
                }

                var responsetrip = await Storage.ReadTrip(goal.TripId);

                if (responsetrip == null)
                {
                    throw new ArgumentException("Such trip doesn't exist");
                }

                if (responsetrip.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this trip");
                }


                if (!responsetrip.GoalIds?.Contains(goal.Id) ?? true)
                {
                    var goalids = responsetrip.GoalIds?.ToList() ?? new List<Guid>();
                    goalids.Add(goal.Id);
                    responsetrip.GoalIds = goalids.ToArray();
                    await Storage.UpsertTrip(responsetrip);
                }

                var response = await Storage.UpsertGoal(goal);

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
        /// Получить цель по id
        /// </summary>
        /// <param name="id">id цели</param>
        /// <param name="token">Токен</param>
        /// <returns>Цель</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Goal))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Read(Guid id, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = await Storage.ReadGoal(id);

                if (response == null)
                {
                    throw new ArgumentException("Such goal doesn't exist");
                }

                if (response.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this goal");
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
        /// Удалить цель
        /// </summary>
        /// <param name="id">id цели</param>
        /// <param name="deletefromtrip">true - удаляет цель из поездки</param>
        /// <param name="token">Токен</param>
        /// <returns>id удаленной цели</returns>
        [HttpDelete("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> Delete(Guid id, bool deletefromtrip, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var readresponse = await Storage.ReadGoal(id);

                if (readresponse == null)
                {
                    throw new ArgumentException("Such goal doesn't exist");
                }

                if (readresponse.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this goal");
                }

                var response = await Storage.DeleteGoal(id, deletefromtrip);

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
        /// Получить все цели
        /// </summary>
        /// <param name="token">Токен</param>
        /// <returns>Список id всех целей</returns>
        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid[]))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<ActionResult> GetAll(string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = await Storage.GetAllGoalIds(usertoken.UserId);

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