using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Yuraani.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Yuraani")
            ?? throw new InvalidOperationException("Connection string 'Yuraani' was not configured.");
        services.AddDbContext<Persistence.YuraaniDbContext>(options => options.UseSqlServer(connectionString));
        return services;
    }
}
