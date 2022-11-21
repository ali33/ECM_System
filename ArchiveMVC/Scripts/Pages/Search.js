//#########################################################
//#Copyright (C) 2013, MIA Solution. All Rights Reserved
//#
//#History:
//# DateTime         Updater         Comment
//# 29/8/2013        Triet Ho        Tao moi
//# 18/09/2013       Nhựt Nguyễn     Edit
//# 18/09/2013       Nhựt Nguyễn     Edit 
//                          (Load Search Queries From Controller)
//##################################################################
var CurrentDocID;
var QueryExpression;
var _idCel = "";
var _valueControlID = "";
var _fieldId = -1;
var $listName = [];
var json_search_advance;
var json_search_global;

//Create TextBox Value in Queries Search
function CreateTextBoxValue(operator, fieldId, valueControlID) {
    _valueControlID = valueControlID;
    $('[id="' + valueControlID + '"]').find("div").ecm_loading_show();

    JsonHelper.helper.post(
       URL_CreateTextBoxValue,
       JSON.stringify({ DocumentTypeId: CurrentDocID ,CacheDocType: CacheDocType ,FieldID: fieldId ,SearchOperator: operator ,ValueControlId: valueControlID }),
       CreateTextBoxValue_Success,
       CreateTextBoxValue_Error);
}

function CreateTextBoxValue_Success(data) {
    $('[id="' + _valueControlID + '"]').empty();
    $('[id="' + _valueControlID + '"]').append($(data));
    $('[id="' + _valueControlID + '"]').find("div").ecm_loading_hide();
}

function CreateTextBoxValue_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $('[id="' + _valueControlID + '"]').find("div").ecm_loading_hide();
}

// CreateComboboxOperator
function CreateComboboxOperator(fieldId, idCel, valueControlID) {
    _idCel = idCel;
    _valueControlID = valueControlID;
    _fieldId = fieldId;
    $('[id="' + idCel + '"]').ecm_loading_show();

    JsonHelper.helper.post(
       URL_CreateOperator,
       JSON.stringify({ DocumentTypeId: CurrentDocID ,CacheDocType: CacheDocType,FieldID: fieldId ,ValueControlId: valueControlID }),
       CreateComboboxOperator_Success,
       CreateComboboxOperator_Error);
}

function CreateComboboxOperator_Success(data) {
    $('[id="' + _idCel + '"]').empty();
    $('[id="' + _idCel + '"]').append($(data));
    CreateTextBoxValue(-1, _fieldId, _valueControlID);
    $('[id="' + _idCel + '"]').ecm_loading_hide();
}

function CreateComboboxOperator_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $('[id="' + _idCel + '"]').ecm_loading_hide();
}

//AddMoreCondition
function AddMoreCondition() {
    $(".form_query_search").ecm_loading_show();
    JsonHelper.helper.post(
        URL_AddMoreCondition,
        JSON.stringify({ DocumentTypeId: CurrentDocID, CacheDocType: CacheDocType }),
        AddMoreCondition_Success,
        AddMoreCondition_Error);
}

function AddMoreCondition_Success(data) {
    $('.layout_table').append($(data));
    $(".form_query_search").ecm_loading_hide();
    resize_vetical_search();
}

function AddMoreCondition_Error() {
    $(".form_query_search").ecm_loading_hide();


}
//CreateCondition

function CreateCondition() {

    ///Get Ajax
    $(".form_query_search").ecm_loading_show();

    JsonHelper.helper.post(
        URL_CreateSearchQueries,
        JSON.stringify({ DocumentTypeId: CurrentDocID, CacheDocType: CacheDocType }),
        CreateCondition_Success,
        CreateCondition_Error);
}

function CreateCondition_Success(data) {
    $('.form_query_search').find(".layout_table").first().html(data);
    $(".form_query_search").ecm_loading_hide();
    resize_vetical_search();

}

function CreateCondition_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".form_query_search").ecm_loading_hide();
    
}

// tao cac dong du lieu khi click vao query name
function QueryNameChange(_this) {

    if ($(_this).val() <= 0) {
        CreateCondition();
        return;
    }
    ///Get Ajax
    $(".form_query_search").ecm_loading_show();
    JsonHelper.helper.post(
       URL_CreateAdvanceSearchFromQuery,
       JSON.stringify({ DocumentTypeId: CurrentDocID, CacheDocType: CacheDocType, QueryId: $(_this).val() }),
       QueryNameChange_Success,
       QueryNameChange_Error);
    
}

function QueryNameChange_Success(data) {
    $('[id="RowCondition"]').remove();
    $('[id="RowComboBoxQueries"]').parent().append($(data));
    $(".form_query_search").ecm_loading_hide();
}

function QueryNameChange_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".form_query_search").ecm_loading_hide();
}




// popup save query name
var kq = false;
function QueryExits(DocID, Name)
{
    JsonHelper.helper.post(
       URL_QueryExisted,
       JSON.stringify({ docTypeId: DocID, name: Name }),
       QueryExits_Success,
       QueryExits_Error);

    return kq;
}

function QueryExits_Success(data) {
    kq = data;
}

function QueryExits_Error(jqXHR, textStatus, errorThrown) {
    alert("Da bi loi khi kiem tra QueryExits : " + jqXHR + "-" + textStatus + "-" + errorThrown);
}

function bt_save_popup_queryname() {
 
    var query_name = $(".popup_layer .query_name").val();
    if (query_name == "" || query_name== null) {
        $(".name_emptyorexits").text("Chưa nhập tên Query");
    }
    else 
    {
        var json = { "Name": query_name, "DocTypeId": CurrentDocID, SearchQueryExpressions: [] };
        $(".layout_table").children('div').each(function (index) {
            if (index != 0) {
                // != 0 de bo qua dong select queryname 
                var Condition = $(this).find('[id="Conjunction" ]').val();
                var FieldName = $(this).find('[id="FieldName" ]').find("option:selected").first().text();
                if (FieldName == null || FieldName == "")
                    FieldName = $(this).find('[id="FieldName" ]').first().text();
                var Operator = $(this).find('[id="Operator" ]').val();
                var OperatorText = $(this).find('[id="' + Operator + '" ]').text();
                var value1 = $(this).find('[id="value1" ]').val();
                var value2 = $(this).find('[id="value2" ]').val();
                var FieldID = -1;
                for (i = 0 ; i < DocType.DocTypes.length; i++) {
                    if (DocType.DocTypes[i].ID == CurrentDocID) {
                        for (j = 0 ; j < DocType.DocTypes[i].Fields.length; j++) {
                            if (DocType.DocTypes[i].Fields[j].Name == FieldName) {
                                FieldID = DocType.DocTypes[i].Fields[j].IDField;
                                break;
                            }

                        }
                    }
                }
                var FieldUniqueId = "Empty";
                for (i = 0 ; i < DocType.DocTypes.length; i++) {
                    if (DocType.DocTypes[i].ID == CurrentDocID) {
                        for (j = 0 ; j < DocType.DocTypes[i].Fields.length; j++) {
                            if (DocType.DocTypes[i].Fields[j].Name == FieldName) {
                                FieldUniqueId = DocType.DocTypes[i].Fields[j].FieldUniqueId;
                                break;
                            }

                        }
                    }
                }
                // thêm vào trường hệ thống để controller kiểm tra
                var IsSystemField = "False";
                for (i = 0 ; i < DocType.DocTypes.length; i++) {
                    if (DocType.DocTypes[i].ID == CurrentDocID) {
                        for (j = 0 ; j < DocType.DocTypes[i].Fields.length; j++) {
                            if (DocType.DocTypes[i].Fields[j].Name == FieldName) {
                                IsSystemField = DocType.DocTypes[i].Fields[j].IsSystemField;
                                break;
                            }

                        }
                    }
                }
                json.SearchQueryExpressions.push({ Condition: Condition, Field: { Name: FieldName, Id: FieldID, IsSystemField: IsSystemField }, OperatorText: OperatorText, Operator: Operator, Value1: value1, Value2: value2, FieldUniqueId: FieldUniqueId });


            }

        });
        $(".form_query_search").ecm_loading_show();
        $.ajax({

            url: URL_SaveQuery,
            async: true,
            type: "POST",
            data: JSON.stringify(json),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            error: function (jqXHR, textStatus, errorThrown) {
                alert(jqXHR + "-" + textStatus + "-" + errorThrown);
            },
            success: function (data, textStatus, jqXHR) {
                $(".form_query_search").ecm_loading_hide();
                LoadQueryNameByName(query_name);
            }
        });
        // hide popup 
        $(".name_emptyorexits").text("");
        $(document).ecm_popup({ hide: true });

        
    } 
    
}

function bt_cancle_popup_queryname()
{
    $(".name_emptyorexits").text("");
    $(document).ecm_popup({ hide: true });
}

var input_name_query = function () {
    
    var bt = $("button.bt_save_query");
    $(bt).next(".popup").ecm_popup({ width: 250, function_bt_first: bt_save_popup_queryname, function_bt_second: bt_cancle_popup_queryname });
   
}
// Popup delete query
function bt_yes_popup_deletequery() {
    var QueryId;
    QueryId = $("#selectqueryname").val();
    $(".form_query_search").ecm_loading_show();
    $.ajax({
        url: URL_DeteleQuery,
        type: "POST",           
        data: { "QueryID": QueryId },         
        success: function (Data) {
            $(".form_query_search").ecm_loading_hide();

            //Không load lại mà chỉ remove item đang chọn
            //CreateCondition();
            $("#selectqueryname option[value='" + $("#selectqueryname").val() + "']").remove();

            
          }
      });
    $(document).ecm_popup({ hide: true });
}

function bt_no_popup_deletequery() {
    $(document).ecm_popup({ hide: true });
}

var delete_query = function ()
{
    var bt = $("button.bt_delete_query");
    $(bt).next(".popup").ecm_popup({ width: 350, function_bt_first: bt_yes_popup_deletequery, function_bt_second: bt_no_popup_deletequery });
}
//disable bt_delete_query
function disable_bt_delete_query()
{
    $("button.bt_delete_query").attr("disabled", true);
}

//enable bt_delete_query
function enable_bt_delete_query()
{
    $("button.bt_delete_query").removeAttr("disabled");
}
//disable bt_save_query
function disable_bt_save_query()
{
    $("button.bt_save_query").attr("disabled", true);
}
//enable bt_save_query
function enable_bt_save_query()
{
    $("button.bt_save_query").removeAttr("disabled");
}
//Duoc dung khi luu query thanh cong va hien thi len Selectquery
function LoadQueryNameByName($name) {
    $.ajax({
        url: URL_GetSaveQueryName,
        type: "POST",
        async: true,
        data: JSON.stringify({ DocID: CurrentDocID }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (Data) {
            //QueryExpression = jQuery.parseJSON(Data);
            QueryExpression = Data;
            $("#selectqueryname").find("option").remove();
            $("#selectqueryname").append("<option value=" + -1 + ">--- Select query ---</option>");

            for (i = 0 ; i < QueryExpression.QueryNames.length; i++) {
                if (QueryExpression.QueryNames[i].QueryName == $name) {
                    $("#selectqueryname").append("<option value=" + QueryExpression.QueryNames[i].QueryID + " selected>" + QueryExpression.QueryNames[i].QueryName + "</option>");
                    QueryId = QueryExpression.QueryNames[i].QueryID;
                }
                else {
                    $("#selectqueryname").append("<option value=" + QueryExpression.QueryNames[i].QueryID + ">" + QueryExpression.QueryNames[i].QueryName + "</option>");
                }
            }
        }
    });
}
// input_name_query new 
var disable_tabpress = function (e) {
    if (e.which == 9) {
        e.preventDefault();
    }
}


// hien thi popup savequery
function ShowPopUpQueryName() {
    input_name_query();

}

function ShowPopUpDeleteQuery() {
    delete_query();

}

function DocumentSearch(_this) {
    $('.datagript_result').ecm_loading_show();

    JsonHelper.helper.post(
       URL_RunSearchDocumentType,
       JSON.stringify({ docType: $(_this).attr("id"), page: 0 }),
       Search_Success,
       Search_Error);
   
}

function GlobalSearch() {
    $('.datagript_result').ecm_loading_show();

    JsonHelper.helper.post(
       URL_RunGlobalSearch,
       JSON.stringify(json_search_global),
       Search_Success,
       Search_Error);    
}

function Search_Success(data) {
    //console.log(data);
    $('.datagript_result').empty();
    $('.datagript_result').append($(data));    
    $('.datagript_result').ecm_loading_hide();
    resize_event();
}

function Search_Error(jqXHR, textStatus, errorThrown) {
    $('.datagript_result').empty();
    $('.datagript_result').ecm_loading_hide();
    resize_event();
}

function ContentSearch() {
    $('.datagript_result').ecm_loading_show();
    var content_search = $(".ecm_popup").find(".content_text").val();
    JsonHelper.helper.post(
       URL_RunContentSearch,
       JSON.stringify({ DocumentTypeId: CurrentDocID, ContentSearch: content_search }),
       ContentSearch_Success,
       SContentSearch_Error);

}

function close() {
    $(".popup.search_by_content").ecm_popup({ hide: true });
}

function ContentSearch_Success(data) {
    $('.datagript_result').empty();
    $('.datagript_result').append($(data));
    $('.datagript_result').ecm_loading_hide();
    resize_event();
    $(".popup.search_by_content").ecm_popup({ hide: true });
}

function SContentSearch_Error(jqXHR, textStatus, errorThrown) {
    $('.datagript_result').empty();
    $('.datagript_result').ecm_loading_hide();
    resize_event();

    $(".popup.search_by_content").ecm_popup({ hide: true });
}

function AdvanceSearch() {
    $('.datagript_result').ecm_loading_show();

    var query_name = $(".query_name").val();
    var json = { "Name": query_name, "DocTypeId": CurrentDocID, SearchQueryExpressions: [] };
    $(".layout_table").children('div').each(function (index) {
        if (index != 0) {
            // continue;
            var Condition = $(this).find('[id="Conjunction"]').val();
            var FieldName = $(this).find('[id="FieldName"]').find("option:selected").first().text();
            if (FieldName == null || FieldName == "")

                FieldName = $(this).find('[id="FieldName" ]').first().text();
            var Operator = $(this).find('[id="Operator" ]').val();
            var OperatorText = $(this).find('[id="Operator" ]').find('[value="' + Operator + '"]').text();
            var value1 = $(this).find('[id="value1" ]').val();
            var value2 = $(this).find('[id="value2" ]').val();
            var FieldID = -1;
            var FieldUniqueId = "";
            for (i = 0 ; i < DocType.DocTypes.length; i++) {
                if (DocType.DocTypes[i].ID == CurrentDocID) {
                    json.DocumentType = DocType.DocTypes[i];
                    for (j = 0 ; j < DocType.DocTypes[i].Fields.length; j++) {
                        if (DocType.DocTypes[i].Fields[j].Name == FieldName) {
                            FieldID = DocType.DocTypes[i].Fields[j].IDField;
                            fieldUniqueId = DocType.DocTypes[i].Fields[j].FieldUniqueId;

                            break;
                        }
                    }
                }
            } 

            if (value1 != "" || (value2 != "" && value2 != null)) {
                json.SearchQueryExpressions.push({ Condition: Condition, Field: { Id: FieldID, FieldUniqueId: fieldUniqueId }, OperatorText: OperatorText, Operator: Operator, Value1: value1, Value2: value2, FieldUniqueId: fieldUniqueId });
            }

        }

        var pageIndex = 0;
        json_search_advance = {
            queryname: json,
            pageindex: pageIndex
        };

    });
    JsonHelper.helper.post(
       URL_RunAdvanceSearch,
       JSON.stringify(json_search_advance),
       Search_Success,
       Search_Error);
}

/*
Create by Triet
Use to Update query when click bt_save_query

*/
function UpdateQuery()
{  
    var QueryName;
    var QueryId;
    QueryName = $("#selectqueryname option:selected").text();
    QueryId = $("#selectqueryname option:selected").val();
    var json = { "Name": QueryName, "Id": QueryId, "DocTypeId": CurrentDocID, SearchQueryExpressions: [] };
    // luot qua moi dong de lay du lieu dua vao chuoi json
    $(".layout_table").children('div').each(function (index) {
        if (index != 0) {
            // != 0 de bo qua dong select queryname 
            var Condition = $(this).find('[id="Conjunction" ]').val();
            var FieldName = $(this).find('[id="FieldName" ]').find("option:selected").first().text();
            if (FieldName == null || FieldName == "")
                FieldName = $(this).find('[id="FieldName" ]').first().text();
            var Operator = $(this).find('[id="Operator" ]').val();
            var OperatorText = $(this).find('[id="Operator" ]').text();
            var value1 = $(this).find('[id="value1" ]').val();
            var value2 = $(this).find('[id="value2" ]').val();
            var FieldID = -1;
            var ExpressionID;
            ExpressionID = $(this).attr("value");
            if (ExpressionID == null || ExpressionID == "") ExpressionID = 0;

            for (i = 0 ; i < DocType.DocTypes.length; i++) {
                if (DocType.DocTypes[i].ID == CurrentDocID) {
                    for (j = 0 ; j < DocType.DocTypes[i].Fields.length; j++)
                    {
                        if (DocType.DocTypes[i].Fields[j].Name == FieldName)
                        {
                            FieldID = DocType.DocTypes[i].Fields[j].IDField;
                            break;
                        }
                        
                    }
                }
            }
            var FieldUniqueId = "Empty";
            for (i = 0 ; i < DocType.DocTypes.length; i++) {
                if (DocType.DocTypes[i].ID == CurrentDocID) {
                    for (j = 0 ; j < DocType.DocTypes[i].Fields.length; j++)
                    {
                        if (DocType.DocTypes[i].Fields[j].Name == FieldName)
                        {
                            FieldUniqueId = DocType.DocTypes[i].Fields[j].FieldUniqueId;
                            break;
                        }
                        
                    }
                }
            }
            // Xử lý đối với Field hệ thống 
            for (i = 0 ; i < DocType.DocTypes.length; i++) {
                if (DocType.DocTypes[i].ID == CurrentDocID) {
                    for (j = 0 ; j < DocType.DocTypes[i].Fields.length; j++) {
                        if (DocType.DocTypes[i].Fields[j].Name == FieldName && DocType.DocTypes[i].Fields[j].IsSystemField == "True") {
                            FieldUniqueId = DocType.DocTypes[i].Fields[j].FieldUniqueId;
                            break;
                        }
                    }
                }
            }
            json.SearchQueryExpressions.push({ Id: ExpressionID, SearchQueryId:QueryId,Condition: Condition, Field: { Id: FieldID, FieldUniqueId: FieldUniqueId }, OperatorText: OperatorText, Operator: Operator, Value1: value1, Value2: value2, FieldUniqueId: FieldUniqueId });
            console.log(ExpressionID);
            
        }

    });
    $(".form_query_search").ecm_loading_show();
    $.ajax({
        url: URL_SaveQuery,
        async: true,
        type: "POST",
        data: JSON.stringify(json),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        error: function (jqXHR, textStatus, errorThrown) {
            alert(jqXHR + "-" + textStatus + "-" + errorThrown);
        },
        success: function (data, textStatus, jqXHR) {         
            $(".form_query_search").ecm_loading_hide();
            LoadQueryNameByName(QueryName);
        }
    });
}

$(document).ready(function () {

    //CurrentDocID = firstIDDocType;
    //if ($("div.document_list"))
        //$("div.document_list:first").click();
    //CreateCondition();
    //Event when click bt_add_conditions
    $("button.bt_add_conditions").click(function () {
        AddMoreCondition();
    });
    //Event when clich bt_reset_conditions 
    // Còn dùng được

    $("button.bt_reset_conditions").click(function () {       
        CreateCondition();
       // QueryId = -1;
    });
    // event when click each DocType in left_panel
    $("div.document_list").click(function ()
    {
        if (CurrentDocID == $(this).attr("id"))
            return;
        var i;
        // luu giu id cua DocType
        CurrentDocID = $(this).attr("id"); 
        //alert(" CurrentDocID = " + CurrentDocID);
        var name = $(".data_title").val();
        $(".selected").removeClass('selected');
        $(this).attr("class", "document_list selected");       
        //load các fields tương ứng với DocType        
        CreateCondition();
        DocumentSearch(this);
        var documentTypeId = $('div.document_list.selected').attr('id').toString();
        var _permission = permission.documentType[documentTypeId];
        createContextMenu(_permission);
    });
   
   $("button.bt_save_query").click(function () 
   {
       var query_id = $("#selectqueryname").val();
       if (query_id == -1) {
           ShowPopUpQueryName();
       }
       else
       {
           UpdateQuery();
       }
       
      
   });

   $("button.bt_delete_query").click(function () {      
       var query_id = $("#selectqueryname").val();
       if (query_id != -1) {
           ShowPopUpDeleteQuery();
       }
     
      
   });
    

    //advance search - event click search button
    $(".bt_search").click(function () {
        AdvanceSearch();
        var documentTypeId = $('div.document_list.selected').attr('id').toString();
        var _permission = permission.documentType[documentTypeId];
        createContextMenu(_permission);
    });

    //global search - event click search icon in top menu
    $(document).on("click", ".searchglobal_button", function () {
        var globalsearch_text = $(".globalsearch_text").val();
        json_search_global = {
            keyword: globalsearch_text,
            pageindex: 0
        };
        GlobalSearch();
    });

    //global search - event search icon in top menu (enter pressed)
    $(document).on("keyup", ".globalsearch_text", function (e) {
        if (e.which == 13) {
            $(".searchglobal_button").click();
        }
    });

    $(document).on("click", "#hasMoreAdvanceResult", function () {

        $('.datagript_result').ecm_loading_show();

        var $this = $(this);
        json_search_advance.pageindex = parseInt($this.attr("data-pageindex")) + 1;

        var uniqueKey = $this.data("uniquekey");
        var $this_div_id = $("#" + uniqueKey);
        var $this_div_class = $("." + uniqueKey);
        var resultCount = parseInt($this.attr("data-resultcount"));

        JsonHelper.helper.post(
           URL_RunAdvanceSearch,
           JSON.stringify(json_search_advance),
          function (data) {

              $this_div_id.find("tbody").append(data);

              var displayCount = parseInt($this_div_class.find(".count_display").text()) + resultCount;
              var totalCount = parseInt($this_div_class.find(".count_total").text());

              $this.attr("data-pageindex", json_search_advance.pageindex);

              if ($this.data("hasmoreresult")=="False") {
                  $this_div_class.find(".count_display").text(totalCount);
                  $this.remove();
              }
              else {
                  $this_div_class.find(".count_display").text(displayCount);
              }
              $(".datagript_result").ecm_loading_hide();
          },
          Search_Error);
    });

    $(document).on("click", "#hasMoreGlobalResult", function () {
        $(".datagript_result").ecm_loading_show();

        var $this = $(this);
        var pageIndex = parseInt($this.attr("data-pageindex")) + 1;
        json_search_global.pageindex = pageIndex;
        JsonHelper.helper.post(
            URL_RunGlobalSearch,
            JSON.stringify(json_search_global),
            function (data) {
                //current page(show)
                var list_global_result = [];
                $.each($(".documenttypeid"), function () {
                    var tmp = $(this).data("id");
                    list_global_result.push(tmp);
                });

                //will append page(not show yet)
                var has_more_data = $("<div></div>").append($(data));
                var $div_doc_id = has_more_data.find(".documenttypeid");

                $.each($div_doc_id, function () {
                    var $this = $(this);
                    var this_doc_id = $this.data("id");
                    var have_doc_id = false;
                    $.each(list_global_result, function (index, value) {
                        if (this_doc_id == value) {
                            have_doc_id = true;
                            return false;
                        }
                    });
                    //append data for view page
                    if (have_doc_id) {
                        var data_append = $this.find("tbody tr");
                        $(".datagript_result").find("#" + this_doc_id).find("tbody").append(data_append);
                        var doc_count = $(".datagript_result").find("#" + this_doc_id).find("tbody tr").length;
                        $(".datagript_result").find("#" + this_doc_id).find(".count_display").text(doc_count);
                    } else {
                        $(".datagript_result").append($this);
                    }
                });

                $("#hasMoreGlobalResult").attr("data-pageindex", pageIndex);
                if(has_more_data.find("#hasMoreGlobalResult").length>0){
                    $("#hasMoreGlobalResult").attr("data-hasmoreresult", has_more_data.find("#hasMoreGlobalResult").data("hasmoreresult"));
                } else {
                    $("#hasMoreGlobalResult").attr("data-hasmoreresult", "False");
                }

                if ($("#hasMoreGlobalResult").attr("data-hasmoreresult") == "False") {
                    $("#hasMoreGlobalResult").remove();
                }

                $(".datagript_result").ecm_loading_hide();
            },
            Search_Error
            );
    });

    //$(".document_list").click(function () {
    //    DocumentSearch(this);
    //    var documentTypeId = $('div.document_list.selected').attr('id').toString();
    //    var _permission = permission.documentType[documentTypeId];
    //    createContextMenu(_permission);
    //});
    $(".bt_search_content").click(function () {
        $(".popup.search_by_content").ecm_popup({ width: 250, index: 9999, function_bt_first: ContentSearch, function_bt_second: close });
    });

    $("div.document_list:first").click();

    $(document).on("change", ":checkbox[title='Check all']", function () {
        var $checked = false;
        if ($(this).is(":checked")) {
            $checked = true;
        }

        var $table_body = $(this).parent().parent().parent().parent().find("tbody");
        var $list_checkbox = $table_body.find(":checkbox:not([title='Check all'])");
        
        if ($checked) {
            $list_checkbox.prop("checked", true);
        } else {
            $list_checkbox.prop("checked", false);
        }
    });

    $(document).on("change", ":checkbox:not([title='Check all'])", function () {
        var $table = $(this).parent().parent().parent().parent();
        var $checkbox_all = $table.find("thead").find(":checkbox[title='Check all']");
        var $checkbox = $table.find("tbody").find(":checkbox:not([title='Check all'])");
        var checked_all = true;
        $.each($checkbox, function () {
            if ($(this).is(":not(:checked)")) {
                checked_all = false;
                return false;
            }
        });

        if (checked_all) {
            $checkbox_all.prop("checked", true);
        } else {
            $checkbox_all.prop("checked", false);
        }

    });

});

function DeleteDocument(id) {
    $.ajax({
        url: URL_Insert,
        type: "POST",
        data: {id: id},
        error: function (jqXHR, textStatus, errorThrown) {
            console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        },
        success: function (data, textStatus, jqXHR) {
            console.log(data.Message);
        }
    });
}

function createContextMenu(permission) {
    var _items = {
        open: {
            name: "Open",
            iconUrl: URL_MenuOpen,
        },
        delete: {
            name: "Delete",
            iconUrl: URL_MenuDelete,
        }
    };
    if (permission.AllowedPrintDocument == true)
        _items.print = {
            name: "Print",
            iconUrl: URL_MenuPrint,
        };
    if (permission.AllowedEmailDocument == true)
        _items.email = {
            name: "Email",
            iconUrl: URL_MenuEmail,
        };
    if (permission.AllowedDownloadOffline == true)
        _items.download = {
            name: "Download",
            iconUrl: URL_MenuDownload,
        };
    $.contextMenu({
        selector: "tr",
        items: _items,
        style: {
            height: 30, fontSize: 11, menuWidth: 250
        },
        callback: function (key) {
            switch (key) {
                case 'open':
                    $(this).dblclick();
                    break;
                case 'delete':{
                    var documentId = $(this).find('input[type=checkbox]').val();
                    var row = $(this);
                    $.messageBox({
                        title: "Delete Document",
                        message: "Are you sure delete this document?",
                        buttons: {
                            OK: function () {
                                try {
                                    DeleteDocument(documentId);
                                    row.remove();
                                    var resultCount = parseInt($('.count span.count_display').text()) - 1;
                                    $('.count span.count_display').text(resultCount);
                                    var totalCount = parseInt($('.count span.count_total').text()) - 1;
                                    $('.count span.count_total').text(totalCount);
                                } catch (e) {
                                    console.log(e);
                                }
                                $(this).dialog("close");
                            },
                            Cancel: function () {
                                $(this).dialog("close");
                            }
                        }
                    });
                    break;
                }
                case 'email': {
                    var docId = $(this).find('td > input').val();
                    Document = {
                        DocumentTypeId: "",
                        DocumentId: docId
                    }
                    createMailForm(Document);
                    break;
                }
                case 'download': {
                    var docId = $(this).find('td > input').val();
                    Document = {
                        DocumentTypeId: "",
                        DocumentId: docId
                    }
                    createFormDownload(Document);
                }
                default: {
                }
            }
        },
        events: {
            show: function (opt) {
                console.log(this);
                $('tr.selected').removeClass('selected');
                $(this).addClass("selected");
            },
            hide: function (opt) {
                $('tr.selected').removeClass('selected');
            }
        }
    });

    function createMailForm(Document) {
        //#region create html element
        var formData = $("<form class='save_or_mail_option'></form>");
        var attach = $("<fieldset>" +
                        "<legend>Attachment File Options </legend>" +
                        "<input type='radio' name='format' value='pdf' checked='checked' id='mail_opt_pdf'/>" +
                        "<label for='mail_opt_pdf'>Email attachment as PDF</label>" +
                        "<input type='radio' name='format' value='tiff'/ id='mail_opt_tiff'>" +
                        "<label for='mail_opt_tiff'>Email attachment as TIFF</label>" +
                    "</fieldset>" +
                    "<fieldset> " +
                            "<legend>Page range</legend>" +
                            "<input type='radio' name='pagerange' value='all' id='mail_opt_all' checked='checked'/>" +
                            "<label for='mail_opt_all'>All</label>" +
                            "<input type='radio' name='pagerange' value='pages' id='mail_opt_pages'/>" +
                            "<label for='mail_opt_pages'>Pages</label>" +
                            "<input type='text' name='listofpage' />" +
                         "</fieldset>");
        var mailBy = $("<fieldset> " +
                            "<legend>Send by</legend>" +
                            "<input type='radio' name='sendby' value='server' checked='checked' id='mail_opt_server'/>" +
                            "<label for='mail_opt_server'>Server</label>" +
                            "<input type='radio' name='sendby' value='client' id='mail_opt_client'/>" +
                            "<label for='mail_opt_client'>Client (Open Outlook)</label>" +
                         "</fieldset>");
        var mailTo = $("<fieldset> " +
                        "<legend>To Email</legend>" +
                        "<span>To email </span><input type='text' name='mailTo'/>" +
                        "<a href='#'>Cc, Bcc</a><br/>" +
                    "</fieldset>");
        var cc = $("<span>CC </span><input type='text' name='cc'/>" +
                 "<span>BCC </span><input type='text' name='bcc'/>");
        //#endregion
        //#region create style
        cc.hide();
        mailTo.children('span').css({ display: 'inline-block', width: '60px' });
        mailTo.children('input').css({ display: 'inline-block', width: '150px' });
        mailTo.append(cc);
        mailTo.children('a').click(function () {
            if (cc.css('display') == "none")
                cc.css('display', 'inline-block')
            else
                cc.hide();
            return false;
        });
        cc.filter('span').css({ width: '60px' });
        cc.filter('input').css({ width: '150px' });
        attach.add(mailBy).add(mailTo).css({ 'font-size': '15px', 'font-weight': 'bold' });
        attach.add(mailBy).add(mailTo).children('span,label').css({ 'font-size': '14px', 'font-weight': 'normal' });
        formData.append(attach).append(mailBy).append(mailTo);
        //#endregion
        $.messageBox({
            title: 'Input mail address',
            type: 'form',
            message: null,
            formData: formData,
            width: "500px",
            buttons: {
                OK: function (data) {
                    var mail = {};
                    var form = $('.save_or_mail_option');
                    mail['format'] = $('input[name="format"]:checked', form).val();
                    mail['range'] = $('input[name="pagerange"]:checked', form).val();
                    mail['pages'] = [];
                    var ps = $('input[name="listofpage"]', form).val();
                    if (mail['range'] == 'pages' && ps && ps != " ") {
                        var pages = $('input[name="listofpage"]', form).val().split(',');
                        $.each(pages, function () {
                            if ($.isNumeric(this))
                                mail.pages.push(parseInt(this));
                            if (this.indexOf('-') > 0) {
                                var p2p = this.split('-');
                                if (!$.isNumeric(p2p[0]) || !$.isNumeric(p2p[1])) {
                                    pageError.show();
                                    return;
                                }
                                var d = p2p[1] - p2p[0];
                                if (d >= 0)
                                    for (i = p2p[0]; i <= p2p[1]; i++)
                                        if (mail.pages.indexOf(i) < 0)
                                            mail.pages.push(parseInt(i))

                            }
                        });
                    }
                    mail['sendby'] = $('input[name="sendby"]', form).val();
                    mail['mailTo'] = $('input[name="mailTo"]', form).val();
                    mail['cc'] = $('input[name="cc"]', form).val();
                    mail['bcc'] = $('input[name="bcc"]', form).val();
                    mail['Document'] = Document;
                    $('section.body').ecm_loading_show();
                    JsonHelper.helper.post(URL_SendMail, JSON.stringify(mail), function (data) {
                            $('section.body').ecm_loading_hide();
                        if (data == true)
                            $.messageBox({ type: 'notify', message: 'Your email has been send' });
                        else
                            $.messageBox({ type: 'notify', message: 'Failed to send your email!' });
                    }, function () {
                        $('section.body').ecm_loading_hide();
                    });
                },
                Cancel: function () { }
            }
        });
    }
    function createFormDownload(Document) {
        var formData = $("<form class='save_or_mail_option'></form>");
        var attach = $("<fieldset>" +
                        "<legend>Attachment File Options </legend>" +
                        "<input type='radio' name='format' value='pdf' checked='checked' id='mail_opt_pdf'/>" +
                        "<label for='mail_opt_pdf'>Email attachment as PDF</label>" +
                        "<input type='radio' name='format' value='tiff'/ id='mail_opt_tiff'>" +
                        "<label for='mail_opt_tiff'>Email attachment as TIFF</label><br/>" +
                    "</fieldset><br/>" +
                    "<fieldset> " +
                            "<legend>Page range</legend>" +
                            "<input type='radio' name='pagerange' value='all' id='mail_opt_all' checked='checked'/>" +
                            "<label for='mail_opt_all'>All</label>" +
                            "<input type='radio' name='pagerange' value='pages' id='mail_opt_pages'/>" +
                            "<label for='mail_opt_pages'>Pages</label>" +
                            "<input type='text' name='listofpage' />" +
                         "</fieldset>");
        attach.children('span,label').css({ 'font-size': '14px', 'font-weight': 'normal' });
        formData.append(attach);
        $.messageBox({
            title: 'Save option',
            type: 'form',
            message: null,
            formData: formData,
            width: "500px",
            buttons: {
                OK: function (data) {
                    var mail = {};
                    var form = $('.save_or_mail_option');
                    mail['format'] = $('input[name="format"]:checked', form).val();
                    mail['range'] = $('input[name="pagerange"]:checked', form).val();
                    mail['pages'] = [];
                    var ps = $('input[name="listofpage"]', form).val();
                    if (mail['range'] == 'pages' && ps && ps != " ") {
                        var pages = $('input[name="listofpage"]', form).val().split(',');
                        $.each(pages, function () {
                            if ($.isNumeric(this))
                                mail.pages.push(parseInt(this));
                            if (this.indexOf('-') > 0) {
                                var p2p = this.split('-');
                                if (!$.isNumeric(p2p[0]) || !$.isNumeric(p2p[1])) {
                                    pageError.show();
                                    return;
                                }
                                var s = p2p[1] - p2p[0];
                                if (d >= 0)
                                    for (i = p2p[0]; i <= p2p[1]; i++)
                                        if (mail.pages.indexOf(i) < 0)
                                            mail.pages.push(parseInt(i))

                            }
                        });
                    }
                    mail['Document'] = Document;
                    $('section.body').ecm_loading_show();
                    JsonHelper.helper.post(URL_SaveLocal, JSON.stringify(mail), function (data) {
                        //var $p = $('<iframe src="/capture/get?key=' + data + '"></iframe>');
                        //$("#printdiv").append($p);
                        $('section.body').ecm_loading_hide();
                        window.open(URL_Get + "?key=" + data, "_blank");
                    });
                },
                Cancel: function () { }
            }
        });
    }
}

function setUrlSearchHasMoreResult(url) {
    $('#hasMoreResult').click(function () {
        if ($(this).data('hasMore')) {
            var pageIndex = parseInt($(this).data('pageIndex')) + 1;
            $('.datagript_result').ecm_loading_show();
            var id =$('div.document_list.selected').attr('id');
            JsonHelper.helper.post(
               url,
               JSON.stringify({ docType: id, pageIndex: pageIndex }),
               function (data) {
                   $(".datagript_table tbody").append(data);
                   $('.datagript_result').ecm_loading_hide();
                   var resultCount = parseInt($('.count span.count_display').text());
                   var totalCount = parseInt($('.count span.count_total').text());

                   if (resultCount >= totalCount)
                       $('#hasMoreResult').remove();
               },
               Search_Error);
        }
    });
}