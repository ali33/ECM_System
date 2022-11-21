(
    function ($) {
        $.EcmAlert = function (message, title, type) {
            BootstrapDialog.alert({
                title: title,
                message: message,
                type: type,//BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
                closable: true, // <-- Default value is false
                draggable: true, // <-- Default value is false
                buttonLabel: 'OK', // <-- Default value is 'OK',
                callback: function (result) {

                }
            });

        }
    }
(jQuery));

