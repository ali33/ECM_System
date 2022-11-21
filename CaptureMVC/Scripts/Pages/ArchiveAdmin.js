//#########################################################
//#Copyright (C) 2013, Innoria Solution. All Rights Reserved
//#
//#History:
//# DateTime         Updater         Comment
//# 7/11/2013       Triet Ho        Tao moi

//##################################################################
var EmptyId = "00000000-0000-0000-0000-000000000000";
var _DocumentID = -1;
var _FieldID = -1;
var json; // chuoi json khi luu content type
var json_delete_fields = { DeletedFields: [] };
         json_delete_fields.DeletedFields.push({ Id: -1 });
var isEdit = -1; // biến dùng để kiểm tra là đang edit field hay add field  1: Edit, -1: Add new
var isEditContentType = -1;//Edit: 1; Add new: -1
var picklistvalue="";
var json_pick_value;
var str_pick_value = "";

var table_field_name;
var table_field_datatype;
var table_field_maxlength;
var table_field_default_value;
var table_field_use_current_date;
var json_table_fields = {};
var json_table_value;
var tableName; // lưu trữ tên field kiểu table khi edit
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
//Loc Ngo
var $contenttype_fields;
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

$(function () {

var current_field; //dung de giu row_field de thuc hien xoa

// Dùng để hiển thị danh sách các content type
function ShowListContentType() {

    ///Get Ajax
    $(".admin_sub_menu").ecm_loading_show();

    Inno.helper.post(
        URL_ShowListContentType,
        JSON.stringify(""),
        ShowListContentType_Success,
        ShowListContentType_Error);
}
function ShowListContentType_Success(data) {
    $(".admin_sub_menu_content").remove();
    //called from effect.js
    $(".admin_sub_menu").append(data);
    sub_menu_height();
    //End called from effect.js
    setButtonWidthInFirefox();
    $(".sub_menu_item").first().find(".row_content_type").click();
    $(".admin_sub_menu").ecm_loading_hide();
}

function ShowListContentType_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".admin_sub_menu").ecm_loading_hide();
}
// ================ Kết thúc ShowListContentType ===================== // 
// Dùng để hiển thị, thêm content properties
function ContentTypeProperties() {

    ///Get Ajax
    $(".sub_properties").ecm_loading_show();

    Inno.helper.post(
        URL_ContentTypeProperties,
        JSON.stringify({DocumentId:_DocumentID}),
        ContentTypeProperties_Success,
        ContentTypeProperties_Error);
}
function ContentTypeProperties_Success(data) {
    // $(".sub_properties").find(".sub_properties_content").remove();
    $(".sub_properties").find(".sub_properties_content").remove();
    $(".sub_properties").append(data);
    if (_DocumentID != -1)
    {
        GetTableValue2(); // trường hợp add new content mới thì không gọi GetTableValue2();
    }
    //called from effect.js
    setButtonWidthInFirefox();
    resize_vetical_properties_content();
    resize_vetical_properties_top_down_height()//(height of down_height class)
    reszie_vetical_fieldset();
    resize_vertical_multi_data();
    //End called from effect.js
    $(".sub_properties").ecm_loading_hide();
}

function ContentTypeProperties_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();
}
// ============== Ket thuc ContentTypeProperties =======//

// ===============  Dùng để xóa content type ============
function DeleteContentType() {
    ///Get Ajax
    Inno.helper.post(
        URL_DeleteContentType,
        JSON.stringify({ DocumentId: _DocumentID }),
        DeleteContentType_Success,
        DeleteContentType_Error);
}
function DeleteContentType_Success(data) {
    $(".sub_properties_content").remove();
    $(".admin_sub_menu").find(".sub_menu_item.selected").remove();
    $(".sub_menu_item").first().find(".row_content_type").click();
}

function DeleteContentType_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();
}
// ============== Ket thuc Delete content =======//
    
//================ reset dialog  add field==================//
function ResetFieldInDialog()
{
    $("#field_name").val("");
    // $("input:checked").removeAttr("checked");
    $("#required:checked").removeAttr("checked");
    $("#restricted:checked").removeAttr("checked");
    $("#use_current_date").removeAttr("checked");
    $("#dataType").prop('selectedIndex', 0);
    $("#max_length").val("0");
    $("#default_value").val("");
    $("#configure_picklist").hide();
    $("#current_date").hide();
    $("#select_true_false").hide();
    $("#configure_table").hide();
    $("#default_value").prop('disabled', false);
    $("#default_value_title").show();
    $("#max_length").prop('disabled', false);
} // kết thúc reset dialog 

// ===============  Dùng để save content type ============
function SaveContentType() {
    ///Get Ajax
    $(".sub_properties").ecm_loading_show();
    json.Name = (json.Name + "").replace(/\s+/g, ' ').trim();
    Inno.helper.post(
        URL_SaveContentType,
        JSON.stringify({ doctypemodel: json, picklist: picklistvalue, keyCacheIcon: keyCacheIcon }),
        SaveContentType_Success,
        SaveContentType_Error);
}
function SaveContentType_Success(data) {
    $(".sub_properties").ecm_loading_hide();
    if (data != EmptyId) {
        if (data.indexOf(EmptyId + "_") != 0) {
            if (isEditContentType == -1) {
                $(".admin_sub_menu_items").ecm_loading_show();
                var new_contentype_menu_item =
                    "<div class='sub_menu_item selected'  id='" + data + "' >" +
                        "<div class='icon'>" +
                            "<img src='" + URL_GetIcon + "?key=" + keyCacheIcon + "' />" +
                        "</div>" +

                        "<div class='sub_item_content row_content_type' title='" + json.Name + "'>" +
                            "<div class='item_content_data'>" +
                                json.Name +
                            "</div>" +
                        "</div>" +

                        "<div class='item_detail'>" +
                            "<div class='configure'>" +
                                "<div class='data'>OCR template</div>" +
                                    "<a href='" + URL_OcrConfigure + "?id=" + data + "' class='ocr_template' title='Ocr configure'>" +
                                        "<div class='icon'>" +
                                        "</div>" +
                                    "</a>" +

                                    "<div class='add_delete_ocr_icon' title='Delete Ocr template' data-id='" + data + "' data-contenttype_name='" + json.Name + "'></div>" +
                                    "</div>" +

                                    "<div class='barcode'>" +
                                        "<div class='data'>Barcode</div>" +
                                        "<div class='icon barcode_config' title='Barcode configure' data-id='" + data + "'></div>" +
                                        "<div class='add_delete_barcode_icon' title='Delete barcode' data-id='" + data + "' data-contenttype_name='" + json.Name + "'></div>" +
                                "</div>" +
                        "</div>" +

                    "</div>";
                $(".admin_sub_menu_items").find(".sub_menu_item").removeClass("selected");
                $(".admin_sub_menu_items").append(new_contentype_menu_item);
                $(".admin_sub_menu_items").ecm_loading_hide();
                $(".admin_sub_menu_items").find(".sub_menu_item.selected").find(".row_content_type").click();
            } else {
                var $this_chage = $(".admin_sub_menu_items").find(".sub_menu_item.selected");
                $this_chage.find(".icon img").removeAttr("style");
                $this_chage.find(".item_content_data").text(json.Name);
                $this_chage.find(".item_content_data").attr("title", json.Name);
                $this_chage.find(".icon img").attr("src", $(".sub_properties").find("#image_icon").attr("src"));
                $(".admin_sub_menu_items").find(".sub_menu_item.selected").find(".row_content_type").click();
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
                    var $this_fields = $("#table_fields").find("#table_fields_body tr#-1");
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
                $(".sub_properties").find("#content_name").focus();
            }
        });
    }
}

function SaveContentType_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();

}
// ============== Ket thuc save content =======//


  // ===============  Dùng để  GetPicklistValue type ============
        function GetPicklistValue() {

            ///Get Ajax
            $(".sub_properties").ecm_loading_show();

            Inno.helper.post(
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
            $(".sub_properties").ecm_loading_show();

            Inno.helper.post(
                URL_GetTableValue,
                JSON.stringify({ DocumentId: _DocumentID }),
                GetTableValue_Success,
                GetTableValue_Error);
        }
        function GetTableValue_Success(data) {
            //json_table_value = data;
            json_table_value = jQuery.parseJSON(data);
            $(".sub_properties").ecm_loading_hide();
        }

        function GetTableValue_Error(jqXHR, textStatus, errorThrown) {
            console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
            $(".sub_properties").ecm_loading_hide();

        }
// ============== Ket thuc GetTableValue =======//
function GetTableValue2()
{
    $.ajax({
        url: URL_GetTableValue,
        type: "POST",
        async: true,
        data: JSON.stringify({ DocumentId: _DocumentID }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            //QueryExpression = jQuery.parseJSON(Data);
            json_table_value = data;
            json_table_fields = {};
            $("#table_fields").find('tr').each(function (index) {
                if (index != 0)
                {
                    var id_field = $(this).attr("id");
                    var td_field_name = $(this).find(".td_field_name").text();
                    var td_data_type = $(this).find(".td_data_type").text();
                    
                    if (td_data_type == "Table")
                    {
                        var Array_Temp = new Array();
                        for (i = 0 ; i < json_table_value.TableValue.length; i++)
                        {
                            if (json_table_value.TableValue[i].ParentFieldId == id_field)
                            {
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
// ===============  Dùng để  Load table configure ============
function LoadTableConfigure() {

    ///Get Ajax

    Inno.helper.post(
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
    // ============== Ket thuc Load table configure  =======//  

    // ===============  Dùng để  Edit table configure khi chưa được lưu ============
function EditTableConfigure() {

    ///Get Ajax
    Inno.helper.post(
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
    // ============== Ket edit table configure  =======//  

   
// Dùng để edit table configuration khi fields table mới được thêm 
function EditTableConfigureEvent()
{   
    EditTableConfigure(); 
}
// =================== Save content =================
function SaveContentTypeEvent()
{
    if ($("#table_fields").find('tbody tr').length > 0) {
        var contenttypename = $("#content_name").val();
        var outlook = $("#outlook_import").val();
        if ($("#outlook_import").is(":checked")) {
            outlook = "true";
        }

        var DisplayOrder = 1;
        json = { "Name": contenttypename, "Id": _DocumentID, "IsOutlook": outlook, Fields: [], IsOutlook: outlook, DeletedFields: [] };
        $("#table_fields").find('tr').each(function (index) {
            if (index != 0) {
                var id_field = $(this).attr("id");
                var td_field_name = $(this).find(".td_field_name").text();
                var td_data_type = $(this).find(".td_data_type").text();
                var td_required = $(this).find(".td_required").text();
                if (td_required == "Yes") {
                    td_required = true;
                }
                else {
                    td_required = false;
                }
                var td_restricted = $(this).find(".td_restricted").text();
                if (td_restricted == "Yes") {
                    td_restricted = true;
                }
                else {
                    td_restricted = false;
                }
                var td_haslookup = $(this).find(".td_haslookup").text();
                var td_default_value = $(this).find(".td_default_value").text();
                var use_current_date = false;
                if (td_data_type == "Date" && td_default_value == "{Use current date}") {
                    td_default_value = "";
                    use_current_date = true;
                }
                /*if (td_default_value == "Yes" && td_data_type == "Boolean") {
                    td_default_value = true;
                }
                else {
                    td_default_value = false;
                }*/
                var td_maxlength = $(this).find(".td_maxlength").text();
                var td_picklist = $(this).find(".td_picklist").text();
                if (td_data_type == "Picklist") {
                    picklistvalue = picklistvalue + td_picklist + "#";
                }
                //json.SearchQueryExpressions.push({ Id: ExpressionID, SearchQueryId: QueryId, Condition: Condition, Field: { Id: FieldID, FieldUniqueId: FieldUniqueId }, OperatorText: OperatorText, Operator: Operator, Value1: value1, Value2: value2, FieldUniqueId: FieldUniqueId });

                json.Fields.push({ DocTypeId: _DocumentID, Id: id_field, Name: td_field_name, DataType: td_data_type, IsRequired: td_required, IsRestricted: td_restricted, MaxLength: td_maxlength, DefaultValue: td_default_value, DisplayOrder: DisplayOrder, UseCurrentDate: use_current_date, Children: json_table_fields[td_field_name] });
                DisplayOrder++;

            }
        });
        //Icon value
        keyCacheIcon = $("#image_icon").data("keycache");

        // nếu thêm outlook được check thì thêm vào các field sau 
        if (outlook == "true" && isEditContentType != 1) {
            json.Fields.push({ DocTypeId: _DocumentID, Name: "Mail body", DataType: "String", IsRequired: "True", IsRestricted: "No", DisplayOrder: DisplayOrder });
            DisplayOrder++;
            json.Fields.push({ DocTypeId: _DocumentID, Name: "Mail from", DataType: "String", IsRequired: "True", IsRestricted: "No", DisplayOrder: DisplayOrder });
            DisplayOrder++;
            json.Fields.push({ DocTypeId: _DocumentID, Name: "Mail to", DataType: "String", IsRequired: "True", IsRestricted: "No", DisplayOrder: DisplayOrder });
            DisplayOrder++;
            json.Fields.push({ DocTypeId: _DocumentID, Name: "Received date", DataType: "Date", IsRequired: "True", IsRestricted: "No", DisplayOrder: DisplayOrder });
        }
        //thêm vào các field được xóa để thực hiện chức năng xóa
        json.DeletedFields = json_delete_fields.DeletedFields;
        SaveContentType();
        /*$.ajax({
            url: URL_SaveContentType,
            async: true,
            type: "POST",
            // data: JSON.stringify(json),
            data:{"doctypemodel":json,"picklist":json_pick_value},
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            error: function (jqXHR, textStatus, errorThrown) {
                alert(jqXHR + "-" + textStatus + "-" + errorThrown);
            },
            success: function (data, textStatus, jqXHR) {
                $(".sub_properties").ecm_loading_hide();
                $(".sub_properties_content").remove();
                ShowListContentType();
            }
        });*/
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
}
    // append first row of table configure 
function AppendFirstRowTableConfigure()
{
    $("#table_configuration").append("<tr class='table_configure_row first_row' id='-1'>"
                                            + "<td id='first_delete_column'><img class='dialog_table_icon' src='" + URL_TableConfigure + "'></td>"
                                            + "<td class='table_td_name'><input type='text' class='archive_input dialog_table_input table_name_value' placeholder='New column'></td>"
                                            + "<td class='table_td_type'>"
                                                + "<select  class='archive_select table_data_type'>"
                                                   + "<option value='String' selected>String</option>"
                                                   + "<option value='Integer'>Integer</option>"
                                                   + "<option value='Decimal'>Decimal</option>"
                                                   + "<option value='Date'>Date</option>"
                                               + "</select>"

                                           + "</td>"
                                           + "<td class='table_td_maxlength'><input type='text' class='archive_input dialog_table_input table_maxlength' value='0'></td>"
                                           + "<td class='table_td_default_value'>"
                                               + "<input type='text' class='archive_input dialog_table_input table_default_value'>"
                                               + "<div class='dialog_table_checkbox table_use_current_date_contain hidden'>"
                                               + "<input type='checkbox' class='table_check_use_current_date' value='false'><a>Use current date</a>"
                                               + "</div>"
                                           + "</td>"
                                         + "</tr>");
}
function ChangeTableDataType(DataType) {
   // var table_data_type = $(".table_data_type").val();
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
    //sự kiện khi thay đổi datatype 
function ChangeDataType()
{
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
                break;
            }
        case "Picklist":
            {
                $("#configure_picklist").show();
                $(".current_date").hide();
                $("#select_true_false").hide();
                $("#configure_table").hide();
                $("#default_value").show();
                $("#default_value").prop('disabled', true);
                $("#max_length").prop('disabled', true);
                $("#max_length").val("0");
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
                break;
            }
        case "Table":
            {
                $("#configure_picklist").hide();
                $(".current_date").hide();
                $("#select_true_false").hide();
                $("#configure_table").show();
                $("#default_value").show();
                $("#default_value").prop('disabled', true);
                $("#max_length").prop('disabled', true);
                $("#max_length").val("0");
                $("#default_value_title").hide();
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
            }

    }
}
// phần xử lý cho phần lookup
// tab in lookup dialog
$(function () {
    $("#dialog_lookup_content").tabs();
});
// ===============  Dùng để  Test connection  ============
function TestConnection() {

    ///Get Ajax
    $(".sub_properties").ecm_loading_show();

    Inno.helper.post(
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
    // ============== Ket test connection =======// 
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
function GetDataSource()
{
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
        data: JSON.stringify({ sourceName: SourceName, connectionString : ConnectString, dataProvider: dataProvider, type: "Table" }),
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

//Loc Ngo
//Choose icon process
function ChooseIconProcess() {
    UploadIcon();
}

//Upload Icon
function UploadIcon() {
    $.ajax({
        url: URL_ContentTypeIcon,
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

//Delete ocr template
function DeleteOcrTemplate() {
    Inno.helper.post(
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
    var $this_barcode_configure = $(".sub_properties").find("#docViewer");
    if ($this_barcode_configure.length > 0) {
        $this_menu_item.find(".ocr_template").click();
    }
}

function DeleteOcrTemplate_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
}

//Show barcode properties
function ShowBarcodeConfigure() {
    $(".sub_properties").ecm_loading_show();
    Inno.helper.post(
        URL_ShowBarcodeConfigure,
        JSON.stringify({id: contenttype_id}),
        ShowBarcodeConfigure_Success,
        ShowBarcodeConfigure_Error
    );
}

function ShowBarcodeConfigure_Success(data) {
    $(".sub_properties").find(".sub_properties_content").remove();
    $("#dialog_barcode_configure").find("#barcode_fields").find("option").remove();
    $(".sub_properties").append(data);
    //ShowListBarcodeFields(lstFields, lstBarcode);
    $list_content_fields = lstFields;
    //called from effect.js
    setButtonWidthInFirefox();
    resize_vetical_properties_content();
    resize_vetical_properties_top_down_height()//(height of down_height class)
    reszie_vetical_fieldset();
    resize_vertical_multi_data();
    //End called from effect.js

    $(".sub_properties").ecm_loading_hide();
}

function ShowBarcodeConfigure_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
}

//Save barcode fields
function SaveBarcode() {
    Inno.helper.post(
        URL_SaveBarcodeFields,
        JSON.stringify($json_barcode_fields),
        SaveBarcode_Success,
        SaveBarcode_Error
    );
}

function SaveBarcode_Success(data) {
    $(".sub_properties").ecm_loading_hide();
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
    Inno.helper.post(
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
    var $this_barcode_configure=$(".sub_properties").find("#barcode_table");
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
    
    //load danh sách các content types khi vừa load trang lên 
    ShowListContentType();
    LookupDataType = $("#display_table").val();
    $(".between_and_right").hide();

    $("#content_types").click(function () {
        //$(".admin_sub_menu_content").find("div").remove();
        $(".archive_admin_menu").find(".admin_menu_item").removeClass("selected");
        $(this).addClass("selected");

        $(".admin_sub_menu").find("div").remove();
        $(".sub_properties").find(".sub_properties_content").remove();
        $(".sub_properties").css({ display: 'inline-block' });
        $(".admin_sub_menu").css({ display: 'inline-block' });
        $(".between_and_right").find(".between_and_right_content").remove();
        $(".between_and_right").hide();
        ShowListContentType();
        


    });

    // sự kiện khi click vào mỗi content type
    $(document).on("click", ".row_content_type", function () {
        $(".sub_properties").find(".sub_properties_content").remove();
        $(".sub_menu_item").removeClass("selected");
        $(this).parent().addClass("selected");
        isEditContentType = 1;
        _DocumentID = $(".admin_sub_menu_items").find(".sub_menu_item.selected").attr("id");
        
        ContentTypeProperties();
        //GetTableValue2();
        // alert("Da click content type");

    });
    // Sự kiện khi click vào nút add content mới 
    $(document).on("click", ".button_add_content", function () {
        $(".sub_properties").find(".sub_properties_content").remove();
        _DocumentID = -1; // đặt lại ID là -1 để không load properties của content hiện tại
        isEditContentType = -1;
        $(".sub_menu_item").removeClass("selected"); //bỏ chọn content type
        $("#button_delete_content").prop("disabled", true);
        ContentTypeProperties();
    });

    // nút delete content
    var dialog_delete_contenttype_yes_function = function () {
        DeleteContentType();
        $(this).dialog("close");
    };

    var dialog_delete_contenttype_no_function = function () {
        $(this).dialog("close");
    };

    $(document).on("click", ".button_delete_content", function () {
        _DocumentID = $(".admin_sub_menu_items").find(".sub_menu_item.selected").attr("id");
        $.innoDialog({
            title: 'Cloud ECM',
            width: 350,
            dialog_data: $('#dialog_delete_contenttype'),
            type: 'Yes_No',
            Yes_Button: dialog_delete_contenttype_yes_function,
            No_Button: dialog_delete_contenttype_no_function
        });
    });
    
    // xử lý đối với dialog add field
    var dialog_add_field_open_function = function (event, ui) {
        if (isEdit == 1) {
            var date_type = $("tr.row_field.selected").find(".td_data_type").text();
            var field_name = $("tr.row_field.selected").find(".td_field_name").text();
            var required = $("tr.row_field.selected").find(".td_required").text();
            var restricted = $("tr.row_field.selected").find(".td_restricted").text();
            var maxlength = $("tr.row_field.selected").find(".td_maxlength").text();
            var default_value = $("tr.row_field.selected").find(".td_default_value").text();
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
            ChangeDataType();
        }
    }

    var dialog_add_field_ok_function = function () {
        var field_name = $("#field_name").val();
        var data_type = $("#dataType").val();
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
                    // table_use_current_date = $(this).find(".table_td_default_value").find(".table_use_current_date_contain").find(".table_check_use_current_date").val();
                    var table_field_id = $(this).attr("id");
                    var td_use_current_date = $(this).find(".table_td_default_value").find(".table_use_current_date_contain").find(".table_check_use_current_date");
                    table_field_use_current_date = td_use_current_date.val();
                    if (td_use_current_date.is(":checked")) {
                        table_field_use_current_date = "true";
                    }
                    /* if (table_field_datatype == "Date") {
                         table_field_default_value = table_field_use_current_date;
                     }*/
                    json_of_each_row.tablefiels.push({ DocTypeId: _DocumentID, ParentFieldId: _FieldID, FieldId: table_field_id, ColumnName: table_field_name, DataType: table_field_datatype, DefaultValue: table_field_default_value, UseCurrentDate: table_field_use_current_date, MaxLength: table_field_maxlength });
                }

                //json.Fields.push({ DocTypeId: _DocumentID, Id: id_field, Name: td_field_name, DataType: td_data_type, IsRequired: td_required, IsRestricted: td_restricted, MaxLength: td_maxlength, DefaultValue: td_default_value, DisplayOrder: DisplayOrder, UseCurrentDate: use_current_date });
            });
            json_table_fields[field_name] = json_of_each_row.tablefiels;
            // reset lại các dòng trên table confugure 
            $("#table_configuration").find("tr.table_configure_row").remove();// xóa tất cả các dòng
            AppendFirstRowTableConfigure(); // thêm vào dòng đầu tiên

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
                            $("#table_fields tbody#table_fields_body").append("<tr class='row_field' id='-1'><td class='td_field_name'>"
                                                            + field_name + "</td><td class='td_data_type'>"
                                                            + data_type + "</td><td class='td_required'>"
                                                            + required_title + "</td><td class='td_restricted'>"
                                                            + restricted_title + "</td><td class='td_haslookup'>"
                                                            + "No" + "</td><td class='td_default_value'>"
                                                            + default_value + "</td><td class='td_maxlength'>"
                                                            + maxlength + "</td><td  class='td_lookup'>"
                                                           // + configure + "</td><td  class='td_picklist hidden'>"
                                                            + "</td><td  class='td_picklist hidden'>"
                                                            + picklist + "</td>");
                            $("#table_fields").find("#table_fields_body").find("tr#-1").last().click();
                        } else {
                            $("tr.row_field.selected").find(".td_data_type").text(data_type);
                            $("tr.row_field.selected").find(".td_field_name").text(field_name);
                            $("tr.row_field.selected").find(".td_required").text(required_title);
                            $("tr.row_field.selected").find(".td_restricted").text(restricted_title);
                            $("tr.row_field.selected").find(".td_maxlength").text(maxlength);
                            $("tr.row_field.selected").find(".td_default_value").text(default_value);
                            $("tr.row_field.selected").find(".td_picklist").text(picklist);
                        }
                        $("#picklist_value").val("");
                        $(this).dialog("close");
                        ResetFieldInDialog();
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

    var dialog_add_field_cancel_function = function () {
        $(this).dialog("close");
        // reset lại các row trên table configuration 
        $("#table_configuration").find("tr.table_configure_row").remove();// xóa tất cả các dòng
        AppendFirstRowTableConfigure(); // thêm vào dòng đầu tiên
        ResetFieldInDialog();
    }

    // sự kiện khi click vào button add field
    $(document).on("click", "#button_add_field", function () {
        isEdit = -1;
        _FieldID = -1;

        $contenttype_fields = new Array();
        var lstField = $("#table_fields").find("#table_fields_body tr");
        $.each(lstField, function () {
            $contenttype_fields.push($(this).find(".td_field_name").text());
        });

        ResetFieldInDialog();

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
    // sự kiện khi thay đổi datatype


    //sự kiện khi click thay đổi các giá trị của select datatye trên combobox
    $(document).on("change", "#dataType", function () {
        ChangeDataType();
    });
    
    //sự kiện khi click vào button cancel
    $(document).on("click", "#button_cancel_content", function () {
        $(".sub_properties").find(".sub_properties_content").remove();
        json_table_fields = {};
        json_delete_fields = { DeletedFields: [] };
        json_delete_fields.DeletedFields.push({ Id: -1 });
        json_table_value = {};
        json_pick_value = "";
        $(".sub_menu_item").first().find(".row_content_type").click();
    });
    // sự kiện khi click vào mỗi dòng (field) của mỗi content type
    $(document).on("click", "tr.row_field", function () {

        current_field = $(this);
        $("tr.row_field").removeClass("selected");
        $(this).addClass("selected");
        var td_name = $(this).find(".td_data_type").text();
        if (td_name == "Table")
        {
            tableName = $(this).find(".td_field_name").text();
        }


    });
    // sự kiện khi click button_delete_field
    $(document).on("click", "#button_delete_field", function () {

       
        var id_field_delete = $("tr.row_field.selected").attr("id");
        var name_field_delete = $("tr.row_field.selected").find(".td_field_name").text();
        $("tr.row_field.selected").remove();        
        json_delete_fields.DeletedFields.push({ Id: id_field_delete, Name: name_field_delete });


    });
    // sự kiện khi click button_save_contenttype
    $(document).on("click", "#button_save_content", function () {
        if (CheckField($("#content_name"), "Content type name", 0, 0)) {
            SaveContentTypeEvent();
            picklistvalue = "";
            _FieldID = -1;
            json_table_fields = {};
        }
    });
    //sự kiện khi click vào button_edit_field 
    $(document).on("click", "#button_edit_field", function () {

        isEdit = 1;
        _FieldID = $("tr.row_field.selected").attr("id");
        if ($("tr.row_field.selected").find(".td_data_type").text() == "Table" && _FieldID != -1) {
            // GetPicklistValue();
            LoadTableConfigure();
        }
        else if($("tr.row_field.selected").find(".td_data_type").text() == "Table" && _FieldID == -1)
        {
            EditTableConfigure();
        }
        //LoadTableConfigure();
        
        $contenttype_fields = new Array();
        var lstField = $("#table_fields").find("#table_fields_body tr");
        $.each(lstField, function () {
            if ($(this).attr("class") != "row_field selected") {
                $contenttype_fields.push($(this).find(".td_field_name").text());
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

    //sự kiện khi click vào configure picklist
    
    var picklist_value_dialog_open_function = function (event, ui) {
        if (isEdit != 1) {
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
        //$("#picklist_value").val("");
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

    // sự kiện khi click vào configure table
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

    $(document).on("click", "#configure_table", function () {
        /*if (isEdit == 1) { 
            //LoadTableConfigure();
        }
        else { 
            $("#dialog_table_configure").dialog("open");
        }*/
        $.innoDialog({
            title: 'Table columns configuration',
            width: 580,
            dialog_data: $('#dialog_table_configure'),
            open: dialog_table_configure_open_function,
            type: 'Close',
            Close_Button: dialog_table_configure_close_function,
        });
        
    });
    // sự kiện khi click vào add new columns in table configuration
    $(document).on("click", "#add_new_column", function () {

        $("#table_configuration").append("<tr class='table_configure_row' id='-1'>"
                                              + "<td class='table_td_delete'><img class='dialog_table_icon' src='" + URL_TableConfigure + "'></td>"
                                              + "<td class='table_td_name'><input type='text' class='archive_input dialog_table_input table_name_value' placeholder='New column'></td>"
                                              +"<td class='table_td_type'>"
                                                  + "<select  class='archive_select table_data_type'>"
                                                     + "<option value='String' selected>String</option>"
                                                     + "<option value='Integer'>Integer</option>"
                                                     + "<option value='Decimal'>Decimal</option>"
                                                     + "<option value='Date'>Date</option>" 
                                                 + "</select>"

                                             + "</td>"
                                             + "<td class='table_td_maxlength'><input type='text' class='archive_input dialog_table_input table_maxlength' value='0'></td>"
                                             + "<td class='table_td_default_value'>"
                                                 + "<input type='text' class='archive_input dialog_table_input table_default_value'>"
                                                 + "<div class='dialog_table_checkbox table_use_current_date_contain hidden'>"
                                                 + "<input type='checkbox' class='table_check_use_current_date' value='false'><a>Use current date</a>"
                                                 + "</div>"
                                             + "</td>"
                                           + "</tr>" );

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
    $(document).on("change","#select_data_source",function(){
    
        SourceName = $("#select_data_source").val();
        $("#table_look_data").find('tr.lookup_data_row').remove();
        $("#table_fields").find('tr').each(function (index) {
            if (index != 0)
            {
                var fieldName = $(this).find(".td_field_name").text(); 
                $("#table_look_data").append("<tr class='lookup_data_row'>"
                                                +"<td>"+ fieldName +"</td>"
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
        if ($(this).is(":checked"))
        {
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

    //Dialog change icon of content type
    //$(".dialog_content_icon").dialog({
    //    title: 'Change content icon',
    //    autoOpen: false,
    //    width: 580,
    //    modal: true,
    //    resizable: false,
    //    dialogClass: 'metro',
    //    autoReposition: true,
    //    buttons:
    //    [
    //        {
    //            text: "OK", click: function () {
    //                var keyCache = $("#dialog_image_icon").attr("data-keycache");
    //                $("#image_icon").attr("src", URL_GetImageFromCache + "?Key=" + keyCache);
    //                $("#image_icon").attr("data-keycache", keyCache);
    //                change_icon = true;
    //                $(this).dialog("close");
    //                $("#dialog_image_icon").attr("src", "#");
    //                $("#dialog_image_icon").attr("data-keycache", "");

    //            }
    //        },
    //        {
    //            text: "Cancel", click: function () {
    //                if (change_icon) {
    //                    $("#dialog_image_icon").attr("src", "#");
    //                    $("#dialog_image_icon").attr("data-keycache", "");
    //                }
    //                $(this).dialog("close");
    //            }
    //        }
    //    ]
    //});

    //Event choose icon for content type
    $(document).on("click", "#sub_image_icon", function () {
        iconPath = $(this).attr("data-icon-path");
        ChooseIconProcess();
    })

    //Event click change icon for content type

    //Loc Ngo
    var change_icon = false;

    var dialog_content_icon_ok_function = function () {
        var keyCache = $("#dialog_image_icon").attr("data-keycache");
        $("#image_icon").attr("src", URL_GetImageFromCache + "?Key=" + keyCache);
        $("#image_icon").attr("data-keycache", keyCache);
        change_icon = true;
        $(this).dialog("close");
        $("#dialog_image_icon").attr("src", "#");
        $("#dialog_image_icon").attr("data-keycache", "");
    };

    var dialog_content_icon_cancel_fuction = function () {
        if (change_icon) {
            $("#dialog_image_icon").attr("src", "#");
            $("#dialog_image_icon").attr("data-keycache", "");
        }
        $(this).dialog("close");
    };

    $(document).on("click", ".change_content_icon", function () {
        var keyCache = $("#image_icon").attr("data-keycache");
        $("#dialog_image_icon").attr("src", URL_GetImageFromCache + "?Key=" + keyCache);
        $("#dialog_image_icon").attr("data-keycache", keyCache);

        $.innoDialog({
            title: 'Change content icon',
            width: 580,
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
        $(this).parentsUntil(".admin_sub_menu_items").find(".row_content_type").click();

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

        var $this_barcode_configure = $(".sub_properties").find("#barcode_table");
        if ($this_barcode_configure.length > 0) {
            
        } else {
            $(this).parentsUntil(".admin_sub_menu_items").find(".row_content_type").click();
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
    function CheckBarcodePosition(optionvalue, position_obj){
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
        $(".sub_properties").find(".sub_properties_content").remove();
        $(".sub_menu_item").removeClass("selected");
        $(this).parentsUntil(".admin_sub_menu_items").last().addClass("selected");
        contenttype_id = $(this).attr("data-id");
        ShowBarcodeConfigure();
    });

    //Event click save button in barcode configure
    $(document).on("click", "#button_save_barcode", function () {
        $(".sub_properties").ecm_loading_show();
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
        $(".sub_properties").find(".sub_properties_content").remove();
    });

    //End Loc Ngo

    ///THODINH
    var draws = [];
    var fieldMetaDatas = {};
    var $documentViewer;
    var $docViewerLoading;
    $(document).on("click", ".ocr_template", function () {
        $(this).parentsUntil(".admin_sub_menu_items").last().find(".row_content_type").click();
        $(".admin_sub_menu").hide();
        $(".sub_properties").hide();
        $(".between_and_right").show();
        $(".between_and_right").ecm_loading_show();

        var url = $(this).attr('href');
        var $ocr_dialog = $("div.sub_properties");
        var $ocr_dialog = $("div.between_and_right");
        $ocr_dialog.load(url, function () {
            var ocrTemplate = $('.ocr_template');
            if (ocrTemplate.length > 0) {
                var data = [];
                ocrTemplate.children('.ocr_template_page').each(function (i, e) {
                    var page = {
                        KeyCache: $(e).attr('data-id').toString(),
                        ocrZones: [],
                    };
                    $(e).children('.ocr_template_zone').each(function (i, e) {
                        var zone = {
                            type: 'ocr_zone',
                            select: $(e).attr('data-name'),
                            left: parseFloat($(e).attr('data-left')),
                            top: parseFloat($(e).attr('data-top')),
                            width: parseFloat($(e).attr('data-width')),
                            height: parseFloat($(e).attr('data-height')),
                        };
                        page.ocrZones.push(zone);
                    });
                    data.push(page);
                });
                $documentViewer = $("#docViewer");
                $docViewerLoading = $("<div id='docViewerTemp'>");
                $docViewerLoading.css({
                    visibility: 'hidden',
                    left: $('body').offset().left + $('body').width(),
                    top: $('body').offset().top + $('body').height(),
                    width: $documentViewer.width(),
                    //height: $documentViewer.height()//block by Loc Ngo
                });
                $('body').append($docViewerLoading);
                setButtonWidthInFirefox();
                resize_vetical_windows_height();
                resize_vetical_bewteen_top_down_height();//called from effect.js(height of down_height class)
                if (data.length > 0) {
                    createPage(data);
                } else {
                    resize_vertical_between_fieldset();
                    $(".between_and_right").ecm_loading_hide();
                }
                
            }
        });
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
                if(typeof data == "string"){
                    alert(data);
                    return;
                }
                if (data.IsTimeOut)
                    window.location = URL_Login;
                else if(data.IsError){
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
    //partern = {
    //    DocTypeId: '0-0-0',
    //    OCRTemplatePages: [
    //        {
    //            Key: 'key img 1',
    //            OCRTemplateZone: [
    //                {
    //                    FieldMetaDataId: 'id1.1',
    //                    Left: 'left', Top: 'top', Width: 'width', Height: 'height'
    //                },
    //                {
    //                    FieldMetaDataId: 'id2.1',
    //                    Left: 'left', Top: 'top', Width: 'width', Height: 'height'
    //                }
    //            ],
    //            PageIndex: 0,
    //            FileExtension: 'TIF'
    //        }
    //    ]
    //}
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
        $(".admin_sub_menu").show();
        $(".admin_sub_menu").css({ display: 'inline - block' });
        $(".sub_properties").show();
        $(".sub_properties").css({ display: 'inline-block' });
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