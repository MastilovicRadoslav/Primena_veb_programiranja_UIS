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

    }
}
