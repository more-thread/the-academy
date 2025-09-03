
using Microsoft.EntityFrameworkCore;
using TRS.Data;
using TRS.Interfaces;
using TRS.Models;

namespace TRS.Services
{
    public class JobClassService : IJobClassService
    {        
        private readonly AppDBContext _context;
        public JobClassService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<JobClass>> GetJobClassByID(string ID)
        {
            var result = _context.mJobClass.Where(w => w.Program.ProgramCode == ID || w.Schedule.TrainingCode == ID).ToListAsync();
            return await result;            
        }
        
        public async Task<List<JobClass>> GetHRJobClassList()
        {
            try
            {
                var result = _context.mJobClass.FromSqlRaw($"Select * FROM TRS.vw_HR_JobClasses")
                .Select(jc => new JobClass { JobClassCode = jc.JobClassCode, JobClassDescription = jc.JobClassDescription});

                return await result.ToListAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
    }
}