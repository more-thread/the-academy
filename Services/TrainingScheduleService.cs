
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

                if (ApplyAutoCloseRegistrationStatus(result))
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

            var hasChanges = false;
            foreach (var schedule in result)
            {
                if (ApplyAutoCloseRegistrationStatus(schedule))
                    hasChanges = true;
            }
            if (hasChanges)
                _context.SaveChanges();

            return result;
        }

        //- Auto-close the Registration Status once Confirmed Participants reaches Class Size, the Start Date has
        //  been reached, or the schedule is canceled. Evaluated on every fetch since there is no background scheduler.
        private bool ApplyAutoCloseRegistrationStatus(TrainingSchedule schedule)
        {
            if (schedule == null || schedule.ScheduleStatus != "AVAILABLE")
                return false;

            if (schedule.IsRegistrationAutoClosed && schedule.RegistrationStatus != "CLOSED")
            {
                schedule.RegistrationStatus = "CLOSED";
                return true;
            }

            return false;
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