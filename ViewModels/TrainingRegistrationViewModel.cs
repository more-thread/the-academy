using TRS.Models;

namespace TRS.ViewModels{
    public class TrainingRegistrationViewModel
    {
        public List<TrainingRegistration>? TrainingRegistrationList { get; set; }
        public TrainingRegistration? TrainingRegistrationDetails { get; set; }
        public List<JobClass>? JobClasses { get; set; }
        public FormControlModel FormControl {get;set;}
    }
}