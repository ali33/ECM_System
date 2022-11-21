/* ==========================================================
 * CaptureAdmin.js
 * Copyright (C) 2014 MIA Solution. All Rights Reserved	
 * History:
 * DateTime             Updater             Description
 * 08/10/2014           Hai.Hoang           Create
 * ========================================================== */

$(document).ready(function () {

    // OUTER-LAYOUT
    $('#fix-content').layout({
        applyDefaultStyles: true,

        // Admin Navigation
        west__paneSelector: "#nav-container",
        west__size: 250,
        west__minSize: 50,
        west__maxSize: 250,
        west__closable: true,
        west__resizable: true,
        west__slidable: true,
        west__spacing_open: 5,
        east__spacing_closed: 5,

        // Main Container
        center__paneSelector: "#main-container",
        center__childOptions: {
            applyDefaultStyles: true,

            // Left Container
            west__paneSelector: "#left-container",
            west__slidable: false,
            west__closable: true,
            west__resizable: true,
            west__slidable: true,
            west__size: 260,
            west__minSize: 0,
            west__maxSize: 1000,
            west__spacing_open: 5,
            west__spacing_closed: 5,

            // Right Container
            center__paneSelector: "#right-container",
            center__childOptions: {
                applyDefaultStyles: true,
                center__paneSelector: '#panel-viewer-wrapper'
            },
        },

        

    });

    //// OCR TEMPLATE-LAYOUT
    //$('#ocr-template-panel').layout({
    //    applyDefaultStyles: true,

    //    // Admin Navigation
    //    west__paneSelector: "#left-ocrtemplate",
    //    west__size: 250,
    //    west__minSize: 50,
    //    west__maxSize: 250,
    //    west__closable: true,
    //    west__resizable: true,
    //    west__slidable: true,
    //    west__spacing_open: 5,
    //    east__spacing_closed: 5,
    //    // Main  OCR TEMPLATE
    //    center__paneSelector: "#main-ocrtemplate",
    //    center__childOptions: {
    //        applyDefaultStyles: true,

    //        //  OCR TEMPLATE Viewer
    //        center__paneSelector: "#ocr_viewer",
    //        center__childOptions: {
    //            applyDefaultStyles: true,
    //            center__paneSelector: '#panel-viewer-ocrtemplate'
    //        },

    //        //  OCR TEMPLATE Tool
    //        west__paneSelector: "#ocr_tool",
    //        west__slidable: false,
    //        west__closable: true,
    //        west__resizable: true,
    //        west__slidable: true,
    //        west__size: 60,
    //        west__minSize: 50,
    //        west__maxSize: 60,
    //        west__spacing_open: 5,
    //        west__spacing_closed: 5,
    //    },
    //});
    $('#nav-container').addClass('navbar-minimalize');

    // Add handler to open or close panel thumbnails groups
    //$('#nav-container > .nav-title > .bnt-nav-closable').click(function () {
    //    $('#nav-container').remove();

    //    $('#nav-container > .nav-title > .bnt-nav-closable').show();
    //});
});

/* ==========================================================
 * CaptureAdminUser.js
 * Copyright (C) 2014 MIA Solution. All Rights Reserved	
 * History:
 * DateTime             Updater             Description
 * 08/10/2014           Hai.Hoang           Create
 * ========================================================== */

var EmptyId = "00000000-0000-0000-0000-000000000000";
var UserGroupID = -1;
var JsonSaveUserGroup;
var listUser;
var isEditUserGroup = -1;//Edit: 1, add new: -1
// Dùng để hiển thị danh sách các usergroup
// Functions
$(function () {
    function LoadUserGroups() {

        ///Get Ajax
        $("#left-container").ecm_loading_show();

        JsonHelper.helper.post(
            URL_LoadUserGroups,
            JSON.stringify(""),
            LoadUserGroups_Success,
            LoadUserGroups_Error);
    }
    function LoadUserGroups_Success(data) {
        // $(".admin_sub_menu_content").find(".admin_sub_menu_content").remove();
        // $(".admin_sub_menu_content").append(data);
        $("#left-container").find(".left-container-child").remove();
        $("#left-container").append(data);
        $(".usergroup_container").first().click();
        //sub_menu_height();//function from affect.js
        //setButtonWidthInFirefox();
        $("#left-container").ecm_loading_hide();
    }

    function LoadUserGroups_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $("#left-container").ecm_loading_hide();

    }
    // end load usergroup

    // Delete usergroup
    function DeleteUserGroup() {

        ///Get Ajax
        $("#left-container").ecm_loading_show();

        JsonHelper.helper.post(
            URL_DeleteUserGroup,
            JSON.stringify({ Id: UserGroupID }),
            DeleteUserGroup_Success,
            DeleteUserGroup_Error);
    }
    function DeleteUserGroup_Success(data) {
        $(".sub_properties").find(".sub_properties_content").remove();
        $(".usergroup_container.selected").remove();
        $("#left-container").ecm_loading_hide();
        $("#left-container").find(".usergroup_container").first().click();

    }

    function DeleteUserGroup_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $("#left-container").ecm_loading_hide();

    }
    // end delete usergroup
    // Dùng để show usergroup properties
    function ShowUserGroupProperties() {

        ///Get Ajax
        $("#panel-viewer-wrapper").ecm_loading_show();

        JsonHelper.helper.post(
            URL_ShowUserGroupProperties,
            JSON.stringify({ Id: UserGroupID }),
            ShowUserGroupProperties_Success,
            ShowUserGroupProperties_Error);
    }
    function ShowUserGroupProperties_Success(data) {
        $("#panel-viewer-wrapper").find(".sub_properties_content").remove();
        $("#panel-viewer-wrapper").append(data);
        //resize_vetical_properties_content();//called from effect.js
        //setButtonWidthInFirefox();
        $("#panel-viewer-wrapper").ecm_loading_hide();
    }

    function ShowUserGroupProperties_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $("#panel-viewer-wrapper").ecm_loading_hide();

    }
    // end show usergroup properties
    // save usergroup
    function SaveUserGroup() {

        ///Get Ajax
        $(".sub_properties").ecm_loading_show();
        JsonSaveUserGroup.Name = (JsonSaveUserGroup.Name + "").replace(/\s+/g, ' ').trim();
        JsonHelper.helper.post(
            URL_SaveUserGroup,
            JSON.stringify({ userGruopModel: JsonSaveUserGroup }),
            SaveUserGroup_Success,
            SaveUserGroup_Error);
    }
    function SaveUserGroup_Success(data) {
        $("#right-container").ecm_loading_hide();
        if (data != EmptyId) {
            if (isEditUserGroup == -1) {
                var new_user_group =
                    "<div class='sub_menu_item user_item usergroup_container well-nav well-nav-sl clearfix selected' id='" + data + "'>" +
                        "<div class='icon'>" +
                            "<img src='" + URL_UserGroupIcon + "'/>" +
                        "</div>" +
                        "<div class='usergroup_infor'>" +
                                "<div class='up_content' title='" + JsonSaveUserGroup.Name + "'>" + JsonSaveUserGroup.Name + "</div>" +
                                "<div class='down_content' title='@userGroupItem.Type'>" + $("#usergroup_type").val() + "</div>" +
                        "</div>" +
                     "</div>";

                $(".admin_sub_menu_items").append(new_user_group);
                $(".admin_sub_menu_items").find(".usergroup_container.selected").click();
            } else {
                var $this_change_up = $(".admin_sub_menu_items").find(".usergroup_container.selected").find(".up_content");
                $this_change_up.text(JsonSaveUserGroup.Name);
                $this_change_up.attr("title", JsonSaveUserGroup.Name);
                var $this_change_down = $(".admin_sub_menu_items").find(".usergroup_container.selected").find(".down_content");
                $this_change_down.text($(".sub_properties").find("#usergroup_type").val());
                $this_change_down.attr("title", $(".sub_properties").find("#usergroup_type").val());
            }
            JsonSaveUserGroup = {};
        } else {
            $.EcmDialog({
                title: 'Warning information',
                width: 350,
                dialog_data: '<div class="message_infor">' + JsonSaveUserGroup.Name + ' already has existed</div>',
                type: 'Ok',
                Ok_Button: function () {
                    $(this).dialog('close');
                    $(".sub_properties").find("#usergroup_name").focus();
                }
            });
        }
    }

    function SaveUserGroup_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $(".sub_properties").ecm_loading_hide();

    }
    // end save usergroup
    // search user 
    function SearchUser() {

        JsonHelper.helper.post(
            URL_SearchUser,
            JSON.stringify({ Users: listUser }),
            SearchUser_Success,
            SearchUser_Error);
    }
    function SearchUser_Success(data) {
        $("#usergroup_table_user").find("tr.usergroup_row_user").remove();
        $("#usergroup_table_user").find("tbody").append(data);
    }

    function SearchUser_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);

    }
    // end search user

    function SaveUserGruopProcess() {
        var userGroupName = $("#usergroup_name").val();
        var jsonUserList = new Array();
        $("#usergroup_member").find(".usergroup_user_row").each(function () {
            var userId = $(this).attr("id");
            jsonUserList.push({ Id: userId });
        });
        console.log(jsonUserList);
        JsonSaveUserGroup = { Id: UserGroupID, Name: userGroupName, Users: jsonUserList };
        SaveUserGroup();
    }

    $(document).ready(function () {

        $('.ui-dialog-buttonset').find('button').addClass('ui-dialog-fullscreen ' + 'btn ' + ' btn-default');
        // OUTER-LAYOUT
        $('#fix-content').layout({
            applyDefaultStyles: true,

            // Left bar menu thumbnails, indexes, comment
            west__paneSelector: "#panel-tabs",
            west__size: 34,
            west__closable: false,
            west__resizable: false,
            west__slidable: false,
            west__spacing_open: 0,

            // Panel thumbnails image and center doc viewer
            center__paneSelector: "#thumbnails-n-viewer",



            // Right bar menu control item
            east__paneSelector: "#panel-controls",
            east__closable: true,
            east__resizable: false,
            east__slidable: false,
            east__spacing_open: 0,
            east__spacing_closed: 0,
            east__size: 50
        });
        // sự kiện khi click vào usergroup in left panel
        $("#user_groups").click(function () {
            $("#nav-container > nav").find(".nav-item").removeClass("selected");
            $(this).addClass("selected");

            $("#left-container").find(".left-container-child").remove();
            $("#right-container").find(".sub_properties_content").remove();
            $("#right-container").css({ display: 'inline-block' });
            $("#left-container").css({ display: 'inline-block' });
            $(".between_and_right").hide();
            LoadUserGroups();
        });
        //sự kiện khi click vào mỗi usergroup
        $(document).on("click", ".usergroup_container", function () {
            $(".left-container-child").find(".usergroup_container").removeClass("selected");
            $(this).addClass("selected");
            UserGroupID = $(this).attr("id");
            isEditUserGroup = 1;
            ShowUserGroupProperties();


        });
        // delete user group
        var dialog_delete_usergroup_yes_function = function () {
            DeleteUserGroup();
            $(this).dialog("close");
        };

        var dialog_delete_usergroup_no_function = function () {
            $(this).dialog("close");
        };

        $(document).on("click", "#usergroup_button_delete", function () {
            if (UserGroupID != -1) {
                $.EcmDialog({
                    title: 'Cloud ECM',
                    width: 350,
                    dialog_data: $('#dialog_delete_usergroup'),
                    type: 'Yes_No',
                    Yes_Button: dialog_delete_usergroup_yes_function,
                    No_Button: dialog_delete_usergroup_no_function
                });
            }
        });

        // sự kiện khi click chọn vào user 
        $(document).on("click", ".usergroup_user_row", function () {
            $("#usergroup_member").find(".usergroup_user_row").removeClass("selected");
            $(this).addClass("selected");

        });
        // sự kiện khi click vào add new usergroup
        $(document).on("click", "#usergroup_button_add", function () {
            UserGroupID = -1;
            isEditUserGroup = -1;
            $(".admin_sub_menu_items").find(".usergroup_container").removeClass("selected");
            $("#right-container").find(".sub_properties_content").remove();
            ShowUserGroupProperties();
        });
        // Save UserGroup
        $(document).on("click", "#usergroup_button_save", function () {
            if (CheckField($("#usergroup_name"), "Group name", 0, 0)) {
                SaveUserGruopProcess();
            }
        });
        // add user cho user group
        // dialog show to choose usergroup
        var dialog_usergroup_select_user_ok_function = function () {
            $("#usergroup_table_user").find('tr.usergroup_row_user').each(function (index) {
                var userName = $(this).find("td.td_user_name").text();
                var userNameId = $(this).attr("id");
                if ($(this).find("td").find(".usergroup_checkbox_select_user").is(":checked")) {
                    $("#usergroup_member").append("<a class='list-group-item usergroup_user_row' id='" + userNameId + "'>" + userName + "</a>");
                }
                //console.log("check" + userGroupName);
            });
            if ($("#usergroup_member").find(".usergroup_user_row").length > 0) {
                $("#usergroup_member").find(".usergroup_user_row").first().addClass("selected");
            }
            $("#usergroup_table_user").find("tr.usergroup_row_user").remove();//xóa các dòng dữ liệu trước khi đóng
            $(this).dialog("close");
        };

        var dialog_usergroup_select_user_cancel_function = function () {
            $("#usergroup_table_user").find("tr.usergroup_row_user").remove();//xóa các dòng dữ liệu trước khi đóng
            $(this).dialog("close");
        };

        $(document).on("click", "#usergroup_button_adduser", function () {
            listUser = new Array();
            $("#usergroup_member").find(".usergroup_user_row").each(function () {
                var userId = $(this).attr("id");
                listUser.push({ Id: userId });
            });
            $.EcmDialog({
                title: 'Select members',
                width: 580,
                height: 500,
                dialog_data: $('#dialog_usergroup_select_user'),
                type: 'Ok_Cancel',
                Ok_Button: dialog_usergroup_select_user_ok_function,
                Cancel_Button: dialog_usergroup_select_user_cancel_function
            });
            SearchUser();
        });
        // serch user to add into usergroup
        $(document).on("click", "#usergroup_search_user", function () {
            SearchUser();
        });
        //cacle add user group
        $(document).on("click", "#usergroup_button_cancel", function () {
            UserGroupID = -1;
            JsonSaveUserGroup = {};
            $(".admin_sub_menu_items").find(".usergroup_container").first().click();
        });
        // delete user of usergroup
        $(document).on("click", "#usergroup_button_deleteuser", function () {
            var $this_user = $("#usergroup_member").find(".usergroup_user_row.selected");
            if ($this_user.length > 0) {
                $this_user.remove();
                $("#usergroup_member").find(".usergroup_user_row").first().addClass("selected");
            } else {
                $.EcmDialog({
                    title: 'Cloud ECM',
                    width: 350,
                    dialog_data: '<div>Select user member!</div>',
                    type: 'Ok',
                    Ok_Button: function () {
                        $(this).dialog("close");
                    }
                });
            }

        });
    });
});
// end show usergroup









