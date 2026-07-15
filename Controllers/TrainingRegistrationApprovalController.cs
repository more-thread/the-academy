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
    public class TrainingRegistrationApprovalController : Controller
    {
        private readonly ILogger<TrainingRegistrationApprovalController> _logger;
        private readonly ITrainingRegistrationService _trainingRegistrationService;
        private readonly ITrainingCoordinatorService _trainingCoordinatorService;
        private readonly GlobalService _globalService;
        private readonly Dictionary<string, string> auditTrail;
        public TrainingRegistrationApprovalController(ILogger<TrainingRegistrationApprovalController> logger,
        ITrainingRegistrationService trainingRegistrationService,
        ITrainingCoordinatorService trainingCoordinatorService,
        IHttpContextAccessor accessor,
        GlobalService globalService
        )
        {
            auditTrail = new Dictionary<string, string>{
                {"HostName", accessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString()},
                {"UserID", accessor.HttpContext?.Session?.GetString("SessionUserID")},
                {"LoggedEmployeeNo", accessor.HttpContext?.Session?.GetString("SessionEmployeeNo")}
            };
            _trainingCoordinatorService = trainingCoordinatorService;
            _globalService = globalService;
            _logger = logger;
            _trainingRegistrationService = trainingRegistrationService;
        }
        [ValidateAccess(ControllerName = "TrainingRegistrationApproval")]
        public IActionResult Index()
        {
            _globalService.PageVisitLog($"{RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail);
            return View(new TraineeRegistrationViewModel());
        }

        public async Task<JsonResult> GetEmployeeList()
        {
            List<VwHrEmployeeInfo> _list = _globalService.GetEmployeeList();
            _list = _list.Where(w =>
                (
                w.SuperiorId == Convert.ToInt64(auditTrail["LoggedEmployeeNo"]) ||
                w.SectionHeadId == Convert.ToInt64(auditTrail["LoggedEmployeeNo"]) ||
                w.DepartmentHeadId == Convert.ToInt64(auditTrail["LoggedEmployeeNo"]) ||
                w.DivisionHeadId == Convert.ToInt64(auditTrail["LoggedEmployeeNo"])
                )
            && w.EmployeeStatus == "A").ToList();

            return Json(_list.OrderBy(s => s.EmployeeName));
        }

        public async Task<IActionResult> GetTraineeList([DataSourceRequest] DataSourceRequest request, string paramFilter, string paramEmployeeNo)
        {
            try
            {
                List<TrainingRegistration> _list = await _trainingRegistrationService.GetTrainingRegistrationList();


                if (paramEmployeeNo != null && paramEmployeeNo != "ALL")
                    _list = _list.Where(w => w.EmployeeNo == paramEmployeeNo).ToList();

                if (paramFilter == "APPROVED")
                    _list = _list.Where(w => w.TrainingRegistrationStatus == "FOR CONFIRMATION"
                    || w.TrainingRegistrationStatus == "REGISTERED"
                    || w.TrainingRegistrationStatus == "SCHEDULE FOR CONFIRMATION").ToList();
                else
                    _list = _list.Where(w => w.TrainingRegistrationStatus == paramFilter).ToList();

                DataSourceResult result = _list.Where(w =>
                w.EmployeeInfo.SuperiorId == Convert.ToInt64(auditTrail["LoggedEmployeeNo"]) ||
                w.EmployeeInfo.SectionHeadId == Convert.ToInt64(auditTrail["LoggedEmployeeNo"]) ||
                w.EmployeeInfo.DepartmentHeadId == Convert.ToInt64(auditTrail["LoggedEmployeeNo"]) ||
                w.EmployeeInfo.DivisionHeadId == Convert.ToInt64(auditTrail["LoggedEmployeeNo"]) 
                ).OrderBy(s => s.DateCreated).ToDataSourceResult(request);

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

                return Content("[]", "application/json");
                throw;
            }
        }


        [HttpPost]
        public async Task<ActionResult> ApproveDisapproveRegistration(string paramRegistrationCode, string paramReason, string paramApproval)
        {
            try
            {
                

                TrainingRegistration _trainingRegistration = await _trainingRegistrationService.GetTrainingRegistrationByCode(paramRegistrationCode);
                VwHrEmployeeInfo _empDetails = _globalService.GetHREmployeeInfoByEmployeeNo(_trainingRegistration.EmployeeNo);

                if (paramApproval == "Approve")
                {
                    _trainingRegistration.TrainingRegistrationStatus = "FOR CONFIRMATION";                    
                    _trainingRegistration.RegistrationApprovedBy = auditTrail["UserID"];
                    _trainingRegistration.RegistrationApprovedDate = _globalService.GetDateTime();

                    //New Registration - For Confirmation
                    //var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +                    
                    //$"This is to inform you that your registration to this training, <b>{paramRegistrationCode} - {_trainingRegistration.TrainingSchedule.Course.CourseTitle}</b> has been approved.<br>" +
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

                    var template = EmailTemplates.Get("RegistrationApproved");
                    var html = EmailTemplates.FillTemplate(template.HtmlBody, new Dictionary<string, string>
                    {
                        ["TrainingCode"] = paramRegistrationCode,
                        ["CourseTitle"] = _trainingRegistration.TrainingSchedule.Course.CourseTitle
                    });

                    _globalService.SendEmail(html, template.Subject, to_recipient,copy_recipient,null);
                }
                else
                {
                    _trainingRegistration.TrainingRegistrationStatus = "DISAPPROVED";
                    _trainingRegistration.RegistrationDisapprovedBy = auditTrail["UserID"];
                    _trainingRegistration.RegistrationDisapprovedDate = _globalService.GetDateTime();

                    //New Registration - For Confirmation
                    //var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +                                        
                    //$"This is to inform you that your registration to this training, <b>{paramRegistrationCode} - {_trainingRegistration.TrainingSchedule.Course.CourseTitle}</b> has been disapproved due to this reason: {paramReason}.<br><br>" +
                    //"Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the status of your training and registrations.</p>"
                    //;
                    //var subject = "Training - New Registration Disapproved";

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
                    
                    var template = EmailTemplates.Get("RegistrationDisapproved");
                    var html = EmailTemplates.FillTemplate(template.HtmlBody, new Dictionary<string, string>
                    {
                        ["TrainingCode"] = paramRegistrationCode,
                        ["CourseTitle"] = _trainingRegistration.TrainingSchedule.Course.CourseTitle,
                        ["Reason"] = paramReason
                    });

                    _globalService.SendEmail(html, template.Subject, to_recipient,copy_recipient,null);
                }


                _trainingRegistration.ReasonForDisapproval = paramReason;

                _trainingRegistrationService.UpdateRegistration();

                //todo generate email
                _globalService.Log($"{paramApproval} the registration: TrainingRegistration ({paramRegistrationCode})", auditTrail, null);
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
        public IActionResult GetTrainingRegistrationCancellationWindow()
        {
            try
            {
                return PartialView("~/Views/TrainingRegistrationApproval/_TrainingRegistrationCancellationDetails.cshtml");
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
