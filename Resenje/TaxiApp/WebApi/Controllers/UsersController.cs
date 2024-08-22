using Common.DTOs;
using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UsersController : ControllerBase
    {

        private IConfiguration _config;
        private readonly Common.Interfaces.IEmailService emailSender;
        public UsersController(IConfiguration config, Common.Interfaces.IEmailService emailSender)
        {
            _config = config;
            this.emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] UserRegistrationModel userData) //FromForm
        {
            if (string.IsNullOrEmpty(userData.Email) || !CheckEmail(userData.Email)) return BadRequest("Invalid email format");
            if (string.IsNullOrEmpty(userData.Password)) return BadRequest("Password cannot be null or empty");
            if (string.IsNullOrEmpty(userData.Username)) return BadRequest("Username cannot be null or empty");
            if (string.IsNullOrEmpty(userData.FirstName)) return BadRequest("First name cannot be null or empty");
            if (string.IsNullOrEmpty(userData.LastName)) return BadRequest("Last name cannot be null or empty");
            if (string.IsNullOrEmpty(userData.Address)) return BadRequest("Address cannot be null or empty");
            if (string.IsNullOrEmpty(userData.TypeOfUser)) return BadRequest("Type of user must be selected!");
            if (string.IsNullOrEmpty(userData.Birthday)) return BadRequest("Birthday need to be selected!");
            if (userData.ImageUrl.Length == 0) return BadRequest("You need to send image while doing registration!");
            try
            {

                UserModel userForRegister = new UserModel(userData);

                var fabricClient = new FabricClient(); //kreiranje FabricClient objekta zbog komunikacije sa ServiceFabric klasterom
                bool result = false;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService")); //vraca listu particija za odredjeni servis
                foreach (var partition in partitionList) //iteriranje kroz particije
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey); //kreiranje kljuca koji se koristi za pristup specificnoj particiji unutar servisa
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey); //komuniakcija sa servisom i u servisu sa odredjenom particijomn na osnovu parittionKey
                    result = await proxy.addNewUser(userForRegister);
                }

                if (result) return Ok($"Successfully registered new User: {userData.Username}");
                else return StatusCode(409, "User already exists in database!");


            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while registering new User");
            }
        }

        [HttpGet]
        public async Task<List<FullUserDTOs>> GetUsers() //dobavljanje svih korisnika
        {

            try
            {
                var fabricClient = new FabricClient();
                var result = new List<FullUserDTOs>();

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey); //kao da se koristi lokalno
                    var partitionResult = await proxy.listUsers(); //preuzimanje korisnika
                    result.AddRange(partitionResult);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new List<FullUserDTOs>(); // Return an empty list or handle the error as needed
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginUserDTOs user) //FromBody
        {
            if (string.IsNullOrEmpty(user.Email) || !CheckEmail(user.Email)) return BadRequest("Invalid email format");
            if (string.IsNullOrEmpty(user.Password)) return BadRequest("Password cannot be null or empty");

            try
            {
                var fabricClient = new FabricClient();
                LogedUserDTOs result = null; // Initialize result to null

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);
                    var partitionResult = await proxy.loginUser(user); //ako se korisnik pronasao

                    if (partitionResult != null)
                    {
                        result = partitionResult;
                        break;
                    }
                }

                if (result != null) //korisnik sa odgovarajucim emailom i passwword je pronadjen
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])); //generisanje sigurnosnog kljuca iz tajnog niza koji sam definisao u konfiguraciji
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); //kredencijali za potpisivanje JWT tokena 

                    List<Claim> claims = new List<Claim>(); //kreiranje liste zahteva
                    claims.Add(new Claim("MyCustomClaim", result.Roles.ToString())); //dodajem korisnicku ulogu u listu zahteva

                    var Sectoken = new JwtSecurityToken(_config["Jwt:Issuer"], //kreira se sporedni JWT token sa postavljenim zahtevima
                        _config["Jwt:Issuer"],
                        claims,
                        expires: DateTime.Now.AddMinutes(360),
                        signingCredentials: credentials);

                    var token = new JwtSecurityTokenHandler().WriteToken(Sectoken); //generisanje JWT tokena u string formatu

                    var response = new
                    {
                        token = token, //generisani token
                        user = result, //pronadjeni korisnik
                        message = "Login successful" //poruka o uspesnoj prijavi
                    };

                    return Ok(response); //ako se nasao korisnik vraca se response sa ovim iznad informacijama
                }
                else
                {
                    return BadRequest("Incorrect email or password");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while login User");
            }
        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllDrivers() //funkcija za preuizmanje svih vozaca koji postoje, dostupno admin-ima
        {
            try
            {

                var fabricClient = new FabricClient();
                List<DriverDetailsDTOs> result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey); //kao da je lokalno
                    var parititonResult = await proxy.listDrivers(); //lista svih vozaca
                    if (parititonResult != null)
                    {
                        result = parititonResult;
                        break;
                    }

                }

                if (result != null)
                {

                    var response = new //kreiranje odgovora
                    {
                        drivers = result, //obavezni vozaci
                        message = "Succesfuly get list of drivers"
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


        [Authorize(Policy = "Admin")]
        [HttpPut]
        public async Task<IActionResult> ChangeDriverStatus([FromBody] DriverStatusUpdateDTOs driver)
        {
            try
            {

                var fabricClient = new FabricClient();
                bool result = false; //da li se uspesno promenilo

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService")); //isto
                foreach (var partition in partitionList) //sve isto 
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);
                    bool parititonResult = await proxy.changeDriverStatus(driver.Id, driver.Status); //azuiranje statusa vozaca
                    result = parititonResult;
                }

                if (result) return Ok("Succesfuly changed driver status");

                else return BadRequest("Driver status is not changed");

            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while registering new User");
            }
        }


        [AllowAnonymous] //metoda moze biti pozvana i bez autentifikacije
        [HttpPut]
        public async Task<IActionResult> ChangeUserFields([FromForm] UserUpdateModel user) //FromForm radi fajlova slike npr
        {
            UserUpdateNetworkModel userForUpdate = new UserUpdateNetworkModel(user);

            try
            {
                var fabricClient = new FabricClient(); //standardno
                FullUserDTOs result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService")); //lista particija
                foreach (var partition in partitionList) //kroz svaku
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey); //specificna particija u servisu
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey); //komuniakcija sa servisom i u servisu sa odredjenom particijomn na osnovu parittionKey
                    var proxyResult = await proxy.changeUserFields(userForUpdate); //poziva se metoda za azuriranje korisnika
                    if (proxyResult != null)
                    {
                        result = proxyResult;
                        break;
                    }
                }

                if (result != null) //ako je korisnik uspesno azuriran
                {
                    var response = new //kreiranje odogovora
                    {
                        changedUser = result, //salje se taj kroisnik azuriran
                        message = "Succesfuly changed user fields!" // i poruka
                    };
                    return Ok(response);
                }
                else return StatusCode(409, "User for change is not in db!");

            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating user");
            }
        }


        [AllowAnonymous] //svkai moze
        [HttpGet]
        public async Task<IActionResult> GetUserInfo([FromQuery] Guid id) //dobavljanje informacija o korisniku na osnovu poslatog id, kroz url je na frontedu na api dodat Id, npr. id = ... ---> FromQuery
        {
            try
            {   //sve isto
                var fabricClient = new FabricClient();
                FullUserDTOs result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);
                    var partitionResult = await proxy.GetUserInfo(id); //dobavi infomracije za taj Id
                    if (partitionResult != null)
                    {
                        result = partitionResult;
                        break;
                    }
                }

                if (result != null) //ako je uspesno nadjen
                {
                    var response = new //spakuj poruku
                    {
                        user = result,
                        message = "Successfully retrieved user info"
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("This id does not exist");
                }
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving user info");
            }
        }

        private bool CheckEmail(string email) //vaidacija email-a
        {
            const string pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            return Regex.IsMatch(email, pattern);
        }

        //OVDJE

        [Authorize(Policy = "Admin")]
        [HttpPut]
        public async Task<IActionResult> VerifyDriver([FromBody] DriverVerificationRequestDTOs driver) //verifikovanje vozaca, SLANJE EMAIL-A
        {   //sve isto
            try
            {
                var fabricClient = new FabricClient();
                bool result = false;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);
                    var partitionResult = await proxy.VerifyDriver(driver.Id, driver.Email, driver.Action); //verifikacija vozaca
                    if (partitionResult != null)
                    {
                        result = partitionResult;
                        break;
                    }
                }

                if (result) //ako je uspesno verifikovan
                {
                    var response = new //kreiram poruku
                    {
                        Verified = result, //vracam tog verifikovanog vozaca
                        message = $"Driver with id:{driver.Id} is now changed status of verification to:{driver.Action}" //i poruku
                    };
                    //ako je vozac "Prihvacen" to jeste verifikovan saljem email tom korisniku da je uspesno verifikovan
                    if (driver.Action == "Prihvacen") await this.emailSender.SendEmailAsync(driver.Email, "Account verification", "Successfuly verified on taxi app now you can drive!");

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


        //OVDJE

        [Authorize(Policy = "Admin")] //samo adminima
        [HttpGet]
        public async Task<IActionResult> GetDriversForVerification() //preuzimanje liste vozaca koji nisu verifikovani
        {
            try
            {
                //povezivanje isto
                var fabricClient = new FabricClient();
                List<DriverDetailsDTOs> result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey); //komunikacija sa zelejnom instancom i odredjenom particijom unutar nje
                    var parititonResult = await proxy.GetNotVerifiedDrivers(); //funkcija za dobavljanje
                    if (parititonResult != null)
                    {
                        result = parititonResult;
                        break;
                    }

                }

                if (result != null)
                {

                    var response = new //kreiram poruku za vracanje na front
                    {
                        drivers = result, //sve vozace
                        message = "Succesfuly get list of drivers" //poruka
                    };
                    return Ok(response);
                }
                else
                {
                    return BadRequest("Incorrect email or password"); //400
                }

            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while registering new User");
            }
        }
    }
}
