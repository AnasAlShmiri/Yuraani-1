using FluentValidation;
using System.Net;
using System.Text.Json;
namespace Yuraani.API.Middleware;
public sealed class ExceptionHandlingMiddleware(RequestDelegate next,ILogger<ExceptionHandlingMiddleware> logger)
{
 public async Task InvokeAsync(HttpContext context)
 {
  try{await next(context);}
  catch(Exception ex)
  {
   logger.LogError(ex,"Request failed.");
   if(context.Response.HasStarted)throw;
   var status=ex switch{ValidationException=> (int)HttpStatusCode.BadRequest,UnauthorizedAccessException=>(int)HttpStatusCode.Unauthorized,InvalidOperationException=>(int)HttpStatusCode.Conflict,_=>(int)HttpStatusCode.InternalServerError};
   context.Response.Clear();context.Response.StatusCode=status;context.Response.ContentType="application/problem+json";
   var detail=status==500?"An unexpected error occurred.":ex.Message;
   await context.Response.WriteAsync(JsonSerializer.Serialize(new{type="about:blank",title= status==400?"Validation failed":status==401?"Unauthorized":status==409?"Request conflict":"Server error",status,detail}));
  }
 }
}