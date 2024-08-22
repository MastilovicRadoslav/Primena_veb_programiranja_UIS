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
        public async Task<IActionResult> AcceptSuggestedDrive([FromBody] AcceptedRoadTripModel acceptedRoadTrip)//Slicno kao registracija samo za Voznje
        {
            try
            {   //Valdacija unosa
                if (string.IsNullOrEmpty(acceptedRoadTrip.Destination)) return BadRequest("You must send destination!");
                if (string.IsNullOrEmpty(acceptedRoadTrip.CurrentLocation)) return BadRequest("You must send location!");
                if (acceptedRoadTrip.Accepted == true) return BadRequest("Ride cannot be automaticaly accepted!");
                if (acceptedRoadTrip.Price == 0.0 || acceptedRoadTrip.Price < 0.0) return BadRequest("Invalid price!");

                //Kreiranje Fabric klijenta
                var fabricClient = new FabricClient();
                RoadTripModel result = null;
                //Kreiranje RoadTripModel instance na osnovu podataka sa frontenda
                RoadTripModel tripFromRider = new RoadTripModel(
                    acceptedRoadTrip.CurrentLocation,
                    acceptedRoadTrip.Destination,
                    acceptedRoadTrip.RiderId,
                    acceptedRoadTrip.Price,
                    acceptedRoadTrip.Accepted,
                    acceptedRoadTrip.MinutesToDriverArrive);
                //Preuzimanje liste particija za servis
                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));
                //Prolazak kroz sve particije da bi se pronasla odgovarajuca
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);
                    //Pozivanje metode AcceptRoadTrip na proxy-ju
                    var partitionResult = await proxy.AcceptRoadTrip(tripFromRider);
                    if (partitionResult != null)
                    {
                        result = partitionResult;
                        break;
                    }
                }
                // Vraćanje odgovora na osnovu rezultata
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

        [Authorize(Policy = "Driver")] // Ova metoda je dostupna samo korisnicima sa ulogom "Driver"
        [HttpPut] // Ova metoda odgovara na HTTP PUT zahteve
        public async Task<IActionResult> AcceptNewRide([FromBody] RideForAcceptDTOs ride)
        {
            try
            {
                var fabricClient = new FabricClient(); // Kreiramo FabricClient za komunikaciju sa Service Fabricom
                RoadTripModel result = null; // Promenljiva koja će držati rezultat nakon prihvatanja vožnje

                // Dobijamo listu particija za "DrivingService"
                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));

                // Iteriramo kroz svaku particiju
                foreach (var partition in partitionList)
                {
                    // Pravimo ključ particije za komunikaciju
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);

                    // Kreiramo proxy za komunikaciju sa IDriveService
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);

                    // Pozivamo metodu AcceptRoadTripDriver na proxy-u i dobijamo rezultat
                    var partitionResult = await proxy.AcceptRoadTripDriver(ride.RideId, ride.DriverId);

                    if (partitionResult != null)
                    {
                        result = partitionResult; // Ako je rezultat dobijen, postavljamo ga kao naš rezultat
                        break; // Prekidamo petlju nakon što pronađemo odgovarajući rezultat
                    }
                }

                if (result != null)
                {
                    // Ako je vožnja uspešno prihvaćena, vraćamo je kao JSON odgovor
                    var response = new
                    {
                        ride = result,
                        message = "Successfully accepted ride!"
                    };
                    return Ok(response); // HTTP 200 status
                }
                else
                {
                    // Ako vožnja ne postoji, vraćamo BadRequest
                    return BadRequest("This ride ID does not exist");
                }

            }
            catch
            {
                // U slučaju greške, vraćamo BadRequest
                return BadRequest("Something went wrong!");
            }
        }



        [Authorize(Policy = "Driver")] // Ova metoda je dostupna samo korisnicima sa ulogom "Driver"
        [HttpGet] // Ova metoda odgovara na HTTP GET zahteve
        public async Task<IActionResult> GetAllUncompletedRides()
        {
            try
            {
                var fabricClient = new FabricClient(); // Kreiramo FabricClient koji omogućava komunikaciju sa Service Fabric-om
                List<RoadTripModel> result = null; // Ova promenljiva će držati rezultat sa svih particija

                // Dobijamo listu svih particija za "DrivingService"
                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));

                // Iteriramo kroz svaku particiju
                foreach (var partition in partitionList)
                {
                    // Pravimo ključ particije za komunikaciju
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);

                    // Kreiramo proxy za komunikaciju sa IDriveService
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);

                    // Pozivamo metodu GetRoadTrips na proxy-u i dobijamo rezultat
                    var parititonResult = await proxy.GetRoadTrips();

                    if (parititonResult != null)
                    {
                        result = parititonResult; // Ako rezultat nije null, postavljamo ga kao naš rezultat
                        break; // Prekidamo petlju jer smo našli nekompletirane vožnje
                    }
                }

                if (result != null)
                {
                    // Vraćamo listu vožnji koje nisu završene kao JSON odgovor
                    var response = new
                    {
                        rides = result,
                        message = "Successfully retrieved list of uncompleted rides"
                    };
                    return Ok(response); // HTTP 200 status
                }
                else
                {
                    // Ako nije pronađeno nijedno nekompletirano putovanje, vraćamo BadRequest
                    return BadRequest("No uncompleted rides found");
                }

            }
            catch (Exception)
            {
                // U slučaju greške vraćamo HTTP 500 status
                return StatusCode(500, "An error occurred while retrieving uncompleted rides");
            }
        }


        [Authorize(Policy = "Driver")]
        [HttpGet]
        public async Task<IActionResult> GetCompletedRidesForDriver([FromQuery] Guid id) //Dobijanje liste voznji za odredjenog vozaca
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
        public async Task<IActionResult> GetCompletedRidesForRider([FromQuery] Guid id) //Dobijanje liste voznji za odredjenog korisnika
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
        public async Task<IActionResult> GetCompletedRidesAdmin() //Dobavlja sve voznje bez uslova
        {
            try
            {
                var fabricClient = new FabricClient();
                List<RoadTripModel> result = null;

                // Dobavljanje liste particija za servis
                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));

                // Prolazak kroz svaku particiju
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);
                    var partitionResult = await proxy.GetListOfCompletedRidesAdmin();

                    // Ako se dobiju rezultati iz particije, postavi rezultat i prekini petlju
                    if (partitionResult != null)
                    {
                        result = partitionResult;
                        break;
                    }
                }

                // Ako je rezultat dobijen, vrati ga sa statusom 200
                if (result != null)
                {
                    var response = new
                    {
                        rides = result,
                        message = "Successfully retrieved the list of completed rides"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("No completed rides found.");
                }
            }
            catch (Exception ex)
            {
                throw; // Ponovo baci izuzetak da bi bio zabeležen u logovima
            }
        }



        [Authorize(Policy = "Rider")] // Ova metoda je dostupna samo korisnicima sa ulogom 'Rider'
        [HttpGet]
        public async Task<IActionResult> GetCurrentTrip(Guid id) // Ova metoda prima `id` korisnika
        {
            try
            {
                var fabricClient = new FabricClient(); // Kreiramo FabricClient za komunikaciju sa Service Fabric klasterom
                RoadTripModel result = null;

                // Dobavljamo listu svih particija za servis `DrivingService`
                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));

                // Iteriramo kroz sve particije kako bismo našli odgovarajući servis koji sadrži potrebne podatke
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);
                    var parititonResult = await proxy.GetCurrentTrip(id); // Pozivamo metodu da dobavimo trenutnu vožnju za korisnika

                    if (parititonResult != null)
                    {
                        result = parititonResult; // Ako pronađemo podatke, prekidamo petlju
                        break;
                    }
                }

                // Ako smo pronašli trenutnu vožnju, vraćamo je klijentu
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
                    return BadRequest(); // Ako nema aktivne vožnje, vraćamo grešku
                }

            }
            catch (Exception ex)
            {
                throw; // U slučaju greške, izbacujemo izuzetak
            }
        }



        [Authorize(Policy = "Driver")] // Ova metoda je dostupna samo korisnicima sa ulogom 'Driver'
        [HttpGet]
        public async Task<IActionResult> GetCurrentTripDriver(Guid id) // Ova metoda prima `id` vozača
        {
            try
            {
                var fabricClient = new FabricClient(); // Kreiramo FabricClient za komunikaciju sa Service Fabric klasterom
                RoadTripModel result = null;

                // Dobavljamo listu svih particija za servis `DrivingService`
                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));

                // Iteriramo kroz sve particije kako bismo našli odgovarajući servis koji sadrži potrebne podatke
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);
                    var parititonResult = await proxy.GetCurrentTripDriver(id); // Pozivamo metodu da dobavimo trenutnu vožnju za vozača

                    if (parititonResult != null)
                    {
                        result = parititonResult; // Ako pronađemo podatke, prekidamo petlju
                        break;
                    }
                }

                // Ako smo pronašli trenutnu vožnju, vraćamo je klijentu
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
                    return BadRequest(); // Ako nema aktivne vožnje, vraćamo grešku
                }

            }
            catch (Exception ex)
            {
                throw; // U slučaju greške, izbacujemo izuzetak
            }
        }

        [Authorize(Policy = "Rider")]
        [HttpGet]
        public async Task<IActionResult> GetAllNotRatedTrips()
        {
            try
            {
                var fabricClient = new FabricClient();
                List<RoadTripModel> result = null;

                // Dobavljanje liste particija servisa
                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));

                foreach (var partition in partitionList)
                {
                    // Kreiranje ključa particije
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);

                    // Kreiranje proxy objekta za komunikaciju sa servisom
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);

                    // Pozivanje metode na servisu da bi se dobile neocenjene vožnje
                    var partitionResult = await proxy.GetAllNotRatedTrips();

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
                        rides = result,
                        message = "Successfully retrieved unrated rides"
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



        [Authorize(Policy = "Rider")]
        [HttpPut]
        public async Task<IActionResult> SubmitRating([FromBody] ReviewModel review)
        {
            try
            {
                var fabricClient = new FabricClient(); // Kreiranje FabricClient-a za komunikaciju sa Service Fabric servisima
                bool result = false; // Inicijalno podešavanje rezultata na false

                // Dobijanje liste svih particija za DrivingService
                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/DrivingService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IDriveService>(new Uri("fabric:/TaxiApp/DrivingService"), partitionKey);

                    // Pokušaj da se pošalje ocena za određenu vožnju
                    var partitionResult = await proxy.SubmitRating(review.tripId, review.rating);
                    if (partitionResult != null)
                    {
                        result = partitionResult; // Ako je rezultat uspešan, postavlja se result na true
                        break;
                    }
                }

                // Ako je ocena uspešno poslata, vraća se odgovor sa statusom 200 OK
                if (result != false)
                {
                    return Ok("Successfully submitted rating");
                }
                else
                {
                    return BadRequest("Rating is not submitted"); // Ako ocena nije uspešno poslata, vraća se BadRequest
                }
            }
            catch (Exception ex)
            {
                throw; // Bacanje izuzetka ako nešto pođe po zlu
            }
        }

    }
}
