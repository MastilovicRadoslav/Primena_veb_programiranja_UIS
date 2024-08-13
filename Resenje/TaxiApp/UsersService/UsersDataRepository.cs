using Common.Entities;
using Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;


namespace UsersService
{
    public class UsersDataRepository
    {
        private CloudStorageAccount cloudAcc;

        private CloudTableClient tableClient;
        private CloudTable _users;

        private CloudBlobClient blobClient;
        public UsersDataRepository(string tableName)
        {
            try
            {

                string dataConnectionString = Environment.GetEnvironmentVariable("DataConnectionString");
                CloudAcc = CloudStorageAccount.Parse(dataConnectionString); // create cloud client for making blob,table or queue 

                BlobClient = CloudAcc.CreateCloudBlobClient();  // blob client 

                TableClient = CloudAcc.CreateCloudTableClient(); // table client

                Users = TableClient.GetTableReference(tableName); // create if not exists Users table 
                Users.CreateIfNotExistsAsync().Wait();

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<CloudBlockBlob> GetBlockBlobReference(string containerName, string blobName)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            return blob;
        }

        public IEnumerable<UserEntity> GetAllUsers()
        {
            var q = new TableQuery<UserEntity>();
            var qRes = Users.ExecuteQuerySegmentedAsync(q, null).GetAwaiter().GetResult();
            return qRes.Results;
        }

        public async Task<bool> UpdateEntity(Guid id, bool status)
        {
            TableQuery<UserEntity> driverQuery = new TableQuery<UserEntity>()
        .Where(TableQuery.GenerateFilterConditionForGuid("Id", QueryComparisons.Equal, id));
            TableQuerySegment<UserEntity> queryResult = await Users.ExecuteQuerySegmentedAsync(driverQuery, null);

            if (queryResult.Results.Count > 0)
            {
                UserEntity user = queryResult.Results[0];
                user.IsBlocked = status;
                var operation = TableOperation.Replace(user);
                await Users.ExecuteAsync(operation);

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task UpdateUser(UserUpdateNetworkModel userOverNetwork, UserModel u)
        {

            TableQuery<UserEntity> usersQuery = new TableQuery<UserEntity>()
       .Where(TableQuery.GenerateFilterConditionForGuid("Id", QueryComparisons.Equal, userOverNetwork.Id));

            TableQuerySegment<UserEntity> queryResult = await Users.ExecuteQuerySegmentedAsync(usersQuery, null);

            if (queryResult.Results.Count > 0)
            {
                UserEntity userFromTable = queryResult.Results[0];
                userFromTable.Email = u.Email;
                userFromTable.FirstName = u.FirstName;
                userFromTable.LastName = u.LastName;
                userFromTable.Address = u.Address;
                userFromTable.Birthday = u.Birthday;
                userFromTable.Username = u.Username;
                userFromTable.Username = u.Username;
                userFromTable.ImageUrl = u.ImageUrl;
                var operation = TableOperation.Replace(userFromTable);
                await Users.ExecuteAsync(operation);
            }
        }

        public async Task UpdateDriverStatus(Guid id, string status)
        {
            TableQuery<UserEntity> usersQuery = new TableQuery<UserEntity>()
       .Where(TableQuery.GenerateFilterConditionForGuid("Id", QueryComparisons.Equal, id));
            TableQuerySegment<UserEntity> queryResult = await Users.ExecuteQuerySegmentedAsync(usersQuery, null);


            if (queryResult.Results.Count > 0)
            {
                UserEntity userFromTable = queryResult.Results[0];
                userFromTable.Status = status;
                if (status == "Prihvacen") userFromTable.IsVerified = true;
                else userFromTable.IsVerified = false;
                var operation = TableOperation.Replace(userFromTable);
                await Users.ExecuteAsync(operation);

            }
        }
        public async Task UpdateDriverRating(Guid id, int sumOfRating, int numOfRating, double averageRating)
        {
            TableQuery<UserEntity> usersQuery = new TableQuery<UserEntity>()
       .Where(TableQuery.GenerateFilterConditionForGuid("Id", QueryComparisons.Equal, id));
            TableQuerySegment<UserEntity> queryResult = await Users.ExecuteQuerySegmentedAsync(usersQuery, null);


            if (queryResult.Results.Count > 0)
            {
                UserEntity userFromTable = queryResult.Results[0];
                userFromTable.SumOfRatings = sumOfRating;
                userFromTable.NumOfRatings = numOfRating;
                userFromTable.AverageRating = averageRating;
                var operation = TableOperation.Replace(userFromTable);
                await Users.ExecuteAsync(operation);

            }

        }

        public async Task<byte[]> DownloadImage(UsersDataRepository dataRepo, UserEntity user, string nameOfContainer)
        {

            CloudBlockBlob blob = await dataRepo.GetBlockBlobReference(nameOfContainer, $"image_{user.Id}");


            await blob.FetchAttributesAsync();

            long blobLength = blob.Properties.Length;

            byte[] byteArray = new byte[blobLength];
            await blob.DownloadToByteArrayAsync(byteArray, 0);

            return byteArray;

        }

        public CloudStorageAccount CloudAcc { get => cloudAcc; set => cloudAcc = value; }
        public CloudTableClient TableClient { get => tableClient; set => tableClient = value; }
        public CloudTable Users { get => _users; set => _users = value; }
        public CloudBlobClient BlobClient { get => blobClient; set => blobClient = value; }
    }
}
