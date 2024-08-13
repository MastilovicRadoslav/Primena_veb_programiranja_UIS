using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IRatingService : IService
    {
        [OperationContract]
        Task<bool> AddRating(Guid driverId, int rating);

    }
}
