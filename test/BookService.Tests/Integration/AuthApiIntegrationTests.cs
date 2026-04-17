using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BookService.Tests.Integration;

/// <summary>
/// Prueba de integración con <see cref="WebApplicationFactory{TEntryPoint}"/> (pipeline HTTP real).
/// </summary>
public sealed class AuthApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Get_authors_sin_JWT_devuelve_401()
    {
        var response = await _client.GetAsync(new Uri("/api/authors", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
