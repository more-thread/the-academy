using TRS.Models;

namespace TRS.Interfaces
{
    public interface ITrainingProgramService
    {
        Task<TrainingProgram> GetTrainingProgramByCode(string code);
        Task<List<TrainingProgram>> GetTrainingProgramList();
        bool AddProgram(TrainingProgram model);
        bool UpdateProgram();
        
    }
}