/* ==========================================================
 * CaptureAdminUser.js
 * Copyright (C) 2014 MIA Solution. All Rights Reserved	
 * History:
 * DateTime             Updater             Description
 * 08/10/2014           Hai.Hoang           Create
 * ========================================================== */

// Parameters
var UserID;
var json_save_user;
var EmptyId = "00000000-0000-0000-0000-000000000000";
var ListUserGroup;
var isEditUser = -1;//Edit: 1; Add new: -1
var keyCache;

// Functions
$(function () {
    // Dùng để hiển thị danh sách các user
    function ShowUser() {

        ///Get Ajax
        $("#left-container").ecm_loading_show();
        JsonHelper.helper.post(
            URL_ShowUser,
            JSON.stringify(""),
            ShowUser_Success,
            ShowUser_Error);
    }
    function ShowUser_Success(data) {
        //$(".admin_sub_menu_content").find(".admin_sub_menu_content").remove();
        //$(".admin_sub_menu_content").append(data);
        $("#left-container").find(".left-container-child").remove();
        $("#left-container").append(data);
        $(".user_container").first().click();
        //sub_menu_height();//function from affect.js
        //setButtonWidthInFirefox();
        $("#left-container").ecm_loading_hide();
    }

    function ShowUser_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $("#left-container").ecm_loading_hide();

    }
    // ================ Kết thúc show user  ===================== //  
    // Show user properties 
    function ShowUserProperties() {

        ///Get Ajax
        $("#panel-viewer-wrapper").ecm_loading_show();

        JsonHelper.helper.post(
            URL_ShowUserProperties,
            JSON.stringify({ userID: UserID }),
            ShowUserProperties_Success,
            ShowUserProperties_Error);
    }
    function ShowUserProperties_Success(data) {
        $("#panel-viewer-wrapper").find(".sub_properties_content").remove();
        $("#panel-viewer-wrapper").append(data);
        //resize_vetical_properties_content();//called from effect.js
        //setButtonWidthInFirefox();
        $("#panel-viewer-wrapper").ecm_loading_hide();
    }

    function ShowUserProperties_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $("#panel-viewer-wrapper").ecm_loading_hide();

    }
    // end show user properties  
    // Delete user
    function DeleteUser() {

        ///Get Ajax
        JsonHelper.helper.post(
            URL_DeleteUser,
            JSON.stringify({ Id: UserID }),
            DeleteUser_Success,
            DeleteUser_Error);
    }
    function DeleteUser_Success(data) {
        $("#right-container").find(".sub_properties_content").remove();
        $("#left-container").find(".user_container.selected").remove();
        $(".user_container").first().click();
    }

    function DeleteUser_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $("#right-container").ecm_loading_hide();

    }
    // end delete user  //  
    // save user
    function SaveUser() {

        ///Get Ajax
        $("#right-container").ecm_loading_show();

        JsonHelper.helper.post(
            URL_SaveUser,
            JSON.stringify({ userModel: json_save_user, keyCache: keyCache }),
            SaveUser_Success,
            SaveUser_Error);
    }
    function SaveUser_Success(data) {
        $("#right-container").ecm_loading_hide();
        if (data != EmptyId) {
            if (isEditUser == -1) {
                $(".admin_sub_menu_items").ecm_loading_show();
                var src = $("#right-container").find("#profile_pic").attr("src");
                var new_user =
                    "<div class='sub_menu_item user_container well-nav well-nav-sl clearfix selected' id='" + data + "'>" +
                        "<div class='col-md-4'>" +
                            "<img src='" + src + "'/>" +
                        "</div>" +
                        "<div class='col-md-8'>" +
                            "<div class='pull-left'>" +
                                "<span title='" + json_save_user.Username + "'>" + json_save_user.Username + "</span>" +
                                "<p class='full_name' title='" + json_save_user.Fullname + "'>" + json_save_user.Fullname + "</p>" +
                            "</div>" +
                            "<div class='pull-right'>" +
                                "<div class='language_content' title='" + $('#user_select_language').val() + "'>" + $('#user_select_language').val() + "</div>" +
                            "</div>" +
                        "</div>" +
                    "</div>";

                $(".admin_sub_menu_items").append(new_user);
                $(".admin_sub_menu_items").ecm_loading_hide();
                $(".admin_sub_menu_items").find(".sub_menu_item.selected").click();
            } else {
                var $this_change = $(".admin_sub_menu_items").find(".sub_menu_item.selected");
                $this_change.find(".full_name").text(json_save_user.Fullname);
                $this_change.find(".full_name").attr("title", json_save_user.Fullname);
                $this_change.find(".icon img").attr("src", $("#profile_pic").attr("src"));
            }
        } else {

            $.EcmDialog({
                title: 'Warning information',
                width: 350,
                dialog_data: '<div class="message_infor">' + json_save_user.Username + ' already has existed</div>',
                type: 'Ok',
                Ok_Button: function () {
                    $(this).dialog('close');
                    $("#right-container").find("#user_name").focus();
                }
            });
        }
    }

    function SaveUser_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $("#right-container").ecm_loading_hide();

    }
    // end save user  //  
    // show list usergroup
    function ShowListUserGroup() {

        JsonHelper.helper.post(
            URL_ShowListUserGroup,
            JSON.stringify({ UserGroups: ListUserGroup }),
            ShowListUserGroup_Success,
            ShowListUserGroup_Error);
    }
    function ShowListUserGroup_Success(data) {

        $("#user_table_user_group").find("tr.row_usergroup").remove();
        $("#user_table_user_group").find("tbody").append(data);
    }

    function ShowListUserGroup_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);

    }
    // end show list usergroup  //  
    function ProcessSaveUser() {
        var jsonUserGroups = new Array();

        var userName = $("#user_name").val();
        var userFullName = $("#user_full_name").val();
        var userPassword = $("#user_password").val();
        var userEmailAddress = $("#user_email_address").val();
        var userIsAdmin = false;
        if ($("#user_is_admin").is(":checked")) {
            userIsAdmin = true;
        }
        var userLanguageId = $("#user_select_language").find("option:selected").attr("id");
        $("#user_list_member_of").find(".row_usergroup_area").each(function () {
            var userGroupId = $(this).attr("id");
            jsonUserGroups.push({ Id: userGroupId });
        });
        keyCache = $("#profile_pic").data("keycache");

        json_save_user = { Id: UserID, Username: userName, Fullname: userFullName, Password: userPassword, Email: userEmailAddress, IsAdmin: userIsAdmin, LanguageId: userLanguageId, UserGroups: jsonUserGroups };
        SaveUser();
    }
    //Upload profile picture
    function UploadProfilePic() {
        $("#profile_pic").ecm_loading_show();
        $("#user_form").ajaxSubmit({
            url: URL_UploadProfilePic,
            success: UploadProfilePic_Success,
            error: UploadProfilePic_Error
        })
    }

    function UploadProfilePic_Success(data) {
        $("#profile_pic").attr("src", URL_GetImageFromCache + "?Key=" + data);
        $("#profile_pic").attr("data-keycache", "" + data);
        $(".profile_pic").ecm_loading_hide();
    }

    function UploadProfilePic_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $("#right-container").ecm_loading_hide();
    }

    //End

    $(document).ready(function () {
        //Kích hoạt Bootstrap-select
        $('.selectpicker').selectpicker();
        // Events Onclick user item in admin navigation
        $("#users").click(function () {
            $("#nav-container > nav").find(".nav-item").removeClass("selected");
            $(this).addClass("selected");

            $("#left-container").find(".left-container-child").remove();
            $("#right-container").find(".sub_properties_content").remove();
            $("#right-container").css({ display: 'inline-block' });
            $("#left-container").css({ display: 'inline-block' });
            $(".between_and_right").hide();
            ShowUser();

        });

        // sự kiện khi click vào mỗi user
        $(document).on("click", ".user_container", function () {
            $("#right-container").find(".sub_properties_content").remove();
            $("#left-container").find(".user_container").removeClass("selected");
            $(this).addClass("selected");
            UserID = $("#left-container").find(".user_container.selected").attr("id");
            isEditUser = 1;
            ShowUserProperties();
        });
        // sự kiện khi click  vào add user button
        $(document).on("click", "#user_button_add", function () {
            UserID = EmptyId;
            isEditUser = -1;
            $("#panel-viewer-wrapper").find(".sub_properties_content").remove();
            $("#left-container").find(".left-container-child").removeClass("selected");
            ShowUserProperties();

        });
        // sự kiện khi click vào nút delete user
        var dialog_delete_yes_function = function () {
            DeleteUser();
            $(this).dialog("close");
        };

        var dialog_delete_no_function = function () {
            $(this).dialog("close");
        };

        $(document).on("click", "#user_button_delete", function () {
            $.EcmDialog({
                title: 'Cloud ECM',
                width: 350,
                dialog_data: $('#dialog_delete_user'),
                type: 'Yes_No',
                Yes_Button: dialog_delete_yes_function,
                No_Button: dialog_delete_no_function
            });
        });
        // sự kiện khi click vào nút save user 
        $(document).on("click", "#user_button_save", function () {
            if (CheckUserName($("#user_name"), "User name", 0, 0)) {
                if (CheckField($("#user_full_name"), "Full name", 0, 0)) {
                    if (CheckPassword($("#user_password"), "Password", 0)) {
                        if (CheckValidEmail($("#user_email_address"))) {
                            ProcessSaveUser();
                        }
                    }
                }
            }
        });
        // sự kiện khi click vào nút cacel add user 
        $(document).on("click", "#user_button_cancel", function () {
            // alert("đã click vào ");
            $("#right-container").find(".sub_properties_content").remove();
            $(".admin_sub_menu_items").find(".user_container").first().click();
        });

        //sự kiện khi click vào add group
        var dialog_user_select_group_ok_function = function () {
            $("#user_table_user_group").find('tr.row_usergroup').each(function (index) {
                var userGroupName = $(this).find("td.td_usergroup_name").text();
                var userGroupId = $(this).attr("id");
                if ($(this).find("td").find(".user_checkbox_select_usergroup").is(":checked")) {
                    $("#user_list_member_of").append("<a class='list-group-item row_usergroup_area' id='" + userGroupId + "'><input name='lstUserGroupId' value='" + userGroupId + "' hidden/>" + userGroupName + "</a>");
                }
                //console.log("check" + userGroupName);
            });
            if ($("#user_list_member_of").find(".row_usergroup_area").length > 0) {
                $("#user_list_member_of").find(".row_usergroup_area").first().addClass("selected");
            }
            $("#user_table_user_group").find("tr.row_usergroup").remove();
            $(this).dialog("close");
        };

        var dialog_user_select_group_cancel_function = function () {
            $("#user_table_user_group").find("tr.row_usergroup").remove();
            $(this).dialog("close");
        }

        $(document).on("click", "#user_button_add_group", function () {
            if (!$("#user_is_admin").is(":checked")) {
                ListUserGroup = new Array();
                $("#user_list_member_of").find(".row_usergroup_area").each(function () {
                    var userGroupID = $(this).attr("id");
                    ListUserGroup.push({ Id: userGroupID });

                });

                $.EcmDialog({
                    title: 'Select groups',
                    width: 580,
                    height: 500,
                    dialog_data: $('#dialog_user_select_group'),
                    type: 'Ok_Cancel',
                    Ok_Button: dialog_user_select_group_ok_function,
                    Cancel_Button: dialog_user_select_group_cancel_function
                });
                ShowListUserGroup();
            } else {
                $.EcmDialog({
                    title: 'Warning information',
                    width: 350,
                    dialog_data: '<div class="message_infor">You must not checked "is Admin"</div>',
                    type: 'Ok',
                    Ok_Button: function () {
                        $(this).dialog('close');
                    }
                });
            }

        });
        // sự kiện khi click vào search usergroup
        $(document).on("click", "#user_search_usergroup", function () {
            //alert("đã click search");
            ShowListUserGroup();
        });
        // sự kiện khi click vào các usergroup
        $(document).on("click", ".row_usergroup_area", function () {
            $("#user_list_member_of").find(".row_usergroup_area").removeClass("selected");
            $(this).addClass("selected");
        });

        //delete usergroup 
        $(document).on("click", "#user_button_delete_usergroup", function () {
            var $this_user_group = $("#user_list_member_of").find(".row_usergroup_area.selected");
            if ($this_user_group.length > 0) {
                $this_user_group.remove();
                $("#user_list_member_of").find(".row_usergroup_area").first().addClass("selected");
            } else {
                $.EcmDialog({
                    title: 'Cloud ECM',
                    width: 350,
                    dialog_data: '<div>Select user group!</div>',
                    type: 'Ok',
                    Ok_Button: function () {
                        $(this).dialog("close");
                    }
                });
            }
        });

        //event click edit profile picture
        $(document).on("click", "#edit_picture", function () {
            $("#profile_pic_upload").click();
        });

        //event after select profile picture to upload 
        $(document).on("change", "#profile_pic_upload", function () {
            UploadProfilePic();
        });

    });
});