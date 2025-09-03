using TRS.Models;

namespace TRS.Interfaces
{
    public interface ITrainingRegistrationService
    {       
        Task<TrainingRegistration> GetTrainingRegistrationByCode(string code);
        Task<List<TrainingRegistration>> GetTrainingRegistrationList();        
        Task<List<VwHrRegion>> GetHRRegionList();
        Task<List<TrainingRegistration>> GetTraineeListByCode(string code);
        Task<List<TrainingRegistration>> GetTrainingListByEmployeeNo(string employeeNo);
        bool AddTrainee(TrainingRegistration model);
        bool UpdateRegistration();
        
    }
}