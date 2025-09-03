using TRS.Models;

namespace TRS.ViewModels{
    public class TrainingProfileViewModel
    {
        public List<TrainingRegistration>? TrainingRegistrationList { get; set; }
        public TrainingRegistration? TrainingRegistrationDetails { get; set; }
        public VwHrEmployeeInfo? EmployeeProfile {get;set;}
        public double TotalManDays { get; set; }
        public int CoursesTaken { get; set; }
    }
}