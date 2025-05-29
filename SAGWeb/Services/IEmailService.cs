namespace SAGWeb.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);

        Task SendEmailWithAttachmentsAsync(
        string to,
        string subject,
        string body,
        List<(byte[] content, string fileName, string contentType)> attachments = null
    );
    }
}
