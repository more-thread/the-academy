using TRS.Data;
using Microsoft.Extensions.Options;
using System.Net;

namespace TRS.DTOs
{
    public class SessionDto
    {
        public string HostName { get; set; } = string.Empty;
        public string UserID { get; set; } = string.Empty;
        public string EmployeeNo { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string SectionId { get; set; } = string.Empty;
        public string DepartmentId { get; set; } = string.Empty;

        public SessionDto(IHttpContextAccessor accessor) {

            HostName = accessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            UserID = accessor.HttpContext?.Session?.GetString("SessionUserID");
            EmployeeNo = accessor.HttpContext?.Session?.GetString("SessionEmployeeNo");
            EmailAddress = accessor.HttpContext?.Session?.GetString("SessionEmailAddress");
            SectionId = accessor.HttpContext?.Session?.GetString("SessionSectionId");
            DepartmentId = accessor.HttpContext?.Session?.GetString("SessionDepartmentId");
        }

    }
}
