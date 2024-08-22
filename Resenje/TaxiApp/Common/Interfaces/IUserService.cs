using Common.DTOs;
using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IUserService : IService
    {
        [OperationContract]
        Task<bool> addNewUser(UserModel user); //za Register

        [OperationContract]
        Task<LogedUserDTOs> loginUser(LoginUserDTOs loginUserDTO); //za Login

        [OperationContract]
        Task<List<FullUserDTOs>> listUsers(); //za GetUsers

        [OperationContract]
        Task<List<DriverDetailsDTOs>> listDrivers(); //Za GetAllDrivers

        [OperationContract]
        Task<bool> changeDriverStatus(Guid id, bool status); //Za ChangeDriverStatus

        [OperationContract]
        Task<FullUserDTOs> changeUserFields(UserUpdateNetworkModel user); //Za ChangeUserFields

        [OperationContract]
        Task<FullUserDTOs> GetUserInfo(Guid id); //Za GetUserInfo

        [OperationContract]
        Task<bool> VerifyDriver(Guid id, string email, string action); //verifikovanje vozaca

        [OperationContract]
        Task<List<DriverDetailsDTOs>> GetNotVerifiedDrivers(); //Dobavljanje neverifikovanih vozaca




    }
}
