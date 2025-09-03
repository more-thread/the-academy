namespace TRS.Models
{
    public static class FormService
    {
        public static List<FormAccess>? FormList { get; set; }
        public static FormAccess? PageAbout { get; set; }

        public static void GetForms(string empid, List<FormAccess> xFormList)
        {
            if (FormList != null)
            {                
                FormList = FormList.Where(obj => obj.EmployeeNo != empid).ToList();

            }
            else
            {
                FormList = new List<FormAccess>();
            }
            
            foreach (var item in xFormList)
            {
                FormList.Add(item);
            }
            
        }

        public static void GetPageAbout(string formID) { 
            PageAbout = FormList.Where(x => x.FormID == formID).FirstOrDefault();
        }

        public static FormAccess GetPagePermission(string formID, string empid)
        {
            return FormList.Where(x => x.FormID == formID && x.EmployeeNo == empid).FirstOrDefault();
        }

    }
}
