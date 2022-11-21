//#########################################################
//#Copyright (C) 2013, MIA Solution. All Rights Reserved
//#
//#History:
//# DateTime         Updater         Comment
//# 7/11/2013       Triet Ho        Tao moi

//##################################################################
// Lookup content

var $lookupContent ='<div class="nav-tabs-custom padding-3" id="dialog_lookup_content">                                                                                                                 '
                     +'    <ul class="nav nav-tabs">                                                                                                                                                    '
                     +'        <li class="active" id="lookupServerInfo"><a data-toggle="tab" href="#tabs-ServerInfo">Server info</a></li>                                                               '
                     +'        <li class="disabled" id="lookupMapInfo"><a href="#tabs-MappingInfo">Mapping info</a></li>                                                                                '
                     +'        <li id="lookupCommandInfo"><a data-toggle="tab" href="#tabs-CommandInfo" id="commandInfo">Command info</a></li>                                                          '
                     +'    </ul>                                                                                                                                                                        '
                     +'    <div class="tab-content padding-3">                                                                                                                                          '
                     +'        <div class="tab-pane padding-3 fade in active" id="tabs-ServerInfo">                                                                                                     '
                     +'            <div class="row">                                                                                                                                                    '
                     +'            <div class="col-md-12 col-sm-12 padding-3">                                                                                                                          '
                     +'                <div class="col-md-6 col-sm-6 padding-3">                                                                                                                        '
                     +'                    <div class="box box-success padding-3">                                                                                                                      '
                     +'                        <div class="box-header padding-3">                                                                                                                       '
                     +'                            <h4>Connection info</h4>                                                                                                                             '
                     +'                        </div>                                                                                                                                                   '
                     +'                        <div class="box-body padding-3">                                                                                                                         '
                     +'                            <div class="form-group">                                                                                                                             '
                     +'                                <label class="label-control">Server name</label><span class="required">*</span>                                                                  '
                     +'                                <input type="text" id="server_name" class="form-control input-sm validate" placeholder="Enter server name...">                                   '
                     +'                            </div>                                                                                                                                               '
                     +'                            <div class="form-group">                                                                                                                             '
                     +'                                <label class="label-control">Database type</label><span class="required">*</span>                                                                '
                     +'                                <select id="database_type_select" class="form-control input-sm">                                                                                 '
                     +'                                    <option value="0">MS SQL Server</option>                                                                                                     '
                     +'                                    <option value="2">Oracle Database</option>                                                                                                   '
                     +'                                    <option value="4">IBM DB2</option>                                                                                                           '
                     +'                                    <option value="1">My SQL</option>                                                                                                            '
                     +'                                    <option value="3">Postgre SQL</option>                                                                                                       '
                     +'                                </select>                                                                                                                                        '
                     +'                            </div>                                                                                                                                               '
                     +'                            <div class="form-group">                                                                                                                             '
                     +'                                <label class="label-control">Data provider</label><span class="required">*</span>                                                                '
                     +'                                <select id="data_provider_select" class="form-control input-sm">                                                                                 '
                     +'                                    <option value="2">ADO.NET provider</option>                                                                                                  '
                     +'                                    <option value="1">Oledb provider</option>                                                                                                    '
                     +'                                </select>                                                                                                                                        '
                     +'                            </div>                                                                                                                                               '
                     +'                            <div class="form-group">                                                                                                                             '
                     +'                                <label class="label-control">Port number</label><span class="required">*</span>                                                                  '
                     +'                                <input type="text" id="port_number" class="form-control input-sm validate" placeholder="Enter port number..." value="1433">                      '
                     +'                            </div>                                                                                                                                               '
                     +'                        </div>                                                                                                                                                   '
                     +'                    </div>                                                                                                                                                       '
                     +'                </div>                                                                                                                                                           '
                     +'                <div class="col-md-6 col-sm-6 padding-3">                                                                                                                        '
                     +'                    <div class="box box-success padding-3">                                                                                                                      '
                     +'                        <div class="box-header padding-3">                                                                                                                       '
                     +'                            <h4>Database info</h4>                                                                                                                               '
                     +'                        </div>                                                                                                                                                   '
                     +'                        <div class="box-body padding-3">                                                                                                                         '
                     +'                            <div class="form-group">                                                                                                                             '
                     +'                                <label class="label-control">User name</label><span class="required">*</span>                                                                    '
                     +'                                <input type="text" id="user_name" class="form-control input-sm validate" placeholder="Enter user name...">                                       '
                     +'                            </div>                                                                                                                                               '
                     +'                            <div class="form-group">                                                                                                                             '
                     +'                                <label class="label-control">Password</label><span class="required">*</span>                                                                     '
                     +'                                <input type="password" id="pass_word" class="form-control input-sm validate">                                                                    '
                     +'                            </div>                                                                                                                                               '
                     +'                            <div class="form-group">                                                                                                                             '
                     +'                                <label class="label-control">Database</label><span class="required">*</span>                                                                     '
                     +'                                <select id="select_database" class="form-control input-sm">                                                                                      '
                     +'                                    <option>Empty</option>                                                                                                                       '
                     +'                                </select>                                                                                                                                        '
                     +'                            </div>                                                                                                                                               '
                     +'                            <div class="form-group">                                                                                                                             '
                     +'                                <label class="label-control">Schema</label><span class="required">*</span>                                                                       '
                     +'                                <select id="select_schema" class="form-control input-sm">                                                                                        '
                     +'                                    <option>Empty</option>                                                                                                                       '
                     +'                                </select>                                                                                                                                        '
                     +'                            </div>                                                                                                                                               '
                     +'                        </div>                                                                                                                                                   '
                     +'                    </div>                                                                                                                                                       '
                     +'                </div>                                                                                                                                                           '
                     +'                <div class="btn-group margin">                                                                                                                                   '
                     +'                    <button type="button" id="button_test_connection" class="btn bg-olive btn-sm">                                                                               '
                     +'                        <i class="glyphicon glyphicon-play"></i>                                                                                                                 '
                     +'                        <span>Test connection</span>                                                                                                                             '
                     +'                    </button>                                                                                                                                                    '
                     +'                </div>                                                                                                                                                           '
                     +'            </div></div>                                                                                                                                                         '
                     +'        </div>                                                                                                                                                                   '
                     +'        <div class="tab-pane padding-3 fade" id="tabs-MappingInfo">                                                                                                              '
                     +'            <div class="box box-success padding-3">                                                                                                                              '
                     +'                <div class="box-header with-border padding-3">                                                                                                                   '
                     +'                    <h4>Mapping info</h4>                                                                                                                                        '
                     +'                </div>                                                                                                                                                           '
                     +'                <div class="box-body padding-3">                                                                                                                                 '
                     +'                    <div class="form-group">                                                                                                                                     '
                     +'                        <label class="label-control">Select data source type</label><span class="required">*</span>                                                              '
                     +'                        <div>                                                                                                                                                    '
                     +'                            <div class="col-md-12 col-sm-12 padding-1">                                                                                                          '
                     +'                                <div class="col-md-3 col-sm-3 padding-1 pull-left">                                                                                              '
                     +'                                    <input type="radio" id="display_table" name="data_source" value="Table" checked="checked"> <label for="display_table" class="label-control"> Display table</label>'
                     +'                                </div>                                                                                                                                           '
                     +'                                <div class="col-md-3 col-sm-3 padding-1 pull-left">                                                                                              '
                     +'                                    <input type="radio" id="display_view" name="data_source" value="View"> <label class="label-control" for="display_view"> Display views</label>'
                     +'                                </div>                                                                                                                                           '
                     +'                                <div class="col-md-6 col-sm-6 padding-1 pull-left">                                                                                              '
                     +'                                    <input type="radio" id="display_procedure" name="data_source" value="StoredProcedure"> <label class="label-control" for="display_procedure"> Display stored procedures</label>'
                     +'                                </div>                                                                                                                                           '
                     +'                            </div>                                                                                                                                               '
                     +'                        </div>                                                                                                                                                   '
                     +'                    </div>                                                                                                                                                       '
                     +'                    <div class="form-group">                                                                                                                                     '
                     +'                        <label class="label-control">Data source</label><span class="required">*</span>                                                                          '
                     +'                        <select class="form-control input-sm" id="select_data_source">                                                                                           '
                     +'                            <option id="Empty">Select</option>                                                                                                                   '
                     +'                        </select>                                                                                                                                                '
                     +'                    </div>                                                                                                                                                       '
                     +'                    <div class="form-group">                                                                                                                                     '
                     +'                        <label class="label-control">Maximum number of lookup rows returned</label>                                                                              '
                     +'                        <input type="text" id="number_of_lookup_rows" class="form-control input-sm" />                                                                           '
                     +'                    </div>                                                                                                                                                       '
                     +'                    <div id="lookupMappingDetail" class="col-md-12">                                                                                                             '
                     +'                        <table id="table_look_data" class="table table-hover">                                                                                                   '
                     +'                            <thead>                                                                                                                                              '
                     +'                                <tr>                                                                                                                                             '
                     +'                                    <th>Archive client</th>                                                                                                                      '
                     +'                                    <th>Lookup data source</th>                                                                                                                  '
                     +'                                </tr>                                                                                                                                            '
                     +'                            </thead>                                                                                                                                             '
                     +'                            <tbody>                                                                                                                                              '
                     +'                            </tbody>                                                                                                                                             '
                     +'                        </table>                                                                                                                                                 '
                     +'                    </div>                                                                                                                                                       '
                     +'                </div>                                                                                                                                                           '
                     +'            </div>                                                                                                                                                               '
                     +'        </div>                                                                                                                                                                   '
                     +'        <div class="tab-pane padding-3 fade" id="tabs-CommandInfo">                                                                                                              '
                     +'            <div class="box box-success padding-3" id="commandtext">                                                                                                             '
                     +'                <div class="box-header with-border padding-1">                                                                                                                   '
                     +'                    <div class="col-md-12 col-sm-12 padding-1">                                                                                                                  '
                     +'                        <div class="col-md-5 col-sm-5 padding-1">                                                                                                                '
                     +'                            <h4>Build Sql command</h4>                                                                                                                           '
                     +'                        </div>                                                                                                                                                   '
                     +'                        <div class="col-md-7 col-sm-7 padding-1">                                                                                                                '
                     +'                            <div class="btn-group pull-right padding-1">                                                                                                         '
                     +'                                <button type="button" id="button_add_condition" class="btn bg-olive btn-sm margin-3">                                                            '
                     +'                                    <i class="glyphicon glyphicon-plus"></i>                                                                                                     '
                     +'                                    <span>Add Condition</span>                                                                                                                   '
                     +'                                </button>                                                                                                                                        '
                     +'                                <button type="button" id="button_clear_condition" class="btn bg-olive btn-sm margin-3">                                                          '
                     +'                                    <i class="glyphicon glyphicon-refresh"></i>                                                                                                  '
                     +'                                    <span>Clear Condition</span>                                                                                                                 '
                     +'                                </button>                                                                                                                                        '
                     +'                            </div>                                                                                                                                               '
                     +'                        </div>                                                                                                                                                   '
                     +'                    </div>                                                                                                                                                       '
                     +'                </div>                                                                                                                                                           '
                     +'                <div class="box-body padding-3">                                                                                                                                 '
                     +'                    <div class="form-group">                                                                                                                                     '
                     +'                        <label class="label-control">Database field</label>                                                                                                      '
                     +'                        <select id="select_database_field" class="form-control input-sm">                                                                                        '
                     +'                        </select>                                                                                                                                                '
                     +'                    </div>                                                                                                                                                       '
                     +'                    <div class="form-group">                                                                                                                                     '
                     +'                        <label class="label-control">Operator</label>                                                                                                            '
                     +'                        <select id="select_operator" class="form-control input-sm">                                                                                              '
                     +'                            <option>Select</option>                                                                                                                              '
                     +'                        </select>                                                                                                                                                '
                     +'                    </div>                                                                                                                                                       '
                     +'                </div>                                                                                                                                                           '
                     +'            </div>                                                                                                                                                               '
                     +'            <div class="box box-success padding-3" id="commandstore" style="display:none">                                                                                       '
                     +'                <div class="box-header padding-1">                                                                                                                               '
                     +'                    <div class="col-md-12 col-sm-12 padding-1">                                                                                                                  '
                     +'                        <div class="col-md-5 col-sm-5 padding-1">                                                                                                                '
                     +'                            <h4>Lookup data</h4>                                                                                                                                 '
                     +'                        </div>                                                                                                                                                   '
                     +'                        <div class="col-md-7 col-sm-7 padding-1">                                                                                                                '
                     +'                                <button type="button" id="build_execute_command" class="btn bg-olive btn-sm pull-right">                                                         '
                     +'                                    <i class="fa fa-upload"></i>                                                                                                                 '
                     +'                                    <span>Build Command</span>                                                                                                                   '
                     +'                                </button>                                                                                                                                        '
                     +'                        </div>                                                                                                                                                   '
                     +'                    </div>                                                                                                                                                       '
                     +'                </div>                                                                                                                                                           '
                     +'                <div class="box-body padding-3">                                                                                                                                 '
                     +'                    <div class="form-group">                                                                                                                                     '
                     +'                        <label class="label-control">Database field</label>                                                                                                      '
                     +'                        <select id="select_database_field_stored" class="form-control input-sm">                                                                                 '
                     +'                        </select>                                                                                                                                                '
                     +'                    </div>                                                                                                                                                       '
                     +'                    <div class="form-group">                                                                                                                                     '
                     +'                        <label class="label-control">Operator</label>                                                                                                            '
                     +'                        <select id="select_operator_stored" class="form-control input-sm">                                                                                       '
                     +'                            <option>Select</option>                                                                                                                              '
                     +'                        </select>                                                                                                                                                '
                     +'                    </div>                                                                                                                                                       '
                     +'                    <div class="padding-3">                                                                                                                                      '
                     +'                        <div id="storeparameter"></div>                                                                                                                          '
                     +'                    </div>                                                                                                                                                       '
                     +'                </div>                                                                                                                                                           '
                     +'            </div>                                                                                                                                                               '
                     +'            <div class="box box-success padding-3" id="text_sql">                                                                                                                '
                     +'                <div class="box-header padding-1">                                                                                                                               '
                     +'                    <div class="col-md-12 col-sm-12 padding-1">                                                                                                                  '
                     +'                        <div class="col-md-5 col-sm-5 padding-1">                                                                                                                '
                     +'                            <h4>Sql command</h4>                                                                                                                                 '
                     +'                        </div>                                                                                                                                                   '
                     +'                        <div class="col-md-7 col-sm-7 padding-1">                                                                                                                '
                     +'                            <button type="button" id="test_lookup_data" class="btn bg-olive btn-sm pull-right">                                                                  '
                     +'                                <i class="fa fa-upload"></i>                                                                                                                     '
                     +'                                <span>Test SQL</span>                                                                                                                            '
                     +'                            </button>                                                                                                                                            '
                     +'                        </div>                                                                                                                                                   '
                     +'                    </div>                                                                                                                                                       '
                     +'                </div>                                                                                                                                                           '
                     +'                <div class="box-body padding-3">                                                                                                                                 '
                     +'                    <div class="form-group">                                                                                                                                     '
                     +'                        <textarea name="text_area_condition" id="sql_command" cols="100" rows="50" style="width:100%; height:70px; overflow:auto"></textarea>                    '
                     +'                    </div>                                                                                                                                                       '
                     +'                </div>                                                                                                                                                           '
                     +'            </div>                                                                                                                                                               '
                     +'            <div class="box box-success padding-3">                                                                                                                              '
                     +'                <div class="box-header padding-1">                                                                                                                               '
                     +'                    <h4>Test lookup data results</h4>                                                                                                                            '
                     +'                </div>                                                                                                                                                           '
                     +'                <div class="box-body padding-3">                                                                                                                                 '
                     +'                    <div id="lookup_data_result" class="col-md-12 col-sm-12"></div>                                                                                              '
                     +'                </div>                                                                                                                                                           '
                     +'            </div>                                                                                                                                                               '
                     +'        </div>                                                                                                                                                                   '
                     +'    </div>                                                                                                                                                                       '
                     +'</div>                                                                                                                                                                           ';

//
var EmptyId = "00000000-0000-0000-0000-000000000000";
var _DocumentID = -1;
var _FieldID = -1;
var json; // json string store content type
var json_delete_fields = { DeletedFields: [] };
         json_delete_fields.DeletedFields.push({ Id: -1 });
var isEdit = -1; //  1: Edit, -1: Add new
var isEditContentType = -1;//Edit: 1; Add new: -1
var picklistvalue="";
var json_pick_value;
var str_pick_value = "";

var table_field_name;
var table_field_datatype;
var table_field_maxlength;
var table_field_default_value;
var table_field_use_current_date;
var table_field_required;
var table_field_restricted;
var json_table_fields = {};
var json_table_value;
var tableName; // Store fiel with table type when edit
var ConnectString;
var dataProvider;
var ListDatabaseName = new Array();
var ListSchemas = new Array();
var DatabaseTable;
var DatabaseType;
var PortNumber;
var Schema;
var DatabaseName;
var SourceName;
var ColumnName;
var ServerName;
var UserName;
var Password;
var LookupDataType;
var $LookupRow;
var iconPath;
var keyCacheIcon;
var contenttype_id;
//Loc Ngo
var $contenttype_fields = [];
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

var testLookupConnectionSuccess = false;
var isEditLookup = false;
var draws = {};
var thumbs = {};
var $selectedPage;

var insOption;

var Options = {
    Import: 0,
    InsertBefore: 1,
    InsertAfter: 2,
    Replace: 3,
    Exist: 4
};

var fieldMetaDatas = {};
var $documentViewer;
var $thumbnailViewer;
var $docViewerLoading;
//OCR Template
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
                return;
            }
            if (data.IsTimeOut)
                window.location = URL_Login;
            else if (data.IsError) {
                $.EcmAlert(data.Message, "WARNNING", BootstrapDialog.TYPE_WARNING);
                return;
            }
            else {
                fieldMetaDatas = data;
                $.each(fieldMetaDatas, function () {
                    _fields.push(this.toString());
                });
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
        }
    });

    var count = 0;

    $.each(data, function (i, v) {
        var imageUrl = URL_LoadImage + "?key=" + this.KeyCache;
        var pageId = "page_" + this.KeyCache;
        var $page = $("<div class='page' data-page-index='" + i + "' id='" + pageId + "' data-key='" + this.KeyCache + "' data-extension='" + this.FileExtension + "'></div>");
        $docViewerLoading.append($page);
        draws[this.KeyCache] = $page.annotationCapture({ image: imageUrl, angle: this.RotateAngle });
        $page.find('img').load(function () {
            count++;
            if (count == data.length) {
                $docViewerLoading.children().appendTo($documentViewer);
            }
        });

        $page.click(pageElementClick);

        draws[this.KeyCache].setFields(_fields);
        draws[this.KeyCache].annoOcrZone();
        if (data[i].ocrZones)
            draws[this.KeyCache].createAnnotations(data[i].ocrZones);
    });

    $(".ocr_template_tool").removeAttr("style");
    $documentViewer.ecm_loading_hide();
}

function createThumbnail(data) {
    $.each(data, function (i, v) {
        var imageUrl = URL_LoadImage + "?key=" + this.KeyCache + "&thumb=true";
        var selectedClass = 'page treeview_title';

        if (i == 0) {
            selectedClass += ' treeview_select';
        }

        var $item = $('<li class="connectedSortable">'
                + '<span class="' + selectedClass + '" id="' + this.KeyCache + '">'
                + '<a href="#"><img src="' + imageUrl + '" /><span>'
                + '<strong class="pageNumber">' + (i + 1) + '</strong>'
                + '<span> ' + this.Resolution + ' dpi</span>'
                + '</span></a></span></li><input type="hidden" class="viewer" value="' + this.FileType + '"/>');

        $item.click(pageClick);

        $item.sortable({
            revert: true,
            opacity: 0.5,
            placeholder: "ui-state-highlight",
            receive: function (e, ui) {
            },
            update: function (e, ui) {
            },
            start: function (e, ui) {
                $recyclebin.droppable({
                    drop: function (event, u) {
                        $recyclebin.remove();
                        $folder.sortable("option", "revert", false);
                        deletePage(u.draggable);
                        $folder.sortable("option", "revert", true);
                    },
                    over: function (event, u) {
                        $recyclebin.css('opacity', '0.5');
                    },
                    out: function (event, u) {
                        $recyclebin.css('opacity', '0.1');
                    }
                })
            },
            stop: function () {
            },
            connectWith: ".connectedSortable",
            dropOnEmpty: true
        }).disableSelection();

        thumbs[this.KeyCache] = $item.thumbnail(this.RotateAngle);
        $thumbnailViewer.find("div.ocr_pages").append($item);
    });

    //$thumbnailViewer.find("div.ocr_pages").append($tree);

    $thumbnailViewer.ecm_loading_hide();
}

function createThumbnailItem(data, i) {
        var imageUrl = URL_LoadImage + "?key=" + data.KeyCache + "&thumb=true";
        var selectedClass = 'page treeview_title';

        var $item = $('<li class="connectedSortable">'
                + '<span class="' + selectedClass + '" id="' + data.KeyCache + '">'
                + '<a href="#"><img src="' + imageUrl + '" /><span>'
                + '<strong class="pageNumber">' + (i + 1) + '</strong>'
                + '<span> ' + data.Resolution + ' dpi</span>'
                + '</span></a></span></li><input type="hidden" class="viewer" value="' + data.FileType + '"/>');

        $item.click(pageClick);

        $item.sortable({
            revert: true,
            opacity: 0.5,
            placeholder: "ui-state-highlight",
            receive: function (e, ui) {
            },
            update: function (e, ui) {
            },
            start: function (e, ui) {
                $recyclebin.droppable({
                    drop: function (event, u) {
                        $recyclebin.remove();
                        $folder.sortable("option", "revert", false);
                        deletePage(u.draggable);
                        $folder.sortable("option", "revert", true);
                    },
                    over: function (event, u) {
                        $recyclebin.css('opacity', '0.5');
                    },
                    out: function (event, u) {
                        $recyclebin.css('opacity', '0.1');
                    }
                })
            },
            stop: function () {
            },
            connectWith: ".connectedSortable",
            dropOnEmpty: true
        }).disableSelection();

        thumbs[this.KeyCache] = $item.thumbnail(data.RotateAngle);
        $thumbnailViewer.find("div.ocr_pages").append($item);

    //$thumbnailViewer.ecm_loading_hide();
    return $item;
}

function createPageItem(data, i) {
    var imageUrl = URL_LoadImage + "?key=" + data.KeyCache;
    var pageId = "page_" + data.KeyCache;
    var $page = $("<div class='page' data-page-index='" + i + "' id='" + pageId + "' data-key='" + data.KeyCache + "'></div>");
    //$docViewerLoading.append($page);

    //draws[data.KeyCache] = $page.annotationCapture({ image: imageUrl, angle: data.RotateAngle });
    $page.find('img').load(function () {
        count++;
        if (count == data.length) {
            $docViewerLoading.children().appendTo($documentViewer);
        }
    });

    $page.click(pageElementClick);

    //draws[data.KeyCache].setFields(_fields);
    //raws[data.KeyCache].annoOcrZone();

    return $page;
}

function importTemplate(data, opt) {
    switch (opt) {
        case Options.Import: {
            createPage(data);
            createThumbnail(data);
            break;
        }
        case Options.InsertAfter: {
            updatePage(data, Options.InsertAfter, $selectedPage);
            break;
        }
        case Options.InsertBefore: {
            updatePage(data, Options.InsertBefore, $selectedPage);
            break;
        }
    }
}

function updatePage(data, opt, targetThumb) {
    var $targetPage = $("#page_" + targetThumb.find(".page").attr("id"));
    var _fields = [];
    $.ajax({
        url: URL_GetFieldMetaData,
        async: false,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ docTypeId: $('.doc_type_id').val() }),
        success: function (data) {
            if (typeof data == "string") {
                return;
            }
            if (data.IsTimeOut)
                window.location = URL_Login;
            else if (data.IsError) {
                $.EcmAlert(data.Message, "WARNNING", BootstrapDialog.TYPE_WARNING);
                return;
            }
            else {
                fieldMetaDatas = data;
                $.each(fieldMetaDatas, function () {
                    _fields.push(this.toString());
                });
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
        }
    });

    $.each(data, function (i) {
        var $item = createThumbnailItem(this, i);
        //var $pageItem = createPageItem(this, i);
        //createPage(data);
        var imageUrl = URL_LoadImage + "?key=" + this.KeyCache;
        var pageId = "page_" + this.KeyCache;
        var $page = $("<div class='page' data-page-index='" + i + "' id='" + pageId + "' data-key='" + this.KeyCache + "' data-extension='" + this.FileExtension + "'></div>");
        //$docViewerLoading.append($page);


        switch (opt) {
            case Options.InsertAfter: {
                $item.insertAfter(targetThumb);
                $page.insertAfter($targetPage);
                break;
            }
            case Options.InsertBefore: {
                $item.insertBefore(targetThumb);
                $page.insertBefore($targetPage);
            }
            case Options.Replace: {
            }
        }

        draws[this.KeyCache] = $page.annotationCapture({ image: imageUrl, angle: '0' });
        draws[this.KeyCache].setFields(_fields);
        $targetPage = $page;

        //$page.find('img').load(function () {
        //    count++;
        //    if (count == data.length) {
        //        //$docViewerLoading.children().appendTo($documentViewer);
        //    }
        //});

        
    });
    $targetPage.click(pageElementClick);
}

//End OCR template

$(function () {

var current_field; // Store row_field to execute delete action

// Display content type list
function ShowListContentType() {

    ///Get Ajax
    $(".admin_sub_menu").ecm_loading_show();

    JsonHelper.helper.post(
        URL_ShowListContentType,
        JSON.stringify(""),
        ShowListContentType_Success,
        ShowListContentType_Error, true);
}
function ShowListContentType_Success(data) {
    $(".sub_properties_list").remove();
    $(".admin_sub_menu").append(data);
    $(".content_sub_menu_item").first().find(".content_type_item").click();
    $(".admin_sub_menu").ecm_loading_hide();

}

function ShowListContentType_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".admin_sub_menu").ecm_loading_hide();
}
// ================ Kết thúc ShowListContentType ===================== // 
// Display, Add content properties
function ContentTypeProperties() {

    ///Get Ajax
    $(".sub_properties").ecm_loading_show();

    JsonHelper.helper.post(
        URL_ContentTypeProperties,
        JSON.stringify({DocumentId:_DocumentID}),
        ContentTypeProperties_Success,
        ContentTypeProperties_Error,true);
}
function ContentTypeProperties_Success(data) {
    $(".sub_properties").find(".sub_properties_content").remove();
    $(".sub_properties").append(data);
    if (_DocumentID != -1)
    {
        GetTableValue2();
    }
    var table = $('#table_fields').dataTable(
        {
            "sScrollY": "250px",
            "sScrollYInner": "220px",
            "bScrollCollapse": false,
            "bPaginate": false,
            "bFilter": false,
            "bSearchable":false,
            "bInfo": false,
            "bDestroy": true,
            "sPaginationType": "bootstrap",
            "bSort": false,
            "aoColumns": [
                  { "sWidth": "25%" },
                  null,
                  null,
                  null,
                  null, null, null, null, null,null
            ]
        }
    );

    $(".sub_properties").ecm_loading_hide();
}

function ContentTypeProperties_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();
}
// ============== End ContentTypeProperties =======//

// ===============  Delete content type ============
function DeleteContentType() {
    ///Get Ajax
    JsonHelper.helper.post(
        URL_DeleteContentType,
        JSON.stringify({ DocumentId: _DocumentID }),
        DeleteContentType_Success,
        DeleteContentType_Error);
}
function DeleteContentType_Success(data) {
    $(".sub_properties_content").remove();
    $(".submenu-list").find(".content_sub_menu_item.active").remove();
    $(".content_sub_menu_item").first().find(".content_type_item").click();
}

function DeleteContentType_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();
}
// ============== End Delete content =======//
    
//================ reset dialog  add field==================//
function ResetFieldInDialog()
{
    $("#field_name").val("");
    // $("input:checked").removeAttr("checked");
    $("#required:checked").removeAttr("checked");
    $("#restricted:checked").removeAttr("checked");
    $("#use_current_date").removeAttr("checked");
    $("#dataType").prop('selectedIndex', 0);
    $("#dataType").change();
    //$("#max_length").val("0");
    $("#default_value").val("");
    $("#configure_picklist").hide();
    $("#current_date").hide();
    $("#select_true_false").hide();
    $("#configure_table").hide();
    $("#default_value").prop('disabled', false);
    $("#default_value_title").show();
    $("#max_length").prop('disabled', false);
} // End reset dialog 

// ===============  save content type ============
function SaveContentType() {
    ///Get Ajax
    $(".sub_properties").ecm_loading_show();
    json.Name = (json.Name + "").replace(/\s+/g, ' ').trim();
    JsonHelper.helper.post(
        URL_SaveContentType,
        JSON.stringify({ doctypemodel: json, picklist: picklistvalue, keyCacheIcon: keyCacheIcon }),
        SaveContentType_Success,
        SaveContentType_Error);
}

function SaveContentType_Success(data) {
    $(".sub_properties").ecm_loading_hide();
    var Id = data.Id;
    var errorMessages = data.ErrorMessages;

    if (Id != EmptyId) {
        if (isEditContentType == -1) {
            $(".sub_properties_list").ecm_loading_show();
            var new_contentype_menu_item =
                    '<div class="info-box content_sub_menu_item hand" id="' + Id + '">' +
                        '<span class="info-box-icon bg-olive content_type_item"><i class="fa fa-file-o" style="padding-top:20px"></i></span>' +
                        '<div class="info-box-content">' +
                            '<div class="info-box-text hastooltip"><h5><b>' + json.Name + '</b></h5></div>' +
                            '<div class="info-box-text">' +
                                '<div class="col-sm-12 no-padding">' +
                                    '<div class="col-sm-8 no-padding"> OCR Template </div>' +
                                    '<div class="col-sm-2 no-padding"><a href="@Url.Action("Configure", "ArchiveAdmin")?id="' + Id + '""><i class="fa fa-cog hand"></i></a></div>' +
                                    '<div class="col-sm-2 no-padding"></div>' +
                                '</div>' +
                            '</div>' +
                            '<div class="info-box-text">' +
                                '<div class="col-sm-12 no-padding">' +
                                    '<div class="col-sm-8 no-padding">Barcode</div>' +
                                    '<div class="col-sm-2 no-padding"><i class="fa fa-barcode hand barcode_config"  data-id="' + Id + '"></i></div>' +
                                    '<div class="col-sm-2 no-padding"></div>' +
                                '</div>' +
                            '</div>' +
                        '</div>' +
                    '</div>';

            $(".submenu-list").find(".content_sub_menu_item").removeClass("active");
            $(".submenu-list").append(new_contentype_menu_item);
            $(".sub_properties_list").ecm_loading_hide();
            $(".submenu-list").find("#" + Id).find(".content_type_item").click();
        } else {
            var $this_chage = $(".submenu-list").find(".content_sub_menu_item.active");
            $this_chage.find(".content-type-name > h5 > b").text(json.Name);
            $this_chage.find(".content-type-name").attr("title", json.Name);
            $this_chage.find(".content_type_item").click();
        }
    } else {
        $.each(errorMessages, function (i, item) {
            if (item.FieldName == "Name") {
                $("#content_name").parent().addClass("has-error");
                $("#content_name").prop("title", item.Error);
            }
        });
    }
}

function SaveContentType_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();

}
// ==============End save content =======//


// ===============  Use to GetPicklistValue type ============
function GetPicklistValue() {

    ///Get Ajax
    $(".sub_properties").ecm_loading_show();

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
// ============== End GetPicklistValue =======//

// ============== User to get values of  fields table =========
function GetTableValue() {

    ///Get Ajax
    $(".sub_properties").ecm_loading_show();

    JsonHelper.helper.post(
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
// ============== End GetTableValue =======//
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

    JsonHelper.helper.post(
        URL_LoadTableConfigure,
        JSON.stringify({ DocTypeID: _DocumentID, FieldID: _FieldID }),
        LoadTableConfigure_Success,
        LoadTableConfigure_Error);
}
function LoadTableConfigure_Success(data) {

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
        var outlook = $("#outlook_import").is(":checked");

        var DisplayOrder = 1;
        json = { "Name": contenttypename, "Id": _DocumentID, "IsOutlook": outlook, Fields: [], IsOutlook: outlook, DeletedFields: [] };
        $("#table_fields").find('tbody tr').each(function (index) {
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
            var td_maxlength = $(this).find(".td_maxlength").text();
            var td_picklist = $(this).find(".td_picklist").text();
            if (td_data_type == "Picklist") {
                picklistvalue = picklistvalue + td_picklist + "#";
            }
            //json.SearchQueryExpressions.push({ Id: ExpressionID, SearchQueryId: QueryId, Condition: Condition, Field: { Id: FieldID, FieldUniqueId: FieldUniqueId }, OperatorText: OperatorText, Operator: Operator, Value1: value1, Value2: value2, FieldUniqueId: FieldUniqueId });
            var lpInfo;//getLookupInfo(id_field);

            if (lookupInfos != undefined) {
                for (var i = 0; i < lookupInfos.length; i++) {
                    if (lookupInfos[i].FieldId == id_field) {
                        lpInfo = lookupInfos[i];
                        break;
                    }
                }
            }


            json.Fields.push({ DocTypeId: _DocumentID, Id: id_field, Name: td_field_name, DataType: td_data_type, IsRequired: td_required, IsRestricted: td_restricted, MaxLength: td_maxlength, DefaultValue: td_default_value, DisplayOrder: DisplayOrder, UseCurrentDate: use_current_date, Children: json_table_fields[td_field_name], IsLookup: lpInfo != undefined, LookupInfo: lpInfo });
            DisplayOrder++;

        });
        //Icon value
        keyCacheIcon = $("#image_icon").data("keycache");

        // nếu thêm outlook được check thì thêm vào các field sau 
        if (outlook && isEditContentType != 1) {
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
    } else {
        $.EcmAlert("Content type has least one field!", "WARNNING", BootstrapDialog.TYPE_WARNING);
    }
}
    // append first row of table configure 
function AppendFirstRowTableConfigure()
{
    $("#table_configuration").append("<tr class='table_configure_row first_row' id='-1'>"
                                            + "<td id='first_delete_column'><i class='dialog_table_icon glyphicon glyphicon-trash large hand'></i></td>"
                                            + "<td class='table_td_name'><input type='text' class='form-control input-sm table_name_value' placeholder='New column'></td>"
                                            + "<td class='table_td_type'>"
                                                + "<select  class='form-control input-sm table_data_type'>"
                                                   + "<option value='String' selected>String</option>"
                                                   + "<option value='Integer'>Integer</option>"
                                                   + "<option value='Decimal'>Decimal</option>"
                                                   + "<option value='Date'>Date</option>"
                                               + "</select>"
                                           + "</td>"
                                           + "<td class='table_td_maxlength'><input type='text' class='form-control input-sm table_maxlength allownumericwithoutdecimal' value='0'></td>"
                                             + "<td class='table_td_required'><input type='checkbox' class='table_check_required'/></td>"
                                             + "<td class='table_td_restricted'><input type='checkbox' class='table_check_restricted'/></td>"
                                           + "<td class='table_td_default_value'>"
                                               + "<input type='text' class='form-control input-sm table_default_value'>"
                                               + "<input type='checkbox' class='table_check_use_current_date' value='false' style='display:none;'><a class='table_check_use_current_date' style='display:none;'>Use current date</a>"
                                           + "</td>"
                                         + "</tr>");
}
function ChangeTableDataType(DataType) {
    $("tr.row_selected").find(".table_td_default_value").removeClass("allownumericwithdecimal");
    $("tr.row_selected").find(".table_td_default_value").removeClass("allownumericwithoutdecimal");

    switch (DataType) {
        case "String":
            {
             
                $("tr.row_selected").find(".table_td_default_value").find(".table_default_value").removeAttr("style").show();
                $("tr.row_selected").find(".table_td_default_value").find(".table_check_use_current_date").hide();
                $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").prop('disabled', false);
                $("tr.row_selected").find(".table_td_default_value").find(".table_default_value").val("");
                break;
            }
        case "Decimal":
            {
                $("tr.row_selected").find(".table_td_default_value").find(".table_default_value").removeAttr("style").show();
                $("tr.row_selected").find(".table_td_default_value").addClass("allownumericwithdecimal");
                $("tr.row_selected").find(".table_td_default_value").find(".table_check_use_current_date").hide();
                $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").prop('disabled', true);
                $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").val("0");
                $("tr.row_selected").find(".table_td_default_value").find(".table_default_value").val("0");
                break;
            }
        case "Integer":
            {
                $("tr.row_selected").find(".table_td_default_value").find(".table_default_value").removeAttr("style").show();
                $("tr.row_selected").find(".table_td_default_value").addClass("allownumericwithoutdecimal");
                $("tr.row_selected").find(".table_td_default_value").find(".table_check_use_current_date").hide();
                $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").prop('disabled', true);
                $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").val("0");
                $("tr.row_selected").find(".table_td_default_value").find(".table_default_value").val("0");
                break;
            }

        case "Date":
            {
                $("tr.row_selected").find(".table_td_default_value").find(".table_default_value").hide();
                $("tr.row_selected").find(".table_td_default_value").find(".table_check_use_current_date").removeAttr("style").show();
                $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").prop('disabled', true);
                $("tr.row_selected").find(".table_td_maxlength").find(".table_maxlength").val("0");
                $("tr.row_selected").find(".table_td_default_value").find(".table_default_value").val("");
                break;
            }
    }
};
    //sự kiện khi thay đổi datatype 
function ChangeDataType()
{
    var datatype = $("#dataType").val();
    $("#default_value").removeClass("allownumericwithdecimal");
    $("#default_value").removeClass("allownumericwithoutdecimal");

    switch (datatype) {
        case "String":
            {
                $("#configure_picklist").hide();
                $(".current_date").hide();
                $("#select_true_false").hide();
                $("#configure_table").hide();
                $("#default_value").show();
                $("#default_value").val("");
                $("#default_value").prop('disabled', false);
                $("#max_length").prop('disabled', false);
                if ($("#max_length").val() == "") {
                    $("#max_length").val("10");
                }
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
                $("#default_value").val("0");
                $("#default_value").addClass("allownumericwithdecimal");
                $("#max_length").val("");
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
                $("#default_value").val("0");
                $("#default_value").addClass("allownumericwithoutdecimal");
                $("#max_length").prop('disabled', true);
                $("#max_length").val("");
                break;
            }
        case "Picklist":
            {
                $("#configure_picklist").show();
                $(".current_date").hide();
                $("#select_true_false").hide();
                $("#configure_table").hide();
                $("#default_value").val("");
                $("#default_value").show();
                $("#default_value").prop('disabled', true);
                $("#max_length").prop('disabled', true);
                $("#max_length").val("");
                break;
            }
        case "Boolean":
            {
                $("#configure_picklist").hide();
                $(".current_date").hide();
                $("#select_true_false").show();
                $("#configure_table").hide();
                $("#default_value").hide();
                $("#default_value").val("");
                $("#max_length").prop('disabled', true);
                $("#max_length").val("");
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
                $("#max_length").val("");
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
                $("#default_value").val("");
                $("#max_length").prop('disabled', true);
                $("#max_length").val("");
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
                $("#default_value").val("");
                $("#default_value").prop('disabled', true);
                $("#max_length").prop('disabled', true);
                $("#max_length").val("");
            }

    }
}
// phần xử lý cho phần lookup
// tab in lookup dialog
$(function () {
    //$("#dialog_lookup_content").tabs();
});
// ===============  Dùng để  Test connection  ============
function TestConnection() {

    ///Get Ajax
    $(".sub_properties").ecm_loading_show();

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
    // ============== Ket test connection =======// 
// test connection 
function TestConnection2(connectionInfo) {
    $.ajax({
        url: URL_TestConnection,
        type: "POST",
        async: true,
        data: JSON.stringify(connectionInfo),
        //data: JSON.stringify({ connectionString: ConnectString, dataProviderString: dataProvider }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.Testconnectionfail == "false") {
                $.EcmAlert("Connect fail", "Information", BootstrapDialog.TYPE_INFO);
            } else {
                ListDatabaseName = data['DatabaseName'];
                ListSchemas = data['SchemaName'];
                lookupInfo.ConnectionInfo = connectionInfo;

                $("#select_database").find("option").remove();

                for (var i = 0; i < ListDatabaseName.length; i++) {
                    var option = "<option>" + ListDatabaseName[i] + "</option>";

                    if (isEditLookup && ListDatabaseName[i] == lookupInfo.ConnectionInfo.DatabaseName) {
                        option = "<option selected>" + ListDatabaseName[i] + "</option>";
                    }

                    $("#select_database").append(option);
                }

                connectionInfo['DatabaseName'] = $("#select_database").val();

                $("#select_schema").find("option").remove();

                for (var i = 0; i < ListSchemas.length; i++) {
                    var option = "<option>" + ListSchemas[i] + "</option>";

                    if (isEditLookup && ListSchemas[i] == lookupInfo.ConnectionInfo.Schema) {
                        option = "<option selected>" + ListSchemas[i] + "</option>";
                    }

                    $("#select_schema").append(option);
                }

                connectionInfo['Schema'] = $("#select_schema").val();

                $("#lookupMapInfo").removeClass('disabled');
                $("#lookupMapInfo > a").attr("data-toggle", "tab");

                $.EcmAlert("Test connection successfully", "Information", BootstrapDialog.TYPE_INFO);

                testLookupConnectionSuccess = true;
                if (lookupInfo == undefined) {
                    lookupInfo = {
                        ConnectionInfo: connectionInfo,
                        FieldId: _FieldID,
                        LookupType: LookupDataType,
                        SqlCommand: '',
                        MaxLookupRow: '',
                        MinPrefixLength: '',
                        SourceName: '',
                        LookupColumn: '',
                        LookupOperator: '',
                        Parameters: {},
                        FieldMappings: {}
                    };
                }
                else {
                    lookupInfo['ConnectionInfo'] = connectionInfo;
                }

                GetDataSource(LookupDataType);

                $('#select_data_source').change();
            }
        }
    });
}
// Lấy các bảng dữ liệu trong databse
function GetDataSource(lookupType)
{
    $.ajax({
        url: URL_GetDataSource,
        type: "POST",
        async: true,
        data: JSON.stringify({ ConnectionInfo: lookupInfo.ConnectionInfo, LookupType: lookupType }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            DatabaseTable = data;
            $("#select_data_source").find("option").remove();
            $("#select_data_source").append("<option>Select</option>");
            for (var i = 0; i < DatabaseTable.DataSources.length; i++) {
                var option = "<option>" + DatabaseTable.DataSources[i] + "</option>";

                if (isEditLookup && lookupInfo.SourceName == DatabaseTable.DataSources[i]) {
                    option = "<option selected>" + DatabaseTable.DataSources[i] + "</option>";
                }

                $("#select_data_source").append(option);
            }
        }
    });
}
// lấy các column trong database table
function GetColumns() {
    $.ajax({
        url: URL_GetColumns,
        type: "POST",
        async: true,
        data: JSON.stringify({ ConnectionInfo: lookupInfo['ConnectionInfo'], SourceName: lookupInfo['SourceName'], LookupType: LookupDataType }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {

            ColumnName = data;
            $(".select_column").find("option").remove();
            $(".select_column").append("<option>Select</option>");

            $("#select_database_field").find("option").remove();
            $("#select_database_field").append("<option>Select</option>");

            $("#select_database_field_stored").find("option").remove();
            $("#select_database_field_stored").append("<option>Select</option>");

            for (var i = 0; i < ColumnName.LookupColumn.length; i++) {
                var fieldOption = "<option>" + ColumnName.LookupColumn[i].Key + "</option>";
                var colOption = "<option>" + ColumnName.LookupColumn[i].Key + "</option>";

                if (isEditLookup && ColumnName.LookupColumn[i].Key == lookupInfo.LookupColumn) {
                    fieldOption = "<option selected>" + ColumnName.LookupColumn[i].Key + "</option>";
                }

                $(".select_column").append(colOption);
                $("#select_database_field").append(fieldOption);
                $("#select_database_field_stored").append(fieldOption);
            }

            if (isEditLookup) {
                var mappingTable = $("#table_look_data");
                var mappingRows = mappingTable.find('.lookup_data_row');
                $.each(mappingRows, function (i, item) {
                    var fieldName = $(item).find('.field_name').text();
                    var selectColumn = $(item).find('.select_column');

                    for (var j = 0; j < lookupInfo.FieldMappings.length; j++) {
                        if (fieldName == lookupInfo.FieldMappings[j].Name && lookupInfo.FieldMappings[j].DataColumn != '') {
                            selectColumn.val(lookupInfo.FieldMappings[j].DataColumn);
                        }
                    }
                });

                $('#select_database_field').change();

            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
        }
    });
}

function GetOperators(fieldType) {
    $.ajax({
        url: URL_GetOperators,
        type: "POST",
        async: true,
        data: JSON.stringify({DataType:fieldType}),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {

            $("#select_operator").find("option").remove();
            $("#select_operator").append("<option>Select</option>");
            for (var i = 0; i < data.length; i++) {
                var option = "<option>" + data[i] + "</option>";

                if (lookupInfo != undefined && lookupInfo.LookupOperator != undefined && lookupInfo.LookupOperator == data[i]) {
                    option = "<option selected>" + data[i] + "</option>";
                }

                $("#select_operator").append(option);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
        }
    });
}

function GetOperatorsFromStored(fieldType) {
    $.ajax({
        url: URL_GetOperatorsFormStored,
        type: "POST",
        async: true,
        data: JSON.stringify({DataType:fieldType}),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {

            $("#select_operator_stored").find("option").remove();
            $("#select_operator_stored").append("<option>Select</option>");
            for (var i = 0; i < data.length; i++) {
                $("#select_operator_stored").append("<option>" + data[i] + "</option>");
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
        }
    });
}

function GetParameters(storeName) {
    JsonHelper.helper.post(
        URL_GetParameters,
        JSON.stringify({ ConnectionInfo: lookupInfo['ConnectionInfo'], StoredName: storeName }),
        LoadParameterSuccess,
        LoadParameterError);
}

function LoadParameterSuccess(data) {
    $("#storeparameter").empty();
    $("#storeparameter").append(data);
}

function LoadParameterError(jqXHR, textStatus, errorThrown) {
    $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
}

function BuildCommandText() {
    var lookupColumn = $('#select_database_field').val();
    var operator = $('#select_operator').val();

    if (lookupColumn != 'Select' && operator != 'Select') {
        AddWhereClause();
    }
    else {
        $.ajax({
            url: URL_BuildCommandText,
            type: "POST",
            async: true,
            data: JSON.stringify({ LookupInfo: lookupInfo }),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                lookupInfo['SqlCommand'] = data;
                $('#sql_command').val(data);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
            }
        });
    }
}

function BuildExecuteCommand() {
    var lookupColumn = $('#select_database_field_stored').val();
    var columnDataType = '';
    var operator = $('#select_operator_stored').val();

    $.ajax({
        url: URL_BuildExecuteCommand,
        type: "POST",
        async: true,
        data: JSON.stringify({ LookupInfo: lookupInfo }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            lookupInfo.SqlCommand = data;
            $('#sql_command').val(data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
        }
    });
}

function AddWhereClause() {
    var lookupColumn = $('#select_database_field').val();
    var columnDataType = '';
    var operator = $('#select_operator').val();

    $.each(ColumnName.LookupColumn, function (i, item) {
        if (item.Key == lookupColumn) {
            columnDataType = item.Value;
        }
    });

    $.ajax({
        url: URL_BuildWhereClause,
        type: "POST",
        async: true,
        data: JSON.stringify({ LookupInfo: lookupInfo, LookupColumn: lookupColumn, LookupDataType: columnDataType, OperatorText: operator }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            lookupInfo['SqlCommand'] = data;
            $('#sql_command').val(data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
        }
    });
}

function ShowLookupData(text) {
    JsonHelper.helper.post(
        URL_GetLookupData,
        JSON.stringify({ LookupInfo: lookupInfo, Text: text }),
        LoadLookupTestSuccess,
        LoadLookupTestError);
}

function LoadLookupTestSuccess(data) {
    var resultContent = $('#lookup_data_result');
    resultContent.empty();
    resultContent.append(data);
    if ($(data).find('tbody').find('tr').length > 0) {
        $('#test_lookup_data_table').dataTable(
            {
                "sScrollY": "100px",
                "sScrollYInner": "80px",
                "bScrollCollapse": false,
                "bPaginate": false,
                "bFilter": false,
                "bSearchable": false,
                "bInfo": false,
                "bDestroy": true,
                "sPaginationType": "bootstrap",
                "bSort": false,
            }
        );
    }
}

function LoadLookupTestError(jqXHR, textStatus, errorThrown) {
    $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
}

function LoadLookupInfo() {
    var $dialogContent = $("#dialog_lookup");
    var lookupContent = $dialogContent.find('#dialog_lookup_content');
    $(lookupContent).find('#server_name').val(lookupInfo.ConnectionInfo.Host);
    //Set default database type
    $("#database_type_select option").filter(function () {
        return $(this).val() == lookupInfo.ConnectionInfo.DatabaseType;
    }).attr('selected', true);
    //Set default provider
    $("#data_provider_select option").filter(function () {
        return $(this).val() == lookupInfo.ConnectionInfo.ProviderType;
    }).attr('selected', true);
    //Set port number
    $(lookupContent).find('#port_number').val(lookupInfo.ConnectionInfo.Port);
    //Set username password
    $(lookupContent).find('#user_name').val(lookupInfo.ConnectionInfo.Username);
    $(lookupContent).find('#pass_word').val(lookupInfo.ConnectionInfo.Password);


    //TestConnection2(lookupInfo.ConnectionInfo);
    ListDatabaseName = lookupInfo['DatabaseNames'];
    ListSchemas = lookupInfo['Schemas'];

    $("#select_database").find("option").remove();

    for (var i = 0; i < ListDatabaseName.length; i++) {
        var option = "<option>" + ListDatabaseName[i] + "</option>";

        if (isEditLookup && ListDatabaseName[i] == lookupInfo.ConnectionInfo.DatabaseName) {
            option = "<option selected>" + ListDatabaseName[i] + "</option>";
        }

        $("#select_database").append(option);
    }

    connectionInfo['DatabaseName'] = $("#select_database").val();

    $("#select_schema").find("option").remove();

    for (var i = 0; i < ListSchemas.length; i++) {
        var option = "<option>" + ListSchemas[i] + "</option>";

        if (isEditLookup && ListSchemas[i] == lookupInfo.ConnectionInfo.Schema) {
            option = "<option selected>" + ListSchemas[i] + "</option>";
        }

        $("#select_schema").append(option);
    }

    connectionInfo['Schema'] = $("#select_schema").val();

    $("#lookupMapInfo").removeClass('disabled');
    $("#lookupMapInfo > a").attr("data-toggle", "tab");

    //GetDataSource(LookupDataType);

    DatabaseTable = lookupInfo.Datasources;
    $("#select_data_source").find("option").remove();
    $("#select_data_source").append("<option>Select</option>");
    for (var i = 0; i < DatabaseTable.length; i++) {
        var option = "<option>" + DatabaseTable[i] + "</option>";

        if (isEditLookup && lookupInfo.SourceName == DatabaseTable[i]) {
            option = "<option selected>" + DatabaseTable[i] + "</option>";
        }

        $("#select_data_source").append(option);
    }


    var $radio = $('input:radio[name=data_source]');
    switch (lookupInfo.LookupType) {
        case 0:
            $radio.filter('[value=Table]').prop('checked', true);
            //$('#display_table').change();
            LookupDataType = $('#display_table').val();
            break;
        case 1:
            $radio.filter('[value=View]').prop('checked', true);
            //$('#display_view').change();
            LookupDataType = $('#display_view').val();
            break;
        case 2:
            $radio.filter('[value=StoredProcedure]').prop('checked', true);
            //$('#display_procedure').change();
            LookupDataType = $('#display_procedure').val();
            break;
    }

    $('#select_data_source').change();
    $(lookupContent).find('#sql_command').val(lookupInfo.SqlCommand);
}

function SaveLookupInfo() {
    if (isEditLookup) {
        for (var i = 0; i < lookupInfos.length; i++) {
            if (lookupInfo.FieldId == lookupInfos[i].FieldId) {
                lookupInfos[i] = lookupInfo;
                return;
            }
        }
    }
    else {
        lookupInfos.push(lookupInfo);
        $LookupRow.find('.td_haslookup').text('Yes');
        $LookupRow.find('.td_haslookup').data("haslookup","True");
        $LookupRow.find('.td_lookup').append('<span> | <Span><i class="delete_lookup glyphicon glyphicon-trash midle hand">');
    }
}

function DeleteLookup(fieldId) {
    for (var i = 0; i < lookupInfos.length; i++) {
        if (lookupInfos[i].FieldId == fieldId) {
            lookupInfos.splice(i,1);
            $LookupRow.find('.td_haslookup').text('No');
            $LookupRow.find('.td_lookup').find('.delete_lookup').remove();
            return;
        }
    }

    $LookupRow = undefined;
}

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
    JsonHelper.helper.post(
        URL_DeleteOcrTemplate,
        JSON.stringify({ id: contenttype_id }),
        DeleteOcrTemplate_Success,
        DeleteOcrTemplate_Error
    );
}

function DeleteOcrTemplate_Success(data) {
    var $this_menu_item = $(".admin_sub_menu_items").find(".content_sub_menu_item.selected");
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
    JsonHelper.helper.post(
        URL_ShowBarcodeConfigure,
        JSON.stringify({id: contenttype_id}),
        ShowBarcodeConfigure_Success,
        ShowBarcodeConfigure_Error
    );
}

function ShowBarcodeConfigure_Success(data) {
    $(".sub_properties").empty();
    $("#dialog_barcode_configure").find("#barcode_fields").find("option").remove();
    $(".sub_properties").append(data);

    var table = $('#barcode_table').dataTable(
    {
        "sScrollY": "350px",
        "sScrollYInner": "330px",
        "bScrollCollapse": false,
        "bPaginate": false,
        "bFilter": false,
        "bSearchable": false,
        "bInfo": false,
        "sPaginationType": "bootstrap",
        "bSort": false,
        "sEmptyTable":"",
    });


    $list_content_fields = lstFields;
    $(".sub_properties").ecm_loading_hide();
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
    $(".sub_properties").ecm_loading_hide();
    ShowBarcodeConfigure();
    $(".admin_sub_menu_items").ecm_loading_show();
    if ($("#barcode_table").find("#barcode_table_body tr").length > 0) {
        $(".submenu-list").find(".content_sub_menu_item.active").find(".barcode-delete").append("<i class='glyphicon glyphicon-trash hand barcode_config_delete' data-id='" + contenttype_id + "'></i>");
    } else {
        $(".submenu-list").find(".content_sub_menu_item.active").find(".barcode-delete").empty();
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
    var $this_menu_item = $(".admin_sub_menu_items").find(".content_sub_menu_item.selected");
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

function displayContentType(obj) {
    $(".sub_properties").find(".sub_properties_content").remove();
    $(".content_sub_menu_item").removeClass("active");
    $(obj).parent().addClass("active");
    isEditContentType = 1;
    _DocumentID = $(obj).parent().attr("id");
        
    ContentTypeProperties();
}

$(document).ready(function () {
    ShowListContentType();
    LookupDataType = $("#display_table").val();

    $("#content_types").click(function () {
        $(".admin-menu > li").removeClass("active");
        $(this).addClass("active");

        $(".admin_sub_menu").find("div").remove();
        $(".sub_properties").find(".sub_properties_content").remove();
        $(".archive_admin_container").find(".sub_properties_content").remove();

        $(".sub_properties").css({ display: 'block' });
        $(".admin_sub_menu").css({ display: 'block' });
        ShowListContentType();
    });

    $(document).on("click", ".content_type_item", function () {
        displayContentType(this);
    });

    $(document).on("click", ".content-type-name", function () {
        displayContentType($(this).parent());
    });

    $(document).on("click", "#addContentType", function () {
        $(".sub_properties").find(".sub_properties_content").remove();
        _DocumentID = -1; 
        isEditContentType = -1;
        $(".content_sub_menu_item").removeClass("selected"); 
        $("#button_delete_content").prop("disabled", true);
        ContentTypeProperties();
    });

    var dialog_delete_contenttype_yes_function = function () {
        DeleteContentType();
    };


    $(document).on("click", "#deleteContentType", function () {
        _DocumentID = $(".submenu-list").find(".content_sub_menu_item.active").attr("id");
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
                    dialog_delete_contenttype_yes_function();
                } 
            }
        });

    });
    
    var dialog_add_field_open_function = function () {
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

    $(document).on("click", "#btsaveField", function () {
        dialog_add_field_ok_function();
    });

    var dialog_add_field_ok_function = function () {
        var hasError = false;
        clearFieldError();
        //Field dialog validation
        if (!CheckField($("#field_name"), "Field name")) {
            $("#field_name").parent().addClass("has-error");
            $("#field_name").prop("title", "Field name is required!");
            return false;
        }

        var field_name_existed = false;
        $.each($contenttype_fields, function (index, value) {
            if ($("#field_name").val() == value.FieldName && _FieldID != value.FieldId && _FieldID != -1) {
                field_name_existed = true;
                return false;
            }
        })

        if (field_name_existed) {
            $("#field_name").parent().addClass("has-error");
            $("#field_name").prop("title", "Field name is already existed!");
            return false;
        }

        if (!fieldValidate()) {
            return false;
        }

        //End validation

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
                    table_field_required = $(this).find(".table_td_required").find(".table_check_required").is(":checked") ? "true" : "false";
                    table_field_restricted = $(this).find(".table_td_restricted").find(".table_check_restricted").is(":checked") ? "true" : "false";

                    var table_field_id = $(this).attr("id");
                    var td_use_current_date = $(this).find(".table_td_default_value").find("input.table_check_use_current_date");
                    table_field_use_current_date = td_use_current_date.val();
                    if (td_use_current_date.is(":checked")) {
                        table_field_use_current_date = "true";
                    }
                    else {
                        table_field_use_current_date = "false";
                    }
                    json_of_each_row.tablefiels.push({ DocTypeId: _DocumentID, ParentFieldId: _FieldID, FieldId: table_field_id, ColumnName: table_field_name, DataType: table_field_datatype, DefaultValue: table_field_default_value, UseCurrentDate: table_field_use_current_date, MaxLength: table_field_maxlength, IsRequired: table_field_required, IsRestricted: table_field_restricted });
                }

            });
            json_table_fields[field_name] = json_of_each_row.tablefiels;
            $("#table_configuration").find("tr.table_configure_row").remove();
            AppendFirstRowTableConfigure();

        }

        if (isEdit == -1) {
            if ($("#table_fields tbody#table_fields_body").find("tr > td").hasClass("dataTables_empty")) {
                $("#table_fields tbody#table_fields_body").find(".dataTables_empty").parent().remove();
            }

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
        $('#dialog_add_field').modal('hide');
        ResetFieldInDialog();

    }

    function fieldValidate() {
        switch ($("#dataType").val()) {
            case "String":
                if ($("#max_length").val() == "" || $("#max_length").val() == "0") {
                    $("#max_length").parent().addClass("has-error");
                    $("#max_length").prop("title", "Max length value is not allowed empty or zero!");
                    return false;
                }

                if ($("#default_value").val().length > parseInt($("#max_length").val())) {
                    $("#default_value").parent().addClass("has-error");
                    $("#default_value").prop("title", "Maximum length value of '" + $("#field_name").val() + "' field can not greater than " + $("#max_length").val());
                    return false;
                }
                break;
            case "Integer":
                if ($("#default_value").val() != "") {
                    if (!CheckIntegerNumber($("#default_value"), "Default value")) {
                        $("#default_value").parent().addClass("has-error");
                        $("#default_value").prop("title", "Default value must be integer number!");

                        return false;
                    }
                }
                break;
            case "Decimal":
                if ($("#default_value").val() != "") {
                    if (!CheckDecimalNumber($("#default_value"), "Default value")) {
                        $("#default_value").parent().addClass("has-error");
                        $("#default_value").prop("title", "Default value must be decimal number!");

                        return false;
                    }
                }
                break;
        }

        return true;
    }

    function clearFieldError() {
        $("#field_name").parent().removeClass("has-error");
        $("#field_name").prop("title", "");

        $("#max_length").parent().removeClass("has-error");
        $("#max_length").prop("title", "");

        $("#default_value").parent().removeClass("has-error");
        $("#default_value").prop("title", "");

    }

    $(document).on("click", "#closeFieldDialog", function () {
        dialog_add_field_cancel_function()
    });

    var dialog_add_field_cancel_function = function () {
        $("#table_configuration").find("tr.table_configure_row").remove();// xóa tất cả các dòng
        AppendFirstRowTableConfigure(); 
        ResetFieldInDialog();
    }

    // sự kiện khi click vào button add field
    $(document).on("click", "#button_add_field", function () {
        isEdit = -1;
        _FieldID = -1;

        $contenttype_fields = [];
        var lstField = $("#table_fields").find("#table_fields_body tr");
        $.each(lstField, function () {
            $contenttype_fields.push({ FieldId: $(this).attr("id"), FieldName: $(this).find(".td_field_name").text() });
        });

        ResetFieldInDialog();
        $('#dialog_add_field').modal("show");     
        $("#field_name").focus();
    });

    $(document).on("change", "#dataType", function () {
        ChangeDataType();
    });
    
    $(document).on("click", "#button_cancel_content", function () {
        $(".sub_properties").find(".sub_properties_content").remove();
        json_table_fields = {};
        json_delete_fields = { DeletedFields: [] };
        json_delete_fields.DeletedFields.push({ Id: -1 });
        json_table_value = {};
        json_pick_value = "";
        $(".content_sub_menu_item").first().find(".row_content_type").click();
    });


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


    $(document).on("click", "#button_delete_field", function () {
        var id_field_delete = $("tr.row_field.selected").attr("id");
        var name_field_delete = $("tr.row_field.selected").find(".td_field_name").text();
        $("tr.row_field.selected").remove();

        if (id_field_delete != -1) {
            json_delete_fields.DeletedFields.push({ Id: id_field_delete, Name: name_field_delete });
        }

    });


    $(document).on("click", "#button_save_content", function () {
        $("#content_name").parent().removeClass("has-error");
        $("#content_name").prop("title", "");

        if (CheckField($("#content_name"), "Content type name", 0, 0)) {
            SaveContentTypeEvent();
            picklistvalue = "";
            _FieldID = -1;
            json_table_fields = {};
        }
        else {
            $("#content_name").parent().addClass("has-error");
            $("#content_name").prop("title", "Content type name is required!");
        }
    });


    function editField(tr, fields) {
        isEdit = 1;
        _FieldID = $(tr).attr("id");

        if (_FieldID == undefined) {
            $.EcmAlert("Please select field you want to edit!", "WARNNING", BootstrapDialog.TYPE_WARNING);
            return false;
        }

        if ($(tr).find(".td_data_type").text() == "Table" && _FieldID != -1) {
            // GetPicklistValue();
            LoadTableConfigure();
        }
        else if ($(tr).find(".td_data_type").text() == "Table" && _FieldID == -1) {
            EditTableConfigure();
        }

        $contenttype_fields = [];
        $.each(fields, function () {
            if ($(this).attr("class") != "row_field selected") {
                $contenttype_fields.push({ FieldId: $(this).attr("id"), FieldName: $(this).find(".td_field_name").text() });
            }
        });

        dialog_add_field_open_function();
        $('#dialog_add_field').modal("show");
        $("#field_name").focus();
    }

    $(document).on("click", "#button_edit_field", function () {
        editField($("tr.row_field.selected"), $("#table_fields").find("#table_fields_body tr"));
    });

    $(document).on('dblclick', '#table_fields_body > tr', function () {
        editField(this, $("#table_fields").find("#table_fields_body tr"));
    });

    
    $(document).on("click", "#savePicklist", function () {
        if ($("#picklist_value").val() == "") {
            $.EcmAlert("Picklist value is required!", "WARNING", BootstrapDialog.TYPE_WARNING);

            return false;
        }

        $("#picklist_value_dialog").modal("hide");
    });

    $(document).on("click", "#configure_picklist", function () {
        $("#picklist_value").focus();
        if (isEdit != 1) {
            $("#picklist_value").val("");
        }

        $("#picklist_value_dialog").modal("show");
        $("#picklist_value").val($("tr.row_field.selected").find(".td_picklist").text()); // gán giá trị cho piclick
    });

    var dialog_table_configure_save_function = function () {
        var $table_configure = $("#table_configuration");
        var lstTableConfigureRow = $table_configure.find("tbody tr.table_configure_row");
        var isValid = true;

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
                            isValid = false;
                            return false;
                        }
                    }

                    if ($table_data_type.val() == "Integer" && $table_default_value.val() != "") {
                        if (default_val == "0") {
                            $table_default_value.val("1");
                        }
                        if (CheckIntegerNumber($table_default_value, "Default value", 0, 0)) {
                            if (default_val == "0") {
                                $table_default_value.val("0");
                            }
                        } else {
                            isValid = false;
                            return false;
                        }
                    }

                    if ($table_data_type.val() == "Decimal" && $table_default_value.val() != "") {
                        if (default_val == "0") {
                            $table_default_value.val("1");
                        }
                        if (CheckDecimalNumber($table_default_value, "Default value", 0, 0)) {
                            if (default_val == "0") {
                                $table_default_value.val("0");
                            }
                        } else {
                            isValid = false;
                            return false;
                        }
                    }

                } else {
                    isValid = false;
                    return false;
                }

            } else {
                isValid = false;
                return false;
            }

        });

        if (!isValid) {
            return false;
        }

        $("#dialog_table_configure").modal('hide');
    };

    $(document).on("click", "#saveTableColumn", function () {
        dialog_table_configure_save_function();
    });

    $(document).on("click", "#closeTableColumn", function () {
        $("#table_configuration  tbody").find("#-1").remove();
    });

    $(document).on("click", "#configure_table", function () {
        $("#dialog_table_configure").modal("show");
    });
    // sự kiện khi click vào add new columns in table configuration
    $(document).on("click", "#add_new_column", function () {

        $("#table_configuration").append("<tr class='table_configure_row' id='-1'>"
                                              + "<td class='table_td_delete'><i class='dialog_table_icon glyphicon glyphicon-trash large hand'></i></td>"
                                              + "<td class='table_td_name'><input type='text' class='form-control input-sm table_name_value' placeholder='New column'></td>"
                                              +"<td class='table_td_type'>"
                                                  + "<select  class='form-control input-sm table_data_type'>"
                                                     + "<option value='String' selected>String</option>"
                                                     + "<option value='Integer'>Integer</option>"
                                                     + "<option value='Decimal'>Decimal</option>"
                                                     + "<option value='Date'>Date</option>" 
                                                 + "</select>"
                                             + "</td>"
                                             + "<td class='table_td_maxlength'><input type='text' class='form-control input-sm table_maxlength allownumericwithoutdecimal' value='0'></td>"
                                             + "<td class='table_td_required'><input type='checkbox' class='table_check_required'/></td>"
                                             + "<td class='table_td_restricted'><input type='checkbox' class='table_check_restricted'/></td>"
                                             +"<td class='table_td_default_value'>"
                                                 + "<input type='text' class='form-control input-sm table_default_value'>"
                                                 + "<div class='dialog_table_checkbox table_use_current_date_contain'>"
                                                 + "<input type='checkbox' class='table_check_use_current_date' value='false' style='display:none'><a class='table_check_use_current_date' style='display:none'>Use current date</a>"
                                             + "</td>"
                                           + "</tr>" );

    });

    // Delete temporary table row table configuration dialog
    $(document).on("click", ".table_td_delete", function () {

        // $(this).parent().remove();
        // var id_field_delete = $("tr.row_field.selected").attr("id");
        var id_field_delete = $(this).parent().attr("id");
        $(this).parent().remove();
        //json_delete_fields.DeletedFields.push({ Id: id_field_delete });

    });
    //Delete exiting table rows
    $(document).on("click", ".id_td_delete", function () {
        var id_field_delete = $(this).parent().attr("id");
        $(this).parent().remove();
        json_delete_fields.DeletedFields.push({ Id: id_field_delete });
    });

    // Delete first row table configuration dialog 
    $(document).on("click", "#first_delete_column", function () {
        $(this).parent().remove();
        AppendFirstRowTableConfigure();


    });
    //sự kiện khi click thay đổi các giá trị của select datatye trên Table configuration
    $(document).on("change", ".table_data_type", function () {
        ChangeTableDataType($(this).val());
    });
    
    $(document).on("focus", ".table_name_value, .table_data_type, .table_maxlength, .table_default_value", function () {
        $(".table_configure_row").removeClass("row_selected");
        $(this).parent().parent().addClass("row_selected");
    });

    // sự kiện khi click vào Configure
    $(document).on("click", "#saveLookup", function () {
        dialog_lookup_ok_function();
    });

    var dialog_lookup_ok_function = function (fieldId) {
        //var  = $("#dialog_lookup").data("fieldid");
        lookupInfo.FieldId = fieldId;
        $.ajax({
            url: URL_TestConnection,
            type: "POST",
            async: true,
            data: JSON.stringify({ ConnectionInfo: lookupInfo.ConnectionInfo }),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.Testconnectionfail == "false") {
                    $.EcmAlert('Lookup connection invalid. Please check lookup information and try again', "WARNNING", BootstrapDialog.TYPE_WARNING);
                }
                else {
                    SaveLookupInfo();
                    lookupInfo = undefined;
                    $LookupRow = undefined;
                    ClearLookupInfoForm($("#dialog_lookup"));
                    isEditLookup = false;
                    $("#dialog_lookup").modal('hide');
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
            }
        });

    };

    var dialog_lookup_cancel_function = function () {
        lookupInfo = undefined;
        $LookupRow = undefined;
        isEditLookup = false;
    };

    var dialog_lookup_open_function = function (fieldId, hasLookup) {

        if (hasLookup.toLowerCase() == "true") {
            $.ajax({
                url: URL_LoadLookupInfo,
                type: 'POST',
                async: false,
                contentType: 'application/json',
                data: JSON.stringify({ fieldId: fieldId }),
                success: function (data) {
                    lookupInfo = data.lookupInfo;
                    if (lookupInfo != undefined) {
                        isEditLookup = true;
                        LoadLookupInfo();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $.EcmAlert('Load lookup info fail!', "WARNNING", BootstrapDialog.TYPE_WARNING);
                }
            });
        }
        else {
            LookupDataType = "Table";
            lookupInfo = { FieldId: fieldId};
        }
    }

    $(document).on("click", ".config_lookup", function () {
        var fieldId = $(this).parent().parent().attr('id');
        var hasLookup = $(this).parent().parent().find(".td_haslookup").data("haslookup");
        $LookupRow = $(this).parent().parent();
        var dialog = BootstrapDialog.show({
            id: 'dialog_lookup',
            type: BootstrapDialog.TYPE_SUCCESS,
            title: 'Lookup configuration',
            message: $lookupContent,
            draggable: true,
            data: { 'fieldid': fieldId },
            onshown: function (dialogRef) {
                dialog_lookup_open_function(fieldId, hasLookup);
                $("#server_name").focus();
            },
            buttons: [{
                label: 'OK',
                cssClass: 'btn-flat bg-olive btn-sm',
                icon: 'glyphicon glyphicon-ok',
                action: function (dialogRef) {
                    dialog_lookup_ok_function(fieldId);
                    dialogRef.close();
                }
            },
            {
                label: 'Close',
                cssClass: 'btn-flat bg-olive btn-sm',
                icon: 'glyphicon glyphicon-remove',
                action: function (dialogRef) {
                    dialogRef.close();
                }
            }]
        });
    });

    $(document).on("click", "#cancelLookup", function () {
        dialog_lookup_cancel_function();
    });

    // sự kiện khi click vào test connection button 
    $(document).on("click", "#button_test_connection", function () {
        var connectionInfo = {};
        var $serverNameControl = $("#server_name");
        var $userNameControl = $("#user_name");
        var $passwordControl = $("#pass_word");
        var $portControl = $("#port_number");

        var isDataValid = true;

        if ($serverNameControl.val() == "") {
            $serverNameControl.parent().addClass("has-error");
            $serverNameControl.prop("title", "Server or ip address name is required!");
            isDataValid = false;
        }

        if ($userNameControl.val() == "") {
            $userNameControl.parent().addClass("has-error");
            $userNameControl.prop("title", "Username is required!");
            isDataValid = false;
        }

        if ($passwordControl.val() == "") {
            $passwordControl.parent().addClass("has-error");
            $passwordControl.prop("title", "Password is required!");
            isDataValid = false;
        }

        if ($portControl.val() == "") {
            $portControl.parent().addClass("has-error");
            $portControl.prop("title", "Port number is required!");
            isDataValid = false;
        }

        if (!isDataValid) {
            return false;
        }

        ServerName = $("#server_name").val();
        UserName = $("#user_name").val();
        Password = $("#pass_word").val();
        DataProvider = $("#data_provider_select").val();
        DatabaseType = $("#database_type_select").val();
        PortNumber = $("#port_number").val();

        connectionInfo = { Host: ServerName, DatabaseType: DatabaseType, ProviderType: DataProvider, Port: PortNumber, Username: UserName, Password: Password, DatabaseName: '', Schema: '' };
        TestConnection2(connectionInfo);
    });

    //$("#dialog_lookup_content").tabs({ disabled: [1] });
    // sự kiện khi change select_database 
    $(document).on("change", "#select_database", function () {
        var databaseName = $("#select_database").val();
        lookupInfo.ConnectionInfo.DatabaseName = databaseName;

        GetDataSource(LookupDataType);
        $("#table_look_data").find('tr.lookup_data_row').remove();
        $('#storeparameter').empty();
        lookupInfo.SqlCommand = '';
        lookupInfo.Parameters = [];
        lookupInfo.SourceName = '';
    });

    $(document).on("change", "#select_schema", function () {
        var schema = $("#select_schema").val();
        lookupInfo.ConnectionInfo.Schema = schema;
        GetDataSource(LookupDataType);
        $("#table_look_data").find('tr.lookup_data_row').remove();
        $('#storeparameter').empty();
        lookupInfo.SqlCommand = '';
        lookupInfo.Parameters = [];
        lookupInfo.SourceName = '';
    });
    //sự kiện khi change select_data_source
    $(document).on("change","#select_data_source",function(){
    
        if ($(this).val() != "Select") {
            $(this).parent().removeClass("has-error");
            $(this).prop("title", "");

        }

        SourceName = $("#select_data_source").val();
        lookupInfo['SourceName'] = SourceName;

        $("#table_look_data").find('tr.lookup_data_row').remove();
        $("#table_fields").find('tr').each(function (index) {
            if (index != 0)
            {
                var fieldName = $(this).find(".td_field_name").text(); 
                var fieldId = $(this).attr('id');
                
                $("#table_look_data tbody").append("<tr class='lookup_data_row'>"
                                                +"<td class='field_name'>"+ fieldName +"<input type='hidden' class='archive_field_id' value='"+ fieldId +"'/></td>"
                                                + "<td class='td_select_column'>"
                                                   + "<select class='form-control input-sm select_column'>"
                                
                                                   + "</select>"
                                              + " </td>"
                                              + "</tr>");
            }
        });


        if (LookupDataType == 'StoredProcedure') {

            if (isEditLookup){//(lookupInfo != undefined && lookupInfo.Parameters != undefined && lookupInfo.Parameters.length > 0) {
                var parameterContent = $('#storeparameter');
                //var parameters = parameterContent.find('.parameter_content');

                //$.each(parameters, function (i, item) {
                $("#storeparameter").empty();

                for (var i = 0; i < lookupInfo.Parameters.length; i++) {
                    var para = '<div class="form-group">' +
                                    '<label class="label-control parameter_name">' + lookupInfo.Parameters[i].ParameterName + '</label>' +
                                    '<input type="text" value="' + lookupInfo.Parameters[i].ParameterValue + '"  class="form-control input-sm parameter_value"/>'+
                    '<input type="hidden" class="parameter_type" value="' + lookupInfo.Parameters[i].ParameterType + '" />'+
                    '</div>';

                    parameterContent.append(para);
                }
                //});
            }
            else {
                GetParameters(SourceName);
            }

        }
        GetColumns();
    });

    $(document).on("change", "#select_database_field", function () {
        var fieldName = $(this).val();
        var fieldType= '';

        $.each(ColumnName.LookupColumn, function (i, item) {
            if (item.Key == fieldName) {
                fieldType = item.Value;
            }
        });

        lookupInfo.LookupColumn = fieldName;
        GetOperators(fieldType);
    });

    $(document).on("change", "#select_operator", function () {
        lookupInfo.LookupOperator = $(this).val();
    });

    $(document).on("change", "#select_database_field_stored", function () {
        var fieldName = $(this).val();
        var fieldType= '';

        $.each(ColumnName.LookupColumn, function (i, item) {
            if (item.Key == fieldName) {
                fieldType = item.Value;
            }
        });

        lookupInfo.LookupColumn = fieldName;
        GetOperatorsFromStored(fieldType);
    });

    $(document).on("change", "#select_operator_stored", function () {
        lookupInfo.LookupOperator = $(this).val();
    });

    $(document).on("click", ".delete_lookup", function () {
        var fieldId = $(this).parent().parent().attr('id');
        $LookupRow = $(this).parent().parent();

        BootstrapDialog.confirm({
            title: 'WARNING',
            message: 'Are you sure you want to delete lookup information?',
            type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
            closable: true, // <-- Default value is false
            draggable: true, // <-- Default value is false
            btnCancelLabel: 'Cancel', // <-- Default value is 'Cancel',
            btnOKLabel: 'OK', // <-- Default value is 'OK',
            btnOKClass: 'btn-warning', // <-- If you didn't specify it, dialog type will be used,
            callback: function (result) {
                // result will be true if button was click, while it will be false if users close the dialog directly.
                if (result) {
                    DeleteLookup(fieldId);
                } 
            }
        });

    });

    // sự kiện 
    $(document).on('change', '#display_table', function () {
        if ($(this).is(":checked"))
        {
            var databaseName = $("#select_database").val();
            lookupInfo.LookupType = 0;
            LookupDataType = $(this).val();
            $("#table_look_data").find('tr.lookup_data_row').remove();
            $('#commandstore').hide();
            $('#commandtext').show();
            GetDataSource(LookupDataType);
        }
       
    });
    // sự kiện khi check vào checkbox display view
    $(document).on('change', '#display_view', function () {
        if ($(this).is(":checked")) {
            var databaseName = $("#select_database").val();
            lookupInfo.LookupType = 1;
            LookupDataType = $(this).val();
            $("#table_look_data").find('tr.lookup_data_row').remove();
            GetDataSource(LookupDataType);
            $('#commandstore').hide();
            $('#commandtext').show();
        }

    });
    // sự kiện khi check vào checkbox display procedure
    $(document).on('change', '#display_procedure', function () {
        if ($(this).is(":checked")) {
            var databaseName = $("#select_database").val();
            lookupInfo.LookupType = 2;
            LookupDataType = $(this).val();

            $("#table_look_data").find('tr.lookup_data_row').remove();
            $('#commandstore').show();
            $('#commandtext').hide();

            GetDataSource(LookupDataType);
        }

    });

    $(document).on('click', '#commandInfo', function () {
        if (lookupInfo != undefined) {
            var $dataSource = $("#select_data_source");
            var isDataValid = true;

            if ($dataSource.val() == "Select") {
                $dataSource.parent().addClass("has-error");
                $dataSource.prop("title", "Please select data source!");
                isDataValid = false;
            }

            var $lookupRows = $('.lookup_data_row');
            lookupInfo.FieldMappings = [];

            $.each($lookupRows, function (i, item) {
                var fieldName = $(this).find('.field_name').text();
                var columnName = $(this).find('.select_column').val();
                var archiveFieldId = $(this).find('.archive_field_id').val();
                if (columnName != 'Select') {
                    lookupInfo.FieldMappings.push({ DataColumn: columnName, Name: fieldName, FieldId: lookupInfo.FieldId, ArchiveFieldId: archiveFieldId });
                }
            });

            if (lookupInfo.FieldMappings.length <= 0) {
                $.EcmAlert('Field mapping must has at least one row!', "WARNNING", BootstrapDialog.TYPE_WARNING);
                isDataValid = false;
            }

            if (!isDataValid) {
                return false;
            }

            if (LookupDataType == 'StoredProcedure') {
                BuildExecuteCommand();
            }
            else {
                BuildCommandText();
            }
        }
    });

    $(document).on('click', '#button_add_condition', function () {
        AddWhereClause();
    });

    $(document).on('click','#button_clear_condition',function () {
            if (LookupDataType == 'StoredProcedure') {
                BuildExecuteCommand();
            }
            else {
                var sql = $("#sql_command").val();
                sql = sql.substring(0, sql.indexOf("WHERE") + 5);
                $("#sql_command").val(sql + " {0}");
            }
    });

    $(document).on('click', '#build_execute_command',function () {
        var $parameters = $('#storeparameter').find('.parameter_content');

        lookupInfo.Parameters = [];

        $.each($parameters, function (i, item) {
            var paraName = $(item).find('.parameter_name').text();
            var paraValue = $(item).find('.parameter_value').val();
            var paraType = $(item).find('.parameter_type').val();

            lookupInfo.Parameters.push({ ParameterName: paraName, ParameterValue: paraValue, ParameterType: paraType, OrderIndex: i + 1 });
        });

        BuildExecuteCommand();
    });

    //$('#test_lookup_data').click(function () {
    //    $("#lookup_value").focus();
    //    $('#dialog_test_lookup').modal('show');
    //});

    $(document).on("click", '#test_lookup_data', function () {
        var $testLookupContent = '<div class="form-group">'+
                                    '<label class="label-control">Lookup value</label>'+
                                    '<input type="text" class="form-control input-sm" id="lookup_value"/>'+
                                '</div>';
        BootstrapDialog.show({
            id: 'dialog_test_lookup',
            title: 'Test lookup',
            message: $testLookupContent,
            onshown: function (dialogRef) {
                $("#lookup_value").focus();
            },
            buttons: [{
                label: 'OK',
                cssClass: 'btn-success',
                icon: 'glyphicon glyphicon-ok',
                action: function (dialogRef) {
                    dialog_test_lookup_ok_function();
                    dialogRef.close();
                }
            },
            {
                label: 'Close',
                action: function (dialogRef) {
                    dialogRef.close();
                }
            }]
        });
        //$('#dialog_test_lookup').modal('show');
    });

    $(document).on("click","#testLookupOk", function(){
        dialog_test_lookup_ok_function();
        $('#dialog_test_lookup').modal('hide');
    });

    $(document).on("click","#testLookupCancel", function(){
        dialog_test_lookup_cancel_function();
    });

    function dialog_test_lookup_ok_function() {
        var lookupValue = $('#lookup_value').val();
        ShowLookupData(lookupValue);
        $('#lookup_value').empty();
        //$(this).dialog("close");
    }

    function dialog_test_lookup_cancel_function() {
        var lookupValue = $('#lookup_value').val();
        $('#lookup_value').empty();
        //$(this).dialog("close");
    }

    String.prototype.format = function () {
        var str = this;
        for (var i = 0; i < arguments.length; i++) {
            var reg = new RegExp("\\{" + i + "\\}", "gm");
            str = str.replace(reg, arguments[i]);
        }
        return str;
    }
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

        $.EcmDialog({
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

        BootstrapDialog.confirm({
            title: 'WARNING',
            message: 'Are you sure you delete Ocr template of ' + contenttype_name,
            type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
            closable: true, // <-- Default value is false
            draggable: true, // <-- Default value is false
            btnCancelLabel: 'Cancel', // <-- Default value is 'Cancel',
            btnOKLabel: 'OK', // <-- Default value is 'OK',
            btnOKClass: 'btn-warning', // <-- If you didn't specify it, dialog type will be used,
            callback: function (result) {
                // result will be true if button was click, while it will be false if users close the dialog directly.
                if (result) {
                    dialog_delete_orc_yes_function();
                } else {
                    dialog_delete_orc_no_function();
                }
            }
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
        BootstrapDialog.confirm({
            title: 'WARNING',
            message: 'Are you sure you delete Barcode of ' + contenttype_name +'?',
            type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
            closable: true, // <-- Default value is false
            draggable: true, // <-- Default value is false
            btnCancelLabel: 'Cancel', // <-- Default value is 'Cancel',
            btnOKLabel: 'OK', // <-- Default value is 'OK',
            btnOKClass: 'btn-warning', // <-- If you didn't specify it, dialog type will be used,
            callback: function (result) {
                // result will be true if button was click, while it will be false if users close the dialog directly.
                if (result) {
                    dialog_delete_barcode_yes_function();
                } else {
                    dialog_delete_barcode_no_function();
                }
            }
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
                        $("#barcode_table").find("tbody#barcode_table_body").empty().append(
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
                    $('#dialog_barcode_configure').modal('hide');
                    barcodeEdit = -1;
                    ResetAllFielsBarcodeConfigure();
                }
            }
        }
    }

    var dialog_barcode_configure_cancel_function = function () {
        barcodeEdit = -1;
        ResetAllFielsBarcodeConfigure();
    }

    $(document).on("click", "#button_add_barcode", function () {
        $("#barcode_table").find(".tr_barcode_table_body").removeClass("selected");
        barcodeEdit = -1;
        dialog_barcode_configure_open_function();
        $('#dialog_barcode_configure').modal('show');

    });
    
    $(document).on("click", "#saveBarcodeConfiguration", function () {
        dialog_barcode_configure_save_function();
    });

    $(document).on("click", "#cancelBarcodeConfiguration", function () {
        dialog_barcode_configure_cancel_function();
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
            dialog_barcode_configure_open_function();
            $('#dialog_barcode_configure').modal('show');
        } else {
            $.EcmAlert('Please select barcode type', "WARNNING", BootstrapDialog.TYPE_WARNING);
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
        $(".sub_properties").empty();
        $(".content_sub_menu_item").removeClass("active");
        $(this).parentsUntil(".content_sub_menu_item").parent().addClass("active");
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

    //OCR Template configuration
    $(document).on("click", ".ocr_template", function () {
        $(this).parentsUntil(".admin_sub_menu_items").last().find(".row_content_type").click();
        $(".archive_admin_container").empty().hide();
        $(".ocr_template_panel").show();
        $(".ocr_template_panel").ecm_loading_show();

        var url = $(this).attr('href');
        var $ocr_dialog = $("div.ocr_template_panel");

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
                        RotateAngle: $(e).attr('data-rotateangle'),
                        FileExtension:$(e).attr('data-fileextension')
                    };

                    var thumb = {
                        KeyCache:$(e).attr('data-id').toString(),
                        pageIndex: i,
                        Resolution: $(e).attr('data-dpi').toString(),
                        RotateAngle:$(e).attr('data-rotateangle'),
                    };

                    $(e).children('.ocr_template_zone').each(function (i, e) {
                        var zone = {
                            type: 'ocr_zone',
                            select: $(e).attr('data-name'),
                            left: parseFloat($(e).attr('data-left')),
                            top: parseFloat($(e).attr('data-top')),
                            width: parseFloat($(e).attr('data-width')),
                            height: parseFloat($(e).attr('data-height')),
                            id:$(e).attr('data-id'),
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
                setButtonWidthInFirefox();
                if (datas.length > 0 && thumbDatas.length > 0) {
                    createPage(datas);
                    createThumbnail(thumbDatas);
                } else {
                    $(".ocr_template_panel").ecm_loading_hide();
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
        //Goi ham submit de upload image len server
        var options = {
            url: URL_UploadFile,
            dataType: "json",
            success: function (data) {
                importTemplate(data, insOption);
                $documentViewer.ecm_loading_hide();
            },
            error: showError
        };
        $('#formUpload').ajaxSubmit(options);
    });

    function showError() {
        $.EcmAlert('Upload fail.', "WARNNING", BootstrapDialog.TYPE_WARNING);
   }

    $(document).on("click", "#ocr_save_button", function () {

        var $doc_view = $("#docViewer").children('.page');
        var anno_ocr_name_used;
        var ocrValid = true;

            $(".ocr_template_panel").ecm_loading_show();
            var $ocrTemplate = $('.doc_type_id');
            var $ocrLanguage = $(".ocr_language").val();
            var ocrTemplate = {
                DocTypeId: $ocrTemplate.val(),
                OCRTemplatePages: [],
                LangId: $ocrLanguage
            };
            
            $doc_view.each(function (i) {
                var id = $(this).attr("id").substring(5);

                if (!draws[id].checkOcrTemplateValid()) {
                    ocrValid = false;
                    return false;
                }

                var key = $(this).attr('data-key');
                var extension = $(this).attr('data-extension');
                var ocrPage = {
                    Key: key,
                    OCRTemplateZone: [],
                    PageIndex: i,
                    FileExtension: extension
                }

                var param = draws[id].getAnnotationsOriginSize();
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

            if (ocrValid) {
                $.ajax({
                    url: URL_SaveOCRTemplate,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ ocrTemplate: ocrTemplate }),
                    success: function (data) {
                        if (data == true) {
                            if ($(".admin_sub_menu_items").find(".content_sub_menu_item.selected").find(".delete_ocr").length == 0) {
                                $(".admin_sub_menu_items").find(".content_sub_menu_item.selected").find(".add_delete_ocr_icon").addClass("delete_ocr");
                                $(".admin_sub_menu_items").find(".content_sub_menu_item.selected").find(".add_delete_ocr_icon").removeClass("add_delete_ocr_icon");
                            }
                        } 
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
                    }
                });
            }
            else {
                $.EcmDialog({
                    title: 'Error information',
                    width: 350,
                    dialog_data: '<div class="message_infor">"' + anno_ocr_name_used + '" field has been used!</div>',
                    type: 'Ok',
                    Ok_Button: function () {
                        $(this).dialog('close');
                    }
                });
            }

            $(".ocr_template_panel").ecm_loading_hide();
            ShowListContentType();
            //$("#content_types").click();

            $(".right_column").show();
            $(".left_column").show();
            $(".ocr_template_panel").hide();
            $(".ocr_template_panel").find(".between_and_right_content").remove();
        //$("#ocr_close_button").click();
    });

    $(document).on("click", "#ocr_close_button", function () {
        $(".right_column").show();
        $(".left_column").show();

        $(".ocr_template_panel").hide();
        $(".ocr_template_panel").find(".between_and_right_content").remove();

        $(".admin_sub_menu").show();
        $(".admin_sub_menu").css({ display: 'inline - block' });
        $(".sub_properties").show();
        $(".sub_properties").css({ display: 'inline-block' });
        $("#content_types").click();
    });

    $(document).on('click', '.rootThumb > li', pageClick);

    $(document).on("click", "#zoom_in", function () {
        var ids = getAllPageIds();
        $.each(ids, function () {
            draws[this].zoomIn();
        });
    });

    $(document).on("click", "#zoom_out", function () {
        var ids = getAllPageIds();
        $.each(ids, function () {
            draws[this].zoomOut();
        });
    });

    $(document).on("click", "#pan", function () {
        var ids = getAllPageIds();
        $.each(ids, function () {
            draws[this].scrollable();
        });
    });

    $(document).on("click", "#draw", function () {
        var ids = getAllPageIds();
        $.each(ids, function () {
            draws[this].annoOcrZone();
        });
    });

    $(document).on("click", "#rotate_left", function () {
        var currentId = $('.treeview_select').attr('id').toString();

        draws[currentId].rotateCounterClockwise();
        thumbs[currentId].rotateThumbLeft();
    });

    $(document).on("click", "#rotate_right", function () {
        var currentId = $('.treeview_select').attr('id').toString();
        draws[currentId].rotateClockwise();
        thumbs[currentId].rotateThumbRight();
    });

    function getAllPageIds() {
        var $temThumb = $('.ocr_pages > li > .page');
        var ids = [];

        $temThumb.each(function () {
            ids.push($(this).attr('id').toString());
        });

        return ids;
    }

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
   
    //autoFixTableHeader(tablename);
});

function relatives(id) {
    var p = $('#' + id).parent();
    var rs = [];
    p.find('li > .treeview_title').each(function () {
        rs.push($(this).attr('id').toString());
    });
    return rs;
}

function autoFixTableHeader(table) {
    var $table = $('#'+ table);
    var $header = $('#' + table +' thead');
    var $headerColumns = $header.find('tr td');

    var $body = $('#' + table + ' tbody');
    var $bodyRows = $body.find('tr');
    var $bodyColumns = $($bodyRows[0]).find('td');

    $.each($bodyColumns, function (i, item) {
        if ($(item).width() > $($headerColumns[i]).width()) {
            $($headerColumns[i]).width($(item).width());
        }
        else {
            $(item).width($($headerColumns[i]).width());
        }
    });
}
});

function ClearLookupInfoForm(dialogContent) {
    $("#lookupServerInfo").addClass("active");
    $("#lookupMapInfo").removeClass("active");
    $("#lookupMapInfo").addClass("disabled");
    $("#lookupMapInfo > a").attr("data-toggle", "");
    $("#lookupCommandInfo").removeClass("active");

    $("#tabs-ServerInfo").addClass("active");
    $("#tabs-CommandInfo").removeClass("active");
    $("#tabs-MappingInfo").removeClass("active");


    var lookupContent = $(dialogContent).find('#dialog_lookup_content');
    $(lookupContent).find('#server_name').val('');
    //Set default database type
    $(".select_column").find("option").remove();
    $("#select_database_field").find("option").remove();
    $("#select_database").find("option").remove();
    $("#select_schema").find("option").remove();
    $("#select_data_source").find("option").remove();
    $(lookupContent).find('#port_number').val('');
    $(lookupContent).find('#user_name').val('');
    $(lookupContent).find('#pass_word').val('');
    $(lookupContent).find('#pass_word').val('');
    $("#lookupMapInfo").addClass('disabled');
    $("#lookupMapInfo > a").removeAttr("data-toggle", "tab");
    $("#dialog_lookup").data("fieldId","");

    var $radio = $('input:radio[name=data_source]');
    $radio.filter('[value=Table]').prop('checked', true);
    $('#number_of_lookup_rows').val('');
    $('#sql_command').val('');
    $('#lookup_data_result').empty();
    $("#table_look_data").find('tr.lookup_data_row').remove();
    $('#storeparameter').empty();

}

function getLookupInfo(fieldId) {
    //if (lookupInfos != undefined) {
    //    for (var i = 0; i < lookupInfos.length; i++) {
    //        if (lookupInfos[i].FieldId == fieldId) {
    //            return lookupInfos[i];
    //        }
    //    }
    //}
    $.ajax({
        url: URL_LoadLookupInfo,
        type: 'POST',
        async: true,
        contentType: 'application/json',
        data: JSON.stringify({ fieldId: fieldId }),
        success: function (data) {
            return data.lookupInfo;
        },
        error: function (jqXHR, textStatus, errorThrown) {
            $.EcmAlert(jqXHR, "WARNNING", BootstrapDialog.TYPE_WARNING);
        }
    });
}

function pageClick(e) {
    currentPageId = $(this).find($('.page')).attr('id');
    //ready create documentvier
    var currentPage = $("#docViewer").find('#page_' + currentPageId);
    //Visible = hidden ==> Page haven't finished loading
    if (loading(currentPageId))
        return false;

    //if (currentPage.length > 0) {
    //    $("#docViewer").children().hide();
    //    currentPage.parent().show();
    //    $("#docViewer").scrollTo(currentPage);
    //}
    $("#docViewer").scrollTo(currentPage);

    /////////////////set treeview_select class
    //if (!e.ctrlKey) {
    //    $('.treeview_select').removeClass('treeview_select');
    //    $(this).parent().parent().find('.folder').addClass('treeview_select');
    //}

    $('.treeview_select').removeClass('treeview_select');

    if (!$(this).children('.treeview_title').hasClass('treeview_select')) {
        $(this).children('.treeview_title').addClass('treeview_select');
    }
    else {
        $(this).children('.treeview_title').removeClass('treeview_select');
    }
    /////////////////////////////////////////////////

    return false;
}

function pageElementClick(e) {
    var $page = $(this).parent().find(".select");

    $.each($page, function (i, item) {
        $(item).removeClass("select");
    });

    $(this).addClass("select");
}

function loading(id) {
    if ($("#" + id).parent().find('.viewer') == 'image' &&
        $("#docViewer").find('#page_' + id))
        return true;
    return false;
}
////////////////////////////////////////////////////////////////////////////////

var pos = 0, ctx = null, saveCB, image = [];
//var cameraWidth = 600, cameraHeight = 450;
var cameraWidth = 320, cameraHeight = 240;
var canvas = document.createElement("canvas");
canvas.setAttribute('width', cameraWidth);
canvas.setAttribute('height', cameraHeight);
ctx = canvas.getContext("2d");
image = ctx.getImageData(0, 0, cameraWidth, cameraHeight);
saveCB = function (data) {
    var col = data.split(";");
    var img = image;
    for (var i = 0; i < cameraWidth; i++) {
        var tmp = parseInt(col[i]);
        img.data[pos + 0] = (tmp >> 16) & 0xff;
        img.data[pos + 1] = (tmp >> 8) & 0xff;
        img.data[pos + 2] = tmp & 0xff;
        img.data[pos + 3] = 0xff;
        pos += 4;
    }
    if (pos >= 4 * cameraWidth * cameraHeight) {
        ctx.putImageData(img, 0, 0);
        $('body').ecm_loading_show();
        $.post(URL_PostImage, { fileUpload: canvas.toDataURL("image/png"), isFromCamera: true }, function (data) {
            //docTypeName = "Test CV"
            importTemplate(data, insOption);
            $('body').ecm_loading_hide();
        });
        pos = 0;
    }
};

function showWebcam() {
    var $camera = $("<div id='camera'></div>");
    $.messageBox({
        title: "Capture from camera",
        type: 'form',
        message: null,
        formData: $camera,
        width: 630,
        height: 600,
        buttons: {
            OK: function () {
                webcam.capture();
                return false;
            },
            Cancel: function () { }
        }
    });
    $camera.webcam({
        width: 600,
        height: 450,
        mode: "callback",
        swffile: URL_Camera,
        onTick: function () { },
        onSave: saveCB,
        onCapture: captureWebcam,
        debug: function () { },
        onLoad: function () { }
    });
}

function captureWebcam() {
    webcam.save();
}
