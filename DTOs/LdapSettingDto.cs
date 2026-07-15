using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;
using Telerik.SvgIcons;
using static System.Collections.Specialized.BitVector32;

namespace TRS.DTOs
{
    public class LdapSettingDto
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? Domain { get; set; }
    }
}
