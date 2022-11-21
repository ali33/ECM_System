//#########################################################
//#Copyright (C) 2013, MIA Solution. All Rights Reserved
//#
//#History:
//# DateTime         Updater         Comment
//# 28/11/2013       Triet Ho        Tao moi

//##################################################################
var ViewBy = "bycontent";
var UserGroup = {};
var PermissionUserGroupId = -1;
var DocTypeID = -1;
var JsonSaveDocPermission;
var JsonSaveAnnotation;
var JsonSaveAudit;
var IsCheckBoxChange = false;

$(function () {
    // Show permission properties
    function ShowPermissionProperties() {

        ///Get Ajax
        $(".sub_properties").ecm_loading_show();

        JsonHelper.helper.post(
            URL_ShowPermissionProperties,
            JSON.stringify({ userGroup: UserGroup, Id: DocTypeID }),
            ShowPermissionProperties_Success,
            ShowPermissionProperties_Error);
    }
    function ShowPermissionProperties_Success(data) {
        $(".sub_properties").find(".sub_properties_content").remove();
        $(".sub_properties").append(data);
        enableAllCheckBox();
        $(".sub_properties").ecm_loading_hide();
    }

    function ShowPermissionProperties_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $(".sub_properties").ecm_loading_hide();

    }
    // ================ Kết thúc ShowPermissionProperties ===================== //  
    // Show permission properties
    function ShowPermission() {

        ///Get Ajax
        $(".admin_sub_menu").ecm_loading_show();

        JsonHelper.helper.post(
            URL_ShowPermission,
            JSON.stringify({viewBy:ViewBy}),
            ShowPermission_Success,
            ShowPermission_Error);
    }

    function ShowPermission_Success(data) {
        $(".permissionMenuBar").find(".menu").remove();
        $(".permissionMenuBar").append(data);
        var $permission_item = $(".admin_sub_menu_items").find(".permission_menu_item");
        if ($permission_item.length > 0) {
            $permission_item.first().click();
        }
        $(".admin_sub_menu").ecm_loading_hide();
    }

    function ShowPermission_Error(jqXHR, textStatus, errorThrown) {
        $.EcmAlert(jqXHR, BootstrapDialog.TYPE_WARNING);
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $(".sub_properties").ecm_loading_hide();

    }
    // ================ Kết thúc showpermission ===================== //  
    // Save permission
    function SavePermissions() {

        ///Get Ajax
        $(".sub_properties").ecm_loading_show();

        JsonHelper.helper.post(
            URL_SavePermissions,
            JSON.stringify({ permissionModel: JsonSaveDocPermission,annotationPermissionModel:JsonSaveAnnotation,auditPermissionModel:JsonSaveAudit }),
            SavePermissions_Success,
            SavePermissions_Error);
    }
    function SavePermissions_Success(data) {
        $(".sub_properties").ecm_loading_hide();
    }

    function SavePermissions_Error(jqXHR, textStatus, errorThrown) {
        $.EcmAlert(jqXHR, BootstrapDialog.TYPE_WARNING);
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $(".sub_properties").ecm_loading_hide();

    }
    // end save permission 
    // disable all checkbox
    function disableAllCheckbox()
    {
        $(".permission_checkbox").prop("disabled", true);
    }
    // enable all checkbox 
    function enableAllCheckBox()
    {
        $(".permission_checkbox").prop("disabled", false);
    }
    // remove checked of all checkbox
    function resetCheckBox()
    {
        $(".permission_checkbox").removeAttr("checked");
    }
    // check if add and delete redation is checked
    function checkRedaction()
    {
        if ($("#permission_annotation_redaction_add").is(":checked") && $("#permission_annotation_redaction_delete").is(":checked"))
        {
            $("#permission_annotation_redaction").attr("checked", "checked");
        }
        else
        {
            $("#permission_annotation_redaction").removeAttr("checked");
        }
      
    }
    // check if add and delete and view highlight is checked
    function checkHighLight()
    {
        if ($("#permission_annotation_highlight_add").is(":checked") && $("#permission_annotation_highlight_delete").is(":checked") && $("#permission_annotation_highlight_view").is(":checked"))
        {
            $("#permission_annotation_highlight").attr("checked", "checked");
        }
        else
        {
            $("#permission_annotation_highlight").removeAttr("checked");
        }
    }
    // check if add and delete and view text is checked
    function checkText() {
        if ($("#permission_text_add").is(":checked") && $("#permission_text_delete").is(":checked") && $("#permission_text_view").is(":checked")) {
            $("#permission_text").attr("checked", "checked");
        }
        else {
            $("#permission_text").removeAttr("checked");
        }
    }
    // save documentpermission
    function saveDocument()
    {
        var documentId = $(".permission_document_id").attr("id");
        var allowCapture = false;
        if ($("#permission_document_capture").is(":checked"))
        {
            allowCapture = true;
        }
        var allowedDeletePage = false;
        if ($("#permission_document_delete").is(":checked")) {
            allowedDeletePage = true;
        }
        var allowedAppendPage = false;
        if ($("#permission_document_append").is(":checked")) {
            allowedAppendPage = true;
        }
        var allowedReplacePage = false;
        if ($("#permission_document_replace").is(":checked")) {
            allowedReplacePage = true;
        }
        var allowedSeeRetrictedField=false;
        if ($("#permission_document_view").is(":checked")) {
            allowedSeeRetrictedField = true;
        }
        var allowedUpdateFieldValue=false;
        if ($("#permission_document_update").is(":checked")) {
            allowedUpdateFieldValue = true;
        }
        var alowedPrintDocument=false;
        if ($("#permission_docment_print").is(":checked")) {
            alowedPrintDocument = true;
        }
        var allowedEmailDocument=false;
        if ($("#permission_document_send").is(":checked")) {
            allowedEmailDocument = true;
        }
        var allowedRotatePage=false;
        if ($("#permission_document_rotate").is(":checked")) {
            allowedRotatePage = true;
        }
        var allowedExportFieldValue=false;
        if ($("#permission_document_export").is(":checked")) {
            allowedExportFieldValue = true;
        }        
        var allowedHideAllAnnotation=false;
        if ($("#permission_document_hide").is(":checked")) {
            allowedHideAllAnnotation = true;
        }        
        var allowedSearch = false;
        if ($("#permission_document_search").is(":checked")) {
            allowedSearch = true;
        }
        JsonSaveDocPermission = {Id:documentId, UserGroupId:PermissionUserGroupId,DocTypeId:DocTypeID, AllowedDeletePage: allowedDeletePage, AllowedAppendPage: allowedAppendPage, AllowedReplacePage: allowedReplacePage, AllowedSeeRetrictedField: allowedSeeRetrictedField, AllowedUpdateFieldValue: allowedUpdateFieldValue, AlowedPrintDocument: alowedPrintDocument, AllowedEmailDocument: allowedEmailDocument, AllowedRotatePage: allowedRotatePage, AllowedExportFieldValue: allowedExportFieldValue, AllowedHideAllAnnotation: allowedHideAllAnnotation, AllowedCapture: allowCapture, AllowedSearch: allowedSearch };

       
    }
    // saveAnnotation
    function saveAnnotation()
    {
        var Id = $(".permission_annotation_id").attr("id");

        var allowedSeeText = false;
        if ($("#permission_text_view").is(":checked")) {
            allowedSeeText = true;
        }
        var allowedAddText = false;
        if ($("#permission_text_add").is(":checked")) {
            allowedAddText = true;
        }
        var allowedDeleteText = false;
        if ($("#permission_text_delete").is(":checked")) {
            allowedDeleteText = true;
        }
        var allowedSeeHighlight = false;
        if ($("#permission_annotation_highlight_view").is(":checked")) {
            allowedSeeHighlight = true;
        }
        var allowedAddHighlight = false;
        if ($("#permission_annotation_highlight_add").is(":checked")) {
            allowedAddHighlight = true;
        }
        var allowedDeleteHighlight = false;
        if ($("#permission_annotation_highlight_delete").is(":checked")) {
            allowedDeleteHighlight = true;
        }        
        var allowedAddRedaction = false;
        if ($("#permission_annotation_redaction_add").is(":checked")) {
            allowedAddRedaction = true;
        }
        var allowedDeleteRedaction = false;
        if ($("#permission_annotation_redaction_delete").is(":checked")) {
            allowedDeleteRedaction = true;
        }
        JsonSaveAnnotation = {Id:Id, UserGroupId:PermissionUserGroupId,DocTypeId:DocTypeID, AllowedSeeText: allowedSeeText, AllowedAddText: allowedAddText, AllowedDeleteText: allowedDeleteText, AllowedSeeHighlight: allowedSeeHighlight, AllowedAddHighlight: allowedAddHighlight, AllowedDeleteHighlight: allowedDeleteHighlight, AllowedAddRedaction: allowedAddRedaction, AllowedDeleteRedaction: allowedDeleteRedaction };
    }
    //save Audit permission
    function saveAudit()
    {
        var Id = $(".permission_audit_id").attr("id");
        var allowedAudit = false
        if ($("#permission_audit").is(":checked")) {
            allowedAudit = true;
        }
        var allowedViewLog = false;
        if ($("#permission_audit_viewlog").is(":checked")) {
            allowedViewLog = true;
        }
        var allowedDeleteLog = false;
        if ($("#permission_audit_delte").is(":checked")) {
            allowedDeleteLog = true;
        }
        var allowedViewReport = false;
        if ($("#permission_audit_view").is(":checked")) {
            allowedViewReport = true;
        }
        var allowedRestoreDocument = false;
        if ($("#permission_audit_restore").is(":checked")) {
            allowedRestoreDocument = true;
        }
        JsonSaveAudit = {Id:Id, UserGroupId: PermissionUserGroupId, DocTypeId: DocTypeID, AllowedAudit: allowedAudit, AllowedViewLog: allowedViewLog, AllowedDeleteLog: allowedDeleteLog, AllowedViewReport: allowedViewReport, AllowedRestoreDocument: allowedRestoreDocument };

    }
    // Event save Permissions
    function EventSavePermissions()
    {
        saveDocument();
        saveAnnotation();
        saveAudit();
        SavePermissions();
    }
   
    $(document).ready(function () {
        // sự kiện khi click vào permission trên left panel

        $("#permissions").click(function () {
            $(".admin-menu > li").removeClass("active");
            $(this).addClass("active");

            var $viewOptions = $('<div class="box no-padding">' +
                                '<div class="box-header no-padding">' +
                                    '<div class="btn-group padding-1" style="left:10px">' +
                                        '<button type="button" class="btn bg-olive btn-flat margin-3 disabled" id="permission_view_contenttype" data-value="bycontent"><i class="glyphicon glyphicon-list"></i> By Content Type </button>' +
                                        '<button type="button" class="btn bg-olive btn-flat margin-3" id="permission_view_usergroup" data-value="byusergroup"><i class="ion ion-ios-people-outline"></i> By Group </button>' +
                                    '</div>'+
                                '</div>'+
                                '<div class="box-body no-padding">'+
                                    '<div class="permissionMenuBar">'+
                                    '</div>'+
                                '</div>'+
                            '</div>');


            $(".archive_admin_container").find(".sub_properties_content").remove();
            $(".admin_sub_menu").empty();
            $(".sub_properties").empty();
            $(".sub_properties").css({ display: 'block' });
            $(".admin_sub_menu").css({ display: 'block' });

            $(".admin_sub_menu").append($viewOptions);
            UserGroup = {};
            DocTypeID = -1;
            ViewBy = "bycontent";
            ShowPermission();
            $("#view_by_option").bootstrapToggle();
            $('#view_by_option').change(function () {
                //$('#console-event').html('Toggle: ' + $(this).prop('checked'))
                alert("hello");
            })
        });
        //sự kiện khi check vào view by user group
        $(document).on("click", "#permission_view_contenttype", function () {;
            ViewBy = $(this).data("value");
            $('#permission_view_usergroup').removeClass('disabled');
            $(this).addClass('disabled');
            ShowPermission();

        });

        $(document).on("click", "#permission_view_usergroup", function () {
            ViewBy = $(this).data("value");
            $('#permission_view_contenttype').removeClass('disabled');
            $(this).addClass('disabled');
            ShowPermission();
        });

        $(document).on("click", ".sub_menu_item_by_content_type", function () {
            $(".sub_properties").find(".sub_properties_content").remove();
            //$(".permission_sub_menu").find(".sub_menu_item_by_content_type").removeClass("active");
            //$(this).addClass("active");
            DocTypeID = $(this).parent().parent().parent().attr("id");
            var UserGroupId = $(this).parent().attr("id");
            PermissionUserGroupId = UserGroupId;
            
            UserGroup = { "Id": UserGroupId };
            ShowPermissionProperties();
        });

        $(document).on("click", ".sub_menu_item_by_user_group", function () {
            $(".sub_properties").find(".sub_properties_content").remove();
            var UserGroupId = $(this).parent().attr("id");
            DocTypeID = $(this).attr("id");
            UserGroup = { "Id": UserGroupId };
            IsCheckBoxChange = true;
            PermissionUserGroupId = UserGroupId;
            ShowPermissionProperties();
        });
        // event when you check or uncheck redaction checkbox
        $(document).on("click", "#permission_annotation_redaction", function () {

            if ($("#permission_annotation_redaction").is(":checked")) {
                //alert("check");
                $("#permission_annotation_redaction_add").attr("checked", "checked");
                $("#permission_annotation_redaction_delete").attr("checked", "checked");
            }
            else {
                //alert("uncheck");
                $("#permission_annotation_redaction_add").removeAttr("checked");
                $("#permission_annotation_redaction_delete").removeAttr("checked");
            }

        });

        // event when you check or uncheck highlight checkbox
        $(document).on("click", "#permission_annotation_highlight", function () {

            if ($("#permission_annotation_highlight").is(":checked")) {
                //alert("check");
                $("#permission_annotation_highlight_add").attr("checked", "checked");
                $("#permission_annotation_highlight_delete").attr("checked", "checked");
                $("#permission_annotation_highlight_view").attr("checked", "checked");
            }
            else {
                //alert("uncheck");
                $("#permission_annotation_highlight_add").removeAttr("checked");
                $("#permission_annotation_highlight_delete").removeAttr("checked");
                $("#permission_annotation_highlight_view").removeAttr("checked");
            }

        });
        // event when you check or uncheck text checkbox
        $(document).on("click", "#permission_text", function () {

            if ($("#permission_text").is(":checked")) {
                //alert("check");
                $("#permission_text_add").attr("checked", "checked");
                $("#permission_text_delete").attr("checked", "checked");
                $("#permission_text_view").attr("checked", "checked");
            }
            else {
                //alert("uncheck");
                $("#permission_text_add").removeAttr("checked");
                $("#permission_text_delete").removeAttr("checked");
                $("#permission_text_view").removeAttr("checked");
            }

        });
        // event  when check or uncheck add, delete redaction
        $(document).on("click", "#permission_annotation_redaction_add", function () {
            checkRedaction();
        });
        $(document).on("click", "#permission_annotation_redaction_delete", function () {
            checkRedaction();
        });
        // event when check or uncheck add, delete, view highlight
        $(document).on("click", "#permission_annotation_highlight_add", function () {
            checkHighLight();
        });
        $(document).on("click", "#permission_annotation_highlight_delete", function () {
            checkHighLight();
        });
        $(document).on("click", "#permission_annotation_highlight_view", function () {
            checkHighLight();
        });
        // event when check or uncheck add, delete, view text
        $(document).on("click", "#permission_text_add", function () {
            checkText();
        });
        $(document).on("click", "#permission_text_delete", function () {
            checkText();
        });
        $(document).on("click", "#permission_text_view", function () {
            checkText();
        });
        // event when click save button 
        $(document).on("click", "#permission_button_save", function () {
            EventSavePermissions();
        });

        //event when click close button
        $(document).on("click", "#permission_button_close", function () {
            $(".sub_properties").find(".sub_properties_content").remove();
        })

        $(document).on("click", ".permission_checkbox", function () {
            IsCheckBoxChange = true;
        });
    }); 
});