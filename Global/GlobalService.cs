using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System.Net.Mail;
using System.Net;
using TRS.Data;
using TRS.Models;

namespace TRS.Global
{        
    public class GlobalService
    {
        private readonly AppDBContext _context;
        public GlobalService(AppDBContext context){
            _context = context;
        }
        
        public DateTime GetDateTime(){
            return _context.Database.SqlQueryRaw<DateTime>($"SELECT getdate()[DateTime]").ToList().First();
        }
        public List<FormAccess> GetUserAccess(string empID)
        {
            try
            {
                var forms = _context.Set<FormAccess>().FromSqlRaw($"EXEC TRS.sp_Global_GetUserForms @p0", empID).ToList();
                return forms;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.ToString());
                return new List<FormAccess>();
            }
        }

        public UserInfo GetUserInfo(string empID)
        {
            try
            {
                var forms = _context.Set<UserInfo>().FromSqlRaw($"EXEC TRS.sp_Global_GetUserInfo @p0", empID).AsEnumerable();
                return forms.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine(ex.ToString());
                return new UserInfo();
            }
        }

        public void SendEmail(string html, string subject,string to = null,string cc = null, string bcc = null)
        {
            try
            {
                var toWrapped = to != null ? $"'{to}'" : "NULL";
                var ccWrapped = cc != null ? $"'{cc}'" : "NULL";
                var bccWrapped = bcc != null ? $"'{bcc}'" : "NULL";
                // Escape single quotes in html and subject for SQL
                var htmlEscaped = html?.Replace("'", "''");
                var subjectEscaped = subject?.Replace("'", "''");

                var sql = $"EXEC TRS.sp_Global_SendEmail {toWrapped}, {ccWrapped}, {bccWrapped}, '{htmlEscaped}', '{subjectEscaped}'";
                var forms = _context.Database.ExecuteSqlRaw(sql);
            }
            catch (Exception ex)    
            {
                // Log the exception
                Console.WriteLine(ex.ToString());
            }
        }

        public VwHrEmployeeInfo GetHREmployeeInfoByEmployeeNo(string paramEmployeeNo)
        {             
            var result = _context.VwHrEmployeeInfos.Where(w => w.EmployeeNo == paramEmployeeNo).FirstOrDefault();
            return result;
        }

        public List<VwHrEmployeeInfo> GetEmployeeList()
        {             
            var result = _context.VwHrEmployeeInfos.ToList();
            return result;
        }

        public List<UserLogs> GetLogs()
        {
            var result = _context.tUserLogs.ToList();
            return result;
        }

        public void Log(string message, Dictionary<string,string> audit, Exception exception = null){            
            
            var errMsg = "";
            if(exception != null)
                errMsg = "--- Ex.Msg:" + exception.Message.ToString() + "--- Ex.Inner.Msg:" + (exception.InnerException == null ? "":exception.InnerException.Message.ToString());

            _context.tUserLogs.Add(new UserLogs(){
                Activity = message + errMsg,
                CreatedBy = audit["UserID"],
                CreatedByComputerUsed = audit["HostName"],
                DateCreated =  GetDateTime()
            });

            _context.SaveChanges();
        }    
        
        public void PageVisitLog(string message, Dictionary<string,string> audit){            
            _context.tUserLogs.Add(new UserLogs(){
                Activity = "Visit Page: "+message,
                CreatedBy = audit["UserID"],
                CreatedByComputerUsed = audit["HostName"],
                DateCreated =  GetDateTime()
            });

            _context.SaveChanges();
        }    
        
    }
}