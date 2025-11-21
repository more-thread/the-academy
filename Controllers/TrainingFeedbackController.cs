using Humanizer;
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
    public class TrainingFeedbackController : Controller
    {
        private readonly ILogger<TrainingFeedbackController> _logger;
        private readonly ITrainingRegistrationService _trainingRegistrationService;
        private readonly ITrainingFeedbackService _trainingFeedbackService;
        private readonly ITrainingScheduleService _trainingScheduleService;
        
        private readonly GlobalService _globalService;
        private readonly Dictionary<string,string> auditTrail;

        public TrainingFeedbackController(ILogger<TrainingFeedbackController> logger,
        ITrainingRegistrationService trainingRegistrationService,
        ITrainingFeedbackService trainingFeedbackService,
        ITrainingScheduleService trainingScheduleService,
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
            _trainingFeedbackService = trainingFeedbackService;
            _trainingScheduleService = trainingScheduleService;
        }
        [ValidateAccess(ControllerName = "TrainingFeedback")]
        public IActionResult Index()
        {

            _globalService.PageVisitLog($"{RouteData.Values["controller"]}/{RouteData.Values["action"]}",auditTrail);
            return View(new TrainingFeedbackViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainingFeedbackDetails(string control,string code)
        {
            try
            {    
                FormControlModel formControlModel = new FormControlModel(control);
                
                
                TrainingRegistration _details = await _trainingRegistrationService.GetTrainingRegistrationByCode(code);

                List<TrainingFeedbackQuestions> listofquestions = await _trainingFeedbackService.GetTrainingFeedbackQuestionsList();

                List<TrainingFeedback> trainingFeedbacks = await _trainingFeedbackService.GetTrainingFeedbackByRegistrationCode(code);

                TrainingFeedbackViewModel trainingScheduleViewModel = new TrainingFeedbackViewModel(){
                    TrainingFeedbackList = trainingFeedbacks,
                    TrainingRegistrationDetails = _details,
                    TrainingFeedbackQuestions = listofquestions
                };
                
                return PartialView("~/Views/TrainingFeedback/_TrainingFeedbackDetails.cshtml",trainingScheduleViewModel);
            }
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest();
                throw;
            }
        }      
        
        [HttpPost]
        public async Task<ActionResult> SubmitFeedback(List<TrainingFeedback> model, string paramRegistrationCode)
        {
            try
            {
                
                TrainingRegistration _details = await _trainingRegistrationService.GetTrainingRegistrationByCode(paramRegistrationCode);
                
                _details.TrainingFeedbackStatus = "COMPLETE";
                _trainingRegistrationService.UpdateRegistration();

                foreach (var item in model)
                {   
                    item.CreatedBy = auditTrail["UserID"];
                    item.CreatedByComputerUsed = auditTrail["HostName"];
                    item.DateCreated = _globalService.GetDateTime();
                    item.Status = true;
                    item.TrainingRegistration = _details;
                    item.TrainingFeedbackQuestions = await _trainingFeedbackService.GetTrainingFeedbackQuestionsByID(item.TrainingFeedbackQuestions.QuestionID);
                    
                    // Add the new record to the database
                    _trainingFeedbackService.AddFeeback(item);
                }
                
                _globalService.Log($"Feedback submitted: TrainingFeedback ({_details.RegistrationCode})", auditTrail, null);

                
            }
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
                
                throw;
            }

            return Ok("");
        }
        public async Task<IActionResult> GetTrainingRegistrationList([DataSourceRequest] DataSourceRequest request,
        string paramProgramCode,
        string paramCourseCode,
        string paramRegionCode,
        string paramTrainingType,
        string paramTrainingCode
        )
        {

            try
            {
                
                List<TrainingRegistration> _list = await _trainingRegistrationService.GetTrainingRegistrationList();


                if (paramProgramCode != null && paramProgramCode != "ALL")
                    _list = _list.Where(w => w.TrainingSchedule.Program.ProgramCode == paramProgramCode).ToList();

                if (paramCourseCode != null && paramCourseCode != "ALL")
                    _list = _list.Where(w => w.TrainingSchedule.Course.CourseCode == paramCourseCode).ToList();
                    
                if (paramRegionCode != null && paramRegionCode != "ALL")
                    _list = _list.Where(w => w.TrainingSchedule.Region == paramRegionCode).ToList();
                    
                if (paramTrainingType != null && paramTrainingType != "ALL")
                    _list = _list.Where(w => w.TrainingSchedule.TrainingType == paramTrainingType).ToList();
                    
                if (paramTrainingCode != null && paramTrainingCode != "ALL")
                    _list = _list.Where(w => w.TrainingSchedule.TrainingCode == paramTrainingCode).ToList();

                DataSourceResult result = _list
                                        .Where(w => w.TrainingRegistrationStatus == "REGISTERED" 
                                        && w.TrainingSchedule.ScheduleStatus == "COMPLETED")
                                        .OrderByDescending(s => s.DateCreated).ToDataSourceResult(request);

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


        [HttpGet]
        public async Task<JsonResult> GetTrainingCodeList()
        {
             List<TrainingSchedule> _list = new()
            {
                new TrainingSchedule
                {
                    TrainingCode = "",
                }
            };
            _list.AddRange(await _trainingScheduleService.GetTrainingScheduleList());
        
            return Json(_list);
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