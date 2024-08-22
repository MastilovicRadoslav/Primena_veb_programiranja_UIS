namespace Common.Models
{
    public class TripModel //prima podatke da fronteda kad se trazi cena za odredjenu piutanju
    {
        public string Destination { get; set; }
        public string CurrentLocation { get; set; }

        public TripModel()
        {
        }
    }
}
