//Inno validate scripts
//Create by Loc Ngo

//Check field empty, min, max lenght
//min_len > 0; min_len=0,max_len=0: no check length
function ErrorDialog(object, message) {
    $.innoDialog({
        title: 'Cloud ECM',
        width: 350,
        dialog_data: '<div class="error_dialog_font">' + message + '</div>',
        type: 'Ok',
        Ok_Button: function () {
            $(this).dialog("close");
            object.focus();
        }
    });
}

//Check empty field, max nad min len
//min_len=0; max_len=0: do not check len
function CheckField(field_object, field_name, min_len, max_len){
    var len = field_object.val().length;
    if (len == 0) {
        ErrorDialog(field_object, field_name + " must be not empty!");
        return false;
    }
    if(min_len!=0)
        if (len < min_len) {
            ErrorDialog(field_object, field_name + " must be more than " + min_len + "character!");
            return false;
        }
    if(max_len!=0)
        if (len > max_len) {
            ErrorDialog(field_object, field_name + " must be less than " + max_len + "character!");
            return false;
        }
    return true;
}

//Check format string
function CheckRegexp(obj, regexp) {
    if (!regexp.test(obj.val())) {
        return false;
    } else {
        return true;
    }
}

//Check format char
function CheckCharRegexp(char_val, regexp) {
    if (!regexp.test(char_val))
        return false;
    else
        return true;
}

//Check user name
function CheckUserName(user_name_object, field_name, min_len, max_len) {
    if (CheckField(user_name_object, field_name, min_len, max_len)) {
        var user_name_val = user_name_object.val();
        if (user_name_val.search(' ') != -1) {
            ErrorDialog(user_name_object, field_name + " must have not one or more spaces!");
            return false;
        }
        var regexp = /([A-Z-.a-z_0-9])/;

        var user_name_len = user_name_val.length;
        for (var i = 0; i < user_name_len; i++) {
            if (!CheckCharRegexp(user_name_val.charAt(i), regexp)) {
                ErrorDialog(user_name_object, field_name + " must have not special characters!");
                return false;
            }
        }
    } else {
        return false;
    }
    return true;
}

//Check email
function CheckValidEmail(email_object) {
    var regexp = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
    if (!CheckRegexp(email_object, regexp)) {
        ErrorDialog(email_object, "Invalid email address!")
        return false;
    }
    return true;
}

//Check password
function CheckPassword(password_object, pass_name ,min_len, max_len) {
    var pass_len = password_object.val().length;
    if (pass_len == 0) {
        ErrorDialog(password_object, pass_name + " must be not empty!");
        return false;
    }
    if (min_len != 0)
        if (pass_len < min_len) {
            ErrorDialog(password_object, pass_name + " must be more than " + min_len + " character!");
            return false;
        }
    if (max_len != 0) {
        if (pass_len > max_len) {
            ErrorDialog(password_object, pass_name + " must be less than " + max_len + " character!");
        }
    }
    return true;
}

//Check confirm password
function CheckConfirmPassword(password_object, confirm_object) {
    var pass_val = password_object.val().toString();
    var confirm_val = confirm_object.val().toString();
    if (pass_val != confirm_val) {
        ErrorDialog(confirm_object, "Confirm password is invalid!");
        return false;
    }
    return true;
}

//Check integer numberic
function CheckIntegerNumber(number_object, number_name){
    var regexp = /^\d+$/;
    if (!CheckRegexp(number_object, regexp)) {
        ErrorDialog(number_object, "Invalid integer number!")
        return false;
    } else {
        if (number_object.val() == 0) {
            ErrorDialog(number_object, number_name + " must be greater than 0!");
            return false;
        }
    }
    return true;
}

//Check decimal numberic
function CheckDecimalNumber(number_object, number_name) {
    var regexp = /^\d+(?:\.\d{1,1})?$/;
    if (!CheckRegexp(number_object, regexp)) {
        ErrorDialog(number_object, "Invalid decimal number!")
        return false;
    } else {
        if (number_object.val() == 0) {
            ErrorDialog(number_object, number_name + " must be greater than 0!");
            return false;
        }
    }
    return true;
}

