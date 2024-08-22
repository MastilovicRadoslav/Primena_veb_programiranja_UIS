using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class PredictionModel //za procenjenu cenu 
    {
        [DataMember]
        public double EstimatedPrice { get; set; } //cena
        [DataMember]
        public TimeSpan DriversArivalSeconds { get; set; } //vreme dolaska vozaca  min

        [DataMember]
        public TimeSpan RideTime { get; set; } //trjanje voznje

        public PredictionModel(double estimatedPrice, TimeSpan driversArivalSeconds, TimeSpan rideTime)
        {
            EstimatedPrice = estimatedPrice;
            DriversArivalSeconds = driversArivalSeconds;
            RideTime = rideTime;
        }
    }
}
