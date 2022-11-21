//Inno validate scripts
//Create by Loc Ngo

//Check field empty, min, max lenght
//min_len > 0; min_len=0,max_len=0: no check length
function ErrorDialog(object, message) {
    BootstrapDialog.alert({
        title: 'WARNING',
        message: message,
        type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
        closable: true, // <-- Default value is false
        draggable: true, // <-- Default value is false
        buttonLabel: 'OK', // <-- Default value is 'OK',
        callback: function (result) {
            object.focus();
        }
    });
}

//Check empty field, max nad min len
//min_len=0; max_len=0: do not check len
function CheckField(field_object, field_name){
    var len = field_object.val().length;
    if (len == 0) {
        return false;
    }
    return true;
}

function CheckFieldLength(field_object, min_len, max_len) {
    if (min_len != 0) {
        if (len < min_len) {
            BootstrapDialog.alert({
                title: 'WARNING',
                message: field_name + ' must be more than ' + min_len + 'character!',
                type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
                closable: true, // <-- Default value is false
                draggable: true, // <-- Default value is false
                buttonLabel: 'OK', // <-- Default value is 'OK',
                callback: function (result) {
                }
            });
            return false;
        }
    }
    if (max_len != 0) {
        if (len > max_len) {
            BootstrapDialog.alert({
                title: 'WARNING',
                message: field_name + ' must be less than ' + max_len + 'character!',
                type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
                closable: true, // <-- Default value is false
                draggable: true, // <-- Default value is false
                buttonLabel: 'OK', // <-- Default value is 'OK',
                callback: function (result) {
                }
            });
            return false;
        }
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
            BootstrapDialog.alert({
                title: 'WARNING',
                message: field_name + ' must have not one or more spaces!',
                type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
                closable: true, // <-- Default value is false
                draggable: true, // <-- Default value is false
                buttonLabel: 'OK', // <-- Default value is 'OK',
                callback: function (result) {
                    user_name_object.focus();
                }
            });

            return false;
        }
        var regexp = /([A-Z-.a-z_0-9])/;

        var user_name_len = user_name_val.length;
        for (var i = 0; i < user_name_len; i++) {
            if (!CheckCharRegexp(user_name_val.charAt(i), regexp)) {
                BootstrapDialog.alert({
                    title: 'WARNING',
                    message: field_name + ' must have not special characters!',
                    type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
                    closable: true, // <-- Default value is false
                    draggable: true, // <-- Default value is false
                    buttonLabel: 'OK', // <-- Default value is 'OK',
                    callback: function (result) {
                        user_name_object.focus();
                    }
                });

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
        //BootstrapDialog.alert({
        //    title: 'WARNING',
        //    message: 'Invalid email address!',
        //    type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
        //    closable: true, // <-- Default value is false
        //    draggable: true, // <-- Default value is false
        //    buttonLabel: 'OK', // <-- Default value is 'OK',
        //    callback: function (result) {
        //        email_object.focus();
        //    }
        //});
        return false;
    }
    return true;
}

//Check password
function CheckPassword(password_object, pass_name ,min_len, max_len) {
    var pass_len = password_object.val().length;
    if (min_len != 0)
        if (pass_len < min_len) {
            BootstrapDialog.alert({
                title: 'WARNING',
                message: pass_name + ' must be more than ' + min_len + ' character!',
                type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
                closable: true, // <-- Default value is false
                draggable: true, // <-- Default value is false
                buttonLabel: 'OK', // <-- Default value is 'OK',
                callback: function (result) {
                    password_object.focus();
                }
            });

            return false;
        }
    if (max_len != 0) {
        if (pass_len > max_len) {
            BootstrapDialog.alert({
                title: 'WARNING',
                message: pass_name + ' must be less than ' + max_len + ' character!',
                type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
                closable: true, // <-- Default value is false
                draggable: true, // <-- Default value is false
                buttonLabel: 'OK', // <-- Default value is 'OK',
                callback: function (result) {
                    password_object.focus();
                }
            });

        }
    }
    return true;
}

//Check confirm password
function CheckConfirmPassword(password_object, confirm_object) {
    var pass_val = password_object.val().toString();
    var confirm_val = confirm_object.val().toString();
    if (pass_val != confirm_val) {
        //BootstrapDialog.alert({
        //    title: 'WARNING',
        //    message: 'Confirm password is invalid!',
        //    type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
        //    closable: true, // <-- Default value is false
        //    draggable: true, // <-- Default value is false
        //    buttonLabel: 'OK', // <-- Default value is 'OK',
        //    callback: function (result) {
        //        confirm_object.focus();
        //    }
        //});
        return false;
    }
    return true;
}

//Check integer numberic
function CheckIntegerNumber(number_object, number_name){
    var regexp = /^\d+$/;
    if (!CheckRegexp(number_object, regexp)) {
        //BootstrapDialog.alert({
        //    title: 'WARNING',
        //    message: number_name + ' is not integer number!',
        //    type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
        //    closable: true, // <-- Default value is false
        //    draggable: true, // <-- Default value is false
        //    buttonLabel: 'OK', // <-- Default value is 'OK',
        //    callback: function (result) {
        //        number_object.focus();
        //    }
        //});
        return false;
    }
    //else {
    //    if (number_object.val() == 0) {
    //        BootstrapDialog.alert({
    //            title: 'WARNING',
    //            message: number_name + ' must be greater than 0!',
    //            type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
    //            closable: true, // <-- Default value is false
    //            draggable: true, // <-- Default value is false
    //            buttonLabel: 'OK', // <-- Default value is 'OK',
    //            callback: function (result) {
    //                number_object.focus();
    //            }
    //        });
    //        return false;
    //    }
    //}
    return true;
}

//Check decimal numberic
function CheckDecimalNumber(number_object, number_name) {
    var regexp = /^\d+(?:\.\d{1,1})?$/;
    if (!CheckRegexp(number_object, regexp)) {
        //BootstrapDialog.alert({
        //    title: 'WARNING',
        //    message: number_name+ ' is not decimal number',
        //    type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
        //    closable: true, // <-- Default value is false
        //    draggable: true, // <-- Default value is false
        //    buttonLabel: 'OK', // <-- Default value is 'OK',
        //    callback: function (result) {
        //        number_object.focus();
        //    }
        //});
        return false;
    }
    //else {
    //    if (number_object.val() == 0) {
    //        BootstrapDialog.alert({
    //            title: 'WARNING',
    //            message: number_name + ' must be greater than 0!',
    //            type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
    //            closable: true, // <-- Default value is false
    //            draggable: true, // <-- Default value is false
    //            buttonLabel: 'OK', // <-- Default value is 'OK',
    //            callback: function (result) {
    //                number_object.focus();
    //            }
    //        });
    //        return false;
    //    }
    //}
    return true;
}

