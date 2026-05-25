using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Easrms.Application.DTOs.Auth;

namespace Easrms.IntegrationTesting.Helpers;

public static class TestAuthHelper
{
    public static async Task<string> LoginAndGetJwtAsync(HttpClient client, string email, string password)
    {
        var dto = new LoginRequestDto { Email = email, Password = password };
        var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
        var res = await client.PostAsync("/api/auth/login", content);
        res.EnsureSuccessStatusCode();

        var body = await res.Content.ReadAsStringAsync();
        // crude parse — real projects should use proper DTO
        if (body.Contains("accessToken"))
        {
            // return whole body for now
            return body;
        }

        return string.Empty;
    }

    public static void AddBearer(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
