// Panel thumbnail
var idPanelThumbnailInner = '#panel-thumbnails-inner';
var idPanelViewer = '#panel-viewer';
var idPanelTotalComment = '#panel-total-comment';
var idPanelInputComment = '#panel-comment-input';
var idPanelPrevNext = '#panel-prev-next';
var idPanelControl = '#panel-controls';

var idTabThumbnail = '#tab-thumbnails';
var idTabBatchIndex = '#tab-batch-indexes';
var idTabDocIndex = '#tab-indexes';
var idTabComment = '#tab-comments';

var idUlThumbnail = '#ul-thumb';
var idBatchIndexPrefix = '#batch-index-';
var idDocIndexPrefix = '#doc-index-';
var idViewerBatchIntro = '#xr_xri';
var idViewerDocPrefix = '#view-doc-';
var idViewerPagePrefix = '#view-page-';
var idBatchCommentPrefix = '#comment-';

// Class active use in left bar tab menu
var classActiveTab = 'active';
var oldActiveTabId = idTabThumbnail;
var classTogglerClose = 'ui-layout-toggler-closed';
var classTogglerOpen = 'ui-layout-toggler-open';
var classInputDisable = 'input-disable';

// Position of scrollbar
var posScrollbarThumbnail = 0;
var posScrollbarBatchIndex = 0;
var posScrollbarDocIndex = 0;
var posScrollbarCommentViewer = 0;

// Sync count of loading image
var syncCountLoadingImage = 0;
// height of capture menu control
var captureMenuControlHeight = 26;
// Flag detect drag and drop thumnail in recyclebin

// Flag detect menu view open item is opened
var flgMenuViewOpened;
// Flag detect panel viewer is focus
var flgPanelViewerFocused = false;

var activeBatchId = '';
var activeDocId = '';
var activePageId = '';
var activeType = 'page';

// Var handle
var handleIntervalRemoveItem;
var handleIntervalScroll;

// Webcam
var pos = 0, ctx = null, saveCB, image = [];
var cameraWidth = 320, cameraHeight = 240;
var canvas = document.createElement("canvas");
canvas.setAttribute('width', cameraWidth);
canvas.setAttribute('height', cameraHeight);
ctx = canvas.getContext("2d");
image = ctx.getImageData(0, 0, cameraWidth, cameraHeight);
var currentLine;
var cameraStart = false;

var $html;
var $body;
var $activeAnno;
var syncAnnoKind = { value: '' };
var $panelViewer;
var isSorting;
var maxScale = 30;
var minScale = 1;

var isSortStopComplete;
var isInRecycleBin;

function LeftBarTabMenus_Click() {
    /// <signature>
    /// <summary>
    /// Click event handler for left bar tab menu: Thumbnail, Batch indexes, Indexes, Comment
    /// </summary>
    /// </signature>

    // Add handle when click thumbnail
    var tabs = idTabThumbnail + ',' + idTabBatchIndex + ',' + idTabDocIndex + ',' + idTabComment;
    $(tabs).click(function () {

        var $this = $(this);
        // Click itself again
        if ($this.hasClass(classActiveTab) || $this.hasClass(classInputDisable)) {
            return;
        }

        // Current active tab
        var $currentActiveTab = $('.leftbar_lable.active');
        var currentActiveTabId = '#' + $currentActiveTab.attr('id');
        $currentActiveTab.removeClass(classActiveTab);

        // Get old position of scrollbar panel thumbnail inner
        var $panelThumbnailInner = $(idPanelThumbnailInner);
        var posPanelThumbnailInner = $panelThumbnailInner.scrollTop();

        var clickedTabId = '#' + $this.attr('id');
        $(clickedTabId).addClass(classActiveTab);

        // Case click on tab comment from another tabs
        // => do not change content of panel thumbnail inner 
        // => change content in panel viewer
        if (clickedTabId == idTabComment) {
            // Store current tab id;
            oldActiveTabId = currentActiveTabId;
            // Save current position scrollbar 
            switch (currentActiveTabId) {
                case idTabThumbnail:
                    posScrollbarThumbnail = posPanelThumbnailInner;
                    break;

                case idTabBatchIndex:
                    posScrollbarBatchIndex = posPanelThumbnailInner;
                    break;

                case idTabDocIndex:
                    posScrollbarDocIndex = posPanelThumbnailInner;
            }

            // Show comment panel in it is closed
            if ($(idPanelTotalComment + '-toggler').hasClass(classTogglerClose)) {
                // Close corresponding panel of current active tab
                switch (activeType) {
                    case 'batch':
                        $(idViewerBatchIntro).hide();
                        break;

                    case 'doc':
                    case 'page':
                        $(idViewerDocPrefix + activeDocId).hide();
                        HideControlPanel();
                }

                var $divComments = $('#comment-' + activeBatchId);
                // Case batch comment is not loaded
                if ($divComments.length == 0) {

                    var $body = $('body');
                    $body.ecm_loading_show();

                    // Load batch comment
                    $.ajax({
                        url: urlGetComment,
                        type: 'POST',
                        data: { id: activeBatchId },
                        success: function (data, textStatus, jqXHR) {

                            var $data = $(data);

                            // Get total comments
                            var $totalComments = $data.children('#div-total-comments');
                            var $panelViewerTop = $(idPanelTotalComment);
                            $panelViewerTop.children().remove();
                            $panelViewerTop.append($totalComments);

                            $(idPanelTotalComment + '-toggler').trigger('click');
                            $(idPanelInputComment + '-toggler').trigger('click');

                            // Append comments
                            $('#panel-viewer').append($data);

                            $body.ecm_loading_hide();
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            console.log(errorThrown);
                            $body.ecm_loading_hide();
                        }
                    });
                } else {
                    $divComments.show();
                    $(idPanelTotalComment + '-toggler').trigger('click');
                    $(idPanelInputComment + '-toggler').trigger('click');
                }
            }

            // Case click on other tabs (not on tab comments)
            // => do not change content in panel viewer
            // => change content of panel thumbnail inner 
        } else {

            var saveActiveTabId;

            // From current tab comment
            if (currentActiveTabId == idTabComment) {
                // Click again old active tab => do nothing
                if (clickedTabId == oldActiveTabId) {
                    return;

                } else {
                    saveActiveTabId = oldActiveTabId;
                }

                // From other tab (not on tab comment)
            } else {
                saveActiveTabId = currentActiveTabId;
            }

            // Save current position scrollbar 
            switch (saveActiveTabId) {
                case idTabThumbnail:
                    posScrollbarThumbnail = posPanelThumbnailInner;
                    $(idUlThumbnail).hide();
                    break;

                case idTabBatchIndex:
                    posScrollbarBatchIndex = posPanelThumbnailInner;
                    $(idBatchIndexPrefix + activeBatchId).hide();
                    break;

                case idTabDocIndex:
                    posScrollbarDocIndex = posPanelThumbnailInner;
                    $(idDocIndexPrefix + activeDocId).hide();
                    HidePrevNextPanel();
            }

            // Show corresponding thumbnail inner
            switch (clickedTabId) {
                case idTabThumbnail:
                    $(idUlThumbnail).show();
                    $panelThumbnailInner.scrollTop(posScrollbarThumbnail)
                    break;

                case idTabBatchIndex:
                    $(idBatchIndexPrefix + activeBatchId).show();
                    $panelThumbnailInner.scrollTop(posScrollbarBatchIndex)
                    break;

                case idTabDocIndex:
                    var $docIndex = $(idDocIndexPrefix + activeDocId);
                    // Set order number for this doc
                    $docIndex.children('.current_content_header')
                                .find('.item-doc-index')
                                .html($('#' + activeDocId).children('.item-content').find('.item-doc-index').html());
                    $docIndex.show();
                    ShowPrevNextPanel();
            }
        }
    });
}
function ButtonPrevious_Click() {
    /// <signature>
    /// <summary>
    ///  Click event handler for button previous in thubmnail panel
    /// </summary>
    /// </signature>

    $('#btn-prev').click(function () {

        if ($(this).hasClass('input-disable')) {
            return;
        }

        // Hide current doc index
        $('#doc-index-' + activeDocId).hide();

        // Get next doc id
        var prevDocId = $('#' + activeDocId).prev().attr('id');

        var $prevDocIndex = $('#doc-index-' + prevDocId);
        // Set order number for this doc
        $prevDocIndex.children('.current_content_header')
                    .find('.item-doc-index')
                    .html($('#' + prevDocId).children('.item-content').find('.item-doc-index').html());

        $prevDocIndex.show();
        //$prevDocIndex.find('.content_fields_input').first().children('input[type="text"]').focus();

        HideCommentPanel(idTabDocIndex);
        // Trigger active doc click
        $('#' + prevDocId).children('.item-content').trigger('click');

        // Show next prev button
        ShowPrevNextPanel();
    });
}
function ButtonNext_Click() {
    /// <signature>
    /// <summary>
    /// Click event handler for button next in thubmnail panel
    /// </summary>
    /// </signature>

    $('#btn-next').click(function () {

        if ($(this).hasClass('input-disable')) {
            return;
        }

        // Hide current doc index
        $('#doc-index-' + activeDocId).hide();

        // Get next doc id
        var nextDocId = $('#' + activeDocId).next().attr('id');

        var $nextDocIndex = $('#doc-index-' + nextDocId);
        // Set order number for this doc
        $nextDocIndex.children('.current_content_header')
                    .find('.item-doc-index')
                    .html($('#' + nextDocId).children('.item-content').find('.item-doc-index').html());

        $nextDocIndex.show();
        //$nextDocIndex.find('.content_fields_input').first().children('input[type="text"]').focus();

        HideCommentPanel(idTabDocIndex);
        // Trigger active doc click
        $('#' + nextDocId).children('.item-content').trigger('click');

        // Show next prev button
        ShowPrevNextPanel();
    });
}

function HideCommentPanel(idActiveTab) {
    /// <signature>
    /// <summary>
    /// Close the panel comment if it is opened.
    /// </summary>
    /// <param name="idActiveTab" type="String">Id of tab want to set active.</param>
    /// </signature>

    var $togglerTotalComment = $(idPanelTotalComment + '-toggler');
    if (!$togglerTotalComment.hasClass(classTogglerOpen)) {
        return;
    }

    // Save scrollbar position
    posScrollbarCommentViewer = $(idPanelViewer).scrollTop();

    $('#comment-' + activeBatchId).hide();
    $togglerTotalComment.trigger('click');
    $(idPanelInputComment + '-toggler').trigger('click');

    $(idTabComment).removeClass(classActiveTab);
    $(idActiveTab).addClass(classActiveTab);
}
function ShowPrevNextPanel() {
    /// <signature>
    /// <summary>
    /// Show panel button previous and next, set status visible for these buttons.
    /// </summary>
    /// </signature>

    var $togglerPanel = $(idPanelPrevNext + '-toggler');

    // Show panel if not open
    if ($togglerPanel.hasClass(classTogglerClose)) {
        $togglerPanel.trigger('click');
    }

    var $btnPrev = $('#btn-prev');
    var $btnNext = $('#btn-next');

    // Default disable
    $btnPrev.addClass('input-disable');
    $btnNext.addClass('input-disable');

    // Set status for button prev and next
    // Get active li doc index
    var $liDoc = $('#' + activeDocId);

    // Get prev li doc
    var $liPrevDoc = $liDoc.prev();
    // Enable button prev
    if ($liPrevDoc.length > 0 && !$liPrevDoc.hasClass('loose-item')) {
        $btnPrev.removeClass('input-disable');
    }

    // Enable button next
    if ($liDoc.next().length > 0) {
        $btnNext.removeClass('input-disable');
    }
}
function HidePrevNextPanel() {
    /// <signature>
    /// <summary>
    /// Hide panel button previous and next.
    /// </summary>
    /// </signature>

    var $togglerPanel = $(idPanelPrevNext + '-toggler');

    // Show panel if not open
    if ($togglerPanel.hasClass(classTogglerOpen)) {
        $togglerPanel.trigger('click');
    }
}

function ShowControlPanel() {
    /// <signature>
    /// <summary>
    /// Show panel control.
    /// </summary>
    /// </signature>

    var $tooglerControlPanel = $(idPanelControl + '-toggler')
    if ($tooglerControlPanel.hasClass(classTogglerClose)) {
        $tooglerControlPanel.trigger('click');
    }
}
function HideControlPanel() {
    /// <signature>
    /// <summary>
    /// Hide panel control.
    /// </summary>
    /// </signature>

    var $tooglerControlPanel = $(idPanelControl + '-toggler')
    if ($tooglerControlPanel.hasClass(classTogglerOpen)) {
        $tooglerControlPanel.trigger('click');
        $('#panel-controls').data('doc-id', '');
    }
}
function SetStatusPanelControl(idBatch, idDoc, type, isNativeImage) {

    if ($('#panel-controls').data('doc-id') == idDoc) {
        return;
    }

    var $liBatch = $('#' + idBatch);
    var $liDoc = $('#' + idDoc);

    var isNormalDoc = !$liDoc.hasClass('loose-item');

    if ($liBatch.data('can-print') == 'True' && isNormalDoc)
        $('#control-print').show();
    else
        $('#control-print').hide();

    if ($liBatch.data('can-email') == 'True' && isNormalDoc)
        $('#control-mail').show();
    else
        $('#control-mail').hide();

    if ($liBatch.data('can-download') == 'True' && isNormalDoc)
        $('#control-save').show();
    else
        $('#control-save').hide();

    if (isNormalDoc) {
        $('#control-select').show();
        $('#control-hide').show();
        if ($liDoc.data('hide-anno') == 'True')
            $('#control-hide').addClass(classActiveTab).attr('title', titleShowAnnos);
        else
            $('#control-hide').removeClass(classActiveTab).attr('title', titleHideAnnos);
    } else {
        $('#control-select').hide();
        $('#control-hide').hide();
    }

    if ($liDoc.data('can-add-highlight') == 'True' && isNormalDoc)
        $('#control-Highlight').show();
    else
        $('#control-Highlight').hide();

    if ($liDoc.data('can-add-redaction') == 'True' && isNormalDoc)
        $('#control-Redaction').show();
    else
        $('#control-Redaction').hide();

    if ($liDoc.data('can-add-text') == 'True' && isNormalDoc)
        $('#control-Text').show();
    else
        $('#control-Text').hide();

    $('#control-zoom-in').removeClass('disable');
    $('#control-zoom-out').removeClass('disable');
    var scale = $liDoc.data('scale');
    if (scale == maxScale)
        $('#control-zoom-in').addClass('disable');
    else if (scale == minScale)
        $('#control-zoom-out').addClass('disable');

    var canModify = $liBatch.data('can-modify-doc');
    if ((canModify == 'True' && type != 'page') || (canModify == 'True' && type == 'page' && isNativeImage == false))
        $('#control-rotate-right, #control-rotate-left').show();
    else
        $('#control-rotate-right, #control-rotate-left').hide();

    $('.control-2-state.' + classActiveTab).removeClass(classActiveTab);
    var statusControl = $liDoc.data('status-control');
    switch (statusControl) {
        case 'pan':
            $.pan('on', idDoc);
            break;

        case 'select':
            $.select('on', idDoc);
            break;

        case 'Highlight':
        case 'Redaction':
        case 'Text':
            $.draw('on', idDoc, statusControl);
    }
    $('#control-' + statusControl).addClass(classActiveTab);

    if ($('#control-hide').hasClass(classActiveTab))
        $('#control-Highlight,#control-Redaction,#control-Text').addClass('disable');
    else
        $('#control-Highlight,#control-Redaction,#control-Text').removeClass('disable');
}

function ShowOrHideAnnos() {
    console.log('ShowOrHideAnnos');
    var $controlHideAnno = $('#control-hide');
    var $annos = $('#view-doc-' + activeDocId).find('.anno')
    var $liCurrentDoc = $('#' + activeDocId);

    // Case control hide is selected
    if ($liCurrentDoc.data('hide-anno') == 'True') {
        $liCurrentDoc.data('hide-anno', 'False');
        // Show all
        $annos.show();
        $controlHideAnno.attr('title', titleHideAnnos).removeClass(classActiveTab);
        $('#control-Highlight,#control-Redaction,#control-Text').removeClass('disable');

        if ($('#control-Highlight.active').length > 0)
            $.draw('on', activeDocId, 'Highlight', true);
        else if ($('#control-Redaction.active').length > 0)
            $.draw('on', activeDocId, 'Redaction', true);
        else if ($('#control-Text.active').length > 0)
            $.draw('on', activeDocId, 'Text', true);

    } else { // Case is not selected
        $liCurrentDoc.data('hide-anno', 'True');
        // Hide all
        $annos.hide();
        $controlHideAnno.attr('title', titleShowAnnos).addClass(classActiveTab);;
        $('#control-Highlight,#control-Redaction,#control-Text').addClass('disable');

        if ($('#control-Highlight.active,#control-Redaction.active,#control-Text.active').size() > 0) {
            $.draw('off', activeDocId);
        }
    }
}

// Set status enable/disable for buttons: Save, Approve
function SetStatusPanelSubmit() {
    console.log('SetStatusPanelSubmit');
    var $buttonSave = $('#btn-save');
    var $buttonApprove = $('#btn-approve');
    var classDisable = 'input-disable';

    // Get li active batch
    var $liBatch = $('#' + activeBatchId);
    var $liLooseDoc = $liBatch.find('.li-doc.loose-item');

    // Case have no page or
    // Can not release loose page but have loose page
    if ((!$liBatch.hasClass('accept') && !$liBatch.hasClass('reject')) ||
        ($liBatch.data('can-release-loose-page') != 'True' && ($liLooseDoc.hasClass('accept') ||
                                                               $liLooseDoc.hasClass('reject')))) {
        $buttonSave.addClass(classDisable);
        $buttonApprove.addClass(classDisable);
        return;
    }

    // Check empty required field
    var $liNormalDocs = $liBatch.find('.li-doc:not(.loose-item)');
    var countNormalDocs = $liNormalDocs.size();

    for (var i = 0; i < countNormalDocs; i++) {
        var $docIndex = $(idDocIndexPrefix + $liNormalDocs[i].id);
        // Have empty required field
        if ($docIndex.find('.required.empty').size() > 0) {
            $buttonSave.addClass(classDisable);
            $buttonApprove.addClass(classDisable);
            return;
        }
    }

    $buttonSave.removeClass(classDisable);

    // 2014/08/16 - HungLe - Temp comment for final action of save and submit button
    //if ($liBatch.hasClass('reject'))
    //    $buttonApprove.addClass(classDisable);
    //else
    //    $buttonApprove.removeClass(classDisable);
}

function InitLayout() {
    /// <signature>
    /// <summary>
    /// Initialize the layout for page
    /// </summary>
    /// </signature>

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

                    center__paneSelector: '#panel-thumbnails',
                    center__onresize_end: function (pName, pElem, pState) {
                        $('#panel-thumbnails-inner').height(pState.innerHeight - 5);
                    },

                    south__paneSelector: '#panel-prev-next',
                    south__resizable: false,
                    south__slidable: false,
                    south__initClosed: true,
                    south__spacing_open: 1,
                    south__spacing_closed: 0,
                    south__onopen_end: function () {
                        $(idPanelThumbnailInner).scrollTop(posScrollbarDocIndex);
                    }
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
                south__onopen_end: function () {
                    $(idPanelViewer).scrollTop(posScrollbarCommentViewer);
                },
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
    $('.leftbar_lable').each(function () {
        var $this = $(this);
        var outWidth = $this.outerWidth();
        var outHeight = $this.outerHeight();

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
}
function ResetLayout() {
    /// <signature>
    /// <summary>
    /// Reset layout to the initialize
    /// </summary>
    /// </signature>

    //// Hide the current active batch li thumbnail
    //$('#' + activeBatchId).hide();
    //// Hide the current active batch index
    //$(idBatchIndexPrefix + activeBatchId).hide();
    //// Hide the current active doc index
    //$(idDocIndexPrefix + activeDocId).hide();
    //// Hide the panel prev and next button
    //HidePrevNextPanel();
    //// Hide the current active batch viewer introduce
    //$(idViewerBatchIntro).hide();
    //// Hide the current active doc viewer
    //$(idViewerDocPrefix + activeDocId).hide();
    //// Hide panel comment
    //$(idBatchCommentPrefix + activeBatchId).hide();
    //HideCommentPanel(idTabThumbnail);
    //// Show control panel
    //ShowControlPanel();

    // Remove the current active batch li thumbnail
    var $batch = $('#' + activeBatchId).remove();
    // Remove the current active batch index
    $(idBatchIndexPrefix + activeBatchId).remove();
    // Remove the doc index and viewer
    $batch.find('.li-doc').each(function () {
        var id = $(this).attr('id');
        $(idDocIndexPrefix + id).remove();
    });
    // Hide the panel prev and next button
    HidePrevNextPanel();
    // Hide the batch viewer introduce
    $(idViewerBatchIntro).hide();
    // Remove the current batch viewer
    $('#view-batch-' + activeBatchId).remove();
    // Remove panel comment
    $(idBatchCommentPrefix + activeBatchId).remove();
    HideCommentPanel(idTabThumbnail);
    // Show control panel
    ShowControlPanel();
}

function InitRecycleBin() {
    /// <signature>
    /// <summary>
    /// Initialize the recycle bin for drop delete
    /// </summary>
    /// </signature>

    var $body = $('body');
    _top = ($body.scrollTop() + $body.height() - 300) / 2;
    _left = ($body.scrollTop() + $body.width() - 300) / 2;
    $recyclebin = $('<img src="' + imageRecycleBin + '" />').css({
        position: 'absolute',
        top: _top,
        left: _left,
        height: 300,
        width: 300,
        opacity: '0.1',
        zIndex: 2
    });
    $body.append($recyclebin);
    $recyclebin.hide();
    $recyclebin.droppable({
        tolerance: 'pointer',
        drop: function (event, u) {
            isInRecycleBin = true;
        },
        over: function (event, u) {
            $recyclebin.css('opacity', '0.3');
        },
        out: function (event, u) {
            $recyclebin.css('opacity', '0.1');
        }
    });
}

function InitJQueryTEComment() {
    /// <signature>
    /// <summary>
    /// Initialize the jQuery TE input comment.
    /// </summary>
    /// </signature>

    $('#input-comment').jqte({
        center: false,
        fsizes: ['8', '9', '10', '11', '12', '14', '16', '18', '20', '24', '26', '28', '36', '72'],
        format: false,
        indent: false,
        link: false,
        left: false,
        ol: false,
        outdent: false,
        remove: false,
        right: false,
        rule: false,
        source: false,
        sub: false,
        strike: false,
        sup: false,
        ul: false,
        unlink: false,
        tagCss: "input-comment",
        change: function () {

            var $buttonSaveComment = $('#btn-save-comment');

            // Get source comment
            var value = $jqteComment.html();

            // Count character
            var countCharacter = CountCharacter($('<div>' + value + '</div>'));
            if (countCharacter <= 0 || countCharacter >= 4000) {
                $buttonSaveComment.addClass('input-disable');
            } else {
                $buttonSaveComment.removeClass('input-disable');
            }

            // Update remain character
            $('#panel-remains-character').children().html(4000 - countCharacter);
        }
    });
    // Get comment input
    var $jqteComment = $('.jqte_editor.input-comment');

    // Add new comment button handler
    $('#btn-save-comment').click(function () {

        if ($(this).hasClass('input-disable')) {
            return;
        }

        var $templateComment = $('.tr-template-comment').clone();
        $templateComment.removeClass('tr-template-comment').addClass('new-comment');
        // Adding comment
        $templateComment.find('.bubble.me').html($jqteComment.html());
        // Adding created time
        var $createDate = $templateComment.find('.comment-create-on');
        var now = new Date();
        $createDate.html(now.toString($createDate.html()));

        var $trComments = $('#tb-comments').find('tr');
        // Insert to panel comments
        $templateComment.insertBefore($trComments.first());
        console.log($trComments.size());
        // Update total count comment
        $('#div-total-comments > span').html($trComments.size());

        $templateComment.show();

        // Reset input control
        $jqteComment.html('');
    });
}
// Count the number plain character of wysiwyg TE editor
function CountCharacter($elem) {
    var total = 0;
    var contents = $elem.contents();

    for (var i = 0; i < contents.length; i++) {
        var content = contents[i];

        if (content.nodeType === 3) {
            if (content.nodeValue.trim().length > 0) {
                total += content.nodeValue.length;
            }
            continue;
        }
        else {
            if (content.tagName.toLowerCase() == 'br') {
                //total++;
                continue;
            }

            total += CountCharacter($(content));
        }
    }

    return total;
}
function SplitNode($elem, parentStyle, arrResult) {
    var contents = $elem.contents();

    for (var i = 0; i < contents.length; i++) {
        var content = contents[i];

        var $span = $('<span>');
        // Add style of
        $span.attr('style', parentStyle);

        if (content.nodeType === 3) {
            $span.append(content.nodeValue);
            arrResult.push($span);
            continue;
        }
        else {
            if (content.tagName.toLowerCase() == 'br') {
                arrResult.push($('<br/>'));
                continue;
            }

            var $content = $(content);
            $span.css('color', $content.css('color'));
            $span.css('font-size', $content.css('font-size'));
            $span.css('font-weight', $content.css('font-weight'));
            $span.css('font-style', $content.css('font-style'));
            $span.css('text-decoration', $content.css('text-decoration'));

            SplitNode($(content), $span.attr('style'), arrResult)
        }
    }
}

function InitUploadFile() {
    /// <signature>
    /// <summary>
    /// Initialize the upload file.
    /// </summary>
    /// </signature>

    // Upload file
    var options = {
        //target: '#output2',   // target element(s) to be updated with server response 
        //beforeSubmit: showRequest,  // pre-submit callback 
        success: showResponse // post-submit callback

        // other available options: 
        //url:       url         // override for form's 'action' attribute 
        //type:      type        // 'get' or 'post', override for form's 'method' attribute 
        //, dataType: 'text/html'        // 'xml', 'script', or 'json' (expected server response type) 
        //, clearForm: true        // clear all form fields after successful submit 
        //,resetForm: true        // reset the form after successful submit 

        // $.ajax options can be used here too, for example: 
        //timeout:   3000 
    };
    $('#frm-upload').ajaxForm(options);
    $('#btn-upload-browser').change(function () {
        $('body').ecm_loading_show();
        $('#btn-upload').trigger('click');
    });
}
function InitCamera() {
    /// <signature>
    /// <summary>
    /// Initialize the camera.
    /// </summary>
    /// </signature>

    $('#camera').webcam({
        width: 530,
        height: 400,
        mode: 'callback',
        swffile: swiffile,
        onCapture: function () {

            if (!cameraStart) {
                return;
            }

            currentLine = 1;
            $('body').ecm_loading_show();
            document.getElementById('XwebcamXobjectX').save();
        },
        onSave: function (data) {
            currentLine++;

            var col = data.split(";");
            var img = image;
            for (var i = 0; i < cameraWidth; i++) {
                var tmp = parseInt(col[i]);
                img.data[pos + 0] = (tmp >> 16) & 0xff;
                img.data[pos + 1] = (tmp >> 8) & 0xff;
                img.data[pos + 2] = tmp & 0xff;
                img.data[pos + 3] = 0xff;
                pos += 4;
            }
            if (currentLine > cameraHeight) {

                ctx.putImageData(img, 0, 0);
                $.post(
                    urlCamera,
                    model = {
                        PageId: activePageId,
                        DocId: activeDocId,
                        BatchId: activeBatchId,
                        Type: $('#upload-type').val(),
                        imageData: canvas.toDataURL("image/jpeg")
                    },
                    function (data) {
                        showResponse(data);
                    }
                );
                pos = 0;

                $('#camera').dialog('close');
            }
        },
        debug: function (type, text) {

            if (text == 'Camera started') {
                cameraStart = true;
            } else if (text == 'Camera stopped') {
                cameraStart = false;
            }
        }
    });

    // Webcam
    $('#camera').dialog({
        title: 'Camera',
        autoOpen: false,
        modal: true,
        width: 560,
        height: 505,
        resizable: false,
        buttons:
            [
                { text: "Ok", click: function () { document.getElementById('XwebcamXobjectX').capture(); } },
                { text: "Close", click: function () { $(this).dialog("close"); } }
            ]
    });
}
// post-submit callback of ajax form upload and camera
function showResponse(responseText, statusText, xhr, $form) {

    var $result = $(responseText);

    var $liActivePage = $('#' + activePageId);
    var $viewActivePage = $('#view-page-' + activePageId);
    var $liActiveDoc = $('#' + activeDocId);

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

    SetStatusPanelSubmit();
    $('body').ecm_loading_hide();
}
function Scan(isShowDialog) {
    try {
        // Get ocx scanner
        if (BrowserDetect.browser == "Chrome") {
            scanner = document.TwainX;
        } else {
            scanner = new ActiveXObject("TwainActiveX.TwainActiveX");
        }

        if (scanner) {

            var $body = $('body');
            $body.ecm_loading_show();

            // Scan pages
            var fileNames = scanner.ScanCapture(true, "http://" + document.location.host + "/" + urlScanUpload, $('#scan-token').val());
            console.log(fileNames);
            // Get pages after scan
            $.post(
                    urlScan,
                    model = {
                        PageId: activePageId,
                        DocId: activeDocId,
                        BatchId: activeBatchId,
                        Type: $('#upload-type').val(),
                        FileNames: fileNames
                    },
                    function (data) {
                        console.log(data);
                        showResponse(data);
                    }
                );
        }
    } catch (e) {
        jAlert('Scanning has error!', ecmTitleMessage)
        $body.ecm_loading_hide();
        console.log(e);
    }
}

function InitOpenedBatchesMenu() {
    /// <signature>
    /// <summary>
    /// Initialize context menu for menu view works (opened batches)
    /// </summary>
    /// </signature>
    console.log('hung');
    $('#opened-batch-menu').contextMenu({
        build: function ($trigger, e) {

            if (syncCountLoadingImage > 0) {
                return false;
            }

            var batches = $('#opened-batch-menu').data('opened-batches');
            var _items = {};

            _items['Close'] = { name: closeThis };
            if (batches.length > 1) {
                _items['CloseOther'] = { name: closeOther };
                _items['CloseAll'] = { name: closeAll };
            }
            _items['sep1'] = "---------";
            // Immediately
            // Create view task menu
            for (var i = 0; i < batches.length; i++) {
                if (batches[i]['Key'] == activeBatchId) {

                    _items[batches[i]['Key']] = {
                        name: batches[i]['Value'],
                        iconUrl: iconActiveBatch,
                        iconDelete: iconDelete,
                        hideMenuImmediately: true
                    };
                } else {
                    _items[batches[i]['Key']] = {
                        name: batches[i]['Value'],
                        iconDelete: iconDelete,
                        hideMenuImmediately: true
                    };
                }
            }

            return {
                callback: function (key, options, subAction) {
                    console.log(subAction);
                    //$(this).contextMenu("show");

                    switch (key) {
                        case 'CloseOther':
                        case 'CloseAll':
                        case 'Close':
                            CloseBatchesWithConfirmSave(key)
                            break;

                        default:
                            if (subAction == 'Delete') {
                                CloseBatchesWithConfirmSave(key)
                                return;
                            }

                            // Case no change select batch view
                            if (key == activeBatchId) {
                                $body.ecm_loading_hide();
                                return;
                            }

                            SaveBatch(
                                function () {
                                    ResetLayout();
                                    OpenBatch(key);
                                },
                                function () {
                                    jAlert('Save batch is fail.', ecmTitleMessage, function () {
                                        ResetLayout();
                                        OpenBatch(activeBatchId);
                                    })
                                }
                            );
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
function InitThumbnailContextMenu() {
    /// <signature>
    /// <summary>
    /// Initialize context menu for thubmnail item
    /// </summary>
    /// </signature>

    // Build context menu in thumbnail
    $('.tree-item.ul-batch').contextMenu({
        build: function ($trigger, e) {

            $trigger.trigger('click');

            if (syncCountLoadingImage > 0) {
                return false;
            }

            var _menus = {}

            // Get current active batch
            var $liActiveBatch = $('#' + activeBatchId);

            // Get permission
            var canDelete = $liActiveBatch.data('can-del');
            var canReject = $liActiveBatch.data('can-reject');
            var canSendLink = $liActiveBatch.data('can-send-link');
            var canModify = $liActiveBatch.data('can-modify-doc') ? 'True' : 'False';

            // li which is clicked
            var $liTrigger = $trigger.parent();

            var flagHasPrevItem = false;
            // Create menu
            if ($liTrigger.hasClass('li-page')) {

                var flag = false;

                // Delete menu
                if (canDelete == 'True') {
                    flag = true;
                    _menus['PageDelete'] = { name: menuThumbDelete };
                }

                // Reject menu
                if (canReject == 'True') {
                    flag = true;
                    if ($liTrigger.hasClass('accept')) {
                        _menus['PageReject'] = { name: menuThumbReject };
                    } else {
                        _menus['PageUnReject'] = { name: menuThumbUnReject };
                    }
                }

                if (canModify == 'True') {
                    flag = true;
                    _menus['PageReplace'] = {
                        name: menuThumbReplace,
                        items: {
                            PageReplaceScan: { name: menuThumbScan },
                            PageReplaceImportFile: { name: menuThumbImportFile, labelFor: 'btn-upload-browser' },
                            PageReplaceCamera: { name: menuThumbCamera }
                        }
                    };
                    _menus['sep1'] = "---------";
                    _menus['PageInsertBefore'] = {
                        name: menuThumbInsertBefore,
                        items: {
                            PageInsertBeforeScan: { name: menuThumbScan },
                            PageInsertBeforeImportFile: { name: menuThumbImportFile, labelFor: 'btn-upload-browser' },
                            PageInsertBeforeCamera: { name: menuThumbCamera }
                        }
                    };
                    _menus['PageInsertAfter'] = {
                        name: menuThumbInsertAfter,
                        items: {
                            PageInsertAfterScan: { name: menuThumbScan },
                            PageInsertAfterImportFile: { name: menuThumbImportFile, labelFor: 'btn-upload-browser' },
                            PageInsertAfterCamera: { name: menuThumbCamera }
                        }
                    };
                    if (!$trigger.children('.item-image').hasClass('native-image')) {
                        _menus['PageRotateRight'] = { name: menuThumbRotateRight };
                        _menus['PageRotateLeft'] = { name: menuThumbRotateLeft };
                    }
                }

                if (flag) {
                    _menus['sep2'] = "---------";
                }

                var $liDoc = $liTrigger.parent().parent().parent();
                if ($liDoc.hasClass('loose-item')) {
                    delete _menus['sep2'];
                } else {
                    _menus['PageIndex'] = { name: menuThumbIndex };
                }

            } else if ($liTrigger.hasClass('li-doc')) {

                //_menus['DocRenameContent'] = { name: menuThumbRenameContent };
                //_menus['DocContentLanguageSetting'] = {
                //    name: menuThumbContentLanguageSetting,
                //    items: {
                //        languageEnglish: { name: 'English' },
                //        languageVietnamese: { name: 'Vietnamese' }
                //    }
                //};
                //_menus['sep1'] = "---------";

                // Delete menu
                if (canDelete == 'True') {
                    _menus['DocDelete'] = { name: menuThumbDelete };
                    _menus['sepDelete'] = "---------";
                }

                var flag = false;
                // Reject menu
                if (canReject == 'True') {
                    flag = true;
                    if ($liTrigger.hasClass('accept')) {
                        _menus['DocReject'] = { name: menuThumbReject };
                    } else if ($liTrigger.hasClass('reject')) {
                        _menus['DocUnReject'] = { name: menuThumbUnReject };
                    } else {
                        flag = true;
                    }
                }

                if (canModify == 'True') {
                    flag = true;
                    _menus['DocAppend'] = {
                        name: menuThumbAppend,
                        items: {
                            DocAppendScan: { name: menuThumbScan },
                            DocAppendImportFile: { name: menuThumbImportFile, labelFor: 'btn-upload-browser' },
                            DocAppendCamera: { name: menuThumbCamera }
                        }
                    };
                    _menus['DocRotateRight'] = { name: menuThumbRotateRight };
                    _menus['DocRotateLeft'] = { name: menuThumbRotateLeft };
                }

                if (flag) {
                    _menus['sep2'] = "---------";
                }

                _menus['DocIndex'] = { name: menuThumbIndex };

            } else if ($liTrigger.hasClass('li-batch')) {
                console.log('hung');
                // Insert menu
                if ($liTrigger.find('.li-page').size() == 0) {
                    _menus['BatchAppend'] = {
                        name: menuThumbAppend,
                        items: {
                            BatchAppendScan: { name: menuThumbScan },
                            BatchAppendImportFile: { name: menuThumbImportFile, labelFor: 'btn-upload-browser' },
                            BatchAppendCamera: { name: menuThumbCamera }
                        }
                    };
                    _menus['sep'] = "---------";
                }

                // Delete menu
                if (canDelete == 'True') {
                    if ($liTrigger.find('.li-page').size() > 0) {
                        _menus['BatchDelete'] = { name: menuThumbDelete };
                        _menus['sep'] = "---------";
                    }
                }

                var flag = false;
                // Reject menu
                if (canReject == 'True') {
                    if ($liTrigger.hasClass('accept')) {
                        _menus['BatchReject'] = { name: menuThumbReject };
                    } else if ($liTrigger.hasClass('reject')) {
                        _menus['BatchUnReject'] = { name: menuThumbUnReject };
                    }
                    flag = true;
                }
                // Send link menu
                if (canSendLink == 'True') {
                    _menus['BatchSendLink'] = { name: menuThumbSendLink };
                    flag = true;
                }
                // Save menu
                if (!$('#btn-save').hasClass('input-disable')) {
                    _menus['BatchSave'] = { name: menuThumbSave };
                    flag = true;
                }

                if (!flag) {
                    delete _menus['sep'];
                }
            }

            // Check menu is empty
            var flgMenuEmpty = true;
            for (var isNotEmpty in _menus) {
                flgMenuEmpty = false;
                break;
            }
            if (flgMenuEmpty) {
                return false;
            }

            return {
                items: _menus,
                callback: function (key, options) {
                    switch (key) {
                        // Rotate right
                        case 'DocRotateRight':
                        case 'PageRotateRight':
                            $('#control-rotate-right').trigger('click');
                            break;

                            // Rotate left
                        case 'DocRotateLeft':
                        case 'PageRotateLeft':
                            $('#control-rotate-left').trigger('click');
                            break;

                            // Delete batch
                        case 'BatchDelete':
                            jConfirm(messageConfirmDeleteBatch, ecmTitleMessage, function (result) {
                                // Case confirm Ok
                                if (result == false) {
                                    return;
                                }

                                $body.ecm_loading_show();

                                var $liBatch = $('#' + activeBatchId);

                                // Delete all page in li doc loose page
                                var $liLooseDoc = $liBatch.find('.li-doc.loose-item')
                                $liLooseDoc.find('.li-page').remove();
                                $('#view-doc-' + $liLooseDoc.attr('id')).children().remove();

                                $liBatch.find('.li-doc:not(.loose-item)').each(function () {
                                    $('#doc-index-' + $(this).attr('id')).remove();
                                    $('#view-doc-' + $(this).attr('id')).remove();
                                    $(this).remove();
                                });

                                // Remove icon accept and reject
                                $liBatch.removeClass('accept').removeClass('reject');

                                SetStatusPanelSubmit();

                                $body.ecm_loading_hide();
                            });
                            break;

                            // Delete doc
                        case 'DocDelete':
                            jConfirm(messageConfirmDeleteDoc, ecmTitleMessage, function (result) {
                                // Case confirm Ok
                                if (result == false) {
                                    return;
                                }

                                $body.ecm_loading_show();
                                // Get active doc
                                var ui = { item: $('#' + activeDocId) };
                                DeleteDoc(ui);
                            });
                            break;

                            // Delete page
                        case 'PageDelete':
                            jConfirm(messageConfirmDeletePage, ecmTitleMessage, function (result) {
                                // Case confirm Ok
                                if (result == false) {
                                    return;
                                }

                                // Get active page
                                var ui = { item: $('#' + activePageId) };

                                DeletePage(ui);
                            });
                            break

                            // Reject batch
                        case 'BatchReject':
                            var $liBatch = $('#' + activeBatchId);
                            $liBatch.removeClass('accept').addClass('reject');
                            $liBatch.find('.li-doc,.li-page').removeClass('accept').addClass('reject');

                            var $liLooseDoc = $liBatch.find('.li-doc.loose-item');
                            if ($liLooseDoc.find('.li-page').size() == 0)
                                $liLooseDoc.removeClass('reject');

                            // 2014/08/16 - HungLe - Temp comment for final action of save and submit button
                            //$('#btn-approve').addClass('input-disable');
                            break;

                            // Reject doc
                        case 'DocReject':
                            $('#' + activeBatchId).removeClass('accept').addClass('reject');

                            var $liDoc = $('#' + activeDocId);
                            $liDoc.removeClass('accept').addClass('reject');
                            $liDoc.find('.li-page').removeClass('accept').addClass('reject');

                            // 2014/08/16 - HungLe - Temp comment for final action of save and submit button
                            //$('#btn-approve').addClass('input-disable');
                            break;

                            // Reject page
                        case 'PageReject':
                            $('#' + activeBatchId).removeClass('accept').addClass('reject');
                            $('#' + activeDocId).removeClass('accept').addClass('reject');
                            $('#' + activePageId).removeClass('accept').addClass('reject');

                            // 2014/08/16 - HungLe - Temp comment for final action of save and submit button
                            //$('#btn-approve').addClass('input-disable');
                            break;

                            // Un-reject batch
                        case 'BatchUnReject':
                            var $liBatch = $('#' + activeBatchId);
                            $liBatch.removeClass('reject').addClass('accept');
                            $liBatch.find('.li-doc,.li-page').removeClass('reject').addClass('accept');

                            var $liLooseDoc = $liBatch.find('.li-doc.loose-item');
                            if ($liLooseDoc.find('.li-page').size() == 0)
                                $liLooseDoc.removeClass('accept');

                            // 2014/08/16 - HungLe - Temp comment for final action of save and submit button
                            //SetStatusPanelSubmit();
                            break;

                            // Un-reject doc
                        case 'DocUnReject':
                            var $liDoc = $('#' + activeDocId);
                            $liDoc.removeClass('reject').addClass('accept');
                            $liDoc.find('.li-page').removeClass('reject').addClass('accept');

                            var $liBatch = $('#' + activeBatchId);
                            if ($liBatch.find('.li-doc.reject').size() == 0)
                                $liBatch.removeClass('reject').addClass('accept');

                            // 2014/08/16 - HungLe - Temp comment for final action of save and submit button
                            //SetStatusPanelSubmit();
                            break;

                            // Un-reject page
                        case 'PageUnReject':
                            $('#' + activePageId).removeClass('reject').addClass('accept');

                            var $liDoc = $('#' + activeDocId);
                            if ($liDoc.find('.li-page.reject').size() == 0)
                                $liDoc.removeClass('reject').addClass('accept');

                            var $liBatch = $('#' + activeBatchId);
                            if ($liBatch.find('.li-doc.reject').size() == 0)
                                $liBatch.removeClass('reject').addClass('accept');

                            // 2014/08/16 - HungLe - Temp comment for final action of save and submit button
                            //SetStatusPanelSubmit();
                            break;

                            // Show doc index
                        case 'DocIndex':
                        case 'PageIndex':
                            $('#tab-indexes').trigger('click');
                            break;

                        case 'PageReplaceImportFile':
                        case 'PageInsertBeforeImportFile':
                        case 'PageInsertAfterImportFile':
                        case 'DocAppendImportFile':
                            $('#upload-page-id').val(activePageId);
                            $('#upload-doc-id').val(activeDocId);
                            $('#upload-batch-id').val(activeBatchId);
                            $('#upload-type').val(key);
                            break;

                        case 'BatchAppendImportFile':
                            activeDocId = $('#' + activeBatchId).find('.li-doc.loose-item').attr('id');
                            $('#upload-doc-id').val(activeDocId);
                            $('#upload-batch-id').val(activeBatchId);
                            $('#upload-type').val(key);
                            break;

                        case 'PageReplaceCamera':
                        case 'PageInsertBeforeCamera':
                        case 'PageInsertAfterCamera':
                        case 'DocAppendCamera':
                            $('#upload-type').val(key);
                            var $camera = $('#camera');
                            $camera.dialog('option', 'position', { my: "center", at: "center", of: window });
                            $camera.dialog("open");
                            break;

                        case 'BatchAppendCamera':
                            activeDocId = $('#' + activeBatchId).find('.li-doc.loose-item').attr('id');
                            $('#upload-type').val(key);
                            var $camera = $('#camera');
                            $camera.dialog('option', 'position', { my: "center", at: "center", of: window });
                            $camera.dialog("open");
                            break;

                        case 'PageReplaceScan':
                        case 'PageInsertBeforeScan':
                        case 'PageInsertAfterScan':
                        case 'DocAppendScan':
                            $('#upload-type').val(key);
                            Scan();
                            break;

                        case 'BatchAppendScan':
                            activeDocId = $('#' + activeBatchId).find('.li-doc.loose-item').attr('id');
                            $('#upload-type').val(key);
                            Scan();
                            break;

                        default:
                            break;
                    }
                },
                position: function (opt, x, y) {
                    var offset = { top: y, left: x };
                    var $win = $(window);
                    var bottom = $win.scrollTop() + $win.height(),
                        right = $win.scrollLeft() + $win.width(),
                        height = opt.$menu.height(),
                        width = opt.$menu.width();

                    if (offset.top + height > bottom)
                        offset.top -= offset.top + height - bottom + 3;
                    if (offset.top < 0)
                        offset.top = 0;

                    if (offset.left + width > right)
                        offset.left -= offset.left + width - right + 3;
                    if (offset.left < 0)
                        offset.left = 0;

                    opt.$menu.css(offset);
                }
            };
        },
        selector: ".item-content",
        style: {
            height: 30, fontSize: 11, menuWidth: 250
        },
        events: {
            show: function (opt) {
                console.log(opt);


            },
            hide: $.noop
        }
    });
}
function InitViewerContextMenu() {
    /// <signature>
    /// <summary>
    /// Initialize context menu for viewer item
    /// </summary>
    /// </signature>

    // Build context menu in thumbnail
    $('#panel-viewer-wrapper').contextMenu({
        build: function ($trigger, e) {

            if (flgMenuViewOpened == true ||
                $(idViewerBatchIntro).is(':visible') ||
                syncCountLoadingImage > 0)
                return false;

            var _menus = {};

            // Get current active batch
            var $liActiveBatch = $('#' + activeBatchId);
            var $liDoc = $('#' + activeDocId);
            // Get permission
            var canPrint = $liActiveBatch.data('can-print');
            var canMail = $liActiveBatch.data('can-email');
            var canSave = $liActiveBatch.data('can-download');

            var flag = false;
            if (!$liDoc.hasClass('loose-item')) {
                // Create menu
                if (canPrint == 'True') {
                    flag = true;
                    _menus['ViewerPrint'] = { name: menuViewerPrint, iconUrl: iconViewerPrint };
                }
                if (canMail == 'True') {
                    flag = true;
                    _menus['ViewerMail'] = { name: menuViewerMail, iconUrl: iconViewerMail };
                }
                if (canSave == 'True') {
                    flag = true;
                    _menus['ViewerSave'] = { name: menuViewerSave, iconUrl: iconViewerSave };
                }
                if (flag)
                    _menus['sep1'] = "---------";

                var canHighlight = $liDoc.data('can-add-highlight');
                var canRedaction = $liDoc.data('can-add-redaction');
                var canText = $liDoc.data('can-add-text');

                var disabledAddAnno = false;
                if ($liDoc.data('hide-anno') == 'True') {
                    disabledAddAnno = true;
                    _menus['ViewerShow'] = { name: titleShowAnnos, iconUrl: iconViewerHide };
                }
                else
                    _menus['ViewerHide'] = { name: titleHideAnnos, iconUrl: iconViewerHide };
                if (canHighlight == 'True')
                    _menus['ViewerHighlight'] = { name: menuViewerHighlight, iconUrl: iconViewerHighlight, disabled: disabledAddAnno };
                if (canRedaction == 'True')
                    _menus['ViewerRedaction'] = { name: menuViewerRedaction, iconUrl: iconViewerRedaction, disabled: disabledAddAnno };
                if (canText == 'True')
                    _menus['ViewerText'] = { name: menuViewerText, iconUrl: iconViewerText, disabled: disabledAddAnno };
                _menus['sep2'] = "---------";

            }

            _menus['ViewerZoomIn'] = { name: menuViewerZoomIn, iconUrl: iconViewerZoomIn, disabled: $('#control-zoom-in').hasClass('disable') };
            _menus['ViewerZoomOut'] = { name: menuViewerZoomOut, iconUrl: iconViewerZoomOut, disabled: $('#control-zoom-out').hasClass('disable') };
            _menus['ViewerFitHeight'] = { name: menuViewerFitHeight, iconUrl: iconViewerFitHeight };
            _menus['ViewerFitWidth'] = { name: menuViewerFitWidth, iconUrl: iconViewerFitWidth };
            _menus['ViewerFitToViewer'] = { name: menuViewerFitToViewer, iconUrl: iconViewerFitToViewer };
            _menus['sep3'] = "---------";

            _menus['ViewerNavigationUp'] = { name: menuViewerNavigationUp, iconUrl: iconViewerNavigationUp };
            _menus['ViewerNavigationDown'] = { name: menuViewerNavigationDown, iconUrl: iconViewerNavigationDown };

            return {
                items: _menus,
                callback: function (key, options) {
                    switch (key) {
                        // Hide anno
                        case 'ViewerHide':
                        case 'ViewerShow':
                            $('#control-hide').trigger('click');
                            break;

                            // Add highlight
                        case 'ViewerHighlight':
                            $('#control-Highlight').trigger('click');
                            break;

                            // Add redaction
                        case 'ViewerRedaction':
                            $('#control-Redaction').trigger('click');
                            break;

                            // Add text
                        case 'ViewerText':
                            $('#control-Text').trigger('click');
                            break;

                            // Zoom in
                        case 'ViewerZoomIn':
                            $('#control-zoom-in').trigger('click');
                            break;

                            // Zoom out
                        case 'ViewerZoomOut':
                            $('#control-zoom-out').trigger('click');
                            break;

                            // Fit to height
                        case 'ViewerFitHeight':
                        case 'ViewerFitWidth':
                            var $viewer = $('#panel-viewer');
                            var pages = $('#view-doc-' + activeDocId).children();
                            var heightDocViewer = $viewer.height();
                            var page;
                            var top;
                            var bottom;

                            // Find the page is have top and bottom is positive
                            for (var i = 0; i < pages.length; i++) {
                                page = $(pages[i]);
                                top = page.position().top;
                                bottom = top + page.height();

                                if ((top <= 0 && bottom >= heightDocViewer) ||
                                    top >= 0 && bottom <= heightDocViewer)
                                    break;
                                else if (top >= 0 && bottom >= heightDocViewer) {
                                    page = $(pages[i - 1]);
                                    break;
                                }
                            }

                            if (key == 'ViewerFitHeight')
                                FitToHeight(page);
                            else
                                FitToWidth(page);
                            break;

                            // Fit to viewer
                        case 'ViewerFitToViewer':
                            $('#control-fit').trigger('click');
                            break;

                            // Zoom in
                        case 'ViewerNavigationUp':
                            $('#control-navigation-up').trigger('click');
                            break;

                            // Zoom out
                        case 'ViewerNavigationDown':
                            $('#control-navigation-down').trigger('click');
                    }
                },
                position: function (opt, x, y) {
                    var offset = { top: y, left: x };
                    var $win = $(window);
                    var bottom = $win.scrollTop() + $win.height(),
                        right = $win.scrollLeft() + $win.width(),
                        height = opt.$menu.height(),
                        width = opt.$menu.width();

                    if (offset.top + height > bottom)
                        offset.top -= offset.top + height - bottom + 3;
                    if (offset.top < 0)
                        offset.top = 0;

                    if (offset.left + width > right)
                        offset.left -= offset.left + width - right + 3;
                    if (offset.left < 0)
                        offset.left = 0;

                    opt.$menu.css(offset);
                }
            };
        },
        selector: "#panel-viewer",
        style: {
            height: 30, fontSize: 11, menuWidth: 250
        }
    });
}

function CloseBatchesWithConfirmSave(key) {
    jConfirm(messageConfirmSave, ecmTitleMessage, function (result) {
        $body.ecm_loading_show();

        if (result === true) {
            if (key == 'Close' || key == 'CloseAll' || key == activeBatchId) {
                SaveBatch(
                    function () {
                        CloseBatches(key, true)
                    },
                    function () {
                        $body.ecm_loading_hide();
                        jAlert('Save batch is fail.', ecmTitleMessage);
                    }
                );
            } else {
                CloseBatches(key, true)
            }
        } else {
            CloseBatches(key, false);
        }
    });
}
function CloseBatches(key, isSave, isSubmit) {

    var postData = {
        batchId: activeBatchId,
        closeType: key,
        isSave: isSave,
        isSubmit: isSubmit
    }

    if (key != 'Close' && key != 'CloseOther' && key != 'CloseAll') {
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
                if (key == 'Close' || key == 'CloseAll' || key == activeBatchId) {
                    ResetLayout();
                    OpenBatch(data[0].Key);
                }
                $body.ecm_loading_hide();
            } else {
                window.location.href = urlSearch;
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            jAlert('Close batch is fail.', ecmTitleMessage)
            $body.ecm_loading_hide();
        }
    });
}

function SaveBatch(successCallback, errorCallback) {
    var batchInfo = {};
    var $liBatch = $('#' + activeBatchId);

    // Get info batch
    batchInfo.Id = $liBatch.attr('id');
    batchInfo.Name = $liBatch.children('.item-content').children('.item-text').children('.item-title').text();
    batchInfo.IsReject = $liBatch.hasClass('reject');

    // Get info batch index
    batchInfo.Indexes = [];
    var $batchIndex = $(idBatchIndexPrefix + batchInfo.Id);
    // Just get index when have permission modify index
    var canModifyIndex = $batchIndex.data('can-modify-index');
    if (canModifyIndex == 'True')
        // Loop all index field
        $batchIndex.children('.current_content_fields').children('.content_fields').each(function () {
            var indexInfo = {};
            var $divInput = $(this).children().slice(1);
            var $input = $divInput.children('input');

            indexInfo.Id = $input.attr('id');
            if ($input.hasClass('date'))
                indexInfo.Value = $input.data('date-value');
            else if ($input.hasClass('boolean'))
                indexInfo.Value = $input.is(":checked") ? 'True' : 'False';
            else
                indexInfo.Value = $input.val();

            batchInfo.Indexes.push(indexInfo);
        });

    // Get info batch comment
    batchInfo.Comments = [];
    // Loop all new comment
    $(idBatchCommentPrefix + batchInfo.Id).find('.new-comment').each(function () {
        var commentInfo = {};
        var $trComment = $(this);

        commentInfo.CreateDate = $trComment.children('.td-comment-create').children('.comment-create-on').text();
        var note = $trComment.children('.td-comment-note').children().html();
        var arrResult = [];
        SplitNode($('<div>' + note + '</div>'), '', arrResult);
        var $note = $('<div></div>');
        for (var i = 0; i < arrResult.length; i++) {
            $note.append(arrResult[i]);
        }
        commentInfo.Note = $note.html();

        batchInfo.Comments.push(commentInfo);
    });

    // Get info document
    batchInfo.Documents = [];
    // Loop all doc
    $liBatch.children('.item-children').children().children().each(function () {
        var docInfo = {};
        var $liDoc = $(this);

        docInfo.Id = $liDoc.attr('id');
        docInfo.Name = $liDoc.children('.item-content').children('.item-text').children('.item-title').children('.item-doc-title').data('doc-name');
        docInfo.IsReject = $liDoc.hasClass('reject');
        docInfo.Scale = $liDoc.data('scale');

        // Get info doc index
        docInfo.Indexes = [];
        var $docIndex = $(idDocIndexPrefix + docInfo.Id);
        // Just get index when have permission modify index
        if (canModifyIndex == 'True')
            // Loop all index field
            $docIndex.children('.current_content_fields').children('.content_fields').each(function () {
                var indexInfo = {};
                var $divInput = $(this).children().slice(1);
                var $input = $divInput.children('input');

                if ($input.length > 0) {
                    indexInfo.Id = $input.attr('id');
                    if ($input.hasClass('date'))
                        indexInfo.Value = $input.data('date-value');
                    else if ($input.hasClass('boolean'))
                        indexInfo.Value = $input.is(":checked") ? 'True' : 'False';
                    else
                        indexInfo.Value = $input.val();
                }
                    // No input tag => case field type picklist or table
                else {
                    var $select = $divInput.children('select');
                    if ($select.length > 0) {
                        indexInfo.Id = $select.attr('id');
                        indexInfo.Value = $select.val();
                    }
                        // No select tag => case field type table
                    else {
                        indexInfo.Id = $divInput.attr('id');

                        indexInfo.Rows = [];
                        // Loop all rows in table
                        var $rows = $('#tbl-' + indexInfo.Id).find('.real-row');
                        for (var i = 0; i < $rows.size() ; i++) {
                            var rowInfo = {};
                            var $row = $($rows[i]);

                            rowInfo.Cols = [];
                            // Loop all column in row
                            var $cols = $row.children('.td-data');
                            var flgHaveValue = false;
                            for (var j = 0; j < $cols.size() ; j++) {
                                var colInfo = {};
                                var $input = $($cols[j]).children().children('input');

                                colInfo.Id = $input.attr('id');
                                colInfo.FieldId = $input.data('field-id');
                                if ($input.hasClass('date'))
                                    colInfo.Value = $input.data('date-value');
                                else
                                    colInfo.Value = $input.val();

                                if (colInfo.Value != undefined && colInfo.Value.trim() != '')
                                    flgHaveValue = true;

                                rowInfo.Cols.push(colInfo);
                            }

                            if (flgHaveValue)
                                indexInfo.Rows.push(rowInfo);
                        }
                    }
                }
                docInfo.Indexes.push(indexInfo);
            });

        // Get info page
        docInfo.Pages = [];
        // Loop all pages
        $liDoc.children('.item-children').children().children().each(function myfunction() {
            var pageInfo = {};
            var $liPage = $(this);

            pageInfo.Id = $liPage.attr('id');
            pageInfo.IsReject = $liPage.hasClass('reject');
            pageInfo.LanguageCode = $liPage.data('language-code');
            pageInfo.RotateAngle = $liPage.data('rotate-angle');;
            pageInfo.OldDocId = $liPage.data('old-doc-id');

            var $viewerPage = $(idViewerPagePrefix + pageInfo.Id);
            pageInfo.DeleteAnnotations = $viewerPage.data('del-annoes');

            // Get info annotation
            pageInfo.Annotations = [];
            $viewerPage.children('.anno').each(function myfunction() {
                var annoInfo = {};
                var $anno = $(this);

                annoInfo.Id = $anno.attr('id');
                var left = $anno.css('left');
                annoInfo.Left = left.substr(0, left.length - 2);
                var top = $anno.css('top');
                annoInfo.Top = top.substr(0, top.length - 2);
                annoInfo.Width = $anno.outerWidth();
                annoInfo.Height = $anno.outerHeight();

                if ($anno.hasClass('Highlight'))
                    annoInfo.Type = 'Highlight';
                else if ($anno.hasClass('Redaction'))
                    annoInfo.Type = 'Redaction';

                var $textInner = $anno.children('.anno-text-inner');
                if ($textInner.length > 0) {
                    annoInfo.Type = 'Text';
                    annoInfo.RotateAngle = $anno.data('rotate-angle');
                    annoInfo.Content = $textInner.html();
                }

                pageInfo.Annotations.push(annoInfo);
            });

            docInfo.Pages.push(pageInfo);
        });

        batchInfo.Documents.push(docInfo);
    });

    console.log(batchInfo);

    $body.ecm_loading_show();
    $.ajax({
        url: urlSaveTempBatch,
        type: 'POST',
        data: JSON.stringify(batchInfo),
        contentType: 'application/json',
        success: function (data, textStatus, jqXHR) {

            if (successCallback != null) {
                successCallback();
            } else {
                $body.ecm_loading_hide();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (errorCallback != null) {
                errorCallback();
            } else {
                jAlert('Save batch is fail.', ecmTitleMessage)
                $body.ecm_loading_hide();
            }
        }
    });
}

// Main js star work here
$(document).ready(function () {

    $html = $('html');
    $body = $('body');
    $panelViewer = $(idPanelViewer);

    $body.ecm_loading_show({ 'background-color': 'white' });

    setTimeout(function () {
        // Initialize layout
        InitLayout();
        $body.ecm_loading_hide();

        $body.ecm_loading_show();

        // Initialize recycle bin
        InitRecycleBin();
        // Initialize input comment
        InitJQueryTEComment();
        // Initialize upload file
        InitUploadFile();
        // Initialize camera
        InitCamera();
        // Initialize opened batches menu
        InitOpenedBatchesMenu();
        // Initialize context menu for thumbnail
        InitThumbnailContextMenu();
        // Initialize context menu for viewer
        InitViewerContextMenu();

        LeftBarTabMenus_Click();
        ButtonPrevious_Click();
        ButtonNext_Click();

        $(idViewerBatchIntro).find('img').mousedown(function (e) {
            e.preventDefault();
        })

        // Control panel function
        $('#control-hide').click(function () {
            ShowOrHideAnnos();
        });
        // Control pan, select, draw anno
        $('.control-2-state').click(function () {

            var $this = $(this);
            if ($this.hasClass(classActiveTab) || $this.hasClass('disable')) {
                return;
            }

            $('.control-2-state.active').removeClass(classActiveTab);
            $this.addClass(classActiveTab)
            var clickControlId = $this.attr('id');

            // Turn on this control
            switch (clickControlId) {
                case 'control-pan':
                    $.pan('on', activeDocId);
                    break;

                case 'control-select':
                    $.select('on', activeDocId);
                    break;

                case 'control-Highlight':
                case 'control-Redaction':
                case 'control-Text':
                    $.draw('on', activeDocId, clickControlId.substr('control-'.length));
            }
        });
        // Control navigation function
        $('#control-navigation-up').click(function () {
            NavigationUp();
        });
        $('#control-navigation-down').click(function () {
            NavigationDown();
        });
        // Control rotate function
        $('#control-rotate-right').click(function () {
            if (activeType == 'page') {
                Rotate($('#view-page-' + activePageId), 90);
            } else if (activeType == 'doc') {
                $('#view-doc-' + activeDocId).children().each(function () {
                    Rotate($(this), 90);
                });
            }
        });
        $('#control-rotate-left').click(function () {
            if (activeType == 'page') {
                Rotate($('#view-page-' + activePageId), -90);
            } else if (activeType == 'doc') {
                $('#view-doc-' + activeDocId).children().each(function () {
                    Rotate($(this), -90);
                });
            }
        });
        // Control zoom
        $('#control-zoom-in').click(function () {
            switch (activeType) {
                case 'doc':
                case 'page':
                    Zoomin(activeDocId, 1);
            }
        });
        $('#control-zoom-out').click(function () {
            switch (activeType) {
                case 'doc':
                case 'page':
                    Zoomout(activeDocId, 1);
            }
        });
        $('#control-fit').click(function () {
            FitToViewer();
        });

        // Loading batch data
        OpenBatch(firstOpenBatchId);

        //// Handler delete anno
        //$(idPanelViewer).click(function () {
        //    $(idPanelViewer).focus();
        //}).keyup(function (e) {
        //    if (e.keyCode === 46) {
        //        $delAnno = $(idViewerDocPrefix + activeDocId).find('.anno.active');
        //        if ($delAnno.length > 0) {

        //            var annoId = $delAnno.attr('id');
        //            // Store real del id of anno
        //            if (annoId != undefined && annoId != '00000000-0000-0000-0000-000000000000') {
        //                var $viewerPage = $delAnno.parent();
        //                $viewerPage.data('del-annoes', $viewerPage.data('del-annoes') + ';' + annoId);
        //            }
        //            $delAnno.remove();
        //        }
        //    }
        //});
        //$('body').mousedown(function () {
        //    console.log('mouse down boy');
        //});

        // Handle save temp batch when switch to another view
        $('#main-menu-item-search').click(function (e) {
            e.preventDefault();
            var href = $(this).attr('href');
            SaveBatch(function () {
                window.location.href = href;
            });
        })

        // Handler button Save
        $('#btn-save').click(function () {
            if ($(this).hasClass('input-disable')) {
                return;
            }

            SaveBatch(
                function () {
                    CloseBatches('Close', true)
                },
                function () {
                    $body.ecm_loading_hide();
                    jAlert('Save batch is fail.', ecmTitleMessage);
                }
            );
        });
        // Handler button Submit
        $('#btn-approve').click(function () {
            if ($(this).hasClass('input-disable')) {
                return;
            }

            SaveBatch(
                function () {
                    CloseBatches('Close', true, true)
                },
                function () {
                    $body.ecm_loading_hide();
                    jAlert('Save batch is fail.', ecmTitleMessage);
                }
            );
        });


        $body.ecm_loading_hide();
    }, 1000);
});

// Function validate input integer
function AddHandlerCheckInputInteger($input, extendPattern) {

    var pattern = /^([+\-]?[0-9]{0,10})$'/;
   

    $input.on('input', null, null, function (event) {
        var currentValue = $input.val();

        if (currentValue.match(pattern))
            if (extendPattern != undefined && extendPattern != '') {
                var extendPatter = new RegExp(extendPattern);
                if (!currentValue.match(pattern))
                    $input.data('old-value', currentValue);
                else
                    $input.val($input.data('old-value'));

            } else
                $input.data('old-value', currentValue);
        else
            $input.val($input.data('old-value'));
    });
}
// Function validate input decimal
function AddHandlerCheckInputDecimal($input) {

    var pattern = /^[+\-]?[0-9]{1,10}[,.]?[0-9]{0,10}$/;

    $input.on('input', null, null, function (event) {
        var currentValue = $input.val();

        if (currentValue.match(pattern)) {
            $input.data('old-value', currentValue);
        } else {
            $input.val($input.data('old-value'));
        }

    });
}
// Function validate input date
function AddHandlerCheckInputDate($input) {

    $input.datepicker({
        dateFormat: "m/d/yy",
        beforeShow: function (input, inst) {
            var $input = $(input);
            $input.removeAttr('readonly');
        },
        onClose: function (dateText, inst) {
            var pattern1 = 'm/d/yy';
            var $input = $('#' + inst.id);
            $input.attr('readonly', 'readonly');

            try {
                var date = $.datepicker.parseDate(pattern1, $input.val());
                $input.data('date-value', date.toString('yyyy-MM-dd'));
            } catch (e) {
                var pattern2 = 'mm/dd/yy';
                try {
                    $.datepicker.parseDate(pattern2, $input.val());
                    $input.data('date-value', date.toString('yyyy-MM-dd'));
                } catch (e) {
                    $input.val('');
                    $input.data('date-value', '');
                }
            }

            $input.trigger('input');
        }
    });

}
// Function clear value in input
function AddHandlerButtonClear($button) {

    var $input = $button.parent().children('input[type="text"]');
    $button.on('click', null, null, function (event) {

        $input.data('old-value', '');
        $input.val('');
        $input.trigger('input');
        $input.focus();
    });
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

// NavigateUp and down function
function NavigationUp() {

    var $viewer = $('#panel-viewer');
    var pages = $('#view-doc-' + activeDocId).children();
    var page;

    // Find the page is have top and bottom is positive
    for (var i = 0; i < pages.length; i++) {
        page = $(pages[i]);
        if (page.position().top >= 0) {
            break;
        }
    }

    // Move previous
    var prevPage = page.prev();
    if (prevPage.length != 0) {
        $viewer.scrollTop($viewer.scrollTop() + $('#' + prevPage.attr('id')).position().top);
    }
}
function NavigationDown() {
    var $viewer = $('#panel-viewer');
    var pages = $('#view-doc-' + activeDocId).children();
    var heightDocViewer = $viewer.height();
    var page;
    var top;
    var bottom;

    // Find the page is have top and bottom is positive
    for (var i = 0; i < pages.length; i++) {
        page = $(pages[i]);
        top = page.position().top;
        bottom = top + page.height();

        if ((top > 0 && top < heightDocViewer && bottom > heightDocViewer) || (top > heightDocViewer)) {
            break;
        }
    }
    // Move next
    $viewer.scrollTop($viewer.scrollTop() + top);
}

// Zoomin function
function Zoomin(docId, plusScale) {

    var liDoc = $('#' + docId);
    var oldScale = liDoc.data('scale');
    if (oldScale == maxScale) {
        return;
    }

    var newScale = oldScale + plusScale;
    if (newScale == maxScale) {
        $('#control-zoom-in').addClass('disable');
    }
    liDoc.data('scale', newScale);

    $('#control-zoom-out').removeClass('disable');

    var docViewer = $(idViewerDocPrefix + docId);
    var scale = newScale / oldScale;

    docViewer.children('.wrapper-image').each(function () {
        Zoom($(this), scale);
    });
}
// Zoomout function
function Zoomout(docId, minusScale) {

    var liDoc = $('#' + docId);
    var oldScale = liDoc.data('scale');
    if (oldScale == minScale) {
        return;
    }

    var newScale = oldScale - minusScale;
    if (newScale == minScale) {
        $('#control-zoom-out').addClass('disable');
    }
    liDoc.data('scale', newScale);

    $('#control-zoom-in').removeClass('disable');

    var docViewer = $(idViewerDocPrefix + docId);
    var scale = newScale / oldScale;
    docViewer.children('.wrapper-image').each(function () {
        Zoom($(this), scale);
    });
}
// Zoom function
function Zoom(page, scale) {
    var widthPage = page.width();
    var heightPage = page.height();

    //  Set new size for div wrapper image
    widthPage = Math.round(widthPage * scale);
    heightPage = Math.round(heightPage * scale);
    page.width(widthPage);
    page.height(heightPage);

    //  Set new size for image
    var image = page.children('img');

    //  Set new transform orgin
    var rotateAngle = $('#' + page.attr('id').substr(idViewerPagePrefix.length - 1)).data('rotate-angle');
    var axis;
    if (rotateAngle == '90' || rotateAngle == '-270') {
        image.width(heightPage);
        image.height(widthPage);
        axis = (widthPage / 2) + 'px';
        origin = axis + ' ' + axis;
        // Set new orgin
        image.css('-moz-transform-origin', origin);
        image.css('-o-transform-origin', origin);
        image.css('-webkit-transform-origin', origin);
        image.css('-ms-transform-origin', origin);
        image.css('transform-origin', origin);
    }
    else if (rotateAngle == '-90' || rotateAngle == '270') {
        console.log('hungsdf');
        image.width(heightPage);
        image.height(widthPage);
        axis = (heightPage / 2) + 'px';
        origin = axis + ' ' + axis;
        // Set new orgin
        image.css('-moz-transform-origin', origin);
        image.css('-o-transform-origin', origin);
        image.css('-webkit-transform-origin', origin);
        image.css('-ms-transform-origin', origin);
        image.css('transform-origin', origin);
    } else {
        image.width(widthPage);
        image.height(heightPage);
    }

    // Loop annotation
    page.children('.anno').each(function () {

        var anno = $(this);
        var widthAnno = anno.width();
        var heightAnno = anno.height();
        var leftAnno = anno.css('left').replace('px', '');
        var topAnno = anno.css('top').replace('px', '');

        //   Set new value
        widthAnno = Math.round(widthAnno * scale);
        heightAnno = Math.round(heightAnno * scale);
        leftAnno = Math.round(leftAnno * scale);
        topAnno = Math.round(topAnno * scale);

        anno.width(widthAnno);
        anno.height(heightAnno);
        anno.css('left', leftAnno + 'px');
        anno.css('top', topAnno + 'px');

        //   Case anno text
        if (anno.hasClass('Text')) {
            var annoRotateAngle = anno.data('rotate-angle');
            var textInner = anno.children('.anno-text-inner');

            var axisInner
            if (annoRotateAngle == '90' || annoRotateAngle == '-270') {
                //   Set new value for div text inner
                textInner.width(heightAnno);
                textInner.height(widthAnno);

                axisInner = (widthAnno / 2) + 'px';
                origin = axisInner + ' ' + axisInner;
                // Set new orgin
                textInner.css('-moz-transform-origin', origin);
                textInner.css('-o-transform-origin', origin);
                textInner.css('-webkit-transform-origin', origin);
                textInner.css('-ms-transform-origin', origin);
                textInner.css('transform-origin', origin);
            }
            else if (annoRotateAngle == '-90' || annoRotateAngle == '270') {
                //   Set new value for div text inner
                textInner.width(heightAnno);
                textInner.height(widthAnno);

                axisInner = (heightAnno / 2) + 'px';
                origin = axisInner + ' ' + axisInner;
                // Set new orgin
                textInner.css('-moz-transform-origin', origin);
                textInner.css('-o-transform-origin', origin);
                textInner.css('-webkit-transform-origin', origin);
                textInner.css('-ms-transform-origin', origin);
                textInner.css('transform-origin', origin);
            } else {
                //   Set new value for div text inner
                textInner.width(widthAnno);
                textInner.height(heightAnno);
            }

            // Scale line-height
            var lineHeight = textInner.css('line-height').replace('px', '');
            lineHeight = Math.round(lineHeight * scale);
            textInner.css('line-height', lineHeight + 'px');

            // Loop all span to scale font-size
            textInner.children('span').each(function () {
                var span = $(this);
                var fontSize = span.css('font-size').replace('px', '');

                fontSize = Math.round(fontSize * scale);
                span.css('font-size', fontSize + 'px');
            });
        }
    });
}

// Fit to viewer
function FitToViewer() {
    var $viewer = $('#panel-viewer');
    var pages = $('#view-doc-' + activeDocId).children();
    var heightDocViewer = $viewer.height();
    var page;
    var top;
    var bottom;

    // Find the page is have top and bottom is positive
    for (var i = 0; i < pages.length; i++) {
        page = $(pages[i]);
        top = page.position().top;
        bottom = top + page.height();

        if ((top <= 0 && bottom >= heightDocViewer) ||
            top >= 0 && bottom <= heightDocViewer)
            break;
        else if (top >= 0 && bottom >= heightDocViewer) {
            page = $(pages[i - 1]);
            break;
        }
    }

    var pageWidth = page.width();
    var pageHeight = page.height();
    var widthDocViewer = $viewer.width();

    // Case width is out of viewer, height is not
    if (pageWidth >= widthDocViewer && pageHeight <= heightDocViewer) {
        FitToWidth(page);

        // Case height is out of viewer, width is not
    } else if (pageHeight >= heightDocViewer && pageWidth <= widthDocViewer) {
        FitToHeight(page);

        // Case both width and height are out of viewer, or both are not
    } else {

        // Calculate scale
        var $liDoc = $('#' + activeDocId);
        var currentScale = $liDoc.data('scale');

        var newScaleWidth = Math.round((widthDocViewer - 50) * currentScale / pageWidth);
        var newScaleHeigth = Math.round((heightDocViewer - 50) * currentScale / pageHeight);

        // Adjust newScaleWidth
        if (newScaleWidth < minScale)
            newScaleWidth = minScale;
        else if (newScaleWidth > maxScale)
            newScaleWidth = maxScale;
        // Adjust newScaleHeight
        if (newScaleHeigth < minScale)
            newScaleHeigth = minScale;
        else if (newScaleHeigth > maxScale)
            newScaleHeigth = maxScale;

        if (newScaleWidth != currentScale) {
            if (newScaleWidth < currentScale) {
                Zoomout(activeDocId, currentScale - Math.min(newScaleWidth, newScaleHeigth));
            } else {
                Zoomin(activeDocId, Math.min(newScaleWidth, newScaleHeigth) - currentScale);
            }
        }
        // Scroll
        $viewer.scrollTop($viewer.scrollTop() + page.position().top);

    }
}
// Fit to width
function FitToWidth($page) {
    var $viewer = $('#panel-viewer');
    var widthDocViewer = $viewer.width();
    var pageWidth = $page.width();

    // Calculate scale
    var $liDoc = $('#' + activeDocId);
    var currentScale = $liDoc.data('scale');
    var newScale = Math.round((widthDocViewer - 50) * currentScale / pageWidth);
    // Adjust newScale
    if (newScale < minScale)
        newScale = minScale;
    else if (newScale > maxScale)
        newScale = maxScale;

    if (newScale != currentScale) {
        if (newScale < currentScale) {
            Zoomout(activeDocId, currentScale - newScale);
        } else {
            Zoomin(activeDocId, newScale - currentScale);
        }
    }

    // Scroll
    $viewer.scrollTop($viewer.scrollTop() + $page.position().top);
}
// Fit to height
function FitToHeight($page) {
    var $viewer = $('#panel-viewer');
    var heightDocViewer = $viewer.height();
    var pageHeight = $page.height();

    // Calculate scale
    var $liDoc = $('#' + activeDocId);
    var currentScale = $liDoc.data('scale');
    var newScale = Math.round((heightDocViewer - 50) * currentScale / pageHeight);
    // Adjust newScale
    if (newScale < minScale)
        newScale = minScale;
    else if (newScale > maxScale)
        newScale = maxScale;

    if (newScale != currentScale) {
        if (newScale < currentScale) {
            Zoomout(activeDocId, currentScale - newScale);
        } else {
            Zoomin(activeDocId, newScale - currentScale);
        }
    }

    // Scroll
    $viewer.scrollTop($viewer.scrollTop() + $page.position().top);
}

// Rotate function
function Rotate(pageViewer, angle) {

    if (pageViewer.hasClass('wrapper-image-native-file')) {
        return;
    }

    var widthPage = pageViewer.width();
    var heightPage = pageViewer.height();

    var image = pageViewer.children().first();
    var $liPage = $('#' + pageViewer.attr('id').substr(idViewerPagePrefix.length - 1));
    console.log(pageViewer.attr('id'));
    var rotateAngleImage = $liPage.data('rotate-angle');
    rotateAngleImage += angle;

    // Adjust angle for angle is 0 <= angle <= 360
    if (rotateAngleImage < 0) {
        do {
            rotateAngleImage += 360;
        } while (rotateAngleImage < 0);
    }
    else if (rotateAngleImage > 359) {
        do {
            rotateAngleImage -= 360;
        } while (rotateAngleImage > 359);
    }

    // Rotate thumbnail
    var thumb = $('#' + pageViewer.attr('id').substr(10)).children().children().first().children('.image-main');
    thumb.removeAttr('class').addClass('image-main').addClass('rotate-' + rotateAngleImage);

    // Rotate div image wrapper by swap width and height
    pageViewer.width(heightPage);
    pageViewer.height(widthPage);

    // Update rotate status
    $liPage.data('rotate-angle', rotateAngleImage);
    image.removeClass('rotate-0').removeClass('rotate-90').removeClass('rotate-180').removeClass('rotate-270');
    image.addClass('rotate-' + rotateAngleImage);

    //image.removeAttr('style');
    var axis;
    var origin
    if (rotateAngleImage == 90) {
        axis = heightPage / 2 + 'px';
    } else if (rotateAngleImage == 270) {
        axis = widthPage / 2 + 'px';
    } else {
        axis = '50%';
    }

    origin = axis + ' ' + axis;
    // Set new orgin
    image.css('-moz-transform-origin', origin);
    image.css('-o-transform-origin', origin);
    image.css('-webkit-transform-origin', origin);
    image.css('-ms-transform-origin', origin);
    image.css('transform-origin', origin);

    // Loop annotation
    pageViewer.children('.anno').each(function () {

        var anno = $(this);
        var widthAnno = anno.width();
        var heightAnno = anno.height();
        var leftAnno = anno.css('left').replace('px', '');
        var topAnno = anno.css('top').replace('px', '');
        var tempLeft;
        var tempTop;

        var rotateAngleAnno = anno.data('rotate-angle');
        rotateAngleAnno += angle;

        // Adjust angle for angle is 0 <= angle <= 360
        if (rotateAngleAnno < 0) {
            do {
                rotateAngleAnno += 360;
            } while (rotateAngleAnno < 0);
        }
        else if (rotateAngleAnno > 359) {
            do {
                rotateAngleAnno -= 360;
            } while (rotateAngleAnno > 359);
        }

        // Calculate new left top
        if (angle == 90) {
            tempLeft = heightPage - topAnno - heightAnno;
            tempTop = leftAnno;
        } else {
            tempLeft = topAnno;
            tempTop = widthPage - leftAnno - widthAnno;
        }
        leftAnno = tempLeft;
        topAnno = tempTop;

        // Rotate div image wrapper by swap width and height, left and top
        anno.width(heightAnno);
        anno.height(widthAnno);
        anno.css('left', leftAnno + 'px');
        anno.css('top', topAnno + 'px');

        // Update rotate status
        anno.data('rotate-angle', rotateAngleAnno);

        // Case anno text
        if (anno.hasClass('Text')) {
            var textInner = anno.children('.anno-text-inner');

            // Update rotate status
            textInner.removeClass('rotate-0').removeClass('rotate-90').removeClass('rotate-180').removeClass('rotate-270');
            textInner.addClass('rotate-' + rotateAngleAnno);

            //image.removeAttr('style');
            var axis;
            var origin
            if (rotateAngleAnno == 90) {
                axis = heightAnno / 2 + 'px';
            } else if (rotateAngleAnno == 270) {
                axis = widthAnno / 2 + 'px';
            } else {
                axis = '50%';
            }

            origin = axis + ' ' + axis;
            // Set new orgin
            textInner.css('-moz-transform-origin', origin);
            textInner.css('-o-transform-origin', origin);
            textInner.css('-webkit-transform-origin', origin);
            textInner.css('-ms-transform-origin', origin);
            textInner.css('transform-origin', origin);
        }
    });
}

(function ($) {

    var $html;
    var $body;
    var $viewPanel;
    var $activeAnno;
    var $jqteEditorAnno;
    var $annoDraw;

    // Initialize jquery variables
    $(document).ready(function () {
        $html = $('html');
        $body = $('body');
        $viewPanel = $('#panel-viewer');
        $annoDraw = $('<div>').addClass(classEnum.anno).addClass(classEnum.draw);
    });

    // Store the current status is paning or
    var status = '';
    var statusEnum = {
        pan: 'pan',
        panning: 'panning',
        select: 'select',
        editing: 'editing',
        drawHighlight: 'Highlight',
        drawRedaction: 'Redaction',
        drawText: 'Text',
        drawing: 'drawing'
    };

    // Enum of class which is use to add to view panel
    var classEnum = {
        panHover: 'pan-hover',
        panDown: 'pan-down',
        select: 'select',
        anno: 'anno',
        highlight: 'Highlight',
        redaction: 'Redaction',
        text: 'Text',
        textInner: 'anno-text-inner',
        newAnno: 'new',
        canDelHighlight: 'can-del-highlight',
        canDelText: 'can-del-text',
        wrapperImage: 'wrapper-image',
        offHover: 'off-hover',
        active: 'active',
        tagCss: 'anno-text',
        edit: 'edit',
        draw: 'draw'
    };

    // Enum of available action
    var actionEnum = {
        on: 'on',
        off: 'off'
    };

    // Enum of data attribute
    var dataEnum = {
        statusControl: 'status-control'
    };

    // Enum of prefix
    var prefixEnum = {
        viewDocId: '#view-doc-'
    };

    // Store position of mouse
    var mouse = {
        pageX: 0,
        pageY: 0,
        offsetLeft: 0,
        offsetTop: 0,
        offsetRight: 0,
        offsetBottom: 0
    };

    var oldDocStatus = {
        docId: '',
        status: ''
    };

    var editorSettings = {
        minWidth: 150,
        minHeight: 70
    };

    $.pan = function (action, docId) {

        // Turn on pan
        if (action == actionEnum.on) {

            // Turn of current control of previous doc
            TurnOffControl();

            // Save status
            oldDocStatus = {
                docId: docId,
                status: statusEnum.pan
            }
            $('#' + oldDocStatus.docId).data(dataEnum.statusControl, oldDocStatus.status);

            $viewPanel.addClass(classEnum.panHover);
            $viewPanel.on('mousedown', null, null, PanViewPanelMouseDown);
            $body.on('mousemove', null, null, PanBodyMouseMove)
                 .on('mouseup', null, null, PanBodyMouseUp);
        }

        // Turn off pan
        if (action == actionEnum.off) {
            $viewPanel.removeClass(classEnum.panHover);
            $viewPanel.off('mousedown', null, PanViewPanelMouseDown);
            $body.off('mousemove', null, PanBodyMouseMove)
                 .off('mouseup', null, PanBodyMouseUp);
        }
    };
    function PanViewPanelMouseDown(e) {
        console.log('PanMouseDown');
        // Prevent not click by left mouse
        if (e.which !== 1)
            return

            // Prevent click on scrollbar
        else if ($viewPanel.offset().left + $viewPanel[0].clientWidth <= e.pageX)
            return;
        else if ($viewPanel.offset().top + $viewPanel[0].clientHeight <= e.pageY)
            return;

            // Prevent duplication mouse down
            // Ex: mouse down => move out of browser and mouse up
            // => move in again and mouse down
        else if (status == statusEnum.panning)
            return;

        status = statusEnum.panning;

        // Store first click position
        mouse.pageX = e.pageX;
        mouse.pageY = e.pageY;

        // Disable zoom
        $html.addClass(classEnum.panDown).on('mousewheel DOMMouseScroll', null, null, DisableZoom);
        SetActiveViewPanel();
    };
    function PanBodyMouseMove(e) {
        console.log('PanMouseMove');
        if (status != statusEnum.panning)
            return;

        // Scrolling
        $viewPanel.scrollLeft($viewPanel.scrollLeft() - (e.pageX - mouse.pageX));
        $viewPanel.scrollTop($viewPanel.scrollTop() - (e.pageY - mouse.pageY));

        // Update position
        mouse.pageX = e.pageX;
        mouse.pageY = e.pageY;
    };
    function PanBodyMouseUp(e) {
        console.log('PanMouseUp');

        if (status != statusEnum.panning)
            return;

        status = '';
        // Enable zoom
        $html.removeClass(classEnum.panDown).off('mousewheel DOMMouseScroll', null, DisableZoom);
        UnSetActiveViewPanel();
    };

    $.select = function (action, docId) {

        var $viewDoc = $(prefixEnum.viewDocId + docId);
        // Create selector anno
        var selectorAnno = '.' + classEnum.anno + '.' + classEnum.newAnno
                               + ',.' + classEnum.anno + '.' + classEnum.redaction;
        if ($viewDoc.hasClass(classEnum.canDelHighlight))
            selectorAnno += ',.' + classEnum.anno + '.' + classEnum.highlight;
        if ($viewDoc.hasClass(classEnum.canDelText))
            selectorAnno += ',.' + classEnum.anno + '.' + classEnum.text;

        // Get all annoes of this current doc
        var $annoes = $viewDoc.children('.' + classEnum.wrapperImage).children(selectorAnno);
        var $annoesText = $annoes.filter('.' + classEnum.text);

        // Turn on select
        if (action == actionEnum.on) {

            // Turn of current control of previous doc
            TurnOffControl();

            oldDocStatus = {
                docId: docId,
                status: statusEnum.select
            }
            $('#' + oldDocStatus.docId).data(dataEnum.statusControl, oldDocStatus.status);

            // Turn on drag-gable
            $annoes.draggable({
                containment: 'parent',
                scroll: false,
                start: function (e, ui) {
                    ui.helper.parent().parent().addClass(classEnum.offHover);
                    $html.addClass(classEnum.select).on('mousewheel DOMMouseScroll', null, null, DisableZoom);
                    SetActiveViewPanel();
                },
                stop: function (e, ui) {
                    ui.helper.parent().parent().removeClass(classEnum.offHover);
                    $html.removeClass(classEnum.select).off('mousewheel DOMMouseScroll', null, DisableZoom);
                    UnSetActiveViewPanel();
                }
            });

            // Turn on re-sizable
            $annoes.resizable({
                containment: 'parent',
                disabled: true,
                handles: "all",
                start: function (e, ui) {
                    ui.element.parent().parent().addClass(classEnum.offHover);
                    $html.addClass(classEnum.select).on('mousewheel DOMMouseScroll', null, null, DisableZoom);
                    SetActiveViewPanel();
                },
                resize: function (e, ui) {
                    if (!ui.element.hasClass('Text')) {
                        return;
                    }

                    var rotateAngle = ui.element.data('rotate-angle');
                    var width = ui.element.width();
                    var height = ui.element.height();
                    var $innerText = ui.element.children('.anno-text-inner');

                    // Resize the div inner text
                    if (rotateAngle == 90 || rotateAngle == 270 || rotateAngle == -90 || rotateAngle == -270)
                        $innerText.width(height).height(width);
                    else
                        $innerText.width(width).height(height);

                    var axisInner
                    // Calculate transform orgin after resize
                    if (rotateAngle == '90' || rotateAngle == '-270') {
                        axisInner = (width / 2) + 'px';
                        var origin = axisInner + ' ' + axisInner;
                        $innerText.css('-moz-transform-origin', origin);
                        $innerText.css('-o-transform-origin', origin);
                        $innerText.css('-webkit-transform-origin', origin);
                        $innerText.css('-ms-transform-origin', origin);
                        $innerText.css('transform-origin', origin);
                    }
                    else if (rotateAngle == '-90' || rotateAngle == '270') {
                        axisInner = (height / 2) + 'px';
                        var origin = axisInner + ' ' + axisInner;
                        $innerText.css('-moz-transform-origin', origin);
                        $innerText.css('-o-transform-origin', origin);
                        $innerText.css('-webkit-transform-origin', origin);
                        $innerText.css('-ms-transform-origin', origin);
                        $innerText.css('transform-origin', origin);
                    }

                },
                stop: function (e, ui) {
                    ui.element.parent().parent().removeClass(classEnum.offHover);
                    $html.removeClass(classEnum.select).off('mousewheel DOMMouseScroll', null, DisableZoom);
                    UnSetActiveViewPanel();
                }
            }).resizable('disable');

            $viewDoc.addClass(classEnum.select);
            $annoes.on('mousedown', null, null, SelectAnnoMouseDown);
            $annoesText.on('dblclick', null, null, SelectAnnoTextDoubleClick)
        }

        // Turn off select
        if (action == actionEnum.off) {
            // Raise focusout of jqteEditor if anno text is editing
            if (status == statusEnum.editing) {
                $jqteEditorAnno.trigger('focusout');
                while (status == statusEnum.editing) { }
            }

            if ($activeAnno != undefined) {
                $activeAnno.removeClass(classEnum.active);
                $activeAnno = undefined;
            }

            $annoes.draggable('destroy');
            $annoes.resizable('destroy');
            $viewDoc.removeClass(classEnum.select);
            $annoes.off('mousedown', null, SelectAnnoMouseDown);
            $annoesText.off('dblclick', null, SelectAnnoTextDoubleClick);
        }
    };
    function SelectAnnoMouseDown(e) {

        console.log('SelectMouseDown');
        // Prevent not click by left mouse
        if (e.which !== 1)
            return

        // Reset class of previous active anno
        if ($activeAnno != null) {
            //  Click on its self again
            if ($activeAnno[0] == this)
                return;

            $activeAnno.removeClass(classEnum.active).resizable('disable');
        }

        // Raise focusout of jqteEditor if anno text is editing
        if (status == statusEnum.editing) {
            $jqteEditorAnno.trigger('focusout');
            while (status == statusEnum.editing) { }
        }

        $activeAnno = $(this);
        // Set active class for clicked anno
        $activeAnno.addClass(classEnum.active).resizable('enable');
    };
    function SelectAnnoTextDoubleClick(e) {
        console.log('SelectTextDoubleClick');
        status = statusEnum.editing;

        var $annoEdit = $('<div><div></div></div>').addClass(classEnum.anno).addClass(classEnum.edit);
        $annoEdit.attr('style', $activeAnno.attr('style'));

        var $page = $activeAnno.parent();
        $activeAnno.hide();
        $page.append($annoEdit);

        // Inite jQueryTE editor
        var $editor = $annoEdit.children();
        $editor.jqte({
            center: false,
            fsizes: ['8', '9', '10', '11', '12', '14', '16', '18', '20', '24', '26', '28', '36', '72'],
            format: false,
            indent: false,
            link: false,
            left: false,
            ol: false,
            outdent: false,
            remove: false,
            right: false,
            rule: false,
            source: false,
            sub: false,
            strike: false,
            sup: false,
            ul: false,
            unlink: false,
            tagCss: classEnum.tagCss,
        });

        // Get element jqte after init
        var $jqte = $('.jqte.' + classEnum.tagCss);
        var $jqteToolbar = $('.jqte_toolbar.' + classEnum.tagCss);
        $jqteEditorAnno = $('.jqte_editor.' + classEnum.tagCss);
        $annoEdit.mousedown(function (e) {
            e.stopPropagation();
        });

        // Adjust default size of jqte
        if ($annoEdit.outerWidth() < editorSettings.minWidth) {
            $annoEdit.outerWidth(editorSettings.minWidth);
        }
        if ($annoEdit.outerHeight() < editorSettings.minHeight) {
            $annoEdit.outerHeight(editorSettings.minHeight);
        }
        $jqteEditorAnno.outerHeight($jqte.outerHeight() - $jqteToolbar.outerHeight());

        // Resize height of editor when main jqte resized
        $annoEdit.resize(function () {
            $jqteEditorAnno.outerHeight($jqte.outerHeight() - $jqteToolbar.outerHeight());
        }).resizable({
            minWidth: editorSettings.minWidth,
            minHeight: editorSettings.minHeight
        });

        $jqteEditorAnno.html($activeAnno.children('.' + classEnum.textInner).html());
        $viewPanel.on('mousedown', null, null, SelectViewPanelMouseDown);
        $jqteEditorAnno.focus().one('focusout', null, null, function (e) {

            $viewPanel.off('mousedown', null, SelectViewPanelMouseDown);

            // Get content input in editor
            var content = $(this).html();
            var countCharacter = CountCharacter($('<div>' + content + '</div>'));

            // Remove editor
            $(this).parent().parent().remove();

            // If input content, paste it to div anno text
            if (countCharacter > 0) {
                arrResult = [];
                SplitNode($('<div>' + content + '</div>'), '', arrResult);

                var $innerText = $activeAnno.children('.' + classEnum.textInner)
                $innerText.html('');
                for (var i = 0; i < arrResult.length; i++) {
                    $innerText.append(arrResult[i]);
                }

                $activeAnno.show();
            } else {
                $activeAnno.remove();
                $activeAnno = undefined;
            }

            status = '';
            $jqteEditorAnno = undefined;
        });
    };
    function SelectViewPanelMouseDown(e) {
        console.log('SelectViewPanelMouseDown');
        // Prevent click on scrollbar
        if ($viewPanel.offset().left + $viewPanel[0].clientWidth <= e.pageX)
            return;
        else if ($viewPanel.offset().top + $viewPanel[0].clientHeight <= e.pageY)
            return;
        else if (status != statusEnum.editing)
            return;

        $jqteEditorAnno.trigger('focusout');
    };

    $.draw = function (action, docId, annoKind, isReset) {

        var $viewDoc = $(prefixEnum.viewDocId + docId);
        // Get all annoes of this current doc
        var $pages = $viewDoc.children('.' + classEnum.wrapperImage);
        var $annoes = $pages.children('.' + classEnum.anno);

        // Turn on draw
        if (action == actionEnum.on) {
            var oldStatus = oldDocStatus.status
            if (docId == oldDocStatus.docId && (oldStatus == statusEnum.drawHighlight
                                                || oldStatus == statusEnum.drawRedaction
                                                || oldStatus == statusEnum.drawText)) {
                oldDocStatus.status = annoKind;
                $('#' + oldDocStatus.docId).data(dataEnum.statusControl, oldDocStatus.status);

                if (isReset !== true)
                    return;
            }

            // Turn of current control of previous doc
            TurnOffControl();

            oldDocStatus = {
                docId: docId,
                status: annoKind
            }
            $('#' + oldDocStatus.docId).data(dataEnum.statusControl, oldDocStatus.status);

            $viewDoc.addClass(classEnum.draw);
            $annoes.on('mousedown', null, null, DrawAnnoMouseDown);
            $pages.on('mousedown', null, null, DrawPageMouseDown);
            $body.on('mousemove', null, null, DrawBodyMouseMove)
                 .on('mouseup', null, null, DrawBodyMouseUp);
        }

        // Turn off draw
        if (action == actionEnum.off) {
            // Raise focusout of jqteEditor if anno text is editing
            if (status == statusEnum.editing) {
                $jqteEditorAnno.trigger('focusout');
                while (status == statusEnum.editing) { }
            }

            if ($activeAnno != undefined) {
                $activeAnno.removeClass(classEnum.active).draggable('destroy').resizable('destroy');
                $activeAnno = undefined;
            }

            $viewDoc.removeClass(classEnum.draw);
            $annoes.off('mousedown', null, DrawAnnoMouseDown);
            $pages.off('mousedown', null, DrawPageMouseDown);
            $body.off('mousemove', null, DrawBodyMouseMove)
                 .off('mouseup', null, DrawBodyMouseUp);
        }
    };
    $.fn.draw = function () {
        this.on('mousedown', null, null, DrawPageMouseDown);
    };
    function DrawAnnoMouseDown(e) {
        e.stopPropagation();
    };
    function DrawPageMouseDown(e) {
        console.log('DrawPageMouseDown');
        // Prevent not click by left mouse
        if (e.which !== 1)
            return

            // Prevent duplication mouse down
            // Ex: mouse down => move out of browser and mouse up
            // => move in again and mouse down
        else if (status == statusEnum.drawing)
            return;

        // Raise focusout of jqteEditor if anno text is editing
        if (status == statusEnum.editing) {
            $jqteEditorAnno.trigger('focusout');
            while (status == statusEnum.editing) { }
        }

        if ($activeAnno != undefined) {
            $activeAnno.removeClass(classEnum.active).draggable('destroy').resizable('destroy');
            $activeAnno = undefined;
        }

        status = statusEnum.drawing;

        // Disable zoom
        $html.addClass(classEnum.draw).on('mousewheel DOMMouseScroll', null, null, DisableZoom);
        SetActiveViewPanel();

        $page = $(this);
        pageOffset = $page.offset();
        mouse.offsetLeft = pageOffset.left;
        mouse.offsetTop = pageOffset.top;
        mouse.offsetRight = mouse.offsetLeft + $page.width();
        mouse.offsetBottom = mouse.offsetTop + $page.height();

        // Store first click position
        mouse.pageX = e.pageX;
        mouse.pageY = e.pageY;

        $annoDraw.css({
            'left': mouse.pageX - mouse.offsetLeft,
            'top': mouse.pageX - mouse.offsetTop,
            'width': 0,
            'height': 0
        });
        $annoDraw.appendTo($page);
    };
    function DrawBodyMouseMove(e) {
        console.log('DrawBodyMouseMove');
        if (status != statusEnum.drawing) {
            return;
        }

        move_x = e.pageX;
        move_y = e.pageY;

        if (move_x > mouse.offsetRight) {
            move_x = mouse.offsetRight;
        } else if (move_x < mouse.offsetLeft) {
            move_x = mouse.offsetLeft;
        }

        if (move_y > mouse.offsetBottom) {
            move_y = mouse.offsetBottom;
        } else if (move_y < mouse.offsetTop) {
            move_y = mouse.offsetTop;
        }

        var width = Math.abs(move_x - mouse.pageX);
        var height = Math.abs(move_y - mouse.pageY);

        new_x = (move_x < mouse.pageX) ? mouse.pageX - width : mouse.pageX;
        new_y = (move_y < mouse.pageY) ? mouse.pageY - height : mouse.pageY;
        console.log(width);
        $annoDraw.css({
            'left': new_x - mouse.offsetLeft,
            'top': new_y - mouse.offsetTop,
            'width': width,
            'height': height
        });
    };
    function DrawBodyMouseUp(e) {
        console.log('DrawBodyMouseUp');
        if (status != statusEnum.drawing)
            return;

        status = '';
        // Enable zoom
        $html.removeClass(classEnum.draw).off('mousewheel DOMMouseScroll', null, DisableZoom);
        UnSetActiveViewPanel();

        var outerWidth = $annoDraw.outerWidth();
        var outerHeight = $annoDraw.outerHeight();
        if (outerWidth <= 2 || outerHeight <= 2) {
            $annoDraw.remove();
            return;
        }

        var $annoNew = $annoDraw.clone();
        $annoNew.removeClass(classEnum.draw).addClass(oldDocStatus.status);
        $annoDraw.parent().append($annoNew);
        $annoDraw.remove();

        $annoNew.on('mousedown', null, null, DrawAnnoMouseDown);
        $activeAnno = $annoNew;
        $activeAnno.addClass(classEnum.active);

        if (oldDocStatus.status == statusEnum.drawText) {
            var $textInner = $('<div class="rotate-0">').addClass(classEnum.textInner);
            $textInner.css({
                width: outerWidth - 2,
                height: outerHeight - 2
            });
            $activeAnno.append($textInner);
        }

        // Add style hover and turn on drag-able
        $activeAnno.draggable({
            containment: 'parent',
            scroll: false,
            start: function (e, ui) {
                ui.helper.parent().parent().addClass(classEnum.offHover);
                $html.addClass(classEnum.select).on('mousewheel DOMMouseScroll', null, null, DisableZoom);
                SetActiveViewPanel();
            },
            stop: function (event, ui) {
                ui.helper.parent().parent().removeClass(classEnum.offHover);
                $html.removeClass(classEnum.select).off('mousewheel DOMMouseScroll', null, DisableZoom);
                UnSetActiveViewPanel();
            }
        });
        // Turn on re-sizable
        $activeAnno.resizable({
            containment: 'parent',
            handles: "all",
            start: function (e, ui) {
                ui.element.parent().parent().addClass(classEnum.offHover);
                $html.addClass(classEnum.select).on('mousewheel DOMMouseScroll', null, null, DisableZoom);
                SetActiveViewPanel();
            },
            resize: function (e, ui) {

                if (!ui.element.hasClass('Text')) {
                    return;
                }

                var rotateAngle = ui.element.data('rotate-angle');
                var width = ui.element.width();
                var height = ui.element.height();
                var $innerText = ui.element.children('.anno-text-inner');

                // Resize the div inner text
                if (rotateAngle == 90 || rotateAngle == 270 || rotateAngle == -90 || rotateAngle == -270)
                    $innerText.width(height).height(width);
                else
                    $innerText.width(width).height(height);

                var axisInner
                // Calculate transform orgin after resize
                if (rotateAngle == '90' || rotateAngle == '-270') {
                    axisInner = (width / 2) + 'px';
                    var origin = axisInner + ' ' + axisInner;
                    $innerText.css('-moz-transform-origin', origin);
                    $innerText.css('-o-transform-origin', origin);
                    $innerText.css('-webkit-transform-origin', origin);
                    $innerText.css('-ms-transform-origin', origin);
                    $innerText.css('transform-origin', origin);
                }
                else if (rotateAngle == '-90' || rotateAngle == '270') {
                    axisInner = (height / 2) + 'px';
                    var origin = axisInner + ' ' + axisInner;
                    $innerText.css('-moz-transform-origin', origin);
                    $innerText.css('-o-transform-origin', origin);
                    $innerText.css('-webkit-transform-origin', origin);
                    $innerText.css('-ms-transform-origin', origin);
                    $innerText.css('transform-origin', origin);
                }

            },
            stop: function (e, ui) {
                ui.element.parent().parent().removeClass(classEnum.offHover);
                $html.removeClass(classEnum.select).off('mousewheel DOMMouseScroll', null, DisableZoom);
                UnSetActiveViewPanel();
            }
        });

        if (oldDocStatus.status == statusEnum.drawText) {
            console.log('statusEnum.drawText');
            SelectAnnoTextDoubleClick();
        }

    };

    function TurnOffControl() {
        console.log('TurnOffControl');
        var docId = oldDocStatus.docId;

        switch (oldDocStatus.status) {
            case statusEnum.pan:
                $.pan(actionEnum.off, docId);
                break;

            case statusEnum.select:
                $.select(actionEnum.off, docId);
                break;

            case statusEnum.drawHighlight:
            case statusEnum.drawRedaction:
            case statusEnum.drawText:
                $.draw(actionEnum.off, docId);
                break;

        }

        oldDocStatus = {
            docId: '',
            status: ''
        }
    };
    function DisableZoom(e) {
        e.preventDefault();
    };
    function SetActiveViewPanel() {
        $('#fix-header').ecm_in_active();
        $('#panel-controls').ecm_in_active();
        $('#thumbnails-groups').ecm_in_active();
        $('#panel-tabs').ecm_in_active();
    };
    function UnSetActiveViewPanel() {
        $('#fix-header').ecm_active();
        $('#panel-controls').ecm_active();
        $('#thumbnails-groups').ecm_active();
        $('#panel-tabs').ecm_active();
    };

}(jQuery));

function SetActivePanelViewer() {
    $('#fix-header').ecm_in_active();
    $(idPanelControl).ecm_in_active();
    $('#thumbnails-groups').ecm_in_active();
    $('#panel-tabs').ecm_in_active();
}
function UnsetActivePanelViewer() {
    $('#fix-header').ecm_active();
    $(idPanelControl).ecm_active();
    $('#thumbnails-groups').ecm_active();
    $('#panel-tabs').ecm_active();
}

function DisableScrollViewer(e) {
    e.preventDefault();
}

function AddHandlerForLiThumbBatch($liBatch) {
    // Get scrollable div
    var $scrollable = $("#panel-thumbnails-inner");
    var bottom = $scrollable.height();

    // Can not modify document
    if ($liBatch.data('can-modify-doc') != 'True') {
        // Hide control rotate
        $('#control-rotate-left, #control-rotate-right').hide();
    } else {

        // Can modify document
        // Show control rotate
        $('#control-rotate-left, #control-rotate-right').show();

        // Add handler sortable for ul doc
        $liBatch.find('.ul-doc').sortable({
            items: '.li-doc:not(.loose-item)',
            placeholder: 'item-select-drag',
            forcePlaceholderSize: true,
            tolerance: 'pointer',
            opacity: 0.5,
            scroll: false,
            start: function (e, ui) {

                // Clear interval
                clearInterval(handleIntervalRemoveItem);
                isInRecycleBin = false;

                // Show recycle bin if have delete permission
                if (ui.item.parent().parent().parent().data('can-del') == 'True')
                    $recyclebin.show();

                // Scroll down
                $('#panel-submit-inner').on('mouseenter', null, function () {
                    handleIntervalScroll = setInterval(function () {
                        $scrollable.scrollTop($scrollable.scrollTop() + 20);
                    }, 100);
                }).on('mouseleave', null, function () {
                    clearInterval(handleIntervalScroll);
                });
                // Scroll up 
                $('#panel-captures-left').on('mouseenter', null, function () {
                    handleIntervalScroll = setInterval(function () {
                        $scrollable.scrollTop($scrollable.scrollTop() - 20);
                    }, 100);
                }).on('mouseleave', null, function () {
                    clearInterval(handleIntervalScroll);
                });
            },
            update: function (e, ui) {

                ui.item.parent().children('.li-doc:not(.loose-item)').each(function (i, v) {
                    $(v).find('.item-doc-index').html(i + 1);
                })
            },
            stop: function (event, ui) {

                isSortStopComplete = false;

                // Hide recycle bin
                $recyclebin.hide();

                // Off scroll down/up
                $('#panel-submit-inner').off('mouseenter', null);
                $('#panel-captures-left').off('mouseenter', null);

                if (isInRecycleBin == true)
                    jConfirm(messageConfirmDeleteDoc, ecmTitleMessage, function (result) {
                        if (!result)
                            return;

                        $body.ecm_loading_show();

                        handleIntervalRemoveItem = setInterval(function () {

                            if (!isSortStopComplete)
                                return;

                            isSortStopComplete = false;
                            clearInterval(handleIntervalRemoveItem);

                            DeleteDoc(ui);

                        }, 1000);

                    });

                ui.item.children('.item-content').trigger('click');
                isSortStopComplete = true;
            }
        });
        // Add handler sortable for ul normal page
        $liBatch.find('.ul-page:not(.loose-item)').sortable({
            placeholder: 'item-select-drag',
            forcePlaceholderSize: true,
            tolerance: 'pointer',
            opacity: 0.5,
            scroll: false,
            start: function (e, ui) {

                // Clear interval
                clearInterval(handleIntervalRemoveItem);
                isInRecycleBin = false;

                // Show recycle bin if have delete permission
                if (ui.item.parent().parent().parent().parent().parent().parent().data('can-del') == 'True')
                    $recyclebin.show();

                // Scroll down
                $('#panel-submit-inner').on('mouseenter', null, function () {
                    handleIntervalScroll = setInterval(function () {
                        $scrollable.scrollTop($scrollable.scrollTop() + 20);
                    }, 100);
                }).on('mouseleave', null, function () {
                    clearInterval(handleIntervalScroll);
                });
                // Scroll up 
                $('#panel-captures-left').on('mouseenter', null, function () {
                    handleIntervalScroll = setInterval(function () {
                        $scrollable.scrollTop($scrollable.scrollTop() - 20);
                    }, 100);
                }).on('mouseleave', null, function () {
                    clearInterval(handleIntervalScroll);
                });
            },
            update: function (e, ui) {

                // Update viewer page position
                var $viewerPage = $(idViewerPagePrefix + ui.item.attr('id'));
                var desPrevLiId = ui.item.prev().attr('id');
                if (desPrevLiId != undefined)
                    $viewerPage.insertAfter(idViewerPagePrefix + desPrevLiId);
                else
                    $viewerPage.insertBefore(idViewerPagePrefix + ui.item.next().attr('id'));

                // Update label index of thumbnail page in list
                ui.item.parent().children('.li-page').each(function (i, v) {
                    $(v).find('.item-page-index').html(i + 1);
                });
            },
            stop: function (e, ui) {

                isSortStopComplete = false;

                // Hide recycle bin
                $recyclebin.hide();
                // Off scroll down/up in thumbnail
                $('#panel-submit-inner').off('mouseenter', null);
                $('#panel-captures-left').off('mouseenter', null);

                if (isInRecycleBin == true)
                    jConfirm(messageConfirmDeletePage, ecmTitleMessage, function (result) {
                        if (!result)
                            return;

                        $body.ecm_loading_show();

                        handleIntervalRemoveItem = setInterval(function () {
                            if (!isSortStopComplete)
                                return;

                            isSortStopComplete = false;
                            clearInterval(handleIntervalRemoveItem);

                            DeletePage(ui);
                        }, 1000);
                    });

                ui.item.children('.item-content').trigger('click');
                isSortStopComplete = true;
            }
        });
        // Add handler sortable for ul loose page
        $liBatch.find('.ul-page.loose-item').sortable({
            connectWith: '.ul-page,.ul-doc',
            placeholder: 'item-select-drag',
            forcePlaceholderSize: true,
            tolerance: 'pointer',
            opacity: 0.5,
            scroll: false,
            start: function (e, ui) {

                // Clear interval
                clearInterval(handleIntervalRemoveItem);
                isInRecycleBin = false;

                // Show recycle bin if have delete permission
                if (ui.item.parent().parent().parent().parent().parent().parent().data('can-del') == 'True')
                    $recyclebin.show();

                // Scroll down
                $('#panel-submit-inner').on('mouseenter', null, function () {
                    handleIntervalScroll = setInterval(function () {
                        $scrollable.scrollTop($scrollable.scrollTop() + 20);
                    }, 100);
                }).on('mouseleave', null, function () {
                    clearInterval(handleIntervalScroll);
                });
                // Scroll up 
                $('#panel-captures-left').on('mouseenter', null, function () {
                    handleIntervalScroll = setInterval(function () {
                        $scrollable.scrollTop($scrollable.scrollTop() - 20);
                    }, 100);
                }).on('mouseleave', null, function () {
                    clearInterval(handleIntervalScroll);
                });
            },
            update: function (e, ui) {
                console.log('update');
                // Case move page to doc level => append it to doc's page
                var $prevLi = ui.item.prev();
                if ($prevLi.length != 0) {
                    if ($prevLi.hasClass('li-doc'))
                        $prevLi.find('.ul-page').append(ui.item);
                } else {
                    var $nextLi = ui.item.next();
                    if ($nextLi.hasClass('li-doc'))
                        $nextLi.find('.ul-page').append(ui.item);
                }

                // Update viewer page position
                var $viewerPage = $(idViewerPagePrefix + ui.item.attr('id'));
                var desPrevLiId = ui.item.prev().attr('id');
                if (desPrevLiId != undefined)
                    $viewerPage.insertAfter(idViewerPagePrefix + desPrevLiId);
                else
                    $viewerPage.insertBefore(idViewerPagePrefix + ui.item.next().attr('id'));
                console.log(ui.item.parent());
                // Update label index of thumbnail page in list
                ui.item.parent().children('.li-page').each(function (i, v) {
                    $(v).find('.item-page-index').html(i + 1);
                });
            },
            remove: function (e, ui) {
                console.log('remove');
                var $liDoc = ui.item.parent().parent().parent();
                // Update total pages
                var $totalPages = $liDoc.children('.item-content').find('.item-doc-count');
                $totalPages.html(Number($totalPages.text()) + 1);

                // Update label index of thumbnail page in loose page list
                var $liLooseDoc = ui.item.parent().parent().parent().parent().children('.loose-item');
                var $liLoosePages = $liLooseDoc.find('.li-page');
                $liLoosePages.each(function (i, v) {
                    $(v).find('.item-page-index').html(i + 1);
                });

                // Scale page with the scale of destination doc
                var srcScale = $liLooseDoc.data('scale');
                var desScale = $liDoc.data('scale');
                if (desScale != srcScale)
                    Zoom($(idViewerPagePrefix + ui.item.attr('id')), desScale / srcScale);

                // Update reject status
                if (ui.item.hasClass('reject')) {
                    $liDoc.removeClass('accept').addClass('reject');
                    if ($liLooseDoc.find('.li-page.reject').size() == 0) {
                        $liLooseDoc.removeClass('reject');
                        if ($liLooseDoc.find('.li-page.accept').size() > 0)
                            $liLooseDoc.addClass('accept');
                    }
                }

                SetStatusPanelSubmit();
            },
            stop: function (e, ui) {
                console.log('stop');
                isSortStopComplete = false;

                // Hide recycle bin
                $recyclebin.hide();
                // Off scroll down/up in thumbnail
                $('#panel-submit-inner').off('mouseenter', null);
                $('#panel-captures-left').off('mouseenter', null);

                if (isInRecycleBin == true)
                    jConfirm(messageConfirmDeletePage, ecmTitleMessage, function (result) {
                        if (!result)
                            return;

                        $body.ecm_loading_show();

                        handleIntervalRemoveItem = setInterval(function () {
                            if (!isSortStopComplete)
                                return;

                            isSortStopComplete = false;
                            clearInterval(handleIntervalRemoveItem);

                            DeletePage(ui);
                        }, 1000);
                    });

                ui.item.children('.item-content').trigger('click');
                isSortStopComplete = true;
            }
        });
    }

    // Add handler toggle list li
    $liBatch.find('.item-content > .item-icon').click(function (e) {

        e.stopPropagation();
        var $itemContent = $(this).parent();
        $itemContent.next('.item-children').slideToggle();
        $itemContent.toggleClass('expand');
    });

    // Handler click on div item-content click
    $liBatch.find('.item-content').click({ $liBatch: $liBatch }, function (e) {

        // Hide panel comment if it is open
        HideCommentPanel(idTabThumbnail);

        var $this = $(this);
        $('.item-content.item-select').removeClass('item-select');
        $(this).addClass('item-select');

        // Get parent li
        $liParent = $this.parent();

        if ($liParent.hasClass('li-batch')) {

            // Hide control panel and view doc
            HideControlPanel();
            $('#view-doc-' + activeDocId).hide();

            // Update status
            activeType = 'batch';
            activeDocId = '';
            activePageId = '';
            $('#tab-indexes').addClass('input-disable');

            // Show view batch
            $('#xr_xri').show();
            return;
        }

        $('#xr_xri').hide();
        ShowControlPanel();

        var liDocId;
        var liPageId;

        // Case click page
        if ($liParent.hasClass('li-page')) {

            var $liDoc = $liParent.parent().parent().parent();

            // Disable tab doc index if li doc is loose page, else enable
            if ($liDoc.hasClass('loose-item'))
                $('#tab-indexes').addClass('input-disable');
            else
                $('#tab-indexes').removeClass('input-disable');

            liDocId = $liParent.parent().parent().parent().attr('id');
            liPageId = $liParent.attr('id');
            activeType = 'page';

        } else if ($liParent.hasClass('li-doc')) { // Case click doc
            // Enable tab doc index
            $('#tab-indexes').removeClass('input-disable');

            liDocId = $liParent.attr('id');
            liPageId = $liParent.find('.tree-item.ul-page').children('.li-page').first().attr('id');
            activeType = 'doc';
        }

        // Case click page not in the same doc
        if (liDocId != activeDocId)
            $('#view-doc-' + activeDocId).hide();

        // Load image viewer if not
        var batchId = e.data.$liBatch.attr('id');
        var suffixUrl = '?batchId=' + batchId + '&docId=';
        var prefixLength = idViewerPagePrefix.length - 1;
        var $viewerDoc = $(idViewerDocPrefix + liDocId).show();;
        $viewerDoc.children('.wrapper-image.not-load').each(function () {
            var $image = $(this);
            var pageId = $image.attr('id').substr(prefixLength);
            LoadImagePage($image, suffixUrl + $('#' + pageId).data('old-doc-id') + '&pageId=' + pageId, true);
        });

        // Update active doc id and active page id
        activeDocId = liDocId;
        activePageId = liPageId;

        // Get top position of page
        var topPosition = $('#view-page-' + activePageId).position().top;

        // Scroll to target element
        var $viewer = $('#panel-viewer');
        $viewer.scrollTop($viewer.scrollTop() + topPosition);

        // Set status control panel
        SetStatusPanelControl(activeBatchId, activeDocId, activeType,
                              $this.children('.item-image').hasClass('native-image'));
    });
}
function AddHandlerForLiThumbBatchCache($liPages) {

    // Handler click on div item-content click
    $liPages.find('.item-content').click(function () {

        // Hide panel comment if it is open
        HideCommentPanel(idTabThumbnail);

        var $this = $(this);
        $('.item-content.item-select').removeClass('item-select');
        $(this).addClass('item-select');

        // Get parent li
        $liParent = $this.parent();

        $('#xr_xri').hide();
        ShowControlPanel();

        var liDocId;
        var liPageId;

        var $liDoc = $liParent.parent().parent().parent();

        // Disable tab doc index if li doc is loose page, else enable
        if ($liDoc.hasClass('loose-item'))
            $('#tab-indexes').addClass('input-disable');
        else
            $('#tab-indexes').removeClass('input-disable');

        liDocId = $liParent.parent().parent().parent().attr('id');
        liPageId = $liParent.attr('id');
        activeType = 'page';

        // Show doc viewer
        $('#view-doc-' + liDocId).show();

        // Update active doc id and active page id
        activeDocId = liDocId;
        activePageId = liPageId;

        // Get top position of page
        var topPosition = $('#view-page-' + activePageId).position().top;

        // Scroll to target element
        var $viewer = $('#panel-viewer');
        $viewer.scrollTop($viewer.scrollTop() + topPosition);

        // Set status control panel
        SetStatusPanelControl(activeBatchId, activeDocId, 'page',
                              $this.children('.item-image').hasClass('native-image'));
    });
}

function LoadThumbnailPage($liPage, batchId, isLoadRealImage) {

    var pageId = $liPage.attr('id');
    var $image = $liPage.find('.item-image');
    var subfixUrl = '?batchId=' + batchId + '&docId=' + $liPage.data('old-doc-id') + '&pageId=' + pageId;

    // Real image
    if ($image.hasClass('real-image')) {

        var scr = urlGetThumbnailRealImage + subfixUrl + '&t=' + (new Date()).getTime();
        var img = $("<img />").attr('src', scr).load(function () {

            $image.children('.image-main').attr('src', scr);

            // Get thumbnail info
            $.ajax({
                type: 'GET',
                dataType: "json",
                cache: false,
                url: urlGetThumbnailRealImageInfo + subfixUrl,
                success: function (data) {
                    if (data == null)
                        return;

                    $image.next().append('<div>' + data.dpi + ' dpi</div>');
                    if (isLoadRealImage === true) {

                        var $viewerPage = $('#view-page-' + $liPage.attr('id'));
                        $viewerPage.height(data.height).width(data.width);
                        $viewerPage.children('img').height(data.height - 1).width(data.width - 1);
                        LoadImagePage($viewerPage, subfixUrl, false);
                    }
                }
            });
        });
    }
        // Native image
    else {

        $.ajax({
            type: 'GET',
            dataType: "json",
            cache: false,
            url: urlGetNativeImage + subfixUrl,
            success: function (data) {

                if (data == null) {
                    data = { thumbnailPath: '' };
                }

                $image.children('.image-main').attr('src', data.thumbnailPath);

                // Change source of img in page viewer if exist
                var $viewPage = $('#view-page-' + pageId);
                if ($viewPage.length > 0) {
                    var $tempDiv = $viewPage.children('.wrapper-image-temp');
                    var $mainImg = $viewPage.children('img');

                    $tempDiv.remove();
                    $mainImg.attr('src', data.thumbnailPath).show();
                }
            }
        });
    }
}
function LoadImagePage($image, subfixUrl, isLoadAnno) {

    // Count plus by 1
    syncCountLoadingImage += 1;
    $('#panel-controls').ecm_disable();

    $image.removeClass('not-load');

    var $tempDiv = $image.children('.wrapper-image-temp');
    var $mainImg = $image.children('img');

    // Disable drag image
    $mainImg.mousedown(function (e) {
        e.preventDefault();
    });

    var scr = urlGetRealImage + subfixUrl + '&t=' + (new Date()).getTime();
    var img = $("<img />").attr('src', scr).load(function () {

        if (isLoadAnno === false) {
            $mainImg.attr('src', scr).show();
            $tempDiv.remove();
            // Count minus by 1
            syncCountLoadingImage -= 1;
            if (syncCountLoadingImage == 0) {
                $('#panel-controls').ecm_enable();
            }

            var selector = '#control-Highlight.active:not(.disable),' +
                           '#control-Redaction.active:not(.disable),' +
                           '#control-Text.active:not(.disable)';
            if ($(selector).size() > 0)
                $image.draw();

            return;
        }

        // Get thumbnail additional information
        $.ajax({
            type: 'GET',
            dataType: 'html',
            cache: false,
            url: urlGetAnnotations + subfixUrl,
            success: function (data) {
                if (data != null) {

                    $mainImg.attr('src', scr).show();
                    $tempDiv.remove();
                    $image.append(data);

                    var $annoes = $image.find('.anno');
                    $annoes.mousedown(function (e) {
                        e.preventDefault();
                    });

                    var flgHideAnno = $('#' + $image.attr('id').substr(idViewerPagePrefix.length - 1)).parent().parent().parent().data('hide-anno');
                    if (flgHideAnno == 'True')
                        $annoes.hide();
                }

                // Count minus by 1
                syncCountLoadingImage -= 1;
                if (syncCountLoadingImage == 0) {
                    $('#panel-controls').ecm_enable();
                }
            },
            error: function () {
                syncCountLoadingImage -= 1;
                if (syncCountLoadingImage == 0) {
                    $('#panel-controls').ecm_enable();
                }
            }
        });

    }).error(function () {
        syncCountLoadingImage -= 1;
        if (syncCountLoadingImage == 0) {
            $('#panel-controls').ecm_enable();
        }
    });
}

// Open select batch
function OpenBatch(batchId) {

    // Batch has been already opened
    if (activeBatchId == batchId) {
        return;
    }
    $('body').ecm_loading_show();

    // Reset layout
    ResetLayout();

    // Load data from server
    $.ajax({
        url: urlOpenBatch + '?batchId=' + batchId,
        type: 'GET',
        cache: false,
        success: function (data, textStatus, jqXHR) {

            var $data = $('<div>').append(data);

            // Set active status
            activeBatchId = batchId;
            activeDocId = $data.children('#active-doc-id').html();
            activePageId = $data.children('#active-page-id').html();
            activeType = 'page';

            // Set data for thumbnail
            $('.tree-item.ul-batch').append($data.children('ul').children());

            // Set data for viewer
            $('#panel-viewer').append($data.children('#view-batch-' + batchId));

            var $liActiveBatch = $('#' + activeBatchId);
            // Load all thumbnail of page in batch
            AddHandlerForLiThumbBatch($liActiveBatch);
            $liActiveBatch.find('.li-page').each(function () {
                LoadThumbnailPage($(this), activeBatchId, false);
            });

            // Add batch index
            var $batchIndex = $data.children('#batch-index-' + batchId);
            // Add validate function
            $batchIndex.find('input.integer').each(function () {
                AddHandlerCheckInputInteger($(this));
            });
            $batchIndex.find('input.decimal').each(function () {
                AddHandlerCheckInputDecimal($(this));
            });
            $batchIndex.find('input.date').each(function () {
                AddHandlerCheckInputDate($(this));
            });

            // Handler clear input
            $batchIndex.find('button.close').each(function () {
                AddHandlerButtonClear($(this));
            })
            $('#panel-thumbnails-inner').append($batchIndex);

            // Add doc index
            $data.children('#doc-index').children().each(function () {

                var $docIndex = $(this);

                // Add validate function
                $docIndex.find('input.integer').each(function () {
                    AddHandlerCheckInputInteger($(this));
                });
                $docIndex.find('input.decimal').each(function () {
                    AddHandlerCheckInputDecimal($(this));
                });
                $docIndex.find('input.date').each(function () {
                    AddHandlerCheckInputDate($(this));
                });

                // Handler clear input
                $docIndex.find('button.close').each(function () {
                    AddHandlerButtonClear($(this));
                });

                // Handler add required border when not input value
                $docIndex.find('input[type="text"].required').on('input', null, null, function () {
                    AddHandlerRequired($(this));
                });
                $docIndex.find('select.required').change(function () {
                    AddHandlerRequired($(this));
                });

                var newIds = [];
                // Add new row value
                $docIndex.find('.click-new-row').click(function () {
                    var $this = $(this);

                    var $table = $this.prev();
                    var $trTemplateRow = $table.find('.template-row').clone(true);

                    $trTemplateRow.removeClass('template-row').addClass('real-row');
                    $table.find('tbody').append($trTemplateRow);
                    $trTemplateRow.find('input[type="text"]').removeClass('empty-template').addClass('empty').first().focus();

                    // Add new key for column value
                    $trTemplateRow.children('.td-data').each(function () {
                        var newId;
                        var existed;
                        do {
                            existed = false;
                            // Get new id
                            newId = 'new-id-' + (new Date()).getTime();

                            // Check this id is existed before
                            if ($('#' + newId).length > 0) {
                                existed = true;
                            }

                            // Check this id is in new list id
                            if (newId in newIds) {
                                existed = true;
                            }

                        } while (existed);

                        var $input = $(this).children().children().first();
                        $input.attr('id', newId);
                        $input.addClass('is-new');

                        // Add handler check input date
                        if ($input.hasClass('date-template')) {
                            $input.removeClass('date-template').addClass('date');
                            AddHandlerCheckInputDate($input);
                        } else if ($input.hasClass('integer'))
                            AddHandlerCheckInputInteger($input);
                        else if ($input.hasClass('decimal'))
                            AddHandlerCheckInputDecimal($input);
                    });

                    // Disable button submit
                });

                // Remove row value
                $docIndex.find('.del-col').click(function () {
                    $(this).parent().parent().remove();
                });

                // Handler for table value
                var $tables = $docIndex.find('.detail-table');
                $tables.dialog({
                    autoOpen: false,
                    modal: true,
                    width: 600,
                    height: 400,
                    resizable: false,
                    buttons: [{ text: "Close", click: function () { $(this).dialog("close"); } }],
                    open: function (event, ui) {
                        $(this).layout({
                            applyDefaultStyles: true,
                            south: {
                                closable: false,
                                resizable: false,
                                slidable: false,
                                spacing_open: 1,
                            }
                        });
                    },
                    close: function (event, ui) {

                        var $this = $(this);
                        var $templateRow = $this.find('.template-row');

                        // No template row => in mode can not modify
                        if ($templateRow.length == 0) {
                            return;
                        }

                        // In mode can modify index
                        // In case field is not required
                        if (!$templateRow.hasClass('required')) {
                            return;
                        }

                        var $clickDetail = $('#' + $this.attr('id').substr('tbl-'.length));

                        // In case field is required => Check for at least on row valid
                        var $rows = $this.find('.real-row');

                        // Check have no row => not valid
                        if ($rows.size() == 0) {
                            $clickDetail.addClass('empty');
                            // Set status panel submit
                            SetStatusPanelSubmit();
                            return;
                        }

                        // Check have at least one row input all value
                        for (var i = 0; i < $rows.size() ; i++) {
                            if ($($rows[i]).find('.required.empty').size() == 0) {
                                $clickDetail.removeClass('empty');
                                // Set status panel submit
                                SetStatusPanelSubmit();
                                return;
                            }
                        }

                        $clickDetail.addClass('empty');
                        // Set status panel submit
                        SetStatusPanelSubmit();
                    }
                });

                // Open table
                $docIndex.find('.click-detail-table').click(function () {
                    var $this = $(this);

                    var $table = $('#tbl-' + $this.attr('id'));

                    $table.dialog('option', 'title', $this.prev().children().first().html());
                    $table.dialog('option', 'position', { my: "center", at: "center", of: window });
                    $table.dialog("open");
                });

                $('#panel-thumbnails-inner').append($docIndex);
            });

            // Trigger click on 
            $('#' + activePageId).children('.item-content').trigger('click');

            SetStatusPanelSubmit();

            $('body').ecm_loading_hide();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            // Do some stuff here
            console.log('error open batch');
            $('body').ecm_loading_hide();
        }
    });
}

function DeleteDoc(ui) {

    var $clickedThumbnail;
    var $removeThumbnail;
    var $removeViewer;

    var $nextLiDoc = ui.item.next();
    var $prevLiDoc = ui.item.prev();

    // Case batch have more than one doc
    if ($nextLiDoc.length != 0 || $prevLiDoc.length != 0) {

        // Get clicked doc
        var newStartIndex = -1;
        if ($nextLiDoc.length != 0) {
            $clickedThumbnail = $nextLiDoc;
            newStartIndex = Number(ui.item.find('.item-doc-index').text());
        }
            // Case prev doc is loose doc
        else if ($prevLiDoc.hasClass('loose-item')) {

            var $firstPage = $prevLiDoc.find('.li-page').first();
            if ($firstPage.length == 0)
                $clickedThumbnail = $prevLiDoc.parent().parent().parent();
            else
                $clickedThumbnail = $firstPage;

        } else
            $clickedThumbnail = $prevLiDoc;

        // Update label index of all doc after remove parent doc
        if (newStartIndex > 0)
            ui.item.nextAll().each(function () {
                $(this).find('.item-doc-index').html(newStartIndex++);
            })
    }
        // Case doc have only one doc (this is loose doc)
    else
        $clickedThumbnail = ui.item.parent().parent().parent();

    var $liBatch = ui.item.parent().parent().parent();

    ui.item.remove();
    $(idViewerDocPrefix + ui.item.attr('id')).remove();
    $clickedThumbnail.children('.item-content').trigger('click');

    // Update reject status
    if ($liBatch.find('.li-doc.reject').size() > 0)
        $liBatch.removeClass('accept').addClass('reject');
    else if ($liBatch.find('.li-doc.accept').size() > 0)
        $liBatch.removeClass('reject').addClass('accept');
    else
        $liBatch.removeClass('accept').removeClass('reject');

    SetStatusPanelControl();

    $body.ecm_loading_hide();
}
function DeletePage(ui) {
    var $clickedThumbnail;
    var $removeThumbnail;
    var $removeViewer;

    var $nextLiPage = ui.item.next();
    var $prevLiPage = ui.item.prev();

    // Case doc have more than one page
    if ($nextLiPage.length != 0 || $prevLiPage.length != 0) {

        // Get clicked page
        var newStartIndex = -1;
        if ($nextLiPage.length != 0) {
            $clickedThumbnail = $nextLiPage;
            newStartIndex = Number(ui.item.find('.item-page-index').text());
        }
        else
            $clickedThumbnail = $prevLiPage;

        // Update label index of all page after remove page
        if (newStartIndex > 0)
            ui.item.nextAll().each(function () {
                $(this).find('.item-page-index').html(newStartIndex++);
            })

        // Update total pages of doc
        var $totalPages = ui.item.parent().parent().prev().find('.item-doc-count');
        $totalPages.html(Number($totalPages.text()) - 1);

        // Set remove page thumbnail and viewer
        $removeThumbnail = ui.item;
        $removeViewer = $(idViewerPagePrefix + ui.item.attr('id'));
    }
        // Case doc have only one page
    else {

        var $parentDoc = ui.item.parent().parent().parent();
        var $nextLiDoc = $parentDoc.next();
        var $prevLiDoc = $parentDoc.prev();

        // Case batch have more than one doc
        if ($nextLiDoc.length != 0 || $prevLiDoc.length != 0) {

            // Get clicked doc
            var newStartIndex = -1;
            if ($nextLiDoc.length != 0) {
                $clickedThumbnail = $nextLiDoc;
                newStartIndex = Number($parentDoc.find('.item-doc-index').text());
            }
                // Case prev doc is loose doc
            else if ($prevLiDoc.hasClass('loose-item')) {

                var $firstPage = $prevLiDoc.find('.li-page').first();
                if ($firstPage.length == 0)
                    $clickedThumbnail = $prevLiDoc.parent().parent().parent();
                else
                    $clickedThumbnail = $firstPage;

            } else
                $clickedThumbnail = $prevLiDoc;

            // Update label index of all doc after remove parent doc
            if (newStartIndex > 0)
                $parentDoc.nextAll().each(function () {
                    $(this).find('.item-doc-index').html(newStartIndex++);
                })

            // Set remove page thumbnail and viewer in case parent doc is loose doc
            if ($parentDoc.hasClass('loose-item')) {
                $removeThumbnail = ui.item;
                $removeViewer = $(idViewerPagePrefix + ui.item.attr('id'));
            }
                // Set remove doc thumbnail and viewer in case parent doc is normal doc
            else {
                $removeThumbnail = $parentDoc;
                $removeViewer = $(idViewerDocPrefix + $parentDoc.attr('id'));
            }
        }
            // Case doc have only one doc (this is loose doc)
        else {

            $clickedThumbnail = $parentDoc.parent().parent().parent();

            // Set remove page thumbnail and viewer in case parent doc is loose doc
            $removeThumbnail = ui.item;
            $removeViewer = $(idViewerPagePrefix + ui.item.attr('id'));
        }
    }

    var $liDoc = $removeThumbnail.parent().parent().parent();

    $removeThumbnail.remove();
    $removeViewer.remove();
    $clickedThumbnail.children('.item-content').trigger('click');

    // Update status reject
    if ($liDoc.find('.li-page.reject').size() > 0)
        $liDoc.removeClass('accept').addClass('reject');
    if ($liDoc.find('.li-page.accept').size() > 0)
        $liDoc.removeClass('reject').addClass('accept');
    else
        $liDoc.removeClass('accept').removeClass('reject');

    var $liBatch = $('#' + activeBatchId);
    if ($liBatch.find('.li-doc.reject').size() > 0)
        $liBatch.removeClass('accept').addClass('reject');
    else if ($liBatch.find('.li-doc.accept').size() > 0)
        $liBatch.removeClass('reject').addClass('accept');
    else
        $liBatch.removeClass('accept').removeClass('reject');

    SetStatusPanelSubmit();

    $body.ecm_loading_hide();
}