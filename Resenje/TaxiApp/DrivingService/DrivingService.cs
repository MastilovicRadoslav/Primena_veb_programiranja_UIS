using Common.Entities;
using Common.Interfaces;
using Common.Mapper;
using Common.Models;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.WindowsAzure.Storage.Table;
using System.Fabric;
namespace DrivingService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    public sealed class DrivingService : StatefulService, IDriveService
    {
        DrivingDataRepo dataRepo;
        public DrivingService(StatefulServiceContext context)
            : base(context)
        {
            dataRepo = new DrivingDataRepo("DrivingTaxiApp");
        }

        public async Task<RoadTripModel> AcceptRoadTrip(RoadTripModel trip)
        {
            var roadTrips = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips"); //isto kao i za User-s koristi IReliableDictionary koleokciju podataka za cuvanje informacija o voznjama
            try
            {
                using (var tx = StateManager.CreateTransaction()) // // Pokretanje transakcije
                {
                    if (!await CheckIfTripAlreadyExists(trip)) // Provera da li već postoji sličan zahtev za vožnju
                    {
                        var enumerable = await roadTrips.CreateEnumerableAsync(tx);// Kreiranje enumeracije za prolazak kroz sve vožnje

                        using (var enumerator = enumerable.GetAsyncEnumerator())
                        {

                            await roadTrips.AddAsync(tx, trip.TripId, trip);// Dodavanje nove vožnje u IReliableDictionary kolekciju
                            // Kreiranje entiteta za vožnju i upisivanje u tabelu pomoću Azure Table Storage operacije
                            RoadTripEntity entity = new RoadTripEntity(trip.RiderId, trip.DriverId, trip.CurrentLocation, trip.Destination, trip.Accepted, trip.Price, trip.TripId, trip.SecondsToDriverArrive);
                            TableOperation operation = TableOperation.Insert(entity); //upisi u tabelu
                            await dataRepo.Trips.ExecuteAsync(operation);
                            // Preuzimanje vrednosti za dodatu vožnju iz IReliableDictionary kolekcije
                            ConditionalValue<RoadTripModel> result = await roadTrips.TryGetValueAsync(tx, trip.TripId);
                            // Commit-ovanje transakcije nakon uspešnog dodavanja
                            await tx.CommitAsync();
                            return result.Value;

                        }

                    }
                    else return null;

                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<bool> CheckIfTripAlreadyExists(RoadTripModel trip)
        {
            // Pristupanje IReliableDictionary kolekciji koja čuva informacije o vožnjama
            var roadTrips = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips");

            try
            {
                // Pokretanje transakcije
                using (var tx = StateManager.CreateTransaction())
                {
                    // Kreiranje enumeracije za prolazak kroz sve vožnje
                    var enumerable = await roadTrips.CreateEnumerableAsync(tx);

                    using (var enumerator = enumerable.GetAsyncEnumerator())
                    {
                        // Prolazak kroz sve postojeće vožnje
                        while (await enumerator.MoveNextAsync(default(CancellationToken)))
                        {
                            // Provera da li već postoji vožnja koja nije prihvaćena, a koju je poslao isti korisnik
                            if (enumerator.Current.Value.RiderId == trip.RiderId && enumerator.Current.Value.Accepted == false)
                            {
                                // Ako takva vožnja postoji, vraća se true
                                return true;
                            }
                        }
                    }
                }

                // Ako takva vožnja ne postoji, vraća se false
                return false;
            }
            catch (Exception)
            {
                // Ako dođe do greške, izuzetak se ponovo baca
                throw;
            }
        }


        private async Task LoadRoadTrips() //pracenje stanja voznji u realnom vremenu
        {
            var roadTrip = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips"); //standardan pristup

            try
            {
                using (var transaction = StateManager.CreateTransaction()) //transkacija
                {
                    var trips = dataRepo.GetAllTrips(); //preuzimanje svih voznji iz baze podataka
                    if (trips.Count() == 0) return; //ako ima voznji
                    else
                    {
                        foreach (var trip in trips)//sve voznje preuzete iz baze podataka se dodaju u kolekciju podataka IReliableDictionary
                        {
                            await roadTrip.AddAsync(transaction, trip.TripId, RoadTripEntityMapper.MapRoadTripEntityToRoadTrip(trip)); // svakako cu se iterirati kroz svaki move next async 
                        }
                    }

                    await transaction.CommitAsync(); //komitovanje

                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
            => this.CreateServiceRemotingReplicaListeners(); //vraca listu listenera-a koji omogucavaju komunikaciju izmedju razilicitih delova Service Fabric aplikacije

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // Dobijanje ili kreiranje kolekcije 'Trips' u Service Fabric-u, koja čuva informacije o svim vožnjama
            var roadTrips = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips");

            // Učitavanje podataka o vožnjama iz spoljnog izvora (npr. Azure Table Storage) u kolekciju 'Trips'
            await LoadRoadTrips();

            // Beskonačna petlja koja se izvršava sve dok se servis ne ugasi ili ne primi signal za zaustavljanje
            while (true)
            {
                // Proverava se da li je zatraženo otkazivanje toka
                cancellationToken.ThrowIfCancellationRequested();

                // Kreiranje transakcije koja omogućava grupisanje operacija kao atomsku jedinicu
                using (var tx = this.StateManager.CreateTransaction())
                {
                    // Pravljenje enumeratora za iteraciju kroz sve elemente u kolekciji 'Trips'
                    var enumerable = await roadTrips.CreateEnumerableAsync(tx);

                    // Proverava se da li u kolekciji 'Trips' ima elemenata (vožnji)
                    if (await roadTrips.GetCountAsync(tx) > 0)
                    {
                        using (var enumerator = enumerable.GetAsyncEnumerator())
                        {
                            // Petlja koja prolazi kroz sve vožnje u kolekciji 'Trips'
                            while (await enumerator.MoveNextAsync(default(CancellationToken)))
                            {
                                // Proverava se da li vožnja nije prihvaćena ili je već završena; ako jeste, preskoči je
                                if (!enumerator.Current.Value.Accepted || enumerator.Current.Value.IsFinished)
                                {
                                    continue;
                                }
                                // Ako je vožnja prihvaćena i vozač još nije stigao, smanjuje se vreme dolaska vozača
                                else if (enumerator.Current.Value.Accepted && enumerator.Current.Value.SecondsToDriverArrive > 0)
                                {
                                    enumerator.Current.Value.SecondsToDriverArrive--;
                                }
                                // Ako je vozač stigao i vožnja traje, smanjuje se preostalo vreme vožnje
                                else if (enumerator.Current.Value.Accepted && enumerator.Current.Value.SecondsToDriverArrive == 0 && enumerator.Current.Value.SecondsToEndTrip > 0)
                                {
                                    enumerator.Current.Value.SecondsToEndTrip--;
                                }
                                // Ako je vožnja završena, označava se kao završena i vrši se ažuriranje u bazi podataka
                                else if (enumerator.Current.Value.IsFinished == false)
                                {
                                    enumerator.Current.Value.IsFinished = true;
                                    await dataRepo.FinishTrip(enumerator.Current.Value.TripId);
                                }

                                // Ažuriranje kolekcije 'Trips' sa izmenjenim podacima o vožnji
                                await roadTrips.SetAsync(tx, enumerator.Current.Key, enumerator.Current.Value);
                            }
                        }
                    }

                    // Potvrđivanje transakcije, što čini sve promene permanentnim
                    await tx.CommitAsync();
                }

                // Pauza od jedne sekunde pre nego što se petlja ponovo pokrene
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }


        /// <summary>
        /// Rider id is sent
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<RoadTripModel> GetCurrentRoadTrip(Guid id)
        {
            var roadTrip = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips");
            try
            {
                using (var tx = StateManager.CreateTransaction())
                {

                    var enumerable = await roadTrip.CreateEnumerableAsync(tx);

                    using (var enumerator = enumerable.GetAsyncEnumerator())
                    {
                        while (await enumerator.MoveNextAsync(default(CancellationToken)))
                        {
                            if ((enumerator.Current.Value.RiderId == id && enumerator.Current.Value.IsFinished == false))
                            {
                                return enumerator.Current.Value;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<List<RoadTripModel>> GetRoadTrips()
        {
            // Dobijamo pouzdanu kolekciju vožnji iz Service Fabric-a ili kreiramo novu ako ne postoji
            var roadTrip = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips");

            // Lista koja će držati vožnje koje nisu završene
            List<RoadTripModel> notCompletedTrips = new List<RoadTripModel>();

            // Dummy GUID koji predstavlja vožnje koje još uvek nemaju vozača (dakle nisu prihvaćene)
            Guid forCompare = new Guid("00000000-0000-0000-0000-000000000000");

            try
            {
                using (var tx = StateManager.CreateTransaction()) // Kreiramo transakciju
                {
                    // Pravljenje enumeratora za iteriranje kroz sve vožnje
                    var enumerable = await roadTrip.CreateEnumerableAsync(tx);

                    using (var enumerator = enumerable.GetAsyncEnumerator())
                    {
                        // Iteriramo kroz sve vožnje
                        while (await enumerator.MoveNextAsync(default(CancellationToken)))
                        {
                            // Ako vožnja još nema dodeljenog vozača (DriverId je jednak `forCompare`)
                            if (enumerator.Current.Value.DriverId == forCompare)
                            {
                                // Dodajemo vožnju u listu nekompletiranih vožnji
                                notCompletedTrips.Add(enumerator.Current.Value);
                            }
                        }
                    }
                    await tx.CommitAsync(); // Zatvaranje transakcije i commit promene
                }

                return notCompletedTrips; // Vraćamo listu vožnji koje nisu završene
            }
            catch (Exception)
            {
                throw; // U slučaju greške, izbacujemo izuzetak koji će biti uhvaćen na višem nivou
            }
        }


        public async Task<RoadTripModel> AcceptRoadTripDriver(Guid rideId, Guid driverId)
        {
            // Dobijamo ili kreiramo pouzdanu kolekciju vožnji (IReliableDictionary)
            var roadTrip = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips");

            // Dummy GUID koji predstavlja vožnje koje još uvek nemaju vozača (dakle nisu prihvaćene)
            Guid forCompare = new Guid("00000000-0000-0000-0000-000000000000");

            try
            {
                using (var tx = StateManager.CreateTransaction()) // Kreiramo transakciju
                {
                    // Pokušavamo da dobijemo vožnju sa datim RideId
                    ConditionalValue<RoadTripModel> result = await roadTrip.TryGetValueAsync(tx, rideId);

                    // Proveravamo da li vožnja postoji i da li nije već prihvaćena (DriverId == forCompare)
                    if (result.HasValue && result.Value.DriverId == forCompare)
                    {
                        // Ažuriramo polja u pouzdanoj kolekciji
                        RoadTripModel tripForAccept = result.Value;
                        tripForAccept.SecondsToEndTrip = 60; // Možda bi ovde mogao da se pozove servis za predikciju trajanja vožnje jer tamo sam vec definisao ta dva vremena al to drugo vrijeme nisam koristio
                        tripForAccept.DriverId = driverId; // Postavljamo DriverId na ID vozača koji prihvata vožnju
                        tripForAccept.Accepted = true; // Obeležavamo vožnju kao prihvaćenu sto znaci ona metoda iznad RunAsync ce sad pocet da radi

                        // Ažuriramo vožnju u pouzdanoj kolekciji
                        await roadTrip.SetAsync(tx, tripForAccept.TripId, tripForAccept);

                        // Ažuriramo vožnju u Azure Table Storage putem `UpdateEntity` metode
                        if (await dataRepo.UpdateEntity(driverId, rideId))
                        {
                            await tx.CommitAsync(); // Zatvaramo transakciju i commit promene
                            return tripForAccept; // Vraćamo ažuriranu vožnju
                        }
                        else return null; // Ako se ažuriranje u bazi nije uspelo, vraćamo null
                    }
                    else return null; // Ako vožnja nije pronađena ili je već prihvaćena, vraćamo null
                }
            }
            catch (Exception)
            {
                throw; // U slučaju greške, izbacujemo izuzetak koji će biti uhvaćen na višem nivou
            }
        }


        public async Task<List<RoadTripModel>> GetListOfCompletedRidesForDriver(Guid driverId) //Dobavlja sve voznje za odredjenog vozaca sa uslovom
        {
            var roadTrip = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips");
            List<RoadTripModel> driverTrips = new List<RoadTripModel>();
            try
            {
                using (var tx = StateManager.CreateTransaction())
                {

                    var enumerable = await roadTrip.CreateEnumerableAsync(tx);

                    using (var enumerator = enumerable.GetAsyncEnumerator())
                    {
                        while (await enumerator.MoveNextAsync(default(CancellationToken)))
                        {
                            if (enumerator.Current.Value.DriverId == driverId && enumerator.Current.Value.IsFinished)
                            {
                                driverTrips.Add(enumerator.Current.Value);
                            }
                        }
                    }
                    await tx.CommitAsync();
                }

                return driverTrips;
            }

            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<RoadTripModel>> GetListOfCompletedRidesForRider(Guid riderId)//Dobavlja sve voznje za odredjenog korisnika sa uslovom
        {
            var roadTrip = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips");
            List<RoadTripModel> riderTrips = new List<RoadTripModel>();
            try
            {
                using (var tx = StateManager.CreateTransaction())
                {

                    var enumerable = await roadTrip.CreateEnumerableAsync(tx);

                    using (var enumerator = enumerable.GetAsyncEnumerator())
                    {
                        while (await enumerator.MoveNextAsync(default(CancellationToken)))
                        {
                            if (enumerator.Current.Value.RiderId == riderId && enumerator.Current.Value.IsFinished)
                            {
                                riderTrips.Add(enumerator.Current.Value);
                            }
                        }
                    }
                    await tx.CommitAsync();
                }

                return riderTrips;
            }

            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<RoadTripModel>> GetListOfCompletedRidesAdmin() ////Dobavlja sve voznje bez uslova
        {
            var roadTrip = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips");
            List<RoadTripModel> driverTrips = new List<RoadTripModel>();
            try
            {
                using (var tx = StateManager.CreateTransaction())
                {
                    var enumerable = await roadTrip.CreateEnumerableAsync(tx);

                    using (var enumerator = enumerable.GetAsyncEnumerator())
                    {
                        while (await enumerator.MoveNextAsync(default(CancellationToken)))
                        {
                            // Dodavanje svake vožnje u listu
                            driverTrips.Add(enumerator.Current.Value);
                        }
                    }
                    await tx.CommitAsync();
                }

                return driverTrips;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<RoadTripModel> GetCurrentTrip(Guid id) // Prima `id` korisnika
        {
            var roadTrip = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips"); // Dobavlja ili kreira pouzdan rečnik za skladištenje vožnji
            try
            {
                using (var tx = StateManager.CreateTransaction()) // Kreira novu transakciju
                {
                    var enumerable = await roadTrip.CreateEnumerableAsync(tx); // Kreira enumerator za iteraciju kroz vožnje

                    using (var enumerator = enumerable.GetAsyncEnumerator()) // Asinhroni enumerator za iteraciju kroz sve vožnje
                    {
                        while (await enumerator.MoveNextAsync(default(CancellationToken))) // Iteracija kroz sve vožnje
                        {
                            if ((enumerator.Current.Value.RiderId == id && enumerator.Current.Value.IsFinished == false))
                            {
                                return enumerator.Current.Value; // Vraća aktivnu vožnju za korisnika ako je pronađena
                            }
                        }
                    }
                }
                return null; // Ako nije pronađena nijedna aktivna vožnja, vraća `null`
            }
            catch (Exception)
            {
                throw; // U slučaju greške, izbacuje izuzetak
            }
        }


        public async Task<RoadTripModel> GetCurrentTripDriver(Guid id)
        {
            var roadTrip = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips"); // Dobavlja ili kreira pouzdan rečnik za skladištenje vožnji
            try
            {
                using (var tx = StateManager.CreateTransaction()) // Kreira novu transakciju
                {
                    var enumerable = await roadTrip.CreateEnumerableAsync(tx); // Kreira enumerator za iteraciju kroz vožnje

                    using (var enumerator = enumerable.GetAsyncEnumerator()) // Asinhroni enumerator za iteraciju kroz sve vožnje
                    {
                        while (await enumerator.MoveNextAsync(default(CancellationToken))) // Iteracija kroz sve vožnje
                        {
                            if ((enumerator.Current.Value.DriverId == id && enumerator.Current.Value.IsFinished == false))
                            {
                                return enumerator.Current.Value; // Vraća aktivnu vožnju za vozača ako je pronađena
                            }
                        }
                    }
                }
                return null; // Ako nije pronađena nijedna aktivna vožnja, vraća `null`
            }
            catch (Exception)
            {
                throw; // U slučaju greške, izbacuje izuzetak
            }
        }


        public async Task<List<RoadTripModel>> GetAllNotRatedTrips() //dobavi sve neocenjenen zavrsene voznje
        {
            var roadTrip = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips");
            List<RoadTripModel> trips = new List<RoadTripModel>();
            try
            {
                using (var tx = StateManager.CreateTransaction())
                {
                    var enumerable = await roadTrip.CreateEnumerableAsync(tx);

                    using (var enumerator = enumerable.GetAsyncEnumerator())
                    {
                        while (await enumerator.MoveNextAsync(default(CancellationToken)))
                        {
                            // Provera da li je vožnja završena ali ne i ocenjena
                            if (!enumerator.Current.Value.IsRated && enumerator.Current.Value.IsFinished)
                            {
                                trips.Add(enumerator.Current.Value);
                            }
                        }
                    }
                }
                return trips;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<bool> SubmitRating(Guid tripId, int rating)
        {
            // Prvo se dobija ili kreira pouzdani rečnik (Reliable Dictionary) za vožnje
            var roadTrip = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, RoadTripModel>>("Trips");

            // Inicijalizacija rezultata na false
            bool result = false;

            // Kreiranje FabricClient-a za komunikaciju sa drugim servisima u Service Fabric klasteru
            var fabricClient = new FabricClient();

            try
            {
                // Kreiranje transakcije za pouzdano skladište
                using (var tx = StateManager.CreateTransaction())
                {
                    // Pokušaj da se pronađe vožnja sa zadatim ID-em
                    var trip = await roadTrip.TryGetValueAsync(tx, tripId);

                    // Ako vožnja nije pronađena, vraća se false
                    if (!trip.HasValue)
                    {
                        return false;
                    }

                    // Dobijanje ID-a vozača iz pronađene vožnje
                    Guid driverId = trip.Value.DriverId;

                    // Dobijanje liste particija za `UsersService`
                    var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));

                    // Iteracija kroz sve particije `UsersService` servisa
                    foreach (var partition in partitionList)
                    {
                        var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                        var proxy = ServiceProxy.Create<IRatingService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);

                        try
                        {
                            // Pozivanje metode `AddRating` za dodavanje ocene vozaču, mora u bazi podataka i u kolekciji
                            var partitionResult = await proxy.AddRating(driverId, rating);

                            if (partitionResult)
                            {
                                // Ako je ocena uspešno dodata, ažurira se rezultat i vožnja se označava kao ocenjena
                                result = true;
                                trip.Value.IsRated = true; //zbog kolekcije

                                // Ažuriranje vožnje u pouzdanom skladištu
                                await roadTrip.SetAsync(tx, trip.Value.TripId, trip.Value);

                                // Ažuriranje vožnje u bazi podataka
                                await dataRepo.RateTrip(trip.Value.TripId);
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            // U slučaju greške, izbacuje se izuzetak
                            throw;
                        }
                    }

                    // Komitovanje transakcije
                    await tx.CommitAsync();
                }

                // Vraćanje rezultata
                return result;
            }
            catch (Exception ex)
            {
                // Logovanje izuzetka i bacanje novog izuzetka sa dodatnim informacijama
                throw new ApplicationException($"Failed to submit rating for TripId: {tripId}", ex);
            }
        }
    }
}
