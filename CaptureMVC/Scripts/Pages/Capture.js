var _$dlgCreateBatch,   // jQuery object of create batch dialog
    _$txtBatchName,     // jQuery object of text box batch name
    _$cboBatchType,     // jQuery object of drop down list batch type
    _$btnCreateBatchDialogOk,
    _$panelBatchName,
    _$panelBatchType,
    _$panelOption,
    _$templateThumbBatch,   // jQuery object of li batch thumbnail template
    _$templateThumbDoc,     // jQuery object of li doc thumbnail template
    _$templateThumbPage,    // jQuery object of li page thumbnail template
    _$templateViewPage,    // jQuery object of div view page template
    _$templateBatchIndex,   // jQuery object of batch index template
    _$templateDocIndex,   // jQuery object of doc index template
    _$templateFieldBool,    // jQuery object of field boolean template
    _$templateFieldPicklist,    // jQuery object of field pick list template
    _$templateFieldTable,    // jQuery object of field table template
    _$templateFieldOther;   // jQuery object of field other type template

var _prevCreateBatchTypeId;
var _batchTypes = {};
var _syncCountLoadingImage = 0;


var _menuBatch = {
    BatchScan: {
        name: _menuThumbScan,
        items: {
            ScanClassifyLater: { name: _menuThumbClassify },
            ScanNewDocument: { name: _menuThumbNewDocument }
        }
    },
    BatchImport: {
        name: _menuThumbImport,
        items: {
            ImportClassifyLater: { name: _menuThumbClassify, labelFor: _idBtnUploadBrowser },
            ImportNewDocument: { name: _menuThumbNewDocument }
        }
    },
    BatchCamera: {
        name: _menuThumbCamera,
        items: {
            CameraClassifyLater: { name: _menuThumbClassify },
            CameraNewDocument: { name: _menuThumbNewDocument }
        }
    },
    Separate1: "---------",
    ChangeBatchType: { name: _menuChangeBatchType, disabled: true },
    RenameBatch: { name: _menuRenameBatch },
    Separate2: "---------",
    BatchDelete: { name: _menuThumbDelete },
    BatchSubmit: { name: _menuThumbSubmit }
};

function InitCaptureVariables() {
    /// <signature>
    /// <summary>Initialize global variable use in "Capture.js".</summary>
    /// </signature>

    _$templateThumbBatch = $('#template-thumb-batch');
    _$templateThumbDoc = $('#template-thumb-doc');
    _$templateThumbPage = $('#template-thumb-page');
    _$templateViewPage = $('#template-view-page');
    _$txtBatchName = $('#txt-batch-name-new');
    _$cboBatchType = $('#cbo-batch-type-id');
    _$panelBatchName = $('#panel-create-batch-name');
    _$panelBatchType = $('#panel-create-batch-type');
    _$panelOption = $('#panel-create-batch-option');
    _$templateBatchIndex = $('#template-batch-index');
    _$templateDocIndex = $('#template-doc-index');
    _$templateFieldBool = $('#template-field-bool');
    _$templateFieldPicklist = $('#template-field-picklist');
    _$templateFieldTable = $('#template-field-table');
    _$templateFieldOther = $('#template-field-other');

}
function InitCreateBatchDialog() {
    /// <signature>
    /// <summary>Initialize create batch dialog.</summary>
    /// </signature>

    _$dlgCreateBatch = $('#create-batch-dialog');
    _$dlgCreateBatch.dialog({
        title: _$dlgCreateBatch.data('title'),
        autoOpen: false,
        modal: true,
        width: 560,
        height: 'auto',
        resizable: false,
        buttons: [
            {
                text: 'Ok',
                id: 'create-batch-dialog-btn-ok',
                click: function (e) {
                    // Do no thing when button is disable
                    if ($(e.currentTarget).hasClass(_clsUIDisable)) {
                        return;
                    }

                    _$dlgCreateBatch.dialog("close");
                    var action = _$dlgCreateBatch.data('action');
                    switch (action) {
                        case 'RenameBatch':
                            var $activeLiBatch = $('#' + _activeBatchId);
                            var batchName = _$txtBatchName.val().trim()
                            var $name = $activeLiBatch.children('.item-content').find('.item-title').text(batchName);

                            // Update batch name in batch index
                            var $batchIndex = $('#' + _prfxBatchIndex + _activeBatchId);
                            if (batchName == '')
                                $batchIndex.find('.name').text(_batchTypes[$activeLiBatch.data('type-id')].Name);
                            else
                                $batchIndex.find('.name').text(batchName);
                            break;

                        case 'ChangeBatchType':

                            var batchTypeId = _$cboBatchType.val();
                            // Load detail information of selected batch type if not loaded
                            if (_batchTypes[batchTypeId] == undefined) {
                                _$body.ecm_loading_show();
                                $.ajax({
                                    url: _urlGetCapturedBatchTypeInfo,
                                    type: 'POST',
                                    data: JSON.stringify({ id: batchTypeId }),
                                    contentType: 'application/json',
                                    dataType: 'json',
                                    success: function (data, textStatus, jqXHR) {
                                        if (data == undefined && data.length > 0) {
                                            ProcessError();
                                            return;
                                        }

                                        _batchTypes[batchTypeId] = ProcessBatchTypeInfoResult(data);
                                        _$body.ecm_loading_hide();

                                        // Remove old batch index
                                        $('#' + _prfxBatchIndex + _activeBatchId).remove();

                                        // Create new batch index
                                        var $liBatch = $('#' + _activeBatchId);
                                        $liBatch.data('type-id', batchTypeId);
                                        var $itemContent = $liBatch.children('.item-content');
                                        $itemContent.find('.type-name').text(_batchTypes[batchTypeId].Name);
                                        var batchName = $itemContent.find('.item-title').text();
                                        CreateBatchIndex(_activeBatchId, batchName, batchTypeId);
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        ProcessError();
                                    }
                                });
                            } else {
                                // Remove old batch index
                                $('#' + _prfxBatchIndex + _activeBatchId).remove();

                                // Create new batch index
                                var $liBatch = $('#' + _activeBatchId);
                                $liBatch.data('type-id', batchTypeId);
                                var $itemContent = $liBatch.children('.item-content');
                                $itemContent.find('.type-name').text(_batchTypes[batchTypeId].Name);
                                var batchName = $itemContent.find('.item-title').text();
                                CreateBatchIndex(_activeBatchId, batchName, batchTypeId);
                            }
                            break;

                        default:
                            _prevCreateBatchTypeId = _$cboBatchType.val();
                            // Load detail information of selected batch type if not loaded
                            if (_batchTypes[_prevCreateBatchTypeId] == undefined) {
                                _$body.ecm_loading_show();
                                $.ajax({
                                    url: _urlGetCapturedBatchTypeInfo,
                                    type: 'POST',
                                    data: JSON.stringify({ id: _prevCreateBatchTypeId }),
                                    contentType: 'application/json',
                                    dataType: 'json',
                                    success: function (data, textStatus, jqXHR) {
                                        if (data == undefined && data.length > 0) {
                                            ProcessError();
                                            return;
                                        }

                                        _batchTypes[_prevCreateBatchTypeId] = ProcessBatchTypeInfoResult(data);

                                        _$body.ecm_loading_hide();
                                        CreateBatch(_prevCreateBatchTypeId);
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        ProcessError();
                                    }
                                });
                            } else {
                                CreateBatch(_prevCreateBatchTypeId);
                            }
                            break;
                    }
                }
            },
            {
                text: 'Cancel',
                id: 'create-batch-dialog-btn-cancel',
                click: function () { _$dlgCreateBatch.dialog("close"); }
            }
        ],
        open: function (event, ui) {
            var action = _$dlgCreateBatch.data('action');

            switch (action) {
                case 'RenameBatch':
                    var $activeLiBatch = $('#' + _activeBatchId);
                    var $name = $activeLiBatch.children('.item-content').find('.item-title');
                    _$txtBatchName.focus().val($name.text()).select();
                    break;

                case 'ChangeBatchType':
                    var $activeLiBatch = $('#' + _activeBatchId);
                    var typeId = $activeLiBatch.data('type-id');
                    _$cboBatchType.children('option.hide').removeClass('hide');
                    _$cboBatchType.children('option[value="' + typeId + '"]').addClass('hide');
                    break;

                default:
                    _$cboBatchType.children('option.hide').removeClass('hide');
                    _$txtBatchName.focus().val('');
                    break;
            }

            // Disable button Ok when have no batch type selected
            if (_$cboBatchType.children('option:not(.hide)').length == 0) {
                _$btnCreateBatchDialogOk.button("disable");
            } else {
                _$btnCreateBatchDialogOk.button("enable");
                var hideOptionTypeId = _$cboBatchType.children('option.hide').attr('value');
                var seletedTypeId = _$cboBatchType.val();

                if (hideOptionTypeId == seletedTypeId) {
                    _$cboBatchType.val(_$cboBatchType.children('option:not(.hide):first').attr('value'));
                }
            }
        }
    });

    // Make all buttons are the same width
    // This function is in file "/Scripts/Pages/CaptureViewer.js"
    _$btnCreateBatchDialogOk = $('#create-batch-dialog-btn-ok');
    SetTheSameWidthButtons([_$btnCreateBatchDialogOk, $('#create-batch-dialog-btn-cancel')], 80);

    // Trigger button OK when press Enter key on dialog
    _$dlgCreateBatch.keyup({ $buttonOk: _$btnCreateBatchDialogOk }, function (e) {
        if (e.keyCode == 13) {
            e.data.$buttonOk.trigger('click');
        }
    });
    // Trigger button OK when double click on batch type
    _$cboBatchType.dblclick({ $buttonOk: _$btnCreateBatchDialogOk }, function (e) {
        e.data.$buttonOk.trigger('click');
    });

    // Add click handler to show dialog
    _$btnCreate.click(function () {
        _$dlgCreateBatch.data('action', 'CreateBatch');
        _$panelBatchName.show();
        _$panelBatchType.show();
        _$panelOption.show();
        _$dlgCreateBatch.dialog('option', 'position', { my: "center", at: "center", of: window });
        _$dlgCreateBatch.dialog("open");
    });
};


function CreateBatch(batchTypeId) {
    /// <signature>
    /// <summary>Create thumbnail batch.</summary>
    /// </signature>

    // Get new guid from server
    $.ajax({
        url: _urlCreateBatch,
        type: 'POST',
        data: {
            batchName: _$txtBatchName.val(),
            batchTypeId: batchTypeId
        },
        success: function (data, textStatus, jqXHR) {

            if (data == undefined && data.length > 0) {
                ProcessError();
                return;
            }

            var $data = $(data);
            var batchId = $data.attr('id');

            // Add to main ul list batch
            _$ulThumb.append($data);

            // Add view batch panel
            var $viewBatch = $('<div>');
            $viewBatch.attr('id', _prfxViewBatch + batchId);
            $viewBatch.addClass(_clsViewBatch);
            _$viewPanel.append($viewBatch);

            // Add loose view doc
            var looseDocId = $data.find('li-doc loose-item').attr('id');
            var $viewLooseDoc = $('<div id="' + _prfxViewDoc + looseDocId + '" class="loose-item">');
            $viewLooseDoc.addClass(_clsViewDoc);
            $viewBatch.append($viewLooseDoc);

            // Add event click handler for li batch
            BatchThumb_Click(batchId);

            //// Create batch type id
            //CreateBatchIndex(data.BatchId, data.BatchName == '' ? data.BatchTypeName : data.BatchName, batchTypeId);

            //// Switch to panel thumbnail
            //_$tabThumbnail.trigger('click');

            //// Scroll to this batch
            //// Scroll to first page
            //var below = $itemContent.belowTheViewPort({ container: _$thumbnailPanel, threshold: -$itemContent.outerHeight() });
            //if (below > 0)
            //    _$thumbnailPanel.scrollTop(_$thumbnailPanel.scrollTop() + below);


            //// Trigger context menu
            //var $typeName = $itemContent.children('.item-text').children('.type-name');
            //var offset = $typeName.offset();
            //var height = $typeName.outerHeight();
            //$itemContent.contextMenu({ x: offset.left, y: offset.top + height });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            ProcessError();
        }
    });
}
function CreateDoc(batchId, datas) {
    /// <signature>
    /// <summary>Create thumbnail doc.</summary>
    /// </signature>

    // Get list page ids
    var pageDatas = datas.Pages,
        countPages = pageDatas.length,
        pageIds = [],
        existedIds = [];

    for (var i = 0; i < countPages; i++) {
        pageIds.push(pageDatas[i].Id);
        // Check this page id is already existed
        if (CheckExistedId(pageDatas[i].Id)) {
            existedIds.push(pageDatas[i].Id);
        }
    }

    // Change item with existed id to new id
    var countExistedIds = existedIds.length;
    for (var i = 0; i < countExistedIds; i++) {
        var newId = NewGuid(pageIds);
        $('#' + countExistedIds[i]).attr('id', newId);
    }

    // Create new thumbnail doc
    var $liBatch = $('#' + batchId),
        $ulDocs = $liBatch.children('.item-children').children(),
        $liDoc = _$templateThumbDoc.children().clone(),
        liId = NewGuid(pageIds);

    $liDoc.attr('id', liId);
    $liDoc.data('type-id', _batchTypes[_batchTypeId].DocTypes[_docTypeId].Id);

    // Set information for doc
    var $itemContent = $liDoc.children('.item-content');
    var $itemText = $itemContent.children('.item-text');

    $itemText.children('.item-title').hide();
    $itemText.children('.item-type-name')
             .children('.type-name').text(_batchTypes[_batchTypeId].DocTypes[_docTypeId].Name);
    $itemText.find('.item-doc-count').text(countPages);

    var countLiDoc = $ulDocs.children('.li-doc:not(.loose-item)').length;
    $itemText.find('.item-doc-index').each(function () {
        $(this).text(countLiDoc + 1);
    })

    // Insert page
    var $ulPages = $liDoc.children('.item-children').children(),
        $workPage,
        $viewDoc = $('<div>');

    // Set attribute for view doc
    $viewDoc.attr('id', _prfxViewDoc + liId);
    $viewDoc.addClass(_clsViewDoc);

    // Create page
    for (var i = 0; i < countPages; i++)
        CreatePage($ulPages, $viewDoc, pageDatas[i], i + 1);

    // Show icon expand/collapse
    $liBatch.removeClass(_clsEmpty);
    $liDoc.removeClass(_clsEmpty);

    // Add to main ul list doc of batch
    $ulDocs.append($liDoc);

    // Add view batch panel
    var $viewBatch = $('#' + _prfxViewBatch + batchId);
    $viewBatch.append($viewDoc);

    // Add event click handler for li doc
    DocThumb_Click(liId);
    for (var i = 0; i < countPages; i++)
        PageThumb_Click(pageDatas[i].Id);

    // Create doc index
    CreateDocIndex(liId, _batchTypes[_batchTypeId].DocTypes[_docTypeId].Name, _batchTypeId, _docTypeId,
                   datas.OcrData, datas.OcrImageIds);

    CheckCanSubmit();

    // Scroll to first page
    var $itemContentFirstPage = $('#' + pageDatas[0].Id).children('.item-content');
    var below = $itemContentFirstPage.belowTheViewPort({
        container: _$thumbnailPanel,
        threshold: -$itemContentFirstPage.outerHeight()
    });
    if (below > 0) {
        _$thumbnailPanel.scrollTop(_$thumbnailPanel.scrollTop() + below);

    }
    // Trigger first page click
    $itemContentFirstPage.trigger('click');

    _$body.ecm_loading_hide();
}
function CreatePage($liDoc, $viewDoc, data, index) {
    var $liPage = _$templateThumbPage.children().clone(),
        $itemContent = $liPage.children('.item-content');

    $liPage.attr('id', data.Id);
    $itemContent.children('.item-image').addClass(data.Class);
    $itemContent.children('.item-text').children('.item-page-index').text(index);

    $liDoc.append($liPage);

    var $viewPage = _$templateViewPage.clone();
    $viewPage.attr('id', _prfxViewPage + data.Id);
    if (data.Class == _clsNativeImage)
        $viewPage.addClass(_clsNativeImage);

    // Disable drag image
    $viewPage.children('.main-image').mousedown(function (e) {
        e.preventDefault();
    });

    $viewDoc.append($viewPage);

    LoadThumbnailPage($liPage, $viewPage);
}

function InsertPageLater(batchId, pageDatas) {
    /// <signature>
    /// <summary>Create thumbnail doc.</summary>
    /// </signature>

    // Get list page ids
    var countPages = pageDatas.length,
        pageIds = [],
        existedIds = [];

    for (var i = 0; i < countPages; i++) {
        pageIds.push(pageDatas[i].Id);
        // Check this page id is already existed
        if (CheckExistedId(pageDatas[i].Id)) {
            existedIds.push(pageDatas[i].Id);
        }
    }

    // Change item with existed id to new id
    var countExistedIds = existedIds.length;
    for (var i = 0; i < countExistedIds; i++) {
        var newId = NewGuid(pageIds);
        $('#' + countExistedIds[i]).attr('id', newId);
    }

    // Create new thumbnail doc
    var $liBatch = $('#' + batchId),
        $liDoc = $liBatch.find('.li-doc.loose-item'),
        $ulPages = $liDoc.children('.item-children').children(),
        liDocId = $liDoc.attr('id'),
        $viewDoc = $('#' + _prfxViewDoc + liDocId),
        continuesIndex = $ulPages.children().length;
    console.log($viewDoc);
    console.log(liDocId);
    // Create page
    for (var i = 0; i < countPages; i++)
        CreatePage($ulPages, $viewDoc, pageDatas[i], continuesIndex + i + 1);

    // Show icon expand/collapse
    $liBatch.removeClass(_clsEmpty);

    for (var i = 0; i < countPages; i++)
        PageThumb_Click(pageDatas[i].Id);

    CheckCanSubmit();

    // Scroll to first page
    var $itemContentFirstPage = $('#' + pageDatas[0].Id).children('.item-content');
    var below = $itemContentFirstPage.belowTheViewPort({
        container: _$thumbnailPanel,
        threshold: -$itemContentFirstPage.outerHeight()
    });
    if (below > 0) {
        _$thumbnailPanel.scrollTop(_$thumbnailPanel.scrollTop() + below);

    }
    // Trigger first page click
    $itemContentFirstPage.trigger('click');

    _$body.ecm_loading_hide();
}

function LoadThumbnailPage($liPage, $viewPage) {

    var pageId = $liPage.attr('id');
    var $image = $liPage.find('.item-image');

    // Real image
    if ($image.hasClass(_clsRealImage)) {

        var src = _urlGetThumbnailRealImage + '?id=' + pageId + '&t=' + (new Date()).getTime();
        var img = $("<img />").attr('src', src).load({
            pageId: pageId, src: src, $image: $image, $viewPage: $viewPage
        },
            function (event) {
                event.data.$image.children('.image-main').attr('src', event.data.src);

                // Get thumbnail info
                $.ajax({
                    url: _urlGetThumbnailRealImageInfo,
                    type: 'POST',
                    data: JSON.stringify({ id: event.data.pageId }),
                    contentType: 'application/json',
                    dataType: 'json',
                    success: function (data) {
                        if (data == null) {
                            ProcessError();
                            return;
                        }

                        event.data.$image.next().append('<div>' + data.dpi + ' dpi</div>');
                        event.data.$viewPage.data('width', data.width);
                        event.data.$viewPage.data('height', data.height);

                        LoadImagePage(event.data.$viewPage[0]);
                    },
                    error: function () {
                        ProcessError();
                    }
                });
            }).error(function () {
                ProcessError();
            });

        // Native image
    } else if ($image.hasClass(_clsNativeImage)) {
        // Get thumbnail info
        $.ajax({
            url: _urlGetNativeImage,
            type: 'POST',
            data: JSON.stringify({ id: pageId }),
            contentType: 'application/json',
            dataType: 'json',
            success: function (data) {
                if (data == null) {
                    ProcessError();
                    return;
                }

                $image.children('.image-main').attr('src', data.thumbnailPath);
                $viewPage.children('.wrapper-image-temp').remove();

                var $mainImage = $viewPage.children('.main-image');
                $mainImage.css({ width: '128px', height: '128px' });
                $mainImage.attr('src', data.thumbnailPath);
                $mainImage.show();
            },
            error: function () {
                ProcessError();
            }
        });

    }
}
function LoadImagePage(wrapperImage) {

    // Count plus by 1
    _syncCountLoadingImage += 1;

    var $image = $(wrapperImage),
        src = _urlGetRealImage + '?id=' + $image.attr('id').substr(_prfxViewPage.length)
                               + '&t=' + (new Date()).getTime();

    var img = $("<img />").attr('src', src).load({ $image: $image, src: src }, function (event) {
        var $image = event.data.$image,
            $tempDiv = $image.children('.wrapper-image-temp'),
            $mainImg = $image.children('.main-image');

        $tempDiv.remove();
        $mainImg.attr('src', event.data.src).show();
        $image.css({
            width: $image.data('width') + 'px',
            height: $image.data('height') + 'px',
        });
        _syncCountLoadingImage -= 1; // Count minus by 1

        //var selector = '#control-Highlight.active:not(.disable),' +
        //               '#control-Redaction.active:not(.disable),' +
        //               '#control-Text.active:not(.disable)';
        //if ($(selector).size() > 0)
        //    $image.draw();

    }).error({ $image: $image }, function (event) {
        _syncCountLoadingImage -= 1;
        event.data.$image.addClass(_clsNotLoad);
        ProcessError();
    });
}

function CreateBatchIndex(batchId, batchName, batchTypeId) {
    /// <signature>
    /// <summary>Create batch index.</summary>
    /// <param name="batchId" type="String">Id of batch.</param>
    /// <param name="batchName" type="String">Name of batch.</param>
    /// <param name="batchTypeId" type="String">Id batch type of batch.</param>
    /// </signature>

    // Add batch index
    var $batchIndex = _$templateBatchIndex.clone();
    $batchIndex.attr('id', _prfxBatchIndex + batchId);
    $batchIndex.find('.name').text(batchName);

    var $listField = $batchIndex.children('.current_content_fields'),
        countBatchField = _batchTypes[batchTypeId].Fields.length,
        currentDate = new Date(),
        newIds = [];

    for (var i = 0; i < countBatchField; i++) {

        var field = _batchTypes[batchTypeId].Fields[i],
            fieldId = NewGuid(newIds),
            $field,
            $input;

        newIds.push(fieldId);

        if (field.DataType == 'Boolean') {
            // Boolean field
            $field = _$templateFieldBool.clone();

            $input = $field.children('.input-control').children('input');
            $input.attr('id', fieldId);
            $input.removeAttr('maxlenght');
            if (field.DefaultValue == 'True') {
                $input.attr('checked', 'checked');
            }
        } else {
            // String, Integer, Decimal, Date field
            var validateOptions = {
                patternValidations: [],
                validCallback: CheckCanSubmit,
                invalidCallback: CanNotSubmit
            },
                invalidFieldType = false,
                checkHandler = Input_Validate;

            switch (field.DataType) {
                case 'String':
                    checkHandler = undefined;
                    break;

                case 'Integer':
                    field.MaxLength = 10;

                    validateOptions.patternValidations.push({
                        pattern: _inputIntegerPattern,
                        isRestoreOldValue: true
                    });
                    validateOptions.patternValidations.push({
                        pattern: _validIntegerPattern,
                        invalidMessage: _messageInvalidInteger,
                    });
                    break;

                case 'Decimal':
                    field.MaxLength = 22;
                    field.DefaultValue = field.DefaultValue.replace('.', _decimalSeparator);

                    validateOptions.patternValidations.push({
                        pattern: _inputDecimalPattern,
                        isRestoreOldValue: true
                    });
                    validateOptions.patternValidations.push({
                        pattern: _validDecimalPattern,
                        invalidMessage: _messageInvalidDecimal,
                    });
                    break;

                case 'Date':
                    field.MaxLength = 10;
                    field.DefaultValue = currentDate.toString(_dateFormat);

                    validateOptions.type = 'Date';
                    validateOptions.dateFormatValidation = {
                        dateFormat: _dateFormat,
                        invalidMessage: _messageInvalidDate
                    };
                    break;

                default:
                    invalidFieldType = true;
                    break;
            }

            if (invalidFieldType)
                continue;

            $field = _$templateFieldOther.clone();
            $input = $field.children('.content_fields_input').children('input');
            $input.attr('id', fieldId);
            $input.attr('maxlength', field.MaxLength);

            // Add check handler
            if (checkHandler != undefined) {
                checkHandler($input, validateOptions);
            }

            $input.val(field.DefaultValue);

            $input.trigger('input');

            var $buttonClear = $input.nextAll('.close');
            ButtonClear_Click($buttonClear);
        }

        $input.addClass(field.DataType);
        $input.data('type-id', field.Id);
        $field.attr('id', '');
        $field.find('.field-name').text(field.Name);
        $listField.append($field);
    }

    _$thumbnailPanel.append($batchIndex);
    _$tabBatchIndex.removeClass(_clsInputDisable);
}
function CreateDocIndex(docId, docName, batchTypeId, docTypeId, ocrData, ocrImageId) {
    /// <signature>
    /// <summary>Create doc index.</summary>
    /// <param name="docId" type="String">Id of doc.</param>
    /// <param name="docName" type="String">Name of doc.</param>
    /// <param name="docTypeId" type="String">Id doc type of doc.</param>
    /// <param name="ocrData" type="Dictionary<Guid,String>">Dictionary of OCR value</param>
    /// <param name="ocrImageId" type="String">Id of dictionary OCR image in session.</param>
    /// </signature>

    // Change List object OCR data to Map object OCR data
    var ocrDatas = {},
        countOcrDatas = ocrData.length;
    for (var i = 0; i < countOcrDatas; i++) {
        var ocrDataId = ocrData[i].FieldId;
        delete ocrData[i].FieldId;
        ocrDatas[ocrDataId] = ocrData[i];
    }
    var ocrImageIds = {};
    for (var i = 0; i < countOcrDatas; i++) {
        var ocrImageFieldId = ocrImageId[i].FieldId;
        delete ocrImageId[i].FieldId;
        ocrImageIds[ocrImageFieldId] = ocrImageId[i];
    }
    console.log(ocrImageIds);
    // Add batch index
    var $docIndex = _$templateDocIndex.clone();
    $docIndex.attr('id', _prfxDocIndex + docId);
    $docIndex.find('.name').text(docName);

    // Add index for this doc
    var indexDoc = $('#' + docId).index();
    $docIndex.find('.item-doc-index').text(indexDoc);

    var $listField = $docIndex.children('.current_content_fields'),
        countDocField = _batchTypes[batchTypeId].DocTypes[docTypeId].Fields.length,
        currentDate = new Date(),
        checkHandler,
        newIds = [];

    for (var i = 0; i < countDocField; i++) {

        var field = _batchTypes[batchTypeId].DocTypes[docTypeId].Fields[i],
            fieldId = NewGuid(newIds),
            $field,
            $input;

        newIds.push(fieldId);

        if (field.DataType == 'Boolean') {

            // Boolean field
            $field = _$templateFieldBool.clone();

            $input = $field.children('.content_fields_input').children('input');
            $input.removeAttr('maxlenght');
            if (field.DefaultValue == 'True') {
                $input.attr('checked', 'checked');
            }

        } else if (field.DataType == 'Picklist') {

            // Picklist field
            $field = _$templateFieldPicklist.clone();
            $input = $field.children('.content_fields_input').children('select');

            if (!field.IsRequired)
                $input.append('<option value=""></option>');

            var flgHitDefault = false,
                defaultValueIndex = -1,
                countPicklists = field.Picklists.length;

            // Set OCR value if any
            if (ocrDatas[field.Id] != undefined) {
                field.DefaultValue = ocrDatas[field.Id].Value;
                $input.data('ocr-image-id', ocrImageIds[field.Id].OcrId);
            }

            for (var j = 0; j < countPicklists; j++) {
                var picklist = field.Picklists[j],
                    $option = $('<option></option>');

                $option.attr('value', picklist.Id);
                $option.text(picklist.Value);
                $input.append($option);

                // Get index of default value
                if (!flgHitDefault)
                    if (picklist.Value == field.DefaultValue) {
                        defaultValueIndex = i;
                        flgHitDefault = true;
                    }
            }

            if (flgHitDefault) {
                // Plus default value index by 1 if field is not required
                if (!field.IsRequired)
                    defaultValueIndex += 1;

                $input[0].options[defaultValueIndex].selected = true;
            } else {
                if (field.IsRequired)
                    $input[0].selectedIndex = -1;
                else
                    $input[0].selectedIndex = 0;
            }

            if (field.IsRequired) {
                var validateOptions = {
                    type: 'Picklist',
                    requiredValidation: {
                        invalidMessage: _messageRequiredField
                    },
                    validCallback: CheckCanSubmit,
                    invalidCallback: CanNotSubmit
                };

                Input_Validate($input, validateOptions);
                $input.trigger('change');
            }

        } else if (field.DataType == 'Table') {

            // Picklist field
            $field = _$templateFieldTable.clone(true);
            $input = $field.children('.content_fields_input');

            // Initialize detail table
            var $tables = $field.find('.detail-table');
            $tables.attr('id', 'tbl-' + fieldId);

            var $trHead = $tables.find('thead > tr'),
                $trDataTemplate = $tables.find('.template-row'),
                $tdDataTemplate = $trDataTemplate.children('.td-data'),
                countChildren = field.Children.length;

            $tdDataTemplate.remove();

            // Click handler new row
            $tables.find('.click-new-row').click(function () {

                var $this = $(this),
                    $table = $this.prev('.table'),
                    $trRow = $table.find('.template-row').clone(true);

                $trRow.removeClass('template-row').addClass('real-row');
                $table.find('tbody').append($trRow);

                // Add check validate handler
                $trRow.find('.index-field').each(function () {

                    var $cellInput = $(this);
                    $cellInput.attr('id', NewGuid());

                    // Add check validate handler
                    var validateOptions = {
                        patternValidations: []
                    },
                        invalidFieldType = false,
                        checkHandler = Input_Validate;

                    var dataType = $cellInput.data('data-type');
                    switch (dataType) {
                        case 'String':
                            checkHandler = undefined;
                            break;

                        case 'Integer':
                            child.MaxLength = 10;

                            validateOptions.patternValidations.push({
                                pattern: _inputIntegerPattern,
                                isRestoreOldValue: true
                            });
                            validateOptions.patternValidations.push({
                                pattern: _validIntegerPattern,
                                invalidMessage: _messageInvalidInteger,
                            });
                            break;

                        case 'Decimal':
                            child.MaxLength = 22;
                            child.DefaultValue = child.DefaultValue.replace('.', _decimalSeparator);

                            validateOptions.patternValidations.push({
                                pattern: _inputDecimalPattern,
                                isRestoreOldValue: true
                            });
                            validateOptions.patternValidations.push({
                                pattern: _validDecimalPattern,
                                invalidMessage: _messageInvalidDecimal,
                            });
                            break;

                        case 'Date':
                            child.MaxLength = 10;
                            child.DefaultValue = currentDate.toString(_dateFormat);

                            validateOptions.type = 'Date';
                            validateOptions.dateFormatValidation = {
                                dateFormat: _dateFormat,
                                invalidMessage: _messageInvalidDate
                            };
                            break;

                        default:
                            invalidFieldType = true;
                            break;
                    }

                    if (!invalidFieldType) {
                        // Add check handler
                        if (checkHandler != undefined) {
                            checkHandler($cellInput, validateOptions);
                        }
                    }

                    $cellInput.val($cellInput.data('template-value'));
                    $cellInput.trigger('input');
                });

                // Remove row value
                $trRow.find('.del-col').click(function () {
                    $(this).parent().parent().remove();
                });

                // Add clear handler
                $trRow.find('.close').each(function () {
                    ButtonClear_Click($(this));
                });
            });

            // Create row template
            for (var j = 0; j < countChildren; j++) {
                var child = field.Children[j];

                // Add header column
                $trHead.append('<th class="td-data">' + child.Name + '</th>');

                // Add template cell data
                var $tdData = $tdDataTemplate.clone();
                var $inputCell = $tdData.find('.index-field');

                switch (child.DataType) {
                    case 'String':
                        checkHandler = undefined;
                        break;

                    case 'Integer':
                        child.MaxLength = 10;
                        break;

                    case 'Decimal':
                        child.MaxLength = 22;
                        child.DefaultValue = child.DefaultValue.replace('.', _decimalSeparator);
                        break;

                    case 'Date':
                        child.MaxLength = 10;
                        child.DefaultValue = currentDate.toString(_dateFormat);
                        break;

                    default:
                        invalidFieldType = true;
                        break;
                }

                if (invalidFieldType) {
                    continue;
                }

                $inputCell.attr('maxlength', child.MaxLength);
                $inputCell.data('template-value', child.DefaultValue);
                $inputCell.data('data-type', child.DataType);
                $trDataTemplate.append($tdData);
            }

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

                    var $this = $(this),
                        $rows = $this.find('.real-row'),
                        countRows = $rows.length;
                    $clickDetail = $('#' + $this.attr('id').substr('tbl-'.length));

                    // Turn off layout
                    $this.layout().destroy();

                    // Check have no row or have invalid row
                    if (countRows == 0 || $rows.find('.index-field.invalid').length > 0) {
                        $clickDetail.addClass('invalid');
                        CanNotSubmit();
                        return;
                    }

                    if (!$clickDetail.hasClass('required')) {
                        $clickDetail.removeClass('invalid');
                        CheckCanSubmit();
                        return;
                    }

                    var countValidRows = 0;
                    // Check have at least one row input all value
                    for (var i = 0; i < countRows ; i++) {
                        var $cells = $($rows[i]).find('.index-field'),
                            countCells = $cells.length,
                            hasValue = false;
                        for (var j = 0; j < countCells; j++) {
                            var $cell = $($cells[j]),
                                value;

                            if ($cell.data('data-type') == 'Date')
                                value = $cell.data('date-value');
                            else
                                value = $cell.val();

                            if (value != undefined && value.trim() != '') {
                                countValidRows++;
                                break;
                            }
                        }
                    }

                    if (countValidRows == 0) {
                        $clickDetail.addClass('invalid');
                        CanNotSubmit();
                    } else {
                        $clickDetail.removeClass('invalid');
                        CheckCanSubmit();
                    }
                }
            });

            if (field.IsRequired) {
                $input.addClass('required').addClass('invalid');
                CanNotSubmit();
            }

            // Click handler show detail of table
            $input.click(function () {
                var $this = $(this);

                var $table = $('#tbl-' + $this.attr('id'));

                $table.dialog('option', 'title', $this.prev().children().first().html());
                $table.dialog('option', 'position', { my: "center", at: "center", of: window });
                $table.dialog("open");
            });

        } else {

            // String, Integer, Decimal, Date field
            var validateOptions = {
                patternValidations: [],
                validCallback: CheckCanSubmit,
                invalidCallback: CanNotSubmit
            },
                invalidFieldType = false,
                checkHandler = Input_Validate;

            if (field.IsRequired) {
                validateOptions.requiredValidation = {
                    invalidMessage: _messageRequiredField
                };
            }

            switch (field.DataType) {
                case 'String':
                    if (!field.IsRequired && field.ValidationPattern == '') {
                        checkHandler = undefined;
                    }
                    break;

                case 'Integer':
                    field.MaxLength = 10;

                    validateOptions.patternValidations.push({
                        pattern: _inputIntegerPattern,
                        isRestoreOldValue: true
                    });
                    validateOptions.patternValidations.push({
                        pattern: _validIntegerPattern,
                        invalidMessage: _messageInvalidInteger,
                    });
                    break;

                case 'Decimal':
                    field.MaxLength = 22;
                    field.DefaultValue = field.DefaultValue.replace('.', _decimalSeparator);

                    validateOptions.patternValidations.push({
                        pattern: _inputDecimalPattern,
                        isRestoreOldValue: true
                    });
                    validateOptions.patternValidations.push({
                        pattern: _validDecimalPattern,
                        invalidMessage: _messageInvalidDecimal,
                    });
                    break;

                case 'Date':
                    field.MaxLength = 10;
                    field.DefaultValue = currentDate.toString(_dateFormat);

                    validateOptions.type = 'Date';
                    validateOptions.dateFormatValidation = {
                        dateFormat: _dateFormat,
                        invalidMessage: _messageInvalidDate
                    };
                    break;

                default:
                    invalidFieldType = true;
                    break;
            }

            if (invalidFieldType)
                continue;

            // Add custom validate pattern
            if (field.ValidationPattern != undefined && field.ValidationPattern.trim() != '') {
                validateOptions.patternValidations.push({
                    pattern: field.ValidationPattern.trim(),
                    invalidMessage: _messageInvalidValidatePattern + ': ' + field.ValidationPattern,
                });
            }

            $field = _$templateFieldOther.clone();
            $input = $field.children('.content_fields_input').children('input');
            $input.attr('id', fieldId);
            $input.attr('maxlength', field.MaxLength);

            // Add check handler
            if (checkHandler != undefined) {
                checkHandler($input, validateOptions);
            }

            // Set OCR value if any
            if (ocrDatas[field.Id] != undefined) {
                $docIndex.data('has-ocr', 'True')

                field.DefaultValue = ocrDatas[field.Id].Value;
                $input.data('ocr-image-id', ocrImageIds[field.Id].OcrId);

                // Show OCR zone when focus in input
                $input.focus({ ocr: field.Ocr }, function (event) {
                    var ocr = event.data.ocr,
                        $liDoc = $('#' + _activeDocId),
                        $liPage = $($liDoc.find('.li-page')[ocr.PageIndex]),
                        $this = $(this);

                    // Do nothing when not is real image
                    if ($liPage.hasClass('native-image') || $this.data('ocr-image-id') == undefined)
                        return;

                    // Show panel ocr
                    //ShowOrHideOcrPanel('show');
                    _$imgOcr.attr('src', _urlGetOcrImage + '?id=' + $this.data('ocr-image-id'));
                    _$imgOcr.show();

                    // Scale OCR zone
                    var scale = Number($liDoc.data('scale')),
                        pageWidthPlus = Number($liPage.data('width')) + 1,
                        pageHeightPlus = Number($liPage.data('height')) + 1,
                        ocrWidth = Math.round(ocr.Width * scale / 10),
                        ocrHeight = Math.round(ocr.Height * scale / 10),
                        ocrTop = Math.round(ocr.Top * scale / 10),
                        ocrLeft = Math.round(ocr.Left * scale / 10);

                    // Do nothing when OCR zone is not in page
                    if (ocrWidth > pageWidthPlus
                        || ocrHeight > pageHeightPlus
                        || ocrLeft + ocrWidth > pageWidthPlus
                        || ocrTop + ocrHeight > pageHeightPlus)
                        return;

                    // Create OCR zone
                    var $ocrRec = $('<div>');
                    $ocrRec.addClass(_clsOcr);
                    $ocrRec.css({
                        width: ocr.Width + 'px',
                        height: ocr.Height + 'px',
                        top: ocr.Top + 'px',
                        left: ocr.Left + 'px'
                    });

                    // Add to page and show it
                    var $viewPage = $('#view-page-' + $liPage.attr('id'));
                    $viewPage.append($ocrRec);

                    // Scroll to this OCR Zone
                    // First scroll to page that own OCR zone
                    _$viewPanel.scrollTop(_$viewPanel.scrollTop() + $viewPage.position().top);
                    // After scroll to page if OCR zone is still hidden => move OCR Zone to middle view panel
                    var f = $ocrRec.belowTheViewPort({ container: _$viewPanel, threshold: -$ocrRec.outerHeight() });
                    if (f > 0) {
                        _$viewPanel.scrollTop(_$viewPanel.scrollTop() + f + _$viewPanel.height() / 2);
                    }
                });
                $input.blur({ ocr: field.Ocr }, function (event) {
                    var ocr = event.data.ocr,
                        $liDoc = $('#' + _activeDocId),
                        $liPage = $($liDoc.find('.li-page')[ocr.PageIndex]),
                        $this = $(this);

                    // Do nothing when not is real image
                    if ($liPage.hasClass('native-image') || $this.data('ocr-image-id') == undefined)
                        return;

                    // Show panel ocr
                    //ShowOrHideOcrPanel('hide');
                    _$imgOcr.hide();

                    // Add to page and show it
                    var $viewPage = $('#view-page-' + $liPage.attr('id'));
                    $viewPage.find('.' + _clsOcr).remove();
                });
            }

            $input.val(field.DefaultValue);
            $input.trigger('input');

            var $buttonClear = $input.nextAll('.close');
            ButtonClear_Click($buttonClear);
        }

        $input.addClass(field.DataType);
        $input.attr('id', fieldId);
        $input.data('type-id', field.Id);
        $field.attr('id', '');
        var $fieldName = $field.find('.field-name');
        $fieldName.text(field.Name);
        if (field.IsRequired) {
            $('<span class="required">&nbsp;*</span>').insertAfter($fieldName);
        }
        $listField.append($field);
    }

    _$thumbnailPanel.append($docIndex);
    _$tabBatchIndex.removeClass(_clsInputDisable);
}

function ProcessBatchTypeInfoResult(data) {
    /// <signature>
    /// <summary>Description.</summary>
    /// </signature>

    var docTypes = {},
        countDocTypes = data.DocTypes.length;

    for (var i = 0; i < countDocTypes; i++) {
        var docTypeId = data.DocTypes[i].Id;
        delete data.DocTypes[i].Id;
        docTypes[docTypeId] = data.DocTypes[i];
    }

    data.DocTypes = docTypes;

    return data;
}

function BatchThumb_Click(liId) {
    /// <signature>
    /// <summary>Click event handler of li batch thumbnail.</summary>
    /// <param name="liId" type="String">Id of li batch</param>
    /// </signature>

    var $itemContent = $('#' + liId).children('.item-content');

    // Expand or collapse list children
    $itemContent.children('.item-icon').click(function (e) {
        e.stopPropagation();
        var $itemContent = $(this).parent();
        $itemContent.next('.item-children').slideToggle();
        $itemContent.toggleClass(_clsExpand);
    });

    // Show panel view batch intro
    $itemContent.click(function (e) {
        var $this = $(this);

        $('.item-content.item-select').removeClass('item-select');
        $this.addClass('item-select');

        _$tabDocIndex.addClass(_clsInputDisable);
        ShowOrHideCommentPanel('hide');
        ShowOrHideControlPanel('hide');
        $('#' + _prfxViewDoc + _activeDocId).hide();

        // Update status
        _activeType = 'batch';
        _activeBatchId = $this.parent().attr('id');
        _activeDocId = '';
        _activePageId = '';

        _$viewBatchIntro.show();

        // Scroll to target element
        _$viewPanel.scrollTop(_$viewPanel.scrollTop() + _$viewBatchIntro.position().top);
    });
}
function DocThumb_Click(liId) {
    /// <signature>
    /// <summary>Click event handler of li doc thumbnail.</summary>
    /// <param name="liId" type="String">Id of li doc</param>
    /// </signature>

    var $itemContent = $('#' + liId).children('.item-content');

    // Expand or collapse list children
    $itemContent.children('.item-icon').click(function (e) {
        e.stopPropagation();
        var $itemContent = $(this).parent();
        $itemContent.next('.item-children').slideToggle();
        $itemContent.toggleClass(_clsExpand);
    });

    // Show panel view doc
    $itemContent.click(function (e) {
        var $this = $(this);

        $('.item-content.item-select').removeClass('item-select');
        $this.addClass('item-select');

        ShowOrHideCommentPanel('hide');
        $('#' + _prfxViewDoc + _activeDocId).hide();
        _$viewBatchIntro.hide();

        // Update status
        var $liDoc = $this.parent(),
            $liBatch = $liDoc.parent().parent().parent();
        _activeType = 'doc';
        _activeBatchId = $liBatch.attr('id');
        _activeDocId = $liDoc.attr('id');
        _activePageId = '';

        _$tabDocIndex.removeClass(_clsInputDisable);
        ShowOrHideControlPanel('show');
        var $viewDoc = $('#' + _prfxViewDoc + _activeDocId).show(),
            $viewPage = $viewDoc.children('.' + _clsViewPage + ':first');

        // Scroll to target element
        _$viewPanel.scrollTop(_$viewPanel.scrollTop() + $viewPage.position().top);
    });
}
function PageThumb_Click(liId) {
    /// <signature>
    /// <summary>Click event handler of li page thumbnail.</summary>
    /// <param name="liId" type="String">Id of li page</param>
    /// </signature>

    var $itemContent = $('#' + liId).children('.item-content');

    // Expand or collapse list children
    $itemContent.children('.item-icon').click(function (e) {
        e.stopPropagation();
        var $itemContent = $(this).parent();
        $itemContent.next('.item-children').slideToggle();
        $itemContent.toggleClass(_clsExpand);
    });

    // Show panel view page
    $itemContent.click(function (e) {
        var $this = $(this);

        $('.item-content.item-select').removeClass('item-select');
        $this.addClass('item-select');

        ShowOrHideCommentPanel('hide');
        $('#' + _prfxViewDoc + _activeDocId).hide();
        _$viewBatchIntro.hide();

        // Update status
        var $liPage = $this.parent(),
            $liDoc = $liPage.parent().parent().parent(),
            $liBatch = $liDoc.parent().parent().parent();
        _activeType = 'page';
        _activeBatchId = $liBatch.attr('id');
        _activeDocId = $liDoc.attr('id');
        _activePageId = $liPage.attr('id');

        // Enable or disable tab doc index
        if ($liDoc.hasClass('loose-item'))
            _$tabDocIndex.addClass(_clsInputDisable);
        else
            _$tabDocIndex.removeClass(_clsInputDisable);

        ShowOrHideControlPanel('show');
        var $viewDoc = $('#' + _prfxViewDoc + _activeDocId).show(),
            $viewPage = $('#' + _prfxViewPage + _activePageId);

        // Scroll to target element
        _$viewPanel.scrollTop(_$viewPanel.scrollTop() + $viewPage.position().top);

        // Load not-loaded real image in this doc
        var $realImages = $viewDoc.children('.' + _clsViewPage + '.' + _clsNotLoad);
        // Remove class not load
        $realImages.each(function (index, elem) {
            $(elem).removeClass(_clsNotLoad);
        });
        // Load real image
        $realImages.each(function (index, elem) {
            LoadImagePage(elem);
        });
    });
}

function InitThumbnailContextMenu() {
    /// <signature>
    /// <summary>Initialize context menu for thumbnail item.</summary>
    /// </signature>

    // Build context menu in thumbnail
    $('.tree-item.ul-batch').contextMenu({
        build: function ($trigger, e) {
            if (_syncCountLoadingImage > 0) {
                return false;
            }

            $trigger.trigger('click');

            var menus;
            var $liTrigger = $trigger.parent();
            var $liActiveBatch = $('#' + _activeBatchId);
            var canClassify = $liActiveBatch.data('can-classify');

            // li which is clicked

            if ($liTrigger.hasClass('li-batch')) {
                var typeId = $liTrigger.data('type-id');
                CreateMenuBatch(typeId);
                menus = _menuBatch;
            } else if ($liTrigger.hasClass('li-doc')) {

            }

            return {
                items: menus,
                callback: function (key, options) {
                    if (key.indexOf('ImportDoc_') == 0) {
                        // Import new doc
                        _insertType = 'ImportDoc';

                        _batchTypeId = key.substr('ImportDoc_'.length, 36);
                        console.log(_batchTypeId);
                        _docTypeId = key.substr('ImportDoc_'.length + 37);

                        $('#upload-do-ocr').val(true);
                        $('#upload-do-barcode').val(true);
                        $('#upload-batch-type-id').val(_batchTypeId);
                        $('#upload-doc-type-id').val(_docTypeId);

                    } else {
                        switch (key) {
                            case 'ImportClassifyLater':
                                _insertType = key;
                                $('#upload-do-ocr').val(true);
                                $('#upload-do-barcode').val(true);
                                var $activeLiBatch = $('#' + _activeBatchId);
                                var typeId = $activeLiBatch.data('type-id');
                                $('#upload-batch-id').val(_activeBatchId);
                                $('#upload-batch-type-id').val(typeId);
                                $('#upload-doc-type-id').val('00000000-0000-0000-0000-000000000000');
                                break;

                            case 'ChangeBatchType':
                                _$dlgCreateBatch.data('action', key);
                                _$panelBatchName.hide();
                                _$panelBatchType.show();
                                _$panelOption.hide();
                                _$dlgCreateBatch.dialog('option', 'position', { my: "center", at: "center", of: window });
                                _$dlgCreateBatch.dialog("open");
                                break;

                            case 'RenameBatch':
                                _$dlgCreateBatch.data('action', key);
                                _$panelBatchName.show();
                                _$panelBatchType.hide();
                                _$panelOption.hide();
                                _$dlgCreateBatch.dialog('option', 'position', { my: "center", at: "center", of: window });
                                _$dlgCreateBatch.dialog("open");
                                break;
                        }
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
            height: 30, fontSize: 11
        }
    });
}

function CreateMenuBatch(batchTypeId) {
    /// <signature>
    /// <summary>Create menu for li batch.</summary>
    /// <param name="batchTypeId" type="String">Id batch type of batch.</param>
    /// </signature>
    var scanDocTypeMenus = {};
    importDocTypeMenus = {},
    cameraDocTypeMenus = {};

    var docTypes = _batchTypes[batchTypeId].DocTypes;
    for (key in docTypes) {
        if (docTypes.hasOwnProperty(key)) {
            var val = docTypes[key];

            scanDocTypeMenus['ScanDoc_' + batchTypeId + '_' + key] = { name: val.Name, preIconUrl: val.Icon };
            importDocTypeMenus['ImportDoc_' + batchTypeId + '_' + key] = {
                name: val.Name, labelFor: _idBtnUploadBrowser, preIconUrl: val.Icon
            };
            cameraDocTypeMenus['CameraDoc_' + batchTypeId + '_' + key] = { name: val.Name, preIconUrl: val.Icon };
        }
    }

    _menuBatch.BatchScan.items.ScanNewDocument.items = scanDocTypeMenus;
    _menuBatch.BatchImport.items.ImportNewDocument.items = importDocTypeMenus;
    _menuBatch.BatchCamera.items.CameraNewDocument.items = cameraDocTypeMenus;

    // Enable or disable menu change batch type
    var docCount = $('#' + _activeBatchId).find('.ul-doc').children().length;
    if (docCount > 1)
        _menuBatch.ChangeBatchType.disabled = true;
    else
        _menuBatch.ChangeBatchType.disabled = false;

    // Enable or disable menu submit
    if (_$btnSubmit.hasClass(_clsInputDisable))
        _menuBatch.BatchSubmit.disabled = true;
    else
        _menuBatch.BatchSubmit.disabled = false;
}

function CreateMenuDoc(docTypeId) {
    /// <signature>
    /// <summary>Create menu for li batch.</summary>
    /// <param name="batchTypeId" type="String">Id batch type of batch.</param>
    /// </signature>

    var scanDocTypeMenus = {};
    importDocTypeMenus = {},
    cameraDocTypeMenus = {};

    var docTypes = _batchTypes[batchTypeId].DocTypes;
    for (key in docTypes) {
        if (docTypes.hasOwnProperty(key)) {
            var val = docTypes[key];
            scanDocTypeMenus['ScanDoc_' + batchTypeId + '_' + key] = { name: val.Name };
            importDocTypeMenus['ImportDoc_' + batchTypeId + '_' + key] = {
                name: val.Name, labelFor: _idBtnUploadBrowser
            };
            cameraDocTypeMenus['CameraDoc_' + batchTypeId + '_' + key] = { name: val.Name };
        }
    }

    _menuBatch.BatchScan.items.ScanNewDocument.items = scanDocTypeMenus;
    _menuBatch.BatchImport.items.ImportNewDocument.items = importDocTypeMenus;
    _menuBatch.BatchCamera.items.CameraNewDocument.items = cameraDocTypeMenus;

    // Enable or disable menu change batch type
    var docCount = $('#' + _activeBatchId).find('.ul-doc').children().length;
    if (docCount > 1)
        _menuBatch.ChangeBatchType.disabled = true;
    else
        _menuBatch.ChangeBatchType.disabled = false;

    // Enable or disable menu submit
    if (_$btnSubmit.hasClass(_clsInputDisable))
        _menuBatch.BatchSubmit.disabled = true;
    else
        _menuBatch.BatchSubmit.disabled = false;
}

function CanNotSubmit() {
    _$btnSubmit.addClass(_clsInputDisable);
}

// Main js start work here
$(document).ready(function () {
    setTimeout(function () {

        InitCaptureViewerVariables();   // This function is in file "/Scripts/Pages/CaptureViewer.js"
        InitCaptureViewerLayout(65);    // This function is in file "/Scripts/Pages/CaptureViewer.js"
        // Make four button "Create batch", "Submit", "Previous", "Next" in left bar have the same width
        // This function is in file "/Scripts/Pages/CaptureViewer.js"
        SetTheSameWidthButtons([_$btnCreate, _$btnSubmit, _$btnPrev, _$btnNext], 80);

        _$btnSubmit.addClass(_clsInputDisable);
        _$tabBatchIndex.addClass(_clsInputDisable);
        _$tabDocIndex.addClass(_clsInputDisable);
        _$tabComment.addClass(_clsInputDisable);
        _$viewBatchIntro.show();
        ShowOrHideControlPanel('hide');

        InitCaptureVariables();

        $(document).tooltip({
            position: { my: "left top", at: "left bottom", collision: "flipfit" }
        });

        InitCreateBatchDialog();
        InitThumbnailContextMenu();
        InitUploadFile();
        InitOcrZone();

        TabThumbnail_Click();
        TabBatchIndex_Click();
        TabDocIndex_Click();

        BtnPrev_Click();
        BtnNext_Click();

        _$btnSubmit.click(function () {

            var item = $('#' + _activePageId).children('.item-content');
            var f = item.belowTheViewPort({ container: _$thumbnailPanel, threshold: -item.outerHeight() });
            if (f > 0) {
                _$thumbnailPanel.scrollTop(_$thumbnailPanel.scrollTop() + f);

            }
        });

        // Expand/Collapse list item of batch or doc
        _$ulThumb.on('click', '.item-icon', function (e) {
            e.stopPropagation();
            $(this).parent().toggleClass(_clsExpand).next('.item-children').slideToggle();
        })

        // Click handler on li batch or li doc or li page
        _$ulThumb.on('click', '.item-content', function () {
            
            var $this = $(this);
            var $li = $this.parent();
            console.log($li[0]);
            //if ($this.hasClass('item-select')) {

            //} else {
            //    $('.item-content.item-select').removeClass('item-select');
            //    $this.addClass('item-select');

            //    _$tabDocIndex.addClass(_clsInputDisable);
            //    ShowOrHideCommentPanel('hide');
            //    ShowOrHideControlPanel('hide');
            //    $('#' + _prfxViewDoc + _activeDocId).hide();

            //    // Update status
            //    _activeType = 'batch';
            //    _activeBatchId = $this.parent().attr('id');
            //    _activeDocId = '';
            //    _activePageId = '';

            //    _$viewBatchIntro.show();

            //    // Scroll to target element
            //    _$viewPanel.scrollTop(_$viewPanel.scrollTop() + _$viewBatchIntro.position().top);
            //}
        })

        return;

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