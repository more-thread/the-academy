using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TRS.Models
{
    [Table("mCourseCategory", Schema= "TRS")]
    public class CourseCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecordNo { get; set; }
        [Key]
        public string CategoryCode { get; set; }
        [Required]
        public string CategoryTitle { get; set; }
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
        [NotMapped]
        public string FormattedStatus
        {
            get { return Status ? "ACTIVE" : "INACTIVE"; }
        }
    }
}
