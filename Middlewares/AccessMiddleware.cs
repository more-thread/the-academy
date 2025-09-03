using Microsoft.AspNetCore.Http;
using TRS.Data;
using System.Linq;
using System.Threading.Tasks;
using TRS.Models;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace TRS.Global
{
    public class AccessMiddleware
    {
        private readonly RequestDelegate _next;

        public AccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Retrieve controller name from the request path
            var controllerName = context.Request.Path.Value?.Split('/')[2];
            var actionName = context.Request.Path.Value.ToString();

            // Skip these paths
            if (controllerName == "" ||
                (controllerName == "Home" && (
                    actionName.ToLower() == "/home/setsession" ||
                    actionName.ToLower() == "/home/error" ||
                    actionName.ToLower() == "/home/sessionexpired" ||
                    actionName.ToLower() == "/home/accessdenied" ||
                    actionName.ToLower() == "/home/redirecttosessionexpired"
                ))
                )
            {
                await _next(context);
                return;
            }

            // Validate session
            var session = context.Session;
            var employeeNo = session?.GetString("SessionEmployeeNo");

            // Check access permissions
            if (string.IsNullOrEmpty(employeeNo) ||
                !FormService.FormList.Any(w => w.Controller.ToLower() == controllerName.ToLower() && w.EmployeeNo == employeeNo))
            {
                // Redirect to AccessDenied
                context.Response.Redirect("EEMS/Home/AccessDenied");
                return;
            }

            // Continue to next middleware if access is valid
            await _next(context);
        }
    }
}
