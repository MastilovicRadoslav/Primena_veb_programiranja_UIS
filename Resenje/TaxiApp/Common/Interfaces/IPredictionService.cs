using Common.Models;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IPredictionService : IService
    {
        [OperationContract]
        Task<PredictionModel> GetPredictionPrice(string currentLocation, string destination); //Servis za predvidjanje cene izabrane voznje
    }
}
