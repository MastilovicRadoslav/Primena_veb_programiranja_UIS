using Common.Entities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace DrivingService
{
    public class DrivingDataRepo
    {
        private CloudStorageAccount cloudAcc; //referenca na bazu podataka, sve operacije nad njom

        private CloudTableClient tableClient; //referenca na tabele, sve operacije nad tabelama
        private CloudTable _trips; //referenca na konkretnu tabelu gde se cuvaju podaci o voznjama, omogucava izvrsavanje osnovnih operacija nad tom tabelom

        public DrivingDataRepo(string tableName)
        {
            try
            {

                string dataConnectionString = Environment.GetEnvironmentVariable("DataConnectionString"); //definisao sam u konfiguraciji za konekciju
                CloudAcc = CloudStorageAccount.Parse(dataConnectionString); // Kreira cloud nalog koristeći connection string


                TableClient = CloudAcc.CreateCloudTableClient();// Kreira klijent za tabele

                Trips = TableClient.GetTableReference(tableName); // Kreira referencu na tabelu sa zadatim imenom
                Trips.CreateIfNotExistsAsync().Wait();// Kreira tabelu ako već ne postoji

            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //Propertiji
        public CloudStorageAccount CloudAcc { get => cloudAcc; set => cloudAcc = value; }
        public CloudTableClient TableClient { get => tableClient; set => tableClient = value; }
        public CloudTable Trips { get => _trips; set => _trips = value; }

        public IEnumerable<RoadTripEntity> GetAllTrips() //dobavlajnej svih voznji iz baze podataka
        {
            var q = new TableQuery<RoadTripEntity>(); //kreiranje upita
            var qRes = Trips.ExecuteQuerySegmentedAsync(q, null).GetAwaiter().GetResult();//izvrsava upit to jest vraca sve voznje
            return qRes.Results; //asihroni metod da se izvrsi sihrono
        }

        public async Task<bool> UpdateEntity(Guid driverId, Guid rideId)
        {
            // Kreiramo upit za dobijanje vožnje sa datim RideId
            TableQuery<RoadTripEntity> rideQuery = new TableQuery<RoadTripEntity>()
                .Where(TableQuery.GenerateFilterConditionForGuid("TripId", QueryComparisons.Equal, rideId));

            // Izvršavamo upit i dobijamo rezultat
            TableQuerySegment<RoadTripEntity> queryResult = await Trips.ExecuteQuerySegmentedAsync(rideQuery, null);

            if (queryResult.Results.Count > 0)
            {
                RoadTripEntity trip = queryResult.Results[0];
                trip.Accepted = true; // Obeležavamo vožnju kao prihvaćenu
                trip.SecondsToEndTrip = 60; // Postavljamo vreme do kraja vožnje na 60 sekundi (ili drugo vreme prema proceni)
                trip.DriverId = driverId; // Postavljamo DriverId na ID vozača koji je prihvatio vožnju

                // Kreiramo operaciju zamene i izvršavamo je
                var operation = TableOperation.Replace(trip);
                await Trips.ExecuteAsync(operation);
                return true; // Vraćamo true ako je operacija uspela
            }
            else
            {
                return false; // Vraćamo false ako vožnja nije pronađena
            }
        }


        public async Task RateTrip(Guid tripId)
        {
            // Kreiranje upita za tabelu Azure Table Storage da pronađe red sa određenim TripId-om
            TableQuery<RoadTripEntity> rideQuery = new TableQuery<RoadTripEntity>()
                .Where(TableQuery.GenerateFilterConditionForGuid("TripId", QueryComparisons.Equal, tripId));

            // Izvršavanje upita i dobijanje rezultata kao segmenta iz tabele
            TableQuerySegment<RoadTripEntity> queryResult = await Trips.ExecuteQuerySegmentedAsync(rideQuery, null);

            // Provera da li je pronađen bar jedan rezultat
            if (queryResult.Results.Count > 0)
            {
                // Ako je pronađen rezultat, dobijamo prvi trip (vožnju) iz rezultata
                RoadTripEntity trip = queryResult.Results[0];

                // Ažuriranje polja 'IsRated' da označi da je vožnja ocenjena
                trip.IsRated = true;

                // Kreiranje operacije zamene (replace) kako bi se ažurirani entitet sačuvao u tabeli
                var operation = TableOperation.Replace(trip);

                // Izvršavanje operacije na Azure Table Storage
                await Trips.ExecuteAsync(operation);
            }
        }



        public async Task<bool> FinishTrip(Guid tripId)
        {
            // Kreiranje TableQuery objekta za upit koji će pronaći vožnju na osnovu njenog jedinstvenog identifikatora (TripId).
            TableQuery<RoadTripEntity> rideQuery = new TableQuery<RoadTripEntity>()
                .Where(TableQuery.GenerateFilterConditionForGuid("TripId", QueryComparisons.Equal, tripId));

            // Izvršavanje upita na tabeli 'Trips' da bi se dobio segment podataka koji odgovara upitu.
            TableQuerySegment<RoadTripEntity> queryResult = await Trips.ExecuteQuerySegmentedAsync(rideQuery, null);

            // Provera da li je upit vratio neki rezultat, tj. da li postoji vožnja sa datim TripId.
            if (queryResult.Results.Count > 0)
            {
                // Ako je vožnja pronađena, uzimamo prvi (i jedini) rezultat iz upita.
                RoadTripEntity trip = queryResult.Results[0];

                // Postavljamo svojstvo 'IsFinished' na true, čime označavamo da je vožnja završena.
                trip.IsFinished = true;

                // Kreiranje operacije zamene (Replace) koja će zameniti postojeći entitet (vožnju) sa ažuriranim.
                var operation = TableOperation.Replace(trip);

                // Izvršavanje operacije na tabeli 'Trips' koja će ažurirati entitet u bazi.
                await Trips.ExecuteAsync(operation);

                // Vraćamo true kao indikator uspešnog završetka operacije.
                return true;
            }
            else
            {
                // Ako vožnja nije pronađena, vraćamo false kao indikator neuspešnog završetka operacije.
                return false;
            }
        }

    }
}
