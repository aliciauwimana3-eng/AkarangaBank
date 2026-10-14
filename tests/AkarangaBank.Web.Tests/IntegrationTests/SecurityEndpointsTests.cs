using System.Net;
using System.Text.RegularExpressions;
using Xunit;

namespace AkarangaBank.Web.Tests.IntegrationTests;

public class SecurityEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SecurityEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task PageMVC_SansSession_RedigeVersLaConnexion()
    {
        var response = await _client.GetAsync("/Comptes");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task HealthLive_EstAccessibleSansSession()
    {
        var response = await _client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Prets_SansAuthentification_RedigeVersConnexion()
    {
        var response = await _client.GetAsync("/Prets");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Home_AvecCultureKirundi_AfficheLaTraduction()
    {
        var response = await _client.GetAsync("https://localhost/?culture=rn-BI");
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, $"Status: {response.StatusCode}; URL: {response.Headers.Location}; body: {body}");
        Assert.Contains("Kaze neza", body);
        Assert.Contains("lang=\"rn-BI\"", body);
        Assert.Contains("href=\"#main-content\"", body);
        Assert.Contains("id=\"main-content\"", body);
        foreach (Match image in Regex.Matches(body, "<img\\b[^>]*>", RegexOptions.IgnoreCase))
            Assert.Matches("\\balt=\\\"[^\\\"]*\\\"", image.Value);
    }
}
