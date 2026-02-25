using Microsoft.AspNetCore.Http;
using Telerik.SvgIcons;
using TRS.Global;
using TRS.Models;

namespace TRS.Services
{
    public interface ISessionService
    {
        Task<bool> ValidateSessionAsync(string employeeNo);
        Task CreateUserSessionAsync(UserInfo user);
        Task ClearUserSessionAsync();
        bool IsSessionValid();
        string GetCurrentUserId();
        string GetCurrentEmployeeNo();
        string GetCurrentTemporaryEmployeeNo();
        UserSessionInfo GetCurrentUserSession();
        Dictionary<string, string> GetAuditTrail();
    }

    public class SessionService : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GlobalService _globalService;
        private readonly ILogger<SessionService> _logger;

        public SessionService(IHttpContextAccessor httpContextAccessor, 
                            GlobalService globalService, 
                            ILogger<SessionService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _globalService = globalService;
            _logger = logger;
        }

        public async Task<bool> ValidateSessionAsync(string employeeNo)
        {
            if (string.IsNullOrEmpty(employeeNo))
                return false;

            try
            {
                var user = _globalService.GetUserInfo(employeeNo);
                return user?.UserID != null && !string.IsNullOrEmpty(user.UserID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating session for employee: {EmployeeNo}", employeeNo);
                return false;
            }
        }

        public async Task CreateUserSessionAsync(UserInfo user)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            try
            {
                // Set session values
                session.SetString("SessionEmployeeNo", user.EmpID ?? "");
                session.SetString("SessionUserID", user.UserID ?? "");
                session.SetString("SessionFullName", user.FullName ?? "");
                session.SetString("SessionDesignation", user.PositionName ?? "");
                session.SetString("SessionSectionName", user.SectionName ?? "");
                session.SetString("SessionDepartmentName", user.DepartmentName ?? "");
                session.SetString("SessionEmailAddress", user.EmailAddress ?? "");

                // Handle display picture
                if (user.DisplayPic != null && user.DisplayPic.Length > 0)
                {
                    string imageDataURL = $"data:image/png;base64,{Convert.ToBase64String(user.DisplayPic)}";
                    session.SetString("SessionDisplayPic", imageDataURL);
                }

                // Set session metadata
                session.SetString("SessionCreatedAt", DateTime.Now.ToString("O"));
                session.SetString("SessionLastActivity", DateTime.Now.ToString("O"));

                _logger.LogInformation("User session created for: {EmployeeNo}", user.EmpID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session for user: {EmployeeNo}", user.EmpID);
                throw;
            }
        }

        public async Task ClearUserSessionAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            try
            {
                var employeeNo = session.GetString("SessionEmployeeNo");
                if (session == null)
                {
                    employeeNo = session.GetString("SessionTemporaryEmployeeNo");
                }
                session.Clear();

                session.SetString("SessionTemporaryEmployeeNo", employeeNo ?? "");
                _logger.LogInformation("User session cleared for: {EmployeeNo}", employeeNo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing user session");
            }
        }

        public bool IsSessionValid()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return false;

            var employeeNo = session.GetString("SessionEmployeeNo");
            var lastActivity = session.GetString("SessionLastActivity");

            if (string.IsNullOrEmpty(employeeNo) || string.IsNullOrEmpty(lastActivity))
                return false;

            // Check session timeout (30 minutes)
            if (DateTime.TryParse(lastActivity, out DateTime lastActivityTime))
            {
                if (DateTime.Now.Subtract(lastActivityTime).TotalMinutes > 30)
                {
                    ClearUserSessionAsync();
                    return false;
                }
            }

            // Update last activity
            session.SetString("SessionLastActivity", DateTime.Now.ToString("O"));
            return true;
        }

        public string GetCurrentUserId() => 
            _httpContextAccessor.HttpContext?.Session?.GetString("SessionUserID") ?? "";

        public string GetCurrentEmployeeNo() => 
            _httpContextAccessor.HttpContext?.Session?.GetString("SessionEmployeeNo") ?? "";

        public string GetCurrentTemporaryEmployeeNo() =>
            _httpContextAccessor.HttpContext?.Session?.GetString("SessionTemporaryEmployeeNo") ?? "";

        public UserSessionInfo GetCurrentUserSession()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            return new UserSessionInfo
            {
                EmployeeNo = session.GetString("SessionEmployeeNo"),
                UserID = session.GetString("SessionUserID"),
                FullName = session.GetString("SessionFullName"),
                Designation = session.GetString("SessionDesignation"),
                SectionName = session.GetString("SessionSectionName"),
                DepartmentName = session.GetString("SessionDepartmentName"),
                EmailAddress = session.GetString("SessionEmailAddress"),
                DisplayPic = session.GetString("SessionDisplayPic")
            };
        }

        public Dictionary<string, string> GetAuditTrail()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return new Dictionary<string, string>();

            return new Dictionary<string, string>
            {
                {"HostName", _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown"},
                {"UserID", session.GetString("SessionUserID") ?? ""},
                {"LoggedEmployeeNo", session.GetString("SessionEmployeeNo") ?? ""}
            };
        }
    }

    public class UserSessionInfo
    {
        public string EmployeeNo { get; set; }
        public string UserID { get; set; }
        public string FullName { get; set; }
        public string Designation { get; set; }
        public string SectionName { get; set; }
        public string DepartmentName { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayPic { get; set; }
    }
}