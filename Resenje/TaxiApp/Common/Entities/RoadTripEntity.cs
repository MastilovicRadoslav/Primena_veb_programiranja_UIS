using Microsoft.WindowsAzure.Storage.Table;

namespace Common.Entities
{
    public class RoadTripEntity : TableEntity
    {
        public Guid RiderId { get; set; }
        public Guid DriverId { get; set; }

        public string CurrentLocation { get; set; }

        public string Destination { get; set; }

        public bool Accepted { get; set; }

        public double Price { get; set; }

        public Guid TripId { get; set; }

        public int SecondsToDriverArive { get; set; }

        public int SecondsToEndTrip { get; set; }

        public bool IsFinished { get; set; }

        public bool IsRated { get; set; }
        public RoadTripEntity()
        {
        }

        public RoadTripEntity(Guid userId, Guid driverId, string currentLocation, string destination, bool accepted, double price, Guid triId, int minutes)
        {
            RiderId = userId;
            DriverId = driverId;
            CurrentLocation = currentLocation;
            Destination = destination;
            Accepted = accepted;
            Price = price;
            TripId = triId;
            RowKey = triId.ToString();
            PartitionKey = triId.ToString();
            SecondsToDriverArive = minutes;
            SecondsToEndTrip = 0;
            IsFinished = false;
            IsRated = false;
        }
    }
}
