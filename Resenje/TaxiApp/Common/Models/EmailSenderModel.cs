using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class EmailSenderModel : IEmailService
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.uns.ac.rs", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("mastilovic.pr106.2020@uns.ac.rs", "coleelektrotehnika")
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("mastilovic.pr106.2020@uns.ac.rs"),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }
}