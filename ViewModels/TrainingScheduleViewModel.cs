using TRS.Models;

namespace TRS.ViewModels{
    public class TrainingScheduleViewModel
    {
        public List<TrainingSchedule>? TrainingScheduleList { get; set; }
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