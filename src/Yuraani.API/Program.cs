using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yuraani.API.Middleware;
using Yuraani.Application;
using Yuraani.Infrastructure;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
var jwt=builder.Configuration.GetSection("Jwt");var secret=jwt["Secret"];var issuer=jwt["Issuer"];var audience=jwt["Audience"];
if(!string.IsNullOrWhiteSpace(secret)&&Encoding.UTF8.GetByteCount(secret)>=32&&!string.IsNullOrWhiteSpace(issuer)&&!string.IsNullOrWhiteSpace(audience))
 builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>o.TokenValidationParameters=new TokenValidationParameters{ValidateIssuer=true,ValidIssuer=issuer,ValidateAudience=true,ValidAudience=audience,ValidateIssuerSigningKey=true,IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),ValidateLifetime=true,ClockSkew=TimeSpan.FromSeconds(30)});
else builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
var app=builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
if(app.Environment.IsDevelopment()){app.UseSwagger();app.UseSwaggerUI();}
app.UseHttpsRedirection();app.UseAuthentication();app.UseAuthorization();app.MapControllers();app.Run();
public partial class Program { }