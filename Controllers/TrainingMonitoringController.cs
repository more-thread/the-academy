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
    public class TrainingMonitoringController : Controller
    {
        private readonly ILogger<TrainingMonitoringController> _logger;
        private readonly ITrainingRegistrationService _trainingRegistrationService;
        private readonly ITrainingScheduleService _trainingScheduleService;
        private readonly ITrainingCoordinatorService _trainingCoordinatorService;
        private readonly IJobClassService _jobclassService;
        private readonly GlobalService _globalService;
        private readonly Dictionary<string,string> auditTrail;
        public TrainingMonitoringController(ILogger<TrainingMonitoringController> logger,
        ITrainingRegistrationService trainingRegistrationService,
        IJobClassService jobclassService,
        ITrainingScheduleService trainingScheduleService,
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
            _jobclassService = jobclassService;   
            _trainingScheduleService = trainingScheduleService;
            _trainingCoordinatorService = trainingCoordinatorService;
            _trainingRegistrationService = trainingRegistrationService;
        }
        [ValidateAccess(ControllerName = "TrainingMonitoring")]
        public IActionResult Index()
        {
            _globalService.PageVisitLog($"{RouteData.Values["controller"]}/{RouteData.Values["action"]}",auditTrail);
            return View(new TrainingScheduleViewModel());
        }

        public async Task<IActionResult> GetTrainingScheduleList([DataSourceRequest] DataSourceRequest request,
        string[] paramProgramCode,
        string[] paramCourseCode,
        string paramRegionCode,
        string paramTrainingType
        )
        {
            try
            {
                

                List<TrainingSchedule> _list = await _trainingScheduleService.GetTrainingScheduleList();

                _list = _list.Where(w => w.ScheduleStatus == "AVAILABLE" || w.ScheduleStatus == "COMPLETED").ToList();

                if (paramProgramCode.Length > 0 && !paramProgramCode.Contains("ALL"))
                    _list = _list.Where(w => paramProgramCode.Contains(w.Program.ProgramCode)).ToList();
                    
                if (paramCourseCode.Length > 0 && !paramCourseCode.Contains("ALL"))
                    _list = _list.Where(w => paramCourseCode.Contains(w.Course.CourseCode)).ToList();
                    
                if (paramRegionCode != null && paramRegionCode != "ALL")
                    _list = _list.Where(w => w.Region == paramRegionCode).ToList();
                    
                if (paramTrainingType != null && paramTrainingType != "ALL")
                    _list = _list.Where(w => w.TrainingType == paramTrainingType).ToList();

                
                if (paramProgramCode.Length == 0 && paramCourseCode.Length == 0 && paramRegionCode == null && paramTrainingType == null)
                    _list = new List<TrainingSchedule>();

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

                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest();
                throw;
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainingMonitoringDetails(string code)
        {
            try
            {            
                List<JobClass> jobclassList =  await _jobclassService.GetHRJobClassList();                
                TrainingSchedule trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(code);
                List<TrainingRegistration> _list = await _trainingRegistrationService.GetTraineeListByCode(trainingSchedule.TrainingCode);                

                TraineeRegistrationViewModel trainingScheduleViewModel = new TraineeRegistrationViewModel(){                
                    JobClasses = jobclassList,
                    TrainingScheduleDetails = trainingSchedule ?? null,
                    TraineeList = _list.Where(w => w.TrainingRegistrationStatus == "REGISTERED").OrderBy(s => s.EmployeeInfo.EmployeeName).ToList()
                };                                           
                
                return PartialView("~/Views/TrainingMonitoring/_TrainingMonitoringDetails.cshtml", trainingScheduleViewModel);
            }
            catch (Exception ex)
            {

                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest();
                throw;
            }
        }


        public async Task<IActionResult> GetTraineeList([DataSourceRequest] DataSourceRequest request, string code)
        {
            try
            {
                List<TrainingRegistration> _list = await _trainingRegistrationService.GetTraineeListByCode(code);

                DataSourceResult result = _list.Where(w => w.TrainingRegistrationStatus == "REGISTERED").OrderBy(s => s.EmployeeInfo.EmployeeName).ToDataSourceResult(request);
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
        public async Task<ActionResult> SaveChanges([DataSourceRequest] DataSourceRequest request, [Bind(Prefix = "models")]IEnumerable<TrainingRegistration> traineeList)
        {
            try
            {
                if (traineeList != null)
                {
                    foreach (var trainee in traineeList)
                    {
                        var traineeRegistration = await _trainingRegistrationService.GetTrainingRegistrationByCode(trainee.RegistrationCode);
                        if (traineeRegistration != null)
                        {
                            var old_traineeRegistration = traineeRegistration;
                            traineeRegistration.Attendance = trainee.Attendance;
                            traineeRegistration.AbsenceReason = trainee.AbsenceReason;
                            traineeRegistration.PostTestFirstScore = trainee.PostTestFirstScore;
                            traineeRegistration.PostTestSecondScore = trainee.PostTestSecondScore;
                            traineeRegistration.PostTestThirdScore = trainee.PostTestThirdScore;
                            traineeRegistration.EvaluationScore = trainee.EvaluationScore;

                            var totalScore = trainee.TrainingSchedule.Course.PostTestTotalScore ?? 0;

                            traineeRegistration.PostTestFirstScorePercentage = totalScore != 0 ? (double)trainee.PostTestFirstScore / totalScore : 0;
                            traineeRegistration.PostTestSecondScorePercentage = totalScore != 0 ? (double)trainee.PostTestSecondScore / totalScore : 0;
                            traineeRegistration.PostTestThirdScorePercentage = totalScore != 0 ? (double)trainee.PostTestThirdScore / totalScore : 0;

                            if (traineeRegistration.PostTestFirstScorePercentage >= .80 ||
                                traineeRegistration.PostTestSecondScorePercentage >= .80 ||
                                traineeRegistration.PostTestThirdScorePercentage >= .80
                            )
                                traineeRegistration.PostTestStatus = "PASSED";
                            else
                                traineeRegistration.PostTestStatus = "FAILED";

                            //Training Feedback Status
                            if (traineeRegistration.TrainingFeedbackStatus != "COMPLETE" || traineeRegistration.TrainingFeedbackStatus != null)
                                traineeRegistration.TrainingFeedbackStatus = "INCOMPLETE";


                            //Training Completion Status
                            if (
                                (trainee.Attendance == "PRESENT" || trainee.Attendance == "PARTIAL") &&
                                (trainee.TrainingSchedule.Course.WithPostTest && trainee.PostTestStatus == "PASSED") &&
                                (traineeRegistration.TrainingFeedbackStatus == "COMPLETE")
                            )
                            {
                                traineeRegistration.TrainingCompletionStatus = "COMPLETE";
                            }
                            else
                            {
                                traineeRegistration.TrainingCompletionStatus = "INCOMPLETE";
                            }

                            if (
                                traineeRegistration.CourseCompletionStatus != "COMPLETE" && traineeRegistration.TrainingCompletionStatus == "COMPLETE"
                            )
                                traineeRegistration.CourseCompletionStatus = "COMPLETE";
                            else
                                traineeRegistration.CourseCompletionStatus = "INCOMPLETE";


                            traineeRegistration.ModifiedBy = auditTrail["UserID"];
                            traineeRegistration.ModifiedByComputerUsed = auditTrail["HostName"];
                            traineeRegistration.DateModified = _globalService.GetDateTime();
                            
                            _trainingRegistrationService.UpdateRegistration();

                            var _logMsg = $"Update registration details: TrainingRegistration ({trainee.RegistrationCode}) - from '{JsonConvert.SerializeObject(old_traineeRegistration, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}' to '{JsonConvert.SerializeObject(traineeRegistration, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}'.";
                            _globalService.Log(_logMsg, auditTrail, null);
                        }
                    }
                }
                return Json(new[] { traineeList }.ToDataSourceResult(request, ModelState));
            }
            catch (Exception ex)
            {

                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest();
                throw;
            }
            
        }

        [HttpPost]
        public async Task<ActionResult> CompleteTraining(string paramTrainingCode)
        {
            try
            {
                TrainingSchedule _schedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(paramTrainingCode);     
                List<TrainingRegistration> _registration = await _trainingRegistrationService.GetTrainingRegistrationList();
                _registration = _registration.Where(w => w.TrainingSchedule.TrainingCode == paramTrainingCode).ToList();

                _schedule.ScheduleStatus = "COMPLETED";
                _schedule.RegistrationStatus = "CLOSED";
                _schedule.ScheduleCompletedBy = auditTrail["UserID"];
                _schedule.ScheduleCompletedDate = _globalService.GetDateTime();

                _trainingScheduleService.UpdateSchedule();

                _globalService.Log($"Set the training schedule as completed: TrainingSchedule ({paramTrainingCode})", auditTrail, null);

                foreach (var item in _registration)
                {
                    if(item.TrainingRegistrationStatus == "FOR APPROVAL"){
                        item.TrainingRegistrationStatus = "CANCELED";
                        item.TrainingCompletionStatus = "INCOMPLETE";
                        _trainingRegistrationService.UpdateRegistration();

                        _globalService.Log($"Set the training registration status as canceled and completion status as incomplete: TrainingRegistration ({item.RegistrationCode})", auditTrail, null);
                    }
                    
                }

                       
                //var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +                                        
                //$"Congratulations on finishing the training course on,{_schedule.TrainingCode} - {_schedule.Course.CourseTitle}.<br>" +
                //"As part of our training analysis, we are conducting a training feedback to determine this training's effectiveness. In this regard, we urge you to complete the form as honestly as possible.<br>" +
                //"Kindly visit the Training Feedback Form through the Registar System.<br>" +
                //"Your sincere and constructive feedback will help us assess the relevance of this program and develop future courses customized to our company's needs. <br>" +
                //"We appreciate your time and effort in providing us with your feedback.</p>"
                //;
                //var subject = "Training - Training Feedback";

                var traineeList = await _trainingRegistrationService.GetTraineeListByCode(_schedule.TrainingCode);

                var traineeEmail = traineeList.Select(e => e.EmployeeInfo.EmailAddress);
                        
                var _coordinatorList = await _trainingCoordinatorService.GetTrainingCoordinatorList();
                var coordinatorEmails = _globalService.GetEmployeeList()
                                        .Join(_coordinatorList, 
                                            e => e.EmployeeNo, 
                                            c => c.EmployeeNo, 
                                            (e, c) => e.EmailAddress)
                                        .ToList();

                var copy_recipient = string.Join(";", coordinatorEmails);
                var blind_recipient = string.Join(";", traineeEmail);

                var template = EmailTemplates.Get("TrainingFeedback");
                var html = EmailTemplates.FillTemplate(template.HtmlBody, new Dictionary<string, string>
                {
                    ["TrainingCode"] = _schedule.TrainingCode,
                    ["CourseTitle"] = _schedule.Course.CourseTitle
                });

                _globalService.SendEmail(html, template.Subject, null,copy_recipient,blind_recipient);

            }
            catch (Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }

            return Ok();
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