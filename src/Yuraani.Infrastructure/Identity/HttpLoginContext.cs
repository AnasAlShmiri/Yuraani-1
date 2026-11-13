using Microsoft.AspNetCore.Http;
using Yuraani.Application.Features.Identity.Auth.Abstractions;
namespace Yuraani.Infrastructure.Identity;
public sealed class HttpLoginContext(IHttpContextAccessor accessor):ILoginContext
{
 public string? IpAddress=>accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
 public string? UserAgent=>accessor.HttpContext?.Request.Headers.UserAgent.ToString();
}