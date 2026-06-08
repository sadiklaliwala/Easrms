namespace Easrms.Infrastructure.Services;

public interface IClaudeService
{
    /// <summary>
    /// Sends the user message to Claude and gets back a JSON intent string.
    /// Example return: { "intent": "get_my_requests", "filters": { "status": "Open" } }
    /// </summary>
    Task<string> ResolveIntentAsync(string userMessage, string userRole);

    /// <summary>
    /// Sends the fetched data context to Claude and gets back a friendly
    /// human-readable reply to show in the chat window.
    /// </summary>
    Task<string> FormatReplyAsync(
        string originalMessage,
        string intent,
        string dataContext,
        string userName,
        string userRole);
}
