using TRS.Models;

namespace TRS.Interfaces
{
    public interface ITrainingFeedbackService
    {
        Task<List<TrainingFeedbackQuestions>> GetTrainingFeedbackQuestionsList();
        Task<TrainingFeedbackQuestions> GetTrainingFeedbackQuestionsByID(string id);
        Task<List<TrainingFeedback>> GetTrainingFeedbackByRegistrationCode(string code); 
        Task<Dictionary<string, Dictionary<string, string>>> GetTrainingFeedbackAnswersByScheduleCode(string code);        
        bool AddFeeback(TrainingFeedback model);
    }
}