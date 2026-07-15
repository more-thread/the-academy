using TRS.Models;

namespace TRS.Interfaces
{
    public interface ITrainingScheduleService
    {       
        Task<TrainingSchedule> GetTrainingScheduleDetailsByCode(string code);
        Task<List<TrainingSchedule>>  GetTrainingScheduleList();
        Task<List<VwHrRegion>> GetHRRegionList();
        bool AddSchedule(TrainingSchedule model);
        bool UpdateSchedule();
        
    }
}