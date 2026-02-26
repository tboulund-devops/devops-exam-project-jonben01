using Microsoft.AspNetCore.Diagnostics;

namespace Api;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, message) = exception switch
        {
            NotFoundException e => (StatusCodes.Status404NotFound, e.Message),
            ValidationException e => (StatusCodes.Status400BadRequest, e.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error has occurred.")
        };
        
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new {error = message}, ct);
        return true;
    }
}


public class NotFoundException(string message) : Exception(message);
public class ValidationException(string message) : Exception(message);