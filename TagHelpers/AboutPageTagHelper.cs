using Microsoft.AspNetCore.Razor.TagHelpers;
using TRS.Global;
using TRS.Models;

namespace TRS.TagHelpers
{
    [HtmlTargetElement("about-page")]
    public class AboutPageTagHelper : TagHelper
    {
        [HtmlAttributeName("form-id")]
        public string FormId { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Get page about information using FormService
            if (!string.IsNullOrEmpty(FormId))
            {
                FormService.GetPageAbout(FormId);
            }

            var pageAbout = FormService.PageAbout;

            output.TagName = "div";
            output.Attributes.SetAttribute("class", "about-page-content");

            if (pageAbout != null)
            {
                var devInfo = pageAbout.DevInfo?.Replace("&lt;br /&gt;", "<br/>") ?? "";
                
                output.Content.SetHtmlContent($@"
                    <table class=""table table-borderless"" style=""padding: 1px !important;"">
                        <tbody>
                            <tr>
                                <td style=""text-align:right; padding: 1px !important;"">Page Name :</td>
                                <td style=""text-align:left; font-weight:bold; padding: 1px !important;"">{pageAbout.FormName ?? ""}</td>
                            </tr>
                            <tr>
                                <td style=""text-align:right; padding: 1px !important;"">Page ID :</td>
                                <td style=""text-align:left; font-weight:bold; padding: 1px !important;"">{pageAbout.FormID ?? ""}</td>
                            </tr>
                            <tr>
                                <td style=""text-align:right;padding: 1px !important;"">Version No :</td>
                                <td style=""text-align:left; font-weight:bold; padding: 1px !important;"">{pageAbout.CurrentVersion ?? ""}</td>
                            </tr>
                            <tr>
                                <td style=""text-align:right;padding: 1px !important;"">Accessible Description :</td>
                                <td style=""text-align:left; font-weight:bold; padding: 1px !important;"">{pageAbout.AccessibleDescription ?? ""}</td>
                            </tr>
                            <tr>
                                <td style=""text-align:right;padding: 1px !important;"">Date Modified :</td>
                                <td style=""text-align:left; font-weight:bold; padding: 1px !important;"">{pageAbout.DateModified?.ToString() ?? ""}</td>
                            </tr>
                            <tr>
                                <td style=""text-align:right;padding: 1px !important; width: 30%;"">Dev. Originator :</td>
                                <td style=""text-align:left; font-weight:bold; padding: 1px !important;"">{pageAbout.DevInitials ?? ""}</td>
                            </tr>
                            <tr>
                                <td style=""text-align:right;padding: 1px !important;"">Dev. Support :</td>
                                <td style=""text-align:left; font-weight:bold; padding: 1px !important;"">{devInfo}</td>
                            </tr>
                        </tbody>
                    </table>
                ");
            }
            else
            {
                output.Content.SetHtmlContent("<p>No information available for this page.</p>");
            }
        }
    }
}