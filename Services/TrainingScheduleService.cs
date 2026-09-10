
using Microsoft.EntityFrameworkCore;
using TRS.Data;
using TRS.Global;
using TRS.Interfaces;
using TRS.Models;

namespace TRS.Services
{
    public class TrainingScheduleService : ITrainingScheduleService
    {
        //todo add database context
        private readonly AppDBContext _context;
        private readonly GlobalService _globalService;
        public TrainingScheduleService(AppDBContext context, GlobalService globalService)
        {
            _context = context;
            _globalService = globalService;
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

                if (result != null && RecalculateRegistrationStatus(result, _globalService.GetDateTime()))
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

            var currentDate = _globalService.GetDateTime();
            var hasChanges = false;
            foreach (var schedule in result)
            {
                if (RecalculateRegistrationStatus(schedule, currentDate))
                    hasChanges = true;
            }
            if (hasChanges)
                _context.SaveChanges();

            return result;
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

        // Registration should auto-close once the class is full, the schedule's start date has been reached, or the schedule was cancelled.
        public bool IsRegistrationAutoCloseConditionMet(TrainingSchedule schedule, DateTime currentDate)
        {
            return schedule.ScheduleStatus == "CANCELLED"
                || schedule.StartDate.Date <= currentDate.Date
                || (schedule.RegisteredEmployeeCount ?? 0) >= schedule.ClassSize;
        }

        // Keeps RegistrationStatus in sync with the auto-close/auto-open conditions whenever a schedule is loaded, since there's no background job to react to the Start Date passing on its own.
        // Once RegistrationStatus has been set manually (IsRegistrationStatusManual), it's left alone unless the auto-close
        // conditions (class full, start date reached, cancelled) are met, which always force it closed.
        private bool RecalculateRegistrationStatus(TrainingSchedule schedule, DateTime currentDate)
        {
            if (schedule.ScheduleStatus != "AVAILABLE")
                return false;

            var autoCloseConditionMet = IsRegistrationAutoCloseConditionMet(schedule, currentDate);
            if (!autoCloseConditionMet && schedule.IsRegistrationStatusManual)
                return false;

            var newStatus = autoCloseConditionMet ? "CLOSED" : "OPEN";
            if (schedule.RegistrationStatus == newStatus)
                return false;

            schedule.RegistrationStatus = newStatus;
            return true;
        }
    }
}