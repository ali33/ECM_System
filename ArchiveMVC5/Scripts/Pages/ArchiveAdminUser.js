//#########################################################
//#Copyright (C) 2013, MIA Solution. All Rights Reserved
//#
//#History:
//# DateTime         Updater         Comment
//# 28/11/2013       Triet Ho        Tao moi

//##################################################################

/*
------------------ Bootstrap Dialog sample
                //BootstrapDialog.show({
                //    type:BootstrapDialog.TYPE_INFO,
                //    title: 'Archive',
                //    message: content,
                //    draggable: true,
                //    buttons: [
                //        {
                //            label: 'OK',
                //            icon: "glyphicon glyphicon-ok",
                //            cssClass: 'btn-flat bg-olive btn-sm',
                //            dismiss:true,
                //            action: function (dialog) {
                //            }
                //        },
                //    {
                //        label: 'Cancel',
                //        icon: 'glyphicon glyphicon-remove',
                //        dismiss:true,
                //        cssClass: 'btn-flat bg-olive btn-sm',
                //        action: function (dialog) {
                //        }
                //    }]
                //});

*/

var UserID;
var json_save_user;
var EmptyId = "00000000-0000-0000-0000-000000000000";
var ListUserGroup;
var isEditUser = -1;//Edit: 1; Add new: -1
var keyCache;

$(function () {
    // Dùng để hiển thị danh sách các user
    function ShowUser() {

        ///Get Ajax
        $(".admin_sub_menu").ecm_loading_show();

        JsonHelper.helper.post(
            URL_ShowUser,
            JSON.stringify(""),
            ShowUser_Success,
            ShowUser_Error);
    }
    function ShowUser_Success(data) {
        $(".admin_sub_menu").find(".sub_properties_list").remove();
        $(".admin_sub_menu").append(data);
        $(".sub_properties").css({ display: 'block' });
        $(".admin_sub_menu").css({ display: 'block' });
        $(".user_sub_menu_item").first().click();
        $(".admin_sub_menu").ecm_loading_hide();
    }

    function ShowUser_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $(".admin_sub_menu").ecm_loading_hide();

    }
    // ================ Kết thúc show user  ===================== //  
    // Show user properties 
    function ShowUserProperties() {

        ///Get Ajax
        $(".sub_properties").ecm_loading_show();

        JsonHelper.helper.post(
            URL_ShowUserProperties,
            JSON.stringify({ userID: UserID }),
            ShowUserProperties_Success,
            ShowUserProperties_Error);
    }
    function ShowUserProperties_Success(data) {
        $(".sub_properties").find(".sub_properties_content").remove();
        $(".sub_properties").append(data);
        $("#user_is_admin").change();
        $(".sub_properties").ecm_loading_hide();
    }

    function ShowUserProperties_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $(".sub_properties").ecm_loading_hide();
    }
    // end show user properties  
    // Delete user
    function DeleteUser() {

        ///Get Ajax
        JsonHelper.helper.post(
            URL_DeleteUser,
            JSON.stringify({ Id : UserID }),
            DeleteUser_Success,
            DeleteUser_Error);
    }
    function DeleteUser_Success(data) {
        $(".sub_properties").find(".sub_properties_content").remove();
        $("#" + UserID + ".user_sub_menu_item").remove();
        $(".user_sub_menu_item").first().click();
    }

    function DeleteUser_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $(".sub_properties").ecm_loading_hide();

    }
    // end delete user  //  
    // save user
    function SaveUser() {

        ///Get Ajax
        $(".sub_properties").ecm_loading_show();

        JsonHelper.helper.post(
            URL_SaveUser,
            JSON.stringify({ userModel: json_save_user, keyCache: keyCache}),
            SaveUser_Success,
            SaveUser_Error);
    }

    function SaveUser_Success(data) {
        $(".sub_properties").ecm_loading_hide();
        var userId = data.Id;
        var errorMessages = data.ErrorMessages;

        $("#user_name").parent().removeClass("has-error");
        $("#user_name").prop("title", "");

        $("#user_email_address").parent().removeClass("has-error");
        $("#user_email_address").prop("title", "");

        if (userId != EmptyId) {
            if (isEditUser == -1) {
                $(".sub_properties_list").ecm_loading_show();
                var src = $(".sub_properties").find("#profile_pic").attr("src");
                var new_user = '<div class="info-box user_sub_menu_item hand" id="' + userId + '">' +
                                '<div class="info-box-icon bg-olive">';

                if (src != undefined) {
                    new_user += '<div class="image">' +
                                    '<img src="' + src + '" width="75" height="75" class="img-circle" style="margin-top:10px"/>' +
                                '</div>';
                }
                else {
                    new_user += '<div class="image"><i class="fa fa-user" style="padding-top: 20px"></i></div>';
                }

                new_user += '</div>' +
                                '<div class="info-box-content">' +
                                    '<div class="info-box-text"><span>' + json_save_user.Username + '</span></div>' +
                                    '<div class="info-box-text"><span>' + json_save_user.Fullname + '</span></div>' +
                                '</div>' +
                            '</div>';

                $(".submenu-list").append(new_user);
                $(".sub_properties_list").ecm_loading_hide();
                $(".submenu-list").find(".user_sub_menu_item").first().click();
            } else {
                var $this_change = $(".submenu-list").find(".user_sub_menu_item.active");
                $this_change.find(".full-name").text(json_save_user.Fullname);
                $this_change.find(".full-name").attr("title", json_save_user.Fullname);
                if ($this_change.find(".image img").length == 0) {
                    var $imageDiv = $this_change.find(".image");
                    var $uploadImg = $('<img width="75" height="75" class="img-circle" style="margin-top:10px"/>');
                    $imageDiv.empty();
                    $uploadImg.attr("src", $("#profile_pic").attr("src"));
                    $imageDiv.append($uploadImg);
                }
                else {
                    $this_change.find(".image img").attr("src", $("#profile_pic").attr("src"));
                }
            }
        }
        else {
            $.each(errorMessages, function (i,item) {
                //var errorModel = item.ErrorMessages;

                if (item.FieldName == "Username") {
                    $("#user_name").parent().addClass("has-error");
                    $("#user_name").prop("title",item.Error);
                }

                if (item.FieldName == "EmailAddress") {
                    $("#user_email_address").parent().addClass("has-error");
                    $("#user_email_address").prop("title",item.Error);
                }

            });
        }
    }

    function SaveUser_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $(".sub_properties").ecm_loading_hide();

    }
    // end save user  //  
    // show list usergroup
    function ShowListUserGroup() {
        ListUserGroup = new Array();
        $('#user_list_member_of option').each(function (i, selected) {
            var userGroupID = $(this).val();
            ListUserGroup.push({ Id: userGroupID });
        });

        var key = $("#user_name_search").val();
        JsonHelper.helper.post(
            URL_ShowListUserGroup,
            JSON.stringify({ UserGroups: ListUserGroup, SearchKey:key }),
            ShowListUserGroup_Success,
            ShowListUserGroup_Error);
    }

    function ShowListUserGroup_Success(data) {

        $('#user_table_user_group').dataTable({
            "sScrollY": "200px",
            "sScrollYInner": "180px",
            "bScrollCollapse": false,
            "bPaginate": false,
            "bFilter": false,
            "bSearchable": false,
            "bDestroy": true,
            "bInfo": false,
            "sPaginationType": "bootstrap",
            "bSort": false,
        });

        $("#user_table_user_group").find("tbody").empty();
        $("#user_table_user_group").find("tbody").append(data);
    }

    function ShowListUserGroup_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);

    }
    // end show list usergroup  //  
    function ProcessSaveUser()
    {
        var jsonUserGroups = new Array();

        var userName = $("#user_name").val();
        var userFullName = $("#user_full_name").val();
        var userPassword = $("#user_password").val();
        var userEmailAddress = $("#user_email_address").val();
        var userIsAdmin = false;
        
        if ($("#user_is_admin").is(":checked"))
        {
            userIsAdmin = true;
        }

        var userLanguageId = $("#user_select_language").find("option:selected").attr("id");
        $("#user_list_member_of option").each(function () {
            var userGroupId = $(this).val();
            jsonUserGroups.push({ Id: userGroupId });
        });
        keyCache = $("#profile_pic").data("keycache");

        json_save_user = { Id: UserID, Username: userName, Fullname: userFullName, Password: userPassword, EmailAddress: userEmailAddress, IsAdmin: userIsAdmin, LanguageId: userLanguageId,UserGroups:jsonUserGroups };
       SaveUser();
    }
    //Loc Ngo
    //Upload profile picture
    function UploadProfilePic() {
        $(".profile_pic").parent().ecm_loading_show();
        $("#user_form").ajaxSubmit({
            url: URL_UploadProfilePic,
            success: UploadProfilePic_Success,
            error: UploadProfilePic_Error
        })
    }

    function UploadProfilePic_Success(data) {
        var $imageDiv = $("#profile_pic").parent();
        var $uploadImg = $('<img data-keycache="' + data + '" class="user-pic" alt="" id="profile_pic"/>');
        $imageDiv.empty();
        $uploadImg.attr("src", URL_GetImageFromCache + "?Key=" + data);
        $imageDiv.append($uploadImg);

        $imageDiv.ecm_loading_hide();
    }

    function UploadProfilePic_Error(jqXHR, textStatus, errorThrown) {
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
        $(".sub_properties").ecm_loading_hide();
    }

    //End Loc Ngo

    $(document).ready(function () {
        
        $("#users").click(function () {
            // $(".admin_sub_menu_content").find(".admin_sub_menu_content").remove();
            $(".admin-menu > li").removeClass("active");
            $(this).addClass("active");
            ShowUser();

        });
        // sự kiện khi click vào mỗi user
        $(document).on("click", ".user_sub_menu_item", function () {
            $(".sub_properties").empty();
            $(".admin_sub_menu").find(".user_sub_menu_item").removeClass("active");
            $(this).addClass("active");
            UserID = $(".admin_sub_menu").find(".user_sub_menu_item.active").attr("id");
            isEditUser = 1;
            ShowUserProperties();
        });
        // sự kiện khi click  vào add user button
        $(document).on("click", "#user_button_add", function () {
            UserID = EmptyId;
            isEditUser = -1;
            $(".sub_properties").find(".sub_properties_content").remove();
            $(".admin_sub_menu").find(".user_container").removeClass("selected");
            ShowUserProperties();
            
        });
        // sự kiện khi click vào nút delete user
        var dialog_delete_yes_function = function () {
            DeleteUser();
        };

        var dialog_delete_no_function = function () {
        };

        $(document).on("click", "#user_button_delete", function () {
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
                        dialog_delete_yes_function();
                    } else {
                        dialog_delete_no_function();
                    }
                }
            });
        });
        // sự kiện khi click vào nút save user 
        $(document).on("click", "#user_button_save", function () {

            $("#user_name").parent().removeClass("has-error");
            $("#user_name").prop("title", "");

            $("#user_password").parent().removeClass("has-error");
            $("#user_password").prop("title", "");

            $("#user_email_address").parent().removeClass("has-error");
            $("#user_email_address").prop("title", "");

            var isValid = true;

            if ($("#user_name").val() == "") {
                $("#user_name").parent().addClass("has-error");
                $("#user_name").prop("title", "User name is required!");
                isValid = false;
            }

            if (!CheckUserName($("#user_name"), "User name", 0, 0)) {
                $("#user_name").parent().addClass("has-error");
                $("#user_name").prop("title", "User name is invalid format!");
                isValid = false;
            }

            if (UserID == EmptyId) {
                if ($("#user_password").val() == "") {
                    $("#user_password").parent().addClass("has-error");
                    $("#user_password").prop("title", "Password is required!");
                    isValid = false;
                }
                else {
                    if (!CheckPassword($("#user_password"), "Password", 0)) {
                        $("#user_password").parent().addClass("has-error");
                        $("#user_password").prop("title", "Password is invalid format!");
                        isValid = false;
                    }
                }
            }
            if ($("#user_email_address").val() == "") {
                $("#user_email_address").parent().addClass("has-error");
                $("#user_email_address").prop("title", "Email address is required!");
                isValid = false;
            }

            if (!CheckValidEmail($("#user_email_address"))) {
                $("#user_email_address").parent().addClass("has-error");
                $("#user_email_address").prop("title", "Email address is invalid format!");
                isValid = false;
            }

            if (!isValid) {
                return false;
            }

            ProcessSaveUser();
        });

        // sự kiện khi click vào nút cacel add user 
        $(document).on("click", "#user_button_cancel", function () {
            $(".sub_properties").find(".sub_properties_content").remove();
            $(".admin_sub_menu_items").find(".user_container").first().click();
        });

        //sự kiện khi click vào add group
        $(document).on("click", "#saveUserGroup", function () {
            dialog_user_select_group_ok_function();
        });

        var dialog_user_select_group_ok_function = function () {
            $("#user_table_user_group").find('tr.row_usergroup').each(function (index) {
                var userGroupName = $(this).find("td.td_usergroup_name").text();
                var userGroupId = $(this).attr("id");
                if ($(this).find("td").find(".user_checkbox_select_usergroup").is(":checked")) {
                    $('#user_list_member_of').append($('<option></option>').attr('value', userGroupId).text(userGroupName));
                }
            });
            if ($("#user_list_member_of option").length > 0) {
                $("#user_list_member_of option").first().prop("selected", true);
            }

            $("#user_table_user_group").find("tr.row_usergroup").remove();
            $('#dialog_user_select_group').modal('hide');
        };

        $(document).on("click", "#user_button_add_group", function () {
            if (!$("#user_is_admin").is(":checked"))
            {
                $("#user_table_user_group").find("tbody").empty();
                $('#dialog_user_select_group').modal('show');


            } else {
                $.EcmAlert("Selected user is administrator. Cannot assign this user to user group", 'WARNING', BootstrapDialog.TYPE_WARNING);
            }
           
        });
        // sự kiện khi click vào search usergroup
        $(document).on("click", "#user_search_usergroup", function () {
            ShowListUserGroup();
        });
        // sự kiện khi click vào các usergroup
        $(document).on("click", ".row_usergroup_area", function () {
            $("#user_list_member_of").find(".row_usergroup_area").removeClass("selected");
            $(this).addClass("selected");
        });

        //delete usergroup 
        $(document).on("click", "#user_button_delete_usergroup", function () {
            var $this_user_group = $("#user_list_member_of option:selected")
            if ($this_user_group.length > 0) {
                BootstrapDialog.confirm({
                    title: 'WARNING',
                    message: 'Are you sure you want to remove selection?',
                    type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
                    closable: true, // <-- Default value is false
                    draggable: true, // <-- Default value is false
                    btnCancelLabel: 'Cancel', // <-- Default value is 'Cancel',
                    btnOKLabel: 'OK', // <-- Default value is 'OK',
                    btnOKClass: 'btn-warning', // <-- If you didn't specify it, dialog type will be used,
                    callback: function (result) {
                        // result will be true if button was click, while it will be false if users close the dialog directly.
                        if (result) {
                            $this_user_group.remove();
                            $("#user_list_member_of option").first().prop("selected", true);
                        } 
                    }
                });
            } else {
                $.EcmAlert( 'Please select user group!','WARNING',BootstrapDialog.TYPE_WARNING);
            }
        });

        //Loc Ngo
        //event click edit profile picture
        $(document).on("click", "#edit_picture", function () {
            $("#profile_pic_upload").click();
        });

        //event after select profile picture to upload 
        $(document).on("change", "#profile_pic_upload", function () {
            UploadProfilePic();
        });

        $(document).on("change", "#user_is_admin", function () {
            if ($(this).is(":checked")) {
                $("#user_list_member_of").addClass("disabled");
                $("#user_button_add_group").addClass("disabled");
                $("#user_button_delete_usergroup").addClass("disabled");
            }
            else {
                $("#user_list_member_of").removeClass("disabled");
                $("#user_button_add_group").removeClass("disabled");
                $("#user_button_delete_usergroup").removeClass("disabled");
            }

        });
    });
});