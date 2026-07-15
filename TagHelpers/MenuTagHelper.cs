using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TRS.Global;
using TRS.Models;

namespace TRS.TagHelpers
{
    [HtmlTargetElement("navigation-menu")]
    public class MenuTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MenuTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var employeeNo = session?.GetString("SessionEmployeeNo");

            output.TagName = "ul";
            output.Attributes.SetAttribute("class", "nav nav-pills nav-sidebar flex-column");
            output.Attributes.SetAttribute("data-widget", "treeview");
            output.Attributes.SetAttribute("role", "menu");
            output.Attributes.SetAttribute("data-accordion", "false");

            var menuHtml = "";
            foreach (var item in MenuService.MenuList.Where(x => x.EmployeeID == employeeNo))
            {
                menuHtml += item.Html;
            }

            output.Content.SetHtmlContent(menuHtml);
        }
    }
}