// input_name_query new 
var disable_tabpress = function (e) {
    if (e.which == 9) {
        e.preventDefault();
    }
}

// Load the advance search section form specify save query
function LoadSavedCondition(queryId) {

    var panelBody = $(idPanelBody);
    panelBody.ecm_loading_show();

    var sync = { syncCount: 1 };

    if (queryId == "-1") {
        // Select no saved query => load default
        Inno.helper.get(
           UrlLoadDefaultCondition,
           { batchTypeId: $(idSelectedId).val() },
           [panelBody],
           sync,
           function (data) {
               var advanveSearchPanel = $(IdAdvanceSearchQueries);
               advanveSearchPanel.children().slice(1).remove();
               advanveSearchPanel.append(data);
           }
        );
    }
    else {
        Inno.helper.get(
           UrlLoadSavedCondition,
           { batchTypeId: $(idSelectedId).val(), savedQueryId: queryId },
           [panelBody],
           sync,
           function (data) {
               var advanveSearchPanel = $(IdAdvanceSearchQueries);

               advanveSearchPanel.find('.RowCondition').slice(1).remove();
               advanveSearchPanel.append(data);

               advanveSearchPanel.find('.RowCondition').slice(1).each(function () {
                   var divConj = $(this).children().first();
                   var divOper = divConj.next().next();
                   var divInput = divOper.next();

                   var conj = divConj.find('#Conjunction');
                   var oper = divOper.find('#Operator');
                   var value1 = divInput.find('#value1');
                   var value2 = divInput.find('#value2');

                   conj.val(conj.data('selected'));
                   oper.val(oper.data('selected'));
                   value1.val(divInput.data('value1'));
                   value2.val(divInput.data('value2'));
               });
           }
        );
    }
}

// Load combo box operator and control input value
function LoadSearchOperatorAndInput(controlFieldName, idCel, valueControlID) {

    var control = $(controlFieldName);

    var option = control.find('option:selected');
    var dataType = option.data('data-type');
    var operator = option.data('operator');

    // Get div
    var divFieldName = control.parent();
    var divOperator = divFieldName.next();
    var divInput = divOperator.next();

    // Load html for div operator
    divOperator.html('');
    switch (dataType) {
        case 'String':
            divOperator.html($(idTemplateOperatorTypeString).contents().clone());
            break;
        case 'Boolean':
            divOperator.html($(idTemplateOperatorTypeBoolean).contents().clone());
            break;
        case 'Integer':
        case 'Decimal':
        case 'Date':
            divOperator.html($(idTemplateOperatorTypeNumber).contents().clone());
            break;
        default:
            break;
    }
    divOperator.children().first().attr('onchange', "LoadSearchInput(this, '" + dataType + "', this.value)");

    // Load html for div control input
    divInput.html('');
    switch (dataType) {
        case 'Boolean':
            divInput.html($(idTemplateInputBoolean).contents().clone());
            break;
        case 'Date':
            divInput.html($(idTemplateInput1Value).contents().clone());
            divInput.find("#value1").datepicker();
            divInput.find("#value2").datepicker();
            break;
        case 'Picklist':
        case 'Folder':
        case 'Table':
            break
        default:
            divInput.html($(idTemplateInput1Value).contents().clone());
            break;
    }
}

//Create search control input when choose a search operation
function LoadSearchInput(controlOpearator, dataType, operator) {

    // Get div control input
    var divInput = $(controlOpearator).parent().next();

    // In case change between operators is not InBetween => do nothing
    if (operator != SEARCH_OPERATION_IN_BETWEEN) {
        if (divInput.find('#value2').length == 0) {
            return
        }
    }

    // Save value1
    var value1 = divInput.find("#value1").val();
    // Create control
    if ('InBetween' == operator) {

        divInput.html($(idTemplateInput2Value).contents().clone());

    } else {
        divInput.html($(idTemplateInput1Value).contents().clone());
    }

    // Set again value1
    divInput.find("#value1").val(value1);

    // Set date picker
    if ('Date' == dataType) {
        divInput.find("#value1").datepicker();
        divInput.find("#value2").datepicker();
    }
}

// Add more search condition
function AddCondition() {
    var table = $('.layout_table');
    table.append($(idTemplateAddedCondition).contents().clone());
    table.scrollTop(table.children().last().offset().top);
}

// Save query
function SaveQuery(queryId, queryName) {

    var panelBody = $(idPanelBody);
    panelBody.ecm_loading_show();

    var json = {
        queryId: queryId,
        queryName: queryName,
        batchTypeId: $(idSelectedId).val(),
        searchQueryExpressions: []
    };

    $(IdAdvanceSearchQueries).find('.RowCondition').slice(1).each(function () {
        var _this = $(this);

        // Get id of search expression
        var id = _this.attr('id');

        var divConjunction = _this.children().first();
        var divFieldName = divConjunction.next();
        var divOperator = divFieldName.next();
        var divControlInput = divOperator.next();

        var conjunction = divConjunction.find('#Conjunction').val();
        var operator = divOperator.find('#Operator').val();
        var value1 = divControlInput.find('#value1').val();
        var value2 = divControlInput.find('#value2').val();

        // Get field name
        var fieldId;
        var fieldDataType;
        var comboBoxFieldName = divFieldName.find('#FieldName');
        if (comboBoxFieldName.length > 0) {
            fieldId = comboBoxFieldName.val();
            var seletedOption = comboBoxFieldName.find('option:selected');
            fieldDataType = seletedOption.data('datatype');
        } else {
            fieldId = divFieldName.children().first().attr('id');
        }

        json.searchQueryExpressions.push({
            Id: id,
            Condition: conjunction,
            FieldId: fieldId,
            Operator: operator,
            Value1: value1,
            Value2: value2
        });
    });

    var sync = { syncCount: 1 };
    Inno.helper.post(
        UrlSaveQuery,
        JSON.stringify(json),
        [panelBody],
        sync,
        function (data) {
            if ('00000000-0000-0000-0000-000000000000' == data) {
                alert(messageNoSearchQueryExpression);
                return;
            }

            var comboBoxQueryName = $(IdSelectQueryName);
            var selectQueryId = comboBoxQueryName.val();
            if (selectQueryId == '-1') {
                // Case add new value
                comboBoxQueryName.append('<option value="' + data + '">' + queryName + '</option>');
                comboBoxQueryName.val(data);
            }
        }
    );
}

// Save query name
function ShowPopUpQueryName() {
    var popup = $(IdSaveQueryPopup);
    var queryNameInput = $(IdSavedQueryNameInput);

    queryNameInput.html('');

    $.innoDialog({
        title: titleSaveQueryPopup,
        dialog_data: popup,
        type: 'Save_Cancel',
        Save_Button: function () {

            var queryNameMessage = $(IdSavedQueryNameMessage);

            if (queryNameInput.val() == null || queryNameInput.val().trim() == "") {
                queryNameMessage.html(messageRequiredQueryName);
                return;
            }

            var popupPanel = popup.parent();
            popupPanel.ecm_loading_show();

            var sync = { syncCount: 1 };

            // Check query name is existed
            Inno.helper.get(
                UrlIsQueryNameExisted,
                { batchTypeId: $(idSelectedId).val(), queryName: queryNameInput.val().trim() },
                [popupPanel],
                sync,
                function (data) {
                    if (data == 'True') {
                        queryNameMessage.html(messageQueryNameExist);
                    } else {
                        popup.dialog('close');
                        SaveQuery('00000000-0000-0000-0000-000000000000', queryNameInput.val().trim());
                    }
                }
            );
        },
        Cancel_Button: function () {
            $(this).dialog('close');
        }
    });
}

// Delete query
function ShowPopUpDeleteQuery() {
    var popup = $(IdDeleteQueryPopup);
    $.innoDialog({
        title: titleDeleteQueryPopup,
        dialog_data: popup,
        type: 'Yes_No',
        Yes_Button: function () {
            popup.dialog('close');

            var panelBody = $(idPanelBody);
            panelBody.ecm_loading_show();
            var sync = { syncCount: 1 };

            Inno.helper.post(
                UrlDeleteQuery,
                JSON.stringify({ queryId: $(IdSelectQueryName).val() }),
                [panelBody],
                sync,
                function (data) {
                    $(IdSelectQueryName + " option:selected").remove();
                    LoadSavedCondition('-1');
                }
            );
        },
        No_Button: function () {
            $(this).dialog('close');
        }
    });
}

// Load advance search section
function LoadAdvancedSearch(syncObject) {

    Inno.helper.get(
        UrlLoadAdvancedSearch,
        { batchTypeId: $(idSelectedId).val() },
        [$(idPanelBody)],
        syncObject,
        function (data) {
            $(IdAdvanceSearchQueries).empty().html(data);
            // Set date picker for date time control
            $(IdAdvanceSearchQueries).find('.RowCondition').each(function () {
                var children = $(this).children();
                if ('Date' == children.eq(1).children().first().data('data-type')) {
                    children.eq(3).find('#value1').datepicker();
                };
            });
        }
    );
}

// Run search by batch type and status
function GetBatches(syncObject, spanCount) {

    Inno.helper.get(
        urlSearchByBatchTypeAndStatus,
        {
            batchTypeId: $(idSelectedId).val(),
            status: $(idSelectedStatus).val(),
            pageIndex: 1
        },
        [$(idPanelBody)],
        syncObject,
        function (data) {
            $(idPanelResult).children().first().html(data);

            var totalCount = $('#total-count').html();
            spanCount.html(totalCount);

            CheckAllBatch_Click();
            CheckBatch_Click();
            $('#has-more-result').click(function () {
                ShowMoreResult(spanCount);
            });
            createContextMenu();
        }
    );
}

// Run search by advance search
function RunAdvancedSearch() {

    var panelBody = $(idPanelBody);
    panelBody.ecm_loading_show();

    $(idSelectedStatus).val('');
    var sync = { syncCount: 1 };
    var expressions = [];

    $(IdAdvanceSearchQueries).find('.RowCondition').slice(1).each(function () {
        var _this = $(this);

        // Get id of search expression
        var id = _this.attr('id');

        var divConjunction = _this.children().first();
        var divFieldName = divConjunction.next();
        var divOperator = divFieldName.next();
        var divControlInput = divOperator.next();

        var conjunction = divConjunction.find('#Conjunction').val();
        var operator = divOperator.find('#Operator').val();
        var value1 = divControlInput.find('#value1').val();
        var value2 = divControlInput.find('#value2').val();

        // Get field name and data type
        var fieldId;
        var fieldName;
        var fieldDataType;
        var comboBoxFieldName = divFieldName.find('#FieldName');
        if (comboBoxFieldName.length > 0) {
            fieldId = comboBoxFieldName.val();
            var seletedOption = comboBoxFieldName.find('option:selected');
            fieldName = seletedOption.data('field-name');
            fieldDataType = seletedOption.data('data-type');
        } else {
            var spanName = divFieldName.children().first();
            fieldId = spanName.attr('id');
            fieldName = spanName.data('field-name');
            fieldDataType = spanName.data('data-type');
        }

        expressions.push({
            FieldId: fieldId,
            Condition: conjunction,
            Operator: operator,
            Value1: value1,
            Value2: value2,
            FieldMetaData: { Name: fieldName, DataType: fieldDataType }
        });
    });

    $(".assigned_work_item").removeClass("selected");

    Inno.helper.post(
        urlSearchByAdvancedSearch,
        JSON.stringify({
            batchTypeId: $(idSelectedId).val(),
            jsonSearchExpressions: JSON.stringify(expressions),
            pageIndex: 1
        }),
        [panelBody],
        sync,
        function (data) {
            $(idPanelResult).children().first().html(data);
            CheckAllBatch_Click();
            CheckBatch_Click();
            $('#has-more-result').click(function () {
                ShowMoreResult();
            });
            createContextMenu();
        }
    );
}

// Run search more item
function ShowMoreResult(spanCount) {

    var panelBody = $(idPanelBody);
    panelBody.ecm_loading_show();
    var linkShowMore = $('#has-more-result');

    var isSearchByAdvance = linkShowMore.hasClass('search-advance');
    var sync = { syncCount: 1 };

    if (isSearchByAdvance) {
        $(".assigned_work_item").removeClass("selected");

        Inno.helper.post(
            urlSearchByAdvancedSearch,
            JSON.stringify({
                batchTypeId: $(idSelectedId).val(),
                jsonSearchExpressions: JSON.stringify(linkShowMore.data('search-query-json')),
                pageIndex: linkShowMore.data('page-index')
            }),
            [panelBody],
            sync,
            function (data) {
                $(idPanelResult).children().first().html(data);
                CheckAllBatch_Click();
                CheckBatch_Click();
                $('#has-more-result').click(function () {
                    ShowMoreResult();
                });
            }
        );
    } else {
        Inno.helper.get(
            urlSearchByBatchTypeAndStatus,
            {
                batchTypeId: $(idSelectedId).val(),
                status: $(idSelectedStatus).val(),
                pageIndex: linkShowMore.data('page-index')
            },
            [panelBody],
            sync,
            function (data) {
                $(idPanelResult).children().first().html(data);

                var totalCount = $('#total-count').html();
                spanCount.html(totalCount);

                CheckAllBatch_Click();
                CheckBatch_Click();
                $('#has-more-result').click(function () {
                    ShowMoreResult();
                });
            }
        );
    }
}

// Check all batch
function CheckAllBatch_Click() {
    $('#chk-all-batch-id').change(function () {
        if ($(this).is(':checked')) {
            $('.chk-batch-id').attr('checked', true);
        } else {
            $('.chk-batch-id').attr('checked', false);
        }
    });
}

// Check all batch
function CheckBatch_Click() {
    var chkBatchs = $('.chk-batch-id');

    chkBatchs.change(function () {
        var checkedCount = chkBatchs.filter(':checked').size();
        var totalCount = chkBatchs.size();
        if (checkedCount == totalCount) {
            $('#chk-all-batch-id').attr('checked', true);
        } else {
            $('#chk-all-batch-id').attr('checked', false);
        }
    });
}

function createContextMenu() {

    $('#row-work-item').contextMenu({
        build: function ($trigger, e) {
            // this callback is executed every time the menu is to be shown
            // its results are destroyed every time the menu is hidden
            // e is the original contextmenu event, containing e.pageX and e.pageY (amongst other data)

            var chkBox = $trigger.children().first().children();
            var status = chkBox.data('status');

            var _menus = {}
            if (status == 'InProcessing') {
                // Do nothing
            }
            else {
                var permission = chkBox.data('permission');

                if (status == 'Error') {
                    // Resume
                    _menus.Resume = {
                        name: DisplayResume,
                        iconUrl: URL_MenuResume,
                    };
                    // Can delete
                    if (permission.Delete == true) {
                        _menus.Delete = {
                            name: DisplayDelete,
                            iconUrl: URL_MenuDelete,
                        };
                    }
                }
                else {
                    // Set menu for available, rejected and locked
                    // Open
                    _menus.Open = {
                        name: DisplayOpen,
                        iconUrl: URL_MenuOpen,
                    };
                    // Approve
                    _menus.Approve = {
                        name: DisplayApprove,
                        iconUrl: URL_MenuApprove,
                    };
                    // Can reject
                    if (status != 'Reject') {
                        if (permission.Reject == true) {
                            _menus.Reject = {
                                name: DisplayReject,
                                iconUrl: URL_MenuReject,
                            };
                        }
                    }
                    // Unlock
                    if (status != 'Reject' && status != 'Available') {
                        _menus.Unlock = {
                            name: DisplayUnlock,
                            iconUrl: URL_MenuUnlock,
                        };
                    }
                    // Can email as link
                    if (permission.SendLink == true) {
                        _menus.SendLink = {
                            name: DisplaySendLink,
                            iconUrl: URL_MenuSendLink,
                        };
                    }
                    // Can delegate
                    if (permission.Delegate == true) {
                        _menus.Delegate = {
                            name: DisplayDelegate,
                            iconUrl: URL_MenuDelegate,
                        };
                    }
                    // Can delete
                    if (permission.Delete == true) {
                        _menus.Delete = {
                            name: DisplayDelete,
                            iconUrl: URL_MenuDelete,
                        };
                    }
                }
            }

            return {
                callback: function (key, options) {
                    var listId = [];
                    var listActivityNames = [];

                    var $batchTypeName = $('#batch-type-name');
                    var batchTypeName = $batchTypeName.text();
                    var indexActivityName = $batchTypeName.data('index-activity-name');

                    // Add all checked value to list
                    $('.chk-batch-id:checked').each(function () {
                        listId.push($(this).val());
                        listActivityNames.push($(this).parent().nextAll().slice(indexActivityName - 1).first().text());
                    });

                    // Add clicked value if not checked
                    clickedValue = chkBox.val();
                    if (listId.indexOf(clickedValue) == -1) {
                        listId.push(clickedValue);
                    }

                    var panelBody = $(idPanelBody);
                    var sync = { syncCount: 3 };
                    var spanCount = $('.assigned_work_item.selected').children().first();
                    var selectedStatus = $(idSelectedStatus).val();

                    switch (key) {
                        case 'Unlock':
                            panelBody.ecm_loading_show();

                            Inno.helper.post(
                                UrlUnlock,
                                JSON.stringify({ batchIds: listId }),
                                [panelBody],
                                sync,
                                function (data) {
                                    if (data == "True") {
                                        // Update menu again
                                        LoadStatusMenu(sync)

                                        if (selectedStatus != '') {
                                            GetBatches(sync, spanCount)
                                        }
                                        else {
                                            ShowMoreResult();
                                        }
                                    }
                                }
                            );
                            break;

                        case 'Delete':
                            panelBody.ecm_loading_show();

                            Inno.helper.post(
                                UrlDelete,
                                JSON.stringify({ batchIds: listId }),
                                [panelBody],
                                sync,
                                function (data) {
                                    if (data == "True") {
                                        // Update menu again
                                        LoadStatusMenu(sync)

                                        if (selectedStatus != '') {
                                            GetBatches(sync, spanCount)
                                        }
                                        else {
                                            ShowMoreResult();
                                        }
                                    }
                                }
                            );
                            break;

                        case 'Approve':
                            panelBody.ecm_loading_show();

                            Inno.helper.post(
                                UrlApprove,
                                JSON.stringify({ batchIds: listId }),
                                [panelBody],
                                sync,
                                function (data) {
                                    if (data == "True") {
                                        // Update menu again
                                        LoadStatusMenu(sync)

                                        if (selectedStatus != '') {
                                            GetBatches(sync, spanCount)
                                        }
                                        else {
                                            ShowMoreResult();
                                        }
                                    }
                                }
                            );
                            break;

                        case 'Resume':
                            panelBody.ecm_loading_show();

                            Inno.helper.post(
                                UrlResume,
                                JSON.stringify({ batchIds: listId }),
                                [panelBody],
                                sync,
                                function (data) {
                                    if (data == "True") {
                                        // Update menu again
                                        LoadStatusMenu(sync)

                                        if (selectedStatus != '') {
                                            GetBatches(sync, spanCount)
                                        }
                                        else {
                                            ShowMoreResult();
                                        }
                                    }
                                }
                            );
                            break;

                        case 'Reject':
                            var popup = $('#popup-reject');
                            var rejectedNote = $('#rejected-note');
                            rejectedNote.val('');

                            $.innoDialog({
                                title: DisplayReject,
                                dialog_data: popup,
                                type: 'Ok_Cancel',
                                Ok_Button: function () {
                                    popup.dialog('close');
                                    panelBody.ecm_loading_show();

                                    Inno.helper.post(
                                        UrlReject,
                                        JSON.stringify({ batchIds: listId, rejectedNote: rejectedNote.val() }),
                                        [panelBody],
                                        sync,
                                        function (data) {
                                            if (data == "True") {
                                                // Update menu again
                                                LoadStatusMenu(sync)

                                                if (selectedStatus != '') {
                                                    GetBatches(sync, spanCount)
                                                }
                                                else {
                                                    ShowMoreResult();
                                                }
                                            }
                                        }
                                    );
                                },
                                Cancel_Button: function () {
                                    $(this).dialog('close');
                                    //sync.syncCount = 1;
                                }
                            });
                            break;

                        case 'Delegate':
                            var popup = $('#popup-delegate');
                            var toUser = $('#delegate-to-user');
                            var delegateNote = $('#delegate-note');
                            delegateNote.val('');

                            $.innoDialog({
                                title: DisplayDelegate,
                                dialog_data: popup,
                                type: 'Ok_Cancel',
                                Ok_Button: function () {
                                    popup.dialog('close');
                                    panelBody.ecm_loading_show();

                                    Inno.helper.post(
                                        UrlDelegate,
                                        JSON.stringify({
                                            batchIds: listId,
                                            toUser: toUser.val(),
                                            delegateNote: delegateNote.val(),
                                        }),
                                        [panelBody],
                                        sync,
                                        function (data) {
                                            if (data == "True") {
                                                // Update menu again
                                                LoadStatusMenu(sync)

                                                if (selectedStatus != '') {
                                                    GetBatches(sync, spanCount)
                                                }
                                                else {
                                                    ShowMoreResult();
                                                }
                                            }
                                        }
                                    );
                                },
                                Cancel_Button: function () {
                                    $(this).dialog('close');
                                    //sync.syncCount = 1;
                                }
                            });
                            break;

                        case 'SendLink':
                            panelBody.ecm_loading_show();

                            var linkEmail = $('#email-as-link');
                            var username = linkEmail.data('username');
                            var subject = linkEmail.data('subject');
                            var body = linkEmail.data('body') + "\n\n";
                            var serverUrl = linkEmail.data('server-url');

                            // Get link item
                            for (var i = 0; i < listId.length; i++) {
                                body += serverUrl + "?" + "mode=workitem&username=" + username + "&workitemid=" + listId[i];
                            }

                            linkEmail.attr('href', "mailto:?Subject=" + subject + "&Body=" + escape(body));

                            // Call send mail application
                            linkEmail[0].click();

                            panelBody.ecm_loading_hide();
                            break;

                        case 'Open':

                            var openBatches = [];

                            for (var i = 0; i < listId.length; i++) {
                                openBatches.push({
                                    Id: listId[i],
                                    BlockingActivityName: listActivityNames[i],
                                });
                            }

                            var panelBody = $(idPanelBody);
                            panelBody.ecm_loading_show();

                            var sync = { syncCount: 1 };

                            Inno.helper.post(
                                urlOpenBatches,
                                JSON.stringify({
                                    batches: openBatches,
                                    batchTypeName: batchTypeName
                                }),
                                [panelBody],
                                sync,
                                function (data) {
                                    window.location=urlViewBatches;
                                }
                            );

                            break;
                        default:
                            break;
                    }
                },
                items: _menus
            };
        },
        selector: "tr",
        style: {
            height: 30, fontSize: 11, menuWidth: 250
        },
        events: {
            show: function (opt) {
                $('tr.selected').removeClass('selected');
                $(this).addClass("selected");
            },
            hide: function (opt) {
                $('tr.selected').removeClass('selected');
            }
        }
    });

    function createMailForm(Document) {
        //#region create html element
        var formData = $("<form class='save_or_mail_option'></form>");
        var attach = $("<fieldset>" +
                        "<legend>Attachment File Options </legend>" +
                        "<input type='radio' name='format' value='pdf' checked='checked' id='mail_opt_pdf'/>" +
                        "<label for='mail_opt_pdf'>Email attachment as PDF</label>" +
                        "<input type='radio' name='format' value='tiff'/ id='mail_opt_tiff'>" +
                        "<label for='mail_opt_tiff'>Email attachment as TIFF</label>" +
                    "</fieldset>" +
                    "<fieldset> " +
                            "<legend>Page range</legend>" +
                            "<input type='radio' name='pagerange' value='all' id='mail_opt_all' checked='checked'/>" +
                            "<label for='mail_opt_all'>All</label>" +
                            "<input type='radio' name='pagerange' value='pages' id='mail_opt_pages'/>" +
                            "<label for='mail_opt_pages'>Pages</label>" +
                            "<input type='text' name='listofpage' />" +
                         "</fieldset>");
        var mailBy = $("<fieldset> " +
                            "<legend>Send by</legend>" +
                            "<input type='radio' name='sendby' value='server' checked='checked' id='mail_opt_server'/>" +
                            "<label for='mail_opt_server'>Server</label>" +
                            "<input type='radio' name='sendby' value='client' id='mail_opt_client'/>" +
                            "<label for='mail_opt_client'>Client (Open Outlook)</label>" +
                         "</fieldset>");
        var mailTo = $("<fieldset> " +
                        "<legend>To Email</legend>" +
                        "<span>To email </span><input type='text' name='mailTo'/>" +
                        "<a href='#'>Cc, Bcc</a><br/>" +
                    "</fieldset>");
        var cc = $("<span>CC </span><input type='text' name='cc'/>" +
                 "<span>BCC </span><input type='text' name='bcc'/>");
        //#endregion
        //#region create style
        cc.hide();
        mailTo.children('span').css({ display: 'inline-block', width: '60px' });
        mailTo.children('input').css({ display: 'inline-block', width: '150px' });
        mailTo.append(cc);
        mailTo.children('a').click(function () {
            if (cc.css('display') == "none")
                cc.css('display', 'inline-block')
            else
                cc.hide();
            return false;
        });
        cc.filter('span').css({ width: '60px' });
        cc.filter('input').css({ width: '150px' });
        attach.add(mailBy).add(mailTo).css({ 'font-size': '15px', 'font-weight': 'bold' });
        attach.add(mailBy).add(mailTo).children('span,label').css({ 'font-size': '14px', 'font-weight': 'normal' });
        formData.append(attach).append(mailBy).append(mailTo);
        //#endregion
        $.messageBox({
            title: 'Input mail address',
            type: 'form',
            message: null,
            formData: formData,
            width: "500px",
            buttons: {
                OK: function (data) {
                    var mail = {};
                    var form = $('.save_or_mail_option');
                    mail['format'] = $('input[name="format"]:checked', form).val();
                    mail['range'] = $('input[name="pagerange"]:checked', form).val();
                    mail['pages'] = [];
                    var ps = $('input[name="listofpage"]', form).val();
                    if (mail['range'] == 'pages' && ps && ps != " ") {
                        var pages = $('input[name="listofpage"]', form).val().split(',');
                        $.each(pages, function () {
                            if ($.isNumeric(this))
                                mail.pages.push(parseInt(this));
                            if (this.indexOf('-') > 0) {
                                var p2p = this.split('-');
                                if (!$.isNumeric(p2p[0]) || !$.isNumeric(p2p[1])) {
                                    pageError.show();
                                    return;
                                }
                                var d = p2p[1] - p2p[0];
                                if (d >= 0)
                                    for (i = p2p[0]; i <= p2p[1]; i++)
                                        if (mail.pages.indexOf(i) < 0)
                                            mail.pages.push(parseInt(i))

                            }
                        });
                    }
                    mail['sendby'] = $('input[name="sendby"]', form).val();
                    mail['mailTo'] = $('input[name="mailTo"]', form).val();
                    mail['cc'] = $('input[name="cc"]', form).val();
                    mail['bcc'] = $('input[name="bcc"]', form).val();
                    mail['Document'] = Document;
                    $('section.body').ecm_loading_show();
                    Inno.helper.post(URL_SendMail, JSON.stringify(mail), function (data) {
                        $('section.body').ecm_loading_hide();
                        if (data == true)
                            $.messageBox({ type: 'notify', message: 'Your email has been send' });
                        else
                            $.messageBox({ type: 'notify', message: 'Failed to send your email!' });
                    }, function () {
                        $('section.body').ecm_loading_hide();
                    });
                },
                Cancel: function () { }
            }
        });
    }
    function createFormDownload(Document) {
        var formData = $("<form class='save_or_mail_option'></form>");
        var attach = $("<fieldset>" +
                        "<legend>Attachment File Options </legend>" +
                        "<input type='radio' name='format' value='pdf' checked='checked' id='mail_opt_pdf'/>" +
                        "<label for='mail_opt_pdf'>Email attachment as PDF</label>" +
                        "<input type='radio' name='format' value='tiff'/ id='mail_opt_tiff'>" +
                        "<label for='mail_opt_tiff'>Email attachment as TIFF</label><br/>" +
                    "</fieldset><br/>" +
                    "<fieldset> " +
                            "<legend>Page range</legend>" +
                            "<input type='radio' name='pagerange' value='all' id='mail_opt_all' checked='checked'/>" +
                            "<label for='mail_opt_all'>All</label>" +
                            "<input type='radio' name='pagerange' value='pages' id='mail_opt_pages'/>" +
                            "<label for='mail_opt_pages'>Pages</label>" +
                            "<input type='text' name='listofpage' />" +
                         "</fieldset>");
        attach.children('span,label').css({ 'font-size': '14px', 'font-weight': 'normal' });
        formData.append(attach);
        $.messageBox({
            title: 'Save option',
            type: 'form',
            message: null,
            formData: formData,
            width: "500px",
            buttons: {
                OK: function (data) {
                    var mail = {};
                    var form = $('.save_or_mail_option');
                    mail['format'] = $('input[name="format"]:checked', form).val();
                    mail['range'] = $('input[name="pagerange"]:checked', form).val();
                    mail['pages'] = [];
                    var ps = $('input[name="listofpage"]', form).val();
                    if (mail['range'] == 'pages' && ps && ps != " ") {
                        var pages = $('input[name="listofpage"]', form).val().split(',');
                        $.each(pages, function () {
                            if ($.isNumeric(this))
                                mail.pages.push(parseInt(this));
                            if (this.indexOf('-') > 0) {
                                var p2p = this.split('-');
                                if (!$.isNumeric(p2p[0]) || !$.isNumeric(p2p[1])) {
                                    pageError.show();
                                    return;
                                }
                                var s = p2p[1] - p2p[0];
                                if (d >= 0)
                                    for (i = p2p[0]; i <= p2p[1]; i++)
                                        if (mail.pages.indexOf(i) < 0)
                                            mail.pages.push(parseInt(i))

                            }
                        });
                    }
                    mail['Document'] = Document;
                    $('section.body').ecm_loading_show();
                    Inno.helper.post(URL_SaveLocal, JSON.stringify(mail), function (data) {
                        //var $p = $('<iframe src="/capture/get?key=' + data + '"></iframe>');
                        //$("#printdiv").append($p);
                        $('section.body').ecm_loading_hide();
                        window.open(URL_Get + "?key=" + data, "_blank");
                    });
                },
                Cancel: function () { }
            }
        });
    }
}

function LoadStatusMenu(sync) {

    var panelBody = $(idPanelBody);

    Inno.helper.get(
        UrlGetCountStatus,
        { batchTypeId: $(idSelectedId).val() },
        [panelBody],
        sync,
        function (data) {

            var error = $('.document_list.selected').next().find('.assigned_work_item').first();
            var inProcess = error.next();
            var locked = inProcess.next();
            var rejected = locked.next();
            var available = rejected.next();

            error.children().first().html(data.Error);
            inProcess.children().first().html(data.InProccess);
            locked.children().first().html(data.Locked);
            rejected.children().first().html(data.Rejected);
            available.children().first().html(data.Available);
        }
    );
}

var $body;

$(document).ready(function () {

    $body = $('body');
    var selectedId = $('#selected-id');             // Selected batch type id
    var selectedStatus = $('#selected-status');     // Selected status
    var lstStatusMenu = $('.assigned_work_item');

    // Create the view menu in top menu to select the batch
    //CreateOpenedBatchesMenuForController('Search');   // This function in file "~Script/OpenedBatchesMenu.js"

    // Open/Hide the count of assigned work item of clicked batch type
    $('.document_list').click(function () {
        $(this).next().slideToggle();
    });

    // Hander menu click
    lstStatusMenu.click(function () {

        $(idPanelBody).ecm_loading_show();
        var _this = $(this);

        var oldSelectedId = selectedId.val();
        // Get the id of div "assigned_work_item_list" ([div]  [div > ul > li] : this = li)
        var clickedBatchType = _this.parent().parent().prev();
        var newSelectedId = clickedBatchType.attr('id');

        // Update status
        selectedStatus.val(_this.data('status'));
        // Update GUI status
        lstStatusMenu.removeClass('selected');
        _this.addClass('selected');

        var sync = { syncCount: 1 };

        // Click different batch type
        if (oldSelectedId != newSelectedId) {
            // Update selected batch type id
            selectedId.val(newSelectedId);

            // Update GUI
            $('#' + oldSelectedId).removeClass("selected");
            clickedBatchType.addClass("selected");

            sync.syncCount = 2;
            // Load section advance search
            LoadAdvancedSearch(sync);
        }

        // Load section search result
        GetBatches(sync, _this.children().first());
    });

    // Handle button add query condition click
    $("#bt_add_conditions").click(function () {
        if ($(IdSelectedBatchTypeId).val() != "") {
            AddCondition();
        }
    });

    // Handle button reset query condition click
    $("#bt_reset_conditions").click(function () {
        if ($(IdSelectedBatchTypeId).val() != "") {
            $("select" + IdSelectQueryName).val('-1');
            LoadSavedCondition('-1');
        }
    });

    // Handle button save query condition click
    $("#bt_save_query").click(function () {
        // Must have selected batch type
        if ($(IdSelectedBatchTypeId).val() != "") {
            var queryId = $(IdSelectQueryName).val();
            // Compare queryId
            if (queryId == '-1') {
                ShowPopUpQueryName();
            }
            else {
                SaveQuery(queryId, "");
            }
        }
    });

    // Handle button delete query condition click
    $("#bt_delete_query").click(function () {
        // Must have selected batch type
        if ($(IdSelectedBatchTypeId).val() != "") {
            // Compare queryId
            if ($("#selectqueryname").val() != -1) {
                ShowPopUpDeleteQuery();
            }
        }
    });

    // Handle button search click
    $("#search-advance").click(function () {
        var _this = $(this);
        // Must have selected batch type
        if ($(idSelectedId).val() != "") {
            RunAdvancedSearch()
        }
    });

    InitOpenedBatchesMenu();
});