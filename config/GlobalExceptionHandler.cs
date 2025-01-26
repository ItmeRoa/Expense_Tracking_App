using Microsoft.AspNetCore.Diagnostics;
using Personal_finance_tracker.exception;
using ILogger = Serilog.ILogger;

namespace Personal_finance_tracker.config;

public class GlobalExceptionHandler(ILogger logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {   
            
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Access denied please contact admin."),
            EntityNotFoundException => (StatusCodes.Status404NotFound, "Entity Not found."),
            EntityAlreadyExistException => (StatusCodes.Status409Conflict, "Entity already exist."),
            RepoException => (StatusCodes.Status500InternalServerError, "External service not responding."),
            CacheException => (StatusCodes.Status404NotFound,"The resource can not be found in our cache."),
            ServiceException => (StatusCodes.Status400BadRequest, "Logic error, check the request"),
            _ => (StatusCodes.Status500InternalServerError, "An Unexpected error.")
        };

        logger.Error(exception, "Exception occurred with status code {StatusCode}: {Message}", statusCode, message);
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(new { StatusCode = statusCode, Message = message },
            cancellationToken: cancellationToken);
        return true;
    }
}