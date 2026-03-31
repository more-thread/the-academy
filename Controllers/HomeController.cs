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
using TRS.Services;

namespace TRS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly GlobalService _globalService;
        private readonly IHttpContextAccessor _accessor;
        private readonly IEmployeeAuthenticationService _authService;
        private readonly ISessionService _sessionService;

        public HomeController(
            ILogger<HomeController> logger,
            GlobalService globalService,
            IHttpContextAccessor accessor,
            IEmployeeAuthenticationService authService,
            ISessionService sessionService)
        {
            _globalService = globalService;      
            _logger = logger;
            _accessor = accessor;
            _authService = authService;
            _sessionService = sessionService;
        }

        public async Task<IActionResult> Crypto()
        {
            try
            {
                var idno = RouteData.Values["id"] + Request.QueryString.ToString();
                var getid = idno.Substring(4, idno.Length - 4);

                if (string.IsNullOrEmpty(getid))
                {
                    _logger.LogWarning("Crypto action called without id parameter");
                    return RedirectToAction("Error");
                }

                var employeeNo = await _authService.ValidateAndDecryptEmployeeIdAsync(getid);

                if (string.IsNullOrEmpty(employeeNo))
                {
                    _logger.LogWarning("Invalid or expired authentication token");
                    return RedirectToAction("Error");
                }

                var user = _globalService.GetUserInfo(employeeNo);
                if (user?.UserID == null)
                {
                    _logger.LogWarning("User not found for employee: {EmployeeNo}", employeeNo);
                    return RedirectToAction("Error");
                }

                await _sessionService.CreateUserSessionAsync(user);
                
                _logger.LogInformation("Employee authenticated successfully: {EmployeeNo}", employeeNo);
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Crypto authentication");
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Check if session is valid using SessionService
                if (!_sessionService.IsSessionValid())
                {
                    _logger.LogWarning("Invalid or expired session");
                    return RedirectToAction("Timeout");
                }

                var employeeNo = _sessionService.GetCurrentEmployeeNo();
                
                // If no employee number in session, redirect to login
                if (string.IsNullOrEmpty(employeeNo))
                {
                    return RedirectToAction("Timeout");
                }

                // Get user access permissions
                var formAccesses = _globalService.GetUserAccess(employeeNo);
                if (formAccesses.Count == 0)
                {
                    _logger.LogWarning("No form access found for user: {EmployeeNo}", employeeNo);
                    return RedirectToAction("AccessDenied");
                }

                // Initialize form and menu services
                FormService.GetForms(employeeNo, formAccesses);
                MenuService.GetMenuItem(employeeNo);

                // Log page visit using SessionService audit trail helper
                var auditTrail = _sessionService.GetAuditTrail();
                _globalService.PageVisitLog($"{RouteData.Values["controller"]}/{RouteData.Values["action"]}", auditTrail);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> ContinueSession()
        {
            try
            {

                var employeeNo = _sessionService.GetCurrentTemporaryEmployeeNo();

                var user = _globalService.GetUserInfo(employeeNo);
                if (user?.UserID == null)
                {
                    _logger.LogWarning("User not found for employee: {EmployeeNo}", employeeNo);
                    return RedirectToAction("Error");
                }

                await _sessionService.CreateUserSessionAsync(user);

                _logger.LogInformation("Employee authenticated successfully: {EmployeeNo}", employeeNo);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                return RedirectToAction("Error");
            }
        }

        public IActionResult Login()
        {
            return View();
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
        public IActionResult Timeout()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public ActionResult SetTheme(string selection)
        {
            if (string.IsNullOrEmpty(selection))
                return BadRequest("Invalid theme selection");

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(365),
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Append("theme", selection, cookieOptions);
            var returnUrl = Request.Headers["Referer"].ToString();

            return Json(new { result = "Redirect", url = returnUrl });
        }

        [HttpPost]
        public IActionResult SetDarkMode(string selection)
        {
            if (string.IsNullOrEmpty(selection))
                return BadRequest("Invalid dark mode selection");

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(365),
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict
            };

            Response.Cookies.Append("darkmode", selection, cookieOptions);
            return Ok();
        }

        public IActionResult ShowAboutPage(string FormID)
        {
            FormService.GetPageAbout(FormID);
            return PartialView("_AboutPage");
        }

        // Keep the static Decrypt method for backward compatibility if needed elsewhere
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
    }
}