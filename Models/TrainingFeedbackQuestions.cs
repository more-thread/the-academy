using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace TRS.Models
{
    [Table("mTrainingFeedbackQuestions", Schema = "TRS")]
    public class TrainingFeedbackQuestions
    {         
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecordNo { get; set;}   
        [Key]        
        public required string QuestionID { get; set; }        
        public required string Question { get; set; }
        public required string QuestionType { get; set; }
        public required string Category { get; set; }        
        public required bool IsRequired { get; set; }
        public required Int64 QuestionSequence { get; set; }                
        public required Int64 CategorySequence { get; set; }        
        public bool Status { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        [Required]
        public string CreatedByComputerUsed { get; set; }
        public DateTime DateCreated { get; set; }   
        public string ModifiedBy { get; set; } = null;
        public string ModifiedByComputerUsed { get; set; } = null;
        public DateTime? DateModified { get; set; }    
    }
}   