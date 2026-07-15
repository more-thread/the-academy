
namespace TRS.Models
{    public partial class FormAccess
    {
        public string EmployeeNo {get;set;} = "";
        public string FormID {get;set;} = "";
        public string FormName {get;set;} = "";
        public string AccessibleDescription {get;set;} = "";
        public string CurrentVersion {get;set;} = "";
        public DateTime? DateModified {get;set;}
        public string DevInitials {get;set;} = "";
        public string DevInfo { get; set; } = "";
        public string SubMenuID { get; set; } = "";
        public string SubMenuName { get; set; } = "";
        public string Controller {get;set;} = ""; 
        public string Action {get;set;} = ""; 
        public string Icon {get;set;} = ""; 
        public string AccessType {get;set;} = ""; 
    }
}
