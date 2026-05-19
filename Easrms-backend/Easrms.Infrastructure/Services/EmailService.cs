using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace Easrms.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendRequestOpenedAsync(string toEmail, string requestNumber, string requestTitle)
    {
        await SendAsync(
            toEmail,
            EmailTemplates.RequestOpenedSubject(),
            EmailTemplates.RequestOpenedBody(requestNumber, requestTitle)
        );
    }

    public async Task SendRequestResolvedAsync(string toEmail, string requestNumber, string requestTitle)
    {
        await SendAsync(
            toEmail,
            EmailTemplates.RequestResolvedSubject(),
            EmailTemplates.RequestResolvedBody(requestNumber, requestTitle)
        );
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var resendApiKey = _configuration["EmailSettings:Resend_Api_Key"]
                ?? throw new KeyNotFoundException("Resend Api Key Not Found");
            var From = _configuration["EmailSettings:FromEmail"]
                ?? throw new KeyNotFoundException("From Email Not Found");

            var client = ResendClient.Create(resendApiKey);

            await client.EmailSendAsync(new EmailMessage
            {
                From     = From,
                To       = toEmail,
                Subject  = subject,
                HtmlBody = htmlBody,
            });
        }
        catch (Exception ex)
        {
            // Email failure must NEVER crash the main flow — just log it
            _logger.LogError(ex, "Failed to send email to {Email} | Subject: {Subject}", toEmail, subject);
        }
    }
}
