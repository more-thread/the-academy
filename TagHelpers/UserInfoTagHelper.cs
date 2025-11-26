using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TRS.Global;

namespace TRS.TagHelpers
{
    [HtmlTargetElement("user-info")]
    public class UserInfoTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserInfoTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var fullName = session?.GetString("SessionFullName") ?? "Guest";
            var designation = session?.GetString("SessionDesignation") ?? "";
            var sectionName = session?.GetString("SessionSectionName") ?? "";
            var displayPic = session?.GetString("SessionDisplayPic");

            output.TagName = "div";
            output.Attributes.SetAttribute("class", "user-info-container");
            
            var imageHtml = string.IsNullOrEmpty(displayPic) 
                ? "<img src=\"~/dist/img/DefaultUser.png\" class=\"img-circle elevation-2\" alt=\"User Image\" style=\"height:100px;width:100px; object-fit: cover\">"
                : $"<img src=\"{displayPic}\" class=\"img-circle elevation-2\" alt=\"User Image\" style=\"height:100px; width:100px; object-fit: cover; padding: 2px;border: 3px solid;\">";

            output.Content.SetHtmlContent($@"
                <div class=""widget-user-image"" style=""margin-bottom:15px;"">
                    {imageHtml}
                </div>
                <a class=""widget-user-username user-info"" style=""font-size: 15px; font-weight: bold; color:black"">
                    {fullName}
                </a><br />
                <a class=""widget-user-desc d-inline-block user-info"" style=""color:black"">
                    {designation}
                </a><br />
                <a class=""widget-user-desc d-inline-block user-info"" style=""color:black"">
                    {sectionName}
                </a><br />
            ");
        }
    }
}