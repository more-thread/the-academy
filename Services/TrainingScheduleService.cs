
using Microsoft.EntityFrameworkCore;
using TRS.Data;
using TRS.Interfaces;
using TRS.Models;

namespace TRS.Services
{
    public class TrainingScheduleService : ITrainingScheduleService
    {
        //todo add database context
        private readonly AppDBContext _context;
        public TrainingScheduleService(AppDBContext context)
        {
            _context = context;
        }
        public async Task<TrainingSchedule> GetTrainingScheduleDetailsByCode(string code)
        {
            try
            {
                var result = _context.tTrainingSchedule.Where(w => w.TrainingCode == code)
                        .Include(c=>c.AdditionalJobClasses)
                        .Include(c => c.Program)
                        .Include(c => c.Program.JobClasses)
                        .Include(c => c.Course)
                        .FirstOrDefaultAsync();
                return await result;    
            }
            catch (System.Exception)
            {
                
                throw;
            }
            
        }
        
        public async Task<List<TrainingSchedule>> GetTrainingScheduleList()
        {             
            var result = _context.tTrainingSchedule
                        .Include(c => c.AdditionalJobClasses)
                        .Include(c => c.Program)
                        .Include(c => c.Program.JobClasses)
                        .Include(c => c.Course)
                        .ToListAsync();            
            return await result;
        }
        public async Task<List<VwHrRegion>> GetHRRegionList()
        {             
            var result = _context.VwHrRegions.GroupBy(r => r.Region)
                .Select(g => g.First())
                .ToListAsync();            
            return await result;
        }

        public bool AddSchedule(TrainingSchedule model)
        {
             _context.Add(model);
            return _context.SaveChanges() > 0;
        }
        
        public bool UpdateSchedule()
        {       
            return _context.SaveChanges() > 0;
        }
    }   
}