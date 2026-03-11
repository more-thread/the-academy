using Azure.Messaging;
using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Packaging.Licenses;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using TRS.Attributes;
using TRS.Global;
using TRS.Interfaces;
using TRS.Models;
using TRS.ViewModels;

namespace TRS.Controllers
{
    [ValidateSession]
    public class TrainingScheduleController : Controller
    {
        private readonly ILogger<TrainingScheduleController> _logger;
        private readonly ITrainingCoordinatorService _trainingCoordinatorService;
        private readonly ITrainingScheduleService _trainingScheduleService;
        private readonly ITrainingRegistrationService _trainingRegistrationService;
        private readonly ITrainingProgramService _trainingProgramService;
        private readonly ITrainingCourseService _trainingCourseService;
        private readonly GlobalService _globalService;
        private readonly IJobClassService _jobclassService;
        private readonly Dictionary<string,string> auditTrail;
        public TrainingScheduleController(ILogger<TrainingScheduleController> logger,
        ITrainingScheduleService trainingScheduleService,
        ITrainingCoordinatorService trainingCoordinatorService,
        ITrainingProgramService trainingProgramService,
        ITrainingCourseService trainingCourseService,
        ITrainingRegistrationService trainingRegistrationService,
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
            _globalService = globalService;
            _trainingCoordinatorService = trainingCoordinatorService;
            _logger = logger;         
            _jobclassService = jobclassService;   
            _trainingScheduleService = trainingScheduleService;     
            _trainingRegistrationService = trainingRegistrationService;
            _trainingProgramService = trainingProgramService;            
            _trainingCourseService = trainingCourseService;          
        }
        [ValidateAccess(ControllerName = "TrainingSchedule")]
        public IActionResult Index()
        {
            _globalService.PageVisitLog($"{RouteData.Values["controller"]}/{RouteData.Values["action"]}",auditTrail);
            return View(new TrainingScheduleViewModel());
        }
       

        [HttpGet]
        public async Task<IActionResult> GetTrainingScheduleWindow(string control,string code)
        {
            try
            {    
                TrainingSchedule trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(code);
                List<JobClass> jobclassList =  await _jobclassService.GetHRJobClassList();
                
                FormControlModel formControlModel = new FormControlModel(control);

                TrainingScheduleViewModel trainingScheduleViewModel = new TrainingScheduleViewModel(){
                    FormControl = formControlModel,                    
                    TrainingScheduleDetails = trainingSchedule ?? null,
                    JobClasses = jobclassList
                };
                
                return PartialView("~/Views/Shared/_TrainingScheduleDetails.cshtml", trainingScheduleViewModel);

            }
            catch (Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
                throw;
            }
        }      
        
        [HttpGet]
        public IActionResult GetTrainingScheduleCancellationWindow()
        {
            try
            {    
               return PartialView("~/Views/TrainingSchedule/_TrainingScheduleCancellationDetails.cshtml");
            }
            catch (System.Exception)
            {
                return BadRequest();
                throw;
            }
        }         
        
        public async Task<IActionResult> GetTrainingScheduleList([DataSourceRequest] DataSourceRequest request,
        string[] paramProgramCode,
        string[] paramCourseCode,
        string paramRegionCode,
        string paramTrainingType,
        string paramScheduleStatus
        )
        {
            try
            {
                
                List<TrainingSchedule> _list = await _trainingScheduleService.GetTrainingScheduleList();

                if (paramProgramCode.Length > 0 && !paramProgramCode.Contains("ALL"))
                    _list = _list.Where(w => paramProgramCode.Contains(w.Program.ProgramCode)).ToList();
                    
                if (paramCourseCode.Length > 0 && !paramCourseCode.Contains("ALL"))
                    _list = _list.Where(w => paramCourseCode.Contains(w.Course.CourseCode)).ToList();
                    
                if (paramRegionCode != null && paramRegionCode != "ALL")
                    _list = _list.Where(w => w.Region == paramRegionCode).ToList();
                    
                if (paramTrainingType != null && paramTrainingType != "ALL")
                    _list = _list.Where(w => w.TrainingType == paramTrainingType).ToList();
                    
                if (paramScheduleStatus != null && paramScheduleStatus != "ALL")
                    _list = _list.Where(w => w.ScheduleStatus == paramScheduleStatus).ToList();

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

        [HttpGet]
        public async Task<JsonResult> GetTrainingCourseProgramEnumList()
        {
             List<TrainingProgram> _list = new()
            {
                new TrainingProgram
                {
                    ProgramCode = "",
                    ProgramTitle = "",
                    Status = true
                }
            };
            _list.AddRange(await _trainingProgramService.GetTrainingProgramList());

            return Json(_list.Where(w=>w.Status == true).ToList());
        }


        public async Task<IActionResult> GetTrainingScheduleActivity([DataSourceRequest] DataSourceRequest request)
        {
            var scheduleList = await _trainingScheduleService.GetTrainingScheduleList();

            List<CalendarActivity> _list = new List<CalendarActivity>();
            
            foreach (var item in scheduleList.Where(x => x.ScheduleStatus == "AVAILABLE" || x.ScheduleStatus == "COMPLETED"))
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


        [HttpGet]
        public async Task<JsonResult> GetTrainingCourseEnumList(string ProgramCode)
        {
            List<TrainingCourse> _list = await _trainingCourseService.GetTrainingCourseList();
            if (ProgramCode != "")
            {
                TrainingProgram _trainingProgram = await _trainingProgramService.GetTrainingProgramByCode(ProgramCode);            
                _list = _trainingProgram.Courses.ToList();
            }
            
            return Json(_list);
        }

        public async Task<JsonResult> GetTrainingCourseDescriptionByCode(string courseCode)
        {
            TrainingCourse _details = await _trainingCourseService.GetTrainingCourseByCode(courseCode);
            
            return Json(_details == null ? null : _details.CourseDescription);
        }
        
        [HttpGet]
        public async Task<ActionResult> ValidateTime(DateTime startDate, DateTime endDate, TimeSpan startTime, TimeSpan endTime,string region,string TrainingCode = null)
        {
            try
            {
                List<TrainingSchedule> existingSchedules = await _trainingScheduleService.GetTrainingScheduleList();
                existingSchedules = existingSchedules.Where(w=> w.TrainingCode != TrainingCode && w.ScheduleStatus != "CANCELLED").ToList();

                var newRegion = region; // region of the schedule being validated

                foreach (var schedule in existingSchedules)
                {
                    DateTime scheduleStartDate = schedule.StartDate;
                    DateTime scheduleEndDate = (DateTime)schedule.EndDate;
                    TimeSpan scheduleStartTime = schedule.StartTime;
                    TimeSpan scheduleEndTime = schedule.EndTime;
                    var existingRegion = schedule.Region;

                    if (startDate.Date <= scheduleEndDate.Date && endDate.Date >= scheduleStartDate.Date)
                    {
                        // Check for overlap on each day within the range
                        for (DateTime date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                        {
                            if (date.Date >= scheduleStartDate.Date && date.Date <= scheduleEndDate.Date)
                            {
                                if ((startTime < scheduleEndTime && endTime > scheduleStartTime) ||
                                    (endTime > scheduleStartTime && startTime < scheduleEndTime))
                                {
                                    // Region-based validation
                                    bool shouldCheckOverlap = false;
                                    if (newRegion == "ALL" || existingRegion == "ALL")
                                    {
                                        // "ALL" should not overlap with any region
                                        shouldCheckOverlap = true;
                                    }
                                    else if (newRegion == existingRegion)
                                    {
                                        // Only check overlap for the same region
                                        shouldCheckOverlap = true;
                                    }

                                    if (shouldCheckOverlap)
                                    {

                                        return Ok(new { isExist = true });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost]        
        public async Task<ActionResult> AddSchedule(TrainingSchedule model)
        {
            try
            {
                List<TrainingSchedule> count = await _trainingScheduleService.GetTrainingScheduleList();
                int nextId = count.Count + 1;               

                model.Program = await _trainingProgramService.GetTrainingProgramByCode(model.Program.ProgramCode);
                model.Course = await _trainingCourseService.GetTrainingCourseByCode(model.Course.CourseCode);
                model.TrainingCode = model.Course.CourseCode +"-"+ _globalService.GetDateTime().Year.ToString() + $"-{nextId.ToString("D3")}";
                model.RegistrationStatus = "CLOSED";
                model.ScheduleStatus = "DRAFT"; 
                model.ScheduleCreatedBy = auditTrail["UserID"];
                model.ScheduleCreatedDate = _globalService.GetDateTime();
                model.CreatedBy = auditTrail["UserID"];
                model.CreatedByComputerUsed = auditTrail["HostName"];
                model.DateCreated = _globalService.GetDateTime();
                model.Status = true;
                
                if (model.AdditionalJobClasses != null)
                {
                    foreach (var item in model.AdditionalJobClasses)
                    {
                        item.Schedule = model;
                        item.Status = true;
                        item.CreatedBy = auditTrail["UserID"];
                        item.CreatedByComputerUsed = auditTrail["HostName"];
                        item.DateCreated = _globalService.GetDateTime();
                    }
                }

                // Add the new record into the database
                _trainingScheduleService.AddSchedule(model);

                _globalService.Log("Add new schedule: TrainingSchedule - " + JsonConvert.SerializeObject(model, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), auditTrail, null);
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
        public async Task<ActionResult> UpdateSchedule(TrainingSchedule model)
        {
            try
            {
                var g_dateModified = _globalService.GetDateTime();
                TrainingSchedule _details = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(model.TrainingCode);
                
                List<TrainingRegistration> traineeList = await _trainingRegistrationService.GetTraineeListByCode(model.TrainingCode);


                var registeredCount = 0;

//- Update the Training Registration Status of employees from REGISTERED or FOR CONFIRMATION to 'SCHEDULE FOR CONFIRMATION' based on the Training Code of the affected registered training of employees.
                foreach (var item in traineeList)
                {
                    if(item.TrainingRegistrationStatus == "REGISTERED")
                    {
                        registeredCount += 1;
                    }

                    if(item.TrainingRegistrationStatus == "REGISTERED" ||item.TrainingRegistrationStatus == "FOR CONFIRMATION")
                    {
                        item.TrainingRegistrationStatus = "SCHEDULE FOR CONFIRMATION";         
                        item.ModifiedBy = auditTrail["UserID"];
                        item.ModifiedByComputerUsed = auditTrail["HostName"];
                        item.DateModified = g_dateModified;
                        _trainingRegistrationService.UpdateRegistration();
                    }
                }

                var _logMsg = $"Change in schedule: TrainingSchedule ({ model.TrainingCode }) - from: '{JsonConvert.SerializeObject(_details, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}' to: '{JsonConvert.SerializeObject(model, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}'.";

                _details.StartDate = model.StartDate;
                _details.EndDate = model.EndDate;
                _details.StartTime = model.StartTime;
                _details.EndTime = model.EndTime;
                _details.ResourceSpeaker = model.ResourceSpeaker;
                _details.ClassSize = model.ClassSize;
                _details.CostPerHead = model.CostPerHead;
                _details.Venue = model.Venue;
                
                _details.ScheduleModifiedBy = auditTrail["UserID"];                
                _details.ScheduleModifiedDate = _globalService.GetDateTime();

//- if the TRAINING SCHEDULE STATUS is AVAILABLE and CLASS SIZE is greater than the number of employees registered to the Training Code, then the REGISTRATION STATUS will be set or updated to OPEN.
                if(_details.ScheduleStatus == "AVAILABLE")
                {
                    if(_details.ClassSize < registeredCount)
                        _details.RegistrationStatus = "OPEN";
                }

                // update the record
                _trainingScheduleService.UpdateSchedule();
                
                _globalService.Log(_logMsg, auditTrail, null);

                // Check if there were changes in DATE/TIME/RESOURCE SPEAKER and Training Schedule STATUS is 'Available'
                if (_details.ScheduleStatus == "AVAILABLE" &&
                    (_details.StartDate != model.StartDate ||
                    _details.EndDate != model.EndDate ||
                    _details.StartTime != model.StartTime ||
                    _details.EndTime != model.EndTime ||
                    _details.ResourceSpeaker != model.ResourceSpeaker))
                {
                    //Update in Training Schedule
                    //send email notification to all employees with Training Registration Status of REGISTERED or FOR CONFIRMATION and Training Schedule Status is AVAILABLE (changes in the schedule)                    

                    //New Schedule
                    var htmlString = $@"
%0A
                %0AThis is to inform you that there were changes in the schedule of this training, {_details.TrainingCode} - {_details.Course.CourseTitle}, that you registered.
                %0APlease login to the The Registrar System to view the changes and confirm your attendance with the new schedule.
%0A"
                    ;

                    //var htmlString = "<p>Ma'am/Sir,</p>%0D%0A"+                    
                    //$"<p>This is to inform you that there were changes in the schedule of this training, {_details.TrainingCode} - {_details.Course.CourseTitle}, that you registered.</p>"+
                    //"<p>Please login to the The Registrar System to view the changes and confirm your attendance with the new schedule.</p>"
                    //;
                    var subject = "Training - Change in Schedule";


                    var traineeEmail = traineeList.Select(e => e.EmployeeInfo.EmailAddress);


                    var _coordinatorList = await _trainingCoordinatorService.GetTrainingCoordinatorList();
                    var coordinatorEmails = _globalService.GetEmployeeList()
                                            .Join(_coordinatorList,
                                                e => e.EmployeeNo,
                                                c => c.EmployeeNo,
                                                (e, c) => e.EmailAddress)
                                            .ToList();

                    var blind_recipient = string.Join(";", traineeEmail);
                    var copy_recipient = string.Join(";", coordinatorEmails);

                    //_globalService.SendEmail(htmlString,subject,null,copy_recipient,blind_recipient);

                    string htmlBody = $@"Dear Ma'am/Sir,
                    {htmlString}                
                    %0D%0AThank you,
                    %0DTraining Registrar System";

                    // Create the mailto link
                    return Json(new
                    {
                        IsEmail = true,
                        EmailTo = "",
                        EmailCC = copy_recipient,
                        EmailBCC = blind_recipient,
                        EmailSubject = subject,
                        EmailBody = htmlBody
                    });
                }
            }            
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }

            // Return success response
            return Ok(new
            {
                IsEmail = false
            });
        }

        [HttpPost]        
        public async Task<ActionResult> CancelSchedule(string paramCode, string paramReason, string paramCancellationType)
        {
            try
            {
                TrainingSchedule _details = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(paramCode);
                

                var _logMsg = $"Set the TrainingSchedule({ paramCode }) to Canceled ";

                _details.CancellationReason = paramReason;
                _details.CancellationType = paramCancellationType;
                _details.RegistrationStatus = "CLOSED";
                _details.ScheduleStatus = "CANCELLED";

                _details.ScheduleCanceledBy = auditTrail["UserID"];                
                _details.ScheduleCanceledDate = _globalService.GetDateTime();
                


//- update the Training Registration Status of all employees registered in reference to the training code to 'Canceled'
                var traineeList = await _trainingRegistrationService.GetTraineeListByCode(_details.TrainingCode);

                foreach(var item in traineeList){
                    item.TrainingRegistrationStatus = "CANCELLED";
                    _trainingRegistrationService.UpdateRegistration();                      
                    _globalService.Log($"Set the training registration to cancelled: TrainingRegistration ({ item.RegistrationCode })", auditTrail, null);  
                }
                
                // update the record
                _trainingScheduleService.UpdateSchedule();

//- generate email notification to employee/participants who registered on the canceled scheduled training based on the Training Code (canceled schedule)                    
                //var htmlString = "<p>Ma'am/Sir,</p>%0D%0A"+                                    
                //$"<p>This is to inform you that the {_details.TrainingCode} - {_details.Course.CourseTitle} has been canceled due to this reason {paramReason}."+
                //"<p>Please login to the The Registrar System to view the details.</p>"
                //;

                var htmlString = $@"
%0A
                %0AThis is to inform you that the {_details.TrainingCode} - {_details.Course.CourseTitle} has been canceled due to this reason {paramReason}.
                %0APlease login to the The Registrar System to view the details.
%0A"
                ;

                var subject = "Training - Canceled Schedule";

                
                var traineeEmail = traineeList.Where(w => w.TrainingRegistrationStatus == "REGISTERED" 
                || w.TrainingRegistrationStatus == "FOR CONFIRMATION"
                || w.TrainingRegistrationStatus == "FOR APPROVAL"
                || w.TrainingRegistrationStatus == "SCHEDULE FOR CONFIRMATION"
                ).Select(e => e.EmployeeInfo.EmailAddress);
                    
                    
                var _coordinatorList = await _trainingCoordinatorService.GetTrainingCoordinatorList();
                var coordinatorEmails = _globalService.GetEmployeeList()
                                        .Join(_coordinatorList, 
                                            e => e.EmployeeNo, 
                                            c => c.EmployeeNo, 
                                            (e, c) => e.EmailAddress)
                                        .ToList();

                var blind_recipient = string.Join(";", traineeEmail);
                var copy_recipient = string.Join(";", coordinatorEmails);

                //_globalService.SendEmail(htmlString,subject,null,copy_recipient,blind_recipient);

                _globalService.Log(_logMsg, auditTrail, null);

                string htmlBody = $@"Dear Ma'am/Sir,
                    {htmlString}                
                    %0D%0AThank you,
                    %0DTraining Registrar System";

                // Create the mailto link
                return Json(new
                {
                    IsEmail = true,
                    EmailTo = "",
                    EmailCC = copy_recipient,
                    EmailBCC = blind_recipient,
                    EmailSubject = subject,
                    EmailBody = htmlBody
                });
            }            
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }
        }

        
        [HttpPost]        
        public async Task<ActionResult> PublishSchedule(string paramCode)
        {
            try
            {
                TrainingSchedule _details = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(paramCode);
                
                //New Schedule
                var htmlString = $@"
%0A
                %0AThis is to inform you that the {paramCode} - {_details.Course.CourseTitle} has been published.
                %0AIf you are interested to attend the said training, please login to the The Registrar System to register.
%0A"
                ;
                var subject = "Training - New Schedule";

                //var employees = from e in _globalService.GetEmployeeList()
                //    join jc in _details.Program.JobClasses on e.JobClassCode equals jc.JobClassCode
                //    where e.EmployeeStatus == "A"
                //    select e;
                    
                    
                var _coordinatorList = await _trainingCoordinatorService.GetTrainingCoordinatorList();
                var coordinatorEmails = _globalService.GetEmployeeList()
                                        .Join(_coordinatorList, 
                                            e => e.EmployeeNo, 
                                            c => c.EmployeeNo, 
                                            (e, c) => e.EmailAddress)
                                        .ToList();

                //var blind_recipient = string.Join(";", employees.Select(e => e.EmailAddress));
                var copy_recipient = string.Join(";", coordinatorEmails);


                var _logMsg = $"Set the training schedule to published: TrainingSchedule ({paramCode})";

                _details.RegistrationStatus = "OPEN";
                _details.ScheduleStatus = "AVAILABLE";

                // update the record
                _trainingScheduleService.UpdateSchedule();

                _globalService.Log(_logMsg, auditTrail, null);

                string htmlBody = $@"Dear Ma'am/Sir,
                {htmlString}                
                %0D%0AThank you,
                %0D%0ATraining Registrar System";

                // Create the mailto link
                return Json(new
                {
                    EmailTo = "",
                    EmailCC = copy_recipient,
                    EmailBCC = "",
                    EmailSubject = subject,
                    EmailBody = htmlBody
                });
            }            
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }

        }

       
        public async Task<JsonResult> GetHRJobClassList(string ProgramCode)
        {
            List<JobClass> _list = await _jobclassService.GetHRJobClassList();
            if (ProgramCode != null)
            {
                TrainingProgram _detail = await _trainingProgramService.GetTrainingProgramByCode(ProgramCode);
                _list = _list.Except(_list.Where(x => _detail.JobClasses.Any(y => y.JobClassCode == x.JobClassCode))).ToList();    
            }            
            return Json(_list);
        }


        public async Task<JsonResult> GetHRRegionList()
        {
            List<VwHrRegion> _list = new()
            {
                new VwHrRegion
                {
                    Region = "ALL"
                }
            };
            
            _list.AddRange(await _trainingScheduleService.GetHRRegionList());

            return Json(_list);
        }

        public async Task<JsonResult> GetHRRegionList_Entry()
        {
             List<VwHrRegion> _list = new()
            {
                new VwHrRegion
                {
                    Region = ""
                }
            };

            _list.AddRange(await _trainingScheduleService.GetHRRegionList());

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