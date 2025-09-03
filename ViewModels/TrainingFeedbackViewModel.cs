using TRS.Models;

namespace TRS.ViewModels{
    public class TrainingFeedbackViewModel
    {
        public List<TrainingRegistration>? TrainingRegistrationList { get; set; }
        public TrainingRegistration? TrainingRegistrationDetails { get; set; }
        public TrainingFeedback? TrainingFeedbackDetails { get; set; }
        public List<TrainingFeedback>? TrainingFeedbackList { get; set; }
        public List<TrainingFeedbackQuestions>? TrainingFeedbackQuestions { get; set; }
    }
}