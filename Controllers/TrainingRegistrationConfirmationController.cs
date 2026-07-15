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
    public class TrainingRegistrationConfirmationController : Controller
    {
        private readonly ILogger<TrainingRegistrationConfirmationController> _logger;
        private readonly ITrainingRegistrationService _trainingRegistrationService;
        private readonly ITrainingCoordinatorService _trainingCoordinatorService;
        private readonly ITrainingScheduleService _trainingScheduleService;
        private readonly IJobClassService _jobclassService;
        private readonly GlobalService _globalService;
        private readonly Dictionary<string,string> auditTrail;
        public TrainingRegistrationConfirmationController(ILogger<TrainingRegistrationConfirmationController> logger,
        ITrainingRegistrationService trainingRegistrationService,
        ITrainingCoordinatorService trainingCoordinatorService,
        IJobClassService jobclassService,
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
            _jobclassService = jobclassService;   
            _trainingScheduleService = trainingScheduleService;
            _trainingRegistrationService = trainingRegistrationService;
            _trainingCoordinatorService = trainingCoordinatorService;
        }
        [ValidateAccess(ControllerName = "TrainingRegistrationConfirmation")]
        public IActionResult Index()
        {
            _globalService.PageVisitLog($"{RouteData.Values["controller"]}/{RouteData.Values["action"]}",auditTrail);
            return View(new TrainingScheduleViewModel());
        }

        public async Task<JsonResult> GetEmployeeList()
        {
            List<VwHrEmployeeInfo> _list = _globalService.GetEmployeeList();
            _list =  _list.Where(w=>w.SuperiorId == Convert.ToInt64(auditTrail["LoggedEmployeeNo"]) && w.EmployeeStatus == "A").ToList();
            

            return Json(_list.OrderBy(s => s.EmployeeName));
        }


        [HttpGet]
        public async Task<IActionResult> GetTrainingRegistrationWindow(string code)
        {
            try
            {            
                List<JobClass> jobclassList =  await _jobclassService.GetHRJobClassList();                
                TrainingSchedule trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(code);
                List<TrainingRegistration> _list = await _trainingRegistrationService.GetTraineeListByCode(trainingSchedule.TrainingCode);                

                TraineeRegistrationViewModel trainingScheduleViewModel = new TraineeRegistrationViewModel(){                
                    JobClasses = jobclassList,
                    TrainingScheduleDetails = trainingSchedule ?? null,
                    TraineeList = _list.Where(w => w.TrainingRegistrationStatus == "REGISTERED" || w.TrainingRegistrationStatus == "FOR CONFIRMATION").OrderBy(s => s.EmployeeInfo.EmployeeName).ToList()
                };                                           
                
                return PartialView("~/Views/TrainingRegistrationConfirmation/_TrainingRegistrationConfirmationDetails.cshtml", trainingScheduleViewModel);
            }
            catch (Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
                throw;
            }
        }      

        public async Task<IActionResult> GetTraineeForConfirmation([DataSourceRequest] DataSourceRequest request, string code)
        {
            try
            {
                List<TrainingRegistration> _list = await _trainingRegistrationService.GetTrainingRegistrationList();

                DataSourceResult result = _list.Where(w=> w.TrainingSchedule.TrainingCode == code && ( w.TrainingRegistrationStatus == "REGISTERED" || w.TrainingRegistrationStatus == "FOR CONFIRMATION")).OrderBy(s => s.DateCreated).ToDataSourceResult(request);
                var res = JsonConvert.SerializeObject(result, Formatting.None,
                            new JsonSerializerSettings()
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            });

                return Content(res, "application/json");
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public async Task<IActionResult> GetTrainingScheduleList([DataSourceRequest] DataSourceRequest request,
        string paramProgramCode,
        string paramCourseCode,
        string paramRegionCode,
        string paramTrainingType
        )
        {
            try
            {
                

                List<TrainingSchedule> _list = await _trainingScheduleService.GetTrainingScheduleList();

                
                if (paramProgramCode != null && paramProgramCode != "ALL")
                    _list = _list.Where(w => w.Program.ProgramCode == paramProgramCode).ToList();

                if (paramCourseCode != null && paramCourseCode != "ALL")
                    _list = _list.Where(w => w.Course.CourseCode == paramCourseCode).ToList();
                    
                if (paramRegionCode != null && paramRegionCode != "ALL")
                    _list = _list.Where(w => w.Region == paramRegionCode).ToList();
                    
                if (paramTrainingType != null && paramTrainingType != "ALL")
                    _list = _list.Where(w => w.TrainingType == paramTrainingType).ToList();
                
                if (paramProgramCode == null && paramCourseCode == null && paramRegionCode == null && paramTrainingType == null)
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
                return BadRequest(ex.Message);
                throw;
            }
            
        }


        [HttpPost]        
        public async Task<ActionResult> ConfirmRejectRegistration(string paramCode, string paramReason, string paramConfirm)
        {
            try
            {
                TrainingRegistration _trainingRegistration = await _trainingRegistrationService.GetTrainingRegistrationByCode(paramCode);
                    
                TrainingSchedule _trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(_trainingRegistration.TrainingSchedule.TrainingCode);

                if(paramConfirm == "Confirm"){
                    _trainingRegistration.TrainingRegistrationStatus = "REGISTERED";    
                    _trainingRegistration.RegistrationConfirmedBy = auditTrail["UserID"];
                    _trainingRegistration.RegistrationConfirmedDate = _globalService.GetDateTime();                   
                    _trainingSchedule.RegisteredEmployeeCount =  (_trainingSchedule.RegisteredEmployeeCount??0) + 1;
                    
                    if(_trainingSchedule.RegisteredEmployeeCount == _trainingSchedule.ClassSize)
                        _trainingSchedule.RegistrationStatus = "CLOSED";

                    //Registration-Confirmed (Individual)
                    //var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +
                    //$"This is to inform you that your registration to this training, <b>{_trainingRegistration.TrainingSchedule.TrainingCode} - {_trainingRegistration.TrainingSchedule.Course.CourseTitle}</b>, has been confirmed.<br>" +
                    //"Your attendance is highly appreciated.<br><br>" +
                    //"Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the details of your training and registrations.</p>"
                    //;
                    //var subject = "Training - Registration Confirmed";

                    var sectionHeadEmail = _globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeInfo.SectionHeadId.ToString()).EmailAddress;
                    var superiorEmail = _globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeInfo.SuperiorId.ToString()).EmailAddress;                

                    var to_recipient = _trainingRegistration.EmployeeInfo.EmailAddress;
                    var copy_recipient = string.Join(";", sectionHeadEmail);

                    var template = EmailTemplates.Get("RegistrationConfirmed");
                    var html = EmailTemplates.FillTemplate(template.HtmlBody, new Dictionary<string, string>
                    {
                        ["TrainingCode"] = _trainingRegistration.TrainingSchedule.TrainingCode,
                        ["CourseTitle"] = _trainingRegistration.TrainingSchedule.Course.CourseTitle
                    });
                    
                    _globalService.SendEmail(html, template.Subject,to_recipient,copy_recipient,null);
                }                    
                else{                    
                    if(_trainingRegistration.TrainingRegistrationStatus == "REGISTERED")
                        _trainingSchedule.RegisteredEmployeeCount -= 1;

                    if(_trainingSchedule.RegisteredEmployeeCount != _trainingSchedule.ClassSize)
                        _trainingSchedule.RegistrationStatus = "OPEN";
                        
                    _trainingRegistration.TrainingRegistrationStatus = "REJECTED";
                    _trainingRegistration.RegistrationRejectedBy = auditTrail["UserID"];
                    _trainingRegistration.RegistrationRejectedDate = _globalService.GetDateTime();  

                     
                    //Registration-Rejected (Individual)
                    //var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +     
                    //$"This is to inform you that your registration to this training, <b>{_trainingRegistration.TrainingSchedule.TrainingCode} - {_trainingRegistration.TrainingSchedule.Course.CourseTitle}</b> has been rejected due to this reason: {paramReason}.<br><br>" +
                    //"Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the details of your training and registrations.</p>"
                    //;
                    //var subject = "Training - Registration Rejected";

                    var sectionHeadEmail = _globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeInfo.SectionHeadId.ToString()).EmailAddress;
                    var superiorEmail = _globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeInfo.SuperiorId.ToString()).EmailAddress;                

                    var to_recipient = _trainingRegistration.EmployeeInfo.EmailAddress;
                    var copy_recipient = string.Join(";", sectionHeadEmail);

                    var template = EmailTemplates.Get("RegistrationRejected");
                    var html = EmailTemplates.FillTemplate(template.HtmlBody, new Dictionary<string, string>
                    {
                        ["TrainingCode"] = _trainingRegistration.TrainingSchedule.TrainingCode,
                        ["CourseTitle"] = _trainingRegistration.TrainingSchedule.Course.CourseTitle,
                        ["Reason"] = paramReason
                    });

                    _globalService.SendEmail(html, template.Subject, to_recipient,copy_recipient,null);
                }
                    
                
                _trainingRegistration.ReasonForDisapproval = paramReason;

                _trainingRegistrationService.UpdateRegistration();

                //todo generate email
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

        [HttpPost]        
        public async Task<ActionResult> ConfirmRejectRegistration_Group(List<string> paramCode, string paramReason, string paramConfirm, string paramTrainingCode)
        {
            try
            {
                TrainingSchedule _trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(paramTrainingCode);
                
                foreach (var item in paramCode)
                {
                    TrainingRegistration _trainingRegistration = await _trainingRegistrationService.GetTrainingRegistrationByCode(item);
                    

                    if(paramConfirm == "Confirm"){
                        _trainingRegistration.TrainingRegistrationStatus = "REGISTERED";
                        _trainingRegistration.RegistrationConfirmedBy = auditTrail["UserID"];
                        _trainingRegistration.RegistrationConfirmedDate = _globalService.GetDateTime();                 

                        _trainingSchedule.RegisteredEmployeeCount =  (_trainingSchedule.RegisteredEmployeeCount??0) + 1;
                        
                        if(_trainingSchedule.RegisteredEmployeeCount == _trainingSchedule.ClassSize)
                            _trainingSchedule.RegistrationStatus = "CLOSED";

                    }                    
                    else{                    
                        if(_trainingRegistration.TrainingRegistrationStatus == "REGISTERED")
                            _trainingSchedule.RegisteredEmployeeCount -= 1;

                        if(_trainingSchedule.RegisteredEmployeeCount != _trainingSchedule.ClassSize)
                            _trainingSchedule.RegistrationStatus = "OPEN";
                            
                        _trainingRegistration.TrainingRegistrationStatus = "REJECTED";
                        _trainingRegistration.RegistrationRejectedBy = auditTrail["UserID"];
                        _trainingRegistration.RegistrationRejectedDate = _globalService.GetDateTime();                 
                    }

                    _trainingRegistration.ReasonForDisapproval = paramReason;
                    
                    //todo generate email
                    _globalService.Log($"{paramConfirm} the registration: TrainingRegistration ({paramCode})", auditTrail, null);
                }
                
                _trainingRegistrationService.UpdateRegistration();
                
                    
                List<string> list_employeeEmailAddress = new();
                List<string> list_superiorEmailAddress= new();
                List<string> list_sectionHeadEmailAddress= new();
                
                foreach (var item in paramCode)
                {
                    TrainingRegistration _trainingRegistration = await _trainingRegistrationService.GetTrainingRegistrationByCode(item);
                    list_employeeEmailAddress.Add(_trainingRegistration.EmployeeInfo.EmailAddress);
                    
                    list_sectionHeadEmailAddress.Add(_globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeInfo.SectionHeadId.ToString()).EmailAddress);
                    list_superiorEmailAddress.Add(_globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeInfo.SuperiorId.ToString()).EmailAddress);

                }
                
                
                //New Registration - For Confirmation
                var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +
                
                $"This is to inform you that your registration to this training, <b>{_trainingSchedule.TrainingCode} - {_trainingSchedule.Course.CourseTitle}</b>, has been confirmed.<br>" +
                "Your attendance is highly appreciated.<br><br>" +
                "Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the details of your training and registrations.</p>"
                ;
                var subject = "Training - Registration Confirmed";

                
                var blind_recipient = string.Join(";",list_employeeEmailAddress) +";"+ string.Join(";", list_sectionHeadEmailAddress) +";"+ string.Join(";", list_superiorEmailAddress);

                var template = EmailTemplates.Get("RegistrationConfirmed");
                var html = EmailTemplates.FillTemplate(template.HtmlBody, new Dictionary<string, string>
                {
                    ["TrainingCode"] = _trainingSchedule.TrainingCode,
                    ["CourseTitle"] = _trainingSchedule.Course.CourseTitle
                });

                _globalService.SendEmail(html, template.Subject, null,null,blind_recipient);

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
        public async Task<ActionResult> SendEmailReminder(string paramCode)
        {
            try
            {                
                TrainingSchedule _trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(paramCode);
                    
                //New Registration - For Confirmation
                //var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +
                
                //$"This is to remind you of your training about <b>{_trainingSchedule.TrainingCode} - {_trainingSchedule.Course.CourseTitle}</b>, scheduled on {_trainingSchedule.StartDate.ToShortDateString()} - {_trainingSchedule.EndDate.ToShortDateString()} at {_trainingSchedule.StartTime} - {_trainingSchedule.EndTime}.<br><br>" +
                //"Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the details of your training and registrations.</p>"
                //;
                //var subject = "Training - Schedule Reminder";


                var traineeList = await _trainingRegistrationService.GetTraineeListByCode(_trainingSchedule.TrainingCode);


                var traineeEmail = traineeList.Where(w=> w.TrainingRegistrationStatus == "REGISTERED").Select(e => e.EmployeeInfo.EmailAddress);
                        
                var _coordinatorList = await _trainingCoordinatorService.GetTrainingCoordinatorList();

                var coordinatorEmails = _globalService.GetEmployeeList()
                                        .Join(_coordinatorList, 
                                            e => e.EmployeeNo, 
                                            c => c.EmployeeNo, 
                                            (e, c) => e.EmailAddress)
                                        .ToList();

                var to_recipient = string.Join(";", coordinatorEmails);
                var blind_recipient = string.Join(";", traineeEmail);

                var template = EmailTemplates.Get("ScheduleReminder");
                var html = EmailTemplates.FillTemplate(template.HtmlBody, new Dictionary<string, string>
                {
                    ["TrainingCode"] = _trainingSchedule.TrainingCode,
                    ["CourseTitle"] = _trainingSchedule.Course.CourseTitle,
                    ["StartDate"] = _trainingSchedule.StartDate.ToShortDateString(),
                    ["EndDate"] = _trainingSchedule.EndDate.ToShortDateString(),
                    ["StartTime"] = _trainingSchedule.StartTime.ToString(),
                    ["EndTime"] = _trainingSchedule.EndTime.ToString()
                });

                _globalService.SendEmail(html, template.Subject, to_recipient,null,blind_recipient);
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
        public async Task<ActionResult> SendEmployeeEmailReminder(string paramRegistrationCode)
        {
            try
            {                
                TrainingRegistration _trainingRegistration = await _trainingRegistrationService.GetTrainingRegistrationByCode(paramRegistrationCode);
                    
                //New Registration - For Confirmation
                //var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +
                
                //$"This is to remind you that there were changes in the schedule of this training, <b>{_trainingRegistration.TrainingSchedule.TrainingCode} - {_trainingRegistration.TrainingSchedule.Course.CourseTitle}</b>, that you registered.<br><br>" +
                //"Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the changes and confirm your attendance with the new schedule.</p>"
                //;
                //var subject = "Training - Schedule For Confirmation Reminder";

                var sectionHeadEmail = _globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeInfo.SectionHeadId.ToString()).EmailAddress;
                var superiorEmail = _globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeInfo.SuperiorId.ToString()).EmailAddress;                

                var to_recipient = _trainingRegistration.EmployeeInfo.EmailAddress;
                var copy_recipient = string.Join(";", sectionHeadEmail);

                var template = EmailTemplates.Get("ScheduleForConfirmationReminder");
                var html = EmailTemplates.FillTemplate(template.HtmlBody, new Dictionary<string, string>
                {
                    ["TrainingCode"] = paramRegistrationCode,
                    ["CourseTitle"] = _trainingRegistration.TrainingSchedule.Course.CourseTitle                    
                });

                _globalService.SendEmail(html, template.Subject, to_recipient,copy_recipient,null);
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
        public async Task<ActionResult> SendConfirmedGroupEmailReminder(string[] paramRegCode,string trainingCode)
        {
            try
            {       
                List<string> list_employeeEmailAddress = new();
                List<string> list_superiorEmailAddress= new();
                List<string> list_sectionHeadEmailAddress= new();
                
                foreach (var item in paramRegCode)
                {
                    TrainingRegistration _trainingRegistration = await _trainingRegistrationService.GetTrainingRegistrationByCode(item);
                    list_employeeEmailAddress.Add(_trainingRegistration.EmployeeInfo.EmailAddress);
                    
                    list_sectionHeadEmailAddress.Add(_globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeInfo.SectionHeadId.ToString()).EmailAddress);
                    list_superiorEmailAddress.Add(_globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeInfo.SuperiorId.ToString()).EmailAddress);

                }
                
                
                TrainingSchedule _trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(trainingCode);
                //New Registration - For Confirmation
                //var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +
                
                //$"This is to inform you that your registration to this training, <b>{_trainingSchedule.TrainingCode} - {_trainingSchedule.Course.CourseTitle}</b>, has been confirmed.<br>" +
                //"Your attendance is highly appreciated.<br><br>" +
                //"Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the details of your training and registrations.</p>"
                //;
                //var subject = "Training - Registration Confirmed";

                
                var blind_recipient = string.Join(";",list_employeeEmailAddress) +";"+ string.Join(";", list_sectionHeadEmailAddress) +";"+ string.Join(";", list_superiorEmailAddress);

                var template = EmailTemplates.Get("RegistrationConfirmed");
                var html = EmailTemplates.FillTemplate(template.HtmlBody, new Dictionary<string, string>
                {
                    ["TrainingCode"] = _trainingSchedule.TrainingCode,
                    ["CourseTitle"] = _trainingSchedule.Course.CourseTitle
                });

                _globalService.SendEmail(html, template.Subject, null,null,blind_recipient);

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
        public async Task<ActionResult> SendRejectedGroupEmailReminder(string[] paramRegCode,string trainingCode, string reason)
        {
            try
            {       
                List<string> list_employeeEmailAddress = new();
                List<string> list_superiorEmailAddress= new();
                List<string> list_sectionHeadEmailAddress= new();
                
                foreach (var item in paramRegCode)
                {
                    TrainingRegistration _trainingRegistration = await _trainingRegistrationService.GetTrainingRegistrationByCode(item);
                    list_employeeEmailAddress.Add(_trainingRegistration.EmployeeInfo.EmailAddress);
                    
                    list_sectionHeadEmailAddress.Add(_globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeInfo.SectionHeadId.ToString()).EmailAddress);
                    list_superiorEmailAddress.Add(_globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeInfo.SuperiorId.ToString()).EmailAddress);

                }
                
                
                TrainingSchedule _trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(trainingCode);
                //Registration-Rejected (Group)
                //var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +
                
                //$"This is to inform you that your registration to this training, <b>{_trainingSchedule.TrainingCode} - {_trainingSchedule.Course.CourseTitle}</b> has been rejected due to this reason: {reason}.<br><br>" +
                //"Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the status of your training and registrations.</p>"
                //;
                //var subject = "Training - Registration Rejected";

                
                var blind_recipient = string.Join(";",list_employeeEmailAddress) +";"+ string.Join(";", list_sectionHeadEmailAddress) +";"+ string.Join(";", list_superiorEmailAddress);


                var template = EmailTemplates.Get("RegistrationRejected");
                var html = EmailTemplates.FillTemplate(template.HtmlBody, new Dictionary<string, string>
                {
                    ["TrainingCode"] = _trainingSchedule.TrainingCode,
                    ["CourseTitle"] = _trainingSchedule.Course.CourseTitle,
                    ["Reason"] = reason
                });

                _globalService.SendEmail(html, template.Subject, null,null,blind_recipient);
                
            }
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }

            // Return success response
            return Ok();
        }

        [HttpGet]
        public IActionResult GetTrainingRegistrationConfirmationRejectWindow()
        {
            try
            {    
               return PartialView("~/Views/TrainingRegistrationConfirmation/_TrainingRegistrationConfirmationReject.cshtml");
            }
            catch (Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
                throw;
            }
        }       

        [HttpGet]
        public IActionResult GetTrainingRegistrationConfirmationRejectMultipleWindow()
        {
            try
            {    
               return PartialView("~/Views/TrainingRegistrationConfirmation/_TrainingRegistrationConfirmationRejectMultiple.cshtml");
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