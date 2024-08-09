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
        Task<bool> addNewUser(UserModel user);
        [OperationContract]
        Task<List<UserDetailsDTO>> listUsers();
        [OperationContract]
        Task<LogedUserDTO> loginUser(LoginUserDTO loginUserDTO);
        [OperationContract]
        Task<List<DriverDetailsDTO>> listDrivers();
        [OperationContract]
        Task<bool> changeDriverStatus(Guid id, bool status);
        [OperationContract]
        Task<UserDetailsDTO> changeUserFields(UserUpdateNetworkModel user);
        [OperationContract]
        Task<UserDetailsDTO> GetUserInfo(Guid id);
        [OperationContract]
        Task<bool> VerifyDriver(Guid id, string email, string action);
        [OperationContract]
        Task<List<DriverDetailsDTO>> GetNotVerifiedDrivers();

    }
}
