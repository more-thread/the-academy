using TRS.Models;

namespace TRS.ViewModels{
    public class TrainingCoordinatorViewModel
    {
        public List<TrainingCoordinator>? TrainingCoordinatorList { get; set; }
        public TrainingCoordinator? TrainingCoordinatorDetails { get; set; }
        public FormControlModel? FormControl { get; set; }
    }
}