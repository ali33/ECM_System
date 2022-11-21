(
    function ($) {
        $.innoDialog = function (options) {
            
            var dialogOptions = {
                modal: true,
                resizable: false,
                dialogClass: 'metro',
                autoReposition: true,
            };
            
            dialogOptions['title'] = options.title;

            if (options.height) {
                dialogOptions['height'] = options.height;
            }

            if (options.width) {
                dialogOptions['width'] = options.width;
            }

            if (options.open) {
                dialogOptions['open'] = options.open;
            }

            switch(options.type){
                case 'Yes_No':
                    dialogOptions['buttons'] = {
                        Yes: function () {
                            if (options.Yes_Button) {
                                options.Yes_Button.call(this);
                            }
                        },
                        No: function () {
                            if (options.No_Button) {
                                options.No_Button.call(this);
                            }
                        }
                    };
                    break;
                case 'Ok_Cancel':
                    dialogOptions['buttons'] = {
                        OK: function () {
                            if(options.Ok_Button){
                                options.Ok_Button.call(this);
                            }
                        },
                        Cancel: function () {
                            if (options.Cancel_Button) {
                                options.Cancel_Button.call(this);
                            }
                        }
                    };
                    break;
                case 'Save_Cancel':
                    dialogOptions['buttons'] = {
                        Save: function () {
                            if (options.Save_Button) {
                                options.Save_Button.call(this);
                            }
                        },
                        Cancel: function () {
                            if (options.Cancel_Button) {
                                options.Cancel_Button.call(this);
                            }
                        }
                    }
                    break;
                case 'Ok':
                    dialogOptions['buttons'] = {
                        OK: function () {
                            if (options.Ok_Button) {
                                options.Ok_Button.call(this);
                            }
                        }
                    }
                    break;
                case 'Close':
                    dialogOptions['buttons'] = {
                        Close: function () {
                            if (options.Close_Button) {
                                options.Close_Button.call(this);
                            }
                        }
                    }
                    break;
            }

            //Create dialog
            $(options.dialog_data).dialog(dialogOptions);
        }
    }
(jQuery));

//Usage:
//Example:
//$.innoDialog({
//    title: 'example',
//    width: 580,
//    dialog_data: $('#example'),
//    open: example_function,
//    type: 'Ok_Cancel',
//    Ok_Button: example_function,
//    Cancel_Button: example_function
//});