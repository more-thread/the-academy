using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Packaging.Licenses;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Diagnostics;
using TRS.Attributes;
using System.IO;
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
        private readonly ITrainingFeedbackService _trainingFeedbackService;
        private readonly IJobClassService _jobclassService;
        private readonly GlobalService _globalService;
        private readonly Dictionary<string,string> auditTrail;
        public TrainingMonitoringController(ILogger<TrainingMonitoringController> logger,
        ITrainingRegistrationService trainingRegistrationService,
        IJobClassService jobclassService,
        ITrainingScheduleService trainingScheduleService,
        ITrainingCoordinatorService trainingCoordinatorService,
        ITrainingFeedbackService trainingFeedbackService,
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
            _trainingFeedbackService = trainingFeedbackService;
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

        [HttpPost]
        public async Task<ActionResult> ExportFeedbackExcel(string paramTrainingCode)
        {
            string handle = Guid.NewGuid().ToString();

            try
            {
                // Validate input parameter
                if (string.IsNullOrEmpty(paramTrainingCode))
                {
                    return BadRequest("Training code is required.");
                }

                // Debug log
                _globalService.Log($"ExportFeedbackExcel: Starting export for training code: {paramTrainingCode}", auditTrail, null);

                TrainingSchedule _schedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(paramTrainingCode);
                if (_schedule == null)
                {
                    return BadRequest("Training schedule not found.");
                }

                List<TrainingRegistration> _trainees = await _trainingRegistrationService.GetTraineeListByCode(paramTrainingCode);
                List<TrainingFeedbackQuestions> feedbackQuestions = await _trainingFeedbackService.GetTrainingFeedbackQuestionsList();
                feedbackQuestions = feedbackQuestions?.Where(w => w.Category != "Comments").ToList() ?? new List<TrainingFeedbackQuestions>();

                Dictionary<string, Dictionary<string, string>> feedbackAnswers = await _trainingFeedbackService.GetTrainingFeedbackAnswersByScheduleCode(paramTrainingCode);
                feedbackAnswers = feedbackAnswers ?? new Dictionary<string, Dictionary<string, string>>();

                // Remove the EmployeeNo filter that was causing the issue
                var registeredTrainees = _trainees?.Where(w => w.TrainingRegistrationStatus == "REGISTERED" && w.EmployeeInfo != null)
                    .OrderBy(s => s.EmployeeInfo.EmployeeName).ToList() ?? new List<TrainingRegistration>();

                // Debug logging
                _globalService.Log($"ExportFeedbackExcel: Found {registeredTrainees.Count} trainees, {feedbackQuestions.Count} questions, {feedbackAnswers.Count} employees with answers", auditTrail, null);

                MemoryStream stream = new MemoryStream();

                using (var package = new ExcelPackage(stream))
                {
                    var workSheet = package.Workbook.Worksheets.Add("Training Feedback");

                    // Create headers starting from row 1
                    var staticHeaders = new List<string> { "EMPLOYEE ID", "EMPLOYEE NAME", "DEPARTMENT", "TRAINING FEEDBACK STATUS" };
                    var currentCol = 1;

                    // Add static headers
                    foreach (var header in staticHeaders)
                    {
                        workSheet.Cells[1, currentCol].Value = header;
                        workSheet.Cells[1, currentCol].Style.Font.Bold = true;
                        workSheet.Cells[1, currentCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        currentCol++;
                    }

                    // Add question headers as "Category.Question"
                    var questionColumns = new Dictionary<string, int>();
                    var sortedQuestions = feedbackQuestions
                        .Where(q => !string.IsNullOrEmpty(q.Category) && !string.IsNullOrEmpty(q.QuestionID))
                        .OrderBy(q => q.CategorySequence)
                        .ThenBy(q => q.QuestionSequence)
                        .ToList();

                    _globalService.Log($"ExportFeedbackExcel: Processing {sortedQuestions.Count} sorted questions", auditTrail, null);

                    foreach (var question in sortedQuestions)
                    {
                        var headerText = $"{question.Category.ToUpper()}.{question.Question}";

                        workSheet.Cells[1, currentCol].Value = headerText;
                        workSheet.Cells[1, currentCol].Style.Font.Bold = true;
                        workSheet.Cells[1, currentCol].Style.WrapText = true;
                        workSheet.Cells[1, currentCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Column(currentCol).Width = 30;
                        questionColumns[question.QuestionID] = currentCol;
                        currentCol++;
                    }

                    // Add data rows starting from row 2
                    var rowIndex = 2;
                    var employeesWithData = 0;
                    var totalAnswersSet = 0;

                    foreach (var trainee in registeredTrainees)
                    {
                        // Add employee info
                        workSheet.Cells[rowIndex, 1].Value = trainee.EmployeeInfo?.EmployeeNo ?? trainee.EmployeeNo ?? "N/A";
                        workSheet.Cells[rowIndex, 2].Value = trainee.EmployeeInfo?.EmployeeName ?? "N/A";
                        workSheet.Cells[rowIndex, 3].Value = trainee.EmployeeInfo?.DepartmentName ?? "N/A";
                        workSheet.Cells[rowIndex, 4].Value = trainee.TrainingFeedbackStatus ?? "N/A";

                        // Add feedback answers
                        var employeeNo = trainee.EmployeeNo ?? trainee.EmployeeInfo?.EmployeeNo;

                        if (!string.IsNullOrEmpty(employeeNo) && feedbackAnswers.ContainsKey(employeeNo))
                        {
                            employeesWithData++;
                            var employeeAnswers = feedbackAnswers[employeeNo];

                            if (employeeAnswers != null)
                            {
                                foreach (var question in sortedQuestions)
                                {
                                    if (!string.IsNullOrEmpty(question.QuestionID) &&
                                        questionColumns.ContainsKey(question.QuestionID) &&
                                        employeeAnswers.ContainsKey(question.QuestionID))
                                    {
                                        var col = questionColumns[question.QuestionID];
                                        var answer = employeeAnswers[question.QuestionID] ?? "";
                                        workSheet.Cells[rowIndex, col].Value = answer;
                                        totalAnswersSet++;
                                    }
                                }
                            }
                        }

                        rowIndex++;
                    }

                    // Debug logging
                    _globalService.Log($"ExportFeedbackExcel: Processed {rowIndex - 2} employees, {employeesWithData} had feedback data, {totalAnswersSet} answers set", auditTrail, null);

                    // Styling
                    workSheet.Cells[1, 1, 1, currentCol - 1].Style.Font.Bold = true;
                    workSheet.Cells[1, 1, 1, currentCol - 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    // Auto fit columns for static headers
                    for (int i = 1; i <= 4; i++)
                    {
                        workSheet.Column(i).AutoFit();
                    }

                    // Set row height for header
                    workSheet.Row(1).Height = 60;

                    // Add AutoFilter
                    if (rowIndex > 2)
                    {
                        workSheet.Cells[1, 1, rowIndex - 1, Math.Max(currentCol - 1, 4)].AutoFilter = true;
                    }

                    // Freeze panes
                    workSheet.View.FreezePanes(2, 5);

                    package.SaveAs(stream);
                }

                stream.Position = 0;
                var filePath = Path.Combine(Path.GetTempPath(), handle + ".xlsx");
                System.IO.File.WriteAllBytes(filePath, stream.ToArray());

                _globalService.Log($"ExportFeedbackExcel: Successfully generated Excel file", auditTrail, null);

                return new JsonResult(new
                {
                    FileGuid = handle,
                    FileName = "Training Feedback Report - " + paramTrainingCode + " " + DateTime.Now.ToString("yyyyMMdd") + ".xlsx"
                });
            }
            catch (Exception ex)
            {
                _globalService.Log($"Error in ExportFeedbackExcel: {ex.Message}", auditTrail, ex);
                return BadRequest($"Export failed: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainingFeedbackDetails(string code)
        {
            try
            {
                TrainingSchedule trainingSchedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(code);
                List<TrainingRegistration> trainingRegistrations = await _trainingRegistrationService.GetTraineeListByCode(trainingSchedule.TrainingCode);                
                List<TrainingFeedbackQuestions> feedbackQuestions = await _trainingFeedbackService.GetTrainingFeedbackQuestionsList();
                feedbackQuestions = feedbackQuestions.Where(static w => w.Category != "Comments").ToList();
                Dictionary<string, Dictionary<string, string>> feedbackAnswers = await _trainingFeedbackService.GetTrainingFeedbackAnswersByScheduleCode(code)
                    ?? new Dictionary<string, Dictionary<string, string>>();

                _globalService.Log($"GetTrainingFeedbackDetails: code={code}, employeesWithAnswers={feedbackAnswers.Count}", auditTrail, null);

                TraineeRegistrationViewModel trainingScheduleViewModel = new TraineeRegistrationViewModel()
                {
                    TrainingScheduleDetails = trainingSchedule ?? null,
                    TraineeList = trainingRegistrations, // Use TraineeList instead of TrainingFeedbackList
                    TrainingFeedbackQuestions = feedbackQuestions
                };

                ViewBag.FeedbackAnswers = feedbackAnswers; // Pass answers through ViewBag

                return PartialView("~/Views/TrainingMonitoring/_TrainingFeedbackDetails.cshtml", trainingScheduleViewModel);
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

                            if (trainee.TrainingSchedule.Course.WithPostTest)
                            {
                                if (
                                    traineeRegistration.PostTestFirstScorePercentage >= .80 ||
                                    traineeRegistration.PostTestSecondScorePercentage >= .80 ||
                                    traineeRegistration.PostTestThirdScorePercentage >= .80
                                )
                                    traineeRegistration.PostTestStatus = "PASSED";
                                else
                                    traineeRegistration.PostTestStatus = "FAILED";

                            }

                            //Training Feedback Status
                            if (traineeRegistration.TrainingFeedbackStatus != "COMPLETE" || traineeRegistration.TrainingFeedbackStatus == null)
                                traineeRegistration.TrainingFeedbackStatus = "INCOMPLETE";


                            //Training Completion Status
                            if (
                                (trainee.Attendance == "PRESENT" || trainee.Attendance == "PARTIAL") &&
                                ((trainee.TrainingSchedule.Course.WithPostTest && traineeRegistration.PostTestStatus == "PASSED")
                                || !trainee.TrainingSchedule.Course.WithPostTest
                                ) &&
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

                var traineeList = await _trainingRegistrationService.GetTraineeListByCode(_schedule.TrainingCode);

                var traineeEmail = traineeList.Where(w => w.Attendance != "ABSENT").Select(e => e.EmployeeInfo.EmailAddress);
                        
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

        [HttpPost]
        public async Task<ActionResult> ExportExcel(string paramTrainingCode)
        {
            string handle;
            handle = Guid.NewGuid().ToString();

            TrainingSchedule _schedule = await _trainingScheduleService.GetTrainingScheduleDetailsByCode(paramTrainingCode);
            List<TrainingRegistration> _trainees = await _trainingRegistrationService.GetTraineeListByCode(paramTrainingCode);
            var _attendees = _trainees.Where(w => w.Attendance is "PRESENT" or "PARTIAL" or "ABSENT" ).ToList();

            MemoryStream stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var lastRow = 7; //constant initial row after column header

                var workSheet = package.Workbook.Worksheets.Add("Trainees");

                var headerTitles = new List<string> { "EMPLOYEE NO.", "EMPLOYEE NAME", "ATTENDANCE", "REASON FOR ABSENCE/PARTIAL", "1", "%", "2", "%", "3", "%", 
                                                      "POST TEST STATUS", "EVALUATION SCORE", "TRAINING FEEDBACK STATUS" };

                //File Info and Column headers
                workSheet.Cells[1, 1].Value = "Program & Course: " + _schedule.Program.ProgramTitle + " - " + _schedule.Course.CourseTitle;
                workSheet.Cells[2, 1].Value = "Training Code: " + _schedule.TrainingCode;
                workSheet.Cells[3, 1].Value = "Date: " + _schedule.StartDate.ToString("yyyy/MM/dd") + " - " + _schedule.EndDate.ToString("yyyy/MM/dd");
                workSheet.Cells[4, 1].Value = "Time: " + DateTime.Today.Add(_schedule.StartTime).ToString("hh:mm tt") + " - " + DateTime.Today.Add(_schedule.EndTime).ToString("hh:mm tt");

                var i = 0;
                foreach (var header in headerTitles)
                {
                    if (i < 4 || i > 9)
                    {
                        var headerCell = workSheet.Cells[7, i+1];
                        headerCell.Value = headerTitles[i];
                    }
                    else
                    {
                        var headerCellScore = workSheet.Cells[6, 5, 6, 10];
                        headerCellScore.Merge = true;
                        headerCellScore.Value = "POST TEST SCORE";
                        headerCellScore.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        var cellScore = workSheet.Cells[7, i+1];
                        cellScore.Value = headerTitles[i];
                    }
                    i++;
                }
                i = 0;

                //trainees
                foreach (var attendee in _attendees)
                {
                    var nextRow = lastRow + 1;
                    workSheet.Cells[nextRow, 1].Value = attendee.EmployeeInfo.EmployeeNo;
                    workSheet.Cells[nextRow, 2].Value = attendee.EmployeeInfo.EmployeeName;
                    workSheet.Cells[nextRow, 3].Value = attendee.Attendance;
                    workSheet.Cells[nextRow, 4].Value = attendee.AbsenceReason;
                    workSheet.Cells[nextRow, 5].Value = attendee.PostTestFirstScore;
                    workSheet.Cells[nextRow, 6].Value = attendee.PostTestFirstScorePercentage;
                    workSheet.Cells[nextRow, 7].Value = attendee.PostTestSecondScore;
                    workSheet.Cells[nextRow, 8].Value = attendee.PostTestSecondScorePercentage;
                    workSheet.Cells[nextRow, 9].Value = attendee.PostTestThirdScore;
                    workSheet.Cells[nextRow, 10].Value = attendee.PostTestThirdScorePercentage;
                    workSheet.Cells[nextRow, 11].Value = attendee.PostTestStatus;
                    workSheet.Cells[nextRow, 12].Value = attendee.EvaluationScore;
                    workSheet.Cells[nextRow, 13].Value = attendee.TrainingFeedbackStatus;

                    lastRow++;
                }

                //styling
                workSheet.Cells[1, 1, 4, 1].Style.Font.Bold = true;
                workSheet.Cells[7, 1, 7, headerTitles.Count()].Style.Font.Bold = true;

                //workSheet.Cells[7, 5, lastRow, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //workSheet.Cells[7, 7, lastRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                //workSheet.Cells[7, 9, lastRow, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                //workSheet.Cells[7, 6, lastRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                //workSheet.Cells[7, 8, lastRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                //workSheet.Cells[7, 10, lastRow, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                workSheet.Cells[7, 1, 7, headerTitles.Count()].AutoFilter = true;

                workSheet.Cells[5, 1, workSheet.Cells.End.Row, workSheet.Cells.End.Column].AutoFitColumns();

                workSheet.View.FreezePanes(8, 1);

                package.SaveAs(stream);
            }

            stream.Position = 0;

            var filePath = Path.Combine(Path.GetTempPath(), handle + ".xlsx");
            System.IO.File.WriteAllBytes(filePath, stream.ToArray());

            return new JsonResult(new
            {
                FileGuid = handle,
                FileName = "Training Monitoring - "+ paramTrainingCode + " " + DateTime.Now.ToString("yyyyMMdd") + ".xlsx"
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