using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TRS.Models
{
    [Table("tTrainingSchedule", Schema = "TRS")]
    public class TrainingSchedule
    {       
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecordNo { get; set;}   
        [Key]        
        public string TrainingCode { get; set; } 
        [Required]
        public string RegistrationStatus { get; set; }
        [Required]
        public string ScheduleStatus { get; set; }
        [Required]
        public string Region { get; set; }
        [Required]
        public string TrainingType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; } 
        [Required]
        public string ResourceSpeaker { get; set; }
        [Required]
        public int ClassSize { get; set; } 
        [Required]
        public int MinimumParticipants { get; set; }
        public double? CostPerHead { get; set; }        
        public string Venue { get; set; }
        public string CancellationType { get; set; }
        public string CancellationReason { get; set; }
        public int? RegisteredEmployeeCount {get;set;}        
        public string ScheduleCreatedBy { get; set; }                
        public DateTime? ScheduleCreatedDate { get; set; }        
        public string ScheduleModifiedBy { get; set; }                
        public DateTime? ScheduleModifiedDate { get; set; }     
        public string ScheduleCanceledBy { get; set; }                
        public DateTime? ScheduleCanceledDate { get; set; }
        public string ScheduleCompletedBy { get; set; }
        public DateTime? ScheduleCompletedDate { get; set; } 
        [Required]
        [DefaultValue(true)]
        public bool Status { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public string CreatedByComputerUsed { get; set; }
        public DateTime DateCreated { get; set; }
        public string ModifiedBy { get; set; } 
        public string ModifiedByComputerUsed { get; set; } 
        public DateTime? DateModified { get; set; }
        [JsonIgnore]
        public TrainingCourse Course { get; set; }
        [JsonIgnore]
        public TrainingProgram Program { get; set; }
        [JsonIgnore]
        public List<JobClass>? AdditionalJobClasses { get; set; }
        [JsonIgnore]
        public List<TrainingRegistration>? TrainingRegistration { get; set; }
        [NotMapped]
        public string FormattedJobClasses
        {
            get {
                var programJobclasses = "";
                if(Program != null)
                programJobclasses = Program.JobClasses != null ? string.Join(", ", Program.JobClasses.Select(jc => jc.JobClassDescription)) : string.Empty;
                
                var additionalJobClasses = "";
                if(AdditionalJobClasses != null)
                additionalJobClasses = AdditionalJobClasses != null ? AdditionalJobClasses.Count > 0 ? string.Join(", ", AdditionalJobClasses.Select(jc => jc.JobClassDescription)) : string.Empty : string.Empty;
                               

                return programJobclasses + (additionalJobClasses=="" ? additionalJobClasses: ", "+additionalJobClasses);
            }
        }
        [NotMapped]
        public string FormattedDate
        {
            get { return $"{StartDate.ToShortDateString()} - {EndDate.ToShortDateString()}"; }
        }
        [NotMapped]
        public string FormattedTime
        {
            get { return $"{StartTime} - {EndTime}"; }
        }
    }
}