//#########################################################
//#Copyright (C) 2013, Innoria Solution. All Rights Reserved
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
    $(".between_and_right").ecm_loading_show();

    Inno.helper.post(
        URL_ShowSettings,
        JSON.stringify(""),
        ShowSettings_Success,
        ShowSettings_Error);
}
function ShowSettings_Success(data) {
    $(".between_and_right").find(".between_and_right_content").remove();
    $(".between_and_right").append(data);
    setButtonWidthInFirefox();
    $(".between_and_right").ecm_loading_hide();
}

function ShowSettings_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".between_and_right").ecm_loading_hide();

}
// end show setting

//Added by locngo - Archive admin setting side
//show configure of Archive admin setting(mapping table)
function ShowSettingConfigureTable() {

    ///Get ajax
    Inno.helper.post(
        URL_SettingConfigure,
        JSON.stringify(language_id),
        ShowSettingConfigureTable_Success,
        ShowSettingConfigureTable_Error);

}

function ShowSettingConfigureTable_Success(data) {
    $(".mapping_table").html(data);
    $(".sub_menu_item").first().click();
    resize_vetical_second_main_admin();//function from affect.js
}

function ShowSettingConfigureTable_Error(jqXHR, textStatus, errorThrown) {
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
    $(".between_and_right").ecm_loading_show();
    Inno.helper.post(
        URL_SaveSetting,
        JSON.stringify(setting_save_data),
        SaveSetting_Success,
        SaveMapping_Error);
}

function SaveSetting_Success() {
    ShowSettings();
    $(".between_and_right").ecm_loading_hide();
}

function SaveSetting_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".between_and_right").ecm_loading_hide();
}

//Show ORC auto correction
function ShowSettingConfigureLanguages() {

    ///Get ajax
    $(".admin_sub_menu").ecm_loading_show();

    Inno.helper.post(
        URL_SettingConfigureLanguages,
        JSON.stringify(""),
        ShowSettingConfigureLanguages_Success,
        ShowSettingConfigureLanguages_Error);

}

function ShowSettingConfigureLanguages_Success(data) {
    $(".admin_sub_menu").find(".admin_sub_menu_content").remove();
    $(".admin_sub_menu").append(data);
    sub_menu_height(); //called from effect.js
    setButtonWidthInFirefox();
    language_id = { languageId: $("#setting_configure_language_select").val() };
    ShowSettingConfigureTable();

    $(".admin_sub_menu").ecm_loading_hide();
}

function ShowSettingConfigureLanguages_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();
}

//add mapping for setting configure
function ShowSettingConfigureDetails() {
    $(".sub_properties").ecm_loading_show();
    Inno.helper.post(
        URL_ShowSettingConfigureDetails,
        JSON.stringify({Id: Ocr_Id}),
        ShowSettingConfigureDetails_Success,
        ShowSettingConfiureDetails_Error);
}

function ShowSettingConfigureDetails_Success(data){
    $(".sub_properties").find(".sub_properties_content").remove();
    $(".sub_properties").append(data);
    resize_vetical_properties_content();//called from effect.js
    setButtonWidthInFirefox();
    $(".sub_properties").ecm_loading_hide();
}

function ShowSettingConfiureDetails_Error(jqXHR, textStatus, errorThrown){
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();
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
    Inno.helper.post(
        URL_SaveSettingConfigireDetails,
        JSON.stringify(add_setting_configure_data),
        SaveMapping_Sucess,
        SaveMapping_Error
        );
}

function SaveMapping_Sucess(data) {
    $(".sub_properties").ecm_loading_hide();
    
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
    var $mapping_menu = $(".admin_sub_menu_items").find(".mapping_table");
    if (isEditMapping == -1) {
        var new_mapping =
            "<div class='sub_menu_item user_item mapping_container selected' data-id='" + data + "'>" +
                "<div class='icon'>" +
                    "<img src='" + URL_MappingIcon + "' />" +
                "</div>" +

                "<div class='sub_item_content_full'>" +
                    "<div class='item_left_content left_mapping'>" +
                        "<div class='left_mapping_content'>" +
                            this_will_add +
                        "</div>" +
                    "</div>" +

                    "<div class='item_right_content right_unicode'><div>" +
                        isUnicode +
                    "</div></div>" +
                "</div>" +
            "</div>";
        
        $(".admin_sub_menu_items").ecm_loading_show();
        $mapping_menu.find(".mapping_container").removeClass("selected");
        $mapping_menu.append(new_mapping);
        $(".admin_sub_menu_items").ecm_loading_hide();
        $(".admin_sub_menu_items").find(".mapping_table").find(".mapping_container.selected").click();
    } else {
        $(".admin_sub_menu_items").ecm_loading_show();
        var $this_change = $(".admin_sub_menu_items").find(".mapping_table").find(".mapping_container.selected");

        $this_change.find(".left_mapping_content").remove();
        var left_content = "<div class='left_mapping_content'>" +
                            this_will_add +
                            "</div>";
        $this_change.find(".left_mapping").append(left_content);
        $this_change.find(".right_unicode div").remove();
        $this_change.find(".right_unicode").append("<div>" + isUnicode + "</div>");

        $(".admin_sub_menu_items").ecm_loading_hide();
    }
    
}

function SaveMapping_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();
}

//Save mapping process(called by click mapping button)
function SaveMappingProcess() {
    var languageId = $("#setting_configure_language_select").val();
    var text = $("textarea").val();
    var isUnicode = $("#unicode_check").data("value");
    add_setting_configure_data = {
        Id: Ocr_Id,
        LanguageId: languageId,
        IsUnicode: isUnicode,
        Text: text
    };
    SaveMapping();
}

// Delete mapping
function DeleteMapping() {
    $(".sub_properties").ecm_loading_show();
    Inno.helper.post(
        URL_DeleteSettingConfigureDetails,
        JSON.stringify({ Id: Ocr_Id }),
        DeleteMapping_Success,
        DeleteMapping_Error
        );
}

function DeleteMapping_Success() {
    $(".sub_properties").find(".sub_properties_content").remove();
    ShowSettingConfigureTable();
    $(".sub_properties").ecm_loading_hide();
}

function DeleteMapping_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();
}


// End archive admin setting side

$(document).ready(function () {
    // event when click setting in left_column 
    $("#settings").click(function () {
        $(".archive_admin_menu").find(".admin_menu_item").removeClass("selected");
        $(this).addClass("selected");

        $(".admin_sub_menu").find(".admin_sub_menu_content").remove();
        $(".sub_properties").find(".sub_properties_content").remove();
        $(".admin_sub_menu").hide();
        $(".sub_properties").hide();
        $(".between_and_right").show();
        $(".between_and_right").find(".between_and_right_content").remove();

        ShowSettings();
    });

    //event when click Configure button
    $(document).on("click", "#archive_admin_setting_configure", function () {
        $(".between_and_right").hide();
        $(".sub_properties").css({ display: 'inline-block' });
        $(".admin_sub_menu").css({ display: 'inline-block' });
        ShowSettingConfigureLanguages();
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
    $(document).on("change", "#setting_configure_language_select", function () {
        $(".sub_properties").find(".sub_properties_content").remove();
        language_id = { languageId: $(this).val() };
        ShowSettingConfigureTable();
    });
    
    //event click "add" button to create new mapping
    $(document).on("click", "#getting_configure_add_button", function () {
        Ocr_Id = emptyId;
        if (mapping_count < 2) {
            $(".sub_properties").find(".sub_properties_content").remove();
            $(".admin_sub_menu_items").find(".mapping_table").find(".mapping_container").removeClass("selected");
            isEditMapping = -1;
            ShowSettingConfigureDetails();
        } else {
            ErrorDialog(null, "Can not add new mapping!<br/>Max mapping for each language: 2");
        }
    });

    //event click in each mapping
    $(document).on("click", ".mapping_container", function () {
        $(".sub_properties").find(".sub_properties_content").remove();
        Ocr_Id = $(this).data("id");
        $(".admin_sub_menu_items").find(".sub_menu_item").removeClass("selected");
        $(this).addClass("selected");
        isEditMapping = 1;
        ShowSettingConfigureDetails();
    });

    //event click Save button in OCR Details
    $(document).on("click", "#mapping_configure_save_button", function () {
        SaveMappingProcess();
    })

    //event click cancel button in OCR Details
    $(document).on("click", "#mapping_configure_cancel_button", function () {
        $(".admin_sub_menu_items").find(".mapping_container").first().click();
    })

    //event click delete setting configure button
    var dialog_delete_mapping_yes_function = function () {
        DeleteMapping();
        $(this).dialog("close");
    };

    var dialog_delete_mapping_no_function = function () {
        $(this).dialog("close");
    }

    $(document).on("click", "#getting_configure_delete_button", function () {
        $.innoDialog({
            title: 'Cloud ECM',
            width: 350,
            dialog_data: $('#dialog_delete_mapping'),
            type: 'Yes_No',
            Yes_Button: dialog_delete_mapping_yes_function,
            No_Button: dialog_delete_mapping_no_function
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

    //event click close setting configure button
    $(document).on("click", "#getting_configure_close_button", function () {
        CloseSettingConfigure();
    })

    

});