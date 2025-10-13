using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Packaging.Licenses;
using System.Diagnostics;
using TRS.Interfaces;
using TRS.Global;
using TRS.Models;
using System.Security.Cryptography;
using System.Text;
using Telerik.SvgIcons;

namespace TRS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GlobalService _globalService;
        private readonly IHttpContextAccessor _accessor;
        public HomeController(ILogger<HomeController> logger,
        GlobalService globalService,
        IHttpContextAccessor accessor
        )
        {
            _globalService = globalService;      
            _logger = logger;
            _accessor = accessor;
        }
        public IActionResult Crypto()
        {
            try
            {
                var idno = RouteData.Values["id"] + Request.QueryString.ToString();
                //idno = "test";
                var id = "";
                if (idno.Length > 0)
                {
                    var getid = idno.Substring(4, idno.Length - 4);
                    id = Decrypt(getid).ToString();
                    //id = "1019241"; //jera
                    //id = "1025434"; //arjay
                    //id = "1023719"; //froy                   
                    //id = "1002746"; //jerose
                    //id = "1025474"; //erol
                    //id = "1023691"; //eloah
                    //id = "1026092"; //eloah
                    HttpContext.Session.SetString("SessionEmployeeNo", id);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return Redirect(Url.Action("Error", "Home"));
                }
            }
            catch (Exception ex)
            {
                return Redirect(Url.Action("Error", "Home"));
                // Handle the exception...
            }
        }

        public IActionResult Index()
        {
            try
            {
                string employeeNo = HttpContext?.Session?.GetString("SessionEmployeeNo");
                UserInfo user = _globalService.GetUserInfo(employeeNo);

                if (user.UserID == null)
                {
                    return Redirect(Url.Action("Error", "Home"));
                }

                HttpContext.Session.SetString("SessionUserID", user.UserID);
                HttpContext.Session.SetString("SessionFullName", user.FullName);
                HttpContext.Session.SetString("SessionDesignation", user.PositionName);
                HttpContext.Session.SetString("SessionSectionName", user.SectionName);

                if (user.DisplayPic.ToString() != "")
                {
                    string imageDataURL = string.Format("data:image/png;base64,{0}",
                    Convert.ToBase64String(user.DisplayPic));
                    
                    HttpContext.Session.SetString("SessionDisplayPic", imageDataURL);
                }
                List<FormAccess> formAccesses = _globalService.GetUserAccess(user.EmpID);

                if (formAccesses.Count == 0)
                {
                    return Redirect(Url.Action("Error", "Home"));
                }

                FormService.GetForms(user.EmpID, formAccesses);
                MenuService.GetMenuItem(user.EmpID);

                var auditTrail = new Dictionary<string, string>{
                    {"HostName", _accessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString()},
                    {"UserID", user.UserID},
                    {"LoggedEmployeeNo", user.EmpID}
                };

                _globalService.PageVisitLog($"{RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail);
                return View(); 
                    
            }
            catch (System.Exception)
            {
                
                return Redirect(Url.Action("Error", "Home"));
                throw;
            }

                       
        }

        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
        public IActionResult Privacy()
        {
            return View();
        }
        
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    
        [HttpPost]
        public ActionResult SetTheme(string selection)
        {
            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddDays(365);

            Response.Cookies.Append("theme", selection, option);

            var returnUrl = Request.Headers["Referer"].ToString();

            return Json(new { result = "Redirect", url = returnUrl });
        }

        public IActionResult ShowAboutPage(string FormID)
        {
            FormService.GetPageAbout(FormID);
            return PartialView("_AboutPage");
        }

        [HttpPost]
        public void SetDarkMode(string selection)
        {
            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddDays(365);

            Response.Cookies.Append("darkmode", selection, option);
        }

    }
}