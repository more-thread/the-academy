using System.Text;

namespace TRS.Models
{   
    public static class MenuService
    {
        public static List<MenuItem>? MenuList { get; set; }

        public static string ToSentenceCase(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (word.Length > 0)
                    words[i] = char.ToUpper(word[0]) + word.Substring(1).ToLower();
            }
            return string.Join(" ", words);
        }
        
        public static void GetMenuItem(string empid)
        {

            if (MenuList != null)
            {
                MenuList = MenuList.Where(obj => obj.EmployeeID != empid).ToList();
            }
            else
            {
                MenuList = new List<MenuItem>();
            }

            GetData(empid);
        }


        public static void GetData(string empid)
        {
            foreach (var SubMenu in FormService.FormList.Where(x => x.EmployeeNo == empid).DistinctBy(x => x.SubMenuName))
            {
                if (SubMenu.SubMenuID == null)
                {
                    var parent = new MenuItem
                    {
                        EmployeeID = SubMenu.EmployeeNo,
                        ID = SubMenu.SubMenuID,
                        Icon = "fa-th",
                        Name = ToSentenceCase(SubMenu.FormName),
                        // Name = SubMenu.FormName,
                        Url = $"{(System.Diagnostics.Debugger.IsAttached ? "" : "/TRS")}/{SubMenu.Controller}/{SubMenu.Action}",
                    };

                    MenuList.Add(parent);
                }
                else
                {
                    var child = new List<ChildMenuItem>();
                    foreach (var Menu in FormService.FormList.Where(x => x.SubMenuName == SubMenu.SubMenuName && x.EmployeeNo == empid))
                    {

                        var innerChild = new List<InnerChildMenuItem>();

                        child.Add(new ChildMenuItem
                        {
                            EmployeeID = SubMenu.EmployeeNo,
                            ID = Menu.FormID,
                            ParentID = Menu.SubMenuID,
                            Name = ToSentenceCase(Menu.FormName),
                            // Name = Menu.FormName,
                            Url = $"{(System.Diagnostics.Debugger.IsAttached ? "" : "/TRS")}/{Menu.Controller}/{Menu.Action}",
                            InnerChildMenuItemList = innerChild
                        });
                    }

                    // Create a parent model instance
                    var parent = new MenuItem
                    {
                        EmployeeID = SubMenu.EmployeeNo,
                        ID = SubMenu.SubMenuID,
                        Icon = "fa-th",
                        Name = ToSentenceCase(SubMenu.SubMenuName),
                        // Name = SubMenu.SubMenuName,
                        Url = $"{(System.Diagnostics.Debugger.IsAttached ? "" : "/TRS")}/{SubMenu.Controller}/{SubMenu.Action}",
                        ChildMenuItemList = child
                    };

                    MenuList.Add(parent);
                    
                }
            }
        }
    }

    public class MenuItem
    {
        public string EmployeeID { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public string Badge { get; set; }
        public string BadgeColor { get; set; }
        public List<ChildMenuItem> ChildMenuItemList { get; set; } = new List<ChildMenuItem>();
        public string Html
        {
            get
            {
                var concatenatedString = new StringBuilder();
                foreach (var item in ChildMenuItemList)
                {
                    concatenatedString.Append(item.Html);
                }

                
                return "<li class='nav-item menu-is-opening menu-open'>" +
                        $"<a href='{Url}' data-ID='{ID}' class='nav-link form-menu parent-menu' onclick='MenuVisit(this)'>" +
                          //  $"<i class='nav-icon fas {Icon}'></i>" +
                            $"<p>{Name}</p>" +
                            (concatenatedString.Length > 0 ? "<i class='right fas fa-angle-left'></i>" : "") +
                          "</a>" +
                        (Badge != null ? $"<span class='right badge badge-{BadgeColor}'>{Badge}</span>" : "") +
                        (concatenatedString.Length > 0 ? $"<ul class='nav nav-treeview'>{concatenatedString}</ul>" : "") +
                       "</li>";
            }
        }
    }
    public class ChildMenuItem
    {

        public string EmployeeID { get; set; }
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string? Name { get; set; }
        public string Url { get; set; }
        public string Badge { get; set; }
        public string BadgeColor { get; set; }
        public List<InnerChildMenuItem> InnerChildMenuItemList { get; set; } = new List<InnerChildMenuItem>();
        public string Html
        {
            get
            {
                var concatenatedString = new StringBuilder();
                foreach (var item in InnerChildMenuItemList)
                {
                    concatenatedString.Append(item.Html);
                }
                return "<li class='nav-item'>" +
                        $"<a href='{Url}' data-id='{ID}' data-parent-id='{ID}' class='nav-link form-menu child-menu' onclick='MenuVisit(this)'>" +
                            $"<i class='nav-icon far fa-circle'></i>" +
                            $"<p>{Name}</p>" +
                            (concatenatedString.Length > 0 ? "<i class='right fas fa-angle-left'></i>" : "") +
                          "</a>" +
                        (Badge != null ? $"<span class='right badge badge-{BadgeColor}'>{Badge}</span>" : "") +
                        (concatenatedString.Length > 0 ? $"<ul class='nav nav-treeview'>{concatenatedString}</ul>" : "") +
                       "</li>";
            }
        }
    }

    public class InnerChildMenuItem
    {
        public string EmployeeID { get; set; }
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Html
        {
            get
            {
                 return "<li class='nav-item'>" +
                        $"<a href='{Url}' data-id='{ID}' data-parent-id='{ID}' class='nav-link form-menu inner-child-menu' onclick='MenuVisit(this)'>" +
                            //"<i class='far fa-dot-circle nav-icon'></i>" +
                            "<span style='margin-right:10px;'>&nbsp;</span> - " +
                            $"<p>{Name}</p>" +
                        "</a>" +
                       "</li>";
            }
        }
    }
}
