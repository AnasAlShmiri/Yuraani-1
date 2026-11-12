using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Yuraani.UnitTests.Integration;

public sealed class ProfessionalApprovalApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProfessionalApprovalApiTests(WebApplicationFactory<Program> factory)
    {
        var app = factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Yuraani"] = "Server=localhost;Database=Yuraani_Integration;User Id=test;Password=NotARealPassword123!;TrustServerCertificate=True",
                    ["Jwt:Secret"] = "integration-test-secret-key-with-32-bytes-minimum",
                    ["Jwt:Issuer"] = "Yuraani.IntegrationTests",
                    ["Jwt:Audience"] = "Yuraani.IntegrationTests"
                })));
        _client = app.CreateClient();
    }

    [Theory]
    [InlineData("/api/professional-approvals/pending")]
    [InlineData("/api/professional-approvals/mine")]
    public async Task Protected_get_endpoints_reject_anonymous_requests(string path)
    {
        using var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Approval_decision_rejects_anonymous_request()
    {
        using var response = await _client.PostAsync(
            $"/api/professional-approvals/{Guid.NewGuid()}/decision",
            new StringContent("{\"approved\":true}", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
