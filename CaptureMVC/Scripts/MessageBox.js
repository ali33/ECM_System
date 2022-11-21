$(document).ready(function(){
    function MessageBox(title,message,iconUrl) {
        $messageBox = $('<div class="message-box" title="' + title + '"></div>');
        if (icon != undefined) {
            $icon = $('<span class="message-box-icon"><img src="' + iconUrl + '"/></span>');
            $messageBox.append($icon);
        }
        $messageBox.append($('<span class="message-box-content">' + message + "</span>"));
        $('body').append($messageBox);
        $messageBox.dialog({
            modal: true,
            buttons: {
                Ok: function () {
                    $(this).dialog("close");
                }
            }
        });
    }
});
function MessageBox(title, message, iconUrl) {
    $messageBox = $('<div class="message-box" title="' + title + '"></div>');
    if (iconUrl != undefined) {
        $icon = $('<span class="message-box-icon"><img src="' + iconUrl + '"/></span>');
        $messageBox.append($icon);
    }
    $messageBox.append($('<span class="message-box-content">' + message + "</span>"));
    $('body').append($messageBox);
    $messageBox.dialog({
        modal: true,
        buttons: {
            Ok: function () {
                $(this).dialog("close");
            }
        }
    });
}
(function ($) {
    $.messageBox = function (options) {
        var opt = {
            title: 'Message Box',
            iconUrl: '',
            message: 'This is message',
            buttons: {
                OK: function () {
                    $(this).dialog("close");                        
                }
            },
            type: 'notify'
        };
        $.extend(opt, options);
        var $box = $("<div class='message-box' title='" + opt.title + "'></div>");
        var $icon = $('<span class="message-box-icon"></span>');
        if (opt.iconUrl) {
            $icon.append('<img src="' + opt.iconUrl + '"/>');
            $box.append($icon);
        }
        if(opt.message != null)
            $box.append($('<span class="message-box-content">' + opt.message + "</span>"));
        //var $buttons = $("<div clas='ui-dialog-buttonpane'><button class='ui-button'>OK</button> </div>");
        //$box.append($buttons);
        $('body').append($box);
        var dialogOptions = {
            modal: true,
            close: function () {
                $box.remove();
            },
            //width: '100%',
            dialogClass: 'metro',
            //draggable: false,
            resizable: false,
            open: function () {
                $('a.ui-dialog-titlebar-close').remove();
            }
        };
        if (opt.width)
            dialogOptions['width'] = opt.width;
        if (opt.height)
            dialogOptions['height'] = opt.height;
        switch (opt.type) {
            case 'notify':
                //var $this = $(this);
                
                $.each(opt.buttons, function (button, func) {
                    if (!dialogOptions['buttons'])
                        dialogOptions['buttons'] = {};
                    dialogOptions['buttons'][button] = function (e) {
                        func.call(this, e);
                        $(this).dialog("close");
                    }
                });

                //dialogOptions['buttons'] = {
                //    OK: function (e) {
                //        if(opt.OK_Click)
                //            opt.OK_Click.call(this, e);
                //        if (opt.buttons && opt.buttons.OK)
                //            opt.buttons.OK.call(this, e);
                //        $(this).dialog("close");
                //    }
                //}
                //if (opt.buttons.Cancel) {
                //    dialogOptions['buttons']['Cancel'] = function (e) {
                //        if (option.buttons && options.buttons.Cancel)
                //            options.buttons.Cancel.call(this, e);
                //        $(this).dialog("close");
                //    }
                //}
                break;
            case 'input':
                var $input = $("<input type='text'/>");
                    $box.append($input);
                dialogOptions['buttons'] = {
                    OK: function () {
                        var rs = $box.find('input').val();
                        if(opt.OK_Click)
                            opt.OK_Click.call(this, rs);
                        $(this).dialog("close");
                    },
                    Cancel: function () {
                        $(this).dialog("close");
                    }
                };
                break;
            case 'form':
                if (options.formData) {
                    $box.append(options.formData);
                }
                //dialogOptions['buttons'] = {
                //    OK: function () {
                //        if (opt.OK_Click);
                //            opt.OK_Click.call(this);
                //        $(this).dialog('close');
                //    },
                //    Cancel: function () {
                //        $(this).dialog('close');
                //    }
                //}
                break;
        }
        $.each(opt.buttons, function (button, func) {
            if (!dialogOptions['buttons'])
                dialogOptions['buttons'] = {};
            dialogOptions['buttons'][button] = function (e) {
                func.call(this, e);
                $(this).dialog("close");
            }
        });
        $box.dialog(dialogOptions);
    }
}(jQuery));