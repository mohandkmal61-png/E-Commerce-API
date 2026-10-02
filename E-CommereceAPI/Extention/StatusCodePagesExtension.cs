using Microsoft.AspNetCore.Mvc;

namespace E_CommereceAPI.Extention
{
    public static class StatusCodePagesExtension
    {
        public static IApplicationBuilder UseCustomStatusCodePages(this IApplicationBuilder app)
        {
            return app.UseStatusCodePages(async context =>
            {
                var response = context.HttpContext.Response;

                if (response.HasStarted)
                    return;

                response.ContentType = "application/problem+json";

                var problemDetails = response.StatusCode switch
                {
                    StatusCodes.Status404NotFound => ApiErrorResponse.Create(
                        StatusCodes.Status404NotFound,
                        "Not Found",
                        $"The path '{context.HttpContext.Request.Path}' was not found."),

                    StatusCodes.Status500InternalServerError => ApiErrorResponse.Create(
                        StatusCodes.Status500InternalServerError,
                        "Internal Server Error",
                        "An unexpected error occurred."),

                    _ => (ProblemDetails?)null
                };

                if (problemDetails is not null)
                    await response.WriteAsJsonAsync(problemDetails);
            });
        }
    }
}
