using Common.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Common.Models
{
    public class EmailSenderModel : IEmailService
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("drsprojekat2023@gmail.com", "Bulevar Despota Stefana 7")
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("drsprojekat2023@gmail.com"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }
}
