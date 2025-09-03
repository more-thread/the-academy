using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TRS.Models
{
    [Table("mTrainingProgram", Schema = "TRS")]
    public class TrainingProgram
    {   
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        
        public long RecordNo { get; set;}    
        [Key]            
        public string ProgramCode { get; set; }
        [Required]
        public string ProgramTitle { get; set; }
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
        public List<JobClass> JobClasses { get; set; }
        [JsonIgnore]
        public List<TrainingCourse>? Courses { get; set; }

        [NotMapped]
        public string FormattedJobClasses
        {
            get { return JobClasses != null ? string.Join(", ", JobClasses.Select(jc => jc.JobClassDescription)) : string.Empty; }
        }

        [NotMapped]
        public string FormattedStatus
        {
            get { return Status ?  "ACTIVE" : "INACTIVE"; }
        }
    }
}   