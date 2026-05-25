using Easrms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Easrms.Infrastructure.BackgroundWorkers.Workers;

// ─────────────────────────────────────────────────────────────────────────────
// ExpiredRefreshTokenCleanupWorker
// Runs once every 24 hours.
// Finds all Users where RefreshTokenExpiryOn < UtcNow and nulls out
// RefreshToken + RefreshTokenExpiryOn — keeping the Users table clean.
//
// Uses IServiceScopeFactory to safely resolve the scoped AppDbContext
// from a singleton-lifetime hosted service.
// ─────────────────────────────────────────────────────────────────────────────

public class ExpiredRefreshTokenCleanupWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredRefreshTokenCleanupWorker> _logger;

    public ExpiredRefreshTokenCleanupWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredRefreshTokenCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ExpiredRefreshTokenCleanupWorker] Started. Interval: {Interval}", Interval);

        // Run once immediately on startup, then every 24 hours.
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }

        _logger.LogInformation("[ExpiredRefreshTokenCleanupWorker] Stopping.");
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            // Resolve AppDbContext directly for an efficient bulk update.
            // Replace "AppDbContext" with your actual DbContext class name if different.
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;

            // Bulk update — no need to load entities into memory.
            int affected = await db.Users
                .Where(u => u.RefreshTokenExpiryOn != null && u.RefreshTokenExpiryOn < now)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.RefreshToken, (string?)null)
                    .SetProperty(u => u.RefreshTokenExpiryOn, (DateTime?)null),
                    ct);

            if (affected > 0)
                _logger.LogInformation(
                    "[ExpiredRefreshTokenCleanupWorker] Cleared expired refresh tokens for {Count} user(s).",
                    affected);
            else
                _logger.LogDebug("[ExpiredRefreshTokenCleanupWorker] No expired refresh tokens found.");
        }
        catch (Exception ex)
        {
            // Log and swallow — a cleanup failure should never crash the host.
            _logger.LogError(ex, "[ExpiredRefreshTokenCleanupWorker] Error during cleanup.");
        }
    }
}
