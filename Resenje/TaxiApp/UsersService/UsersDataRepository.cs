using Common.Entities;
using Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;


namespace UsersService
{
    public class UsersDataRepository
    {

        private CloudStorageAccount cloudAcc;//referenca na bazu podataka, sve operacije nad njom

        private CloudTableClient tableClient;  //referenca na tabele, sve operacije nad tabelama
        private CloudTable _users;//referenca na konkretnu tabelu gde se cuvaju podaci o korisnicima sistem, omogucava izvrsavanje osnovnih operacija nad tom tabelom
        private CloudBlobClient blobClient; //referenca na blob, sve operacije nad blobom
        //Propertiji
        public CloudStorageAccount CloudAcc { get => cloudAcc; set => cloudAcc = value; }
        public CloudTableClient TableClient { get => tableClient; set => tableClient = value; }
        public CloudTable Users { get => _users; set => _users = value; }
        public CloudBlobClient BlobClient { get => blobClient; set => blobClient = value; }

        public UsersDataRepository(string tableName)
        {
            try
            {

                string dataConnectionString = Environment.GetEnvironmentVariable("DataConnectionString");
                CloudAcc = CloudStorageAccount.Parse(dataConnectionString); // // Kreira cloud nalog koristeći connection string, za prvaljenje blob,table i queue 

                BlobClient = CloudAcc.CreateCloudBlobClient();  //  Kreira klijent za blob

                TableClient = CloudAcc.CreateCloudTableClient(); // Kreira klijent za tabele

                Users = TableClient.GetTableReference(tableName);// Kreira referencu na tabelu sa zadatim imenom
                Users.CreateIfNotExistsAsync().Wait(); // Kreira tabelu ako već ne postoji

            }
            catch (Exception ex)
            {
                throw;
            }


        }

        public async Task<CloudBlockBlob> GetBlockBlobReference(string containerName, string blobName) //Register
        {
            CloudBlobContainer container = blobClient.GetContainerReference(containerName); //referenca na kontejner blob-ova
            await container.CreateIfNotExistsAsync(); //ako ne posotji kreira novi
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName); //referenca na specifican blob unutar kontejnera

            return blob;
        }

        public IEnumerable<UserEntity> GetAllUsers() //LoadUsers prezuiamms ve 
        {
            var q = new TableQuery<UserEntity>(); //upit
            var qRes = Users.ExecuteQuerySegmentedAsync(q, null).GetAwaiter().GetResult(); //izvrsava upit i vraca rezz segm. zbog velikih kolicina pdoataka
            return qRes.Results; //vracam rez
        }

        public async Task<bool> UpdateEntity(Guid id, bool status) //azuiranje statusa vozaca u bazi podataka
        {
            TableQuery<UserEntity> driverQuery = new TableQuery<UserEntity>()
        .Where(TableQuery.GenerateFilterConditionForGuid("Id", QueryComparisons.Equal, id)); //kreiramo upit i filtriramo korisnike na osnovu id
            TableQuerySegment<UserEntity> queryResult = await Users.ExecuteQuerySegmentedAsync(driverQuery, null); //izvrsavanje zadanog upita

            if (queryResult.Results.Count > 0) //ako su nadjeni korisnici, tj veci broj od 0
            {
                UserEntity user = queryResult.Results[0]; //preuzimamo ga
                user.IsBlocked = status; //azuriramo mu statis
                var operation = TableOperation.Replace(user); //zamenjuje postojeci entitet novim entitetom sa promijenjenim statusom
                await Users.ExecuteAsync(operation); //zapisivanje azuriranog entiteta

                return true; //sve ok 
            }
            else
            {
                return false;
            }
        }

        public async Task UpdateDriverStatus(Guid id, string status)
        {
            TableQuery<UserEntity> usersQuery = new TableQuery<UserEntity>()
       .Where(TableQuery.GenerateFilterConditionForGuid("Id", QueryComparisons.Equal, id)); //upit za pretrazivanje po Id
            TableQuerySegment<UserEntity> queryResult = await Users.ExecuteQuerySegmentedAsync(usersQuery, null); //izvrsi pretragu


            if (queryResult.Results.Count > 0) //ako postoji
            {
                UserEntity userFromTable = queryResult.Results[0]; //preuzmi ga
                userFromTable.Status = status; //azurieaj status
                if (status == "Prihvacen") userFromTable.IsVerified = true; //verifikovan
                else userFromTable.IsVerified = false;
                var operation = TableOperation.Replace(userFromTable); //zameni ih
                await Users.ExecuteAsync(operation); //izvrsi opraciju zamenjivanja

            }

        }


        public async Task UpdateDriverRating(Guid id, int sumOfRating, int numOfRating, double averageRating)
        {
            // Kreiranje upita za tabelu Azure Table Storage da pronađe red sa određenim ID-om korisnika (vozača)
            TableQuery<UserEntity> usersQuery = new TableQuery<UserEntity>()
               .Where(TableQuery.GenerateFilterConditionForGuid("Id", QueryComparisons.Equal, id));

            // Izvršavanje upita i dobijanje rezultata kao segmenta iz tabele
            TableQuerySegment<UserEntity> queryResult = await Users.ExecuteQuerySegmentedAsync(usersQuery, null);

            // Provera da li je pronađen bar jedan rezultat
            if (queryResult.Results.Count > 0)
            {
                // Ako je pronađen rezultat, dobijamo prvog korisnika iz rezultata
                UserEntity userFromTable = queryResult.Results[0];

                // Ažuriranje polja korisnika sa novim vrednostima za zbir ocena, broj ocena i prosečnu ocenu
                userFromTable.SumOfRatings = sumOfRating;
                userFromTable.NumOfRatings = numOfRating;
                userFromTable.AverageRating = averageRating;

                // Kreiranje operacije zamene (replace) kako bi se ažurirani entitet sačuvao u tabeli
                var operation = TableOperation.Replace(userFromTable);

                // Izvršavanje operacije na Azure Table Storage
                await Users.ExecuteAsync(operation);
            }
        }


        public async Task UpdateUser(UserUpdateNetworkModel userOverNetwork, UserModel u) //azuriranje korisnika u bazi podataka
        {

            TableQuery<UserEntity> usersQuery = new TableQuery<UserEntity>()
       .Where(TableQuery.GenerateFilterConditionForGuid("Id", QueryComparisons.Equal, userOverNetwork.Id)); //kreiramo upit za pretragu po Id

            TableQuerySegment<UserEntity> queryResult = await Users.ExecuteQuerySegmentedAsync(usersQuery, null); //izvrsavanje upita i nalazenje korisnika

            if (queryResult.Results.Count > 0) //ako je nadjen
            {   //Azuriranje
                UserEntity userFromTable = queryResult.Results[0]; //pronadjeni korisnik
                userFromTable.Email = u.Email;
                userFromTable.FirstName = u.FirstName;
                userFromTable.LastName = u.LastName;
                userFromTable.Address = u.Address;
                userFromTable.Birthday = u.Birthday;
                userFromTable.Username = u.Username;
                userFromTable.Username = u.Username;
                userFromTable.ImageUrl = u.ImageUrl;
                var operation = TableOperation.Replace(userFromTable); //zamena postojeceg entiteta sa novim entitetom - operacija
                await Users.ExecuteAsync(operation); //izvrsi operaciju
            }
        }


        public async Task<byte[]> DownloadImage(UsersDataRepository dataRepo, UserEntity user, string nameOfContainer) //LoadUsers, preuzimanje slike iz bloba
        {

            CloudBlockBlob blob = await dataRepo.GetBlockBlobReference(nameOfContainer, $"image_{user.Id}"); //referenca na blom kao i kod upisa i azuriranja korisnika sa slikomm


            await blob.FetchAttributesAsync(); //preuzima inf o odredjenom blobu

            long blobLength = blob.Properties.Length; //duzina bloba

            byte[] byteArray = new byte[blobLength]; //definisanje bajta sa odredjenom duzinom
            await blob.DownloadToByteArrayAsync(byteArray, 0); //preuzimam sadrzaj slike iz bloba kao niz bajtova

            return byteArray; //rezz

        }


    }
}
