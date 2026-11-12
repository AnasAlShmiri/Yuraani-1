using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Yuraani.Infrastructure.Persistence;
using Xunit;

namespace Yuraani.IntegrationTests;

public sealed class ProfessionalApprovalAuthorizationTests : IClassFixture<YuraaniApiFactory>
{
    private readonly HttpClient _client;
    private readonly YuraaniApiFactory _factory;

    public ProfessionalApprovalAuthorizationTests(YuraaniApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Approval_submission_requires_authentication()
    {
        using var response = await _client.PostAsJsonAsync("/api/professional-approvals",
            new { professionalType = 1 });
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Pending_approvals_requires_explicit_review_permission()
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _factory.CreateToken(Guid.NewGuid(), "Patient"));
        using var response = await _client.GetAsync("/api/professional-approvals/pending");
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }
}

public sealed class YuraaniApiFactory : WebApplicationFactory<Program>
{
    private const string Secret = "integration-test-secret-key-with-at-least-32-bytes";
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        // Use host settings so the minimal-hosting Program receives test configuration
        // before its startup-time JWT validation executes.
        builder.UseSetting("Jwt:Secret", Secret);
        builder.UseSetting("Jwt:Issuer", "Yuraani.IntegrationTests");
        builder.UseSetting("Jwt:Audience", "Yuraani.IntegrationTests");
        builder.UseSetting("ConnectionStrings:Yuraani", "Data Source=:memory:");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<YuraaniDbContext>();
            services.RemoveAll<DbContextOptions<YuraaniDbContext>>();
            services.AddSingleton(_connection);
            services.AddDbContext<YuraaniDbContext>((provider, options) =>
                options.UseSqlite(provider.GetRequiredService<SqliteConnection>()));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<YuraaniDbContext>().Database.EnsureCreated();
        return host;
    }

    public string CreateToken(Guid userId, string role)
    {
        var now = DateTime.UtcNow;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var token = new JwtSecurityToken("Yuraani.IntegrationTests", "Yuraani.IntegrationTests", claims,
            now.AddMinutes(-1), now.AddMinutes(5),
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}
