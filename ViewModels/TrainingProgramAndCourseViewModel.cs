using TRS.Models;

namespace TRS.ViewModels{
    public class TrainingProgramAndCourseViewModel
    {
        public List<TrainingProgram>? TrainingProgramList { get; set; }
        public List<TrainingCourse>? TrainingCourseList { get; set; }
        public TrainingProgram? TrainingProgram { get; set; }
        public TrainingCourse? TrainingCourse { get; set; }
        public List<JobClass> JobClasses {get;set;}
        public FormControlModel? FormControl { get; set; }
    }
}