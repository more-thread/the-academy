namespace TRS.Models
{
    public class FormControlModel
    {
        public string? PropDisabled {get;set;} = "";
        public string? Required {get;set;} = "";
        public bool Enabled {get;set;} = true;
        public string? AddOnlyRequired {get;set;} = "";
        public string? EditOnlyRequired {get;set;} = "";
        public string? EditOnlyPropDisabled {get;set;} = "";
        public string? AddOnlyPropDisabled {get;set;} = "";
        public bool AddOnlyEnabled { get; set; } = false;
        public bool EditOnlyEnabled { get; set; } = false;
        public string? FormMode {get;set;} = ""; 
        public string? Text {get;set;} = ""; 
        public FormControlModel(string formMode){     
            FormMode = formMode;       
            Text = formMode;
            switch (formMode)
            {
                case "View":
                    PropDisabled = "disabled";
                    Enabled = false;       
                    EditOnlyPropDisabled = "disabled";   
                    AddOnlyPropDisabled = "disabled";
                    break;
                case "Add":
                    Required = "required-field";                    
                    AddOnlyRequired = "required-field";
                    AddOnlyEnabled = true;
                    AddOnlyPropDisabled = "disabled";
                    break;
                case "Edit":
                    Required = "required-field"; 
                    EditOnlyRequired = "required-field"; 
                    EditOnlyEnabled = true;                     
                    EditOnlyPropDisabled = "disabled";
                    break;
                default:
                    break;
            }            
        }
    }
}
