using E_Commerse.Sharde.CommenResult;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Presentation.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
        {
            if (result is null)
                return controller.StatusCode(StatusCodes.Status500InternalServerError);

            if (result.IsSuccess)
                return result.Value is null ? controller.NoContent() : controller.Ok(result.Value);

            var error = result.Errors.FirstOrDefault();
            if (error is null)
                return controller.StatusCode(StatusCodes.Status500InternalServerError);

            return error.Type switch
            {
                ErrorType.NotFound => controller.NotFound(error),
                ErrorType.validatio => controller.BadRequest(error),
                ErrorType.Unauthorized => controller.Unauthorized(error),
                ErrorType.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, error),
                ErrorType.InvalidCredentials => controller.StatusCode(StatusCodes.Status401Unauthorized, error),
                _ => controller.StatusCode(StatusCodes.Status500InternalServerError, error)
            };
        }

        public static IActionResult ToActionResult(this Result result, ControllerBase controller)
        {
            if (result is null)
                return controller.StatusCode(StatusCodes.Status500InternalServerError);

            if (result.IsSuccess)
                return controller.NoContent();

            var error = result.Errors.FirstOrDefault();
            if (error is null)
                return controller.StatusCode(StatusCodes.Status500InternalServerError);

            return error.Type switch
            {
                ErrorType.NotFound => controller.NotFound(error),
                ErrorType.validatio => controller.BadRequest(error),
                ErrorType.Unauthorized => controller.Unauthorized(error),
                ErrorType.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, error),
                ErrorType.InvalidCredentials => controller.StatusCode(StatusCodes.Status401Unauthorized, error),
                _ => controller.StatusCode(StatusCodes.Status500InternalServerError, error)
            };
        }
    }
}
