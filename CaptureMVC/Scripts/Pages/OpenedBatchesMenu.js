// Need to be set value for belows variable in main page
// - closeAll : menu close all batch
// - messageConfirmSave: message confirm save batch when close
// - ecmTitleMessage : title of message dialog confirm save batch when close
// - urlCloseBatches : url of action 'CloseBatches' in controller 'View'
// - urlViewIndex : url of action 'Index' in controller 'View'

function InitOpenedBatchesMenu() {
    /// <signature>
    /// <summary>
    /// Initialize context menu for menu view works (opened batches)
    /// </summary>
    /// </signature>

    var menuView = $('#opened-batch-menu');
    if (menuView.length == 0) {
        return;
    }

    $('#opened-batch-menu').contextMenu({
        build: function ($trigger, e) {

            var batches = $('#opened-batch-menu').data('opened-batches');
            if (batches == undefined) {
                return false;
            }
            var _items = {};

            _items['CloseAll'] = { name: closeAll };
            _items['sep1'] = "---------";
            // Immediately
            // Create view task menu
            for (var i = 0; i < batches.length; i++) {
                _items[batches[i]['Key']] = {
                    name: batches[i]['Value'],
                    iconDelete: iconDelete,
                    hideMenuImmediately: true
                };
            }

            return {
                callback: function (key, options, subAction) {

                    //$(this).contextMenu("show");

                    switch (key) {
                        case 'CloseAll':
                            CloseBatchesWithConfirmSave(key)
                            break;

                        default:
                            if (subAction == 'Delete') {
                                CloseBatchesWithConfirmSave(key)
                                return;
                            }

                            window.location.href = urlViewIndex + "?id=" + key;
                    }
                },
                items: _items,
                position: function (opt, x, y) {
                    var offset = {
                        top: $trigger.position().top + $trigger.outerHeight() + 2,
                        left: $trigger.position().left
                    };
                    var $win = $(window);
                    var right = $win.scrollLeft() + $win.width(),
                        width = opt.$menu.width();

                    if (offset.left + width > right)
                        offset.left -= offset.left + width - right + 3;
                    if (offset.left < 0)
                        offset.left = 0;

                    opt.$menu.css(offset);

                }
            };
        },
        selector: 'a',
        style: {
            height: 30, fontSize: 11
        },
        trigger: 'left'
    });
}

function CloseBatchesWithConfirmSave(key) {
    jConfirm(messageConfirmSave, ecmTitleMessage, function (result) {
        $body.ecm_loading_show();

        if (result === true) {
            CloseBatches(key, true)
        } else {
            CloseBatches(key, false);
        }
    });
}
function CloseBatches(key, isSave, isSubmit) {

    var postData = {
        batchId: '00000000-0000-0000-0000-000000000000',
        closeType: key,
        isSave: isSave
    }

    if (key != 'CloseAll') {
        postData.batchId = key;
    }

    $.ajax({
        url: urlCloseBatches,
        type: 'POST',
        data: JSON.stringify(postData),
        contentType: 'application/json',
        success: function (data, textStatus, jqXHR) {
            if (data != undefined && data.length > 0) {
                $('#opened-batch-menu').data('opened-batches', data)
                $body.ecm_loading_hide();
            } else {
                $('#opened-batch-menu').remove();
                $body.ecm_loading_hide();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            jAlert('Close batch is fail.', ecmTitleMessage)
            $body.ecm_loading_hide();
        }
    });
}

