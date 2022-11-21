//#########################################################
//#Copyright (C) 2013, MIA Solution. All Rights Reserved
//#
//#History:
//# DateTime         Updater         Comment
//#12/12/2013       Triet Ho        Tao moi

//##################################################################
//Loc Ngo
var language_id;
var add_setting_configure_data;
var Ocr_Id;
var emptyId = "00000000-0000-0000-0000-000000000000";
var mapping_save_data;
var setting_save_data;
var isEditMapping = -1;//Edit: 1; Add new: -1
// show setting
function ShowSettings() {

    ///Get Ajax
    $(".archive_admin_container").ecm_loading_show();

    JsonHelper.helper.post(
        URL_ShowSettings,
        JSON.stringify(""),
        ShowSettings_Success,
        ShowSettings_Error);
}
function ShowSettings_Success(data) {

    $(".archive_admin_container").find(".sub_properties_content").remove();
    $(".archive_admin_container").append(data);
    var table = $('#autoCorrectionList').dataTable(
    {
        "sScrollY": "135px",
        "sScrollYInner": "115px",
        "bScrollCollapse": false,
        "bPaginate": false,
        "bDestroy": true,
        "bFilter": false,
        "bSearchable": false,
        "bInfo": false,
        "sPaginationType": "bootstrap",
        "bSort": false,
    });

    $('#autoCorrectionList tbody').on('click', 'tr', function () {
        if ($("#autoCorrectionList tbody tr").hasClass('selected')) {
            $("#autoCorrectionList tbody tr").removeClass('selected');
            $(this).addClass('selected');
        }
        else {
            //table.$('tr.selected').removeClass('selected');
            $(this).addClass('selected');
        }
    });
    $(".archive_admin_container").ecm_loading_hide();
}

function ShowSettings_Error(jqXHR, textStatus, errorThrown) {
    $.EcmAlert(jqXHR,"WARNNING", BootstrapDialog.TYPE_WARNING);
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties_content").ecm_loading_hide();

}
// end show setting

//Added by locngo - Archive admin setting side
//show configure of Archive admin setting(mapping table)
function ShowSettingConfigureTable() {

    ///Get ajax
    var languageId = $("#select_languages").val();
    JsonHelper.helper.post(
        URL_SettingConfigure,
        JSON.stringify({ LanguageId: languageId }),
        ShowSettingConfigureTable_Success,
        ShowSettingConfigureTable_Error);

}

function ShowSettingConfigureTable_Success(data) {
    $("#autoCorrectionList tbody").empty();
    $("#autoCorrectionList tbody").append(data);
    $("#autoCorrectionList tbody tr").first().click();
}

function ShowSettingConfigureTable_Error(jqXHR, textStatus, errorThrown) {
    $.EcmAlert(jqXHR.responseText, "WARNNING", BootstrapDialog.TYPE_WARNING);
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
}

//Save setting process
function SaveSettingProcess() {
    //var numperrow = $("#archive_admin_setting_numofrow").val();
    var workingfolder = $("#archive_admin_setting_server_folder").val();
    var searchperpage = $("#archive_admin_setting_numofrow").val();

    setting_save_data = {
        //MaxSearchRows: "",
        ServerWorkingFolder: workingfolder,
        MaxSearchRows: searchperpage
    };

    SaveSetting();
}

function SaveSetting() {
    $(".sub_properties_content").ecm_loading_show();
    JsonHelper.helper.post(
        URL_SaveSetting,
        JSON.stringify(setting_save_data),
        SaveSetting_Success,
        SaveMapping_Error);
}

function SaveSetting_Success() {
    ShowSettings();
    $(".sub_properties_content").ecm_loading_hide();
}

function SaveSetting_Error(jqXHR, textStatus, errorThrown) {
    $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties_content").ecm_loading_hide();
}

// Close setting configure
function CloseSettingConfigure() {
    $(".between_and_right").show();
    $(".admin_sub_menu").find(".admin_sub_menu_content").remove();
    $(".admin_sub_menu").hide();
    $(".sub_properties").find("sub_properties_content").remove();
    $(".sub_properties").hide();
    $(".sub_properties").ecm_loading_hide();
}

//Save mapping into database
function SaveMapping() {
    $(".sub_properties").ecm_loading_show();
    JsonHelper.helper.post(
        URL_SaveSettingConfigireDetails,
        JSON.stringify(add_setting_configure_data),
        SaveMapping_Sucess,
        SaveMapping_Error
        );
}

function SaveMapping_Sucess(data) {
    $(".archive_admin_container").ecm_loading_hide();
    
    var isUnicode;
    if (add_setting_configure_data.IsUnicode == true) {
        isUnicode = "Yes";
    } else {
        isUnicode = "No";
    }
    var temp = add_setting_configure_data.Text.split("\n");
    var this_will_add = "";
    $.each(temp, function (index, value) {
        this_will_add += value + "<br/>";
    });

    if (isEditMapping == -1) {
        var new_mapping =
            "<tr id='" + data + "' class='selected'>" +
            "<td>" + add_setting_configure_data.Text + "</td>" +
            "<td>" + isUnicode + "</td><td id='" + add_setting_configure_data.LanguageId + " class='hidden'></td></tr>";

        $(".archive_admin_container").ecm_loading_show();
        $("#autoCorrectionList").find("tbody > tr").removeClass("selected");
        $("#autoCorrectionList").find("tbody").append(new_mapping);
        $(".archive_admin_container").ecm_loading_hide();

        $("#autoCorrectionList").find("tbody > tr.selected").click();
    } else {
        $(".archive_admin_container").ecm_loading_show();
        var $this_change = $("#" + data);//$(".admin_sub_menu_items").find(".mapping_table").find(".mapping_container.selected");
        $this_change.find(".ocr_correction").text(add_setting_configure_data.Text);
        $this_change.find(".unicode").text(isUnicode);

        $(".archive_admin_container").ecm_loading_hide();
    }
    
}

function SaveMapping_Error(jqXHR, textStatus, errorThrown) {
    $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
    console.log(jqXHR.respontText + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();
}

//Save mapping process(called by click mapping button)
function SaveMappingProcess() {
    var languageId = $("#select_languages").val();
    var language = $("#select_languages option:selected").text();
    var text = $("textarea").val();
    var isUnicode = $("#unicode_check").is(":checked");
    Ocr_Id = $("#autoCorrectionList > tbody > tr.selected").attr("id");

    add_setting_configure_data = {
        Id: Ocr_Id,
        LanguageId: languageId,
        IsUnicode: isUnicode,
        Text: text
    };
    
    if (Ocr_Id == undefined) {
        if (!checkMappingValid(languageId)) {
            $.EcmAlert("Ocr auto correction the define to" + language + " is already existed. Please change selected language and try again!", "WARNNING", BootstrapDialog.TYPE_WARNING);
            return false;
        }
    }

    SaveMapping();
}

function checkMappingValid(languageId) {
    var isValid = true;
    var $mappings = $("#autoCorrectionList > tbody > tr");

    $.each($mappings, function (index, item) {
        if ($(item).find("#" + languageId) != undefined && $mappings.length >= 2) {
            isValid = false;
            return false;
        }
    });

    return isValid;
}

// Delete mapping
function DeleteMapping() {
    $(".archive_admin_container").ecm_loading_show();
    Ocr_Id = $("#autoCorrectionList tbody tr.selected").attr("id");
    JsonHelper.helper.post(
        URL_DeleteSettingConfigureDetails,
        JSON.stringify({ Id: Ocr_Id }),
        DeleteMapping_Success,
        DeleteMapping_Error
        );
}

function DeleteMapping_Success() {
    ShowSettingConfigureTable();
    $(".archive_admin_container").ecm_loading_hide();
}

function DeleteMapping_Error(jqXHR, textStatus, errorThrown) {
    $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();
}


// End archive admin setting side

$(document).ready(function () {
    // event when click setting in left_column 
    $("#settings").click(function () {
        //$(".archive_admin_menu").find(".admin_menu_item").removeClass("active");
        //$(this).addClass("active");
        $(".admin-menu > li").removeClass("active");
        $(this).addClass("active");

        $(".admin_sub_menu").find("div").remove();
        $(".sub_properties").find("div").remove();
        $(".admin_sub_menu").hide();
        $(".sub_properties").hide();

        ShowSettings();
    });

    //event when click save Configure button
    $(document).on("click", "#archive_admin_setting_save", function () {
        if (CheckField($("#archive_admin_setting_numofrow"), "Number of rows per search result", 0, 0)) {
            if (CheckIntegerNumber($("#archive_admin_setting_numofrow"), "Number of rows per search result")) {
                if (CheckField($("#archive_admin_setting_server_folder"), "Server working folder", 0, 0)) {
                    SaveSettingProcess();
                }
            }
        }
    })

    //event change value of combox languages
    $(document).on("change", "#select_languages", function () {
        language_id = { languageId: $(this).val() };
        ShowSettingConfigureTable();
    });
    
    //event click "add" button to create new mapping
    $(document).on("click", "#add_ocr_correction", function () {
        Ocr_Id = emptyId;
        var languageId = $("#select_languages").val();

        if (checkMappingValid(languageId)) {
            isEditMapping = -1;
            $("#autoCorrectionDetail").val("");
            $("#unicode_check").data("value", false);
        } else {
            $.EcmAlert("Can not add new mapping! Max mapping for each language: 2", "WARNNING", BootstrapDialog.TYPE_WARNING);
        }
    });

    //event click in each mapping
    $(document).on("click", "#autoCorrectionList > tbody > tr", function () {
        Ocr_Id = $(this).data("id");
        $("#autoCorrectionDetail").val("");
        $("#unicode_check").prop("checked", false);

        var text = $(this).find(".ocr_correction").text();
        var isUnicode = $(this).find(".unicode").text() == "Yes" ? true : false;

        $("#autoCorrectionDetail").val(text);
        $("#unicode_check").prop("checked", isUnicode);

        isEditMapping = 1;
    });

    //event click Save button in OCR Details
    $(document).on("click", "#mapping_configure_cancel_button", function () {
        $(".admin_sub_menu_items").find(".mapping_container").first().click();
    })

    //event click delete setting configure button
    var dialog_delete_mapping_yes_function = function () {
        DeleteMapping();
    };

    $(document).on("click", "#delete_ocr_correction", function () {
        BootstrapDialog.confirm({
            title: 'WARNING',
            message: 'Are you sure you want to delete selection?',
            type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
            closable: true, // <-- Default value is false
            draggable: true, // <-- Default value is false
            btnCancelLabel: 'Cancel', // <-- Default value is 'Cancel',
            btnOKLabel: 'OK', // <-- Default value is 'OK',
            btnOKClass: 'btn-warning', // <-- If you didn't specify it, dialog type will be used,
            callback: function (result) {
                // result will be true if button was click, while it will be false if users close the dialog directly.
                if (result) {
                    dialog_delete_mapping_yes_function();
                } 
            }
        });
    })

    //set value for is unicode checkbox
    $(document).on("click", "#unicode_check", function(){
        var val = $(this).data("value");

        if (val == true)
            $(this).data("value", false);
        else
            $(this).data("value", true);
    })

    $(document).on("click", "#save_ocr_correction", function () {
        SaveMappingProcess();
    });
});