using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")] //autorizacija JWT tokenom
    public class PredictionController : ControllerBase
    {
        [Authorize(Policy = "Rider")]
        [HttpGet]
        public async Task<IActionResult> GetPredictionPrice([FromQuery] TripModel trip) //procena cene voznje, FromQuery radi upita
        {
            PredictionModel prediction = await ServiceProxy.Create<IPredictionService>(new Uri("fabric:/TaxiApp/PredictionService")).GetPredictionPrice(trip.CurrentLocation, trip.Destination);
            if (prediction != null) //ako je predikcija uspesno dobijena
            {

                var response = new //dormiram odgoovr
                {
                    price = prediction, //cenu
                    message = "Succesfuly get prediction" //poruku
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
