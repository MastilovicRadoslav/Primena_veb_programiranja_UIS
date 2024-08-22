using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IRatingService : IService //interfejs koji sam dodao da vozacu dodam ocenu
    {
        [OperationContract]
        Task<bool> AddRating(Guid driverId, int rating);

    }
}
