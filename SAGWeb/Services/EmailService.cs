using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace SAGWeb.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            await SendEmailWithAttachmentsAsync(to, subject, body, null);
        }

        public async Task SendEmailWithAttachmentsAsync(
            string to,
            string subject,
            string body,
            List<(byte[] content, string fileName, string contentType)> attachments = null)
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var smtpUser = _configuration["EmailSettings:SmtpUser"];
            var smtpPass = _configuration["EmailSettings:SmtpPass"];
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"] ?? "SAGRISA";
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = enableSsl,
                    UseDefaultCredentials = false, // Importante: establecer en false primero
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000 // 30 segundos timeout
                };

                using var message = new MailMessage()
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    Priority = MailPriority.Normal
                };

                message.To.Add(to);

                // Agregar archivos adjuntos si existen
                if (attachments != null && attachments.Any())
                {
                    foreach (var attachment in attachments)
                    {
                        var stream = new MemoryStream(attachment.content);
                        var mailAttachment = new Attachment(stream, attachment.fileName, attachment.contentType);
                        message.Attachments.Add(mailAttachment);
                    }
                }

                _logger.LogInformation($"Enviando correo a: {to} con asunto: {subject}");
                await client.SendMailAsync(message);
                _logger.LogInformation($"Correo enviado exitosamente a: {to}");
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, $"Error SMTP al enviar correo a {to}: {smtpEx.Message}");
                throw new Exception($"Error de servidor de correo: {smtpEx.Message}. Verifique la configuración SMTP y las credenciales.", smtpEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error general al enviar correo a {to}: {ex.Message}");
                throw new Exception($"Error al enviar el correo: {ex.Message}", ex);
            }
        }
    }
}