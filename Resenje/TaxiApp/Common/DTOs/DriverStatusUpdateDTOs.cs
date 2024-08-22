namespace Common.DTOs
{
    public class DriverStatusUpdateDTOs //DTO koji sadrzi podatke potrebne za azuriranje statusa vozaca (Id vozaca i novi Status)
    {
        public Guid Id { get; set; }
        public bool Status { get; set; }
        public DriverStatusUpdateDTOs() { }
    }
}
