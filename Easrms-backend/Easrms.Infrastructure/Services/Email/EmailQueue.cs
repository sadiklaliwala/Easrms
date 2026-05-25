using System.Collections.Concurrent;

namespace Easrms.Infrastructure.Services.Email;

// ─────────────────────────────────────────────────────────────────────────────
// EmailQueue
// Registered as Singleton. Your existing email-send code catches exceptions
// and calls EmailQueue.Enqueue(...) so the RetryFailedEmailWorker can pick
// them up and retry automatically.
// ─────────────────────────────────────────────────────────────────────────────

public class FailedEmailItem
{
    public string To { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public int RetryCount { get; set; } = 0;
    public DateTime FirstFailedAt { get; init; } = DateTime.UtcNow;
}

public interface IEmailQueue
{
    void Enqueue(FailedEmailItem item);
    bool TryDequeue(out FailedEmailItem? item);
    int Count { get; }
}

public class EmailQueue : IEmailQueue
{
    private const int MaxQueueSize = 500;

    private readonly ConcurrentQueue<FailedEmailItem> _queue = new();

    /// <summary>
    /// Call this from your email service catch block.
    /// Silently drops items if queue is already at MaxQueueSize to
    /// prevent unbounded memory growth.
    /// </summary>
    public void Enqueue(FailedEmailItem item)
    {
        if (_queue.Count >= MaxQueueSize) return;
        _queue.Enqueue(item);
    }

    public bool TryDequeue(out FailedEmailItem? item)
        => _queue.TryDequeue(out item);

    public int Count => _queue.Count;
}
