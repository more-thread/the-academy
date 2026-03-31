using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Graph;
using Newtonsoft.Json;
using NuGet.Packaging.Licenses;
using NuGet.Protocol;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using TRS.Attributes;
using TRS.Global;
using TRS.Interfaces;
using TRS.Models;
using TRS.ViewModels;

namespace TRS.Controllers
{
    [ValidateSession]
    public class TrainingProgramAndCourseController : Controller
    {
        private readonly ILogger<TrainingProgramAndCourseController> _logger;
        private readonly  ITrainingProgramService _trainingProgramService;
        private readonly  ITrainingCourseService _trainingCourseService;
        private readonly IJobClassService _jobclassService;
        private readonly GlobalService _globalService;
        private readonly Dictionary<string,string> auditTrail;

        public TrainingProgramAndCourseController(ILogger<TrainingProgramAndCourseController> logger,
        ITrainingProgramService trainingProgramService,
        ITrainingCourseService trainingCourseService,
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
            _trainingProgramService = trainingProgramService;            
            _trainingCourseService = trainingCourseService;            
            _jobclassService = jobclassService;   
            _logger = logger;
        }
        [ValidateAccess(ControllerName = "TrainingProgramAndCourse")]
        public IActionResult Index()
        {            
            _globalService.PageVisitLog($"{RouteData.Values["controller"]}/{RouteData.Values["action"]}",auditTrail);
            return View(new TrainingProgramAndCourseViewModel());
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

#region "TRAINING PROGRAM REGION"
        public async Task<IActionResult> GetTrainingProgramList([DataSourceRequest] DataSourceRequest request,
        string[] JobClassCodeFilterArr,
        string paramProgramCode)
        {
            var _list = await _trainingProgramService.GetTrainingProgramList();      
                   
            if (JobClassCodeFilterArr.Length > 0 && !JobClassCodeFilterArr.Contains("ALL"))
                _list = _list.Where(w => w.JobClasses.Any(ac => JobClassCodeFilterArr.Contains(ac.JobClassCode))).ToList();

            if (paramProgramCode != null && paramProgramCode != "ALL")
                _list = _list.Where(w => w.ProgramCode == paramProgramCode).ToList();

            if(JobClassCodeFilterArr.Length == 0 && paramProgramCode == null)
                _list = new List<TrainingProgram>();

            DataSourceResult result = _list.ToDataSourceResult(request);
            var res = JsonConvert.SerializeObject(result, Formatting.None,
                        new JsonSerializerSettings()
                        { 
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
            return Content(res, "application/json");            
        }

        public async Task<JsonResult> GetHRJobClassList()
        {
            List<JobClass> _list = new()
            {
                new JobClass
                {
                    JobClassCode = "ALL",
                    JobClassDescription = "ALL",
                }
            };
            _list.AddRange(await _jobclassService.GetHRJobClassList());            
            return Json(_list);
        }

        public async Task<JsonResult> GetTrainingProgramEnumList()
        {
            List<TrainingProgram> _list = new()
            {
                new TrainingProgram
                {
                    ProgramCode = "ALL",
                    ProgramTitle = "ALL",
                }
            };
            _list.AddRange(await _trainingProgramService.GetTrainingProgramList());

            return Json(_list);
        }
        
        public async Task<JsonResult> GetTrainingProgramJobClassesByCode(string paramProgramCode)
        {
            TrainingProgram _trainingDetails = await _trainingProgramService.GetTrainingProgramByCode(paramProgramCode);
            
            return Json(_trainingDetails == null ? null : _trainingDetails.JobClasses);
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainingProgramDetails(string control, string code)
        {
            try
            {
                FormControlModel formControlModel = new FormControlModel(control);
                List<JobClass> jobclassList =  await _jobclassService.GetHRJobClassList();
                TrainingProgram programDetails = await _trainingProgramService.GetTrainingProgramByCode(code);

                TrainingProgramAndCourseViewModel trainingProgramAndCourseViewModel = new TrainingProgramAndCourseViewModel(){                
                    TrainingProgram = programDetails,
                    FormControl = formControlModel,
                    JobClasses = jobclassList
                };
                
                return PartialView("~/Views/TrainingProgramAndCourse/_TrainingProgramDetails.cshtml", trainingProgramAndCourseViewModel);
            }
            catch (System.Exception)
            {
                return BadRequest();
                throw;
            }
        }        
        [HttpGet]
        public async Task<ActionResult> CheckIfProgramTitleExist(string programTitle){            
            //check if program title exists in database
            var programList = await _trainingProgramService.GetTrainingProgramList();
            
            if(programList.Any(w => w.ProgramTitle == programTitle))
            {
                return Ok(new {isExist = true }); 
            }
            return Ok(new {isExist = false}); 
        }

        [HttpPost]        
        public async Task<ActionResult> AddProgram(TrainingProgram model)
        {
            try
            {
                List<TrainingProgram> count = await _trainingProgramService.GetTrainingProgramList();
                int nextId = count.Count + 1;               

                model.ProgramCode = $"PROG-{nextId.ToString("D3")}";
                model.CreatedBy = auditTrail["UserID"];
                model.CreatedByComputerUsed = auditTrail["HostName"];
                model.DateCreated = _globalService.GetDateTime();
                model.Status = true;

                foreach (var item in model.JobClasses)
                {
                    item.Program = model;
                    item.Status = true;
                    item.CreatedBy = auditTrail["UserID"];
                    item.CreatedByComputerUsed = auditTrail["HostName"];
                    item.DateCreated = _globalService.GetDateTime();
                }

                // Add the new record into the database
                _trainingProgramService.AddProgram(model);

                _globalService.Log("Add new program: TrainingProgram - " + JsonConvert.SerializeObject(model, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), auditTrail, null);
            }
            catch (System.Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, null);
                return BadRequest(ex.Message);
            }

            // Return success response
            return Ok();
        }

        [HttpPost]        
        public async Task<ActionResult> UpdateProgram(TrainingProgram model)
        {
            try
            {
                TrainingProgram _programDetails = await _trainingProgramService.GetTrainingProgramByCode( model.ProgramCode);                

                var _logMsg = $"Set the title from '{_programDetails.ProgramTitle}' to '{model.ProgramTitle}': TrainingProgram ({ model.ProgramCode })";

                _programDetails.ProgramTitle = model.ProgramTitle;
                _programDetails.ModifiedBy = auditTrail["UserID"];
                _programDetails.ModifiedByComputerUsed = auditTrail["HostName"];
                _programDetails.DateModified = _globalService.GetDateTime();
                
                // update the record
                _trainingProgramService.UpdateProgram();
                
                _globalService.Log(_logMsg, auditTrail, null);
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
        public async Task<ActionResult> SetTrainingProgramStatus(string paramCode, string paramStatus)
        {
            try
            {
                TrainingProgram _programDetails = await _trainingProgramService.GetTrainingProgramByCode(paramCode);               

                if(_programDetails.Courses.Count > 0){
                    var _courseDetals = _programDetails.Courses.Find(w => w.Program.ProgramCode == paramCode);
                    _courseDetals.Status =  paramStatus == "Active"? true: false;
                    _courseDetals.ModifiedBy = auditTrail["UserID"];
                    _courseDetals.ModifiedByComputerUsed = auditTrail["HostName"];
                    _courseDetals.DateModified = _globalService.GetDateTime();  
                    _globalService.Log($"Set the Status to {paramStatus}: TrainingCourse ({_courseDetals.CourseCode})",auditTrail,null);                              
                }

                _programDetails.Status = paramStatus == "Active"? true: false;
                _programDetails.ModifiedBy = auditTrail["UserID"];
                _programDetails.ModifiedByComputerUsed = auditTrail["HostName"];
                _programDetails.DateModified = _globalService.GetDateTime();                                
                
                // update the record
                _trainingProgramService.UpdateProgram();

                _globalService.Log($"Set the Status to {paramStatus}: TrainingProgram({paramCode})",auditTrail,null);
            }            
            catch (System.Exception ex)
            {                 
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}",auditTrail,ex);
                return BadRequest(ex.Message);
            }

            // Return success response
            return Ok();
        }
#endregion
        
#region "TRAINING COURSE REGION"
        public async Task<IActionResult> GetTrainingCourseList([DataSourceRequest] DataSourceRequest request,
        string paramProgramCode,
        string paramCourseCode)
        {
            try
            {
                var _list = await _trainingCourseService.GetTrainingCourseList();

                if (paramProgramCode != null && paramProgramCode != "ALL")
                    _list = _list.Where(w => w.Program.ProgramCode == paramProgramCode).ToList();

                if (paramCourseCode != null && paramCourseCode != "ALL")
                    _list = _list.Where(w => w.CourseCode == paramCourseCode).ToList();

                if (paramCourseCode == null && paramProgramCode == null)
                    _list = new List<TrainingCourse>();

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
                return BadRequest(ex.Message);
                throw;
            }
            
        }
        
        public async Task<JsonResult> GetTrainingCourseProgramEnumList()
        {
             List<TrainingProgram> _list = new()
            {
                new TrainingProgram
                {
                    ProgramCode = "",
                    ProgramTitle = "",
                }
            };
            _list.AddRange(await _trainingProgramService.GetTrainingProgramList());

            return Json(_list.Where(w => w.Status == true).ToList());
        }

        public async Task<JsonResult> GetTrainingCourseEnumList()
        {
            List<TrainingCourse> _list = new()
            {
                new TrainingCourse
                {
                    CourseCode = "ALL",
                    CourseTitle = "ALL",
                }
            };
            
            _list.AddRange(await _trainingCourseService.GetTrainingCourseList());

            return Json(_list);
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainingCourseDetails(string control, string code)
        {
            try
            {
                
                TrainingCourse trainingCourse =  await _trainingCourseService.GetTrainingCourseByCode(code);
                TrainingProgram programDetails = null;
                if(trainingCourse != null)
                programDetails = await _trainingProgramService.GetTrainingProgramByCode(trainingCourse.Program.ProgramCode);

                List<JobClass> jobclassList =  await _jobclassService.GetHRJobClassList();
                TrainingProgramAndCourseViewModel trainingProgramAndCourseViewModel = new TrainingProgramAndCourseViewModel(){
                    FormControl = new FormControlModel(control),
                    TrainingProgram = programDetails,
                    JobClasses = jobclassList,
                    TrainingCourse = trainingCourse
                };

                return PartialView("~/Views/TrainingProgramAndCourse/_TrainingCourseDetails.cshtml", trainingProgramAndCourseViewModel);                
            }
            catch (Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
                throw;
            }
            
        }

        [HttpPost]        
        public async Task<ActionResult> AddCourse(TrainingCourse model)
        {
            try
            {
                List<TrainingCourse> count = await _trainingCourseService.GetTrainingCourseList();
                int nextId = count.Count + 1;               

                model.CourseCode = model.Program.ProgramCode +"-"+ nextId.ToString("D3");                
                model.CreatedBy = auditTrail["UserID"];
                model.CreatedByComputerUsed = auditTrail["HostName"];
                model.DateCreated = _globalService.GetDateTime();
                model.Status = true;               
                model.Program = await _trainingProgramService.GetTrainingProgramByCode(model.Program.ProgramCode);
                
                // Add the new record into the database
                _trainingCourseService.AddCourse(model);

                _globalService.Log("Add new course: TrainingCourse - " + JsonConvert.SerializeObject(model, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), auditTrail, null);
            }
            catch (Exception ex)
            {
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail, ex);
                return BadRequest(ex.Message);
            }

            // Return success response
            return Ok();
        }
        [HttpPost]        
        public async Task<ActionResult> UpdateCourse(TrainingCourse model)
        {
            try
            {
                TrainingCourse _courseDetails = await _trainingCourseService.GetTrainingCourseByCode(model.CourseCode);
                

                var _logMsg = $"Update course: TrainingCourse ({ model.CourseCode }) from '{JsonConvert.SerializeObject(_courseDetails, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}' to '{JsonConvert.SerializeObject(model, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })}'.";

                _courseDetails.CourseTitle = model.CourseTitle;
                _courseDetails.CourseDescription = model.CourseDescription;
                _courseDetails.WithPreTest = model.WithPreTest;
                _courseDetails.PreTestTotalScore = model.PreTestTotalScore;
                _courseDetails.WithPostTest = model.WithPostTest;
                _courseDetails.PostTestTotalScore = model.PostTestTotalScore;
                _courseDetails.WithEvaluation = model.WithEvaluation;

                _courseDetails.ModifiedBy = auditTrail["UserID"];
                _courseDetails.ModifiedByComputerUsed = auditTrail["HostName"];
                _courseDetails.DateModified = _globalService.GetDateTime();
                
                // update the record
                _trainingCourseService.UpdateCourse();
                
                _globalService.Log(_logMsg, auditTrail, null);
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
        public async Task<ActionResult> CheckIfCourseTitleExist(string courseTitle){
            //check if program title exists in database
            var programList = await _trainingCourseService.GetTrainingCourseList();
            
            if(programList.Any(w => w.CourseTitle == courseTitle))
            {
                return Ok(new {isExist = true }); 
            }
            return Ok(new {isExist = false}); 
        }

        [HttpPost]        
        public async Task<ActionResult> SetTrainingCourseStatus(string paramCode, string paramStatus)
        {
            try
            {
                TrainingCourse _courseDetails = await _trainingCourseService.GetTrainingCourseByCode(paramCode);
                

                _courseDetails.Status = paramStatus == "Active"? true: false;
                _courseDetails.ModifiedBy = auditTrail["UserID"];
                _courseDetails.ModifiedByComputerUsed = auditTrail["HostName"];
                _courseDetails.DateModified = _globalService.GetDateTime();                                
                
                // update the record
                _trainingCourseService.UpdateCourse();

                _globalService.Log($"Set the Status to {paramStatus}: TrainingCourse ({paramCode})",auditTrail,null);
            }            
            catch (System.Exception ex)
            {                 
                _globalService.Log($"Error: {RouteData.Values["controller"]}/{RouteData.Values["action"]}",auditTrail,ex);
                return BadRequest(ex.Message);
            }

            // Return success response
            return Ok();
        }
#endregion

    }
}
