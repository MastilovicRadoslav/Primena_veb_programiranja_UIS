using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
namespace Common.Interfaces
{
    public interface IDriveService : IService
    {
        [OperationContract]
        Task<RoadTripModel> AcceptRoadTrip(RoadTripModel trip);
        [OperationContract]
        Task<RoadTripModel> AcceptRoadTripDriver(Guid rideId, Guid driverId);
        [OperationContract]
        Task<List<RoadTripModel>> GetRoadTrips();
    }
}
