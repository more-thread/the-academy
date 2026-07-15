using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TRS.Models
{
    [Table("tTrainingRegistration", Schema = "TRS")]
    public class TrainingRegistration
    {       
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RecordNo { get; set;}   
        [Key]        
        public string RegistrationCode { get; set; }
        [Required]
        public string EmployeeNo { get; set; }
        public string Attendance {get;set;}
        public string AbsenceReason {get;set;}
        public Int64 PostTestFirstScore {get;set;}
        public Int64 PostTestSecondScore {get;set;}
        public Int64 PostTestThirdScore {get;set;}      
        public double PostTestFirstScorePercentage {get;set;}
        public double PostTestSecondScorePercentage {get;set;}
        public double PostTestThirdScorePercentage {get;set;}     
        public Int64 EvaluationScore {get;set;}        
        public string PostTestStatus { get; set; }
        public string ReasonForCancellation { get; set; }
        public string ReasonForDenying { get; set; }
        public string ReasonForDisapproval { get; set; }
        [Required]
        public string TrainingRegistrationStatus { get; set; }        
        public string TrainingCompletionStatus { get; set; }
        public string CourseCompletionStatus { get; set; }
        public string TrainingFeedbackStatus { get; set; }
        [JsonIgnore]
        public TrainingSchedule TrainingSchedule { get; set; }
        public string RegistrationCreatedBy { get; set; }
        public DateTime? RegistrationCreatedDate { get; set; }
        public string RegistrationCanceledBy { get; set; }
        public DateTime? RegistrationCanceledDate { get; set; }        
        public string RegistrationAcceptedBy { get; set; }
        public DateTime? RegistrationAcceptedDate { get; set; }
        public string RegistrationDeniedBy { get; set; }
        public DateTime? RegistrationDeniedDate { get; set; }
        public string RegistrationApprovedBy { get; set; }
        public DateTime? RegistrationApprovedDate { get; set; }        
        public string RegistrationDisapprovedBy { get; set; }
        public DateTime? RegistrationDisapprovedDate { get; set; }
        public string RegistrationConfirmedBy { get; set; }
        public DateTime? RegistrationConfirmedDate { get; set; }
        public string RegistrationRejectedBy { get; set; }
        public DateTime? RegistrationRejectedDate { get; set; }
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
        [NotMapped]
        public VwHrEmployeeInfo EmployeeInfo {get; set;}
    }
}