using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Server.HttpSys;


namespace TRS.Models
{    
    [Table("mTrainingCoordinator", Schema = "TRS")]
    public class TrainingCoordinator
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]                
        public long RecordNo { get;  set;}    
        [Key]       
        [Required]
        public string EmployeeNo { get; set;}
        [Required]
        public string EmployeeName { get; set;}                    
        [NotMapped]         
        public string PositionCode { get; set;}                     
        [NotMapped]
        public string PositionName { get; set;}                
        [Required]
        [DefaultValue(true)]        
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