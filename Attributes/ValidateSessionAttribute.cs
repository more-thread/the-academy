using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TRS.Models;
using TRS.Services;

namespace TRS.Attributes
{
    public class ValidateSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionService = context.HttpContext.RequestServices.GetService<ISessionService>();
            
            if (sessionService == null || !sessionService.IsSessionValid())
            {
                context.Result = new RedirectToActionResult("Timeout", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}