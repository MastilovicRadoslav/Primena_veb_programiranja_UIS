using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        [Authorize(Policy = "Rider")]
        [HttpGet]
        public async Task<IActionResult> GetPredictionPrice([FromQuery] TripModel trip)
        {
            PredictionModel prediction = await ServiceProxy.Create<IPredictionService>(new Uri("fabric:/TaxiApp/PredictionService")).GetPredictionPrice(trip.CurrentLocation, trip.Destination);
            if (prediction != null)
            {

                var response = new
                {
                    price = prediction,
                    message = "Succesfuly get prediction"
                };
                return Ok(response);
            }
            else
            {
                return StatusCode(500, "An error occurred while predicted price");
            }

        }
    }
}
