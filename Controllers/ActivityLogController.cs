using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TRS.Global;
using TRS.Models;

namespace TRS.Controllers
{
    public class ActivityLogController : Controller
    {
        private readonly GlobalService _globalService;
        private readonly Dictionary<string, string> auditTrail;
        public ActivityLogController(
        IHttpContextAccessor accessor,
        GlobalService globalService) {

            auditTrail = new Dictionary<string, string>{
                {"HostName", accessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString()},
                {"UserID", accessor.HttpContext?.Session?.GetString("SessionUserID")},
                {"LoggedEmployeeNo", accessor.HttpContext?.Session?.GetString("SessionEmployeeNo")}
            };
            _globalService = globalService;
        }
        public IActionResult Index()
        {
            if (auditTrail["LoggedEmployeeNo"] != "1019241")
            {
                return Redirect(Url.Action("Error", "Home"));
            }
            var _list = _globalService.GetLogs();
            return View(_list.OrderByDescending(s => s.DateCreated).ToList());
        }

        public IActionResult GetLogs([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                var _list = _globalService.GetLogs();

                DataSourceResult result = _list.OrderByDescending(s => s.DateCreated).ToDataSourceResult(request);
                var res = JsonConvert.SerializeObject(result, Formatting.None,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });

                return Content(res, "application/json");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
    }
}
