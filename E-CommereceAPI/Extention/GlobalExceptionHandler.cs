using E_Commerse.Sharde.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace E_CommereceAPI.Extention
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(IHostEnvironment environment)
        {
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title, detail) = MapException(exception);

            var problemDetails = ApiErrorResponse.Create(
                statusCode,
                title,
                detail,
                _environment,
                exception);

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static (int StatusCode, string Title, string Detail) MapException(Exception exception)
            => exception switch
            {
                NotFoundException ex => (
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    ex.Message),

                KeyNotFoundException ex => (
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    ex.Message),

                ArgumentException ex => (
                    StatusCodes.Status400BadRequest,
                    "Bad Request",
                    ex.Message),

                InvalidOperationException => (
                    StatusCodes.Status400BadRequest,
                    "Bad Request",
                    "Invalid operation."),

                RedisConnectionException => (
                    StatusCodes.Status503ServiceUnavailable,
                    "Service Unavailable",
                    "Redis is not available. Start Redis on localhost:6379 or set Basket:UseInMemory to true in appsettings.Development.json."),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred.")
            };
    }
}
