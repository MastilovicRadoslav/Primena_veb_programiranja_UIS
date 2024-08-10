using Common.DTOs;
using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;

namespace WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DrivingController : ControllerBase
    {

        [Authorize(Policy = "Rider")]
        [HttpPut]
        public async Task<IActionResult> AcceptSuggestedDrive([FromBody] AcceptedRoadTripModel acceptedRoadTrip)
        {
            try
            {
                if (string.IsNullOrEmpty(acceptedRoadTrip.Destination)) return BadRequest("You must send destination!");
                if (string.IsNullOrEmpty(acceptedRoadTrip.CurrentLocation)) return BadRequest("You must send location!");
                if (acceptedRoadTrip.Accepted == true) return BadRequest("Ride cannot be automaticaly accepted!");
                if (acceptedRoadTrip.Price == 0.0 || acceptedRoadTrip.Price < 0.0) return BadRequest("Invalid price!");


                var fabricClient = new FabricClient();
                RoadTripModel result = null;
                RoadTripModel tripFromRider = new RoadTripModel(acceptedRoadTrip.CurrentLocation, acceptedRoadTrip.Destination, acceptedRoadTrip.RiderId, acceptedRoadTrip.Price, acceptedRoadTrip.Accepted, acceptedRoadTrip.MinutesToDriverArrive);
                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);
                    var partitionResult = await proxy.AcceptRoadTrip(tripFromRider);
                    if (partitionResult != null)
                    {
                        result = partitionResult;
                        break;
                    }
                }

                if (result != null)
                {
                    var response = new
                    {
                        Drive = result,
                        message = "Successfully scheduled"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("You already submited ticked!");
                }


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while accepting new drive!");
            }
        }

        [Authorize(Policy = "Driver")]
        [HttpPut]
        public async Task<IActionResult> AcceptNewRide([FromBody] RideForAcceptDTO ride)
        {
            try
            {
                var fabricClient = new FabricClient();
                RoadTripModel result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);
                    var partitionResult = await proxy.AcceptRoadTripDriver(ride.RideId, ride.DriverId);
                    if (partitionResult != null)
                    {
                        result = partitionResult;
                        break;
                    }
                }

                if (result != null)
                {
                    var response = new
                    {
                        ride = result,
                        message = "Sucessfuly accepted driver!"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("This id does not exist");
                }

            }
            catch
            {
                return BadRequest("Something went wrong!");
            }
        }

        [Authorize(Policy = "Driver")]
        [HttpGet]
        public async Task<IActionResult> GetAllUncompletedRides()
        {
            try
            {

                var fabricClient = new FabricClient();
                List<RoadTripModel> result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);
                    var parititonResult = await proxy.GetRoadTrips();
                    if (parititonResult != null)
                    {
                        result = parititonResult;
                        break;
                    }

                }

                if (result != null)
                {

                    var response = new
                    {
                        rides = result,
                        message = "Succesfuly get list of not completed rides"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("Incorrect email or password");
                }

            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while registering new User");
            }
        }

        [Authorize(Policy = "Driver")]
        [HttpGet]
        public async Task<IActionResult> GetCompletedRidesForDriver([FromQuery] Guid id)
        {
            try
            {

                var fabricClient = new FabricClient();
                List<RoadTripModel> result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);
                    var parititonResult = await proxy.GetListOfCompletedRidesForDriver(id);
                    if (parititonResult != null)
                    {
                        result = parititonResult;
                        break;
                    }

                }

                if (result != null)
                {

                    var response = new
                    {
                        rides = result,
                        message = "Succesfuly get list completed rides"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("Incorrect email or password");
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [Authorize(Policy = "Rider")]
        [HttpGet]
        public async Task<IActionResult> GetCompletedRidesForRider([FromQuery] Guid id)
        {
            try
            {

                var fabricClient = new FabricClient();
                List<RoadTripModel> result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);
                    var parititonResult = await proxy.GetListOfCompletedRidesForRider(id);
                    if (parititonResult != null)
                    {
                        result = parititonResult;
                        break;
                    }

                }

                if (result != null)
                {

                    var response = new
                    {
                        rides = result,
                        message = "Succesfuly get list completed rides"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("Incorrect email or password");
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetCompletedRidesAdmin()
        {
            try
            {

                var fabricClient = new FabricClient();
                List<RoadTripModel> result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);
                    var parititonResult = await proxy.GetListOfCompletedRidesAdmin();
                    if (parititonResult != null)
                    {
                        result = parititonResult;
                        break;
                    }

                }

                if (result != null)
                {

                    var response = new
                    {
                        rides = result,
                        message = "Succesfuly get list completed rides"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("Incorrect email or password");
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Authorize(Policy = "Rider")]
        [HttpGet]
        public async Task<IActionResult> GetCurrentTrip(Guid id)
        {
            try
            {

                var fabricClient = new FabricClient();
                RoadTripModel result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);
                    var parititonResult = await proxy.GetCurrentTrip(id);
                    if (parititonResult != null)
                    {
                        result = parititonResult;
                        break;
                    }

                }

                if (result != null)
                {

                    var response = new
                    {
                        trip = result,
                        message = "Succesfuly get current ride"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Authorize(Policy = "Driver")]
        [HttpGet]
        public async Task<IActionResult> GetCurrentTripDriver(Guid id)
        {
            try
            {

                var fabricClient = new FabricClient();
                RoadTripModel result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);
                    var parititonResult = await proxy.GetCurrentTripDriver(id);
                    if (parititonResult != null)
                    {
                        result = parititonResult;
                        break;
                    }

                }

                if (result != null)
                {

                    var response = new
                    {
                        trip = result,
                        message = "Succesfuly get current ride"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
