using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ProjectManagement.BusinessLogic.Exceptions;
using ProjectManagement.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ProjectManagement.CustomExceptionMiddleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _logger = logger;
            _next = next;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (MemberAlreadyExistsException maeEx)
            {
                _logger.LogError($"A new violation exception has been thrown: {maeEx}");
                httpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                await HandleExceptionAsync(httpContext, maeEx);
            }
            catch (ObjectNotFoundException onfEx)
            {
                _logger.LogError($"A new violation exception has been thrown: {onfEx}");
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await HandleExceptionAsync(httpContext, onfEx);
            }
            catch (AccessViolationException avEx)
            {
                _logger.LogError($"A new violation exception has been thrown: {avEx}");
                httpContext.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
                await HandleExceptionAsync(httpContext, avEx);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message
            }.ToString()); 
        }
    }
}
