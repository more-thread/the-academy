using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace TRS.Models
{
    [Table("tTrainingFeedback", Schema = "TRS")]
    public class TrainingFeedback
    {         
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]        
        public long RecordNo { get; set;}   
        public required string EmployeeNo { get; set; }
        public string Answer { get; set; }        
        public bool Status { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public string CreatedByComputerUsed { get; set; }
        public DateTime DateCreated { get; set; }   
        public string ModifiedBy { get; set; } = null;
        public string ModifiedByComputerUsed { get; set; } = null;
        public DateTime? DateModified { get; set; }    
        [JsonIgnore]
        public TrainingFeedbackQuestions TrainingFeedbackQuestions { get; set; }
        [JsonIgnore]
        public TrainingRegistration TrainingRegistration { get; set; }
        [NotMapped]
        public VwHrEmployeeInfo EmployeeInfo {get; set;}

    }
}   