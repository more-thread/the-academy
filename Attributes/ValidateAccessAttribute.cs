using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TRS.Models;
using TRS.Services;

namespace TRS.Attributes
{
    public class ValidateAccessAttribute : ActionFilterAttribute
    {
        public string ControllerName { get; set; }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionService = context.HttpContext.RequestServices.GetService<ISessionService>();
            
            if (sessionService == null || !sessionService.IsSessionValid())
            {
                context.Result = new RedirectToActionResult("Timeout", "Home", null);
                return;
            }
            else
            {

                //CHECK PERMISSION TO PAGE
                if (FormService.FormList.Any(w => w.Controller == ControllerName && w.EmployeeNo == sessionService.GetCurrentEmployeeNo()) == false)
                {
                    context.Result =
                    new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "Home",
                        action = "AccessDenied"
                    }));
                }
            }

            base.OnActionExecuting(context);
        }
    }
}