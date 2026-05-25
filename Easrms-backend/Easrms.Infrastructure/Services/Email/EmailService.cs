using Easrms.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace Easrms.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IEmailQueue _emailQueue;           // ← NEW

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        IEmailQueue emailQueue)                         // ← NEW
    {
        _configuration = configuration;
        _logger = logger;
        _emailQueue = emailQueue;                    // ← NEW
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

    public async Task SendSLANearingBreachAsync(string toEmail, string requestNumber, string requestTitle)
    {
        await SendAsync(
            toEmail,
            EmailTemplates.SLANearingBreachSubject(),
            EmailTemplates.SLANearingBreachBody(requestNumber, requestTitle)
        );
    }

    public async Task SendSLABreachedAsync(string toEmail, string requestNumber, string requestTitle)
    {
        await SendAsync(
            toEmail,
            EmailTemplates.SLABreachedSubject(),
            EmailTemplates.SLABreachedBody(requestNumber, requestTitle)
        );
    }

    public async Task SendRequestEscalatedAsync(string toEmail, string requestNumber, string requestTitle, string escalationReason)
    {
        await SendAsync(
            toEmail,
            EmailTemplates.RequestEscalatedSubject(),
            EmailTemplates.RequestEscalatedBody(requestNumber, requestTitle, escalationReason)
        );
    }

    // ── Private ───────────────────────────────────────────────────────────────

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var resendApiKey = _configuration["EmailSettings:Resend_Api_Key"]
                ?? throw new KeyNotFoundException("Resend Api Key Not Found");

            var from = _configuration["EmailSettings:FromEmail"]
                ?? throw new KeyNotFoundException("From Email Not Found");

            var client = ResendClient.Create(resendApiKey);

            await client.EmailSendAsync(new EmailMessage
            {
                From = from,
                To = toEmail,
                Subject = subject,
                HtmlBody = htmlBody,
            });
        }
        catch (Exception ex)
        {
            // Email failure must NEVER crash the main flow — log it and queue for retry
            _logger.LogError(ex, "Failed to send email to {Email} | Subject: {Subject} | Queued for retry.", toEmail, subject);

            // ← NEW: enqueue for RetryFailedEmailWorker to pick up within 5 minutes
            _emailQueue.Enqueue(new FailedEmailItem
            {
                To = toEmail,
                Subject = subject,
                Body = htmlBody
            });
        }
    }
}