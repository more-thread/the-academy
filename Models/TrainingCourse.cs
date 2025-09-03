using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace TRS.Models
{
    [Table("mTrainingCourse", Schema = "TRS")]
    public class TrainingCourse
    {         
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecordNo { get; set;}   
        [Key]        
        public required string CourseCode { get; set; }
        [Required]
        public string CourseTitle { get; set; }
        [Required]
        public string CourseDescription { get; set; }
        public bool WithPreTest { get; set; }
        public Int64? PreTestTotalScore { get; set; }
        public bool WithPostTest { get; set; }
        public Int64? PostTestTotalScore { get; set; }        
        public bool WithEvaluation { get; set; }
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
        [Required]
        public TrainingProgram Program { get; set; }

        [NotMapped]
        public string FormattedWithEvaluation
        {
            get { return WithEvaluation ? "YES" : "NO"; }
        }
        [NotMapped]
        public string FormattedWithPreTest
        {
            get { return WithPreTest ? "YES" : "NO"; }
        }
        [NotMapped]
        public string FormattedWithPostTest
        {
            get { return WithPostTest ? "YES" : "NO"; }
        }

    }
}   