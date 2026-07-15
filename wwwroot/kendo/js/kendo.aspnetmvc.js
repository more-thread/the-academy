/**
 * Kendo UI v2023.2.606 (http://www.telerik.com/kendo-ui)
 * Copyright 2023 Progress Software Corporation and/or one of its subsidiaries or affiliates. All rights reserved.
 *
 * Kendo UI commercial licenses may be obtained at
 * http://www.telerik.com/purchase/license-agreement/kendo-ui-complete
 * If you do not own a commercial license, this file shall be governed by the trial license terms.
 */
import "./kendo.data.js";
import "./kendo.combobox.js";
import "./kendo.dropdownlist.js";
import "./kendo.dropdowntree.js";
import "./kendo.multiselect.js";
import "./kendo.validator.js";
import "./aspnetmvc/kendo.data.aspnetmvc.js";
import "./aspnetmvc/kendo.combobox.aspnetmvc.js";
import "./aspnetmvc/kendo.multicolumncombobox.aspnetmvc.js";
import "./aspnetmvc/kendo.dropdownlist.aspnetmvc.js";
import "./aspnetmvc/kendo.dropdowntree.aspnetmvc.js";
import "./aspnetmvc/kendo.multiselect.aspnetmvc.js";
import "./aspnetmvc/kendo.imagebrowser.aspnetmvc.js";
import "./aspnetmvc/kendo.validator.aspnetmvc.js";
import "./aspnetmvc/kendo.filemanager.aspnetmvc.js";

var __meta__ = {
    id: "aspnetmvc",
    name: "ASP.NET MVC",
    category: "wrappers",
    description: "Scripts required by Telerik UI for ASP.NET MVC and Telerik UI for ASP.NET Core",
    depends: [ "data", "combobox", "multicolumncombobox", "dropdownlist", "multiselect", "validator" ]
};

(function($, undefined) {
    var extend = $.extend;

    $(function() { kendo.__documentIsReady = true; });

    function syncReady(cb) {
        if (kendo.__documentIsReady) { //sync operation
            cb();
        }
        else { //async operation
            $(cb);
        }
    }

    extend(kendo, {
        syncReady: syncReady
    });
})(window.kendo.jQuery);
