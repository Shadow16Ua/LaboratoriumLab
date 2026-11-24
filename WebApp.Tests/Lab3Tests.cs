using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WebApp.Tests;

public class Lab3Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public Lab3Tests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task BooksByYear_ValidYear_ReturnsOkWithFilteredBooks()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/books/year/1961");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Solaris", html);
        Assert.DoesNotContain("Cyberiada", html);
        Assert.True(response.Headers.Contains("X-Elapsed-Ms"));
        Assert.True(response.Headers.Contains("X-Request-Count"));
        Assert.True(response.Headers.Contains("X-Action-Ms"));
    }

    [Theory]
    [InlineData("/books/year/abc")]
    [InlineData("/books/year/1200")]
    [InlineData("/books/isbn/123")]
    [InlineData("/archive/1961/13")]
    [InlineData("/nie-ma-takiej-strony")]
    [InlineData("/Book/Details/9999")]
    public async Task InvalidOrMissingRoutes_ReturnCustom404Page(string url)
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Nie znaleziono strony lub zasobu.", html);
    }

    [Fact]
    public async Task BooksByIsbn_ExistingIsbn_RedirectsToDetails()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync("/books/isbn/9788308065341");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Book/Details/1", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Archive_ValidYearAndMonth_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/archive/1965/05");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cyberiada", html);
    }

    [Fact]
    public async Task Docs_CatchAll_CapturesFullPath()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/docs/a/b/c");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("a/b/c", html);
    }

    [Fact]
    public async Task Blokada_ShortCircuitsWith403()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/blokada");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BookBoom_HandledByGlobalExceptionFilter_Returns500ErrorView()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Book/Boom");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("500", html);
    }

    [Fact]
    public async Task AdminArea_DashboardAndAuthors_ReturnOkWithAdminLayout()
    {
        var client = _factory.CreateClient();

        var dashboard = await client.GetAsync("/Admin");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
        var dashboardHtml = await dashboard.Content.ReadAsStringAsync();
        Assert.Contains("Panel administracyjny", dashboardHtml);

        var authors = await client.GetAsync("/Admin/Authors");
        Assert.Equal(HttpStatusCode.OK, authors.StatusCode);
        var authorsHtml = WebUtility.HtmlDecode(await authors.Content.ReadAsStringAsync());
        Assert.Contains("Stanisław Lem", authorsHtml);
    }

    [Fact]
    public async Task HomeRoutes_ReturnsRouteTable()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/Home/Routes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("/Book/Details/{id:int}", html);
    }
}
