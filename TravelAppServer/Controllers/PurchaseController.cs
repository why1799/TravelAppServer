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
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        private readonly IStorage Storage;

        public PurchaseController(IStorage storage)
        {
            Storage = storage;
        }

        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Upsert([FromBody] Purchase purchase, string token)
        {
            try
            {
                if (purchase == null)
                {
                    throw new ArgumentException("Purchase can't be null");
                }

                var usertoken = await Storage.FindUserByToken(token);

                if (purchase.Id == Guid.Empty)
                {
                    purchase.Id = Guid.NewGuid();
                }

                var readresponse = await Storage.ReadPurchase(purchase.Id);

                if (readresponse != null)
                {
                    if (readresponse.UserId != usertoken.UserId)
                    {
                        throw new ArgumentException("You don't have permission to this purchase");
                    }
                    purchase.UserId = readresponse.UserId;
                }
                else
                {
                    purchase.UserId = usertoken.UserId;
                }

                var response = await Storage.UpsertPurchase(purchase);

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

        [HttpPost("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpsertWithTripId([FromBody] TravelAppModels.ModelsWithTripId.Purchase purchase, string token)
        {
            try
            {
                if (purchase == null)
                {
                    throw new ArgumentException("Purchase can't be null");
                }

                var usertoken = await Storage.FindUserByToken(token);

                if (purchase.Id == Guid.Empty)
                {
                    purchase.Id = Guid.NewGuid();
                }

                var readresponse = await Storage.ReadPurchase(purchase.Id);

                if (readresponse != null)
                {
                    if (readresponse.UserId != usertoken.UserId)
                    {
                        throw new ArgumentException("You don't have permission to this purchase");
                    }
                    purchase.UserId = readresponse.UserId;
                }
                else
                {
                    purchase.UserId = usertoken.UserId;
                }

                var responsetrip = await Storage.ReadTrip(purchase.TripId);

                if (responsetrip == null)
                {
                    throw new ArgumentException("Such trip doesn't exist");
                }

                if (responsetrip.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this trip");
                }


                if (!responsetrip.PurchaseIds.Contains(purchase.Id))
                {
                    var purchaseids = responsetrip.PurchaseIds.ToList();
                    purchaseids.Add(purchase.Id);
                    responsetrip.PurchaseIds = purchaseids.ToArray();
                    await Storage.UpsertTrip(responsetrip);
                }

                var response = await Storage.UpsertPurchase(purchase);

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

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Read(Guid id, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = await Storage.ReadPurchase(id);

                if (response == null)
                {
                    throw new ArgumentException("Such purchase doesn't exist");
                }

                if (response.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this purchase");
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

        [HttpDelete("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(Guid id, string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var readresponse = await Storage.ReadPurchase(id);

                if (readresponse == null)
                {
                    throw new ArgumentException("Such purchase doesn't exist");
                }

                if (readresponse.UserId != usertoken.UserId)
                {
                    throw new ArgumentException("You don't have permission to this purchase");
                }

                var response = await Storage.DeletePurchase(id);

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

        [HttpGet("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetAll(string token)
        {
            try
            {
                var usertoken = await Storage.FindUserByToken(token);

                var response = await Storage.GetAllPurchases(usertoken.UserId);

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