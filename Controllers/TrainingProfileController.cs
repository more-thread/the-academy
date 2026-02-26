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
    public class TrainingProfileController : Controller
    {
        private readonly ILogger<TrainingProfileController> _logger;
        private readonly ITrainingRegistrationService _trainingRegistrationService;
        private readonly ITrainingScheduleService _trainingScheduleService;
        private readonly ITrainingCourseService _trainingCourseService;
        private readonly GlobalService _globalService;
        private readonly Dictionary<string,string> auditTrail;
        public TrainingProfileController(ILogger<TrainingProfileController> logger,
        ITrainingRegistrationService trainingRegistrationService,
        ITrainingScheduleService trainingScheduleService,
        ITrainingCourseService trainingCourseService,
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
            _trainingRegistrationService = trainingRegistrationService;
            _trainingScheduleService = trainingScheduleService;
            _trainingCourseService = trainingCourseService;
        }
        [ValidateAccess(ControllerName = "TrainingProfile")]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<TrainingRegistration> _list = await _trainingRegistrationService.GetTrainingListByEmployeeNo(auditTrail["LoggedEmployeeNo"]);

                var totalHours = 0.0;
                var totalDays = 0.0;

                foreach (var trainingRegistration in _list.Where(w => w.TrainingSchedule.ScheduleStatus == "COMPLETED"
                && w.TrainingCompletionStatus == "COMPLETE"
                && w.TrainingRegistrationStatus == "REGISTERED"
                && w.TrainingSchedule.TrainingCode.Substring(13, 4) == _globalService.GetDateTime().Year.ToString()))
                {
                    var trainingSchedule = trainingRegistration.TrainingSchedule;
                    var startDate = trainingSchedule.StartDate;
                    var endDate = trainingSchedule.EndDate;
                    var startTime = trainingSchedule.StartTime;
                    var endTime = trainingSchedule.EndTime;

                    var startDateTime = startDate + startTime;
                    var endDateTime = endDate + endTime;

                    var duration = endDateTime - startDateTime;
                    
                    totalHours += duration.TotalHours;
                }

                totalDays = totalHours / 8;

                var totalCoursesTaken = _list.Where(w => w.TrainingSchedule.ScheduleStatus == "COMPLETED"
                && w.TrainingCompletionStatus == "COMPLETE"
                && w.TrainingSchedule.TrainingCode.Substring(13, 4) == _globalService.GetDateTime().Year.ToString()).Count();

                _globalService.PageVisitLog($"{RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail);

                TrainingProfileViewModel vm = new TrainingProfileViewModel()
                {
                    EmployeeProfile = _globalService.GetHREmployeeInfoByEmployeeNo(auditTrail["LoggedEmployeeNo"]),
                    TotalManDays = Math.Round(totalDays, 2),
                    CoursesTaken = totalCoursesTaken
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, null);
                return BadRequest(ex.Message);
                throw;
            }
           
        }

        public async Task<IActionResult> GetTrainingListByEmployeeNo([DataSourceRequest] DataSourceRequest request,
        string paramProgramCode,
        string paramCourseCode,
        string paramEmployeeNo = null)
        {
            try
            {
                
                List<TrainingRegistration> _list = await _trainingRegistrationService.GetTrainingListByEmployeeNo(paramEmployeeNo??auditTrail["LoggedEmployeeNo"]);
                

                if (paramProgramCode != null && paramProgramCode != "ALL")
                    _list = _list.Where(w => w.TrainingSchedule.Program.ProgramCode == paramProgramCode).ToList();

                if (paramCourseCode != null && paramCourseCode != "ALL")
                    _list = _list.Where(w => w.TrainingSchedule.Course.CourseCode == paramCourseCode).ToList();

                DataSourceResult result = _list.Where(w=>w.TrainingSchedule.ScheduleStatus == "COMPLETED" && w.TrainingRegistrationStatus == "REGISTERED").OrderBy(s => s.DateCreated).ToDataSourceResult(request);
                var res = JsonConvert.SerializeObject(result, Formatting.None,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });

                return Content(res, "application/json");
            }
            catch (Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, null);
                return BadRequest(ex.Message);
                throw;
            }
        }
        
        
        public async Task<JsonResult> GetSubordinateList()
        {
            List<VwHrEmployeeInfo> _list = _globalService.GetEmployeeList().Where(w=>w.SuperiorId.ToString() == auditTrail["LoggedEmployeeNo"] && w.EmployeeStatus == "A").ToList();          

            return Json(_list.OrderBy(s => s.EmployeeName));
        }

        
        public async Task<IActionResult> GetRecommendedCourseList([DataSourceRequest] DataSourceRequest request,string paramEmployeeNo = null)
        {
            try
            {
                
                List<TrainingRegistration> _registrationlist = await _trainingRegistrationService.GetTrainingRegistrationList();       
                List<TrainingCourse> _list = await _trainingCourseService.GetTrainingCourseList();

                List<TrainingRegistration> _recommendedList = new();

                
                VwHrEmployeeInfo _empDetails = _globalService.GetHREmployeeInfoByEmployeeNo(paramEmployeeNo??auditTrail["LoggedEmployeeNo"]);

                _list = _list.Where(w => w.Status && w.Program.JobClasses.Any(match => match.JobClassCode == _empDetails.JobClassCode)).ToList();

                foreach (var item in _list)
                {
                    
                    var trainingDetails = _registrationlist.Where(w => w.TrainingSchedule.Course.CourseCode == item.CourseCode && w.EmployeeNo == _empDetails.EmployeeNo).FirstOrDefault();
                    var TrainingCompletionStatus = "";

                    if(trainingDetails != null)
                        TrainingCompletionStatus = trainingDetails.CourseCompletionStatus;
                    else
                        TrainingCompletionStatus = null;
                    
                    _recommendedList.Add( new TrainingRegistration{
                         TrainingSchedule = new TrainingSchedule(){
                            Course = item,
                            Program = item.Program
                         },
                         CourseCompletionStatus =  TrainingCompletionStatus??"RECOMMENDED"
                    });
                }

                DataSourceResult result = _recommendedList.OrderBy(s => s.DateCreated).ToDataSourceResult(request);
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
                return BadRequest(ex.Message);
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