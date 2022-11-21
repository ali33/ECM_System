
/* ==========================================================
 * CaptureAdminBatchTypes.js
 * Copyright (C) 2014 MIA Solution. All Rights Reserved	
 * History:
 * DateTime             Updater             Description
 * 08/10/2014           Hai.Hoang           Create
 * ========================================================== */

var EmptyId = "00000000-0000-0000-0000-000000000000";
var _BatchID = -1;
var _DocumentID = -1;
var _FieldID = -1;
var _ContentTypeFieldID = -1;
var json;
var json_batchType_deletefields = { batchType_DeletedFields: [] };
json_batchType_deletefields.batchType_DeletedFields.push({ Id: -1 });

var json_contentType_deletefields = { contentType_DeletedFields: [] };
json_contentType_deletefields.contentType_DeletedFields.push({ Id: -1 });

var isEdit = -1; // biến dùng để kiểm tra là đang edit field hay add field  1: Edit, -1: Add new
var isEditBatchType = -1;//Edit: 1; Add new: -1
var isEditContentType = -1;
var isEditIndexField = -1;
var picklistvalue = "";
var json_pick_value;
var str_pick_value = "";

// Variable declaration for table configure columns
var table_field_name;
var table_field_datatype;
var table_field_maxlength;
var table_field_default_value;
var table_field_use_current_date;
var json_table_fields = {};

var json_list_contenttypes = {};

//Variable declaration for table content type fields
var table_content_field_name;
var table_content_field_datatype;
var table_content_field_isrequired;
var table_content_field_isrestricted;
var table_content_field_hasloopup;
var table_content_field_defaultvalue;
var table_content_field_maxlenth;
var table_content_field_picklist;
var table_content_field_valida_script;
var table_content_field_valida_pattern;
var table_content_field_use_current_date;

//Variable declaration for table content types
var table_contenttype_name;
var table_contenttype_description;
var table_contenttype_keyCache;

var json_table_value;
var tableName;
var tableContentTypeName;
var ConnectString;
var dataProvider;
var ListDatabaseName = new Array();
var DatabaseTable;
var SourceName;
var ColumnName;
var ServerName;
var UserName;
var Password;
var LookupDataType;
var iconPath;
var keyCacheIcon;
var contenttype_id;

var $contenttype_fields;
var $contenttype_properties;
var $batchtype_fields;
var barcodeEdit = -1;//1:Edit barcode, -1: Add new barcode
var $barcode_type;
var $barcode_position;
var $barcode_separate_document;
var $barcode_remove_separator;
var $barcode_copy_value_to_field_id;
var $barcode_copy_value_to_field;
var $barcode_do_lookup;
var barcode_configure_id;
var $json_barcode_fields = { save_barcode_fields: [], delete_barcode_fields: [] };

// Using Bootstrap Js
$(function () {
    $("[rel='tooltip']").tooltip();
    $(".tooltip").tooltip();
});

$(function () {

    var current_field; //dung de giu row_field de thuc hien xoa
    var current_index_field;
    var current_content_field;

    // Show List Batch Type
    function ShowListContentType() {

        ///Get Ajax
        $("#left-container").ecm_loading_show();

        JsonHelper.helper.post(
            URL_ShowListBatchType,
            JSON.stringify(""),
            ShowListContentType_Success,
            ShowListContentType_Error);
    }
    function ShowListContentType_Success(data) {
        $(".left-container-child").remove();
        $("#left-container").append(data);
        $(".sub_menu_item").first().find(".row_batch_type").click();
        $("#left-container").ecm_loading_hide();
    }

    function ShowListContentType_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $("#left-container").ecm_loading_hide();
    }
    // End Show List BatchType ===================== // 

    // Display batch type properties, Add batch type properties
    function BatchTypeProperties() {

        ///Get Ajax
        $("#right-container").ecm_loading_show();

        JsonHelper.helper.post(
            URL_BacthTypeProperties,
            JSON.stringify({ BatchId: _BatchID }),
            BatchTypeProperties_Success,
            BatchTypeProperties_Error);
    }
    function BatchTypeProperties_Success(data) {
        $("#panel-viewer-wrapper").find(".sub_properties_content").remove();
        $("#panel-viewer-wrapper").append(data);
        //if (_BatchID != -1) {
        //    GetTableValue2(); // trường hợp add new content mới thì không gọi GetTableValue2();
        //}
        $("#right-container").ecm_loading_hide();
    }
    function BatchTypeProperties_Error(data) {
        $("#panel-viewer-wrapper").find(".sub_properties_content").remove();
        $("#panel-viewer-wrapper").append(data);
        $("#right-container").ecm_loading_hide();
    }

    //End Bacth Type Properties

    // Display content type properties
    function ContentTypeProperties() {

        ///Get Ajax
        JsonHelper.helper.post(
            URL_ContentTypeProperties,
            JSON.stringify({ DocumentId: _DocumentID }),
            ContentTypeProperties_Success,
            ContentTypeProperties_Error);
    }
    function ContentTypeProperties_Success(data) {
        $("#table_content_field").find("tr.row_field").remove();
        $("#table_content_field").find("tbody").append(data);
        //$("#content_icon").attr("src", URL_GetImageFromCache + "?Key=" + data.tb);

    }

    function ContentTypeProperties_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    }
    //End Content Type Properties

    //Delete Batch type 
    function DeleteBatchType() {
        ///Get Ajax
        JsonHelper.helper.post(
            URL_DeleteBatchType,
            JSON.stringify({ BatchId: _BatchID }),
            DeleteBatchType_Success,
            DeleteBatchType_Error);
    }

    function DeleteBatchType_Success(data) {
        $(".sub_properties_content").remove();
        $("#left-container").find(".sub_menu_item.selected").remove();
        $(".sub_menu_item").first().find(".row_batch_type").click();
    }

    function DeleteBatchType_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $("#right-container").ecm_loading_hide();
    }
    //End Delete Batch Type


    //Reset dialog Content fields properies
    function ResetContentFieldProperiesInDialog() {
        $("#content_name").val("");
        $("#content_description").val("");
        $("#table_content_field").find("tr.row_field").remove();
        $("#content_icon").attr("src", "/Resources/DocumentTypeIcon/appbar.page.text.png");
        $("#content_icon").attr("data-keycache", "");
    }
    //Reset dialog  add content field
    function ResetContentFieldInDialog() {
        $("#field_name").val("");
        $("#required:checked").removeAttr("checked");
        $("#restricted:checked").removeAttr("checked");
        $("#use_current_date").removeAttr("checked");
        $("#dataType").prop('selectedIndex', 0);
        $("#max_length").val("0");
        $("#default_value").val("");
        $("#valida_script_value").val("");
        $("#valida_pattern_value").val("");
        $("#configure_picklist").hide();
        $("#current_date").hide();
        $("#valida_script").show();
        $("#valida_pattern").show();

        $("#select_true_false").hide();
        $("#configure_table").hide();
        $("#default_value").prop('disabled', false);
        $("#default_value_title").show();
        $("#max_length").prop('disabled', false);
    }
    //Reset dialog  add field
    function ResetFieldInDialog() {
        $("#index_field_name").val("");
        $("#index_use_current_date").removeAttr("checked");
        $("#index_dataType").prop('selectedIndex', 0);
        $("#index_max_length").val("0");
        $("#index_default_value").val("");
        $("#index_current_date").hide();

        $("#index_select_true_false").hide();
        $("#index_default_value").prop('disabled', false);
        $("#default_value_title").show();
        $("#index_max_length").prop('disabled', false);
    }

    //Save batch type
    function SaveBatchType() {
        ///Get Ajax
        $("#right-container").ecm_loading_show();
        json.Name = (json.Name + "").replace(/\s+/g, ' ').trim();
        JsonHelper.helper.post(
            URL_SaveBatchType,
            JSON.stringify({ batchtypemodel: json, listdoctypemodel: json_list_contenttypes[json.Name], picklist: picklistvalue, keyCacheIcon: keyCacheIcon }),
            SaveBatchType_Success,
            SaveBatchType_Error);
    }
    function SaveBatchType_Success(data) {
        $("#right-container").ecm_loading_hide();
        if (data != EmptyId) {
            if (data.indexOf(EmptyId + "_") != 0) {
                if (isEditContentType == -1) {
                    $(".left-container-child").ecm_loading_show();
                    var new_contentype_menu_item =
                        "<div class='sub_menu_item content_type_item clearfix selected'  id='" + data + "' >" +
                        "<div class='row_batch_type'>"+
                         "<div class='Dropdown-profile'>"+   
                                "<img class='Photo' src='" + URL_GetIcon + "?key=" + keyCacheIcon + "'/>" +
                                "<span class='Name'>"+json.Name+"</span>"+
                                "<span class='MenuIcon'>"+
                                    "<span class='MenuIcon-line'></span>"+
                                    "<span class='MenuIcon-line'></span>"+
                                    "<span class='MenuIcon-line'></span>"+
                                "</span>" +
                          "</div>"+
                           "<nav class='Dropdown-nav'>"+
                            "<ul class='Dropdown-group'>"+
                                "<li class='NavLink clearfix'>"+
                                    "<span class='pull-left'>Workflow</span>"+
                                    "<span class='pull-right entypo-plus'>"+
                                        "<a href='' class='' id='button_add_field'>"+
                                            "<i class='glyphicon glyphicon-cog white'></i>"+
                                        "</a> | <a class='' id='button_add_field'>"+
                                            '<i class="glyphicon glyphicon-trash white"></i>'+
                                        "</a>"+
                                    "</span>"+
                                "</li>"+
                                "<li class='NavLink clearfix'>"+
                                    "<span class='pull-left'>Barcode</span>"+
                                    "<span class='pull-right entypo-plus'>"+
                                        "<a href='' class='barcode_config' id='button_add_field' data-id='@item.Value.Id'>"+
                                            "<i class='glyphicon glyphicon glyphicon-barcode white'></i>"+
                                        "</a> | <a class='' id='button_add_field'>"+
                                            "<i class='glyphicon glyphicon-trash white'></i>"+
                                        "</a>"+
                                    "</span>"+
                                "</li>"+
                            "</ul>"+
                          "</nav>"                           
                        "</div>";
                    $(".admin_sub_menu_items").find(".sub_menu_item").removeClass("selected");
                    $(".admin_sub_menu_items").append(new_contentype_menu_item);
                    $(".left-container-child").ecm_loading_hide();
                    $(".admin_sub_menu_items").find(".sub_menu_item.selected").find(".row_batch_type").click();
                } else {
                    var $this_chage = $(".admin_sub_menu_items").find(".sub_menu_item.selected");
                    $this_chage.find(".icon img").removeAttr("style");
                    $this_chage.find(".item_content_data").text(json.Name);
                    $this_chage.find(".item_content_data").attr("title", json.Name);
                    $this_chage.find(".icon img").attr("src", $("#right-container").find("#image_icon").attr("src"));
                    $(".admin_sub_menu_items").find(".sub_menu_item.selected").find(".row_batch_type").click();
                }
            } else {
                var field_name = data.split("00000000-0000-0000-0000-000000000000_");
                $.innoDialog({
                    title: 'Warning information',
                    width: 350,
                    dialog_data: '<div class="message_infor">' + field_name[1] + ' already has existed</div>',
                    type: 'Ok',
                    Ok_Button: function () {
                        $(this).dialog('close');
                        var $this_fields = $("#table_content").find("#table_content_body tr#-1");
                        $.each($this_fields, function () {
                            if ($(this).find(".td_field_name").text() == field_name[1]) {
                                $(this).addClass("selected")
                                return false;
                            }

                        });
                    }
                });

            }
        } else {
            $.innoDialog({
                title: 'Warning information',
                width: 350,
                dialog_data: '<div class="message_infor">' + json.Name + ' already has existed</div>',
                type: 'Ok',
                Ok_Button: function () {
                    $(this).dialog('close');
                    $("#right-container").find("#content_name").focus();
                }
            });
        }
    }

    function SaveBatchType_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $("#right-container").ecm_loading_hide();

    }
    // The End save content
     
    // Get Picklist Value type
    function GetPicklistValue() {

        ///Get Ajax
        $("#right-container").ecm_loading_show();

        JsonHelper.helper.post(
            URL_GetPicklistValue,
            JSON.stringify({ DocTypeID: _DocumentID, FieldID: _FieldID }),
            GetPicklistValue_Success,
            GetPicklistValue_Error);
    }
    function GetPicklistValue_Success(data) {

        //$("#picklist_value").text("");
        $("#picklist_value").val(data);
        $("#picklist_value_dialog").dialog("open");
        //$(".sub_properties").ecm_loading_hide();
    }

    function GetPicklistValue_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        // $(".sub_properties").ecm_loading_hide();

    }
    // ============== Ket thuc GetPicklistValue =======//

    // ============== Dung de lay gia tri cua cac fields table =========
    function GetTableValue() {

        ///Get Ajax
        $("#right-container").ecm_loading_show();

        JsonHelper.helper.post(
            URL_GetTableValue,
            JSON.stringify({ BatchId: _BatchID }),
            GetTableValue_Success,
            GetTableValue_Error);
    }
    function GetTableValue_Success(data) {
        //json_table_value = data;
        json_table_value = jQuery.parseJSON(data);
        $("#right-container").ecm_loading_hide();
    }

    function GetTableValue_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $("#right-container").ecm_loading_hide();

    }
    // ============== Ket thuc GetTableValue =======//
    function GetTableValue2() {
        $.ajax({
            url: URL_GetTableValue,
            type: "POST",
            async: true,
            data: JSON.stringify({ BatchtId: _BatchID }),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                //QueryExpression = jQuery.parseJSON(Data);
                json_table_value = data;
                json_table_fields = {};
                $("#table_fields").find('tr').each(function (index) {
                    if (index != 0) {
                        var id_field = $(this).attr("id");
                        var td_field_name = $(this).find(".td_field_name").text();
                        var td_data_type = $(this).find(".td_data_type").text();

                        if (td_data_type == "Table") {
                            var Array_Temp = new Array();
                            for (i = 0 ; i < json_table_value.TableValue.length; i++) {
                                if (json_table_value.TableValue[i].ParentFieldId == id_field) {
                                    Array_Temp.push(json_table_value.TableValue[i]);
                                    //json_table_fields[td_field_name] = json_table_value.TableValue[i];
                                }
                            }
                            json_table_fields[td_field_name] = Array_Temp;
                        }
                    }
                });
            }
        });
    }
    //Load table configure
    function LoadTableConfigure() {

        ///Get Ajax
        JsonHelper.helper.post(
            URL_LoadTableConfigure,
            JSON.stringify({ DocTypeID: _DocumentID, FieldID: _FieldID }),
            LoadTableConfigure_Success,
            LoadTableConfigure_Error);
    }
    function LoadTableConfigure_Success(data) {

        //$("#picklist_value").text("");
        //$("#picklist_value").val(data);
        // $("#picklist_value_dialog").dialog("open");
        //$(".sub_properties").ecm_loading_hide();
        $("#table_configuration").find("tr.table_configure_row").remove();
        $("#table_configuration").find("tbody").append(data);
    }

    function LoadTableConfigure_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    }
    // End Load table configure //  

    //Edit table configure unsaved 
    function EditTableConfigure() {

        ///Get Ajax
        JsonHelper.helper.post(
            URL_EditTableConfigure,
            JSON.stringify({ tableColumn: json_table_fields[tableName] }),
            EditTableConfigure_Success,
            EditTableConfigure_Error);
    }
    function EditTableConfigure_Success(data) {

        //$("#picklist_value").text("");
        //$("#picklist_value").val(data);
        // $("#picklist_value_dialog").dialog("open");
        //$(".sub_properties").ecm_loading_hide();
        $("#table_configuration").find("tr.table_configure_row").remove();
        $("#table_configuration").find("tbody").append(data);
    }

    function EditTableConfigure_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    }
    // End edit table configure 

    // Dùng để edit table configuration khi fields table mới được thêm 
    function EditTableConfigureEvent() {
        EditTableConfigure();
    }

    //Edit table content type properties 
    function EditTableContentType() {
        ///Get Ajax
        JsonHelper.helper.post(
            URL_EditTableContentType,
            JSON.stringify({ tableColumn: json_table_fields[tableContentTypeName] }),
            EditTableContentType_Success,
            EditTableContentType_Error);
    }

    function EditTableContentType_Success(data) {
        $("#table_content_field").find("tr.row_field").remove();
        $("#table_content_field").find("tbody").append(data);
    }

    function EditTableContentType_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    }
    // End edit table content type properties  

    // Save content
    function SaveBatchTypeEvent() {
        if ($("#table_indexfield").find('tbody tr').length > 0)
        {
            var batchtype_name = $("#batch_name").val();
            var batchtype_description = $("#batch_description").val();
            var outlook = $("#outlook_import").val();
            if ($("#outlook_import").is(":checked")) {
                outlook = "true";
            }

            var batchField_DisplayOrder = 1;
            json = { "Name": batchtype_name, "Id": _BatchID, "Description": batchtype_description, "IsApplyForOutlook": outlook, Fields: [], IsOutlook: outlook, DeletedFields: [] };
            $("#table_indexfield").find('tr').each(function (index) {
                if (index != 0) {
                    var id_field = $(this).attr("id");
                    var td_field_name = $(this).find(".td_index_field_name").text();
                    var td_data_type = $(this).find(".td_index_data_type").text();
                    var td_default_value = $(this).find(".td_index_default_value").text();
                    var td_maxlength = $(this).find(".td_index_maxlength").text();
                    var use_current_date = false;
                    if (td_data_type == "Date" && td_default_value == "{Use current date}") {
                        td_default_value = "";
                        use_current_date = true;
                    }                   
                    json.Fields.push({ BatchTypeId: _BatchID, Id: id_field, Name: td_field_name, DataType: td_data_type, MaxLength: td_maxlength, DefaultValue: td_default_value, DisplayOrder: batchField_DisplayOrder, UseCurrentDate: use_current_date });
                    batchField_DisplayOrder++;
                }
            });

            //Icon value
            keyCacheIcon = $("#image_icon").data("keycache");

            // if outlook is oncheck 
            //if (outlook == "true" && isEditBatchType != 1) {
            //    json.Fields.push({ BatchTypeId: _BatchID, Name: "Mail body", DataType: "String", IsRequired: "True", IsRestricted: "No", DisplayOrder: batchField_DisplayOrder });
            //    batchField_DisplayOrder++;
            //    json.Fields.push({ BatchTypeId: _BatchID, Name: "Mail from", DataType: "String", IsRequired: "True", IsRestricted: "No", DisplayOrder: batchField_DisplayOrder });
            //    batchField_DisplayOrder++;
            //    json.Fields.push({ BatchTypeId: _BatchID, Name: "Mail to", DataType: "String", IsRequired: "True", IsRestricted: "No", DisplayOrder: batchField_DisplayOrder });
            //    batchField_DisplayOrder++;
            //    json.Fields.push({ BatchTypeId: _BatchID, Name: "Received date", DataType: "Date", IsRequired: "True", IsRestricted: "No", DisplayOrder: batchField_DisplayOrder });
            //}

            //added to fields be removed to perform the delete function
            json.DeletedFields = json_batchType_deletefields.batchType_DeletedFields;

            var json_of_each_row = { tablefiels: [] };
            if ($("#table_content_types").find('tbody tr').length > 0)
            {
                $("#table_content_types").find('tr').each(function (index) {
                    if (index != 0) {
                        var id_field = $(this).attr("id");
                        var table_contenttype_name = $(this).find(".td_content_name").text();
                        var table_contenttype_description = $(this).find(".td_content_description").text();
                        var table_contenttype_keyCache = $(this).find(".td_content_keyCache").text();

                        json_of_each_row.tablefiels.push({ DocTypeId: _DocumentID, Id: id_field, Name: table_contenttype_name, Description: table_contenttype_description, IconBase64: table_contenttype_keyCache, Fields: json_table_fields[tableContentTypeName] });
                    }
                });
                json_list_contenttypes[batchtype_name] = json_of_each_row.tablefiels;

                //added to fields be removed to perform the delete function
                json_list_contenttypes[batchtype_name].DeleteContentType = json_contentType_deletefields.contentType_DeletedFields;

                SaveBatchType();

            } else {
                $.innoDialog({
                    title: 'Cloud ECM',
                    width: 350,
                    dialog_data: '<div>Content type has least one field!</div>',
                    type: 'Ok',
                    Ok_Button: function () {
                        $(this).dialog("close");
                    }
                });
            }
        }else {
            $.innoDialog({
                title: 'Cloud ECM',
                width: 350,
                dialog_data: '<div>Batch type has least one field!</div>',
                type: 'Ok',
                Ok_Button: function () {
                    $(this).dialog("close");
                }
            });
        }
    }

    // Append first row of table configure 
    function AppendFirstRowTableConfigure() {
        $("#table_configuration").append("<tr class='table_configure_row first_row' id='-1'>"
                                                + "<td id='first_delete_column'><button type='button' class='btn-delele btn-primary'><i class='glyphicon glyphicon-remove'></i></button></td>"
                                                + "<td class='table_td_name'><input type='text' class='table_name_value' placeholder='New column'></td>"
                                                + "<td class='table_td_type'>"
                                                    + "<select  class='table_data_type'>"
                                                       + "<option value='String' selected>String</option>"
                                                       + "<option value='Integer'>Integer</option>"
                                                       + "<option value='Decimal'>Decimal</option>"
                                                       + "<option value='Date'>Date</option>"
                                                   + "</select>"

                                               + "</td>"
                                               + "<td class='table_td_maxlength'><input type='text' class='table_maxlength' value='0'></td>"
                                               + "<td class='table_td_default_value'>"
                                                   + "<input type='text' class='table_default_value'>"
                                                   + "<div class='table_use_current_date_contain' style='display: none;'>"
                                                   + "<input type='checkbox' class='table_check_use_current_date' value='false'><a>Use current date</a>"
                                                   + "</div>"
                                               + "</td>"
                                             + "</tr>");
    }

    //Event Change Table Data Type 
    function ChangeTableDataType(DataType) {
        switch (DataType) {
            case "String":
                {

                    $("tr.row_selected").find(".table_td_default_value").find(".table_default_value").show();
                    $("tr.row_selected").find(".table_td_default_value").find(".table_use_current_date_contain").hide();
                    $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").prop('disabled', false);

                    break;

                }
            case "Decimal":
                {
                    $("tr.row_selected").find(".table_td_default_value").find(".table_default_value").show();
                    $("tr.row_selected").find(".table_td_default_value").find(".table_use_current_date_contain").hide();
                    $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").prop('disabled', true);
                    $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").val("0");
                    break;
                }
            case "Integer":
                {
                    $("tr.row_selected").find(".table_td_default_value").find(".table_default_value").show();
                    $("tr.row_selected").find(".table_td_default_value").find(".table_use_current_date_contain").hide();
                    $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").prop('disabled', true);
                    $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").val("0");
                    break;
                }

            case "Date":
                {
                    $("tr.row_selected").find(".table_td_default_value").find(".table_default_value").hide();
                    $("tr.row_selected").find(".table_td_default_value").find(".table_use_current_date_contain").show();
                    $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").prop('disabled', true);
                    $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").val("0");
                    break;
                }
        }
    };

    //Event change datatype  content fields
    function ChangeDataTypeContentField() {
        var datatype = $("#dataType").val();
        switch (datatype) {
            case "String":
                {
                    $("#configure_picklist").hide();
                    $(".current_date").hide();
                    $("#select_true_false").hide();
                    $("#configure_table").hide();
                    $("#default_value").show();
                    $("#default_value").prop('disabled', false);
                    $("#max_length").prop('disabled', false);
                    $("#valida_script").show();
                    $("#valida_pattern").show();
                    break;

                }
            case "Decimal":
                {
                    $("#configure_picklist").hide();
                    $(".current_date").hide();
                    $("#select_true_false").hide();
                    $("#configure_table").hide();
                    $("#default_value").show();
                    $("#default_value").prop('disabled', false);
                    $("#max_length").val("0");
                    $("#max_length").prop('disabled', true);
                    $("#valida_script").show();
                    $("#valida_pattern").show();
                    break;
                }
            case "Integer":
                {
                    $("#configure_picklist").hide();
                    $(".current_date").hide();
                    $("#select_true_false").hide();
                    $("#configure_table").hide();
                    $("#default_value").show();
                    $("#default_value").prop('disabled', false);
                    $("#max_length").prop('disabled', true);
                    $("#max_length").val("0");
                    $("#valida_script").show();
                    $("#valida_pattern").show();
                    break;
                }
            case "Picklist":
                {
                    $("#configure_picklist").show();
                    $(".current_date").hide();
                    $("#select_true_false").hide();
                    $("#configure_table").hide();
                    $("#default_value").show();
                    $("#default_value").prop('disabled', false);
                    $("#max_length").prop('disabled', true);
                    $("#max_length").val("0");
                    $("#valida_script").hide();
                    $("#valida_pattern").hide();
                    break;
                }
            case "Boolean":
                {
                    $("#configure_picklist").hide();
                    $(".current_date").hide();
                    $("#select_true_false").show();
                    $("#configure_table").hide();
                    $("#default_value").hide();
                    $("#max_length").prop('disabled', true);
                    $("#max_length").val("0");
                    $("#valida_script").hide();
                    $("#valida_pattern").hide();
                    break;
                }
            case "Date":
                {
                    $("#configure_picklist").hide();
                    $(".current_date").show();
                    $("#select_true_false").hide();
                    $("#configure_table").hide();
                    $("#default_value").hide();
                    $("#max_length").prop('disabled', true);
                    $("#max_length").val("0");
                    $("#valida_script").hide();
                    $("#valida_pattern").hide();
                    break;
                }
            case "Table":
                {
                    $("#configure_picklist").hide();
                    $(".current_date").hide();
                    $("#select_true_false").hide();
                    $("#configure_table").show();
                    $("#default_value").show();
                    $("#default_value").prop('disabled', false);
                    $("#max_length").prop('disabled', true);
                    $("#max_length").val("0");
                    $("#default_value_title").hide();
                    $("#valida_script").hide();
                    $("#valida_pattern").hide();
                    break;
                }
            case "Folder":
                {
                    $("#configure_picklist").hide();
                    $(".current_date").hide();
                    $("#select_true_false").hide();
                    $("#configure_table").hide();
                    $("#default_value").show();
                    $("#default_value").prop('disabled', true);
                    $("#max_length").prop('disabled', true);
                    $("#max_length").val("0");
                    $("#valida_script").hide();
                    $("#valida_pattern").hide();
                }

        }
    }

    //Event change datatype fields
    function ChangeDataTypeField() {
        var datatype = $("#index_dataType").val();
        switch (datatype) {
            case "String":
                {
                    $(".index_current_date").hide();
                    $("#index_select_true_false").hide();
                    $("#index_default_value").show();
                    $("#index_default_value").prop('disabled', false);
                    $("#index_max_length").prop('disabled', false);
                    break;

                }
            case "Decimal":
                {
                    $(".index_current_date").hide();
                    $("#index_select_true_false").hide();
                    $("#index_default_value").show();
                    $("#index_default_value").prop('disabled', false);
                    $("#index_max_length").val("0");
                    $("#index_max_length").prop('disabled', true);
                    break;
                }
            case "Integer":
                {
                    $(".index_current_date").hide();
                    $("#index_select_true_false").hide();
                    $("#index_default_value").show();
                    $("#default_value").prop('disabled', false);
                    $("#index_max_length").prop('disabled', true);
                    $("#index_max_length").val("0");
                    break;
                }
            case "Boolean":
                {
                    $(".index_current_date").hide();
                    $("#index_select_true_false").show();
                    $("#index_default_value").hide();
                    $("#index_max_length").prop('disabled', true);
                    $("#index_max_length").val("0");
                    break;
                }
            case "Date":
                {
                    $(".index_current_date").show();
                    $("#index_select_true_false").hide();
                    $("#index_default_value").hide();
                    $("#index_max_length").prop('disabled', true);
                    $("#index_max_length").val("0");
                    break;
                }
        }
    }

    // tab in lookup dialog
    $(function () {
        $("#dialog_lookup_content").tabs();
    });

    //Test connection
    function TestConnection() {

        ///Get Ajax
        $("#right-container").ecm_loading_show();

        JsonHelper.helper.post(
            URL_TestConnection,
            JSON.stringify({ connectionString: ConnectString, dataProviderString: dataProvider }),
            TestConnection_Success,
            TestConnection_Error);
    }

    function TestConnection_Success(data) {

        ListDatabaseName = data;
        /* for (var i = 0; i < data.length; i++)
         {
             ListDatabaseName.push(data[i]);
         }*/
        alert("Connect success");
    }

    function TestConnection_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        // $(".sub_properties").ecm_loading_hide();

    }
    // End test connection

    // test connection 
    function TestConnection2() {
        $.ajax({
            url: URL_TestConnection,
            type: "POST",
            async: true,
            data: JSON.stringify({ connectionString: ConnectString, dataProviderString: dataProvider }),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.Testconnectionfail == "Test connection fail") {
                    alert("Connect fail");
                } else {
                    ListDatabaseName = data;

                    // $("#database_select").append();
                    // alert("Success");
                    $("#select_database").find("option").remove();
                    // $("#select_database").append("<option>Empty</option>");
                    for (var i = 0; i < ListDatabaseName.TableValue.length; i++) {
                        $("#select_database").append("<option>" + ListDatabaseName.TableValue[i] + "</option>");
                    }
                    $("#dialog_lookup_content").tabs("enable", 1);
                    alert("Test success");
                }
            }
        });
    }
    // Lấy các bảng dữ liệu trong databse
    function GetDataSource() {
        $.ajax({
            url: URL_GetDataSource,
            type: "POST",
            async: true,
            data: JSON.stringify({ connetionString: ConnectString, dataProvider: dataProvider, type: LookupDataType }),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (data) {

                DatabaseTable = data;
                $("#select_data_source").find("option").remove();
                $("#select_data_source").append("<option>Empty</option>");
                for (var i = 0; i < DatabaseTable.data_table.length; i++) {
                    $("#select_data_source").append("<option>" + DatabaseTable.data_table[i] + "</option>");
                }
                alert("success");
            }
        });
    }
    // lấy các column trong database table
    function GetColumns() {
        $.ajax({
            url: URL_GetColumns,
            type: "POST",
            async: true,
            data: JSON.stringify({ sourceName: SourceName, connectionString: ConnectString, dataProvider: dataProvider, type: "Table" }),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (data) {

                ColumnName = data;
                $(".select_column").find("option").remove();
                $(".select_column").append("<option>Empty</option>");
                for (var i = 0; i < ColumnName.ColumnbValue.length; i++) {
                    $(".select_column").append("<option>" + ColumnName.ColumnbValue[i] + "</option>");
                }
                alert("success");
            }
        });
    }
    
    //Choose icon process
    function ChooseIconProcess() {
        UploadIcon();
    }

    //Upload Icon
    function UploadIcon() {
        $.ajax({
            url: URL_BatchTypeIcon,
            type: 'post',
            data: { path: iconPath },
            success: UploadIcon_Success,
            error: UploadIcon_Error
        })
    }

    function UploadIcon_Success(data) {
        $("#dialog_image_icon").attr("src", URL_GetImageFromCache + "?Key=" + data);
        $("#dialog_image_icon").attr("data-keycache", data);
    }
    function UploadIcon_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    }

    function UploadBatchIcon() {
        $(".fieldset_batch_icon").ecm_loading_show();
        $("#user_form").ajaxSubmit({
            url: URL_UploadProfilePic,
            success: UploadBatchIcon_Success,
            error: UploadBatchIcon_Error
        })
    }

    function UploadBatchIcon_Success(data) {
        $("#dialog_image_icon").attr("src", URL_GetImageFromCache + "?Key=" + data);
        $("#dialog_image_icon").attr("data-keycache", "" + data);
        $(".fieldset_batch_icon").ecm_loading_hide();
    }

    function UploadBatchIcon_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $(".fieldset_batch_icon").ecm_loading_hide();
    }


    //Delete ocr template
    function DeleteOcrTemplate() {
        JsonHelper.helper.post(
            URL_DeleteOcrTemplate,
            JSON.stringify({ id: contenttype_id }),
            DeleteOcrTemplate_Success,
            DeleteOcrTemplate_Error
        );
    }

    function DeleteOcrTemplate_Success(data) {
        var $this_menu_item = $(".admin_sub_menu_items").find(".sub_menu_item.selected");
        $this_menu_item.find(".delete_ocr").addClass("add_delete_ocr_icon");
        $this_menu_item.find(".delete_ocr").removeClass("delete_ocr");
        var $this_barcode_configure = $("#right-container").find("#docViewer");
        if ($this_barcode_configure.length > 0) {
            $this_menu_item.find(".ocr_template").click();
        }
    }

    function DeleteOcrTemplate_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    }

    //Show barcode properties
    function ShowBarcodeConfigure() {
        $("#right-container").ecm_loading_show();
        JsonHelper.helper.post(
            URL_ShowBarcodeConfigure,
            JSON.stringify({ id: contenttype_id }),
            ShowBarcodeConfigure_Success,
            ShowBarcodeConfigure_Error
        );
    }

    function ShowBarcodeConfigure_Success(data) {
        $("#right-containers").find(".sub_properties_content").remove();
        $("#dialog_barcode_configure").find("#barcode_fields").find("option").remove();
        $("#right-container").append(data);
        //ShowListBarcodeFields(lstFields, lstBarcode);
        $list_content_fields = lstFields;
        //called from effect.js
        //setButtonWidthInFirefox();
        resize_vetical_properties_content();
        resize_vetical_properties_top_down_height()//(height of down_height class)
        reszie_vetical_fieldset();
        resize_vertical_multi_data();
        //End called from effect.js

        $("#right-container").ecm_loading_hide();
    }

    function ShowBarcodeConfigure_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    }

    //Save barcode fields
    function SaveBarcode() {
        JsonHelper.helper.post(
            URL_SaveBarcodeFields,
            JSON.stringify($json_barcode_fields),
            SaveBarcode_Success,
            SaveBarcode_Error
        );
    }

    function SaveBarcode_Success(data) {
        $("#right-container").ecm_loading_hide();
        ShowBarcodeConfigure();
        $(".admin_sub_menu_items").ecm_loading_show();
        if ($("#barcode_table").find("#barcode_table_body tr").length > 0) {
            $(".admin_sub_menu_items").find(".sub_menu_item.selected").find(".add_delete_barcode_icon").addClass("delete_barcode");
            $(".admin_sub_menu_items").find(".sub_menu_item.selected").find(".add_delete_barcode_icon").removeClass("add_delete_barcode_icon");
        } else {
            $(".admin_sub_menu_items").find(".sub_menu_item.selected").find(".delete_barcode").addClass("add_delete_barcode_icon");
            $(".admin_sub_menu_items").find(".sub_menu_item.selected").find(".delete_barcode").removeClass("delete_barcode");
        }
        $(".admin_sub_menu_items").ecm_loading_hide();
    }

    function SaveBarcode_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    }

    //Delete barcode config
    function DeleteBarcode() {
        JsonHelper.helper.post(
            URL_DeleteBarcode,
            JSON.stringify({ id: contenttype_id }),
            DeleteBarcode_Success,
            DeleteBarcode_Error
        );
    }

    function DeleteBarcode_Success(data) {
        var $this_menu_item = $(".admin_sub_menu_items").find(".sub_menu_item.selected");
        $this_menu_item.find(".delete_barcode").addClass("add_delete_barcode_icon");
        $this_menu_item.find(".delete_barcode").removeClass("delete_barcode");
        var $this_barcode_configure = $("#right-container").find("#barcode_table");
        if ($this_barcode_configure.length > 0) {
            $this_menu_item.find(".barcode_config").click();
        }
    }

    function DeleteBarcode_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    }

    //Show list dialog barcode fields
    //First: show ui
    var $list_barcode_field = [];
    var $list_used_field = [];
    //Second: event for add, edit, delete
    var $list_content_fields;

    var $list_used_field_in_ui = [];
    var $list_field_will_show_ui = [];
    var $copy_value_to_field_id_edit;

    var get_field_used_in_ui = function () {
        var $this_tr = $("#barcode_table").find("#barcode_table_body tr");
        var result = [];
        $.each($this_tr, function () {
            var push_value = $(this).find(".td_copy_value_to_field").attr("id");
            if (push_value != "null" && push_value != $copy_value_to_field_id_edit) {
                result.push(push_value);
            }
        });
        return result;
    }

    var get_field_will_show_ui = function () {
        var result = [];
        $.each($list_content_fields, function (index_field, field_item) {
            if (!field_item.IsSystemField) {
                if ($list_used_field_in_ui.toString() != '') {
                    var this_has_used = false;
                    $.each($list_used_field_in_ui, function (index_used, used_field_item) {
                        if (field_item.Id == used_field_item) {
                            this_has_used = true;
                            return false;
                        }
                    })
                    if (!this_has_used) {
                        result.push(field_item);
                    }

                } else {
                    result.push(field_item);
                }
            }
        });
        return result;
    }

    function ShowListBarcodeUIFields() {
        var result = "<option id='null' value=''>Select field</option>";
        $.each($list_field_will_show_ui, function (index, field_show_item) {
            result += "<option id='" + field_show_item.Id + "' value='" + field_show_item.Name + "'>" + field_show_item.Name + "</option>";
        });
        $("#dialog_barcode_configure").find("select#barcode_fields").append(result);
        $copy_value_to_field_id_edit = "";
    };

    $(document).ready(function () {

        $('#user_form').submit(function () {
            $.ajax({
                type: "POST",
                url: URL_UploadBatchIcon,
                data: $(this).serialize(),
                success: function (response) {
                    alert('ok-' + data + '-' + respText + '-' + xhr);
                    alert("Details saved successfully!!!");
                },
                error: function (request, status, error) {
                    alert(request.responseText);
                }
            });
        })
        //Reveals an elements popover.
        $('#myButton').tooltip();
        $(".validascript").popover({
            trigger: 'click',
            placement: 'left',
            html: 'true',
            content: '<div>Greater Than:<<value>> > 5 <br /> Greater Than or Equals:<<value>> > 5 <br />Greater Than than with And:<<value>> > 5 && <<value>> <= 10<br />Less Than with Or:<<value>> > 5 || <<value>> >=10"></div>'
        })
        $(".validapattern").popover({
            trigger: 'click',
            placement: 'left',
            html: 'true',
            content: '<div>Greater Than:<<value>> > 5 <br /> Greater Than or Equals:<<value>> > 5 <br />Greater Than than with And:<<value>> > 5 && <<value>> <= 10<br />Less Than with Or:<<value>> > 5 || <<value>> >=10"></div>'
        })
        //Load list bacth types on click nav menu
        ShowListContentType();
        LookupDataType = $("#display_table").val();
        $(".between_and_right").hide();

        $("#batch_types").click(function () {
            $("#nav-containerar").find(".nav-item").removeClass("selected");
            $(this).addClass("selected");

            $("#left-container").find("div").remove();
            $("#right-container").find(".sub_properties_content").remove();
            $("#right-container").css({ display: 'inline-block' });
            $("#left-container").css({ display: 'inline-block' });
            $(".between_and_right").find(".between_and_right_content").remove();
            $(".between_and_right").hide();
            ShowListContentType();
        });
        // sự kiện khi click vào mỗi content type
        $(document).on("click", ".row_batch_type", function () {
            $("#right-container").find(".sub_properties_content").remove();
            $(".row_batch_type").removeClass("is-expanded");
            $(this).toggleClass('is-expanded');
            $(".sub_menu_item").removeClass("selected");
            $("#button_delete_batch").prop("disabled", false);
            $(this).parent().addClass("selected");
            isEditBatchType = 1;
            _BatchID = $(".admin_sub_menu_items").find(".sub_menu_item.selected").attr("id");

            BatchTypeProperties();


            //GetTableValue2();
            // alert("Da click content type");

        });
        // Event on click button Add New Batch 
        $(document).on("click", "#button_add_batch", function () {
            $(".row_batch_type").removeClass("is-expanded");
            $("#right-container").find(".sub_properties_content").remove();
            _BatchID = -1; // đặt lại ID là -1 để không load properties của content hiện tại
            isEditBatchType = -1;
            $(".sub_menu_item").removeClass("selected"); // remove selected batch type
            $("#button_delete_batch").prop("disabled", true);
            BatchTypeProperties();
        });

        // nút delete content
        var dialog_delete_batchtype_yes_function = function () {
            DeleteBatchType();
            $(this).dialog("close");
        };

        var dialog_delete_batchtype_no_function = function () {
            $(this).dialog("close");
        };

        $(document).on("click", "#button_delete_batch", function () {
            _BatchID = $(".admin_sub_menu_items").find(".sub_menu_item.selected").attr("id");
            $.innoDialog({
                title: 'Cloud ECM',
                width: 350,
                dialog_data: $('#dialog_delete_batchtype'),
                type: 'Yes_No',
                Yes_Button: dialog_delete_batchtype_yes_function,
                No_Button: dialog_delete_batchtype_no_function
            });
        });


        // Event on click button add content fields
        $(document).on("click", "#btn_add_contentfield", function () {
            isEdit = -1;
            _FieldID = -1;

            $contenttype_fields = new Array();
            var lstField = $("#table_content_field").find("#table_content_field_body tr");
            $.each(lstField, function () {
                $contenttype_fields.push($(this).find(".td_field_name").text());
            });

            ResetContentFieldInDialog();

            $.innoDialog({
                title: 'Fields Properties',
                width: 580,
                dialog_data: $('#dialog_add_content_field'),
                open: dialog_add_content_field_open_function,
                type: 'Ok_Cancel',
                Ok_Button: dialog_add_content_field_ok_function,
                Cancel_Button: dialog_add_content_field_cancel_function
            });

        });

        // issue open dialog add content field
        var dialog_add_content_field_open_function = function (event, ui) {
            if (isEdit == 1) {
                var date_type = $("tr.row_field.selected").find(".td_data_type").text();
                var field_name = $("tr.row_field.selected").find(".td_field_name").text();
                var required = $("tr.row_field.selected").find(".td_required").text();
                var restricted = $("tr.row_field.selected").find(".td_restricted").text();
                var maxlength = $("tr.row_field.selected").find(".td_maxlength").text();
                var default_value = $("tr.row_field.selected").find(".td_default_value").text();
                var valida_script_value = $("tr.row_field.selected").find(".td_valida_script").text();
                var valida_pattern_value = $("tr.row_field.selected").find(".td_valida_pattern").text();
                if (default_value == "{Use current date}") {
                    $("#use_current_date").prop("checked", true);
                }
                $("#field_name").val(field_name);
                $("#dataType").val(date_type);
                if (required == "Yes") {
                    $("#required").prop("checked", true);
                }
                if (restricted == "Yes") {
                    $("#restricted").prop("checked", true);
                }
                $("#default_value").val(default_value);
                $("#select_true_false").val(default_value);
                $("#max_length").val(maxlength);
                $("#valida_script_value").val(valida_script_value);
                $("#valida_pattern_value").val(valida_pattern_value);
                ChangeDataTypeContentField();
            }
        }

        var dialog_add_content_field_ok_function = function () {
            var field_name = $("#field_name").val();
            var data_type = $("#dataType").val();
            var valida_script = $("#valida_script_value").val();
            var valida_pattern = $("#valida_pattern_value").val();
            var configure = "";
            if (data_type == "String" || data_type == "Integer" || data_type == "Decimal") {
                configure = "Configure";
            }

            var required = $("#required").val();
            var required_title = "No";
            if ($("#required").is(":checked")) {
                required = "true";
                required_title = "Yes"
            }
            var restricted = $("#restricted").val();
            var restricted_title = "No"
            if ($("#restricted").is(":checked")) {
                restricted = "true";
                restricted_title = "Yes"
            }
            var maxlength = $("#max_length").val();
            var default_value = $("#default_value").val();
            var picklist = $("#picklist_value").val();

            var use_current_date = $("#use_current_date").val();
            if ($("#use_current_date").is(":checked")) {
                use_current_date = "true";
            }
            if (data_type == "Boolean") {
                default_value = $("#select_true_false").val();
            }
            if (data_type == "Date") {
                if (use_current_date == "true") {
                    default_value = "{Use current date}";
                }
                else default_value = "";
            }
            if (data_type == "Table") {
                var json_of_each_row = { tablefiels: [] };
                $("#table_configuration").find('tr').each(function (index) {
                    if (index != 0) {
                        table_field_name = $(this).find(".table_td_name").find(".table_name_value").val();
                        table_field_datatype = $(this).find(".table_td_type").find(".table_data_type").val();
                        table_field_maxlength = $(this).find(".table_td_maxlength").find(".table_maxlength").val();
                        table_field_default_value = $(this).find(".table_td_default_value").find(".table_default_value").val();
                        var table_field_id = $(this).attr("id");
                        var td_use_current_date = $(this).find(".table_td_default_value").find(".table_use_current_date_contain").find(".table_check_use_current_date");
                        table_field_use_current_date = td_use_current_date.val();
                        if (td_use_current_date.is(":checked")) {
                            table_field_use_current_date = "true";
                        }

                        json_of_each_row.tablefiels.push({ DocTypeId: _DocumentID, ParentFieldId: _FieldID, Id: table_field_id, Name: table_field_name, DataType: table_field_datatype, DefaultValue: table_field_default_value, UseCurrentDate: table_field_use_current_date, MaxLength: table_field_maxlength });
                    }
                });
                json_table_fields[field_name] = json_of_each_row.tablefiels;

                // reset column on table confugure 
                $("#table_configuration").find("tr.table_configure_row").remove();// delete all lines
                AppendFirstRowTableConfigure(); // added to the first line

            }

            var field_name_existed = false;
            $.each($contenttype_fields, function (index, value) {
                if ($("#field_name").val() == value) {
                    field_name_existed = true;
                    return false;
                }
            })

            if (CheckField($("#field_name"), "Field name", 0, 0)) {
                if (!field_name_existed) {
                    if ($("#dataType").val() != "String") {
                        $("#max_length").val("1");
                    }
                    if (CheckIntegerNumber($("#max_length"), "Max lengh")) {
                        if ($("#dataType").val() != "String") {
                            $("#max_length").val("0");
                        }
                        var checkDefaultValue = true;

                        if ($("#dataType").val() == "String") {
                            if ($("#default_value").val().length > parseInt($("#max_length").val())) {
                                ErrorDialog($("#default_value"), "Maximum length value of '" + $("#field_name").val() + "' field: " + $("#max_length").val());
                                checkDefaultValue = false;
                            }
                        }

                        if ($("#dataType").val() == "Integer") {
                            if (default_value == "0") {
                                $("#default_value").val("1");
                            }
                            if (!CheckIntegerNumber($("#default_value"), "Default value")) {
                                checkDefaultValue = false;
                            }
                        }

                        if ($("#dataType").val() == "Decimal") {
                            if (default_value == "0") {
                                $("#default_value").val("1");
                            }
                            if (!CheckDecimalNumber($("#default_value"), "Default value")) {
                                checkDefaultValue = false;
                            }
                        }
                        if (checkDefaultValue) {
                            if (isEdit == -1) {
                                $("#table_content_field_body").append("<tr class='row_field' id='-1'><td class='td_field_name'>"
                                                                + field_name + "</td><td class='td_data_type'>"
                                                                + data_type + "</td><td class='td_required'>"
                                                                + required_title + "</td><td class='td_restricted'>"
                                                                + restricted_title + "</td><td class='td_haslookup hidden'>"
                                                                + "No" + "</td><td class='td_default_value'>"
                                                                + default_value + "</td><td class='td_maxlength'>"
                                                                + maxlength + "</td><td  class='td_picklist hidden'>"
                                                                + picklist + "</td><td  class='td_valida_script hidden'>"
                                                                + valida_script + "</td><td  class='td_valida_pattern hidden'>"
                                                                + valida_pattern + "</td");
                                $("#table_content_field").find("#table_content_field_body").find("tr#-1").last().click();
                            } else {
                                $("tr.row_field.selected").find(".td_data_type").text(data_type);
                                $("tr.row_field.selected").find(".td_field_name").text(field_name);
                                $("tr.row_field.selected").find(".td_required").text(required_title);
                                $("tr.row_field.selected").find(".td_restricted").text(restricted_title);
                                $("tr.row_field.selected").find(".td_maxlength").text(maxlength);
                                $("tr.row_field.selected").find(".td_default_value").text(default_value);
                                $("tr.row_field.selected").find(".td_picklist").text(picklist);
                                $("tr.row_field.selected").find(".td_valida_script").text(valida_script);
                                $("tr.row_field.selected").find(".td_valida_pattern").text(valida_pattern);
                            }
                            $("#picklist_value").val("");
                            $(this).dialog("close");
                            ResetContentFieldInDialog();
                        }
                    }
                } else {
                    $.innoDialog({
                        title: 'Warning information',
                        width: 350,
                        dialog_data: '<div class="message_infor">' + $("#field_name").val() + ' already has existed!</div>',
                        type: 'Ok',
                        Ok_Button: function () {
                            $(this).dialog('close');
                            $("#field_name").focus();
                        }
                    });
                }
            }
        }

        var dialog_add_content_field_cancel_function = function () {
            $(this).dialog("close");
            // reset rows on table configuration 
            $("#table_configuration").find("tr.table_configure_row").remove();// delete all lines
            AppendFirstRowTableConfigure(); // added to the first line
            ResetContentFieldInDialog();
        }
        // End Event on click button add content fields



        // Event on click button add batch fields
        $(document).on("click", "#button_add_field", function () {
            isEditIndexField = -1;

            $batchtype_fields = new Array();
            var lstField = $("#table_indexfield").find("#table_indexfield_body tr");
            $.each(lstField, function () {
                $batchtype_fields.push($(this).find(".td_index_field_name").text());
            });

            ResetFieldInDialog();

            $.innoDialog({
                title: 'Fields Properties',
                width: 580,
                dialog_data: $('#dialog_add_field'),
                open: dialog_add_field_open_function,
                type: 'Ok_Cancel',
                Ok_Button: dialog_add_field_ok_function,
                Cancel_Button: dialog_add_field_cancel_function
            });

        });

        // issue open dialog add field
        var dialog_add_field_open_function = function (event, ui) {
            if (isEditIndexField == 1) {
                var date_type = $("tr.row_index_field.selected").find(".td_index_data_type").text();
                var field_name = $("tr.row_index_field.selected").find(".td_index_field_name").text();
                var maxlength = $("tr.row_index_field.selected").find(".td_index_maxlength").text();
                var default_value = $("tr.row_index_field.selected").find(".td_index_default_value").text();
                if (default_value == "{Use current date}") {
                    $("#index_use_current_date").prop("checked", true);
                }
                $("#index_field_name").val(field_name);
                $("#index_dataType").val(date_type);
                $("#index_default_value").val(default_value);
                $("#index_select_true_false").val(default_value);
                $("#index_max_length").val(maxlength);
                ChangeDataTypeField();
            }
        }

        var dialog_add_field_ok_function = function () {
            var index_field_name = $("#index_field_name").val();
            var index_data_type = $("#index_dataType").val();
            var index_configure = "";
            if (index_data_type == "String" || index_data_type == "Integer" || index_data_type == "Decimal") {
                index_configure = "Configure";
            }
            var index_maxlength = $("#index_max_length").val();
            var index_default_value = $("#index_default_value").val();
            var index_use_current_date = $("#index_use_current_date").val();
            if ($("#index_use_current_date").is(":checked")) {
                index_use_current_date = "true";
            }
            if (index_data_type == "Boolean") {
                index_default_value = $("#index_select_true_false").val();
            }
            if (index_data_type == "Date") {
                if (index_use_current_date == "true") {
                    index_default_value = "{Use current date}";
                }
                else index_default_value = "";
            }

            var batchtype_fieldname_existed = false;
            $.each($batchtype_fields, function (index, value) {
                if ($("#index_field_name").val() == value) {
                    batchtype_fieldname_existed = true;
                    return false;
                }
            })

            if (CheckField($("#index_field_name"), "Field name", 0, 0)) {
                if (!batchtype_fieldname_existed) {
                    if ($("#index_dataType").val() != "String") {
                        $("#index_max_length").val("1");
                    }
                    if (CheckIntegerNumber($("#index_max_length"), "Max lengh")) {
                        if ($("#index_dataType").val() != "String") {
                            $("#index_max_length").val("0");
                        }
                        var checkDefaultValue = true;

                        if ($("#index_dataType").val() == "String") {
                            if ($("#index_default_value").val().length > parseInt($("#index_max_length").val())) {
                                ErrorDialog($("#index_default_value"), "Maximum length value of '" + $("#index_field_name").val() + "' field: " + $("#index_max_length").val());
                                checkDefaultValue = false;
                            }
                        }

                        if ($("#index_dataType").val() == "Integer") {
                            if (default_value == "0") {
                                $("#index_default_value").val("1");
                            }
                            if (!CheckIntegerNumber($("#index_default_value"), "Default value")) {
                                checkDefaultValue = false;
                            }
                        }

                        if ($("#index_dataType").val() == "Decimal") {
                            if (default_value == "0") {
                                $("#index_default_value").val("1");
                            }
                            if (!CheckDecimalNumber($("#index_default_value"), "Default value")) {
                                checkDefaultValue = false;
                            }
                        }
                        if (checkDefaultValue) {
                            if (isEditIndexField == -1) {
                                $("#table_indexfield_body").append("<tr class='row_index_field' id='-1'><td class='td_index_field_name'>"
                                                                + index_field_name + "</td><td class='td_index_data_type'>"
                                                                + index_data_type + "</td><td class='td_index_default_value'>"
                                                                + index_default_value + "</td><td class='td_index_maxlength'>"
                                                                + index_maxlength + "</td>");
                                $("#table_indexfield").find("#table_indexfield_body").find("tr#-1").last().click();
                            } else {
                                $("tr.row_index_field.selected").find(".td_index_data_type").text(index_data_type);
                                $("tr.row_index_field.selected").find(".td_index_field_name").text(index_field_name);
                                $("tr.row_index_field.selected").find(".td_index_maxlength").text(index_maxlength);
                                $("tr.row_index_field.selected").find(".td_index_default_value").text(index_default_value);
                            }
                            $(this).dialog("close");
                            ResetFieldInDialog();
                        }
                    }
                } else {
                    $.innoDialog({
                        title: 'Warning information',
                        width: 350,
                        dialog_data: '<div class="message_infor">' + $("#index_field_name").val() + ' already has existed!</div>',
                        type: 'Ok',
                        Ok_Button: function () {
                            $(this).dialog('close');
                            $("#index_field_name").focus();
                        }
                    });
                }
            }
        }

        var dialog_add_field_cancel_function = function () {
            $(this).dialog("close");
            ResetFieldInDialog();
        }
        //End Event on click button add batch fields



        //Event change value on datatype combobox
        $(document).on("change", "#dataType", function () {
            ChangeDataTypeContentField();
        });
        //Event change value on index_dataType combobox
        $(document).on("change", "#index_dataType", function () {
            ChangeDataTypeField();
        });

        //sự kiện khi click vào button cancel
        $(document).on("click", "#button_cancel_content", function () {
            $("#right-container").find(".sub_properties_content").remove();
            json_table_fields = {};
            json_list_contenttypes = {};
            json_delete_fields = { DeletedFields: [] };
            json_delete_fields.DeletedFields.push({ Id: -1 });
            json_table_value = {};
            json_pick_value = "";
            $(".sub_menu_item").first().find(".row_batch_type").click();
        });


        // Event when click on each line (field) of each content type
        $(document).on("click", "tr.row_field", function () {

            current_field = $(this);
            $("tr.row_field").removeClass("selected");
            $(this).addClass("selected");
            var td_name = $(this).find(".td_data_type").text();
            if (td_name == "Table") {
                tableName = $(this).find(".td_field_name").text();
            }
        });

        // Event when click on each line (field) of each index_field
        $(document).on("click", "tr.row_index_field", function () {

            current_index_field = $(this);
            $("tr.row_index_field").removeClass("selected");
            $(this).addClass("selected");
        });

        // Event when click on each line (field) of each index_field
        $(document).on("click", "tr.row_content", function () {

            current_content_field = $(this);
            $("tr.row_content").removeClass("selected");
            $(this).addClass("selected");
            tableContentTypeName = $(this).find(".td_content_type").text();
        });


        // Event on click button #btn_delete_contentfield
        $(document).on("click", "#btn_delete_contentfield", function () {
            var id_field_delete = $("tr.row_field.selected").attr("id");
            var name_field_delete = $("tr.row_field.selected").find(".td_field_name").text();
            $("tr.row_field.selected").remove();
            json_delete_fields.DeletedFields.push({ Id: id_field_delete, Name: name_field_delete });
        });

        // Event on click button #button_delete_field
        $(document).on("click", "#button_delete_field", function () {
            var id_field_delete = $("tr.row_index_field.selected").attr("id");
            var name_field_delete = $("tr.row_index_field.selected").find(".td_index_field_name").text();
            $("tr.row_index_field.selected").remove();
            json_delete_fields.DeletedFields.push({ Id: id_field_delete, Name: name_field_delete });
        });


        // Event on click button add content types
        $(document).on("click", "#btn_add_content", function () {
            isEditContentType = -1;
            _ContentTypeFieldID = -1;

            $contenttype_properties = new Array();
            var lstField = $("#table_content_types").find("#table_content_types_body tr");
            $.each(lstField, function () {
                $contenttype_properties.push($(this).find(".td_content_name").text());
            });


            ResetContentFieldProperiesInDialog();
            $("#table_content_field").find("tr.row_field").remove();

            $.EcmDialog({
                title: 'Content type properties',
                width: 780,
                dialog_data: $('#dialog_add_Content'),
                open: dialog_add_content_open_function,
                type: 'Ok_Cancel',
                Ok_Button: dialog_add_content_ok_function,
                Cancel_Button: dialog_add_content_cancel_function
            });
        });
        // processing dialog  for add content types
        var dialog_add_content_open_function = function (event, ui) {

            if (isEditContentType == 1) {
                var content_type = $("tr.row_content.selected").find(".td_content_name").text();
                var content_description = $("tr.row_content.selected").find(".td_content_description").text();
                var content_keyCache = $("tr.row_content.selected").find(".td_content_keyCache").text();
                $("#content_name").val(content_type);
                $("#content_description").val(content_description);
                $("#content_icon").attr("src", URL_GetImageFromCache + "?Key=" + content_keyCache);
                $("#content_icon").attr("data-keycache", content_keyCache);

                _DocumentID = $("tr.row_content.selected").attr("id");
                if (_DocumentID != EmptyId && _DocumentID != -1) {

                    ContentTypeProperties();
                }
            }
        }
        // processing dialog for add content types successfully
        var dialog_add_content_ok_function = function () {
           var content_type = $("#content_name").val();
           var content_description = $("#content_description").val();
           var content_keyCache = $("#content_icon").attr("data-keycache");

           var json_of_each_row = { tablefiels: [] };
           var contentType_DisplayOrder = 1;
           $("#table_content_field").find('tr').each(function (index) {
               if (index != 0) {
                   table_content_field_name = $(this).find(".td_field_name").text();
                   table_content_field_datatype = $(this).find(".td_data_type").text();

                   var restricted_value = $(this).find(".td_restricted").text();
                   if (restricted_value == "Yes") {
                       table_content_field_isrestricted = true;
                   } else {
                       table_content_field_isrestricteds = false;
                   }

                   var required_value = $(this).find(".td_required").text();
                   if (required_value == "Yes") {
                       table_content_field_isrequired = true;
                   } else {
                       table_content_field_isrequired = false;
                   }

                   table_content_field_hasloopup = $(this).find(".td_haslookup").text();
                   table_content_field_defaultvalue = $(this).find(".td_default_value").text();
                   table_content_field_maxlenth = $(this).find(".td_maxlength").text();
                   table_content_field_picklist = $(this).find(".td_picklist").text();
                   table_content_field_valida_script = $(this).find(".td_valida_script").text();
                   table_content_field_valida_pattern = $(this).find(".td_valida_pattern").text();

                   table_content_field_use_current_date = false;
                   if (table_content_field_datatype == "Date" && table_content_field_defaultvalue == "{Use current date}") {

                       table_content_field_defaultvalue = "";
                       table_content_field_use_current_date = true;
                   }

                   var table_field_id = $(this).attr("id");

                   if (table_content_field_datatype == "Picklist") {

                       //picklistvalue = picklistvalue + table_content_field_picklist + "#";
                       json_pick_value.tablefiels.push({ Id: table_field_id, FieldId: _ContentTypeFieldID, Value: table_content_field_picklist });
                   }

                   json_of_each_row.tablefiels.push({ DocTypeId: _DocumentID, Id: table_field_id, ParentFieldId: _ContentTypeFieldID, Name: table_content_field_name, DataType: table_content_field_datatype, IsRequired: table_content_field_isrequired, IsRestricted: table_content_field_isrestricted, MaxLength: table_content_field_maxlenth, DefaultValue: table_content_field_defaultvalue, DisplayOrder: contentType_DisplayOrder, ValidationScript: table_content_field_valida_script, ValidationPattern: table_content_field_valida_pattern, UseCurrentDate: table_content_field_use_current_date, Children: json_table_fields[table_content_field_name] });
                   contentType_DisplayOrder++;
               }
           });
           json_table_fields[content_type] = json_of_each_row.tablefiels;

           var contenttype_fieldname_existed = false;
           //$.each($contenttype_properties, function (index, value) {
           //     if ($("#content_name").val() == value) {
           //         contenttype_fieldname_existed = true;
           //         return false;
           //     }
           // })

            if (CheckField($("#content_name"), "Field name", 0, 0)) {
                if (!contenttype_fieldname_existed) {
                    if (isEditContentType == -1) {

                        $("#table_content_types_body").append("<tr class='row_content' id='-1'><td class='td_content_name'>"
                                                        + content_type + "</td><td class='td_content_description'>"
                                                        + content_description + 
                                                        "</td><td class='td_OCRTemplate'>" +
                                                            "<a  class='btn-mini btn' id='button_add_field' type='button'>" +
                                                                "<span>Configure</span><i class='glyphicon glyphicon-cog'></i>" +
                                                            "</a><a class='btn-mini btn' id='button_add_field' type='button'>" +
                                                                "<span>Delete</span><i class='glyphicon glyphicon-trash'></i></a>"+
                                                        "</td><td class='td_content_keyCache hidden'>"
                                                        + content_keyCache + "</td>");
                        $("#table_content_types").find("#table_content_types_body").find("tr#-1").last().click();
                    } else {
                        $("tr.row_content.selected").find(".td_content_name").text(content_type);
                        $("tr.row_content.selected").find(".td_content_description").text(content_description);
                        $("tr.row_content.selected").find(".td_content_keyCache").text(content_keyCache);
                    }
                    $(this).dialog("close");
                    ResetContentFieldProperiesInDialog();
                } else {
                    $.EcmDialog({
                        title: 'Warning information',
                        width: 350,
                        dialog_data: '<div class="message_infor">' + $("#content_name").val() + ' already has existed!</div>',
                        type: 'Ok',
                        Ok_Button: function () {
                            $(this).dialog('close');
                            $("#content_name").focus();
                        }
                    });
                }
            }
        }

        // processing dialog for add content types failure
        var dialog_add_content_cancel_function = function () {
            $(this).dialog("close");
            // reset lại các row trên table configuration 
            $("#table_configuration").find("tr.table_configure_row").remove();// xóa tất cả các dòng
            AppendFirstRowTableConfigure(); // thêm vào dòng đầu tiên
            ResetContentFieldProperiesInDialog();
        }


        //Event on click  button #btn_edit_content 
        $(document).on("click", "#btn_edit_content", function () {

            isEditContentType = 1;         
            _ContentTypeFieldID = $("tr.row_content.selected").attr("id");

            $contenttype_properties = new Array();
            var lstField = $("#table_content_types").find("#table_content_types_body tr");
            $.each(lstField, function () {
                if ($(this).attr("class") != ".row_content selected") {
                    $contenttype_properties.push($(this).find(".td_content_name").text());
                }
            });

            if (_ContentTypeFieldID == -1) {

                EditTableContentType();
            }           

            $.innoDialog({
                title: 'Content type properties',
                width: 780,
                dialog_data: $('#dialog_add_Content'),
                open: dialog_add_content_open_function,
                type: 'Ok_Cancel',
                Ok_Button: dialog_add_content_ok_function,
                Cancel_Button: dialog_add_content_cancel_function
            });
        });


        // sự kiện khi click button_save_Batch
        $(document).on("click", "#button_save_batchType", function () {
            if (CheckField($("#batch_name"), "Batch type name", 0, 0)) {
                SaveBatchTypeEvent();
                picklistvalue = "";
                _FieldID = -1;
                _ContentTypeFieldID = -1;
                json_table_fields = {};
                json_list_contenttypes = {};
            }
        });

        //Event on click  button #btn_edit_contentfield 
        $(document).on("click", "#btn_edit_contentfield", function () {

            isEdit = 1;
            _FieldID = $("tr.row_field.selected").attr("id");
            if ($("tr.row_field.selected").find(".td_data_type").text() == "Table" && _FieldID != -1 || _FieldID == EmptyId) {
                //GetPicklistValue();
                LoadTableConfigure();
            }
            else if ($("tr.row_field.selected").find(".td_data_type").text() == "Table" && _FieldID == -1) {
                EditTableConfigure();
            }
            //LoadTableConfigure();

            $contenttype_fields = new Array();
            var lstField = $("#table_content_field").find("#table_content_field_body tr");
            $.each(lstField, function () {
                if ($(this).attr("class") != "row_field selected") {
                    $contenttype_fields.push($(this).find(".td_field_name").text());
                }
            });

            $.innoDialog({
                title: 'Fields properties',
                width: 580,
                dialog_data: $('#dialog_add_content_field'),
                open: dialog_add_content_field_open_function,
                type: 'Ok_Cancel',
                Ok_Button: dialog_add_content_field_ok_function,
                Cancel_Button: dialog_add_content_field_cancel_function
            });
                Close_Button: dialog_table_configure_close_function
        });
        //Event on click  button #button_edit_field 
        $(document).on("click", "#button_edit_field", function () {

            isEditIndexField = 1;

            $batchtype_fields = new Array();
            var lstField = $("#table_indexfield").find("#table_indexfield_body tr");
            $.each(lstField, function () {
                if ($(this).attr("class") != "row_index_field selected") {
                    $batchtype_fields.push($(this).find(".td_index_field_name").text());
                }
            });

            $.innoDialog({
                title: 'Fields properties',
                width: 580,
                dialog_data: $('#dialog_add_field'),
                open: dialog_add_field_open_function,
                type: 'Ok_Cancel',
                Ok_Button: dialog_add_field_ok_function,
                Cancel_Button: dialog_add_field_cancel_function
            });
        });

        //Event on click configure picklist
        var picklist_value_dialog_open_function = function (event, ui) {
            if (isEditIndexField != 1) {
                $("#picklist_value").val("");
            }
        };

        var picklist_value_dialog_ok_function = function () {
            //alert("" + $("#picklist_value").val());
            //$(".td_picklist").text($("#picklist_value").val());
            // picklistvalue = picklistvalue +"#"+ $("#picklist_value").val();              

            // $("#picklist_value").val("");
            $(this).dialog("close");
            // ResetFieldInDialog();
        };

        var picklist_value_dialog_cancel_function = function () {
            $("#picklist_value").val("");
            $(this).dialog("close");
            // ResetFieldInDialog();
        };

        $(document).on("click", "#configure_picklist", function () {

            $.innoDialog({
                title: 'Configure picklist',
                width: 580,
                dialog_data: $('#picklist_value_dialog'),
                open: picklist_value_dialog_open_function,
                type: 'Ok_Cancel',
                Ok_Button: picklist_value_dialog_ok_function,
                Cancel_Button: picklist_value_dialog_cancel_function
            });

            $("#picklist_value").val($("tr.row_field.selected").find(".td_picklist").text()); // gán giá trị cho piclick
        });

        // Event on click configure table
        // dialog table configure
        var dialog_table_configure_open_function = function (event, ui) {
            //if (isEdit == 1) {
            //    $("#table_configuration").find("tr.table_configure_row").remove();
            //}
        };

        var dialog_table_configure_close_function = function () {
            var $table_configure = $("#table_configuration");
            var lstTableConfigureRow = $table_configure.find("tbody tr.table_configure_row");
            var isClose = false;

            var $this = $table_configure.find(".table_configure_row.first_row").val();
            var table_name_value_val = $(this).find(".table_name_value").val();
            var table_data_type_val = $(this).find(".table_data_type").val();
            var table_maxlength_val = $(this).find(".table_maxlength").val();
            var table_default_value_val = $(this).find(".table_default_value").val();


            if (lstTableConfigureRow.length == 1
                && table_name_value_val == ""
                && table_data_type_val == "String"
                && table_maxlength_val == "0"
                && table_default_value_val == "") {
                isClose = true;
            } else {
                $.each(lstTableConfigureRow, function (index) {
                    $(".table_configure_row").removeClass("checking");
                    $(this).addClass("checking");
                    var $checking = $table_configure.find("tbody tr.table_configure_row.checking");
                    var $table_name_value = $(this).find(".table_name_value");
                    var $table_data_type = $(this).find(".table_data_type");
                    var $table_maxlength = $(this).find(".table_maxlength");
                    var $table_default_value = $(this).find(".table_default_value");

                    var default_val = $table_default_value.val();

                    if (CheckField($table_name_value, "Column name", 0, 0)) {
                        if ($table_data_type.val() != "String") {
                            $table_maxlength.val("1");
                        }

                        if (CheckIntegerNumber($table_maxlength, "Max lengh")) {
                            if ($table_data_type.val() != "String") {
                                $table_maxlength.val("0");
                            }

                            var checkDefaultValue = true;
                            if ($table_data_type.val() == "String") {
                                if ($table_default_value.val().length > parseInt($table_maxlength.val())) {
                                    ErrorDialog($table_default_value, "Maximum length value of '" + $table_name_value.val() + "' column: " + $table_maxlength.val());
                                    return false;
                                }
                            }

                            if ($table_data_type.val() == "Integer") {
                                if (default_val == "0") {
                                    $table_default_value.val("1");
                                }
                                if (CheckIntegerNumber($table_default_value, "Default value", 0, 0)) {
                                    if (default_val == "0") {
                                        $table_default_value.val("0");
                                    }
                                } else {
                                    return false;
                                }
                            }

                            if ($table_data_type.val() == "Decimal") {
                                if (default_val == "0") {
                                    $table_default_value.val("1");
                                }
                                if (CheckDecimalNumber($table_default_value, "Default value", 0, 0)) {
                                    if (default_val == "0") {
                                        $table_default_value.val("0");
                                    }
                                } else {
                                    return false;
                                }
                            }

                        } else {
                            return false;
                        }

                    } else {
                        return false;
                    }
                    if (index == lstTableConfigureRow.length - 1) {
                        isClose = true;
                    }
                });
            }
            if (isClose) {
                $(this).dialog("close");
            }
        };

        // Event on click btn configure table
        $(document).on("click", "#configure_table", function () {
            $.innoDialog({
                title: 'Table columns configuration',
                width: 860,
                dialog_data: $('#dialog_table_configure'),
                open: dialog_table_configure_open_function,
                type: 'Close',
                Close_Button: dialog_table_configure_close_function,
            });

        });
        // Event on click add new columns in table configuration
        $(document).on("click", "#add_new_column", function () {

            $("#table_configuration").append("<tr class='table_configure_row' id='-1'>"
                                                  + "<td class='table_td_delete'><button type='button' class='btn-delele btn-primary'><i class='glyphicon glyphicon-remove'></i></button></td>"
                                                  + "<td class='table_td_name'><input type='text' class='table_name_value' placeholder='New column'></td>"
                                                  + "<td class='table_td_type'>"
                                                      + "<select  class='table_data_type'>"
                                                         + "<option value='String' selected>String</option>"
                                                         + "<option value='Integer'>Integer</option>"
                                                         + "<option value='Decimal'>Decimal</option>"
                                                         + "<option value='Date'>Date</option>"
                                                     + "</select>"

                                                 + "</td>"
                                                 + "<td class='table_td_maxlength'><input type='text' class='table_maxlength' value='0'></td>"
                                                 + "<td class='table_td_default_value'>"
                                                     + "<input type='text' class='table_default_value'>"
                                                     + "<div class='table_use_current_date_contain' style='display:none;'>"
                                                     + "<input type='checkbox' class='table_check_use_current_date' value='false'><a>Use current date</a>"
                                                     + "</div>"
                                                 + "</td>"
                                               + "</tr>");

        });
        // sự kiện khi click vào dấu x trên table configuration trên table configuration dialog
        $(document).on("click", ".table_td_delete", function () {

            // $(this).parent().remove();
            // var id_field_delete = $("tr.row_field.selected").attr("id");
            var id_field_delete = $(this).parent().attr("id");
            $(this).parent().remove();
            json_delete_fields.DeletedFields.push({ Id: id_field_delete });

        });
        // sự kiện khi click vào dấu x đầu tiên trên table configuration dialog 
        $(document).on("click", "#first_delete_column", function () {
            $(this).parent().remove();
            AppendFirstRowTableConfigure();


        });
        //sự kiện khi click thay đổi các giá trị của select datatye trên Table configuration
        $(document).on("change", ".table_data_type", function () {



            //$(".table_configure_row").removeClass("row_selected");
            // $(this).parent().parent().addClass("row_selected");
            ChangeTableDataType($(this).val());
            //alert("Đã change "+ $(this).val());

        });

        $(document).on("focus", ".table_name_value, .table_data_type, .table_maxlength, .table_default_value", function () {
            $(".table_configure_row").removeClass("row_selected");
            $(this).parent().parent().addClass("row_selected");
        });

        // sự kiện khi click vào Configure
        var dialog_lookup_ok_function = function () {
            $(this).dialog("close");
        };

        var dialog_lookup_cancel_function = function () {
            $(this).dialog("close");
        };

        $(document).on("click", ".td_lookup", function () {
            $.innoDialog({
                title: 'Lookup configuration information',
                width: 580,
                dialog_data: $('#dialog_lookup'),
                type: 'Ok_Cancel',
                Ok_Button: dialog_lookup_ok_function,
                Cancel_Button: dialog_lookup_cancel_function
            });
        });
        // sự kiện khi click vào test connection button 
        $(document).on("click", "#button_test_connection", function () {
            //alert("Đã click vào test connection");
            ServerName = $("#server_name").val();
            UserName = $("#user_name").val();
            Password = $("#pass_word").val();
            ConnectString = "Server = " + ServerName + "; User Id = " + UserName + "; Password = " + Password + ";";
            dataProvider = "System.Data.SqlClient";
            TestConnection2();
            //alert("Đã click vào test connection" + servername + " user : " + username + "pass : " + password);
        });
        $("#dialog_lookup_content").tabs({ disabled: [1] });
        // sự kiện khi change select_database 
        $(document).on("change", "#select_database", function () {
            var databaseName = $("#select_database").val();
            ConnectString = "Server = " + ServerName + "; User Id = " + UserName + "; Password = " + Password + ";" + "Database= " + databaseName + ";";
            GetDataSource();


        });
        //sự kiện khi change select_data_source
        $(document).on("change", "#select_data_source", function () {

            SourceName = $("#select_data_source").val();
            $("#table_look_data").find('tr.lookup_data_row').remove();
            $("#table_fields").find('tr').each(function (index) {
                if (index != 0) {
                    var fieldName = $(this).find(".td_field_name").text();
                    $("#table_look_data").append("<tr class='lookup_data_row'>"
                                                    + "<td>" + fieldName + "</td>"
                                                    + "<td class='td_select_column'>"
                                                       + "<select class='select_column'>"

                                                       + "</select>"
                                                  + " </td>"
                                                  + "</tr>");
                }
            });
            GetColumns();
        });
        // sự kiện 
        $('#display_table').change(function () {
            if ($(this).is(":checked")) {
                var databaseName = $("#select_database").val();
                LookupDataType = $(this).val();
                $("#table_look_data").find('tr.lookup_data_row').remove();
                ConnectString = "Server = " + ServerName + "; User Id = " + UserName + "; Password = " + Password + ";" + "Database= " + databaseName + ";";
                GetDataSource();
            }

        });
        // sự kiện khi check vào checkbox display view
        $('#display_view').change(function () {
            if ($(this).is(":checked")) {
                var databaseName = $("#select_database").val();
                LookupDataType = $(this).val();
                $("#table_look_data").find('tr.lookup_data_row').remove();
                ConnectString = "Server = " + ServerName + "; User Id = " + UserName + "; Password = " + Password + ";" + "Database= " + databaseName + ";";
                GetDataSource();
            }

        });
        // sự kiện khi check vào checkbox display procedure
        $('#display_procedure').change(function () {
            if ($(this).is(":checked")) {
                var databaseName = $("#select_database").val();
                LookupDataType = $(this).val();
                $("#table_look_data").find('tr.lookup_data_row').remove();
                ConnectString = "Server = " + ServerName + "; User Id = " + UserName + "; Password = " + Password + ";" + "Database= " + databaseName + ";";
                GetDataSource();
            }

        });

        //Event choose icon for content type
        $(document).on("click", "#sub_image_icon", function () {
            iconPath = $(this).attr("data-icon-path");
            ChooseIconProcess();
        })

        //Event click change icon for Batch type
        var change_icon = false;

        var dialog_batch_icon_ok_function = function () {
            var keyCache = $("#dialog_image_icon").attr("data-keycache");
            $("#image_icon").attr("src", URL_GetImageFromCache + "?Key=" + keyCache);
            $("#image_icon").attr("data-keycache", keyCache);
            change_icon = true;
            $(this).dialog("close");
            $("#dialog_image_icon").attr("src", "#");
            $("#dialog_image_icon").attr("data-keycache", "");
        };

        var dialog_batch_icon_cancel_fuction = function () {
            if (change_icon) {
                $("#dialog_image_icon").attr("src", "#");
                $("#dialog_image_icon").attr("data-keycache", "");
            }
            $(this).dialog("close");
        };

        $(document).on("click", ".change_batch_icon", function () {
            var keyCache = $("#image_icon").attr("data-keycache");
            $("#dialog_image_icon").attr("src", URL_GetImageFromCache + "?Key=" + keyCache);
            $("#dialog_image_icon").attr("data-keycache", keyCache);

            $.innoDialog({
                title: 'Change batch icon',
                width: 650,
                dialog_data: $('.dialog_content_icon'),
                type: 'Ok_Cancel',
                Ok_Button: dialog_batch_icon_ok_function,
                Cancel_Button: dialog_batch_icon_cancel_fuction
            });
        });

        //event click choose batch icon to upload 
        $(document).on("click", "#Choosebatchicon", function () {
            $("#uploadbatchicon").click();
        });

        //event click select batch icon to upload 
        $(document).on("change", "#uploadbatchicon", function () {
            UploadBatchIcon();
        });

        //Event click change icon for content type
        var content_change_icon = false;

        var dialog_content_icon_ok_function = function () {
            var keyCache = $("#dialog_image_icon").attr("data-keycache");
            $("#content_icon").attr("src", URL_GetImageFromCache + "?Key=" + keyCache);
            $("#content_icon").attr("data-keycache", keyCache);
            content_change_icon = true;
            $(this).dialog("close");
            $("#dialog_image_icon").attr("src", "#");
            $("#dialog_image_icon").attr("data-keycache", "");
        };

        var dialog_content_icon_cancel_fuction = function () {
            if (content_change_icon) {
                $("#dialog_image_icon").attr("src", "#");
                $("#dialog_image_icon").attr("data-keycache", "");
            }
            $(this).dialog("close");
        };

        $(document).on("click", ".change_content_icon", function () {

            //var keyCache = $("#content_icon").attr("data-keycache");
            //$("#dialog_image_icon").attr("src", URL_GetImageFromCache + "?Key=" + keyCache);
            //$("#dialog_image_icon").attr("data-keycache", keyCache);
            var url_content_icon = $("#content_icon").attr('src');
            $("#dialog_image_icon").attr("src", url_content_icon);

            $.innoDialog({
                title: 'Change content icon',
                width: 650,
                dialog_data: $('.dialog_content_icon'),
                type: 'Ok_Cancel',
                Ok_Button: dialog_content_icon_ok_function,
                Cancel_Button: dialog_content_icon_cancel_fuction
            });
        });

        //Event click delete ocr template
        var dialog_delete_orc_yes_function = function () {
            DeleteOcrTemplate();
            $(this).dialog("close");
        };

        var dialog_delete_orc_no_function = function () {
            $(this).dialog("close");
        };

        $(document).on("click", ".delete_ocr", function () {
            contenttype_id = $(this).attr("data-id");
            var contenttype_name = $(this).attr("data-contenttype_name");
            $(this).parentsUntil(".admin_sub_menu_items").find(".row_batch_type").click();

            $.innoDialog({
                title: 'Cloud ECM',
                width: 350,
                dialog_data: '<div>Are you sure you delete Ocr template of ' + contenttype_name + '</div>',
                type: 'Yes_No',
                Yes_Button: dialog_delete_orc_yes_function,
                No_Button: dialog_delete_orc_no_function
            });
        });

        //Event click delete barcode
        var dialog_delete_barcode_yes_function = function () {
            DeleteBarcode();
            $(this).dialog("close");
        };

        var dialog_delete_barcode_no_function = function () {
            $(this).dialog("close");
        };

        $(document).on("click", ".delete_barcode", function () {
            contenttype_id = $(this).attr("data-id");
            var contenttype_name = $(this).attr("data-contenttype_name");

            var $this_barcode_configure = $("#right-container").find("#barcode_table");
            if ($this_barcode_configure.length > 0) {

            } else {
                $(this).parentsUntil(".admin_sub_menu_items").find(".row_batch_type").click();
            }

            $.innoDialog({
                title: 'Cloud ECM',
                width: 350,
                dialog_data: '<div>Are you sure you delete Barcode of ' + contenttype_name + '</div>',
                type: 'Yes_No',
                Yes_Button: dialog_delete_barcode_yes_function,
                No_Button: dialog_delete_barcode_no_function
            });
        });

        //Reset all fields dialog barcode configure
        function ResetAllFielsBarcodeConfigure() {
            $("#barcode_type").val($("#barcode_type option:first").val());
            $("#barcode_position").val("0");
            $("#document_separator").prop('checked', false);
            $("#remove_separator_page").prop('checked', false);
            $("#barcode_fields").val($("#barcode_fields option:first").val());
            $("#barcode_fields").removeAttr("disabled");
            $("#do_lookup").prop('checked', false);
        };

        //Event click tr_barcode_table_body in barcode configuration
        $(document).on("click", "tr.tr_barcode_table_body", function () {
            barcode_configure_id = $(this).attr("id");
            $("#barcode_table_body").find("tr.tr_barcode_table_body").removeClass("selected");
            $(this).addClass("selected");
        });



        //Event click add fields of barcode
        function CheckBarcodePosition(optionvalue, position_obj) {
            var lstBarcodeOneType = $("#barcode_table").find(".tr_barcode_table_body");
            var lstPosition = [];
            var result = true;
            $.each(lstBarcodeOneType, function () {
                if (barcodeEdit == -1) {//edit
                    var type = $(this).find(".td_barcode_type").text();
                    var pos = $(this).find(".td_barcode_position").text();
                    if (type == optionvalue) {
                        lstPosition.push(pos);
                    }
                } else {//add new
                    if ($(this).attr("class") != "tr_barcode_table_body selected") {
                        var type = $(this).find(".td_barcode_type").text();
                        var pos = $(this).find(".td_barcode_position").text();
                        if (type == optionvalue) {
                            lstPosition.push(pos);
                        }
                    }
                }
            });

            $.each(lstPosition, function (index, value) {
                if (position_obj.val() == value) {
                    result = false;
                    return false;
                }
            });
            if (!result) {
                ErrorDialog(position_obj, "Position '" + position_obj.val() + "' has been used!");
                return result;
            } else {
                return result;
            }
        };

        var dialog_barcode_configure_save_function = function () {
            if (CheckField($("#barcode_position"), "Barcode position", 0, 0)) {
                if (CheckIntegerNumber($("#barcode_position"), "Barcode position")) {
                    $barcode_type = $("#barcode_type").val();
                    $barcode_position = $("#barcode_position").val();

                    if (CheckBarcodePosition($barcode_type, $("#barcode_position"))) {
                        if ($("#document_separator").is(":checked")) {
                            $barcode_separate_document = "Yes";
                        } else {
                            $barcode_separate_document = "No";
                        }

                        if ($("#remove_separator_page").is(":checked")) {
                            $barcode_remove_separator = "Yes";
                        } else {
                            $barcode_remove_separator = "No";
                        }
                        if ($barcode_separate_document == "No") {
                            if ($("#barcode_fields").val() != "") {
                                $barcode_copy_value_to_field_id = $("#barcode_fields").children(":selected").attr("id");
                            } else {
                                $barcode_copy_value_to_field_id = "null";
                            }
                            $barcode_copy_value_to_field = $("#barcode_fields").val();
                        } else {
                            $barcode_copy_value_to_field = "";
                            $barcode_copy_value_to_field_id = "null";
                        }
                        if ($("#do_lookup").is(":checked")) {
                            $barcode_do_lookup = "Yes";
                        } else {
                            $barcode_do_lookup = "No";
                        }

                        if (barcodeEdit == -1) {
                            $("#barcode_table").find("tbody#barcode_table_body").append(
                                "<tr class='tr_barcode_table_body' data-field_status='add_barcode_field'>"
                                    + "<td class='td_barcode_type'>" + $barcode_type + "</td>"
                                    + "<td class='td_barcode_position'>" + $barcode_position + "</td>"
                                    + "<td class='td_separate_document'>" + $barcode_separate_document + "</td>"
                                    + "<td class='td_remove_separator'>" + $barcode_remove_separator + "</td>"
                                    + "<td id='" + $barcode_copy_value_to_field_id + "' class='td_copy_value_to_field'>" + $barcode_copy_value_to_field + "</td>"
                                    + "<td class='td_do_lookup'>" + $barcode_do_lookup + "</td>" +
                                "</tr>"
                             );
                            $("#dialog_barcode_configure").find("#barcode_fields").find("option#" + $barcode_copy_value_to_field_id).remove();
                            $("#barcode_table").find("#barcode_table_body").find(".tr_barcode_table_body").last().click();
                        } else {
                            $("#barcode_table").find("tbody#barcode_table_body tr.selected").attr("data-field_status", "edit_barcode_field");
                            $("tr.tr_barcode_table_body.selected").find("td.td_barcode_type").text($barcode_type);
                            $("tr.tr_barcode_table_body.selected").find("td.td_barcode_position").text($barcode_position);
                            if ($("#document_separator").is(":checked")) {
                                $("tr.tr_barcode_table_body.selected").find("td.td_separate_document").text("Yes");
                            } else {
                                $("tr.tr_barcode_table_body.selected").find("td.td_separate_document").text("No");
                            }

                            if ($("#remove_separator_page").is(":checked")) {
                                $("tr.tr_barcode_table_body.selected").find("td.td_remove_separator").text("Yes");
                            } else {
                                $("tr.tr_barcode_table_body.selected").find("td.td_remove_separator").text("No");
                            }

                            $("tr.tr_barcode_table_body.selected").find("td.td_copy_value_to_field").text($barcode_copy_value_to_field);
                            $("tr.tr_barcode_table_body.selected").find("td.td_copy_value_to_field").attr("id", $barcode_copy_value_to_field_id);

                            if ($("#do_lookup").is(":checked")) {
                                $("tr.tr_barcode_table_body.selected").find("td.td_do_lookup").text("Yes");
                            } else {
                                $("tr.tr_barcode_table_body.selected").find("td.td_do_lookup").text("No");
                            }
                        }
                        $(this).dialog("close");
                        barcodeEdit = -1;
                        ResetAllFielsBarcodeConfigure();
                    }
                }
            }
        }

        var dialog_barcode_configure_cancel_function = function () {
            $(this).dialog("close");
            barcodeEdit = -1;
            ResetAllFielsBarcodeConfigure();
        }

        $(document).on("click", "#button_add_barcode", function () {
            $("#barcode_table").find(".tr_barcode_table_body").removeClass("selected");
            barcodeEdit = -1;
            $.innoDialog({
                title: 'Add barcode configuration',
                width: 580,
                dialog_data: $('#dialog_barcode_configure'),
                open: dialog_barcode_configure_open_function,
                type: 'Save_Cancel',
                Save_Button: dialog_barcode_configure_save_function,
                Cancel_Button: dialog_barcode_configure_cancel_function
            });

        });

        //Event click edit field of barcode dialog
        var dialog_barcode_configure_open_function = function () {

            $("#barcode_fields").find("option").remove();
            $list_used_field_in_ui = get_field_used_in_ui();
            $list_field_will_show_ui = get_field_will_show_ui();
            ShowListBarcodeUIFields();
            if (barcodeEdit == 1) {
                var $barcode_table_selected = $("#barcode_table").find(".tr_barcode_table_body.selected");
                var barcode_type = $barcode_table_selected.find(".td_barcode_type").text();
                var barcode_position = $barcode_table_selected.find(".td_barcode_position").text();
                var saperate_document = $barcode_table_selected.find(".td_separate_document").text();
                var remove_saperator = $barcode_table_selected.find(".td_remove_separator").text();
                var copy_value_to_field = $barcode_table_selected.find(".td_copy_value_to_field").text();
                var copy_value_to_field_id_edit = $barcode_table_selected.find(".td_copy_value_to_field").attr("id");
                var do_lookup = $barcode_table_selected.find(".td_do_lookup").text();

                var $dialog_barcode = $("#dialog_barcode_configure");
                $dialog_barcode.find("#barcode_type").val(barcode_type);
                $dialog_barcode.find("#barcode_position").val(barcode_position);

                if (saperate_document == "Yes") {
                    $dialog_barcode.find("#document_separator").prop("checked", true);
                    $("#barcode_fields").attr("disabled", "disabled");
                } else {
                    $dialog_barcode.find("#document_separator").prop("checked", false);
                    $("#barcode_fields").removeAttr("disabled");
                }

                if (remove_saperator == "Yes") {
                    $dialog_barcode.find("#remove_separator_page").prop("checked", true);
                } else {
                    $dialog_barcode.find("#remove_separator_page").prop("checked", false);
                }

                $dialog_barcode.find("#barcode_fields").val(copy_value_to_field);

                if (do_lookup == "Yes") {
                    $dialog_barcode.find("#do_lookup").prop("checked", true);
                } else {
                    $dialog_barcode.find("#do_lookup").prop("checked", false);
                }

            }
        };

        $(document).on("click", "#button_edit_barcode", function () {
            var $barcode_table_selected = $("#barcode_table").find(".tr_barcode_table_body.selected");
            if ($barcode_table_selected.length > 0) {
                barcodeEdit = 1;
                $copy_value_to_field_id_edit = $barcode_table_selected.find(".td_copy_value_to_field").attr("id");

                $.innoDialog({
                    title: 'Edit barcode configuration',
                    width: 580,
                    dialog_data: $('#dialog_barcode_configure'),
                    open: dialog_barcode_configure_open_function,
                    type: 'Save_Cancel',
                    Save_Button: dialog_barcode_configure_save_function,
                    Cancel_Button: dialog_barcode_configure_cancel_function
                });
            } else {
                $.innoDialog({
                    title: 'Warning information',
                    width: 350,
                    dialog_data: '<div class="message_infor">Select barcode</div>',
                    type: 'Ok',
                    Ok_Button: function () {
                        $(this).dialog('close');
                    }
                });
            }
        });

        $(document).on("click", "#button_delete_barcode", function () {
            var $barcode_table_selected = $("#barcode_table").find(".tr_barcode_table_body.selected");
            var tr_barcode_id = $barcode_table_selected.attr("id");
            if (tr_barcode_id != "add_new_barcode_field") {
                $json_barcode_fields.delete_barcode_fields.push(tr_barcode_id);
            }
            $barcode_table_selected.remove();
        });


        //Event check Document separator checkbox
        $(document).on("change", "#document_separator", function () {
            if ($(this).is(':checked')) {
                $("#barcode_fields").attr("disabled", "disabled");
            } else {
                $("#barcode_fields").removeAttr("disabled");
            }
        });

        //Show, edit barcode configure
        $(document).on("click", ".barcode_config", function () {
            $("#right-container").find(".sub_properties_content").remove();
            $(".sub_menu_item").removeClass("selected");
            $(this).parentsUntil(".admin_sub_menu_items").last().addClass("selected");
            contenttype_id = $(this).attr("data-id");
            ShowBarcodeConfigure();
        });

        //Event click save button in barcode configure
        $(document).on("click", "#button_save_barcode", function () {
            $("#right-container").ecm_loading_show();
            var document_id = $("#barcode_table").data("barcode_document_id");

            $("#barcode_table").find("#barcode_table_body tr").each(function (index) {
                if ($(this).data("field_status") == "add_barcode_field" || $(this).data("field_status") == "edit_barcode_field") {
                    var barcode_id = $(this).attr("id");
                    var barcode_type = $(this).find(".td_barcode_type").text();
                    var barcode_position = parseInt($(this).find(".td_barcode_position").text());
                    var saperate_document = $(this).find(".td_separate_document").text();
                    if (saperate_document == "Yes") {
                        saperate_document = "true";
                    } else {
                        saperate_document = "false";
                    }
                    var remove_saperator = $(this).find(".td_remove_separator").text();
                    if (remove_saperator == "Yes") {
                        remove_saperator = "true";
                    } else {
                        remove_saperator = "false";
                    }
                    var copy_value_to_field = $(this).find(".td_copy_value_to_field").attr("id");
                    var do_lookup = $(this).find(".td_do_lookup").text();
                    if (do_lookup == "Yes") {
                        do_lookup = "true";
                    } else {
                        do_lookup = "false";
                    }
                    var data = {
                        "DocumentTypeId": document_id,
                        "Id": barcode_id,
                        "BarcodeType": barcode_type,
                        "BarcodePosition": barcode_position,
                        "IsDocumentSeparator": saperate_document,
                        "RemoveSeparatorPage": remove_saperator,
                        "MapValueToFieldId": copy_value_to_field,
                        "HasDoLookup": do_lookup
                    };
                    $json_barcode_fields.save_barcode_fields.push(data);
                }
            });
            SaveBarcode();
            $json_barcode_fields.save_barcode_fields = [];
            $json_barcode_fields.delete_barcode_fields = [];
        });

        //event click close button in barcode configure
        $(document).on("click", "#button_cancel_barcode", function () {
            $("#right-container").find(".sub_properties_content").remove();
        });
        var draws = [];
        var fieldMetaDatas = {};
        var $documentViewer;
        var $docViewerLoading;

        function URL_OcrConfigure_Success(data) {

            $(".sub_properties").append(data);
            $("#ocr-template-panel").ecm_loading_hide();
        }

        function URL_OcrConfigure_Error(jqXHR, textStatus, errorThrown) {
            console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        }


        // Event Click close button in OCR Template Configure
        $(document).on("click", ".ocr_template", function () {
            //$(this).parentsUntil(".admin_sub_menu_items").last().find(".row_batch_type").click();
            $("#nav-container").hide();      
            $("#main-container").hide();
 
            $("#fix-content").find("#nav-container-resizer").remove();

            $("#ocr_template_panel").show();
            $("#ocr_template_panel").ecm_loading_show();
            var url = $(this).attr('href');
            var $ocr_dialog = $("#ocr_template_panel");

            $ocr_dialog.load(url, function () {
                var ocrTemplate = $('.ocr_template');
                if (ocrTemplate.length > 0) {
                    var datas = [];
                    var thumbDatas = [];

                    ocrTemplate.children('.ocr_template_page').each(function (i, e) {
                        var page = {
                            KeyCache: $(e).attr('data-id').toString(),
                            ocrZones: [],
                            Width: $(e).attr('data-width'),
                            Height: $(e).attr('data-height'),
                            RotateAngle: $(e).attr('data-rotateangle')
                        };

                        var thumb = {
                            KeyCache: $(e).attr('data-id').toString(),
                            pageIndex: i,
                            Resolution: $(e).attr('data-dpi').toString(),
                            RotateAngle: $(e).attr('data-rotateangle'),
                        };

                        $(e).children('.ocr_template_zone').each(function (i, e) {
                            var zone = {
                                type: 'ocr_zone',
                                select: $(e).attr('data-name'),
                                left: parseFloat($(e).attr('data-left')),
                                top: parseFloat($(e).attr('data-top')),
                                width: parseFloat($(e).attr('data-width')),
                                height: parseFloat($(e).attr('data-height')),
                                id: $(e).attr('data-id'),
                            };
                            page.ocrZones.push(zone);
                        });

                        thumbDatas.push(thumb);
                        datas.push(page);
                    });
                    $documentViewer = $("#docViewer");
                    $thumbnailViewer = $("#ocr_thumbnail");

                    $docViewerLoading = $("<div id='docViewerTemp'>");
                    $docViewerLoading.css({
                        visibility: 'hidden',
                        left: $('body').offset().left + $('body').width(),
                        top: $('body').offset().top + $('body').height(),
                        width: $documentViewer.width(),
                    });
                    $('body').append($docViewerLoading);
                    //setButtonWidthInFirefox();
                    if (datas.length > 0 && thumbDatas.length > 0) {
                        createPage(datas);
                        createThumbnail(thumbDatas);
                    } else {
                        $("#ocr_template_panel").ecm_loading_hide();
                    }

                }
            });

            //createContextMenu();

            return false;
        });
       
        $(document).on("click", "#ocr_template_file", function () {
            $("#filePath").click();
        });

        $(document).on('change', '#filePath', function () {
            $documentViewer.ecm_loading_show();
            //$("#docViewer").annotationCapture({ image: imgURL });
            //Goi ham submit de upload image len server
            var options = {
                url: URL_UploadFile,
                dataType: "json",
                success: createPage,
                error: showError
            };
            $('#formUpload').ajaxSubmit(options);
        });

        function showError() {
            alert('Upload fail.');
        }

        function createPage(data) {
            $documentViewer.children('.page').remove();
            draws = [];
            var _fields = [];
            $.ajax({
                url: URL_GetFieldMetaData,
                async: false,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({ docTypeId: $('.doc_type_id').val() }),
                success: function (data) {
                    if (typeof data == "string") {
                        alert(data);
                        return;
                    }
                    if (data.IsTimeOut)
                        window.location = URL_Login;
                    else if (data.IsError) {
                        alert(data.Message);
                        return;
                    }
                    else {
                        fieldMetaDatas = data;
                        $.each(fieldMetaDatas, function () {
                            _fields.push(this.toString());
                        });
                    }
                },
                error: function () {
                    alert("Error");
                }
            });
            //$(".fieldMetaData").each(function () {
            //    _fields.push($(this).attr('data-name'));
            //});
            var count = 0;
            $.each(data, function (i, v) {
                var imageUrl = URL_LoadImage + "?key=" + this.KeyCache;
                var $page = $("<div class='page' data-page-index='" + i + "'"
                                             + " data-key='" + this.KeyCache + "'></div>");
                //$documentViewer.css({ width: '100%', height: '100%', overflow: 'auto' });
                $docViewerLoading.append($page);
                //$documentViewer.append($page);
                draws[i] = $page.annotationCapture({ image: imageUrl });
                $page.find('img').load(function () {
                    count++;
                    if (count == data.length) {
                        $docViewerLoading.children().appendTo($documentViewer);
                    }
                });
                draws[i].setFields(_fields);
                draws[i].annoOcrZone();
                //var zones = this.ocrZones;
                //setTimeout(draws[i].createAnnotations(zones), 5000);
                //draws[i].ready(function () {
                //    draws[i].createAnnotations(zones);
                //});
                //if (this.ocrZones)
                //    draws[i].createAnnotations(this.ocrZones);
            });
            $.each(data, function (i) {
                if (data[i].ocrZones)
                    draws[i].createAnnotations(data[i].ocrZones);
            });
            resize_vertical_between_fieldset();
            $(".ocr_template_tool").removeAttr("style");
            $documentViewer.ecm_loading_hide();
        }

        $(document).on("click", "#ocr_save_button", function () {

            var $doc_view = $("#docViewer").children('.page');
            var $anno_ocr_name = $doc_view.find(".anno_ocr_zone_name");
            var anno_ocr_name_used;
            var check_anno_ocr_name_pass = true;
            $.each($anno_ocr_name, function () {
                var this_ocr_name = $(this).text();
                var duplicate_count = 0;

                $.each($anno_ocr_name, function () {
                    if ($(this).text() == this_ocr_name) {
                        duplicate_count += 1;
                    }
                });

                if (duplicate_count > 1) {
                    check_anno_ocr_name_pass = false;
                    anno_ocr_name_used = this_ocr_name;
                    return false;
                }

            });

            if (check_anno_ocr_name_pass) {
                $(".between_and_right").ecm_loading_show();
                var $ocrTemplate = $('.doc_type_id');
                var $ocrLanguage = $(".ocr_language").val();
                var ocrTemplate = {
                    DocTypeId: $ocrTemplate.val(),
                    ///HARD CODE
                    FileExtension: "TIF",
                    OCRTemplatePages: [],
                    LangId: $ocrLanguage
                };

                $doc_view.each(function (i) {
                    var key = $(this).attr('data-key');
                    var ocrPage = {
                        Key: key,
                        OCRTemplateZone: [],
                        PageIndex: i
                    }
                    var param = draws[i].getAnnotationsOriginSize();
                    $(this).children('.anno_ocr_zone').each(function (i) {
                        var name = $(this).children('.anno_ocr_zone_name').text();
                        var id = getIdByName(name);
                        ocrZone = {
                            FieldMetaDataId: id,
                            Left: param[i].left,
                            Top: param[i].top,
                            Width: param[i].width,
                            Height: param[i].height
                        }
                        ocrPage.OCRTemplateZone.push(ocrZone);
                    });
                    ocrTemplate.OCRTemplatePages.push(ocrPage);
                });

                $.ajax({
                    url: URL_SaveOCRTemplate,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ ocrTemplate: ocrTemplate }),
                    success: function (data) {
                        if (data == true) {
                            if ($(".admin_sub_menu_items").find(".sub_menu_item.selected").find(".delete_ocr").length == 0) {
                                $(".admin_sub_menu_items").find(".sub_menu_item.selected").find(".add_delete_ocr_icon").addClass("delete_ocr");
                                $(".admin_sub_menu_items").find(".sub_menu_item.selected").find(".add_delete_ocr_icon").removeClass("add_delete_ocr_icon");
                            }
                            //alert("Success");
                            $(".between_and_right").ecm_loading_hide();
                        } else {
                            alert("Fail");
                            $(".between_and_right").ecm_loading_hide();
                        }
                    },
                    error: function () {
                        alert("Error");
                    }
                });
            } else {
                $.innoDialog({
                    title: 'Error information',
                    width: 350,
                    dialog_data: '<div class="message_infor">"' + anno_ocr_name_used + '" field has been used!</div>',
                    type: 'Ok',
                    Ok_Button: function () {
                        $(this).dialog('close');
                    }
                });
            }
        });

        $(document).on("click", "#ocr_close_button", function () {
            $(".between_and_right").hide();
            $(".between_and_right").find(".between_and_right_content").remove();
            $("#left-container").show();
            $("#left-container").css({ display: 'inline - block' });
            $("#right-container").show();
            $("#right-container").css({ display: 'inline-block' });
        });

        $(document).on("click", "#zoom_in", function () {
            $.each(draws, function () {
                this.zoomIn();
            });
        });

        $(document).on("click", "#zoom_out", function () {
            $.each(draws, function () {
                this.zoomOut();
            });
        });

        $(document).on("click", "#pan", function () {
            $.each(draws, function () {
                this.scrollable();
            });
        });

        $(document).on("click", "#draw", function () {
            $.each(draws, function () {
                this.annoOcrZone();
            });
        });

        function getIdByName(name) {
            var id;
            $.each(fieldMetaDatas, function (i) {
                if (this == name) {
                    id = i;
                    return false;
                }
            });
            return id;
        }

    });
});