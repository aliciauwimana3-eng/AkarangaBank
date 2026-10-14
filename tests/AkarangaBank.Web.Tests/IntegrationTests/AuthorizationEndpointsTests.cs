using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AkarangaBank.Web.Services;

namespace AkarangaBank.Web.Tests.IntegrationTests;

public class AuthorizationEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _plain;
    private readonly WebApplicationFactory<Program> _authenticated;

    public AuthorizationEndpointsTests(CustomWebApplicationFactory factory)
    {
        _plain = factory;
        _authenticated = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ICurrentUserService>();
                services.AddScoped<ICurrentUserService, FakeCurrentUserService>();
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultForbidScheme = TestAuthHandler.SchemeName;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
            }));
    }

    private HttpClient CreateClientWithRole(string role)
    {
        var client = _authenticated.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    // TI-16.2 : un utilisateur au role client ne peut pas ouvrir /AdminUtilisateurs
    [Fact]
    public async Task AdminUtilisateurs_AvecRoleClient_Retourne403()
    {
        var client = CreateClientWithRole("client");

        var response = await client.GetAsync("https://localhost/AdminUtilisateurs");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // TI-16.3 : un administrateur peut ouvrir /AdminUtilisateurs
    [Fact]
    public async Task AdminUtilisateurs_AvecRoleAdmin_Retourne200()
    {
        var client = CreateClientWithRole("admin");

        var response = await client.GetAsync("https://localhost/AdminUtilisateurs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // TI-16.5 : POST /Culture/SetLanguage avec jeton anti-falsification -> cookie de culture
    [Fact]
    public async Task SetLanguage_PosteAvecJeton_PoseLeCookieDeCulture()
    {
        var client = _plain.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // La page de connexion contient un formulaire POST, donc un jeton anti-falsification
        var page = await client.GetAsync("https://localhost/Identity/Account/Login");
        var html = await page.Content.ReadAsStringAsync();
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"(?<t>[^\"]+)\"");
        Assert.True(match.Success, "Jeton anti-falsification introuvable dans la page de connexion.");

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["culture"] = "rn-BI",
            ["returnUrl"] = "/",
            ["__RequestVerificationToken"] = match.Groups["t"].Value
        });
        var response = await client.PostAsync("https://localhost/Culture/SetLanguage", form);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies!, c => c.Contains(".AspNetCore.Culture") && c.Contains("rn-BI"));
    }

    // Pages reservees au personnel : un client recoit 403
    [Theory]
    [InlineData("/Clients")]
    [InlineData("/Clients/Create")]
    [InlineData("/Clients/Edit/1")]
    [InlineData("/Comptes/Create")]
    [InlineData("/Comptes/Edit/1")]
    [InlineData("/Agences/Create")]
    [InlineData("/Agences/Edit/1")]
    [InlineData("/VerificationsKYC/Create?clientId=1")]
    [InlineData("/Comptabilite/Journal")]
    [InlineData("/Prets/Create")]
    [InlineData("/Cartes/Create")]
    [InlineData("/Documents/Upload?clientId=1")]
    public async Task PagesDuPersonnel_AvecRoleClient_Retournent403(string url)
    {
        var client = CreateClientWithRole("client");

        var response = await client.GetAsync("https://localhost" + url);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Pages que le client a le droit d'ouvrir : agences (lecture), ses comptes, ses operations
    [Theory]
    [InlineData("/Agences")]
    [InlineData("/Comptes")]
    [InlineData("/Operations")]
    [InlineData("/Virements")]
    [InlineData("/Prets")]
    [InlineData("/Documents")]
    public async Task PagesDuClient_AvecRoleClient_Retournent200(string url)
    {
        var client = CreateClientWithRole("client");

        var response = await client.GetAsync("https://localhost" + url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}