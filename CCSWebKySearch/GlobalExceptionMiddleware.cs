using System.Net;
using CCSWebKySearch.Exceptions;
using System.Text.Json;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Serilog.ILogger _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, Serilog.ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        HttpStatusCode status;
        string message;

        switch (exception)
        {
            case InvalidInputException _:
                status = HttpStatusCode.BadRequest;
                message = exception.Message;
                _logger.Warning(exception, "Invalid input: {Message}", exception.Message);
                break;
            case NotFoundException _:
                status = HttpStatusCode.NotFound;
                message = exception.Message;
                _logger.Warning(exception, "Not found: {Message}", exception.Message);
                break;
            case BadHttpRequestException _:
                status = HttpStatusCode.BadRequest;
                message = "Invalid request data.";
                _logger.Warning(exception, "Bad request: {Message}", exception.Message);
                break;

            case FileNotFoundException _:
                status = HttpStatusCode.BadRequest;
                message = "Invalid request data, not file found.";
                _logger.Warning(exception, "Bad request: {Message}", exception.Message);
                break;
                
            default:
                status = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred.";
                _logger.Error(exception, "Unexpected error: {Message}", exception.Message);
                break;
        }

        context.Response.StatusCode = (int)status;
        var result = JsonSerializer.Serialize(new { error = message });
        return context.Response.WriteAsync(result);
    }
}
