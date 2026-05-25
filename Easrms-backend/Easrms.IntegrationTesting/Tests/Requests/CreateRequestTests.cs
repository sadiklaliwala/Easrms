using System.Net.Http;
using System.Text;
using System.Text.Json;
using Easrms.Application.DTOs.Request;
using Easrms.IntegrationTesting.Base;
using Easrms.IntegrationTesting.Helpers;
using FluentAssertions;
using Xunit;

namespace Easrms.IntegrationTesting.Tests.Requests;

public class CreateRequestTests : IntegrationTestBase
{
    [Fact]
    public async Task Should_CreateRequest_When_ValidPayload()
    {
        // Arrange - client is already authenticated via TestAuthHandler (Admin)
        var createDto = new CreateRequestDto
        {
            CategoryId = Guid.Parse("E1000000-0000-0000-0000-000000000001"),
            Title = "Integration Test Request",
            Description = "Test",
            Priority = Easrms.Common.Enums.PriorityEnums.Medium
        };

        var content = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");

        // Act
        var res = await Client.PostAsync("/api/request", content, TestContext.Current.CancellationToken);

        // Assert
        res.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var body = await res.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        body.Should().Contain("Request created successfully");
    }
}
