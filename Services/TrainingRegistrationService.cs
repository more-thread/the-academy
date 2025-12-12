
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using TRS.Data;
using TRS.Interfaces;
using TRS.Models;

namespace TRS.Services
{
    public class TrainingRegistrationService : ITrainingRegistrationService
    {
        //todo add database context
        private readonly AppDBContext _context;
        public TrainingRegistrationService(AppDBContext context)
        {
            _context = context;
        }
        public async Task<TrainingRegistration> GetTrainingRegistrationByCode(string code)
        {
            try
            {
                var result = await _context.tTrainingRegistration.Where(w => w.RegistrationCode == code)                
                        .Include(c => c.TrainingSchedule)                        
                        .Include(c => c.TrainingSchedule.Program)
                        .Include(c => c.TrainingSchedule.Course)  
                        .Include(c => c.TrainingSchedule.Program.JobClasses) 
                        .Include(c => c.TrainingSchedule.AdditionalJobClasses)
                        .FirstOrDefaultAsync();

                            
                // Fetch the VwHrEmployeeInfo list
                // var employeeInfos = _context.VwHrEmployeeInfos.Where(w => w.EmployeeNo ==  result.EmployeeNo).FirstOrDefault();
               
                result.EmployeeInfo = _context.VwHrEmployeeInfos.Where(w => w.EmployeeNo ==  result.EmployeeNo).FirstOrDefault();

                return result;    
            }
            catch (System.Exception)
            {
                throw;
            }
            
        }
        
        public async Task<List<TrainingRegistration>> GetTrainingRegistrationList()
        {             
           // Fetch the TrainingRegistration list with related data
            var registrations = await _context.tTrainingRegistration
                        .Include(c => c.TrainingSchedule)                        
                        .Include(c => c.TrainingSchedule.Program)
                        .Include(c => c.TrainingSchedule.Course)                
                        .Include(c => c.TrainingSchedule.Program.JobClasses)       
                        .Include(c => c.TrainingSchedule.AdditionalJobClasses)
                        .ToListAsync();            

            // Fetch the VwHrEmployeeInfo list
            var employeeInfos = await _context.VwHrEmployeeInfos.ToListAsync();

            // Perform the join in memory and assign EmployeeInfo to each TrainingRegistration
            foreach (var reg in registrations)
            {
                reg.EmployeeInfo = employeeInfos.FirstOrDefault(emp => emp.EmployeeNo == reg.EmployeeNo);
            }

            return registrations;
        }
        public async Task<List<VwHrRegion>> GetHRRegionList()
        {             
            var result = _context.VwHrRegions.GroupBy(r => r.Region)
                .Select(g => g.First())
                .ToListAsync();            
            return await result;
        }        
        
        public async Task<List<TrainingRegistration>> GetTraineeListByCode(string code)
        {                       
            var result = _context.tTrainingRegistration.Where(w => w.TrainingSchedule.TrainingCode == code)
            .Join(_context.VwHrEmployeeInfos, tc => tc.EmployeeNo, ei => ei.EmployeeNo, (tc, ei) => new TrainingRegistration
            {   
                TrainingRegistrationStatus = tc.TrainingRegistrationStatus,
                TrainingCompletionStatus = tc.TrainingCompletionStatus,
                Attendance = tc.Attendance,
                AbsenceReason = tc.AbsenceReason,
                PostTestFirstScore = tc.PostTestFirstScore,
                PostTestSecondScore = tc.PostTestSecondScore,
                PostTestThirdScore = tc.PostTestThirdScore,
                EvaluationScore = tc.EvaluationScore,
                PostTestStatus = tc.PostTestStatus,
                RegistrationCode = tc.RegistrationCode,  
                RegistrationCreatedBy = tc.RegistrationCreatedBy,
                RegistrationCreatedDate = tc.RegistrationCreatedDate,
                TrainingFeedbackStatus = tc.TrainingFeedbackStatus,               
                TrainingSchedule = new TrainingSchedule(){
                    ScheduleStatus = tc.TrainingSchedule.ScheduleStatus,
                    TrainingCode =  tc.TrainingSchedule.TrainingCode,
                    Course = new TrainingCourse(){
                        CourseCode = tc.TrainingSchedule.Course.CourseCode,
                        PostTestTotalScore = tc.TrainingSchedule.Course.PostTestTotalScore,                        
                        WithPostTest = tc.TrainingSchedule.Course.WithPostTest,                        
                        WithEvaluation = tc.TrainingSchedule.Course.WithEvaluation
                    }
                },
                EmployeeInfo = ei
            });
            return await result.ToListAsync();
        }

        public async Task<List<TrainingRegistration>> GetTrainingListByEmployeeNo(string employeeNo)
        {                       
            var result = _context.tTrainingRegistration.Where(w => w.EmployeeNo == employeeNo)
            .Join(_context.VwHrEmployeeInfos, tc => tc.EmployeeNo, ei => ei.EmployeeNo, (tc, ei) => new TrainingRegistration
            {   
                TrainingRegistrationStatus = tc.TrainingRegistrationStatus,
                TrainingCompletionStatus = tc.TrainingCompletionStatus,
                Attendance = tc.Attendance,
                AbsenceReason = tc.AbsenceReason,
                PostTestFirstScore = tc.PostTestFirstScore,
                PostTestSecondScore = tc.PostTestSecondScore,
                PostTestThirdScore = tc.PostTestThirdScore,
                EvaluationScore = tc.EvaluationScore,
                PostTestStatus = tc.PostTestStatus,
                RegistrationCode = tc.RegistrationCode,     
                TrainingFeedbackStatus = tc.TrainingFeedbackStatus,               
                TrainingSchedule = new TrainingSchedule(){
                    ScheduleStatus = tc.TrainingSchedule.ScheduleStatus,
                    TrainingCode =  tc.TrainingSchedule.TrainingCode,
                    Course =  tc.TrainingSchedule.Course,
                    Program = tc.TrainingSchedule.Program,
                    StartTime = tc.TrainingSchedule.StartTime,
                    EndTime = tc.TrainingSchedule.EndTime,
                    StartDate = tc.TrainingSchedule.StartDate,
                    EndDate = tc.TrainingSchedule.EndDate
                }
            });
            return await result.ToListAsync();
        }

        public bool AddTrainee(TrainingRegistration model)
        {
             _context.Add(model);
            return _context.SaveChanges() > 0;
        }
        
        public bool UpdateRegistration()
        {       
            return _context.SaveChanges() > 0;
        }
    }   
}