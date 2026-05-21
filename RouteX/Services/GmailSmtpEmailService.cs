using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RouteX.Services
{
    public class GmailSmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailSmtpEmailService> _logger;

        public GmailSmtpEmailService(IConfiguration configuration, ILogger<GmailSmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var host = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
                var portStr = _configuration["Smtp:Port"] ?? "587";
                var enableSslStr = _configuration["Smtp:EnableSsl"] ?? "true";
                var username = _configuration["Smtp:Username"] ?? "routex.otp@gmail.com";
                var password = _configuration["Smtp:Password"] ?? "your-gmail-app-password";

                int.TryParse(portStr, out int port);
                bool.TryParse(enableSslStr, out bool enableSsl);

                using (var client = new SmtpClient(host, port))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(username, password);
                    client.EnableSsl = enableSsl;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(username, "RouteX Authentication"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(to);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("Email sent successfully to {To}", to);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }
    }
}
