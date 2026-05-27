using Easrms.Application.Interfaces.Email;
using Easrms.Infrastructure.Services.Email;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Easrms.Infrastructure.BackgroundWorkers.Workers;

// ─────────────────────────────────────────────────────────────────────────────
// RetryFailedEmailWorker
// Runs every 5 minutes. Drains the in-memory EmailQueue and retries each
// failed email up to MaxRetries times. After that the item is permanently
// discarded (logged as a warning).
//
// IEmailSender is YOUR existing email service — inject it here.
// Replace the placeholder interface below with your actual one.
// ─────────────────────────────────────────────────────────────────────────────

public class RetryFailedEmailWorker : BackgroundService
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    private readonly IEmailQueue _emailQueue;
    private readonly IEmailService _emailService;
    private readonly ILogger<RetryFailedEmailWorker> _logger;

    public RetryFailedEmailWorker(
        IEmailQueue emailQueue,
        IEmailService emailService  ,
        ILogger<RetryFailedEmailWorker> logger)
    {
        _emailQueue = emailQueue;
        _emailService = emailService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[RetryFailedEmailWorker] Started. Interval: {Interval}", Interval);

        // Small startup delay so the app finishes booting before first run.
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessQueueAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }

        _logger.LogInformation("[RetryFailedEmailWorker] Stopping.");
    }
    private async Task ProcessQueueAsync(CancellationToken ct)
    {
        if (_emailQueue.Count == 0) return;

        _logger.LogInformation("[RetryFailedEmailWorker] Processing queue. Items: {Count}", _emailQueue.Count);

        // Snapshot the current queue depth so we don't loop forever if
        // re-enqueued items keep getting added during this cycle.
        int itemsThisCycle = _emailQueue.Count;

        for (int i = 0; i < itemsThisCycle; i++)
        {
            if (!_emailQueue.TryDequeue(out var item) || item is null) break;

            try
            {
                await _emailService.SendAsync(item.To, item.Subject, item.Body);

                _logger.LogInformation(
                    "[RetryFailedEmailWorker] Email sent successfully. To: {To} | Subject: {Subject} | WasRetry: {Retry}",
                    item.To, item.Subject, item.RetryCount);
            }
            catch (Exception ex)
            {
                item.RetryCount++;

                if (item.RetryCount < MaxRetries)
                {
                    // Put back in queue for the next cycle.
                    _emailQueue.Enqueue(item);

                    _logger.LogWarning(ex,
                        "[RetryFailedEmailWorker] Retry {Retry}/{Max} failed. To: {To} | Subject: {Subject}",
                        item.RetryCount, MaxRetries, item.To, item.Subject);
                }
                else
                {
                    // Permanently drop after MaxRetries — log as error so it's visible.
                    _logger.LogError(ex,
                        "[RetryFailedEmailWorker] PERMANENTLY FAILED after {Max} retries. " +
                        "To: {To} | Subject: {Subject} | FirstFailedAt: {At}",
                        MaxRetries, item.To, item.Subject, item.FirstFailedAt);
                }
            }
        }
    }
}
