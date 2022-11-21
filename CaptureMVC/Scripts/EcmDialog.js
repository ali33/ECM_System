(
    function ($) {
        $.EcmDialog = function (options) {

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
            if (options.paraname != undefined && options.paravalue != undefined) {
                $(options.dialog_data).data(options.paraname, options.paravalue).dialog(dialogOptions);
            }
            else {
                $(options.dialog_data).dialog(dialogOptions);
            }
        }
    }
(jQuery));

//Usage:
//Example:
//$.EcmDialog({
//    title: 'example',
//    width: 580,
//    dialog_data: $('#example'),
//    open: example_function,
//    type: 'Ok_Cancel',
//    Ok_Button: example_function,
//    Cancel_Button: example_function
//});

//Edit By @Hai.Hoang - 17/10/2014
// Add view fullscreen button for ui-dialog
(function () {
    $('.ui-dialog-buttonset').find('button').addClass('ui-dialog-fullscreen ' + 'btn ' + ' btn-default');
    var old = $.ui.dialog.prototype._create;
    $.ui.dialog.prototype._create = function (d) {
        old.call(this, d);
        var self = this,
			options = self.options,
			oldHeight = options.height,
			oldWidth = options.width,
			uiDialogTitlebarFull = $(document.createElement('div'))
				.addClass(
					'ui-dialog-fullscreen ' + 'btn ' + ' btn-default' + ' btn-mini'
				)
				.attr('data-perform', 'panel-collapse')
				.toggle(
					function () {
					    self._setOptions({
					        height: window.innerHeight - 10,
					        width: window.innerWidth - 30
					    });
					    self._position('center');
					    return false;
					},
					function () {
					    self._setOptions({
					        height: oldHeight,
					        width: oldWidth
					    });
					    self._position('center');
					    return false;
					}
				)
				.appendTo(self.uiDialogTitlebar),

			uiDialogTitlebarFullText = $('<i></i>')
				.addClass(
					'glyphicon ' +
					' glyphicon-fullscreen'
				)
				.text(options.fullText)
				.appendTo(uiDialogTitlebarFull)

            //@Hai.hoang edit button 
        $('.ui-dialog-buttonpane')
             .find('button:contains("Yes")')
             .addClass('btn-large ' + ' btn-primary');
                 //.append('<i class="glyphicon glyphicon-ok-sign pull-right"></i>');

            $('.ui-dialog-buttonpane')
                .find('button:contains("No")')
                .addClass('btn-large ' + ' btn-primary');
                //.append('<i class="glyphicon glyphicon-minus-sign pull-right"></i>');

            $('.ui-dialog-buttonpane')
                .find('button:contains("Save")')
                .addClass('btn-large ' + ' btn-primary');
                //.append('<i class="glyphicon glyphicon-floppy-disk pull-right"></i>');

            $('.ui-dialog-buttonpane')
               .find('button:contains("OK")')
               .addClass('btn-large ' + ' btn-primary');
               //.prepend('<i class="glyphicon glyphicon-ok pull-right"></i>');

            $('.ui-dialog-buttonpane')
               .find('button:contains("Close")')
               .addClass('btn-large ' + ' btn-primary');
               //.prepend('<i class="glyphicon glyphicon-ban-circle pull-right"></i>');

            $('.ui-dialog-buttonpane')
                .find('button:contains("Cancel")')
                .addClass('btn-large ' + ' btn-primary');
                //.prepend('<i class="glyphicon glyphicon-remove-circle pull-right"></i>');

            $('.ui-dialog-buttonpane')
                .find('button:contains("Delete")')
                .addClass('btn-large ' + ' btn-primary');
                //.prepend('<i class="glyphicon glyphicon-trash pull-right"></i>');
    };
})();
jQuery.fn.toggle = function (fn, fn2) {
    // Don't mess with animation or css toggles
    if (!jQuery.isFunction(fn) || !jQuery.isFunction(fn2)) {
        return oldToggle.apply(this, arguments);
    }
    // migrateWarn("jQuery.fn.toggle(handler, handler...) is deprecated");
    // Save reference to arguments for access in closure
    var args = arguments,
    guid = fn.guid || jQuery.guid++,
    i = 0,
    toggler = function (event) {
        // Figure out which function to execute
        var lastToggle = (jQuery._data(this, "lastToggle" + fn.guid) || 0) % i;
        jQuery._data(this, "lastToggle" + fn.guid, lastToggle + 1);
        // Make sure that clicks stop
        event.preventDefault();
        // and execute the function
        return args[lastToggle].apply(this, arguments) || false;
    };
    // link all the functions, so any of them can unbind this click handler
    toggler.guid = guid;
    while (i < args.length) {
        args[i++].guid = guid;
    }
    return this.click(toggler);
};
//
var modal = document.createElement('div');
$(modal).dialog()

//End Edit @Hai.Hoang 