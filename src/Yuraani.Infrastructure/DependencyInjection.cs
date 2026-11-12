using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
using Yuraani.Infrastructure.Identity;
namespace Yuraani.Infrastructure;
public static class DependencyInjection
{
 public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
 {
  var connection=configuration.GetConnectionString("Yuraani")??throw new InvalidOperationException("Connection string 'Yuraani' was not configured.");
  services.AddDbContext<Persistence.YuraaniDbContext>(o=>o.UseSqlServer(connection));
  services.AddHttpContextAccessor();
  services.AddScoped<IAuthRepository,AuthRepository>();services.AddScoped<ILoginContext,HttpLoginContext>();
  services.AddSingleton<ISecurePasswordService,SecurePasswordService>();services.AddSingleton<IAuthTokenService,JwtAuthTokenService>();
  services.AddScoped<IVerificationCodeSender,DevelopmentVerificationCodeSender>();services.AddSingleton(TimeProvider.System);
  return services;
 }
}