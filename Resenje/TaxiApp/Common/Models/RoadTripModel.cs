using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class RoadTripModel //isto kao UserModel koristim za smestanje u IreliableDictionary "Trips" pa sadrzi sve informaicje
    {
        [DataMember]
        public string CurrentLocation { get; set; }

        [DataMember]
        public string Destination { get; set; }

        [DataMember]
        public Guid RiderId { get; set; }

        [DataMember]
        public Guid DriverId { get; set; } //DODATO kreira se voznja i za vozaca i za korisnika voznje

        [DataMember]
        public double Price { get; set; }

        [DataMember]
        public bool Accepted { get; set; }

        [DataMember]
        public Guid TripId { get; set; } //DODATO

        [DataMember]
        public int SecondsToDriverArrive { get; set; }

        [DataMember]
        public int SecondsToEndTrip { get; set; } //DODATO  koliko voznja traje

        [DataMember]
        public bool IsFinished { get; set; } //DODATO, kao default je false a kad Vozac prihvati voznju voznja krece sa odbrojavanjem i na kraju se postavlja na true

        [DataMember]
        public bool IsRated { get; set; } //DDDATO
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
            TripId = Guid.NewGuid(); //dodato Id voznje
        }
        //AcceptSuggestedDrive koristim ga kad kreiram AcceptRoadTripModel pa kreiram RoadTripModel i dodam neka polja  u konsturkotr kao sto su TripId, DriverId, IsFinished, IsRated
        public RoadTripModel(string currentLocation, string destination, Guid riderId, double price, bool accepted, int minutes)
        {
            CurrentLocation = currentLocation;
            Destination = destination;
            RiderId = riderId;
            Price = price;
            Accepted = accepted; // nije prihvacena je na pocetku
            TripId = Guid.NewGuid();
            DriverId = new Guid("00000000-0000-0000-0000-000000000000"); // putanja nema vozaca
            SecondsToDriverArrive = minutes * 60;
            IsFinished = false;
            IsRated = false;
        }
        //Koristim kad mapiram RoadTripEntity na RoadTripModel
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
