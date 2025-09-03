using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Server.HttpSys;


namespace TRS.Models
{    
    [Table("mJobClass", Schema = "TRS")]
    public class JobClass
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]        
        [Key]                
        public long RecordNo { get;  set;}           
        [Required]
        public string JobClassCode { get; set;}
        [Required]
        public string JobClassDescription { get; set;}        
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
        [JsonIgnore]
        public TrainingProgram? Program {get;set;}  
        [JsonIgnore]      
        public TrainingSchedule? Schedule {get;set;}
    }
}