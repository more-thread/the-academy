using System.Net;

namespace TRS.DTOs
{
    public class FtpSettingDto
    {
        public string IPAddressNoPort { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UserID { get; set; } = string.Empty;
    }
}
