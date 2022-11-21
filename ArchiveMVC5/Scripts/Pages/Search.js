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
    $('#searchcondition').append($(data));
    $('.layout_table').scrollTo($('.layout_table').children().last());
    $(".form_query_search").ecm_loading_hide();
}

function AddMoreCondition_Error() {
    $(".form_query_search").ecm_loading_hide();


}
//Load field value condition
function LoadTextBoxValue(searchDiv, operator, fieldid, value1, value2) {
    searchDiv.ecm_loading_show();
    JsonHelper.helper.post(
        URL_LoadSearchValueControl,
        JSON.stringify({ DocumentTypeId: CurrentDocID, FieldId: fieldid, searchOperator: operator, Value1: value1, Value2: value2 }),
        function (data) {
            searchDiv.empty();
            searchDiv.append(data);
            searchDiv.ecm_loading_hide();
        },
        function (jqXHR, textStatus, errorThrown) {
            console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
            searchDiv.ecm_loading_hide();
        });
}

//Clear condition

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

function CreateConditionFromQuery(queryId) {

    ///Get Ajax
    $(".form_query_search").ecm_loading_show();

    JsonHelper.helper.post(
        URL_CreateSearchConditionFromQueries,
        JSON.stringify({ QueryId: queryId }),
        CreateConditionFromQuery_Success,
        CreateCondition_Error);
}

function CreateCondition_Success(data) {
    $('.form_query_search').find(".layout_table").first().html(data);
    $(".form_query_search").ecm_loading_hide();
    //resize_vetical_search();

}

function CreateConditionFromQuery_Success(data) {
    $('.form_query_search').find("#searchcondition").empty();
    $('.form_query_search').find("#searchcondition").append(data);
    AdvanceSearch();
    $(".form_query_search").ecm_loading_hide();

}

function CreateCondition_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".form_query_search").ecm_loading_hide();
    
}

// tao cac dong du lieu khi click vao query name
function QueryNameChange(_this) {

    if ($(_this).val() != '-1') {
        CreateConditionFromQuery($(_this).val());
    }
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
    $.EcmAlert("System error. Please contact administrator!", "WARNNING", BootstrapDialog.TYPE_WARNING);
}

function bt_save_popup_queryname(e) {
 
    var $query_name = $("#queryName");

    if (!$query_name.val()) {
        // Add errors highlight
        $query_name.removeClass('has-success').addClass('has-error');
        // Stop submission of the form
        e.preventDefault();
    } else {
        // Remove the errors highlight
        $query_name.removeClass('has-error').addClass('has-success');
        var json = { "Name": $query_name.val(), "DocTypeId": CurrentDocID, SearchQueryExpressions: [] };
        $(".layout_table").find('#searchcondition').children('div').each(function (index) {
            //if (index != 0) {
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

                if ((value1 != undefined && value1 != "") || (value2 != undefined && value2!="")) {
                    json.SearchQueryExpressions.push({ Condition: Condition, Field: { Name: FieldName, Id: FieldID, IsSystemField: IsSystemField }, OperatorText: OperatorText, Operator: Operator, Value1: value1, Value2: value2, FieldUniqueId: FieldUniqueId });
                }

            //}

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
                $.EcmAlert("Save query fail. Please contact administrator", "WARNNING", BootstrapDialog.TYPE_WARNING);
            },
            success: function (data, textStatus, jqXHR) {
                $(".form_query_search").ecm_loading_hide();
                LoadQueryNameByName($query_name.val());
            }
        });
        // hide popup 
        //$(document).ecm_popup({ hide: true });
        $("#saveQuery").modal("hide");
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
    $("#deleteQuery").modal("hide");
}

//disable bt_delete_query
function disable_bt_delete_query()
{
    $("button.bt_delete_query").attr("disabled", true);
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


function DocumentSearch(_this) {
    $('.datagript_result').ecm_loading_show();

    JsonHelper.helper.post(
       URL_RunSearchDocumentType,
       JSON.stringify({ docType: $(_this).attr("id"), pageIndex: 0, pageSize: 20 }),
       Search_Success,
       Search_Error);

    $('.table > thead > tr > th').removeClass('desc');
    $('.table > thead > tr > th').removeClass('asc');
    $('.table > thead > tr > th.sortable').addClass('both');
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
}

function Search_Error(jqXHR, textStatus, errorThrown) {
    $('.datagript_result').empty();
    $('.datagript_result').ecm_loading_hide();
    //resize_event();
}

function Paging_Error(jqXHR, textStatus, errorThrown) {
    $('.datagript_result').empty();
    $('.datagript_result').ecm_loading_hide();
}

function Paging_Success(data) {
    $('.datagript_result').empty();
    $('.datagript_result').append($(data));
    $('.datagript_result').ecm_loading_hide();
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
    //resize_event();
    $(".popup.search_by_content").ecm_popup({ hide: true });
}

function SContentSearch_Error(jqXHR, textStatus, errorThrown) {
    $('.datagript_result').empty();
    $('.datagript_result').ecm_loading_hide();
    //resize_event();

    $(".popup.search_by_content").ecm_popup({ hide: true });
}

function AdvanceSearch() {
    $('.datagript_result').ecm_loading_show();

    var jsonCondition = getAdvanceSearchConditions();

    json_search_advance = {
        queryname: jsonCondition,
        paging: { PageIndex: 0, PageSize: 20 }
    };

    JsonHelper.helper.post(
       URL_RunAdvanceSearch,
       JSON.stringify(json_search_advance),
       Search_Success,
       Search_Error);
}

function getAdvanceSearchConditions() {
    var json = { "DocTypeId": CurrentDocID, SearchQueryExpressions: [] };

    $("#searchcondition").children('div').each(function (index) {
        var Condition = $(this).find('[id="Conjunction"]').val();
        var FieldName = $(this).find('[id="FieldName"]').find("option:selected").first().text();

        if (FieldName == null || FieldName == "") {
            FieldName = $(this).find('[id="FieldName" ]').first().text();
        }

        var Operator = $(this).find('[id="Operator" ]').val();
        var OperatorText = $(this).find('[id="Operator" ]').find('[value="' + Operator + '"]').text();
        var value1 = $(this).find('[id="value1" ]').val();
        var value2 = $(this).find('[id="value2" ]').val();
        var FieldID = -1;
        var FieldUniqueId = "";
        var dataType = $(this).find('[id="fieldDataType" ]').val();

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

        if ((value1 != "" && value1 != undefined) || (value2 != "" && value2 != undefined)) {
            json.SearchQueryExpressions.push({ Condition: Condition, Field: { Id: FieldID, FieldUniqueId: fieldUniqueId, DataType: dataType, Name: FieldName }, OperatorText: OperatorText, Operator: Operator, Value1: value1, Value2: value2, FieldUniqueId: fieldUniqueId });
        }


    });

    return json;
}

function UpdateQuery()
{  
    var QueryName;
    var QueryId;
    QueryName = $("#selectqueryname option:selected").text();
    QueryId = $("#selectqueryname option:selected").val();
    var json = { "Name": QueryName, "Id": QueryId, "DocTypeId": CurrentDocID, SearchQueryExpressions: [] };
    // luot qua moi dong de lay du lieu dua vao chuoi json
    $("#searchcondition").children('div').each(function (index) {
        //if (index != 0) {
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
            
        //}

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
            $.EcmAlert("Save query fail. Please contact administrator!", "WARNNING", BootstrapDialog.TYPE_WARNING);
        },
        success: function (data, textStatus, jqXHR) {         
            $(".form_query_search").ecm_loading_hide();
            LoadQueryNameByName(QueryName);
        }
    });
}

function sortData(columnName) {
}

function changePage(index, pagesize, sortcolumn, sortdirection) {
    $('.datagript_result').ecm_loading_show();

    var jsonCondition = getAdvanceSearchConditions();
    json_search_advance = {
        queryname: jsonCondition,
        paging: { PageIndex: index, PageSize: pagesize, sortcolumnname: sortcolumn, sortdirection: sortdirection }
    };

    JsonHelper.helper.post(
       URL_RunAdvanceSearch,
       JSON.stringify(json_search_advance),
       Paging_Success,
       Paging_Error);


    $('.table > thead > tr > th').removeClass('desc');
    $('.table > thead > tr > th').removeClass('asc');
    $('.table > thead > tr > th.sortable').addClass('both');

    if (sortcolumn != "") {
        $('.table > thead > tr').find("#" + sortcolumn.replace(" ", "")).removeClass('both').addClass(sortdirection);
        $('.table > thead > tr').find("#" + sortcolumn.replace(" ", "")).data("sortdir", sortdirection);
    }

}

$(document).ready(function () {

    //Event when click bt_add_conditions
    $("button.bt_add_conditions").click(function () {
        AddMoreCondition();
    });
    //Event when clich bt_reset_conditions 
    $(document).on("click", ".delete-new",function (e) {
        e.preventDefault();
        $(this).parent().remove();
    });
    $("button.bt_reset_conditions").click(function () {       
        CreateCondition();
       // QueryId = -1;
    });
    // event when click each DocType in left_panel
    $(".document_list").click(function ()
    {
        var i;
        CurrentDocID = $(this).attr("id"); 
       var name = $(".data_title").val();
        $(".active").removeClass('active');
        $(this).attr("class", "document_list active");       
        CreateCondition();
        DocumentSearch(this);
        var documentTypeId = $('.document_list.active').attr('id').toString();
        var _permission = permission.documentType[documentTypeId];
        createContextMenu(_permission);
    });
   
   $(".bt_save_query").click(function () 
   {
       var query_id = $("#selectqueryname").val();
       if (query_id == -1) {
           $("#saveQuery").modal("show");
       }
       else
       {
           UpdateQuery();
       }
   });

   $(".bt_delete_query").click(function () {      
       var query_id = $("#selectqueryname").val();
       if (query_id != -1) {
           $("#deleteQuery").modal("show");
       }
   });

   $(document).on('click', '#btSaveQuery', function (e) {
       bt_save_popup_queryname(e);
   });

   $(document).on('click', '#btDeleteQuery', function (e) {
       bt_yes_popup_deletequery();
   });
    //advance search - event click search button
    $(".bt_search").click(function () {
        AdvanceSearch();
        var documentTypeId = $('.document_list.active').attr('id').toString();
        var _permission = permission.documentType[documentTypeId];
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

    $(document).on('dblclick', 'tr', function () {
        var docId = $(this).attr('id');
        $("section.body").ecm_loading_show();
        window.location = URL_EditDocID + docId;
    });

    $(".bt_search_content").click(function () {
        $(".popup.search_by_content").ecm_popup({ width: 250, index: 9999, function_bt_first: ContentSearch, function_bt_second: close });
    });

    $(".document_list:first").click();

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

    $(document).on("change", "#selectqueryname", function () {
        QueryNameChange(this);
    });

    $(document).on("change", "#Operator", function (e) {
        var operator = $(this).val();
        var fieldId = $(this).parent().parent().data("fieldid");
        var $valueDiv = $(this).parent().parent().find("#" + fieldId);
        var value1 = $valueDiv.find("#value1").val();
        var value2 = $valueDiv.find("#value2").val();

        LoadTextBoxValue($valueDiv, operator, fieldId, value1, value2);
    });

    $(document).on("click", "#next_button", function (e) {
        var $pagingControl = $("#paging");
        var $searchTable = $("#advance_search_result");
        var currentIndex = $pagingControl.data("pageindex");
        var pageSize = $pagingControl.data("pagesize");
        var sortcolumn = $searchTable.data("sortcolumn");
        var sortdir = $searchTable.data("sortdirection");

        changePage(currentIndex + 1, pageSize, sortcolumn, sortdir);
    });

    $(document).on("click", "#previous_button", function (e) {
        var $pagingControl = $("#paging");
        var $searchTable = $("#advance_search_result");
        var currentIndex = $pagingControl.data("pageindex");
        var pageSize = $pagingControl.data("pagesize");
        var sortcolumn = $searchTable.data("sortcolumn");
        var sortdir = $searchTable.data("sortdirection");

        changePage(currentIndex - 1, pageSize, sortcolumn, sortdir);
    });

    $(document).on("click", ".paging_item_button", function (e) {
        var currentIndex = $(this).attr("id");
        var $pagingControl = $("#paging");
        var $searchTable = $("#advance_search_result");
        var pageSize = $pagingControl.data("pagesize");
        var sortcolumn = $searchTable.data("sortcolumn");
        var sortdir = $searchTable.data("sortdirection");

        changePage(currentIndex, pageSize, sortcolumn, sortdir);
    });

    $(document).on("click", ".sortable", function (e) {
        var sortColumn = $(this).data("sortcolumn");
        var sortDir = $(this).data("sortdir");
        var $pagingControl = $("#paging");
        var $searchTable = $("#advance_search_result");
        var currentIndex = $pagingControl.data("pageindex");
        var pageSize = $pagingControl.data("pagesize");

        if (sortDir == "") {
            sortDir = "asc";
        }
        else if (sortDir == "asc") {
            sortDir = "desc";
        }
        else if (sortDir == "desc") {
            sortDir = "asc";
        }

        changePage(currentIndex, pageSize, sortColumn, sortDir);
    });

    $(document).on("click", ".page_size", function (e) {
        var $pagingControl = $("#paging");
        var $searchTable = $("#advance_search_result");
        var currentIndex = $pagingControl.data("pageindex");
        var pageSize = $(this).text();
        var sortcolumn = $searchTable.data("sortcolumn");
        var sortdir = $searchTable.data("sortdirection");

        if (pageSize == "All") {
            pageSize = -1;
        }

        if (sortdir == "") {
            sortdir = "asc";
        }
        else if (sortdir == "asc") {
            sortdir = "desc";
        }

        changePage(currentIndex, pageSize, sortcolumn, sortdir);
    });
});

function DeleteDocument(id) {
    $.ajax({
        url: URL_Delete,
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
    if (permission == undefined) {
        return false;
    }
    var _items = {
        open: {
            name: "Open",
            imgClass: "glyphicon glyphicon-open-file large",
        },
    };
    if (permission.AllowedPrintDocument == true)
        _items.print = {
            name: "Print",
            imgClass: "fa fa-print large",
        };
    if (permission.AllowedEmailDocument == true)
        _items.email = {
            name: "Email",
            imgClass: "fa fa-envelope large",
        };
    if (permission.AllowedDownloadOffline == true)
        _items.download = {
            name: "Download",
            imgClass: "glyphicon glyphicon-save-file large",
        };
    _items.delete = {
        name: "Delete",
        imgClass: "glyphicon glyphicon-trash large",
    };

    $.contextMenu({
        selector: "tbody > tr",
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
                    var documentId = $(this).attr('id');
                    var row = $(this);
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
                            } 
                        }
                    });
                    break;
                }
                case 'email': {
                    var docId = $(this).attr('id');
                    Document = {
                        DocumentTypeId: "",
                        DocumentId: docId
                    }
                    createMailForm(Document);
                    break;
                }
                case 'download': {
                    var docId = $(this).attr('id');
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
        var formData = $("<div class='box box-info save_or_mail_option'>" +
                            "<div class='box-header with-border'>Sent mail information</div>" +
                            "<div class='box-body'></div>"+
                        "</div>");
        var attach = $("<div class='form-group'><h4>Attachment File Options</h4></div>" +
                        "<div class='form-group'>" +
                            "<input type='radio' name='format' value='pdf' checked='checked' id='mail_opt_pdf' class='col-md-1'/>" +
                            "<label for='mail_opt_pdf' class='label-control col-md-5'>Email attachment as PDF</label>" +
                            "<input type='radio' name='format' value='tiff'/ id='mail_opt_tiff' class='col-md-1'/>" +
                            "<label for='mail_opt_tiff' class='label-control col-md-5'>Email attachment as TIFF</label>" +
                        "</div>" +
                    "</div>" +
                    "<div class='form-group'><h4>Page range</h4></div>" +
                        "<div class='form-group'>" +
                            "<input type='radio' name='pagerange' value='all' id='mail_opt_all' checked='checked' class='col-md-1'/>" +
                            "<label for='mail_opt_all' class='label-control col-md-3'>All</label>" +
                            "<input type='radio' name='pagerange' value='pages' id='mail_opt_pages' class='col-md-1'/>" +
                            "<label for='mail_opt_pages' class='label-control col-md-3'>Pages</label>" +
                        "</div>"+
                        "<div class='form-group'>" +
                            "<input type='text' name='listofpage' class='form-control input-sm' />" +
                        "</div>");
        var mailBy = $("<div class='form-group'><h4>Send by</h4></div>" +
                            "<div class='form-group'>"+
                                "<input type='radio' name='sendby' value='server' checked='checked' id='mail_opt_server' class='col-md-1'/>" +
                                "<label for='mail_opt_server' class='label-control col-md-5'>Send email immediately</label>" +
                                "<input type='radio' name='sendby' value='client' id='mail_opt_client' class='col-md-1'/>" +
                                "<label for='mail_opt_client' class='label-control col-md-5'>Compose email</label>" +
                        "</div>");
        var mailTo = $("<div class='form-group'> " +
                            "<h4>Send mmail information</h4>" +
                        "</div>" +
                        "<div class='form-group'>"+
                            "<label class='label-control'>To email </label>"+
                            "<input type='text' name='mailTo' class='form-control input-sm'/>" +
                            "<a href='#'>Add Cc, Bcc email address</a><br/>" +
                        "</div>"+
                    "</div>");
        var cc = $("<div class='form-group'><label class='label-control'>CC </label><input class='form-control input-sm' type='text' name='cc'/></div>" +
                 "<div class='form-group'><label class='label-control'>BCC </label><input class='form-control input-sm' type='text' name='bcc'/></div>");
        //#endregion
        //#region create style
        mailBy.children("input[name=sendby]:radio").change(function () {
            var checkVal = $(this).val();
            if (checkVal == "server") {
                mailTo.show();
            }
            else {
                mailTo.hide();
            }
        });

        cc.hide();
        mailTo.append(cc);
        mailTo.children('a').click(function () {
            if (cc.is(":visible"))
                cc.show();
            else
                cc.hide();
            return false;
        });
        formData.find('.box-body').append(attach).append(mailBy).append(mailTo);
        BootstrapDialog.show({
            title: "Mail options",
            message: formData,
            buttons: [{
                label: 'OK',
                cssClass: 'btn-success',
                icon: 'glyphicon glyphicon-ok',
                action: function (dialogRef) {
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
                    mail['sendby'] = $('input[name="sendby"]:checked', form).val();

                    if ($('input[name="mailTo"]', form).val() == "" && $('input[name="sendby"]:checked', form).val() == "server") {
                        $.EcmAlert("Mail to is required", "INFO", BootstrapDialog.TYPE_INFO);
                        return false;
                    }

                    mail['Document'] = Document;

                    if ($('input[name="sendby"]:checked', form).val() == 'client') {
                        var maildiv = $("#mail_div");
                        var divcc = $("#div_cc");
                        var divbcc = $("#div_bcc");
                        var btncc = $("#show_cc");
                        var btnbcc = $("#show_bcc");

                        divcc.hide();
                        divbcc.hide();
                        maildiv.children('input').val();
                        maildiv.find("#mail_body").val();

                        btncc.click(function () {
                            if (divcc.is(":visible")) {
                                divcc.hide();
                                $(this).empty().text("Show cc");
                            }
                            else {
                                divcc.show();
                                $(this).empty().text("Hide cc");
                            }
                        });

                        btnbcc.click(function () {
                            if (divbcc.is(":visible")) {
                                divbcc.hide()
                                $(this).empty().text("Show bcc");
                            }
                            else {
                                divbcc.show();
                                $(this).empty().text("Hide bcc");
                            }
                        });

                        $("#btn_send").click(function () {
                            $("#dialog_send_mail").ecm_loading_show();

                            mail['mailTo'] = $("#mail_div").find("#mail_to").val();

                            if ($("#mail_div").find("#mail_cc").val() != "") {
                                mail['cc'] = $("#mail_div").find("#mail_cc").val();
                            }

                            if ($("#mail_div").find("#mail_bcc").val() != "") {
                                mail['bcc'] = $("#mail_div").find("#mail_bcc").val();
                            }

                            mail['subject'] = $("#mail_div").find("#mail_subject").val();
                            mail['body'] = $("#mail_div").find("#mail_body").val();

                            JsonHelper.helper.post(URL_SendMail, JSON.stringify(mail), function (data) {
                                $.EcmAlert(data, "INFO", BootstrapDialog.TYPE_INFO);
                            }, function (jqXHR, textStatus, errorThrown) {
                                $.EcmAlert("Failed to send your email!", "INFO", BootstrapDialog.TYPE_INFO);
                            }, false);

                            $("#dialog_send_mail").ecm_loading_hide();
                            $("#dialog_send_mail").modal("hide");
                        });

                        $("#dialog_send_mail").modal("show");
                        dialogRef.close();
                    }
                    else {
                        mail['mailTo'] = $('input[name="mailTo"]', form).val();

                        if ($('input[name="cc"]', form).val() != "") {
                            mail['cc'] = $('input[name="cc"]', form).val();
                        }

                        if ($('input[name="bcc"]', form).val() != "") {
                            mail['bcc'] = $('input[name="bcc"]', form).val();
                        }
                        dialogRef.getModal().ecm_loading_show();

                        JsonHelper.helper.post(URL_SendMail, JSON.stringify(mail), function (data) {
                            dialogRef.getModal().ecm_loading_hide();
                            $.EcmAlert(data, "INFO", BootstrapDialog.TYPE_INFO);
                        }, function (jqXHR, textStatus, errorThrown) {
                            $.EcmAlert("Failed to send your email!", "INFO", BootstrapDialog.TYPE_INFO);
                            dialogRef.getModal().ecm_loading_hide();
                        },false);

                        dialogRef.close();
                    }
                }
            }, {
                label: 'Cancel',
                action: function (dialogRef) {
                    dialogRef.close();
                }
            }]
        });
        //#endregion
    }

    function createFormDownload(Document) {
        var formData = $("<div class='box box-info save_or_mail_option'>" +
                            "<div class='box-header with-border'>Save information</div>" +
                            "<div class='box-body'></div>" +
                        "</div>");

        var attach = $("<div class='form-group'><h4>Attachment File Options</h4></div>" +
                "<div class='form-group'>" +
                    "<input type='radio' name='format' value='pdf' checked='checked' id='mail_opt_pdf' class='col-md-1'/>" +
                    "<label for='mail_opt_pdf' class='label-control col-md-5'>Email attachment as PDF</label>" +
                    "<input type='radio' name='format' value='tiff'/ id='mail_opt_tiff' class='col-md-1'/>" +
                    "<label for='mail_opt_tiff' class='label-control col-md-5'>Email attachment as TIFF</label>" +
                "</div>" +
            "</div>" +
            "<div class='form-group'><h4>Page range</h4></div>" +
                "<div class='form-group'>" +
                    "<input type='radio' name='pagerange' value='all' id='mail_opt_all' checked='checked' class='col-md-1'/>" +
                    "<label for='mail_opt_all' class='label-control col-md-3'>All</label>" +
                    "<input type='radio' name='pagerange' value='pages' id='mail_opt_pages' class='col-md-1'/>" +
                    "<label for='mail_opt_pages' class='label-control col-md-3'>Pages</label>" +
                "</div>" +
                "<div class='form-group'>" +
                    "<input type='text' name='listofpage' class='form-control input-sm' />" +
                "</div>");

        formData.find('.box-body').append(attach);

        BootstrapDialog.show({
            title: 'Save option',
            message: formData,
            buttons: [{
                label: 'OK',
                cssClass: 'btn-success',
                icon: 'glyphicon glyphicon-ok',
                action: function (dialogRef) {
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
                    dialogRef.close();
                }
            },
            {
            label: 'Cancel',
            action: function (dialogRef) {
                dialogRef.close();
            }
            }]
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