//#########################################################
//#Copyright (C) 2013, Innoria Solution. All Rights Reserved
//#
//#History:
//# DateTime         Updater         Comment
//# 3/12/2013       Triet Ho        Tao moi

//##################################################################
var EmptyId = "00000000-0000-0000-0000-000000000000";
var UserGroupID = -1;
var JsonSaveUserGroup;
var listUser;
var isEditUserGroup = -1;//Edit: 1, add new: -1
// Dùng để hiển thị danh sách các usergroup
function LoadUserGroups() {

    ///Get Ajax
    $(".admin_sub_menu").ecm_loading_show();

    Inno.helper.post(
        URL_LoadUserGroups,
        JSON.stringify(""),
        LoadUserGroups_Success,
        LoadUserGroups_Error);
}
function LoadUserGroups_Success(data) {
   // $(".admin_sub_menu_content").find(".admin_sub_menu_content").remove();
   // $(".admin_sub_menu_content").append(data);
    $(".admin_sub_menu").find(".admin_sub_menu_content").remove();
    $(".admin_sub_menu").append(data);
    $(".sub_menu_item").first().click();
    sub_menu_height();//function from affect.js
    setButtonWidthInFirefox();
    $(".admin_sub_menu").ecm_loading_hide();
}

function LoadUserGroups_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".admin_sub_menu").ecm_loading_hide();

}
// end load usergroup

// Dùng để delete usergroup
function DeleteUserGroup() {

    ///Get Ajax
    $(".admin_sub_menu").ecm_loading_show();

    Inno.helper.post(
        URL_DeleteUserGroup,
        JSON.stringify({Id:UserGroupID}),
        DeleteUserGroup_Success,
        DeleteUserGroup_Error);
}
function DeleteUserGroup_Success(data) {
    $(".sub_properties").find(".sub_properties_content").remove();
    $(".usergroup_container.selected").remove(); 
    $(".admin_sub_menu").ecm_loading_hide();
    $(".admin_sub_menu").find(".usergroup_container").first().click();

}

function DeleteUserGroup_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".admin_sub_menu").ecm_loading_hide();

}
// end delete usergroup
// Dùng để show usergroup properties
function ShowUserGroupProperties() {

    ///Get Ajax
    $(".sub_properties").ecm_loading_show();

    Inno.helper.post(
        URL_ShowUserGroupProperties,
        JSON.stringify({ Id: UserGroupID }),
        ShowUserGroupProperties_Success,
        ShowUserGroupProperties_Error);
}
function ShowUserGroupProperties_Success(data) {
    $(".sub_properties").find(".sub_properties_content").remove();
    $(".sub_properties").append(data);
    resize_vetical_properties_content();//called from effect.js
    setButtonWidthInFirefox();
    $(".sub_properties").ecm_loading_hide();
}

function ShowUserGroupProperties_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    $(".sub_properties").ecm_loading_hide();

}
// end show usergroup properties
// save usergroup
function SaveUserGroup() {

    ///Get Ajax
    $(".sub_properties").ecm_loading_show();
    JsonSaveUserGroup.Name = (JsonSaveUserGroup.Name + "").replace(/\s+/g, ' ').trim();
    Inno.helper.post(
        URL_SaveUserGroup,
        JSON.stringify({ userGruopModel: JsonSaveUserGroup }),
        SaveUserGroup_Success,
        SaveUserGroup_Error);
}
function SaveUserGroup_Success(data) {
    $(".sub_properties").ecm_loading_hide();
    if (data != EmptyId) {
        if (isEditUserGroup == -1) {
            var new_user_group =
                "<div class='sub_menu_item user_item usergroup_container selected' id='" + data + "'>" +
                    "<div class='icon'>" +
                        "<img src='" + URL_UserGroupIcon + "'/>" +
                    "</div>" +
                    "<div class='sub_item_content_full'>" +
                        "<div class='item_left_content user_groups'>" +
                            "<div class='up_content' title='" + JsonSaveUserGroup.Name + "'>" + JsonSaveUserGroup.Name + "</div>" +
                            "<div class='down_content' title='@userGroupItem.Type'>" + $("#usergroup_type").val() + "</div>" +
                        "</div>" +
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
        $.innoDialog({
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

    Inno.helper.post(
        URL_SearchUser,
        JSON.stringify({Users:listUser}),
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

function SaveUserGruopProcess()
{
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
    // sự kiện khi click vào usergroup in left panel
    $("#user_groups").click(function () {
        //$(".admin_sub_menu_content").find(".admin_sub_menu_content").remove();
        $(".archive_admin_menu").find(".admin_menu_item").removeClass("selected");
        $(this).addClass("selected");

        $(".admin_sub_menu").find(".admin_sub_menu_content").remove();
        $(".sub_properties").find(".sub_properties_content").remove();
        $(".sub_properties").css({ display: 'inline-block' });
        $(".admin_sub_menu").css({ display: 'inline-block' });
        $(".between_and_right").hide();
        LoadUserGroups();
    });
    //sự kiện khi click vào mỗi usergroup
    $(document).on("click", ".usergroup_container", function () {
        $(".admin_sub_menu_content").find(".usergroup_container").removeClass("selected");
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
        if (UserGroupID != -1)
        {
            $.innoDialog({
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
        $(".sub_properties").find(".sub_properties_content").remove();
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
                $("#usergroup_member").append("<div class='usergroup_user_row' id='" + userNameId + "'>" + userName + "</div>");
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
        $.innoDialog({
            title: 'Select members',
            width: 580,
            dialog_data: $('#dialog_usergroup_select_user'),
            type: 'Ok_Cancel',
            Ok_Button: dialog_usergroup_select_user_ok_function,
            Cancel_Button: dialog_usergroup_select_user_cancel_function
        });
    });
    // serch user to add into usergroup
    $(document).on("click", "#usergroup_search_user", function () {
        //alert("đã search user");

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
            $.innoDialog({
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
// end show usergroup