using TRS.Models;

namespace TRS.Interfaces
{
    public interface ITrainingRegistrationApprovalService
    {       
        bool AddRegistration(TrainingRegistration model);
        bool UpdateRegistration();
        
    }
}