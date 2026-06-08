using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
namespace Easrms.Infrastructure.Services;

public class ClaudeService : IClaudeService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    private const string ValidIntents =
        "get_my_requests | get_request_detail | get_dashboard_summary | " +
        "get_my_approvals | get_my_tasks | get_request_history | " +
        "get_request_comments | unknown";

    public ClaudeService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        _apiKey = configuration["Groq:ApiKey"]
            ?? throw new InvalidOperationException(
                "Groq:ApiKey is not configured.");

        _model = configuration["Groq:Model"]
            ?? "llama-3.3-70b-versatile";

        _httpClient.BaseAddress =
            new Uri("https://api.groq.com/openai/v1/");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<string> ResolveIntentAsync(string userMessage, string userRole)
    {
        var systemPrompt = $$"""
        You are an intent parser for the EASRMS (Employee Asset & Service Request Management System).

        Your ONLY job is to read the user's message and return a JSON object with the correct intent and filters.

        Current user role: {{userRole}}

        Available intents based on role:
        - Employee  : get_my_requests, get_request_detail, get_request_history, get_request_comments
        - Manager   : get_my_requests, get_my_approvals, get_request_detail, get_dashboard_summary, get_request_history, get_request_comments
        - Admin     : get_my_requests, get_request_detail, get_dashboard_summary, get_request_history, get_request_comments
        - SupportUser: get_my_tasks, get_request_detail, get_request_history, get_request_comments


        Available filter values:
        - status    : Open | Pending Approval | Approved | Rejected | Assigned | In Progress | Resolved | Closed
        - priority  : High | Medium | Low

        Response format (return ONLY this JSON, no extra text, no markdown):
        {
          "intent": "<one of: {{ValidIntents}}>",
          "filters": {
            "status": "<status value or null>",
            "priority": "<priority value or null>",
            "requestNumber": "<e.g. REQ-0001 or null>",
            "requestId": "<guid or null>"
          }
        }

        Rules:
        - If the user asks about "my requests", "my tasks", "my approvals" use the role-appropriate intent.
        - If the user mentions a request number like REQ-0001, set requestNumber in filters.
        - If the user asks for a summary or dashboard, use get_dashboard_summary.
        - If you cannot determine the intent, return "unknown".
        - NEVER return anything outside the JSON block.
        """;

        return await CallGroqAsync(systemPrompt, userMessage);
    }

    public async Task<string> FormatReplyAsync(
        string originalMessage,
        string intent,
        string dataContext,
        string userName,
        string userRole)
    {
        var systemPrompt = $"""
        You are a helpful assistant inside the EASRMS portal.

        You are talking to {userName} who has the role of {userRole}.

        Your job is to take the raw data provided and write a short,
        friendly conversational reply.

        Rules:
        - Keep reply under 6 lines.
        - Do not invent data.
        - If no requests found, respond politely.
        - You CANNOT create new requests or perform any actions. You can ONLY fetch existing data.
        - If the user asks to create a request, tell them you cannot do that and they should use the "New Request" button in the application UI.
        - If intent is unknown, suggest valid queries based ONLY on the data you can fetch.
        Rules:
        - Keep reply under 6 lines.
        - Do not invent data.
        - If no requests found, respond politely.
        - You CANNOT create new requests or perform any actions. You can ONLY fetch existing data.
        - If the user asks to create a request, tell them you cannot do that and they should use the "New Request" button in the application UI.
        - If intent is unknown, suggest valid queries based ONLY on the data you can fetch.

        """;

        var userPrompt = $"""
        User asked: "{originalMessage}"

        Intent detected: {intent}

        Data from system:
        {(string.IsNullOrWhiteSpace(dataContext)
            ? "No data available."
            : dataContext)}
        """;
        return await CallGroqAsync(systemPrompt, userPrompt);
    }

    //private async Task<string> CallGeminiAsync(
    //    string systemPrompt,
    //    string userMessage)
    //{
    //    var fullPrompt = $"""
    //    {systemPrompt}

    //    User Message:
    //    {userMessage}
    //    """;

    //    var requestBody = new
    //    {
    //        contents = new[]
    //        {
    //            new
    //            {
    //                parts = new[]
    //                {
    //                    new
    //                    {
    //                        text = fullPrompt
    //                    }
    //                }
    //            }
    //        }
    //    };

    //    var json = JsonSerializer.Serialize(requestBody);

    //    var content = new StringContent(
    //        json,
    //        Encoding.UTF8,
    //        "application/json");

    //    var response = await _httpClient.PostAsync(
    //        $"v1beta/models/{_model}:generateContent?key={_apiKey}",
    //        content);

    //    response.EnsureSuccessStatusCode();

    //    var responseJson =
    //        await response.Content.ReadAsStringAsync();

    //    using var doc = JsonDocument.Parse(responseJson);

    //    var text = doc.RootElement
    //        .GetProperty("candidates")[0]
    //        .GetProperty("content")
    //        .GetProperty("parts")[0]
    //        .GetProperty("text")
    //        .GetString();

    //    return text?.Trim() ?? string.Empty;
    //}
    private async Task<string> CallGroqAsync(
    string systemPrompt,
    string userMessage)
    {
        try
        {
            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                new
                {
                    role = "system",
                    content = systemPrompt
                },
                new
                {
                    role = "user",
                    content = userMessage
                }
            },
                temperature = 0.3,
                max_tokens = 1024
            };

            var json = JsonSerializer.Serialize(requestBody);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                "chat/completions",
                content);

            var responseText =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Groq Error:");
                Console.WriteLine(responseText);

                return "Sorry, AI service is temporarily unavailable.";
            }

            using var doc =
                JsonDocument.Parse(responseText);

            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return text?.Trim()
                   ?? "No response generated.";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            return "Sorry, something went wrong while contacting the AI service.";
        }
    }
}