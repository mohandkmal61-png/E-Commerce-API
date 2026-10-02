using Microsoft.AspNetCore.Mvc;

namespace E_CommereceAPI.Extention
{
    public static class ApiErrorResponse
    {
        public static ProblemDetails Create(
            int statusCode,
            string title,
            string detail,
            IHostEnvironment? environment = null,
            Exception? exception = null)
        {
            return new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = environment?.IsDevelopment() == true && exception is not null
                    ? exception.ToString()
                    : detail
            };
        }
    }
}
