using Microsoft.AspNetCore.Mvc.Controllers;

namespace TRS.Middlewares
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var session = context.Session;
            var employeeNo = session.GetString("SessionEmployeeNo");

            var url = context.Request.Path.Value;
            url = url.ToLower();

            //skip these. no need session
            if (url == "/" || 
                url == "/home/setsession" ||
                url == "/home/error" || 
                url == "/home/sessionexpired" ||                 
                url == "/home/redirecttosessionexpired" ||
                url == "/home/accessdenied"
                )
            {
                await _next(context);
                return;
            }

            if (string.IsNullOrEmpty(employeeNo))
            {
                context.Response.Redirect("EEMS/Home/RedirectToSessionExpired");
                return;
            }

            await _next(context);
        }
    }
}
