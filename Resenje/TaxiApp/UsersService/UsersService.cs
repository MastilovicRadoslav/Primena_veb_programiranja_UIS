using Common.DTOs;
using Common.Entities;
using Common.Enums;
using Common.Interfaces;
using Common.Mappers;
using Common.Models;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System.Fabric;
using static Common.Enums.UserVerificationStatus;

namespace UsersService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class UsersService : StatefulService, IUserService
    {
        public UsersDataRepository dataRepo;

        public UsersService(StatefulServiceContext context)
            : base(context)
        {
            dataRepo = new UsersDataRepository("UsersTable");
        }

        public async Task<bool> addNewUser(UserModel user)
        {
            var userDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities");

            try
            {
                using (var transaction = StateManager.CreateTransaction())
                {
                    if (!await CheckIfUserAlreadyExists(user))
                    {

                        await userDictionary.AddAsync(transaction, user.Id, user); // dodaj ga prvo u reliable 

                        //insert image of user in blob
                        CloudBlockBlob blob = await dataRepo.GetBlockBlobReference("users", $"image_{user.Id}");
                        blob.Properties.ContentType = user.ImageFile.ContentType;
                        await blob.UploadFromByteArrayAsync(user.ImageFile.FileContent, 0, user.ImageFile.FileContent.Length);
                        string imageUrl = blob.Uri.AbsoluteUri;

                        //insert user in database
                        UserEntity newUser = new UserEntity(user, imageUrl);
                        TableOperation operation = TableOperation.Insert(newUser);
                        await dataRepo.Users.ExecuteAsync(operation);




                        await transaction.CommitAsync();
                        return true;
                    }
                    return false;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task<bool> CheckIfUserAlreadyExists(UserModel user)
        {
            var users = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities");
            try
            {
                using (var tx = StateManager.CreateTransaction())
                {
                    var enumerable = await users.CreateEnumerableAsync(tx);

                    using (var enumerator = enumerable.GetAsyncEnumerator())
                    {
                        while (await enumerator.MoveNextAsync(default(CancellationToken)))
                        {
                            if (enumerator.Current.Value.Email == user.Email || enumerator.Current.Value.Id == user.Id || enumerator.Current.Value.Username == user.Username)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task LoadUsers()
        {
            var userDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities");
            try
            {
                using (var transaction = StateManager.CreateTransaction())
                {
                    var users = dataRepo.GetAllUsers();
                    if (users.Count() == 0) return;
                    else
                    {
                        foreach (var user in users)
                        {
                            byte[] image = await dataRepo.DownloadImage(dataRepo, user, "users");
                            await userDictionary.AddAsync(transaction, user.Id, UserEntityMapper.MapUserEntityToUser(user, image));
                        }
                    }

                    await transaction.CommitAsync();

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
            => this.CreateServiceRemotingReplicaListeners();

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities");
            // ovde ce biti load users 
            await LoadUsers();
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }


        public async Task<List<UserDetailsDTO>> listUsers()
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities");

            List<UserDetailsDTO> userList = new List<UserDetailsDTO>();

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await users.CreateEnumerableAsync(tx);

                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken)))
                    {
                        userList.Add(UserEntityMapper.MapUserToFullUserDto(enumerator.Current.Value));
                    }
                }
            }

            return userList;

        }

        public async Task<LogedUserDTO> loginUser(LoginUserDTO loginUserDTO)
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await users.CreateEnumerableAsync(tx);

                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken)))
                    {
                        if (enumerator.Current.Value.Email == loginUserDTO.Email && enumerator.Current.Value.Password == loginUserDTO.Password)
                        {
                            return new LogedUserDTO(enumerator.Current.Value.Id, enumerator.Current.Value.TypeOfUser);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<List<DriverDetailsDTO>> listDrivers()
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities");
            List<DriverDetailsDTO> drivers = new List<DriverDetailsDTO>();
            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await users.CreateEnumerableAsync(tx);
                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken)))
                    {
                        if (enumerator.Current.Value.TypeOfUser == UserRoleType.Roles.Driver)
                        {
                            drivers.Add(new DriverDetailsDTO(enumerator.Current.Value.Email, enumerator.Current.Value.FirstName, enumerator.Current.Value.LastName, enumerator.Current.Value.Username, enumerator.Current.Value.IsBlocked, enumerator.Current.Value.AverageRating, enumerator.Current.Value.Id, enumerator.Current.Value.Status));
                        }
                    }
                }
            }

            return drivers;
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
