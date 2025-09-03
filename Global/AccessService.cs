using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using TRS.Data;
using TRS.Models;

namespace TRS.Global
{        
    public class AccessService : ActionFilterAttribute
    {
        public string ControllerName { get; set; }
        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session == null ||
                             !context.HttpContext.Session.TryGetValue("SessionUserID", out byte[] val))
            {
                context.Result =
                    new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "Home",
                        action = "Index"
                    }));
            }
            else
            { 
                //CHECK PERMISSION TO PAGE
                if (FormService.FormList.Any(w => w.Controller == ControllerName && w.EmployeeNo == context.HttpContext.Session.GetString("SessionEmployeeNo")) == false)
                {
                    context.Result =
                    new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        controller = "Home",
                        action = "AccessDenied"
                    }));
                }
            };


            base.OnActionExecuting(context);
        }

    }
}