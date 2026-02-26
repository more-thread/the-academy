
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
    public class TrainingRegistrationController : Controller
    {
        private readonly ILogger<TrainingRegistrationController> _logger;
        private readonly ITrainingRegistrationService _trainingRegistrationService;
        private readonly ITrainingScheduleService _trainingScheduleService;
        private readonly ITrainingCoordinatorService _trainingCoordinatorService;
        private readonly GlobalService _globalService;
        private readonly IJobClassService _jobclassService;
        private readonly Dictionary<string,string> auditTrail;
        public TrainingRegistrationController(ILogger<TrainingRegistrationController> logger,
        ITrainingRegistrationService trainingRegistrationService,
        ITrainingScheduleService trainingScheduleService,
        ITrainingCoordinatorService trainingCoordinatorService,
        IJobClassService jobclassService,
        IHttpContextAccessor accessor,    
        GlobalService globalService
        )
        {
            auditTrail = new Dictionary<string,string>{
                {"HostName", accessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString()},
                {"UserID", accessor.HttpContext?.Session?.GetString("SessionUserID")},
                {"LoggedEmployeeNo", accessor.HttpContext?.Session?.GetString("SessionEmployeeNo")}
            };
            _trainingScheduleService = trainingScheduleService;
            _trainingCoordinatorService = trainingCoordinatorService;
            _globalService = globalService;
            _logger = logger;         
            _jobclassService = jobclassService;   
            _trainingRegistrationService = trainingRegistrationService;     
        }
        [ValidateAccess(ControllerName = "TrainingRegistration")]
        public IActionResult Index()
        {
            _globalService.PageVisitLog($"{RouteData.Values["controller"]}/{RouteData.Values["action"]}",auditTrail);
            return View(new TrainingRegistrationViewModel());
        }
        
        public async Task<IActionResult> GetTrainingRegistrationList([DataSourceRequest] DataSourceRequest request)
        {
            try
            {
                
                List<TrainingRegistration> _list = await _trainingRegistrationService.GetTrainingRegistrationList();

               
                DataSourceResult result = _list.Where(x => x.EmployeeNo == auditTrail["LoggedEmployeeNo"]).OrderByDescending(s => s.DateCreated).ToDataSourceResult(request);

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
        
        [HttpGet]
        public async Task<IActionResult> GetTrainingRegistrationWindow(string control,string code)
        {
            try
            {    
                FormControlModel formControlModel = new FormControlModel(control);
                
                List<JobClass> jobclassList =  await _jobclassService.GetHRJobClassList();
                TrainingRegistration _details = await _trainingRegistrationService.GetTrainingRegistrationByCode(code);

                TrainingRegistrationViewModel trainingScheduleViewModel = new TrainingRegistrationViewModel(){
                    FormControl = formControlModel,                    
                    JobClasses = jobclassList,
                    TrainingRegistrationDetails = _details
                };                                           
                
                return PartialView("~/Views/TrainingRegistration/_TrainingRegistrationDetails.cshtml", trainingScheduleViewModel);
            }
            catch (Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
                throw;
            }
        }      
        
        [HttpGet]
        public IActionResult GetTrainingRegistrationConfirmationWindow()
        {
            try
            {  
                return PartialView("~/Views/TrainingRegistration/_TrainingRegistrationConfirmation.cshtml");
            }
            catch (System.Exception)
            {
                return BadRequest();
                throw;
            }
        }      
        
       
        [HttpGet]
        public async Task<IActionResult> CheckIfAlreadyRegistered(string paramTrainingCode)
        {
            
            try
            {
                List<TrainingRegistration> existingSchedules = await _trainingRegistrationService.GetTrainingListByEmployeeNo(auditTrail["LoggedEmployeeNo"]);
                TrainingSchedule training = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(paramTrainingCode);
                

                if (existingSchedules.Any(w=>w.TrainingSchedule.TrainingCode == paramTrainingCode) || training.RegistrationStatus == "CLOSED")
                    return Ok(new {isExist = true }); 
                else
                    return Ok(new {isExist = false });

            }
            catch (Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost]        
        public async Task<ActionResult> RegisterTraining(string  paramTrainingCode)
        {
            try
            {

                List<TrainingRegistration> registered = await _trainingRegistrationService.GetTrainingRegistrationList();
                //int nextId = registered.Where(e => e.TrainingSchedule.TrainingCode == paramTrainingCode).ToList().Count + 1;

                VwHrEmployeeInfo _empDetails = _globalService.GetHREmployeeInfoByEmployeeNo(auditTrail["LoggedEmployeeNo"]);
                TrainingSchedule _trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(paramTrainingCode);


                var latestCode = registered
               .Where(e => e.TrainingSchedule.TrainingCode == paramTrainingCode)
               .Select(e => e.RegistrationCode)
               .OrderByDescending(code => code)
               .FirstOrDefault();

                int nextId = 0; 

                if (latestCode != null)
                {
                    // Extract the last 5 digits and convert to integer
                    string lastFiveDigits = latestCode.Substring(latestCode.Length - 5);
                    int lastId = int.Parse(lastFiveDigits);

                    // Increment the ID
                    nextId = lastId + 1;
                }

                int EmployeeLevel = int.Parse(_empDetails.EmployeeLevel.Substring(_empDetails.EmployeeLevel.Length - 1));

                if(EmployeeLevel <= 5){

                    //New Registration - For Approval
                    //var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +
                    //$"This is to inform you that {_empDetails.EmployeeName} has registered to this training,<br>" +
                    //$"<b>{paramTrainingCode} - {_trainingSchedule.Course.CourseTitle}</b>.<br>" +
                    //"Your approval is required to proceed with the registration.<br><br>" +
                    //"Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to approve or disapprove the registration.</p>"
                    //;
                    //var subject = "Training - New Registration For Approval";

                    var superiorEmail = _globalService.GetHREmployeeInfoByEmployeeNo(_empDetails.SuperiorId.ToString()).EmailAddress;
                    
                    var to_recipient = superiorEmail;
                    var copy_recipient = _empDetails.EmailAddress;
                    var template = EmailTemplates.Get("RegistrationForApproval");
                    var html = EmailTemplates.FillTemplate(template.HtmlBody, new Dictionary<string, string>
                    {
                        ["EmployeeName"] = _empDetails.EmployeeName,
                        ["TrainingCode"] = paramTrainingCode,
                        ["CourseTitle"] = _trainingSchedule.Course.CourseTitle
                    });

                    _globalService.SendEmail(html, template.Subject, to_recipient,copy_recipient,null);
                    
                }else{

                    //New Registration - For Confirmation
                    //var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +
                    //$"This is to inform you that you have been registered to this training,<br>" +
                    //$"<b>{paramTrainingCode} - {_trainingSchedule.Course.CourseTitle}</b>.<br>" +
                    //"Training Coordinators will review and confirm your registration.<br><br>" +
                    //"Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the status of your training and registrations.</p>"
                    //;
                    //var subject = "Training - New Registration For Confirmation";

                    var superiorEmail = _globalService.GetHREmployeeInfoByEmployeeNo(_empDetails.SuperiorId.ToString()).EmailAddress;
                    var sectionHeadEmail = _globalService.GetHREmployeeInfoByEmployeeNo(_empDetails.SectionHeadId.ToString()).EmailAddress;
                    
                    var _coordinatorList = await _trainingCoordinatorService.GetTrainingCoordinatorList();

                    var coordinatorEmails = _globalService.GetEmployeeList()
                                            .Join(_coordinatorList, 
                                                e => e.EmployeeNo, 
                                                c => c.EmployeeNo, 
                                                (e, c) => e.EmailAddress)
                                            .ToList();

                    var to_recipient = _empDetails.EmailAddress;
                    var copy_recipient =  string.Join(";", coordinatorEmails) +";"+superiorEmail + ";" + sectionHeadEmail;

                    var template = EmailTemplates.Get("RegistrationForConfirmation");
                    var html = EmailTemplates.FillTemplate(template.HtmlBody, new Dictionary<string, string>
                    {
                        ["TrainingCode"] = paramTrainingCode,
                        ["CourseTitle"] = _trainingSchedule.Course.CourseTitle
                    });

                    _globalService.SendEmail(html, template.Subject, to_recipient,copy_recipient,null);
                    
                }
                
                TrainingRegistration model = new()
                {
                    TrainingSchedule = _trainingSchedule,
                    RegistrationCode = paramTrainingCode + $"-{nextId.ToString("D5")}",
                    TrainingRegistrationStatus = EmployeeLevel <= 5? "FOR APPROVAL":"FOR CONFIRMATION",
                    EmployeeNo = _empDetails.EmployeeNo,
                    
                    RegistrationCreatedBy = auditTrail["UserID"],                    
                    RegistrationCreatedDate = _globalService.GetDateTime(),
                    
                    CreatedBy = auditTrail["UserID"],
                    CreatedByComputerUsed = auditTrail["HostName"],
                    DateCreated = _globalService.GetDateTime(),
                    Status = true
                };

                // Add the new record into the database
                _trainingRegistrationService.AddTrainee(model);

                
                _trainingScheduleService.UpdateSchedule();

                _globalService.Log("Add new trainee: TrainingRegistration - " + JsonConvert.SerializeObject(model, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), auditTrail, null);
            }
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }

            // Return success response
            return Ok();
        }
        
        public async Task<IActionResult> GetTrainingScheduleActivity([DataSourceRequest] DataSourceRequest request)
        {
            var scheduleList = await _trainingScheduleService.GetTrainingScheduleList();

            List<CalendarActivity> _list = new List<CalendarActivity>();
            
            scheduleList = scheduleList.Where(w => w.ScheduleStatus == "AVAILABLE" || w.ScheduleStatus =="COMPLETED").ToList();

            foreach (var item in scheduleList)
            {   
                _list.Add(
                    new CalendarActivity()
                    {
                        ID = item.TrainingCode,
                        Schedule = item,
                        Title = item.Program.ProgramTitle,
                        Start = (DateTime)item.StartDate.Date + item.StartTime,
                        End = (DateTime)item.EndDate.Date + item.EndTime,
                        TrainingScheduleStatus = item.ScheduleStatus
                    }
                );
            }
            DataSourceResult result = _list.ToDataSourceResult(request);
            var res = JsonConvert.SerializeObject(result, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

            return Content(res, "application/json");
        }

        [HttpPost]        
        public async Task<ActionResult> ConfirmCancellation(string paramCode,string paramReason)
        {
            try
            {
                TrainingRegistration _trainingRegistration = await _trainingRegistrationService.GetTrainingRegistrationByCode(paramCode);
                TrainingSchedule _trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(_trainingRegistration.TrainingSchedule.TrainingCode);

                if(_trainingRegistration.TrainingRegistrationStatus == "REGISTERED")
                    _trainingSchedule.RegisteredEmployeeCount -= 1;

                _trainingRegistration.TrainingRegistrationStatus = "CANCELLED";
                
                _trainingRegistration.RegistrationCanceledBy = auditTrail["UserID"];
                _trainingRegistration.RegistrationCanceledDate = _globalService.GetDateTime();
                    
                _trainingRegistration.ReasonForCancellation = paramReason;

                if(_trainingSchedule.RegisteredEmployeeCount == _trainingSchedule.ClassSize)
                {
                    _trainingSchedule.RegistrationStatus = "CLOSED";
                }else
                    _trainingSchedule.RegistrationStatus = "OPEN";

                _trainingRegistrationService.UpdateRegistration();

                _globalService.Log($"Cancel registration: TrainingRegistration ({paramCode})", auditTrail, null);
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
        public async Task<ActionResult> ConfirmRegistration(string paramCode, string paramReason, string paramConfirm)
        {
            try
            {
                TrainingRegistration _trainingRegistration = await _trainingRegistrationService.GetTrainingRegistrationByCode(paramCode);
                TrainingSchedule _trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(_trainingRegistration.TrainingSchedule.TrainingCode);


                if(paramConfirm == "Accept"){
                    _trainingRegistration.TrainingRegistrationStatus = "FOR CONFIRMATION";                                     
                    _trainingRegistration.RegistrationAcceptedBy = auditTrail["UserID"];
                    _trainingRegistration.RegistrationAcceptedDate = _globalService.GetDateTime();
                }                    
                else
                {   
                    _trainingRegistration.TrainingRegistrationStatus = "DENIED";                 
                    _trainingRegistration.RegistrationDeniedBy = auditTrail["UserID"];
                    _trainingRegistration.RegistrationDeniedDate = _globalService.GetDateTime();                    
                }
                    
                _trainingRegistration.ReasonForDenying = paramReason;

                _trainingRegistrationService.UpdateRegistration();

                _globalService.Log($"{paramConfirm} the registration: TrainingRegistration ({paramCode})", auditTrail, null);
            }
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }

            // Return success response
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