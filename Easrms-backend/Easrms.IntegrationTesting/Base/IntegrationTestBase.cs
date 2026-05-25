using Easrms.IntegrationTesting.Factory;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;

namespace Easrms.IntegrationTesting.Base;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected HttpClient Client { get; private set; } = null!;

    protected IntegrationTestBase()
    {
        Factory = new CustomWebApplicationFactory();
    }

    public ValueTask InitializeAsync()
    {
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return ValueTask.CompletedTask;
    }
}
