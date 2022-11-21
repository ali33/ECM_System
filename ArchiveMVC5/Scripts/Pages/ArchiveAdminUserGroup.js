//#########################################################
//#Copyright (C) 2013, MIA Solution. All Rights Reserved
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

    JsonHelper.helper.post(
        URL_LoadUserGroups,
        JSON.stringify(""),
        LoadUserGroups_Success,
        LoadUserGroups_Error);
}
function LoadUserGroups_Success(data) {
    $(".admin_sub_menu").find(".sub_properties_list").remove();
    $(".admin_sub_menu").append(data);

    $(".sub_properties").css({ display: 'block' });
    $(".admin_sub_menu").css({ display: 'block' });

    $(".group_sub_menu_item").first().click();
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

    JsonHelper.helper.post(
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

    JsonHelper.helper.post(
        URL_ShowUserGroupProperties,
        JSON.stringify({ Id: UserGroupID }),
        ShowUserGroupProperties_Success,
        ShowUserGroupProperties_Error);
}
function ShowUserGroupProperties_Success(data) {
    $(".sub_properties").find(".sub_properties_content").remove();
    $(".sub_properties").append(data);
    //resize_vetical_properties_content();//called from effect.js
    //setButtonWidthInFirefox();
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
    JsonHelper.helper.post(
        URL_SaveUserGroup,
        JSON.stringify({ userGruopModel: JsonSaveUserGroup }),
        SaveUserGroup_Success,
        SaveUserGroup_Error);
}
function SaveUserGroup_Success(data) {
    $(".sub_properties").ecm_loading_hide();
    if (data != EmptyId) {
        if (isEditUserGroup == -1) {
            $(".usergroup-MenuBar .menu").find(".group_sub_menu_item").removeClass("active");
            var new_user_group = '<li id="' + data + '" title="' + JsonSaveUserGroup.Name + '" class="group_sub_menu_item active">'
                                    + '<a href="#" class="group_menu_item">' + JsonSaveUserGroup.Name  + '</a>'
                                +'</li>';
            //";
            //    "<div class='sub_menu_item usergroup_container selected' id='" + data + "'>" +
            //        "<div class='usergroup_infor'>" +
            //            "<div class='group_name' title='" + JsonSaveUserGroup.Name + "'>" + JsonSaveUserGroup.Name + "</div>" +
            //            "<div class='group_type' title='@userGroupItem.Type'>" + $("#usergroup_type").val() + "</div>" +
            //        "</div>" +
            //     "</div>";
            $(".usergroup-MenuBar .menu").append(new_user_group);
            $(".usergroup-MenuBar .menu").find(".group_sub_menu_item.active").click();
            //$(".admin_sub_menu_items").append(new_user_group);
            //$(".admin_sub_menu_items").find(".usergroup_container.selected").click();
        } else {
            //var $this_change_up = $(".admin_sub_menu_items").find(".usergroup_container.selected").find(".up_content");
            var $this_change_up=$(".usergroup-MenuBar .menu").find(".group_sub_menu_item.active")
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
    var key = $("#usergroup_name_search").val();
    JsonHelper.helper.post(
        URL_SearchUser,
        JSON.stringify({ Users: listUser, SearchKey: key }),
        SearchUser_Success,
        SearchUser_Error);
}

function SearchUser_Success(data) {
    $('#usergroup_table_user').dataTable({
        "sScrollY": "200px",
        "sScrollYInner": "180px",
        "bScrollCollapse": false,
        "bPaginate": false,
        "bFilter": false,
        "bSearchable": false,
        "bInfo": false,
        "bDestroy": true,
        "sPaginationType": "bootstrap",
        "bSort": false,
    });

    $("#usergroup_table_user").find("tbody").empty();
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
    $("#usergroup_member option").each(function () {
        var userId = $(this).val();
        jsonUserList.push({ Id: userId });
    });

    JsonSaveUserGroup = { Id: UserGroupID, Name: userGroupName, Users: jsonUserList };
    SaveUserGroup();
}

$(document).ready(function () {
    // sự kiện khi click vào usergroup in left panel
    $("#user_groups").click(function () {
        $(".admin-menu > li").removeClass("active");
        $(this).addClass("active");
        LoadUserGroups();
    });
    //sự kiện khi click vào mỗi usergroup
    $(document).on("click", ".group_sub_menu_item", function () {
        $(".group_sub_menu_item").removeClass("active");
        $(this).addClass("active");
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
            var userId = $(this).attr("id");
            if ($(this).find("td").find(".usergroup_checkbox_select_user").is(":checked")) {
                $("#usergroup_member").append($('<option></option>').attr('value', userId).text(userName)); 
            }
        });
        if ($("#usergroup_member option").length > 0) {
            $("#usergroup_member option").first().prop("selected", true);
        }

        $("#user_table_user_group").find("tr.row_usergroup").remove();

        $("#usergroup_table_user").find("tr.usergroup_row_user").remove();
        $("#dialog_usergroup_select_user").modal("hide");
    };

    var dialog_usergroup_select_user_cancel_function = function () {
        $("#usergroup_table_user").find("tr.usergroup_row_user").remove();//xóa các dòng dữ liệu trước khi đóng
        $(this).dialog("close");
    };

    $(document).on("click", "#usergroup_button_adduser", function () {
        listUser = new Array();
        $("#usergroup_member option").each(function () {
            var userId = $(this).val();
            listUser.push({ Id: userId });
        });

        $("#usergroup_table_user").find("tbody").empty();
        $("#dialog_usergroup_select_user").modal("show");
    });
    // serch user to add into usergroup
    $(document).on("click", "#usergroup_search_user", function () {
        SearchUser();
    });
    //cancel add user group
    $(document).on("click", "#usergroup_button_cancel", function () {
        UserGroupID = -1;
        JsonSaveUserGroup = {};
        $(".admin_sub_menu_items").find(".usergroup_container").first().click();
    });
    // delete user of usergroup
    $(document).on("click", "#usergroup_button_deleteuser", function () {
        var $this_user = $("#usergroup_member option:selected");
        if ($this_user.length > 0) {
            BootstrapDialog.confirm({
                title: 'WARNING',
                message: 'Are you sure you want to remove selection?',
                type: BootstrapDialog.TYPE_WARNING,
                closable: true, 
                draggable: true,
                btnCancelLabel: 'Cancel', 
                btnOKLabel: 'OK',
                btnOKClass: 'btn-warning',
                callback: function (result) {
                    if (result) {
                        $this_user.remove();
                        $("#usergroup_member option").first().addClass("selected");
                    }
                }
            });
        } else {
            $.EcmAlert("Select user member!", "WARNNING", BootstrapDialog.TYPE_WARNING);
        }

    });

    $(document).on("click", "#saveUser", function () {
        dialog_usergroup_select_user_ok_function();
    });
});
// end show usergroup