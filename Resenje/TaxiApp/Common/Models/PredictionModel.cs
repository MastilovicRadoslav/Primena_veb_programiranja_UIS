using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class PredictionModel
    {
        [DataMember]
        public double PredictionPrice { get; set; }
        [DataMember]
        public TimeSpan DriversArivalSeconds { get; set; }

        [DataMember]
        public TimeSpan RideTime { get; set; }

        public PredictionModel(double predictionPrice, TimeSpan driversArivalSeconds, TimeSpan rideTime)
        {
            PredictionPrice = predictionPrice;
            DriversArivalSeconds = driversArivalSeconds;
            RideTime = rideTime;
        }
    }
}
