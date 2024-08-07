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
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        private IConfiguration _configuration;
        private readonly IEmailService emailSender;

        public UsersController(IConfiguration config, IEmailService emailSender)
        {
            _configuration = config;
            this.emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] UserRegistrationModel userData)
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

                var fabricClient = new FabricClient();
                bool result = false;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);
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
        public async Task<List<UserDetailsDTO>> GetUsers()
        {

            try
            {
                var fabricClient = new FabricClient();
                var result = new List<UserDetailsDTO>();

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);
                    var partitionResult = await proxy.listUsers();
                    result.AddRange(partitionResult);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                return new List<UserDetailsDTO>(); // Return an empty list or handle the error as needed
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO user)
        {
            if (string.IsNullOrEmpty(user.Email) || !CheckEmail(user.Email)) return BadRequest("Invalid email format");
            if (string.IsNullOrEmpty(user.Password)) return BadRequest("Password cannot be null or empty");

            try
            {
                var fabricClient = new FabricClient();
                LogedUserDTO result = null; // Initialize result to null

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);
                    var partitionResult = await proxy.loginUser(user);

                    if (partitionResult != null)
                    {
                        result = partitionResult;
                        break;
                    }
                }

                if (result != null)
                {
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    List<Claim> claims = new List<Claim>();
                    claims.Add(new Claim("MyCustomClaim", result.Roles.ToString()));

                    var Sectoken = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                        _configuration["Jwt:Issuer"],
                        claims,
                        expires: DateTime.Now.AddMinutes(360),
                        signingCredentials: credentials);

                    var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

                    var response = new
                    {
                        token = token,
                        user = result,
                        message = "Login successful"
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
                return StatusCode(500, "An error occurred while login User");
            }
        }

        private bool CheckEmail(string email)
        {
            const string pattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            return Regex.IsMatch(email, pattern);
        }

        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllDrivers()
        {
            try
            {

                var fabricClient = new FabricClient();
                List<DriverDetailsDTO> result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);
                    var parititonResult = await proxy.listDrivers();
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
                        drivers = result,
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
        public async Task<IActionResult> ChangeDriverStatus([FromBody] DriverStatusUpdateDTO driver)
        {
            try
            {

                var fabricClient = new FabricClient();
                bool result = false;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);
                    bool parititonResult = await proxy.changeDriverStatus(driver.Id, driver.Status);
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

        [AllowAnonymous]
        [HttpPut]
        public async Task<IActionResult> ChangeUserFields([FromForm] UserUpdateModel user)
        {
            UserUpdateNetworkModel userForUpdate = new UserUpdateNetworkModel(user);

            try
            {
                var fabricClient = new FabricClient();
                UserDetailsDTO result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);
                    var proxyResult = await proxy.changeUserFields(userForUpdate);
                    if (proxyResult != null)
                    {
                        result = proxyResult;
                        break;
                    }
                }

                if (result != null)
                {
                    var response = new
                    {
                        changedUser = result,
                        message = "Succesfuly changed user fields!"
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

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetUserInfo([FromQuery] Guid id)
        {
            try
            {
                var fabricClient = new FabricClient();
                UserDetailsDTO result = null;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);
                    var partitionResult = await proxy.GetUserInfo(id);
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

        [Authorize(Policy = "Admin")]
        [HttpPut]
        public async Task<IActionResult> VerifyDriver([FromBody] DriverVerificationRequestDTO driver)
        {
            try
            {
                var fabricClient = new FabricClient();
                bool result = false;

                var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/TaxiApp/UsersService"));
                foreach (var partition in partitionList)
                {
                    var partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)partition.PartitionInformation).LowKey);
                    var proxy = ServiceProxy.Create<IUserService>(new Uri("fabric:/TaxiApp/UsersService"), partitionKey);
                    var partitionResult = await proxy.VerifyDriver(driver.Id, driver.Email, driver.Action);
                    if (partitionResult != null)
                    {
                        result = partitionResult;
                        break;
                    }
                }

                if (result)
                {
                    var response = new
                    {
                        Verified = result,
                        message = $"Driver with id:{driver.Id} is now changed status of verification to:{driver.Action}"
                    };
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
    }
}
