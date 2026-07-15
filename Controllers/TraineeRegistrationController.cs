using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Packaging.Licenses;
using OfficeOpenXml;
using System.Diagnostics;
using TRS.Attributes;
using TRS.Global;
using TRS.Interfaces;
using TRS.Models;
using TRS.ViewModels;

namespace TRS.Controllers
{
    [ValidateSession]
    public class TraineeRegistrationController : Controller
    {
        private readonly ILogger<TraineeRegistrationController> _logger;
        private readonly ITrainingRegistrationService _trainingRegistrationService;
        private readonly ITrainingScheduleService _trainingScheduleService;
        private readonly ITrainingCoordinatorService _trainingCoordinatorService;
        private readonly GlobalService _globalService;
        private readonly IJobClassService _jobclassService;
        private Dictionary<string,string> auditTrail;
        private readonly IHttpContextAccessor _accessor;
        public TraineeRegistrationController(ILogger<TraineeRegistrationController> logger,
        ITrainingRegistrationService trainingRegistrationService,
        ITrainingScheduleService trainingScheduleService,
        ITrainingCoordinatorService trainingCoordinatorService,
        IJobClassService jobclassService,
        IHttpContextAccessor accessor,
        GlobalService globalService
        )
        {            
            _accessor= accessor;
            
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
        [ValidateAccess(ControllerName = "TraineeRegistration")]
        public IActionResult Index()
        {
            _globalService.PageVisitLog($"{RouteData.Values["controller"]}/{RouteData.Values["action"]}",auditTrail);
            return View(new TrainingScheduleViewModel());
        }
        
        [HttpGet]
        public async Task<IActionResult> GetTraineeRegistrationWindow(string code)
        {
            try
            {            
                List<JobClass> jobclassList =  await _jobclassService.GetHRJobClassList();                
                TrainingSchedule trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(code);
                List<TrainingRegistration> _list = await _trainingRegistrationService.GetTraineeListByCode(trainingSchedule.TrainingCode);                

                TraineeRegistrationViewModel trainingScheduleViewModel = new TraineeRegistrationViewModel(){                
                    JobClasses = jobclassList,
                    TrainingScheduleDetails = trainingSchedule ?? null,
                    TraineeList = _list.Where(w => w.TrainingRegistrationStatus == "REGISTERED" 
                    || w.TrainingRegistrationStatus == "FOR CONFIRMATION"
                    || w.TrainingRegistrationStatus == "SCHEDULE FOR CONFIRMATION"
                    || w.TrainingRegistrationStatus == "FOR APPROVAL").OrderBy(s => s.EmployeeInfo.EmployeeName).ToList()
                };
                
                return PartialView("~/Views/TraineeRegistration/_TraineeRegistrationDetails.cshtml", trainingScheduleViewModel);
            }
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest();
                throw;
            }
        }
        
        public async Task<JsonResult> GetEmployeeList()
        {
            List<VwHrEmployeeInfo> _list = _globalService.GetEmployeeList();
            
            var _isCoordinator = await _trainingCoordinatorService.GetTrainingCoordinatorByEmployeeNo(auditTrail["LoggedEmployeeNo"]);
            
            if(_isCoordinator != null)
                if(!_isCoordinator.Status) 
                    _list = new();

            if(_isCoordinator == null)
                _list =  _list.Where(w=>w.SuperiorId == Convert.ToInt64(auditTrail["LoggedEmployeeNo"]) && w.EmployeeStatus == "A").ToList();

            return Json(_list.OrderBy(s => s.EmployeeName));
        }

        public async Task<IActionResult> GetTraineeList([DataSourceRequest] DataSourceRequest request, string code)
        {
            try
            {
                List<TrainingRegistration> _list = await _trainingRegistrationService.GetTraineeListByCode(code);

                DataSourceResult result = _list.Where(w =>  w.TrainingRegistrationStatus == "REGISTERED" 
                    || w.TrainingRegistrationStatus == "FOR CONFIRMATION"
                    || w.TrainingRegistrationStatus == "SCHEDULE FOR CONFIRMATION"
                    || w.TrainingRegistrationStatus == "FOR APPROVAL").OrderBy(s => s.EmployeeInfo.EmployeeName).ToDataSourceResult(request);
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
        
        public async Task<IActionResult> GetTrainingScheduleList([DataSourceRequest] DataSourceRequest request,
        string paramProgramCode,
        string paramCourseCode,
        string paramRegionCode,
        string[] JobClassCodeFilterArr
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
                    
                if (JobClassCodeFilterArr.Length > 0 && !JobClassCodeFilterArr.Contains("ALL"))
                    _list = _list.Where(w => w.Program.JobClasses.Any(ac => JobClassCodeFilterArr.Contains(ac.JobClassCode))).ToList();
                    
                if (paramProgramCode == null && JobClassCodeFilterArr.Length == 0 && paramRegionCode == null && paramCourseCode == null)
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
        public IActionResult GetTraineeRegistrationDenyConfirmationWindow()
        {
            try
            {  
                return PartialView("~/Views/TraineeRegistration/_TraineeRegistrationDenyConfirmation.cshtml");
            }
            catch (System.Exception)
            {
                return BadRequest();
                throw;
            }
        }              
       
        [HttpGet]
        public IActionResult GetTraineeRegistrationCancellationConfirmationWindow()
        {
            try
            {  
                return PartialView("~/Views/TraineeRegistration/_TraineeRegistrationCancellationConfirmation.cshtml");
            }
            catch (System.Exception)
            {
                return BadRequest();
                throw;
            }
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
                _trainingRegistration.ReasonForCancellation = paramReason;


                if(_trainingSchedule.RegisteredEmployeeCount == _trainingSchedule.ClassSize)
                {
                    _trainingSchedule.RegistrationStatus = "CLOSED";
                }else
                    _trainingSchedule.RegistrationStatus = "OPEN";


                _trainingRegistrationService.UpdateRegistration();

                _globalService.Log($"Cancel trainee: TrainingRegistration ({paramCode})", auditTrail, null);
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
        public async Task<ActionResult> AcceptDenyTrainee(string paramCode, string paramReason, string paramConfirm)
        {
            try
            {
                TrainingRegistration _trainingRegistration = await _trainingRegistrationService.GetTrainingRegistrationByCode(paramCode);
                TrainingSchedule _trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(_trainingRegistration.TrainingSchedule.TrainingCode);


                if(paramConfirm == "Accept")
                    _trainingRegistration.TrainingRegistrationStatus = "FOR CONFIRMATION";
                else
                    _trainingRegistration.TrainingRegistrationStatus = "DENIED";
                
                _trainingRegistration.ReasonForDenying = paramReason;


                _trainingRegistrationService.UpdateRegistration();

                _globalService.Log($"{paramConfirm} trainee: TrainingRegistration ({paramCode})", auditTrail, null);
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
        public async Task<ActionResult> ValidateEmployee(string paramTrainingCode,string paramEmployeeNo)
        {

            //check if program title exists in database
            var _traineeList = await _trainingRegistrationService.GetTraineeListByCode(paramTrainingCode);
            
            if(_traineeList.Any(w => w.EmployeeInfo.EmployeeNo == paramEmployeeNo && w.TrainingCompletionStatus == "COMPLETE"))
            {
                return Ok("completed"); 
            }
            if(_traineeList.Any(w => w.EmployeeInfo.EmployeeNo == paramEmployeeNo))
            {
                return Ok("already registered"); 
            }

            return Ok("false"); 
        }

        [HttpPost]        
        public async Task<ActionResult> RegisterEmployee(string  paramTrainingCode,string paramEmployeeNo)
        {
            try
            {
                List<TrainingRegistration> registered = await _trainingRegistrationService.GetTrainingRegistrationList();                
                //int nextId = registered.Where(e => e.TrainingSchedule.TrainingCode == paramTrainingCode).ToList().Count + 1;
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
                    

                TrainingRegistration model = new()
                {
                    TrainingSchedule = _trainingSchedule,
                    RegistrationCode = paramTrainingCode + $"-{nextId.ToString("D5")}",
                    TrainingRegistrationStatus = "FOR CONFIRMATION",
                    EmployeeNo = paramEmployeeNo,
                    CreatedBy = auditTrail["UserID"],
                    CreatedByComputerUsed = auditTrail["HostName"],
                    DateCreated = _globalService.GetDateTime(),
                    Status = true
                };

                // Add the new record into the database
                _trainingRegistrationService.AddTrainee(model);

                _globalService.Log("Add new trainee: TrainingRegistration - " + JsonConvert.SerializeObject(model, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), auditTrail, null);

                
                //New Registration - For Confirmation
                //var htmlString = "<p>Dear Ma'am/Sir,<br><br>" +
                //$"This is to inform you that you have been registered to this training,<br>" +
                //$"<b>{paramTrainingCode} - {_trainingSchedule.Course.CourseTitle}</b>.<br>" +
                //"Training Coordinators will review and confirm your registration.<br><br>" +
                //"Please login to the <a href=\"https://hrgateway.universalleaf.com.ph\">Training Registrar System</a> to view the status of your training and registrations.</p>"
                //;
                //var subject = "Training - New Registration For Confirmation";
                
                VwHrEmployeeInfo _empDetails = _globalService.GetHREmployeeInfoByEmployeeNo(paramEmployeeNo);
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
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }

            // Return success response
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> ExportExcel(string paramTrainingCode)
        {
            string handle;
            handle = Guid.NewGuid().ToString();

            TrainingSchedule _schedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(paramTrainingCode);
            List<TrainingRegistration> _trainees = await _trainingRegistrationService.GetTraineeListByCode(paramTrainingCode);
            var _attendees = _trainees.Where(w => w.Attendance == "PRESENT" || w.Attendance == "PARTIAL").ToList();

            MemoryStream stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var lastRow = 6; //constant initial row after column headers

                var workSheet = package.Workbook.Worksheets.Add("Trainees");

                var headerTitles = new List<string>() { "EMPLOYEE NO.", "EMPLOYEE NAME", "POSITION", "DEPARTMENT", "SUPERIOR", "EMAIL ADDRESS", "CONTACT NO.", 
                                                        "TRAINING COMPLETION STATUS", "TRAINING REGISTRATION STATUS", "REGISTERED BY", "REGISTERED DATE AND TIME" };

                //file info and column headers 
                workSheet.Cells[1, 1].Value = "Program & Course: " + _schedule.Program.ProgramTitle + " - " + _schedule.Course.CourseTitle;
                workSheet.Cells[2, 1].Value = "Training Code: " + _schedule.TrainingCode;
                workSheet.Cells[3, 1].Value = "Date: " + _schedule.StartDate.ToString("yyyy/MM/dd") + " - " + _schedule.EndDate.ToString("yyyy/MM/dd");
                workSheet.Cells[4, 1].Value = "Time: " + DateTime.Today.Add(_schedule.StartTime).ToString("hh:mm tt") + " - " + DateTime.Today.Add(_schedule.EndTime).ToString("hh:mm tt");

                workSheet.Cells[1, 1, 4, 1].Style.Font.Bold = true;

                var i = 0;
                foreach (var header in headerTitles)
                {
                    var headerCell = workSheet.Cells[6, i + 1];
                    headerCell.Value = headerTitles[i];

                    i++;
                }
                i = 0;

                //trainees
                foreach(var attendee in _attendees)
                {
                    var nextRow = lastRow + 1;
                    var employeeInfo = attendee.EmployeeInfo;

                    workSheet.Cells[nextRow, 1].Value = employeeInfo.EmployeeNo;
                    workSheet.Cells[nextRow, 2].Value = employeeInfo.EmployeeName;
                    workSheet.Cells[nextRow, 3].Value = employeeInfo.PositionName;
                    workSheet.Cells[nextRow, 4].Value = employeeInfo.DepartmentName;
                    workSheet.Cells[nextRow, 5].Value = employeeInfo.SuperiorFullname;
                    workSheet.Cells[nextRow, 6].Value = employeeInfo.EmailAddress;
                    workSheet.Cells[nextRow, 7].Value = employeeInfo.PersonalPhoneNo;
                    workSheet.Cells[nextRow, 8].Value = attendee.TrainingCompletionStatus;
                    workSheet.Cells[nextRow, 9].Value = attendee.TrainingRegistrationStatus;
                    workSheet.Cells[nextRow, 10].Value = attendee.RegistrationCreatedBy;
                    workSheet.Cells[nextRow, 11].Value = attendee.RegistrationCreatedDate?.ToString("yyyy/MM/dd hh:mm tt");

                    lastRow++;
                }

                //styling
                workSheet.Cells[6, 1, 6, headerTitles.Count()].Style.Font.Bold = true;

                workSheet.Cells[6, 1, 6, headerTitles.Count()].AutoFilter = true;

                workSheet.Cells[6, 1, workSheet.Cells.End.Row, workSheet.Cells.End.Column].AutoFitColumns();

                package.SaveAs(stream);
            };

            stream.Position = 0;

            var filepath = Path.Combine(Path.GetTempPath(), handle + ".xlsx");
            System.IO.File.WriteAllBytes(filepath, stream.ToArray());

            return new JsonResult(new
            {
                FileGuid = handle,
                FileName = "Trainee Registration - " + paramTrainingCode + " " + DateTime.Now.ToString("yyyyMMdd") + ".xlsx"
            });
        }

        public virtual ActionResult Download(string fileGuid, string fileName)
        {
            var filePath = Path.Combine(Path.GetTempPath(), fileGuid + ".xlsx");

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var file = File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            return file;

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