using Common.DTOs;
using Common.Entities;
using Common.Enums;
using Common.Interfaces;
using Common.Mapper;
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
    public sealed class UsersService : StatefulService, IUserService, IRatingService
    {
        public UsersDataRepository dataRepo;

        public UsersService(StatefulServiceContext context)
            : base(context)
        {

            dataRepo = new UsersDataRepository("UsersTaxiApp");
        }



        public async Task<bool> addNewUser(UserModel user)
        {
            var userDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities"); //koristim IReliableDictionary kolekciju podataka Azure Service Fabric, lakse je tu provjeravat nego u bazi

            try
            {
                using (var transaction = StateManager.CreateTransaction()) //Transkacija za grupisanje vise operacija u jednu atomsku celinu
                {
                    if (!await CheckIfUserAlreadyExists(user)) //ako user vec postoji u IReliableDictionary
                    {

                        await userDictionary.AddAsync(transaction, user.Id, user); // dodaj ga prvo u reliable, zajedno sa tim i slika korisnika sto se trazilo u impl

                        //cuvanje slike u blobu
                        CloudBlockBlob blob = await dataRepo.GetBlockBlobReference("users", $"image_{user.Id}"); //kreiranje reference za blob
                        blob.Properties.ContentType = user.ImageFile.ContentType; //jpeg, img...
                        await blob.UploadFromByteArrayAsync(user.ImageFile.FileContent, 0, user.ImageFile.FileContent.Length); //upload slike u blob, //zbog ovoga smo u bajtove prebacivali i pravili onu funkciju
                        string imageUrl = blob.Uri.AbsoluteUri; //dobijanje URL

                        //cuvanje korisnika u tabeli
                        UserEntity newUser = new UserEntity(user, imageUrl); //kreiramo UserEntity model za upis korisnika u bazu pdoataka sa potrebnim podacima, uz referencu slike u blobu
                        TableOperation operation = TableOperation.Insert(newUser); //dodavanje korisnika u Azure Table Storage
                        await dataRepo.Users.ExecuteAsync(operation);




                        await transaction.CommitAsync(); //ako sve prodje uspesno transakcija se potvrdjuje
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
            var users = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities"); //isto
            try
            {
                using (var tx = StateManager.CreateTransaction()) //konzistentnost podataka
                {
                    var enumerable = await users.CreateEnumerableAsync(tx); //za iteriranje korz sve parove kljuc-vrednost

                    using (var enumerator = enumerable.GetAsyncEnumerator()) //pristup svkaom elementu u kolekciji jedan po jedan
                    {
                        while (await enumerator.MoveNextAsync(default(CancellationToken))) //sledeci
                        {
                            if (enumerator.Current.Value.Email == user.Email || enumerator.Current.Value.Id == user.Id || enumerator.Current.Value.Username == user.Username) //uslov da postoji isti
                            {
                                return true; //vec postoji
                            }
                        }
                    }
                }
                return false; //ne postoji
            }
            catch (Exception)
            {
                throw;
            }
        }



        private async Task LoadUsers() //ucitavanje korisnika iz baze podataka i smestanje u koleokciju IReliable
        {
            var userDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities"); //kao i do sad distirbuirana kolekcija koju koristim za cuvanje korisnika, upisivao sam tu korisnike kad su se registrovali
            try
            {
                using (var transaction = StateManager.CreateTransaction()) //isto sve omogucava mi operacije nad distriburianom kolekcijom
                {
                    var users = dataRepo.GetAllUsers(); //preuzimam sve korisnike iz baze
                    if (users.Count() == 0) return;
                    else
                    {
                        foreach (var user in users) //ako postoje korisnici prolazim kroz njih
                        {
                            byte[] image = await dataRepo.DownloadImage(dataRepo, user, "users"); //preuzimam za svakog korisnika sliku iz bloba gde sam je i upisao kao niz bajtova i sve one informacije
                            await userDictionary.AddAsync(transaction, user.Id, UserEntityMapper.MapUserEntityToUser(user, image)); //mapiram UserEntity model sa kojim sam upisao korisnika u bazu na UserModel sa kojim sam upisivao u kolekciju podataka IREliableDictionary
                        }
                    }

                    await transaction.CommitAsync(); //komit transkacije

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
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() //kreiram listeners za replikaciju servisa
            => this.CreateServiceRemotingReplicaListeners(); //zbog komunikacije izmedju razlicitih servisa

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken) //GLAVNA METODA koja se izvrsava u servisu
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary"); //brojac
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities");
            // ucitavanm korisnike u distirburianu kolekciju pre koristenja servisa 
            await LoadUsers();
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();//za kraj

                using (var tx = this.StateManager.CreateTransaction()) //transacija
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter"); //dobijanje vrednosti za kljuc

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}", //loguje trenutnu vrednost brojaca
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value); //dodaje/azurira vr.

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync(); //proemen trajno zapisane
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken); //pauza
            }
        }
        public async Task<LogedUserDTOs> loginUser(LoginUserDTOs loginUserDTO)
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities"); //Reliable kolekcija

            using (var tx = this.StateManager.CreateTransaction()) //transakcija
            {
                var enumerable = await users.CreateEnumerableAsync(tx);

                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken)))
                    {
                        if (enumerator.Current.Value.Email == loginUserDTO.Email && enumerator.Current.Value.Password == loginUserDTO.Password)
                        {
                            return new LogedUserDTOs(enumerator.Current.Value.Id, enumerator.Current.Value.TypeOfUser); //ako se pronadje korisnik sa odgovarajucim emailom i sifrom
                        }
                    }
                }
            }
            return null;
        }


        public async Task<List<FullUserDTOs>> listUsers() //dobavlajnje svih korisnika iz liste
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities");  //kolekcija pristup

            List<FullUserDTOs> userList = new List<FullUserDTOs>(); //lista

            using (var tx = this.StateManager.CreateTransaction()) // sve operacije nad kolekcijom se izvrsvaaju kao jedinica rada
            {
                var enumerable = await users.CreateEnumerableAsync(tx); //dobavljanje svih

                using (var enumerator = enumerable.GetAsyncEnumerator()) //uzimanje jednog
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken))) //prelazak na sledeci
                    {
                        userList.Add(UserEntityMapper.MapUserToFullUserDto(enumerator.Current.Value)); //mapitanje podataka iz UserModel u FullUserDTOs jer je kolekcija IReliable UserModel
                    }
                }
            }

            return userList;

        }

        public async Task<List<DriverDetailsDTOs>> listDrivers() //funkcija za preuzimanje svih vozaca
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities"); //standardno preuzimanje iz kolekcije, cuva korisnicke podatke, to je skladiste unutar servisa
            List<DriverDetailsDTOs> drivers = new List<DriverDetailsDTOs>(); //ovo vracamo
            using (var tx = this.StateManager.CreateTransaction()) //transkacija
            {
                var enumerable = await users.CreateEnumerableAsync(tx);  //pristup svima
                using (var enumerator = enumerable.GetAsyncEnumerator()) //po jedan pristupanje
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken))) //sledeci
                    {
                        if (enumerator.Current.Value.TypeOfUser == UserRoleType.Roles.Driver) //ako je vozac 
                        {
                            drivers.Add(new DriverDetailsDTOs(enumerator.Current.Value.Email, enumerator.Current.Value.FirstName, enumerator.Current.Value.LastName, enumerator.Current.Value.Username, enumerator.Current.Value.IsBlocked, enumerator.Current.Value.AverageRating, enumerator.Current.Value.Id, enumerator.Current.Value.Status)); //kreiramo vozaca i ubacujemo u listu
                        }
                    }
                }
            }

            return drivers; //lista koja sadrzi sve vozace prilagodjene za vracanje

        }

        public async Task<bool> changeDriverStatus(Guid id, bool status) //metoda koja mijenja status i u kolekciji i u bazi podataka
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities"); //kao i do sad pristupanje kolekciji podatkaa
            using (var tx = this.StateManager.CreateTransaction()) //transkacija
            {
                ConditionalValue<UserModel> result = await users.TryGetValueAsync(tx, id);  //pronalazi vozaca u kolekciji sa zadanim id
                if (result.HasValue)
                {
                    UserModel user = result.Value; //pronadjeni vozac
                    user.IsBlocked = status; //azurira se njegov status
                    await users.SetAsync(tx, id, user); //azurirani korisnik se ponovo smesta u distriburanu kolekciju

                    await dataRepo.UpdateEntity(id, status); //metoda koja komunicira sa bazom podataka i azurira status vozaca u bazi podataka

                    await tx.CommitAsync(); //commit da su promene trajne

                    return true; //sve ok
                }
                else return false;
            }
        }

        public async Task<FullUserDTOs> changeUserFields(UserUpdateNetworkModel user)
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities"); //kolekcija podataka u kojem se nalaze svi user-i
            using (var tx = this.StateManager.CreateTransaction())//kreiranje transkacije
            {
                ConditionalValue<UserModel> result = await users.TryGetValueAsync(tx, user.Id); //dobijanje korisnika iz kolekcije na osnovu Id
                if (result.HasValue) //ako je korisnik pronadjen
                {
                    UserModel userFromReliable = result.Value; //nadjeni korisnik, smestam ga ovdes
                    //svako polje korisnika se azurira ako je novo i nije prazno
                    if (!string.IsNullOrEmpty(user.Email)) userFromReliable.Email = user.Email;

                    if (!string.IsNullOrEmpty(user.FirstName)) userFromReliable.FirstName = user.FirstName;

                    if (!string.IsNullOrEmpty(user.LastName)) userFromReliable.LastName = user.LastName;

                    if (!string.IsNullOrEmpty(user.Address)) userFromReliable.Address = user.Address;

                    if (user.Birthday != DateTime.MinValue) userFromReliable.Birthday = user.Birthday;

                    if (!string.IsNullOrEmpty(user.Password)) userFromReliable.Password = user.Password;

                    if (!string.IsNullOrEmpty(user.Username)) userFromReliable.Username = user.Username;

                    if (user.ImageFile != null && user.ImageFile.FileContent != null && user.ImageFile.FileContent.Length > 0) userFromReliable.ImageFile = user.ImageFile; //ovo moze za kolekciju vako

                    await users.TryRemoveAsync(tx, user.Id); // uklanjam proslog korisnika iz kolekcije korisnika

                    await users.AddAsync(tx, userFromReliable.Id, userFromReliable); // novog korisnika sa istim Id dodajem kao i kod Registracije u kolekciju

                    if (user.ImageFile != null && user.ImageFile.FileContent != null && user.ImageFile.FileContent.Length > 0) // ako je slika promenjena
                    {
                        CloudBlockBlob blob = await dataRepo.GetBlockBlobReference("users", $"image_{userFromReliable.Id}"); // nadji prethodni blok u blobu kroz referenciranje
                        await blob.DeleteIfExistsAsync(); // obrisi taj blok iz bloba to jeste prehodnu sliku

                        CloudBlockBlob newblob = await dataRepo.GetBlockBlobReference("users", $"image_{userFromReliable.Id}"); // kreiraj blok blob za novi username
                        newblob.Properties.ContentType = userFromReliable.ImageFile.ContentType; //format slike
                        await newblob.UploadFromByteArrayAsync(userFromReliable.ImageFile.FileContent, 0, userFromReliable.ImageFile.FileContent.Length); // upload novu sliku 
                    }

                    await dataRepo.UpdateUser(user, userFromReliable); // azuriraj korisnika i u bazi pdoataka, PODACI MORAJU BITI KONZISTENTI U KOLEKCIJI I BAZI PODATAKA
                    await tx.CommitAsync(); //trajne promene
                    return UserEntityMapper.MapUserToFullUserDto(userFromReliable); //vracam u drugom formatu modela za sta mi treba maper

                }
                else return null;
            }

        }

        public async Task<FullUserDTOs> GetUserInfo(Guid id) //dobavi iz kolekcije User-a, nema potrebe iz baze jer su baza i kolekcija sinhronizovane
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities");//kolekcija
            using (var tx = this.StateManager.CreateTransaction()) //transakcija
            {
                ConditionalValue<UserModel> result = await users.TryGetValueAsync(tx, id); //nadji u kolekciji sa tim id
                if (result.HasValue) //ako je nadjen
                {
                    FullUserDTOs user = UserEntityMapper.MapUserToFullUserDto(result.Value); //sa Mapper-om ga iz UserModel pretvori u FullUserDTOs
                    return user; //vrati
                }
                else return new FullUserDTOs();


            }
        }

        public async Task<bool> VerifyDriver(Guid id, string email, string action) //mora se verifikovati i u Reliable i u Bazi da bi ostalo konzistentno
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities");
            using (var tx = this.StateManager.CreateTransaction())
            {
                ConditionalValue<UserModel> result = await users.TryGetValueAsync(tx, id); //nadji sa tim Id
                if (result.HasValue) //nadjen
                {
                    UserModel userForChange = result.Value; //preuzmi ga
                    if (action == "Prihvacen") //ako je kacija prihvacen
                    {
                        userForChange.IsVerified = true; //da
                        userForChange.Status = Status.Prihvacen; //promeni
                    }
                    else userForChange.Status = Status.Odbijen; //inace

                    await users.SetAsync(tx, id, userForChange); //asihrono azuriraj

                    await dataRepo.UpdateDriverStatus(id, action); //mora isto i u bazi

                    await tx.CommitAsync();
                    return true;

                }
                else return false;
            }
        }

        public async Task<List<DriverDetailsDTOs>> GetNotVerifiedDrivers() //ona dobavlja vozace koji nisu verifikovani
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities"); //dobijanje Reliable kolekcije
            List<DriverDetailsDTOs> drivers = new List<DriverDetailsDTOs>(); //lista za vracanje
            using (var tx = this.StateManager.CreateTransaction()) //kreiranje transackije 
            {
                var enumerable = await users.CreateEnumerableAsync(tx); //sve korisnike
                using (var enumerator = enumerable.GetAsyncEnumerator()) //prolazim kroz sve korisnike u Reliable
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken)))
                    {
                        if (enumerator.Current.Value.TypeOfUser == UserRoleType.Roles.Driver && enumerator.Current.Value.Status != Status.Odbijen) //ukoliko je korisnik vozac i nije odbijen
                        {
                            drivers.Add(new DriverDetailsDTOs(enumerator.Current.Value.Email, enumerator.Current.Value.FirstName, enumerator.Current.Value.LastName, enumerator.Current.Value.Username, enumerator.Current.Value.IsBlocked, enumerator.Current.Value.AverageRating, enumerator.Current.Value.Id, enumerator.Current.Value.Status)); //dodajem ga
                        }
                    }
                }
            }

            return drivers; //vrati
        }

        public async Task<bool> AddRating(Guid driverId, int rating)
        {
            var users = await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, UserModel>>("UserEntities");
            bool operation = false;

            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await users.CreateEnumerableAsync(tx);
                using (var enumerator = enumerable.GetAsyncEnumerator())
                {
                    while (await enumerator.MoveNextAsync(default(CancellationToken)))
                    {
                        if (enumerator.Current.Value.Id == driverId) //nadjen vozac sa tim Id
                        {
                            var user = enumerator.Current.Value; //to je taj vozac
                            user.NumOfRatings++; //azurira se broj ocena
                            user.SumOfRatings += rating; //zbir ocena
                            user.AverageRating = (double)user.SumOfRatings / (double)user.NumOfRatings; // i prosecna ocena koja ce se adminu prikazati u listi za blokiranje vozaca ako je prosecna ocena slaba

                            // Ažuriranje korisnika u pouzdanom skladištu
                            await users.SetAsync(tx, driverId, user);

                            // Ažuriranje korisnika u bazi podataka
                            await dataRepo.UpdateDriverRating(user.Id, user.SumOfRatings, user.NumOfRatings, user.AverageRating);

                            // Komitovanje transakcije
                            await tx.CommitAsync();

                            operation = true;
                            break;
                        }
                    }
                }
            }

            return operation;
        }

    }
}
