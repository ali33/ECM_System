// Global variable declaration
var _$html,  // jQuery object of HTML document
    _$body,  // jQuery object of body HTML
    _$btnCreate, // jQuery object of button "Craete batch"
    _$btnSave,   // jQuery object of button "Save"
    _$btnSubmit, // jQuery object of button "Submit"
    _$btnPrev,   // jQuery object of button "Previous" index
    _$btnNext,   // jQuery object of button "Next" index
    _$tabThumbnail,  // jQuery object of tab "Thumbnail"
    _$tabBatchIndex, // jQuery object of tab "Batch indexes"
    _$tabDocIndex,   // jQuery object of tab "Indexes"
    _$tabComment,    // jQuery object of tab "Comments"
    _$viewPanel,
    _$viewBatchIntro,    // jQuery object of div "#xr_xri"
    _$togglerControlPanel,       // jQuery object of toggler control panel
    _$togglerTitleIndexPanel,       // jQuery object of toggler title index panel
    _$togglerTotalCommentPanel,  // jQuery object of toggler total comment panel
    _$togglerPrevNextPanel,      // jQuery object of toggler prev and next panel
    _$togglerCommentPanel,       // jQuery object of toggler comment panel
    _$togglerOcrPanel,       // jQuery object of toggler comment ocr image
    _$ulThumb,  // jQuery object of main ul thumbnail
    _$thumbnailPanel,   // jQuery object of panel contains thumbnail, index
    _$titleIndexPanel,   // jQuery object of panel title index
    _$imgOcr,
    _$ocrInnerPanel;

var _clsTabMenu = 'leftbar_lable',
    _clsTogglerClose = 'ui-layout-toggler-closed',
    _clsTogglerOpen = 'ui-layout-toggler-open',
    _clsExpand = 'expand',
    _clsActive = 'active',
    _clsInvalid = 'invalid',
    _clsEmpty = 'empty',
    _clsInputDisable = 'input-disable',
    _clsUIDisable = 'ui-state-disabled',
    _clsBatchIndex = 'batch-index',
    _clsDocIndex = 'doc-index',
    _clsViewBatch = 'view-batch',
    _clsViewDoc = 'view-doc',
    _clsViewPage = 'wrapper-image',
    _clsNotLoad = 'not-load',
    _clsRealImage = 'real-image',
    _clsNativeImage = 'native-image',
    _clsOcr = 'ocr';



var _idBtnUploadBrowser = 'btn-upload-browser';

var _prfxViewBatch = 'view-batch-',
    _prfxViewDoc = 'view-doc-',
    _prfxViewPage = 'view-page-',
    _prfxBatchIndex = 'batch-index-',
    _prfxDocIndex = 'doc-index-';


var _activeBatchId = '',
    _activeDocId = '',
    _activePageId = '',
    _activeType = '',
    _batchTypeId = '',
    _docTypeId = '',
    _insertType = '',
    _syncCountLoadingImage = 0;

var _inputIntegerPattern = '^[+\-]?[0-9]*$',
    _validIntegerPattern = '(^$)|(^0$)|(^[\-]?[1-9][0-9]{0,9}$)',
    _inputDecimalPattern = '^[+\-]?[0-9]*[.,]?[0-9]*$',
    _validDecimalPattern = '(^$)|(^0$)|(^[\-]?[1-9][0-9]{0,9}$)|(^[\-]?[1-9][0-9]{0,9}['
                           + _decimalSeparator + '][0-9]{1,10}$)';

function InitCaptureViewerVariables() {
    /// <signature>
    /// <summary>Initialize value for global variables.</summary>
    /// </signature>

    _$html = $('html');
    _$body = $('body');

    _$btnCreate = $('#btn-create');
    _$btnSave = $('#btn-save');
    _$btnSubmit = $('#btn-submit');
    _$btnPrev = $('#btn-prev');
    _$btnNext = $('#btn-next');

    _$tabThumbnail = $('#tab-thumbnails');
    _$tabBatchIndex = $('#tab-batch-indexes');
    _$tabDocIndex = $('#tab-indexes');
    _$tabComment = $('#tab-comments');

    _$ulThumb = $('#ul-thumb');
    _$viewBatchIntro = $('#xr_xri');
    _$viewPanel = $('#panel-viewer');
    _$thumbnailPanel = $('#panel-thumbnails-inner');
    _$titleIndexPanel = $('#panel-title-index-inner');

    _$imgOcr = $('#img-ocr');
    _$ocrInnerPanel = $('#panel-ocr-inner');
}
function InitCaptureViewerLayout(captureMenuControlHeight) {
    /// <signature>
    /// <summary>Initialize the layout of page.</summary>
    /// </signature>

    _$body.ecm_loading_show({ 'background-color': 'white' });

    // OUTER-LAYOUT
    $('#fix-content').layout({
        applyDefaultStyles: true,

        // Left bar menu thumbnails, indexes, comment
        west__paneSelector: "#panel-tabs",
        west__size: 34,
        west__closable: false,
        west__resizable: false,
        west__slidable: false,
        west__spacing_open: 0,

        // Panel thumbnails image and center doc viewer
        center__paneSelector: "#thumbnails-n-viewer",
        center__childOptions: {
            applyDefaultStyles: true,

            west__paneSelector: "#thumbnails-groups",
            west__slidable: false,
            west__minSize: 80,
            west__size: 295,
            west__spacing_open: 2,
            west__spacing_closed: 0,
            west__childOptions: {
                applyDefaultStyles: true,

                // Panel capture menus
                north__paneSelector: "#panel-captures",
                north__size: captureMenuControlHeight,
                north__closable: false,
                north__resizable: false,
                north__slidable: false,
                north__spacing_open: 0,
                north__childOptions: {
                    applyDefaultStyles: false,

                    center__paneSelector: '#panel-captures-left',

                    east__paneSelector: '#panel-captures-right',
                    east__closable: false,
                    east__resizable: false,
                    east__slidable: false,
                    east__spacing_open: 0,
                    east__size: 25
                },

                // Panel thumbnails and 2 button previous-next
                center__paneSelector: "#thumbnails-n-prev-next",
                center__childOptions: {
                    applyDefaultStyles: true,

                    north__paneSelector: '#panel-title-index',
                    north__resizable: false,
                    north__slidable: false,
                    north__initClosed: true,
                    north__spacing_open: 0,
                    north__spacing_closed: 0,

                    center__paneSelector: '#thumbnails-n-ocr',
                    center__childOptions: {
                        applyDefaultStyles: true,

                        center__paneSelector: '#panel-thumbnails',
                        center__onresize_end: function (pName, pElem, pState) {
                            $('#panel-thumbnails-inner').height(pState.innerHeight - 5);
                        },

                        south__paneSelector: '#panel-ocr',
                        south__resizable: false,
                        south__slidable: false,
                        south__initClosed: true,
                        south__spacing_open: 1,
                        south__spacing_closed: 0
                        //south__onopen_end: function () {
                        //    $(idPanelThumbnailInner).scrollTop(posScrollbarDocIndex);
                        //}
                    },

                    south__paneSelector: '#panel-prev-next',
                    south__resizable: false,
                    south__slidable: false,
                    south__initClosed: true,
                    south__spacing_open: 1,
                    south__spacing_closed: 0
                    //south__onopen_end: function () {
                    //    //$(idPanelThumbnailInner).scrollTop(posScrollbarDocIndex);
                    //}
                },

                // Panel create and submit batch
                south__paneSelector: "#panel-submit",
                south__closable: false,
                south__resizable: false,
                south__slidable: false,
                south__spacing_open: 1,
            },

            // Panel document viewer wrapper
            center__paneSelector: "#panel-viewer-wrapper",
            center__childOptions: {
                applyDefaultStyles: true,

                // Div total comment
                north__paneSelector: '#panel-total-comment',
                north__resizable: false,
                north__slidable: false,
                north__initClosed: true,
                north__spacing_open: 1,
                north__spacing_closed: 0,

                // Main viewer
                center__paneSelector: '#panel-viewer',

                // Wrapper input comment
                south__paneSelector: '#panel-comment-input',
                south__size: 150,
                south__resizable: false,
                south__slidable: false,
                south__initClosed: true,
                south__spacing_open: 1,
                south__spacing_closed: 0,
                //south__onopen_end: function () {
                //    $(idPanelViewer).scrollTop(posScrollbarCommentViewer);
                //},
                south__childOptions: {
                    applyDefaultStyles: true,

                    // Input comment text area
                    center__paneSelector: '#panel-comment-input-textarea',

                    // Panel remains character and button save comment
                    east__paneSelector: '#panel-remains-save',
                    east__size: 100,
                    east__resizable: false,
                    east__slidable: false,
                    east__closable: false,
                    east__spacing_open: 0,
                    east__childOptions: {
                        // Remains character
                        north__paneSelector: '#panel-remains-character',
                        north__resizable: false,
                        north__slidable: false,
                        north__closable: false,

                        // Preserve
                        center__paneSelector: '#panel-remains-save-center',

                        // Save comment
                        south__paneSelector: '#panel-save-comment',
                        south__resizable: false,
                        south__slidable: false,
                        south__closable: false,
                    }
                }
            }
        },

        // Right bar menu control item
        east__paneSelector: "#panel-controls",
        east__closable: true,
        east__resizable: false,
        east__slidable: false,
        east__spacing_open: 0,
        east__spacing_closed: 0,
        east__size: 50
    });

    // Rotate leftbar_lable
    $('.' + _clsTabMenu).each(function () {
        var $this = $(this),
            outWidth = $this.outerWidth(),
            outHeight = $this.outerHeight();

        $this.outerHeight(outWidth);
        $this.outerWidth(30);
    });

    // Resize height of panel thumbnails, indexes, comments
    $('#panel-tabs-inner').css({ marginTop: captureMenuControlHeight + 'px' })
                          .height(bodyLayout.state.center.innerHeight - captureMenuControlHeight);
    bodyLayout.options.center.onresize_end = function (pName, pElem, pState) {
        $('#panel-tabs-inner').height(pState.innerHeight - captureMenuControlHeight);
    };

    // Resize height of panel thumbnails when initialize
    $('#panel-thumbnails-inner').height($('#panel-thumbnails').height() - 5);

    // Add handler to open or close panel thumbnails groups
    $('#panel-captures-right > .nav_resize').click(function () {
        $('#thumbnails-groups-toggler').trigger('click');
        $('#panel-tabs-inner > .nav_resize').show();
    });
    $('#panel-tabs-inner > .nav_resize').click(function () {
        $('#panel-tabs-inner > .nav_resize').hide();
        $('#thumbnails-groups-toggler').trigger('click');
    });

    _$togglerControlPanel = $('#panel-controls-toggler');
    _$togglerTotalCommentPanel = $('#panel-total-comment-toggler');
    _$togglerCommentPanel = $('#panel-comment-input-toggler');
    _$togglerPrevNextPanel = $('#panel-prev-next-toggler');
    _$togglerTitleIndexPanel = $('#panel-title-index-toggler');
    _$togglerOcrPanel = $('#panel-ocr-toggler');

    _$body.ecm_loading_hide();
}
function InitUploadFile() {
    /// <signature>
    /// <summary>Initialize the upload file..</summary>
    /// </signature>

    // Upload file
    var options = {
        success: InsertCallback, // post-submit callback
        error: ProcessError
    };
    $('#frm-upload').ajaxForm(options);
    $('#btn-upload-browser').change(function () {
        $('body').ecm_loading_show();
        $('#btn-upload').trigger('click');
    });
}
function InsertCallback(responseText, statusText, xhr, $form) {

    switch (_insertType) {
        case 'ImportClassifyLater':
            InsertPageLater(_activeBatchId, responseText);
            break;
        case 'ImportDoc':
            CreateDoc(_activeBatchId, responseText)
            break;
    }

    return;

    var $result = $(responseText);

    var $liActivePage = $('#' + _activePageId);
    var $viewActivePage = $('#view-page-' + _activePageId);
    var $liActiveDoc = $('#' + _activeDocId);

    // User for re-order li page after replace
    var $desUl = $liActivePage.parent();

    // Get thumbnail and viewer upload
    var $liPages = $result.children('ul').children();
    var $viewPages = $result.children('#viewer').children();

    // Get current click page and viewer
    var $currentLi = $liActivePage;
    var $currentView = $viewActivePage;

    var uploadType = $result.children('#upload-type').html();
    if (uploadType == 'PageReplaceImportFile' ||
        uploadType == 'PageReplaceCamera' ||
        uploadType == 'PageReplaceScan') {

        for (var i = 0; i < $liPages.size() ; i++) {
            if (i == 0) {
                // Append to current replace page
                $currentLi = $($liPages[i]).insertAfter($liActivePage);
                $currentView = $($viewPages[i]).insertAfter($viewActivePage);

                // Remove current replace page thumbnail
                $liActivePage.remove();
                $('#view-page-' + activePageId).remove();

                // Update active page id
                activePageId = $currentLi.attr('id');
            } else {
                $currentLi = $($liPages[i]).insertAfter($currentLi);
                $currentView = $($viewPages[i]).insertAfter($currentView);
            }
        }
    } else if (uploadType == 'PageInsertBeforeImportFile' ||
               uploadType == 'PageInsertBeforeCamera' ||
               uploadType == 'PageInsertBeforeScan') {
        for (var i = 0; i < $liPages.size() ; i++) {
            $($liPages[i]).insertBefore($currentLi);
            $($viewPages[i]).insertBefore($currentView);
        }

        activePageId = $($liPages[0]).attr('id');
    } else if (uploadType == 'PageInsertAfterImportFile' ||
               uploadType == 'PageInsertAfterCamera' ||
               uploadType == 'PageInsertAfterScan') {
        for (var i = 0; i < $liPages.size() ; i++) {
            $currentLi = $($liPages[i]).insertAfter($currentLi);
            $currentView = $($viewPages[i]).insertAfter($currentView);
        }

        activePageId = $($liPages[0]).attr('id');
    } else if (uploadType == 'DocAppendImportFile' ||
               uploadType == 'DocAppendCamera' ||
               uploadType == 'DocAppendScan') {

        $currentLi = $liActiveDoc.find('.li-page').last();
        $currentView = $('#view-page-' + $currentLi.attr('id'))

        for (var i = 0; i < $liPages.size() ; i++) {
            $currentLi = $($liPages[i]).insertAfter($currentLi);
            $currentView = $($viewPages[i]).insertAfter($currentView);
        }

        activePageId = $($liPages[0]).attr('id');
    } else if (uploadType == 'BatchAppendImportFile' ||
               uploadType == 'BatchAppendCamera' ||
               uploadType == 'BatchAppendScan') {

        //var $looseDocLi = $('#' + activeBatchId).find('.li-doc-loose-page');
        //activeDocId = $looseDocLi.attr('id');
        var $currentLoosePageUl = $('#' + activeBatchId).find('.ul-page.loose-item');
        var $currentViewLooseDocUl = $('#view-doc-' + activeDocId)

        for (var i = 0; i < $liPages.size() ; i++) {
            $currentLoosePageUl.append($liPages[i]);
            $currentViewLooseDocUl.append($viewPages[i]);
        }

        activePageId = $($liPages[0]).attr('id');
    }

    AddHandlerForLiThumbBatchCache($liPages);
    $liPages.each(function () {
        LoadThumbnailPage($(this), activeBatchId, true);
    });

    // Re-order li page after replace
    var countPages = 0;
    $desUl.children('li').each(function (index, value) {
        countPages++;
        $(value).find('.item-page-index').html(index + 1);
    });
    $desUl.parent().prev().find('.item-doc-count').html(countPages);

    // Update status reject
    var $liBatch = $('#' + activeBatchId);
    var $liDoc = $('#' + activeDocId);
    var $liPage = $('#' + activePageId);

    if ($liDoc.find('.li-page.reject').size() == 0) {
        $liDoc.removeClass('reject').addClass('accept');
    }
    if ($liBatch.find('.li-doc.reject').size() == 0) {
        $liBatch.removeClass('reject').addClass('accept');
    }

    // Trigger click active page
    $('#' + activePageId).children('.item-content').trigger('click');

    //SetStatusPanelSubmit();
    //CheckCanSubmit();
    $('body').ecm_loading_hide();
}

// Region OCR panel
var is_mouse_down_in_ocr,
    ocr_down_x,
    ocr_down_y;
function InitOcrZone() {
    /// <signature>
    /// <summary>Initialize event for panel OCR.</summary>
    /// </signature>

    // Mouse down in OCR panel
    _$ocrInnerPanel.mousedown(function (e) {
        e.preventDefault();
    });

    // Mouse down in OCR image
    _$imgOcr.mousedown(function (e) {
        e.preventDefault();

        if (is_mouse_down_in_ocr)
            return;

        is_mouse_down_in_ocr = true;
        ocr_down_x = e.pageX;
        ocr_down_y = e.pageY;

        // Mouse move when in mode OCR
        _$body.on('mousemove', null, null, OcrMouseMove);
    });

    // Mouse up
    _$body.mouseup(function (e) {
        if (is_mouse_down_in_ocr) {
            is_mouse_down_in_ocr = false;
            _$body.off('mousemove', null, OcrMouseMove);
        }
    });

    _$imgOcr.load(function () {
        // Set ocr image position
        var ocrImageWidth = _$imgOcr[0].clientWidth,
            ocrImageHeight = _$imgOcr[0].clientHeight,
            ocrPanelWidth = _$ocrInnerPanel[0].clientWidth,
            ocrPanelHeight = _$ocrInnerPanel[0].clientHeight;

        if (ocrImageWidth < ocrPanelWidth)
            _$imgOcr.css({ left: ((ocrPanelWidth - ocrImageWidth) / 2) + 'px' });
        else
            _$imgOcr.css({ left: '0px' });

        if (ocrImageHeight < ocrPanelHeight)
            _$imgOcr.css({ top: ((ocrPanelHeight - ocrImageHeight) / 2) + 'px' });
        else
            _$imgOcr.css({ top: '0px' });
    });
}
function OcrMouseMove(e) {
    if (is_mouse_down_in_ocr) {

        var ocr_move_x = e.pageX,
            ocr_move_y = e.pageY,
            imgPos = _$imgOcr.position(),
            ocrPanelWidth = _$ocrInnerPanel[0].clientWidth,
            ocrPanelHeight = _$ocrInnerPanel[0].clientHeight,
            ocrImageWidth = _$imgOcr[0].clientWidth,
            ocrImageHeight = _$imgOcr[0].clientHeight,
            margin = 20,
            newLeft,
            newTop;

        // Coordinate X from left to right
        if (ocr_down_x <= ocr_move_x) {
            newLeft = imgPos.left + ocr_move_x - ocr_down_x;

            if (ocrPanelWidth - newLeft < margin)
                newLeft = ocrPanelWidth - margin;
        }
            // Coordinate X from right to left
        else {
            newLeft = imgPos.left - (ocr_down_x - ocr_move_x);

            if (ocrImageWidth + newLeft < margin)
                newLeft = -(ocrImageWidth - margin);
        }

        // Coordinate Y from top to bottom
        if (ocr_down_y <= ocr_move_y) {
            newTop = imgPos.top + ocr_move_y - ocr_down_y;

            if (ocrPanelHeight - newTop < margin)
                newTop = ocrPanelHeight - margin;
        }
            // Coordinate X from right to left
        else {
            newTop = imgPos.top - (ocr_down_y - ocr_move_y);

            if (ocrImageHeight + newTop < margin)
                newTop = -(ocrImageHeight - margin);
        }

        _$imgOcr.css({ left: newLeft, top: newTop });

        ocr_down_x = ocr_move_x;
        ocr_down_y = ocr_move_y;
    }
}
// End region OCR panel


function ResetFlagsMouseDown() {
    /// <signature>
    /// <summary>Reset all flag determine position of mouse down.</summary>
    /// </signature>

    is_mouse_down_in_ocr = false;
}

function ShowOrHideControlPanel(action) {
    /// <signature>
    /// <summary>Show or hide panel control.</summary>
    /// <param name="action" type="String">Action: "show" or "hide"</param>
    /// </signature>

    if (action == 'show') {
        if (_$togglerControlPanel.hasClass(_clsTogglerOpen))
            return;
    } else if (action == 'hide') {
        if (_$togglerControlPanel.hasClass(_clsTogglerClose))
            return;
    } else
        return;

    _$togglerControlPanel.trigger('click');
}
function ShowOrHideTitleIndexPanel(action) {
    /// <signature>
    /// <summary>Show or hide panel title index.</summary>
    /// <param name="action" type="String">Action: "show" or "hide"</param>
    /// </signature>

    if (action == 'show') {
        if (_$togglerTitleIndexPanel.hasClass(_clsTogglerOpen))
            return;
    } else if (action == 'hide') {
        if (_$togglerTitleIndexPanel.hasClass(_clsTogglerClose))
            return;
    } else
        return;

    _$togglerTitleIndexPanel.trigger('click');
}
function ShowOrHideCommentPanel(action) {
    /// <signature>
    /// <summary>Show or hide panel comment.</summary>
    /// <param name="action" type="String">Action: "show" or "hide"</param>
    /// </signature>

    if (action == 'show') {
        if (_$togglerTotalCommentPanel.hasClass(_clsTogglerOpen))
            return;
    } else if (action == 'hide') {
        if (_$togglerTotalCommentPanel.hasClass(_clsTogglerClose))
            return;
    } else
        return;

    _$togglerTotalCommentPanel.trigger('click');
    _$togglerCommentPanel.trigger('click');
}
function ShowOrHidePrevNextPanel(action, tabKind) {
    /// <signature>
    /// <summary>Show or hide panel prev and next.</summary>
    /// <param name="action" type="String">Action: "show" or "hide"</param>
    /// <param name="tabKind" type="String">"batch" or "doc" index</param>
    /// </signature>

    if (action == 'show') {
        if (_$togglerPrevNextPanel.hasClass(_clsTogglerClose))
            _$togglerPrevNextPanel.trigger('click');
    } else if (action == 'hide') {
        if (_$togglerPrevNextPanel.hasClass(_clsTogglerOpen))
            _$togglerPrevNextPanel.trigger('click');
    } else
        return;

    if (action == 'hide') {
        return;
    }

    // Init status of prev and next button
    if (tabKind == 'batch') {
        var $activeLiBatch = $('#' + _activeBatchId);

        if ($activeLiBatch.prev().length == 0)
            _$btnPrev.addClass(_clsInputDisable);
        else
            _$btnPrev.removeClass(_clsInputDisable);

        if ($activeLiBatch.next().length == 0)
            _$btnNext.addClass(_clsInputDisable);
        else
            _$btnNext.removeClass(_clsInputDisable);
    }
    else if (tabKind == 'doc') {
        var $activeLiDoc = $('#' + _activeDocId);

        if ($activeLiDoc.prev(':not(.loose-item)').length == 0)
            _$btnPrev.addClass(_clsInputDisable);
        else
            _$btnPrev.removeClass(_clsInputDisable);

        if ($activeLiDoc.next().length == 0)
            _$btnNext.addClass(_clsInputDisable);
        else
            _$btnNext.removeClass(_clsInputDisable);
    }
}
function ShowOrHideOcrPanel(action) {
    /// <signature>
    /// <summary>Show or hide panel ocr image.</summary>
    /// <param name="action" type="String">Action: "show" or "hide"</param>
    /// </signature>

    if (action == 'show') {
        if (_$togglerOcrPanel.hasClass(_clsTogglerOpen))
            return;
    } else if (action == 'hide') {
        if (_$togglerOcrPanel.hasClass(_clsTogglerClose))
            return;
    } else
        return;

    _$togglerOcrPanel.trigger('click');
}

function TabThumbnail_Click() {
    /// <signature>
    /// <summary>Click event handler of tab menu thumbnail.</summary>
    /// </signature>

    _$tabThumbnail.click(function () {
        if (_$tabThumbnail.hasClass(_clsInputDisable) || _$tabThumbnail.hasClass(_clsActive)) {
            return;
        }

        // Hide other view in left panel
        $('.' + _clsBatchIndex + '.' + _clsActive).hide();
        $('.' + _clsDocIndex + '.' + _clsActive).hide();

        // Show main view in left panel
        _$ulThumb.show();

        // Set status tab menu
        $('.' + _clsTabMenu + '.' + _clsActive).removeClass(_clsActive);
        _$tabThumbnail.addClass(_clsActive);

        ShowOrHideTitleIndexPanel('hide');
        ShowOrHidePrevNextPanel('hide');
        ShowOrHideOcrPanel('hide');
    });
}
function TabBatchIndex_Click() {
    /// <signature>
    /// <summary>Click event handler of tab menu batch index.</summary>
    /// </signature>

    _$tabBatchIndex.click(function () {
        if (_$tabBatchIndex.hasClass(_clsInputDisable) || _$tabBatchIndex.hasClass(_clsActive)) {
            return;
        }

        // Hide other view in left panel
        _$ulThumb.hide();
        $('.' + _clsBatchIndex + '.' + _clsActive).hide();
        $('.' + _clsDocIndex + '.' + _clsActive).hide();

        // Show main view in left panel
        var $batchIndex = $('#' + _prfxBatchIndex + _activeBatchId).addClass(_clsActive).show();
        _$titleIndexPanel.html($batchIndex.find('.current_content_header').html());

        // Focus first input
        $batchIndex.children('.current_content_fields').children('.content_fields:first')
                   .children('.content_fields_input').children().first().focus();

        // Set status tab menu
        $('.' + _clsTabMenu + '.' + _clsActive).removeClass(_clsActive);
        _$tabBatchIndex.addClass(_clsActive);

        ShowOrHideTitleIndexPanel('show');
        ShowOrHidePrevNextPanel('show', 'batch');
        ShowOrHideOcrPanel('hide');
    });
}
function TabDocIndex_Click() {
    /// <signature>
    /// <summary>Click event handler of tab menu doc index.</summary>
    /// </signature>

    _$tabDocIndex.click(function () {
        if (_$tabDocIndex.hasClass(_clsInputDisable) || _$tabDocIndex.hasClass(_clsActive)) {
            return;
        }

        // Hide other view in left panel
        _$ulThumb.hide();
        $('.' + _clsBatchIndex + '.' + _clsActive).hide();
        $('.' + _clsDocIndex + '.' + _clsActive).hide();

        // Show main view in left panel
        var $docIndex = $('#' + _prfxDocIndex + _activeDocId).addClass(_clsActive).show();
        _$titleIndexPanel.html($docIndex.find('.current_content_header').html());

        // Focus first input
        $docIndex.children('.current_content_fields').children('.content_fields:first')
                 .children('.content_fields_input').children().first().focus();

        if ($docIndex.data('has-ocr') == 'True')
            ShowOrHideOcrPanel('show');
        else
            ShowOrHideOcrPanel('hide');

        // Set status tab menu
        $('.' + _clsTabMenu + '.' + _clsActive).removeClass(_clsActive);
        _$tabDocIndex.addClass(_clsActive);

        ShowOrHideTitleIndexPanel('show');
        ShowOrHidePrevNextPanel('show', 'doc');

    });
}

function BtnPrev_Click() {
    /// <signature>
    /// <summary>Click event handler of button "Previous".</summary>
    /// </signature>

    _$btnPrev.click(function () {

        if (_$btnPrev.hasClass(_clsInputDisable)) {
            return;
        }

        var tabBachtIndexActive = _$tabBatchIndex.hasClass(_clsActive);
        if (tabBachtIndexActive) {
            // Case view batch index is shown
            var $activeLiBatch = $('#' + _activeBatchId);
            var $prevLiBatch = $activeLiBatch.prev();
            console.log('sdf');
            $('#' + _prfxBatchIndex + _activeBatchId).hide();
            $prevLiBatch.children('.item-content').trigger('click');
            var $batchIndex = $('#' + _prfxBatchIndex + _activeBatchId).addClass(_clsActive).show();

            // Focus first input
            $batchIndex.children('.current_content_fields').children('.content_fields:first')
                       .children('.content_fields_input').children().first().focus();

            // Set status button prev and next
            if ($prevLiBatch.prev().length == 0) {
                _$btnPrev.addClass(_clsInputDisable);
            }
            _$btnNext.removeClass(_clsInputDisable);
        } else {
            // Case view doc index is shown
            var $activeLiDoc = $('#' + _activeDocId);
            var $prevLiDoc = $activeLiDoc.prev();

            $('#' + _prfxDocIndex + _activeDocId).hide();
            $prevLiDoc.children('.item-content').trigger('click');
            var $docIndex = $('#' + _prfxDocIndex + _activeDocId).addClass(_clsActive).show();

            // Focus first input
            $docIndex.children('.current_content_fields').children('.content_fields:first')
                      .children('.content_fields_input').children().first().focus();

            // Set status button prev and next
            if ($prevLiDoc.prev(':not(.loose-item)').length == 0) {
                _$btnPrev.addClass(_clsInputDisable);
            }
            _$btnNext.removeClass(_clsInputDisable);

            if ($docIndex.data('has-ocr') == 'True')
                ShowOrHideOcrPanel('show');
            else
                ShowOrHideOcrPanel('hide');
        }
    });
}
function BtnNext_Click() {
    /// <signature>
    /// <summary>Click event handler of button "Previous".</summary>
    /// </signature>

    _$btnNext.click(function () {

        if (_$btnNext.hasClass(_clsInputDisable)) {
            return;
        }

        var tabBachtIndexActive = _$tabBatchIndex.hasClass(_clsActive);
        if (tabBachtIndexActive) {
            // Case view batch index is shown
            var $activeLiBatch = $('#' + _activeBatchId);
            var $nextLiBatch = $activeLiBatch.next();
            console.log('sdf');
            $('#' + _prfxBatchIndex + _activeBatchId).hide();
            $nextLiBatch.children('.item-content').trigger('click');
            var $batchIndex = $('#' + _prfxBatchIndex + _activeBatchId).addClass(_clsActive).show();

            // Focus first input
            $batchIndex.children('.current_content_fields').children('.content_fields:first')
                       .children('.content_fields_input').children().first().focus();

            // Set status button prev and next
            if ($nextLiBatch.next().length == 0) {
                _$btnNext.addClass(_clsInputDisable);
            }
            _$btnPrev.removeClass(_clsInputDisable);
        } else {
            // Case view doc index is shown
            var $activeLiDoc = $('#' + _activeDocId);
            var $nextLiDoc = $activeLiDoc.next();

            $('#' + _prfxDocIndex + _activeDocId).hide();
            $nextLiDoc.children('.item-content').trigger('click');
            var $docIndex = $('#' + _prfxDocIndex + _activeDocId).addClass(_clsActive).show();

            // Focus first input
            $docIndex.children('.current_content_fields').children('.content_fields:first')
                      .children('.content_fields_input').children().first().focus();

            // Set status button prev and next
            if ($nextLiDoc.next().length == 0) {
                _$btnNext.addClass(_clsInputDisable);
            }
            _$btnPrev.removeClass(_clsInputDisable);

            if ($docIndex.data('has-ocr') == 'True')
                ShowOrHideOcrPanel('show');
            else
                ShowOrHideOcrPanel('hide');
        }
    });
}

function SetTheSameWidthButtons($buttons, minWidth) {
    /// <signature>
    /// <summary>Set the width of buttons to the max width of buttons.</summary>
    /// <param name="buttons" type="Array of jQuery object">Array of buttons.</param>
    /// <param name="minWidth" type="Integer">
    /// The width use to set if max width of all buttons are greater than.
    /// </param>
    /// </signature>

    var maxWidth = 0;
    var count = $buttons.length;
    // Get max width in all buttons's width
    for (var i = 0; i < count; i++) {
        var buttonWidth = $buttons[i].width();
        maxWidth = buttonWidth > maxWidth ? buttonWidth : maxWidth;
    }

    // Set max width to default min width
    if (minWidth != undefined || maxWidth < minWidth) {
        maxWidth = minWidth;
    }

    // Set width for all button
    for (var i = 0; i < count; i++) {
        $buttons[i].width(maxWidth);
    }
}
function NewGuid(ids) {
    /// <signature>
    /// <summary>Create new guid.</summary>
    /// <param name="ids" type="String">List guid use to check duplicated.</param>
    /// <returns type="String" />
    /// </signature>

    var newGuid;

    do {
        newGuid = guid().toUpperCase();
    } while ($('#' + newGuid).length > 0);

    if (ids != undefined) {
        var countIds = ids.length;
        for (var i = 0; i < countIds; i++) {
            if (ids[i] == newGuid) {
                newGuid = guid().toUpperCase();
                i = -1;
                continue;
            }
        }
    }

    return newGuid;
}
function CheckExistedId(id) {
    /// <signature>
    /// <summary>Check guid is duplication.</summary>
    /// <returns type="Boolean" />
    /// </signature>

    return $('#' + id).length > 0;
}
/**
 * Generates a GUID string.
 * @returns {String} The generated GUID.
 * @example af8a8416-6e18-a307-bd9c-f2c947bbb3aa
 * @author Slavik Meltser (slavik@meltser.info).
 * @link http://slavik.meltser.info/?p=142
 */
function guid() {
    function _p8(s) {
        var p = (Math.random().toString(16) + "000000000").substr(2, 8);
        return s ? "-" + p.substr(0, 4) + "-" + p.substr(4, 4) : p;
    }
    return _p8() + _p8(true) + _p8(true) + _p8();
}
function ProcessError(message, title) {
    /// <signature>
    /// <summary>Display error message notification.</summary>
    /// </signature>

    var messageShow = _defaultJsErrorMessage;
    var titleShow = _ecmTitleMessage;

    if (message != undefined) {
        messageShow = message
    }
    if (message != undefined) {
        titleShow = title
    }

    jAlert(messageShow, titleShow);
    _$body.ecm_loading_hide();
}

// Function show required red border when not input
function AddHandlerRequired($input) {

    if ($input.val().trim() == '') {
        $input.addClass('empty');
    } else {
        $input.removeClass('empty');
    }

    SetStatusPanelSubmit();
}

function CheckCanSubmit() {
    /// <signature>
    /// <summary>Description.</summary>
    /// </signature>

    // Check invalid index field
    var selectInvalidField = '#panel-thumbnails-inner .index-field.invalid',
        countInvalidField = $(selectInvalidField).length;

    if (countInvalidField > 0) {
        _$btnSubmit.addClass(_clsInputDisable);
        return;
    }

    // Check empty batch
    // Just check count > 1, template-batch is also have class empty
    if ($('.li-batch.empty').length > 1) {
        _$btnSubmit.addClass(_clsInputDisable);
        return;
    }

    _$btnSubmit.removeClass(_clsInputDisable);
}
function Input_Validate($input, options) {
    /// <signature>
    /// <summary>Description.</summary>
    /// </signature>

    $input.tooltip({
        position: { my: "left top+3", at: "left bottom", collision: "flipfit" }
    });

    var onEvent = 'input';
    if (options.type == 'Date') {
        $input.datepicker({
            dateFormat: 'custom-' + options.dateFormatValidation.dateFormat
        });

        onEvent = 'input change';
    } else if (options.type == 'Picklist')
        onEvent = 'change';

    $input.on(onEvent, null, { options: options }, function (event) {
        var re,
            $this = $(this),
            value,
            isValueEmpty,
            options = event.data.options;

        if (options.type == 'Picklist') {
            value = $this[0].selectedIndex;
            isValueEmpty = value < 0;
        } else {
            value = $this.val();
            isValueEmpty = value == undefined || value.trim() == '';
        }

        if (options.type == 'Date') {
            $this.data('date-value', '');
        }

        // Validate required
        if (options.requiredValidation != null && isValueEmpty) {
            $this.addClass(_clsInvalid);

            // Show validate message
            ShowErrorMessage($this, options.requiredValidation.invalidMessage);

            // Run invalid callback
            if (options.invalidCallback != undefined)
                options.invalidCallback();

            return;
        }

        // Just validate if have value
        if (!isValueEmpty) {
            // Validate date format for input type Date
            if (options.dateFormatValidation != null) {

                var date = Date.parseExact(value, options.dateFormatValidation.dateFormat);
                if (date == null) {
                    $this.addClass(_clsInvalid);

                    // Show validate message
                    ShowErrorMessage($this, options.dateFormatValidation.invalidMessage);

                    // Run invalid callback
                    if (options.invalidCallback != undefined)
                        options.invalidCallback();

                    return;
                } else {
                    $this.data('date-value', date.toString('yyyy-MM-dd'));
                }
            }

            // Validate pattern
            if (options.patternValidations != undefined) {
                var countPatternOptions = options.patternValidations.length;

                // Loop all validate option
                for (var i = 0; i < countPatternOptions; i++) {
                    var option = options.patternValidations[i],
                        re = new RegExp(option.pattern);

                    // Not valid case
                    if (!re.test(value)) {

                        if (options.type == 'Date') {
                            $this.data('date-value', '');
                        } else {
                            // Restore value
                            if (option.isRestoreOldValue === true)
                                $this.val($this.data('old-value'));
                            else {
                                $this.data('old-value', value);
                                $this.addClass(_clsInvalid);
                            }
                        }

                        // Show validate message
                        ShowErrorMessage($this, option.invalidMessage);

                        // Run invalid callback
                        if (options.invalidCallback != undefined)
                            options.invalidCallback();

                        return;
                    }
                }
            }
        }

        if (options.type != 'Date' && options.type != 'Picklist') {
            // Default valid action
            $this.data('old-value', value);
        }

        $this.tooltip('close');
        $this.attr('title', '');
        $this.removeClass(_clsInvalid);

        // Custom valid action
        if (options.validCallback != undefined) {
            options.validCallback();
        }
    });
}
function ButtonClear_Click($button) {

    $button.on('click', null, null, function () {
        var $input = $(this).parent().children('input[type="text"]');

        $input.data('old-value', '');
        $input.val('');
        $input.trigger('input');
        $input.focus();
    });
}

function ShowErrorMessage($input, errorMessage) {
    /// <signature>
    /// <summary>Show error message in tooltip.</summary>
    /// <param name="$input" type="jQuery object">Input which error message is belong.</param>
    /// <param name="errorMessage" type="String">Error message show in tooltip.</param>
    /// </signature>

    // Show validate message
    if (errorMessage != undefined) {

        if (!$input.inViewPort({ threshold: 0 })) {
            $input.tooltip('close');
        }
        else {
            if ($input.data('old-message') != errorMessage) {
                $input.data('old-message', errorMessage);
                $input.tooltip('close');
            }
            $input.attr('title', errorMessage);
            $input.tooltip('open');
        }
    }
}