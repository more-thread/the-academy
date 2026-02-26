using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Packaging.Licenses;
using System.Diagnostics;
using TRS.Attributes;
using TRS.Global;
using TRS.Interfaces;
using TRS.Models;
using TRS.ViewModels;

namespace TRS.Controllers
{
    [ValidateSession]
    public class TrainingCoordinatorController : Controller
    {
        private readonly ILogger<TrainingCoordinatorController> _logger;
        private readonly ITrainingCoordinatorService _trainingCoordinatorService;
        private readonly GlobalService _globalService;
        private readonly Dictionary<string,string> auditTrail;
        public TrainingCoordinatorController(ILogger<TrainingCoordinatorController> logger,
        ITrainingCoordinatorService trainingCoordinatorService,
        IHttpContextAccessor accessor,    
        GlobalService globalService
        )
        {            
            
            auditTrail = new Dictionary<string,string>{
                {"HostName", accessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString()},
                {"UserID", accessor.HttpContext?.Session?.GetString("SessionUserID")},
                {"LoggedEmployeeNo", accessor.HttpContext?.Session?.GetString("SessionEmployeeNo")}
            };
            _globalService = globalService;
            _logger = logger;
            _trainingCoordinatorService = trainingCoordinatorService;            
        }

        [ValidateAccess(ControllerName = "TrainingCoordinator")]
        public IActionResult Index()
        {
            _globalService.PageVisitLog($"{RouteData.Values["controller"]}/{RouteData.Values["action"]}",auditTrail);
            return View(new TrainingCoordinatorViewModel());
        }
        
        [HttpGet]
        public async Task<IActionResult> GetTrainingCoordinatorDetails(string paramEmployeeNo)
        {
            try
            {                               
                TrainingCoordinator trainingCoordinatorDetails = await _trainingCoordinatorService.GetTrainingCoordinatorByEmployeeNo(paramEmployeeNo);
                TrainingCoordinatorViewModel trainingCoordinatorViewModel = new TrainingCoordinatorViewModel(){
                    TrainingCoordinatorDetails = trainingCoordinatorDetails
                };
                
                return PartialView("~/Views/TrainingCoordinator/_TrainingCoordinatorDetails.cshtml", trainingCoordinatorViewModel);
            }
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest();
                throw;
            }
        }        

        public async Task<IActionResult> GetTrainingCoordinatorList([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                var _list = await _trainingCoordinatorService.GetTrainingCoordinatorList();

                DataSourceResult result = _list.ToDataSourceResult(request);
                var res = JsonConvert.SerializeObject(result, Formatting.None,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });
                return Content(res, "application/json");
            }
            catch (Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest();
                throw;
            }
            
        }

        [HttpPost]
        public ActionResult AddTrainingCoordinator(TrainingCoordinator model)
        {
            try
            {   
                model.CreatedBy = auditTrail["UserID"];
                model.CreatedByComputerUsed = auditTrail["HostName"];
                model.DateCreated = _globalService.GetDateTime();
                model.Status = true;

                // Add the new record into the database
                _trainingCoordinatorService.AddTrainingCoordinator(model);
 
                _globalService.Log("Add new coordinator: TrainingCoordinator - " + JsonConvert.SerializeObject(model, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), auditTrail, null);
            }
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }

            // Return success response
            return Ok();
        }
        [HttpPost]
        public async Task<ActionResult> SetTrainingCoordinatorStatus(string paramEmployeeNo, string paramStatus)
        {
            try
            {
                TrainingCoordinator _details = await _trainingCoordinatorService.GetTrainingCoordinatorByEmployeeNo(paramEmployeeNo);

                _details.Status =  paramStatus == "Active"? true: false;
                _details.ModifiedBy = auditTrail["UserID"];
                _details.ModifiedByComputerUsed = auditTrail["HostName"];
                _details.DateModified = _globalService.GetDateTime();
                _globalService.Log($"Set the Status to {paramStatus}: TrainingCoordinator ({paramEmployeeNo})",auditTrail,null);                              
                // update the record
                _trainingCoordinatorService.UpdateStatus();

            }            
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }

            // Return success response
            return Ok();
        }
        

        public async Task<JsonResult> GetActiveEmployeeList()
        {
            List<VwHrEmployeeInfo> _list =  new()
            {
                new VwHrEmployeeInfo
                {
                    EmployeeNo = "",
                    EmployeeName = "",
                }
            };
            
            var abc = await _trainingCoordinatorService.GetActiveEmployeeList();

            _list.AddRange(abc.OrderBy(s=>s.EmployeeName));            

            return Json(_list);
        }

        [HttpGet]
        public async Task<ActionResult> GetEmployeePositionName(string paramEmployeeNo)
        {
            try
            {
                var _result = await  _trainingCoordinatorService.GetActiveEmployeeByEmployeeNo(paramEmployeeNo);                            
                return Ok(new {PositionName = _result == null ? "": _result.PositionName });     
            }
            catch (System.Exception ex)
            {

                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest();
                throw;
            }
            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}