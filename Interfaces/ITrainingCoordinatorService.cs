using TRS.Models;

namespace TRS.Interfaces
{
    public interface ITrainingCoordinatorService
    {     
        Task<List<VwHrEmployeeInfo>> GetActiveEmployeeList();
        Task<List<TrainingCoordinator>> GetTrainingCoordinatorList();
        Task<TrainingCoordinator> GetTrainingCoordinatorByEmployeeNo(string EmployeeNo);
        Task<VwHrEmployeeInfo> GetActiveEmployeeByEmployeeNo(string EmployeeNo);
        bool AddTrainingCoordinator(TrainingCoordinator trainingCoordinator);
        bool UpdateStatus();

    }
}
