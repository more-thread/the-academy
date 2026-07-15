$(document).ready(function(){
  $('body').append('<div id="dlg_Confirm"></div>');
  $('body').append('<div id="dlg_Info"></div>');
  $('body').append('<div id="win_AboutPage"></div>');
  
  $("<style>").appendTo("head").html(`
    .btnStandard {
       width: 80px;
       height: 30px;                
       background-color: #f4a434 !important;
       border-color: #f4a434 !important;
       color: white !important;
     }
 
    .is-invalid {
       box-shadow: 0 0 5px 0px #ff000094!important;
       border-color: #ff000094!important;
       background-color: white!important;
     }

    .is-invalid-radio {
       box-shadow: 0 0 5px 0px red!important;
       border-color: red!important;
       border-radius: 7px;
       background-color: white!important;
     }
   `);
  
  $("#dlg_Info").kendoDialog({
    title: "INFORMATION MESSAGE",
    width: 400,
    modal: true,
    visible: false,
    closable: false,
    actions: [
      {
        text: "Ok",
        primary: true,
        cssClass: "btnStandard"
      }
    ],
    buttonLayout: "normal"
  });

  $("#win_AboutPage").kendoWindow({
    draggable: true,
    pinned: true,
    modal: true,
    visible: false,
    width: 700,
    height: 400,
    actions: ["Close"],
    // content: '<div class="col-md-auto"><div class="col-sm-auto"><h5 style="margin-top: 5px"><i class="k-icon k-i-info" style="font-size: 30px; margin-top: -6px"></i> About this Page</h5><div class="k-card" style="margin-top: 10px; min-height: 230px; margin-bottom: 30px"><div class="k-card-body" id="divAboutForm"></div></div></div></div>'
  });
  
  
  $(this).on('change', '.is-invalid', function() {
      $(this).removeClass("is-invalid");
  });

  $(this).on('change', '.is-invalid-radio', function() {
    $(this).removeClass("is-invalid-radio");
  });

  kendo.syncReady(function(){jQuery("#dlg_Info").kendoDialog({"actions":[{"text":"Ok","primary":true,"cssClass":"btnStandard"}],"buttonLayout":"normal","closable":false,"visible":false,"title":"INFORMATION MESSAGE","width":"400px"});});
  kendo.syncReady(function(){jQuery("#win_AboutPage").kendoWindow({"pinned":true,"scrollable":true,"visible":false,"content":null,"actions":["Close"],"resizable":false,"modal":true,"height":"400px","width":"700px"});});
  $("#win_AboutPage").data("kendoWindow").center();
})

var $mong = function() {

      function kendoConfirmationDialog(message) {
        return new Promise(function (resolve) {
            var dialog = $("#dlg_Confirm").kendoDialog({
                title: "CONFIRMATION MESSAGE",
                modal: true,
                width: 400,
                buttonLayout: "normal",
                content:
                    `
                    <div class="row">
                            <div style="align-items: center;display: flex;padding: 5px;">
                                <i class=" k-icon k-i-help" style="color: #656565;font-size: 30px;"></i>                
                                <span class="" style="padding-left: 10px; vertical-align: middle; font-size: 11px; width: 340px;">`+ message + `</span>
                            </div>
                    </div>
                    `,
                actions: [
                    {
                        text: "Yes",
                        primary: true,
                        cssClass: "btnStandard",
                        action: function (e) {
                            dialog.close();
                            resolve(true);
                        }
                    },
                    {
                        text: "No",
                        cssClass: "btnStandard",
                        action: function (e) {
                            dialog.close();
                            resolve(false);
                        }
                    }
                ]
            }).data("kendoDialog");
    
            // Open the dialog
            dialog.open();
        });
      }
    
      function kendoAlertInfo(msg) {
          var iconalign = "middle";
          if (msg.length > 62) { iconalign = "top" };
          var msgicon = "<i class = 'col-md-1 k-icon k-i-info d-inline-block' style = 'color: #656565;font-size: 30px; vertical-align: " + iconalign + "'></i>";
          var dialog = $("#dlg_Info").data("kendoDialog");
    
          var html = `
                    <div class="row">
                            <div style="align-items: center;display: flex; padding: 5px;" >
                                <i class=" k-icon k-i-info" style="color: #656565;font-size: 30px;"></i>                
                                <span class="" style="padding-left: 10px; vertical-align: middle;font-size: 11px; width: 340px;">`+ msg + `</span>
                            </div>
                    </div>
                    `
          dialog.content(html).open();
      };
    
      // Get all required fields in a form and check if are empty/blank or null    
      // Requirement/s: a "required-field" class should be added in an input field
      function ValidateRequiredFields(requiredClass = 'required-field'){
        
        var InvalidCounter = 0        
        $.each($('.'+requiredClass), function (index, item) { 
          if($(item).is('input[data-mask]')){
            //get the valid count of input in the input field
            var validInputCount = $(item)[0].inputmask.maskset._buffer.filter(function(item) { return item === '_'; }).length;
            //check if unmasked value is equal to valid count
            if($(item).inputmask('unmaskedvalue').length !== validInputCount){        
              InvalidCounter++;
              $(item).addClass("is-invalid");
            }else{          
              $(item).removeClass("is-invalid");
            }
            return;
          }
          
           // Check if it's a kendo datepicker
          if ($(item).hasClass("k-datepicker")) {
            // Check if the value is empty/blank or null
            if ($(item).find('input').val() === "" || $(item).find('input').val() === null) {
              InvalidCounter++;
              $(item).addClass("is-invalid");
            }else{              
              $(item).removeClass("is-invalid");
            }
            return;
          }
          
           // Check if it's a kendo timepicker
          if ($(item).hasClass("k-timepicker")) {
            // Check if the value is empty/blank or null
            if ($(item).find('input').data("kendoTimePicker").value() === "" || $(item).find('input').data("kendoTimePicker").value() === null) {
              InvalidCounter++;
              $(item).addClass("is-invalid");
            }else{              
              $(item).removeClass("is-invalid");
            }
            return;
          }
          
          // Check if it's a kendo multiselect
          if ($(item).hasClass("k-dropdownlist")) {
            // Check if the value is empty/blank or null
            if ($(item)[0].innerText === "" || $(item)[0].innerText === null) {
              InvalidCounter++;
              $(item).addClass("is-invalid");
            }else{              
              $(item).removeClass("is-invalid");
            }
            return;
          }
          
          // Check if it's a kendo multiselect
          if ($(item).hasClass("k-input")) {
            // Check if the value is empty/blank or null
            if ($(item)[0].children[0].value === "" || $(item)[0].children[0].value === null) {
              InvalidCounter++;
              $(item).addClass("is-invalid");
            }else{              
              $(item).removeClass("is-invalid");
            }
            return;
          }
          
          // Check if it's a kendo multiselect
          if ($(item).hasClass("k-multiselect")) {
            // Check if the value is empty/blank or null
            if ($(item)[0].innerText === "" || $(item)[0].innerText === null) {
              InvalidCounter++;
              $(item).addClass("is-invalid");
            }else{              
              $(item).removeClass("is-invalid");
            }
            return;
          }
    
          // Check if it's an input field
          if ($(item).is('input[type="text"]')) {
            // Check if the value of the input field is empty/blank or null
            if ($(item).val() === "" || $(item).val() === null) {
              InvalidCounter++;
              $(item).addClass("is-invalid");
            }else{              
              $(item).removeClass("is-invalid");
            }
            return;
          }
          
          // Check if it's an input field number
          if ($(item).is('input[type="number"]')) {
            // Check if the value of the input field is empty/blank or null
            if ($(item).val() === "" || $(item).val() === null) {
              InvalidCounter++;
              $(item).addClass("is-invalid");
            }else{              
              $(item).removeClass("is-invalid");
            }
            return;
          }
          
          // Check if it's a textarea field
          if ($(item).is('textarea')) {
            // Check if the value is empty/blank or null
            if ($(item).val() === "" || $(item).val() === null) {
              InvalidCounter++;
              $(item).addClass("is-invalid");
            }else{              
              $(item).removeClass("is-invalid");
            }
            return;
          }

          if($(item).is('input[type="radio"]')) {
            const radioButtons = document.querySelectorAll('input[name="'+$(item)[0].name+'"]');
            let isChecked = false;
            radioButtons.forEach((radioButton) => {
              if (radioButton.checked) {
                isChecked = true;
              }
            });
            
            if (isChecked) {              
              $(item).closest('tr').removeClass("is-invalid");
              $(item).closest('.form-group.row').removeClass("is-invalid-radio");
            } else {
              InvalidCounter++;
              $(item).closest('tr').addClass("is-invalid");
              $(item).closest('.form-group.row').addClass("is-invalid-radio");
            }
          }

        })
        return InvalidCounter;
      }


      return {
        kendoAlertInfo: kendoAlertInfo,
        kendoConfirmationDialog: kendoConfirmationDialog,
        ValidateRequiredFields : ValidateRequiredFields
      }

}();