using TRS.Models;

namespace TRS.ViewModels{
    public class TraineeRegistrationViewModel
    {
        public List<TrainingSchedule>? TrainingScheduleList { get; set; }
        public List<TrainingRegistration>? TraineeList { get; set; }
        public List<TrainingFeedback>? TrainingFeedbackList { get; set; }
        public List<TrainingFeedbackQuestions>? TrainingFeedbackQuestions { get; set; }
        public TrainingSchedule? TrainingScheduleDetails { get; set; }
        public List<TrainingProgram>? TrainingProgramList { get; set; }
        public TrainingProgram? TrainingProgramDetails { get; set; }
        public TrainingCourse? TrainingCourseDetails { get; set; }
        
        public List<TrainingCourse> TrainingCourseList { get; set; }
        public List<JobClass> JobClasses {get;set;}
        public List<VwHrRegion>? RegionList { get; set; }
        public FormControlModel? FormControl { get; set; }
        public CalendarActivity? CalendarActivity { get; set; }
    }
}