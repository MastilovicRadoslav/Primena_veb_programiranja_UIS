namespace Common.DTOs
{
    public class RideForAcceptDTOs
    {
        public Guid DriverId { get; set; } // ID vozača koji prihvata vožnju
        public Guid RideId { get; set; } // ID vožnje koja se prihvata
        public RideForAcceptDTOs() { }
    }
}
