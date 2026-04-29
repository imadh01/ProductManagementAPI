using System.Net;
using System.Text.Json;
using ProductManagement.API.Models.Responses;

namespace ProductManagement.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex,
                "********************************************\n" +
                "ExceptionHandlingMiddleware :: Unhandled exception\n" +
                "Method: {Method} | Path: {Path}\n" +
                "********************************************",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var errorResponse = new ErrorResponse
        {
            Errors = new List<ErrorDetail>
            {
                new()
                {
                    Type = "INTERNAL_ERROR",
                    Code = "9999999",
                    Message = "An unexpected error occurred. Please try again later."
                }
            }
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsJsonAsync(errorResponse, options);
    }
}