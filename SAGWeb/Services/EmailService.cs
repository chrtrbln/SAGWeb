using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace SAGWeb.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            using var smtp = new SmtpClient
            {
                Host = _config["EmailSettings:SmtpHost"],
                Port = int.Parse(_config["EmailSettings:Port"]),
                Credentials = new NetworkCredential(
                    _config["EmailSettings:User"],
                    _config["EmailSettings:Password"]),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }
    }
}
