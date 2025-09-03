using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Server.HttpSys;


namespace TRS.Models
{    
    [Table("tUserLogs", Schema = "TRS")]
    public class UserLogs
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]        
        [Key]                
        public long RecordNo { get;  set;}           
        [Required]
        public string Activity { get; set;}
        [Required]
        public string CreatedBy { get; set; }
        [Required]        
        public string CreatedByComputerUsed { get; set; }        
        public DateTime DateCreated { get; set; }
    }
}