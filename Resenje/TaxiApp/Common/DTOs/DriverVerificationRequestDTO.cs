namespace Common.DTOs
{
    public class DriverVerificationRequestDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; }

        public string Action { get; set; }
    }
}
