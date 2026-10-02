using Microsoft.AspNetCore.Mvc.Filters;

namespace E_CommereceAPI.Attributes
{
    public class RedisCasheAttribute : ActionFilterAttribute    
    {
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            return base.OnActionExecutionAsync(context, next);
        }

    }
}
