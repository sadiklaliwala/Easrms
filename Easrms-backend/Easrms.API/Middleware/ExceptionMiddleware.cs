using Easrms.Common.Response;
using Serilog;
using System.Net;
using System.Text.Json;

namespace Easrms.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception occurred. Path: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            KeyNotFoundException ex => new
            {
                response = ApiResponse<object>.FailResponse(ex.Message, (int)HttpStatusCode.NotFound),
                statusCode = (int)HttpStatusCode.NotFound
            },
            UnauthorizedAccessException ex => new
            {
                response = ApiResponse<object>.FailResponse(ex.Message, (int)HttpStatusCode.Unauthorized),
                statusCode = (int)HttpStatusCode.Unauthorized
            },
            InvalidOperationException ex => new
            {
                response = ApiResponse<object>.FailResponse(ex.Message, (int)HttpStatusCode.Conflict),
                statusCode = (int)HttpStatusCode.Conflict
            },
            _ => new
            {
                response = ApiResponse<object>.FailResponse("An unexpected error occurred", (int)HttpStatusCode.InternalServerError),
                statusCode = (int)HttpStatusCode.InternalServerError
            }
        };

        context.Response.StatusCode = response.statusCode;

        var json = JsonSerializer.Serialize(response.response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}