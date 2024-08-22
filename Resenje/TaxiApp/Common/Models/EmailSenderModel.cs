using Common.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Common.Models
{
    public class EmailSenderModel : IEmailService //slanje email-a
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.uns.ac.rs", 587)
            {
                EnableSsl = true, //sigurna komunikacija
                UseDefaultCredentials = false, //nije heskodovano
                Credentials = new NetworkCredential("mastilovic.pr106.2020@uns.ac.rs", "coleelektrotehnika")
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("mastilovic.pr106.2020@uns.ac.rs"),//adresa posiljaoca
                Subject = subject, //naslov poruke
                Body = message, //tijelo poruke
                IsBodyHtml = true //intikator za HTML
            };

            mailMessage.To.Add(email); //dodaje se adresa primaoca

            return client.SendMailAsync(mailMessage); //saljem poruku asihrono
        }
    }
}