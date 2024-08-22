namespace Common.DTOs
{
    public class DriverVerificationRequestDTOs //prenos podataka izmedju frontenda i backenda kada se verifikuje vozac iz VerifyDrivers
    {
        public Guid Id { get; set; }
        public string Email { get; set; }

        public string Action { get; set; }
    }
}
