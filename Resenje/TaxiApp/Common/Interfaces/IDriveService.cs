using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IDriveService : IService
    {
        [OperationContract]
        Task<RoadTripModel> AcceptRoadTrip(RoadTripModel trip); //prihvatanje voznje

        [OperationContract]
        Task<RoadTripModel> GetCurrentRoadTrip(Guid id); //Dobavljanje trenutne voznje za vozaca i korisnika

        [OperationContract]
        Task<List<RoadTripModel>> GetRoadTrips(); //dobavljanje svih voznji koje vozac moze da prihvati

        [OperationContract]
        Task<RoadTripModel> AcceptRoadTripDriver(Guid rideId, Guid driverId); //prihvata vozac voznju



        [OperationContract]
        Task<List<RoadTripModel>> GetListOfCompletedRidesForDriver(Guid driverId); //Za Drivera-a prikaz voznji

        [OperationContract]
        Task<List<RoadTripModel>> GetListOfCompletedRidesForRider(Guid riderId); //Za Rider-a prikaz voznji


        [OperationContract]
        Task<List<RoadTripModel>> GetListOfCompletedRidesAdmin(); //Za admina prikaz voznji

        [OperationContract]
        Task<RoadTripModel> GetCurrentTrip(Guid id); //Dobavljanje trenutne voznje za DashboardRider


        [OperationContract]
        Task<RoadTripModel> GetCurrentTripDriver(Guid id); //Dobavljanje trenutne voznje za DashboardDriver

        [OperationContract]
        Task<List<RoadTripModel>> GetAllNotRatedTrips(); //dobavljanje svih neocenjihn voznji za korisnika

        [OperationContract]
        Task<bool> SubmitRating(Guid tripId, int rating); //za postavljaje ocene za odredjenju voznju
    }
}
