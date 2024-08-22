namespace Common.Interfaces
{
    public interface IEmailService //ugovor o slanju email-ova
    {
        Task SendEmailAsync(string email, string subject, string message); //asihrona metoda, nema povratnu vrednost i koristim Task da bi radilo asihrono
    }
}
