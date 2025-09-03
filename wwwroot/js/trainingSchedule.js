// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function filterCourse() {
    return {
        ProgramCode : $("#dd_Program").val()
    };
}   

function filterJobClasses() {
    return {
        ProgramCode : $("#dd_Program").val()
    };
}   

function dd_Course_OnChangeCourse(){
    var _thisDropDownvalue = this.value();           
    
    $.ajax({
    type: "GET",
    url: getTrainingCourseDescriptionByCodeUrl,
    data: { courseCode: _thisDropDownvalue },
    success: function (data) {
        $(txtCourseDescription).val(data);
    }
    });
}

function dd_Program_OnChange(e) {      
    g_programCodeFilter = this.value();
    var multiSelect = $("#multi_jobClass").data("kendoMultiSelect");

    var multiSelect = $("#multi_jobClass").data("kendoMultiSelect");
    $.ajax({
        type: "GET",
        url: getTrainingProgramJobClassesByCodeUrl,
        data: { paramProgramCode: g_programCodeFilter },
        success: function (data) {
            var jobClasses = data;


            var addMultiSelect = $("#multi_addJobClass").data("kendoMultiSelect");
            addMultiSelect.dataSource.read()
            addMultiSelect.value([]);

            if(jobClasses == null || jobClasses == "")                
                multiSelect.value([]);
            else
                multiSelect.value(jobClasses.map(function (item) { return item.JobClassCode; }));
        }
    });
}


function dtp_StartDate_OnChange() {
    var startDate = $("#dtp_StartDate").data("kendoDatePicker").value();
    var endDate = $("#dtp_EndDate").data("kendoDatePicker").value();
    if (startDate && endDate && startDate > endDate) {
        $mong.kendoAlertInfo("Start date cannot be greater than end date");
        $("#dtp_StartDate").data("kendoDatePicker").value(endDate);
    }
}

function dtp_EndDate_OnChange() {
    var startDate = $("#dtp_StartDate").data("kendoDatePicker").value();
    var endDate = $("#dtp_EndDate").data("kendoDatePicker").value();
    if (startDate && endDate && endDate < startDate) {
        $mong.kendoAlertInfo("End date cannot be less than start date");
        $("#dtp_EndDate").data("kendoDatePicker").value(startDate);
    }
}

function tp_StartTime_OnChange() {
    var startTime = $("#tp_StartTime").data("kendoTimePicker").value();
    var endTime = $("#tp_EndTime").data("kendoTimePicker").value();
    if (startTime && endTime && startTime > endTime) {
        $mong.kendoAlertInfo("Start time cannot be greater than end time");
        $("#tp_StartTime").data("kendoTimePicker").value(endTime);
    }
}

function tp_EndTime_OnChange() {
    var startTime = $("#tp_StartTime").data("kendoTimePicker").value();
    var endTime = $("#tp_EndTime").data("kendoTimePicker").value();
    if (startTime && endTime && endTime < startTime) {
        $mong.kendoAlertInfo("End time cannot be less than start time");
        $("#tp_EndTime").data("kendoTimePicker").value(startTime);
    }
}
function CloseWindow(){
    $("#window_details").data("kendoWindow").close();         
}