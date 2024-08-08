using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class RoadTripModel
    {
        [DataMember]
        public string CurrentLocation { get; set; }

        [DataMember]
        public string Destination { get; set; }

        [DataMember]
        public Guid RiderId { get; set; }

        [DataMember]
        public Guid DriverId { get; set; }

        [DataMember]
        public double Price { get; set; }

        [DataMember]
        public bool Accepted { get; set; }

        [DataMember]
        public Guid TripId { get; set; }

        [DataMember]
        public int SecondsToDriverArrive { get; set; }

        [DataMember]
        public int SecondsToEndTrip { get; set; }

        [DataMember]
        public bool IsFinished { get; set; }

        [DataMember]
        public bool IsRated { get; set; }
        public RoadTripModel()
        {
        }

        public RoadTripModel(string currentLocation, string destination, Guid riderId, Guid driverId, double price, bool accepted)
        {
            CurrentLocation = currentLocation;
            Destination = destination;
            RiderId = riderId;
            DriverId = driverId;
            Price = price;
            Accepted = accepted;
            TripId = Guid.NewGuid();
        }

        public RoadTripModel(string currentLocation, string destination, Guid riderId, double price, bool accepted, int minutes)
        {
            CurrentLocation = currentLocation;
            Destination = destination;
            RiderId = riderId;
            Price = price;
            Accepted = accepted; // by default is false 
            TripId = Guid.NewGuid();
            DriverId = new Guid("00000000-0000-0000-0000-000000000000"); // that say this trip dont have driver
            SecondsToDriverArrive = minutes * 60;
            IsFinished = false;
            IsRated = false;
        }

        public RoadTripModel(string currentLocation, string destination, Guid riderId, Guid driverId, double price, bool accepted, Guid tripId, int minutesToDriverArrive, int minutesToEnd, bool isFinished, bool isRated) : this(currentLocation, destination, riderId, driverId, price, accepted)
        {
            TripId = tripId;
            SecondsToDriverArrive = minutesToDriverArrive;
            SecondsToEndTrip = minutesToEnd;
            IsFinished = isFinished;
            IsRated = isRated;
        }
    }
}
