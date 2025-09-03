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

                var sql = $"EXEC TRS.sp_Global_SendEmail {toWrapped}, {ccWrapped}, {bccWrapped}, '{html}', '{subject}'";
                var forms = _context.Database.ExecuteSqlRaw(sql);
            }
            catch (Exception ex)    
            {
                // Log the exception
                Console.WriteLine(ex.ToString());
            }
        }

        //public void SMTP_SendEmail(string html, string subject, List<string> to = null, List<string> cc = null, List<string> bcc = null)
        //{
        //    using (var message = new MailMessage())
        //    {
        //        message.From = new MailAddress("jerameel_rivera@universalleaf.com.ph", "Your Name");

        //        if (to != null)
        //        {
        //            foreach (var item in to)
        //            {
        //                message.To.Add(new MailAddress(item));
        //            }
        //        }
        //        if (cc != null)
        //        {
        //            foreach (var item in cc)
        //            {
        //                message.CC.Add(new MailAddress(item));
        //            }
        //        }
        //        if(bcc != null)
        //        {
        //            foreach (var item in bcc)
        //            {
        //                message.Bcc.Add(new MailAddress(item));
        //            }
        //        }
                

        //        message.Subject = subject;
        //        message.Body = html;
        //        message.IsBodyHtml = true; // Change to true if body msg is in HTML

        //        using (var client = new SmtpClient("smtpserver.universalleaf.com"))
        //        {
        //            client.UseDefaultCredentials = false;
        //            client.Port = 25;
        //            client.Credentials = new NetworkCredential("jerameel_rivera", "H@ngl00se1234", "universalleaf");
        //            client.EnableSsl = false;
        //            client.Timeout = 600000;

        //            try
        //            {
        //                client.SendMailAsync(message); // Email sent
                        
        //            }
        //            catch (SmtpException smtpEx)
        //            {
        //                // Log SMTP-specific exceptions
        //                // You can use a logging framework like Serilog, NLog, etc.
        //                Console.WriteLine($"SMTP Error: {smtpEx.Message}");
        //            }
        //            catch (Exception ex)
        //            {
        //                // Log general exceptions
        //                Console.WriteLine($"Error: {ex.Message}");
        //            }
        //        }
        //    }
        //}


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