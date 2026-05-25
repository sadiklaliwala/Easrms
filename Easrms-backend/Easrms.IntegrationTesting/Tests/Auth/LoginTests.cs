using System.Net.Http;
using System.Text;
using System.Text.Json;
using Easrms.Application.DTOs.Auth;
using Easrms.IntegrationTesting.Base;
using FluentAssertions;
using Xunit;

namespace Easrms.IntegrationTesting.Tests.Auth;

public class LoginTests : IntegrationTestBase
{
    [Fact]
    public async Task Should_Login_When_CredentialsAreValid()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "arjun.mehta@easrms.com", Password = "123456" };
        var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

        // Act
        var res = await Client.PostAsync("/api/auth/login", content, TestContext.Current.CancellationToken);

        // Assert
        res.IsSuccessStatusCode.Should().BeTrue();
        var body = await res.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Console.WriteLine(body);
        Console.WriteLine("status code ",res.StatusCode);
        body.Should().Contain("Login successful");
    }
}
