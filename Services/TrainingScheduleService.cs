
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
                var result = await _context.tTrainingSchedule.Where(w => w.TrainingCode == code)
                        .Include(c=>c.AdditionalJobClasses)
                        .Include(c => c.Program)
                        .Include(c => c.Program.JobClasses)
                        .Include(c => c.Course)
                        .FirstOrDefaultAsync();

                if (result != null && ApplyAutoRegistrationStatus(result))
                    _context.SaveChanges();

                return result;
            }
            catch (System.Exception)
            {

                throw;
            }

        }

        public async Task<List<TrainingSchedule>> GetTrainingScheduleList()
        {
            var result = await _context.tTrainingSchedule
                        .Include(c => c.AdditionalJobClasses)
                        .Include(c => c.Program)
                        .Include(c => c.Program.JobClasses)
                        .Include(c => c.Course)
                        .ToListAsync();

            var changed = false;
            foreach (var schedule in result)
                changed |= ApplyAutoRegistrationStatus(schedule);

            if (changed)
                _context.SaveChanges();

            return result;
        }

        // Recomputes RegistrationStatus based on class size, start date, and cancellation - see TrainingSchedule.ComputeRegistrationStatus().
        private bool ApplyAutoRegistrationStatus(TrainingSchedule schedule)
        {
            var desiredStatus = schedule.ComputeRegistrationStatus();
            if (schedule.RegistrationStatus == desiredStatus)
                return false;

            schedule.RegistrationStatus = desiredStatus;
            return true;
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