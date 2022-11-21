//##################################################################
//# Copyright (C) 2008-2013, Innoria Solution. All Rights Reserved. #
//# 
//# History: 
//# Date Time   Updater     Comment 
//# 23/08/2013  ThoDinh       Tạo mới 
//##################################################################
$(document).ready(function () {
    var insOption;
    var pageCurrent;
    var list = [];
    var docTypeName;
    var Annotations = [];
    var $classifyLaterFolder = $('<ul class="default_batch connectedSortable"></ul>').css({ 'min-height': '2px' });
    var $docTypeId = $("#docTypeId");
    var fields = {};
    var $thumbnail = $('.thumbnail');
    var $selectedPage;
    var $selectedFolder;
    var draw;
    //Khai bao Enum
    var Options = {
        Import: 0,
        InsertBefore: 1,
        InsertAfter: 2,
        Replace: { Import: 31, Scan: 32 },
        NewFromSelected: 4,
        NewStartingHer: 5,
        ClassifyLater: 6
    };

    var tool = new Toolbar();
    $thumbnail.append($classifyLaterFolder);
    _top = ($('body').scrollTop() + $('body').height() - 300) / 2;
    _left = ($('body').scrollTop() + $('body').width() - 300) / 2;
    $recyclebin = $('<img src="' + URL_Recycle + '"/>').css({
        position: 'absolute', top: _top, left: _left, height: 300, width: 300, opacity: '0.1', zIndex: 2
    });
    $itemToRemove = null;

    $("ul.default_batch").sortable({
        revert: false,
        opacity: 0.5,
        placeholder: "ui-state-highlight",
        update: function (e, ui) {
            updatePageElements($(this));
        },
        receive: function (e, ui) {
            //ID cua page nhan dc
            var id = ui.item.find('.page').attr('id');
            var $page = $("#page_" + id);
            //Tim document se them page nay vao
            //$(this) trong context nay la <ul> chua cac thumbnail <li>
            $documentViewer.children().hide();
            var p = $("<div class='document'/>");
            p.appendTo($documentViewer);
            $page.appendTo(p);
            p.show();
            $documentViewer.scrollTo($page);
        },
        start: function (e, ui) {
            $itemToRemove = null;
            $('body').append($recyclebin);
            $recyclebin.droppable({
                drop: function (event, u) {
                    $recyclebin.remove();
                    deletePage(u.draggable);
                    //u.draggable.remove();
                    $('#documentViewer').children(':not(.content)').hide();
                    $('#documentViewer').children('.content').show();
                },
                over: function (event, u) {
                    $recyclebin.css('opacity', '0.3');
                },
                out: function (event, u) {
                    $recyclebin.css('opacity', '0.1');
                }
            });
        },
        stop: function () {
            $recyclebin.remove();
        },
        connectWith: ".connectedSortable",
        dropOnEmpty: true
    });

    //Drag drog on  folder title
    $(".treeview_second").sortable({
        revert: false,
        opacity: 0.5,
        placeholder: "ui-state-highlight",
        update: function (e, ui) {
            updateDocElements($(this));
            moveDefaultBatchToFirst();
        },
        start: function (e, ui) {
            $itemToRemove = null;
            $('body').append($recyclebin);
            $doc = $(this);
            $recyclebin.droppable({
                drop: function (event, u) {
                    $recyclebin.remove();
                    deleteDoc(u.draggable);
                    //u.draggable.remove();
                    updateDocElements();
                    //$('#documentViewer').children(':not(.content)').hide();
                    $('#documentViewer').children('.content').show();
                },
                over: function (event, u) {
                    $recyclebin.css('opacity', '0.3');
                },
                out: function (event, u) {
                    $recyclebin.css('opacity', '0.1');
                }
            });
        },
        stop: function () {
            $recyclebin.remove();
        },
        dropOnEmpty: true
    });

    $("ul.default_batch").disableSelection();
    
    //Annotation array when view loaded
    var annotations = {};
    var rotate = {};
    $.ajax({
        url: URL_GetDocType,
        type: "GET",
        success: function (data) {
            list = data;
            //if View not register context menu on Default batch area
            if (!$('.list_page').length)
                createContextMenu(list);
            createContextMenuPage(list);

            /////For View
            if ($('.list_page').children().length) {
                var pages = [];
                $.each($('.list_page').children(), function () {
                    var p = {
                        KeyCache: $(this).attr('data-id').toString(),
                        FileType: $(this).attr('data-type').toString(),
                        Resolution: $(this).attr('data-dpi').toString()
                    }
                    pages.push(p);
                    var annos = [];
                    var ang = $(this).attr('data-rotate-angle');
                    $.each($('.annotation', this), function () {
                        var anno = {
                            left: parseFloat($(this).attr('data-left')),
                            top: parseFloat($(this).attr('data-top')),
                            width: parseFloat($(this).attr('data-width')),
                            height: parseFloat($(this).attr('data-height')),
                            content: $(this).attr('data-content').toString(),
                            type: $(this).attr('data-type').toLocaleLowerCase(),
                            angle: ang
                        };
                        annos.push(anno);
                    });
                    rotate[$(this).attr('data-id').toString()] = parseFloat($(this).attr('data-rotate-angle'));
                    annotations[$(this).attr('data-id').toString()] = annos;
                });
                docTypeName = $('.list_page').attr('data-doc-type').toString();
                createPage(pages, Options.Import);
            }
        }
    });

    function searchDocType(key){
        rs = $.grep(list, function (item) {
            return item.DocType.Name == key;
        });
        if(rs.length > 0)
            return rs[0];
    }
    
    //Context menu for DEFAULT BATCH
    function createContextMenu(list) {
        //Context Menu cho selector
        var _items = {};
        _items = {
            "scan": { name: RMenuCapture['scan'], iconUrl: URL_ScanIcon },
            "import": {
                name: RMenuCapture['import'], iconUrl: URL_ImportIcon
            },
            "sep": "-----",
            "delete": { name: RMenuCapture['delete'], icon: "none" }
        };
        if (list != undefined && list.length > 0) {
            _items.import.items = {};
            _items.scan.items = {};
            _items.import.items['classify'] = {
                name: RMenuCapture['classifyLater'],
                callback: function (k, o) {
                    callback_ClassifyLater.call(this);
                }
            };
            _items.scan.items['scan-classify'] = {
                name: RMenuCapture['classifyLater'],
                callback: function (k,o) {
                    callback_ClassifyLater.call(this, 'scan');
                }
            };
            _items.import.items.sep1 = '-----';
            _items.scan.items.sep1 = '-----';
            for (i = 0; i < list.length; i++) {
                _items.import.items[list[i].DocType.Name] = {
                    name: list[i].DocType.Name,
                    callback: function (key) {
                        console.log(key);
                        docType = searchDocType(key);
                        if (docType) {
                            insOption = Options.Import;
                            docTypeName = key;
                            $("#formUpload input[name=docTypeId]").val(docType.DocType.Id);
                            $("#filePath").click();
                        } else {

                        }
                    }
                };
                _items.scan.items['scan-' + list[i].DocType.Name] = {
                    name: list[i].DocType.Name,
                    callback: function (key) {
                        key = key.replace("scan-","");
                        docType = searchDocType(key);
                        if (docType) {
                            insOption = Options.Import;
                            docTypeName = key;
                            try {
                                var obj;
                                if ($.browser.chrome)
                                    obj = document.TwainX;
                                else
                                    obj = new ActiveXObject("TwainOcx.WebTwain");
                                if (obj) {
                                    var s = obj.Scan(true, "http://" + document.location.host + "/" + URL_PostImage);
                                    var pages = JSON.parse(s);
                                    var pagePerDoc = parseInt($('.page_per_doc').val());
                                    if (pagePerDoc == 0)
                                        createPage(pages, Options.Import);
                                    else {
                                        for (var i = 0; i < pages.length; i += pagePerDoc) {
                                            if (i + pagePerDoc < pages.length) {
                                                var data = pages.slice(i, i + pagePerDoc);
                                                createPage(data, Options.Import);
                                            } else {
                                                var data = pages.slice(i);
                                                createPage(data, Options.Import);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (e) {
                                console.log(e);
                            }
                        } else {

                        }
                    }
                };
                if (list[i].IconKey) {
                    _items.import.items[list[i].DocType.Name].iconUrl
                        = URL_GetIcon + "/?key=" + list[i].IconKey
                    _items.scan.items['scan-'+list[i].DocType.Name].iconUrl
                        = URL_GetIcon + "/?key=" + list[i].IconKey
                }
            }
            _items.import.style = { height: 30, fontSize: 12, menuWidth: 200 };
            _items.scan.style = _items.import.style;
        }
        console.log(_items);
        $.contextMenu({
            selector: "#openFile",
            items: _items,
            style: { height: 30, fontSize: 12, menuWidth: 200 },
            callback: function (key, options) {
                switch (key) {
                    case 'delete': {
                        $('.default_batch > li').remove();
                        $('.treeview_second > li').remove();
                        $documentViewer.children(':not(.content)').remove();
                        $documentViewer.children('.content').show();
                        break;
                    }
                }
            },
            show: function(opt) {
                console.log("show");
            }
        });
    }
    function callback_ClassifyLater() {
        insOption = Options.ClassifyLater;            
        $("#filePath").click();
    }
    $.contextMenu({
        selector: ".capture_content",
        items: {
            print: { name: RMenuCapture['print'], iconUrl: URL_PrintIcon },
            mail: { name: RMenuCapture['mail'], iconUrl: URL_MailIcon },
            save: { name: RMenuCapture['save'], iconUrl: URL_SaveIcon },
            sep1: "---",
            hide: {name: RMenuCapture['hide'], iconUrl: URL_HideIcon},
            highlight: { name: RMenuCapture['highlight'], iconUrl: URL_HighlightIcon },
            redaction: { name: RMenuCapture['redaction'], iconUrl: URL_RedactionIcon },
            comment: { name: RMenuCapture['note'], iconUrl: URL_NoteIcon },
            sep2: "---",
            rotationleft: { name: RMenuCapture['rotateLeft'], iconUrl: URL_RotateLeftIcon },
            rotationright: { name: RMenuCapture['rotateRight'], iconUrl: URL_RotateRightIcon },
            zoomin: { name: RMenuCapture['zoomIn'], iconUrl: URL_ZoomIn },
            zoomout: { name: RMenuCapture['zoomOut'], iconUrl: URL_ZoomOut },
            fitviewer: { name: RMenuCapture['fitViewer'], iconUrl: URL_FitViewer },
            sep3: "---",
            navigationup: { name: RMenuCapture['previous'], iconUrl: URL_PreviousIcon },
            navigationdown: { name: RMenuCapture['next'], iconUrl: URL_NextIcon },
        },
        style: { height: 30, fontSize: 12, menuWidth: 200 },
        callback: function (key, options) {
            $(".control_" + key).click();
        },
        show: function (opt) {
            console.log("show");
        }
    });
    function createContextMenuPage(list) {
        
        var _itemsPage = {
            'newFromPage': {
                name: RMenuCapture['newDocFromPage']
            },
            'lang': { name: RMenuCapture['contentLang'] },
            'newHere': {
                name: RMenuCapture['newDocHere']
            },
            'sep': '----',
            'delete': { name: RMenuCapture['delete'] },
            'replace': { name: RMenuCapture['replace'] },
            'sep1': '----',
            'insBefore': { name: RMenuCapture['insertBefore'] },
            'insAfter': { name: RMenuCapture['insertAfter'] },
            'rotRight': { name: RMenuCapture['rotateRight'] },
            'rotLeft': { name: RMenuCapture['rotateLeft'] },
        };
        var _itemsPageDefault = {
            'newFromPage': {
                name: RMenuCapture['newDocFromPage']
            },
            'lang': { name: RMenuCapture['contentLang'] },
            'newHere': {
                name: RMenuCapture['newDocHere']
            },
            'sep': '----',
            'delete': { name: RMenuCapture['delete'] },
            'replace': { name: RMenuCapture['replace'] },
            'sep1': '----',
            'insBefore': { name: RMenuCapture['insertBefore'] },
            'insAfter': { name: RMenuCapture['insertAfter'] },
            'rotRight': { name: RMenuCapture['rotateRight'] },
            'rotLeft': { name: RMenuCapture['rotateLeft'] },
        };
        _itemsFolder = {
            'change': { name: RMenuCapture['changeContent'] },
            'lang': { name: RMenuCapture['contentLang'], },
            'sep': '----',
            'delete': { name: RMenuCapture['delete'] },
            'sep1': '----',
            'append': {
                name: RMenuCapture['append'], items: {
                    "apScan": { name: RMenuCapture['scan'], iconUrl: URL_ScanIcon },
                    "apImport": {
                        name: RMenuCapture['import'], iconUrl: URL_ImportIcon
                    }
                }
            },
            'rotRight': { name: RMenuCapture['rotateRight'] },
            'rotLeft': { name: RMenuCapture['rotateLeft'] },
            'sep2': '----',
            'index': { name: RMenuCapture['index'] },
        };
        if (list != undefined && list.length > 0) {
            _itemsPage.newFromPage.items = {};
            _itemsPage.newHere.items = {};
            //_itemsFolder.append.items = {};
            _itemsFolder.change.items = {};
            for (i = 0; i < list.length; i++) {
                //Check icon is not null
                if (list[i].IconKey) {
                    _itemsPage.newHere.items[list[i].DocType.Name + '-new_from_here'] = {
                        name: list[i].DocType.Name,
                        iconUrl: URL_GetIcon + "/?key=" + list[i].IconKey,
                        callback: function (key, options) {
                            insOption = key;
                            var name = key.split('-')[0];
                            _docType = searchDocType(name);
                            classifyMultiPage($(this), _docType);
                        }
                    };
                    _itemsPage.newFromPage.items[list[i].DocType.Name + '-new_here_only'] = {
                        name: list[i].DocType.Name,
                        iconUrl: URL_GetIcon + "/?key=" + list[i].IconKey,
                        callback: function (key, options) {
                            insOption = list[i].DocType.Name;
                            var name = key.split('-')[0];
                            _docType = searchDocType(name);
                            classifySinglePage($(this), _docType);
                        }
                    };
                    //_itemsFolder.append.items[list[i].DocType.Name] = {
                    //    name: list[i].DocType.Name,
                    //    iconUrl: URL_GetIcon + "/?key=" + list[i].IconKey
                    //};
                    _itemsFolder.change.items[list[i].DocType.Name] = {
                        name: list[i].DocType.Name,
                        iconUrl: URL_GetIcon + "/?key=" + list[i].IconKey,
                        callback: changeDocument
                    };
                }
                else {
                    _itemsPage.newHere.items[list[i].DocType.Name + '-new_here_only'] = {
                        name: list[i].DocType.Name,
                        callback: function (key, options) {
                            insOption = key;
                            var name = key.split('-')[0];
                            _docType = searchDocType(name);
                            classifySinglePage($(this), _docType);
                        }
                    };
                    _itemsPage.newFromPage.items[list[i].DocType.Name + '-new_from_here'] = {
                        name: list[i].DocType.Name,
                        callback: function (key, options) {
                            insOption = list[i].DocType.Name;
                            var name = key.split('-')[0];
                            _docType = searchDocType(name);
                            classifyMultiPage($(this), _docType);
                        }
                    };
                    //_itemsFolder.append.items[list[i].DocType.Name] = {
                    //    name: list[i].DocType.Name,
                    //    iconUrl: URL_GetIcon + "/?key=" + list[i].IconKey
                    //};
                    _itemsFolder.change.items[list[i].DocType.Name] = {
                        name: list[i].DocType.Name,
                        iconUrl: URL_GetIcon + "/?key=" + list[i].IconKey,
                        callback: changeDocument
                    };
                }
            }
            _itemsFolder.append.style = _itemsFolder.change.style =
                _itemsPage.newFromPage.style = _itemsPage.newHere.style =
                    { height: 30, fontSize: 12, menuWidth: 200 };
        }
        
        _eventsLangCheck = {
            click: function (e) {
                $('.language').removeAttr('checked');
                $(this).attr('checked', 'checked');
                $selectedPage.find('input[type="hidden"].language').val($(this).attr('class').split(' ')[1]);
                var p = $selectedPage.parentsUntil('.treeview_second').filter('.treeview_second > li');
                var children = p.find('ul.treeview_three > li');
                var last = $(children[0]).find('.language').val();
                $.each(children, function () {
                    if (last != $(this).find('.language').val()) {
                        p.find('.language:first').val('eng');
                        return;
                    } else {
                        p.find('.language:first').val(last);
                    }
                });
                
            }
        };
        //Sub-menu language
        _itemsPage.lang.items = {
            'eng': { name: RMenuCapture['eng'], type: 'checkbox', events: _eventsLangCheck, onclass: 'language eng', selected: true },
            'vie': { name: RMenuCapture['vie'], type: 'checkbox', events: _eventsLangCheck, onclass: 'language vie' }
        };
        _eventsLangCheck = {
            click: function (e) {
                $('.language').removeAttr('checked');
                $(this).attr('checked', 'checked');
                $selectedFolder.find('input[type="hidden"].language').val($(this).attr('class').split(' ')[1]);
                //var p = $selectedPage.parentsUntil('.treeview_second').filter('.treeview_second > li');
                //var children = p.find('ul.treeview_three > li');
                //var last = $(children[0]).find('.language').val();
                //$.each(children, function () {
                //    if (last != $(this).find('.language').val()) {
                //        p.find('.language:first').val('eng');
                //        return;
                //    } else {
                //        p.find('.language:first').val(last);
                //    }
                //});
            }
        };
        _itemsFolder.lang.items = {
            'eng': { name: RMenuCapture['eng'], type: 'checkbox', events: _eventsLangCheck, onclass: 'language eng', selected: true },
            'vie': { name: RMenuCapture['vie'], type: 'checkbox', events: _eventsLangCheck, onclass: 'language vie' }
        };
        _itemsFolder.lang.style = _itemsPage.lang.style = { height: 30, fontSize: 12, menuWidth: 200 };
        //Sub-menu replace
        _itemsPage.replace.items = {
            "reScan": { name: RMenuCapture['scan'], iconUrl: URL_ScanIcon },
            "reImport": {
                name: RMenuCapture['import'], iconUrl: URL_ImportIcon
            }
        };
        
        _itemsPage.replace.style = { height: 30, fontSize: 12, menuWidth: 200 };
        //Sub-menu insert before
        _itemsPage.insBefore.items = {
            "insBefore_Scan": { name: RMenuCapture['scan'], iconUrl: URL_ScanIcon },
            "insBefore_Import": {
                name: RMenuCapture['import'], iconUrl: URL_ImportIcon
            }
        };
        _itemsPage.insBefore.style = { height: 30, fontSize: 12, menuWidth: 200 };
        //Sub-menu insert after
        _itemsPage.insAfter.items = {
            "insAfter_Scan": { name: RMenuCapture['scan'], iconUrl: URL_ScanIcon },
            "insAfter_Import": {
                name: RMenuCapture['import'], iconUrl: URL_ImportIcon
            }
        };
        _itemsPage.insAfter.style = { height: 30, fontSize: 12, menuWidth: 200 };
        //Context Menu for pages in <ul class="default_batch">
        $.contextMenu({
            selector: "ul.default_batch li",
            items: _itemsPage,
            style: {
                height: 30, fontSize: 12, menuWidth: 350
            },
            callback: function (key, options) {
                switch (key) {
                    case 'delete': {
                        deletePage($(this));
                        $documentViewer.children('.content').show();
                        break;
                    }
                    case 'reImport': {
                        $(this).click();
                        $selectedPage = $(this);
                        $selectedFolder = $('ul.default_batch');
                        insOption = Options.Replace.Import;
                        $("#filePath").click();
                        break;
                    }
                    case 'insBefore_Import': {
                        $(this).click();
                        $selectedPage = $(this);
                        $selectedFolder = $('ul.default_batch');
                        insOption = Options.InsertBefore;
                        $("#filePath").click();
                        break;
                    }
                    case 'insAfter_Import': {
                        $(this).click();
                        $selectedPage = $(this);
                        $selectedFolder = $('ul.default_batch');
                        insOption = Options.InsertAfter;
                        $("#filePath").click();
                        break;
                    }
                }
            },
            events: {
                show: function (opt) {
                    var $this = this;
                    $selectedPage = $this;
                    var value = $this.data();
                    if (value['vie'] == value['eng'])
                        $.contextMenu.setInputValues(opt, { key: value.key, 'eng': true, 'vie': false });
                    else
                        $.contextMenu.setInputValues(opt, $this.data());
                },
                hide: function (opt) {
                    var $this = this;
                    $selectedPage = $this;
                    $.contextMenu.getInputValues(opt, $this.data());
                }
            }

        });
        //Context Menu for pages in Classified Document
        _itemsPage.sep2 = '---------';
        delete _itemsPage.newHere;
        _itemsPage.index = { name: RMenuCapture['index'] };
        $.contextMenu({
            selector: "ul.treeview_three li",
            items: _itemsPage,
            style: {
                height: 30, fontSize: 12, menuWidth: 350
            },
            callback: function (key, options) {
                switch (key) {
                    case 'delete': {
                        deletePage($(this));
                        break;
                    }
                    case 'reImport': {
                        $(this).click();
                        $selectedPage = $(this);
                        $selectedFolder = $selectedPage.parentsUntil('.treeview_second').filter('.treeview_second > li');
                        insOption = Options.Replace.Import;
                        $("#filePath").click();
                        break;
                    }
                    case 'insBefore_Import': {
                        $(this).click();
                        $selectedPage = $(this);
                        $selectedFolder = $selectedPage.parentsUntil('.treeview_second').filter('.treeview_second > li');
                        insOption = Options.InsertBefore;
                        $("#filePath").click();
                        break;
                    }
                    case 'insAfter_Import': {
                        $(this).click();
                        $selectedPage = $(this);
                        $selectedFolder = $selectedPage.parentsUntil('.treeview_second').filter('.treeview_second > li');
                        insOption = Options.InsertAfter;
                        $("#filePath").click();
                        break;
                    }
                    case 'index': {
                        $(this).click();
                        $('#index').click();
                        break;
                    }
                    default: {
                        docType = searchDocType(key);
                        if (docType) {
                            insOption = docType.DocType.Name;
                            $("#filePath").click();
                        } else {

                        }
                    }
                }
            },
            events: {
                show: function (opt) {
                    var $this = this;
                    $selectedPage = $this;
                    var value = $this.data();
                    if (value['vie'] == value['eng'])
                        $.contextMenu.setInputValues(opt, { key: value.key, 'eng' : true , 'vie' : false});
                    else
                        $.contextMenu.setInputValues(opt, $this.data());
                },
                hide: function (opt) {
                    var $this = this;
                    $selectedPage = $this;
                }
            }
        });
        //Context Menu for folder title
        $.contextMenu({
            selector: ".folder",
            items: _itemsFolder,
            style: {
                height: 30, fontSize: 12, menuWidth: 350
            },
            callback: function (key, options) {
                switch (key) {
                    case 'delete': {
                        deleteDoc($(this));
                        $documentViewer.children('.content').show();
                        break;
                    }
                    case 'index': {
                        $(this).click();
                        $('#index').click();
                        break;
                    }
                    case 'apImport': {
                        $selectedFolder = $(this).parent();
                        $selectedPage = $selectedFolder.find(".treeview_three > li:last-child");
                        console.log($selectedPage);
                        insOption = Options.InsertAfter;
                        $("#filePath").click();
                        break;
                    }
                    default: {
                        docType = searchDocType(key);
                        if (docType) {
                            insOption = docType.DocType.Name;
                            $("#filePath").click();
                        } else {

                        }
                    }
                }
            },
            events: {
                show: function (opt) {
                    var $this = $(this);
                    $selectedFolder = $this.parent();
                    delete $selectedPage;
                    var value = $this.data();
                    var lang = $selectedFolder.find('.language:first').val();
                    var _eng = lang == "eng";
                    var _vie = lang == "vie";
                    $.contextMenu.setInputValues(opt, { key: value.key, 'eng' : _eng , 'vie' : _vie});
                },
                hide: function (opt) {
                    var $this = $(this);
                    $selectedFolder = $this.parent();
                    delete $selectedPage;
                    $.contextMenu.getInputValues(opt, $this.data());
                }
            }
        });
    }
    multiPage = function (key, options) {
        insOption = key;
        _docType = searchDocType(key);
        classifyMultiPage($(this), _docType);
    };

    //Su kien chon file xong va file co su thay doi,
    $("#filePath").change(function () {
        $('body').ecm_loading_show();
        //Goi ham submit de upload image len server
        options = {
            url: URL_PostImage,
            dataType: "json",
            success: function (data) {
                createPage(data, insOption);
            },
            error: showError,
        };
        $('#formUpload').ajaxSubmit(options);
    });
    $("#filePath").click(function () {
        $(this).val("");
    });

    function showError(jqXHR, textStatus, errorThrown ) {
        alert('Upload fail.');
        console.log(textStatus);
        console.log(errorThrown);
        console.log(jqXHR);
        $('body').ecm_loading_hide();
    }
    var $documentViewer = $("#documentViewer");
    var $docViewerLoading = $("<div id='docViewerTemp'>");
    $docViewerLoading.css({
        visibility: 'hidden',
        left: $('body').offset().left + $('body').width(),
        top: $('body').offset().top + $('body').height(),
        width: $documentViewer.width(),
        height: $documentViewer.height()
    });
    $('body').append($docViewerLoading);

    ///////////////////////////////////////////////
    //items: danh sach cac page
    //$docOnViewer: element de add page
    //$parent: element se add cac thumbnail items
    //[$pos]: element se dc append
    //[opt]: lua chon append 'before' or 'after' or 'replace'
    ///////////////////////////////////////////////
    function createItems(items, $parent, $pos, opt) {
        var itemElements = [];
        var $document = $("<div class='document' data-loading='true'/>");
        //$target is a page to replace or insert aftter or insert before
        var $target;
        //$targetitem is a thumbnail item to replace or insert aftter or insert before
        var $targetItem;
        var $pageToRemove;
        if ($pos) {
            var id = $pos.find('.page').attr('id');
            $target = $('#page_' + id);
            $targetItem = $pos;
            
            $document = $target.parent();
            //remove 'document' cu.document se dc cap nhat lai 
            //va them vao documentViewer sau khi load tat cac cac page
            $target.parent().remove();
            if (opt == 'replace')
                $pageToRemove = $target
        }
        var countLoaded = 0;
        //$docViewerLoading.children('.content').hide();
        $docViewerLoading.append($document);
        $.each(items, function (i) {
            var id = this.KeyCache;
            var $item = createThumbnail(this);
            if ($targetItem) {
                //If insert before,the first item will be insert before the target
                //And the next items will be insert after the first item
                if (opt == 'before' && i == 0) {
                    $item.insertBefore($targetItem);
                } else {
                    $item.insertAfter($targetItem);
                }
                $targetItem = $item;
            } else {
                $parent.append($item);
            }
            $item.find('.treeview_title').ecm_loading_show();
            //$item.find('a > img').load(function () {
            //    $item.find('.treeview_title').ecm_loading_hide();
            //});
            $item.click(pageClick)
            itemElements.push($item);
            ///////////////Create document viewer

            if (this.FileType != "image") {
                var URL_Text = "/View/GetDocument/?key=" + this.KeyCache;
                var pageID = "page_" + this.KeyCache;
                var $page = $("<div id='" + pageID + "'/>");
                $page.css({
                    width: '100%', height: '100%', overflow: 'hidden',
                    //'background-color': 'white'
                });
                $.get(URL_Text, function (data) {
                    $page.append(data);
                    $item.find('.treeview_title').ecm_loading_hide();
                });
                $document.append($page);
                $documentViewer.children().hide();
                $documentViewer.append($document);
                $docViewerLoading.find($document).remove();
            } else {
                var pageID = "page_" + this.KeyCache;
                var $page = $("<div id='" + pageID + "'></div>");
                var imageUrl = URL_LoadImage + "/?key=" + this.KeyCache;
                //var padding = $('<div style="height:10px;width:100%;float:clear;"/>');
                //$document.css('visibility', 'hidden');
                if ($target) {
                    if (opt == 'before' && i == 0)
                        $page.insertBefore($target);
                    else
                        $page.insertAfter($target);
                    //padding.insertAfter($target);
                    $target = $page;
                } else {
                    $document.append($page);
                    //$document.append(padding);
                }
                draws[this.KeyCache] = $page.annotationCapture({ image: imageUrl, width: ($documentViewer.width() - 50)});
                tool.add(this.KeyCache, draws[this.KeyCache]);
                $page.find('img').load(function () {
                    countLoaded++;
                    if (countLoaded == items.length) {
                        //$document.hide();
                        //$document.css('visibility', 'visible');
                        if ($pageToRemove)
                            $pageToRemove.remove();
                        $document.attr('data-loading', false);
                        $documentViewer.children().hide();
                        $documentViewer.append($document);
                        $docViewerLoading.find($document).remove();
                        $("#documentViewer").scrollTo($document.children().first());
                    }
                    $item.find('.treeview_title').ecm_loading_hide();
                    //tool.rotateRight(id, rotate[id]);
                    if (annotations[id])
                        tool.init(id, annotations[id], rotate[id]);
                });
            }
        });
        if (opt == 'replace') {
            $pos.remove();
        }
        return itemElements;
    }
    
    function createThumbnail(item) {
        ////////////Create Thumbnail on left side bar
        var imageUrl = URL_LoadImage + "/?key=" + item.KeyCache + "&thumb=true";
        var dpi = "";
        if (item.FileType == "image")
            dpi = item.Resolution + " dpi";
        //Sau khi submit thanh cong server tra ve danh sach cac key,tuong ung voi so trang cua image
        //Hien thi thumbnail cac trang tren left menu
        var $item = $('<li><input type="hidden" class="language" value="eng"/>'
                + '<span class="page treeview_title" id="' + item.KeyCache + '">'
                + '<a href="#"><img src="' + imageUrl + '" /><span>'
                + '<strong class="pageNumber">' + (i + 1) + '</strong>'
                + '<span> ' + dpi + '</span>'
                + '</span></a></span></li>');
        $viewer = $('<input type="hidden" class="viewer" value="' + item.FileType + '"/>');
        $item.append($viewer);
        $item.data('key', item.KeyCache);
        return $item;
    }

    function updatePageNotClassify(items, $pos ,opt) {        
        //$target is a page to replace or insert aftter or insert before
        var $target;
        //$targetitem is a thumbnail item to replace or insert aftter or insert before
        var $targetItem;
        var $pageToRemove;
        if ($pos) {
            var id = $pos.find('.page').attr('id');
            $target = $('#page_' + id);
            $targetItem = $pos;
            $target.parent().remove();
            if (opt == 'replace')
                $pageToRemove = $target;
        }
        //$docViewerLoading.children('.content').hide();
        $.each(items, function (i) {
            var $document = $("<div class='document' data-loading='true'/>");
            $docViewerLoading.append($document);
            var $item = createThumbnail(this);

            if ($targetItem) {
                //If insert before,the first item will be insert before the target
                //And the next items will be insert after the first item
                if (opt == 'before' && i == 0) {
                    $item.insertBefore($targetItem);
                } else {
                    $item.insertAfter($targetItem);
                }
                $targetItem = $item;
            } else {
                $parent.append($item);
            }

            $item.find('.treeview_title').ecm_loading_show();
            //$item.find('a > img').load(function () {
            //    $item.find('.treeview_title').ecm_loading_hide();
            //});
            $item.click(pageClick)
            ///////////////Create document viewer

            if (this.FileType != "image") {
                var URL_Text = "/View/GetDocument/?key=" + this.KeyCache;
                var pageID = "page_" + this.KeyCache;
                var $page = $("<div id='" + pageID + "'/>");
                $page.css({
                    width: '100%', height: '100%', overflow: 'hidden',
                    //'background-color': 'white'
                });
                $.get(URL_Text, function (data) {
                    $page.append(data);
                    $item.find('.treeview_title').ecm_loading_hide();
                });
                $document.append($page);
                $documentViewer.children().hide();
                $documentViewer.append($document);
                $docViewerLoading.find($document).remove();
            } else {
                var pageID = "page_" + this.KeyCache;
                var $page = $("<div id='" + pageID + "'></div>");
                var imageUrl = URL_LoadImage + "/?key=" + this.KeyCache;
                $document.append($page);
                draws[this.KeyCache] = $page.annotationCapture({ image: imageUrl });
                tool.add(this.KeyCache, draws[this.KeyCache]);
                $page.find('img').load(function () {
                    if ($pageToRemove)
                        $pageToRemove.remove();
                    $documentViewer.children().hide();
                    $documentViewer.append($document);
                    $docViewerLoading.find($document).remove();
                    $item.find('.treeview_title').ecm_loading_hide();
                });
            }
            
        });
        if (opt == 'replace') {
            $pos.remove();
        }
    }
    
    function createPage(data, insOption) {
        var set,
        $parentItem;
        doctype = searchDocType(docTypeName);
        var $doc;
        var $parentItem;

        switch (insOption) {
            case Options.ClassifyLater: {
                //Check selector is selected
                if ($('.default_batch').length == 0) {
                    $classifyLaterFolder = $('<ul class="default_batch connectedSortable"></ul');
                    $thumbnail.append($classifyLaterFolder);
                }
                $parentItem = $classifyLaterFolder;
                $.each(data, function () {
                    var _data = [];
                    _data.push(this);
                    createItems(_data, $parentItem);
                });
                
                //$.each(itemElements, function () {
                //    $parentItem.append(this);
                //});
                updatePageElements($parentItem);
                break;
            }
            case Options.Replace.Import: {
                $doc = $selectedFolder;
                $parentItem = $doc.find('ul.treeview_three').length ? $doc.find('ul.treeview_three') : $doc;
                //var oldItems = $parentItem.children('li');
                //var topItems = oldItems.filter(function (index) {
                //    return index < $selectedPage.index();
                //});
                //var bottomItems = oldItems.filter(function (index) {
                //    return index > $selectedPage.index();
                //});
                //var insertItems = createItems(data);
                //$parentItem.empty();
                //$.each(topItems, function () {
                //    $parentItem.append($(this).click(pageClick));
                //});
                //$.each(insertItems, function () {
                //    $parentItem.append($(this).click(pageClick));
                //});
                //$.each(bottomItems, function () {
                //    $parentItem.append($(this).click(pageClick));
                //});
                //var $docOnViewer = $documentViewer.children(':visible');
                if ($parentItem.hasClass('default_batch')) {
                    updatePageNotClassify(data, $selectedPage, 'replace');
                } else
                    createItems(data, $parentItem, $selectedPage, 'replace');
                updatePageElements($parentItem);
                break;
            }
            case Options.InsertBefore: {
                $doc = $selectedFolder;
                $parentItem = $doc.find('ul.treeview_three').length ? $doc.find('ul.treeview_three') : $doc;
                //var oldItems = $parentItem.children('li');
                //var topItems = oldItems.filter(function (index) {
                //    return index < $selectedPage.index();
                //});
                //var bottomItems = oldItems.filter(function (index) {
                //    return index > $selectedPage.index();
                //});
                //var insertItems = createItems(data);
                //$parentItem.empty();
                //$.each(topItems, function () {
                //    $parentItem.append($(this).click(pageClick));
                //});
                //$.each(insertItems, function () {
                //    $parentItem.append($(this).click(pageClick));
                //});
                //$parentItem.append($selectedPage.click(pageClick));
                //$.each(bottomItems, function () {
                //    $parentItem.append($(this).click(pageClick));
                //});
                if ($parentItem.hasClass('default_batch')) {
                    updatePageNotClassify(data, $selectedPage, 'before');
                } else
                    createItems(data, $parentItem, $selectedPage, 'before');
                updatePageElements($parentItem);
                break;
            }
            case Options.InsertAfter: {
                $doc = $selectedFolder;
                $parentItem = $doc.find('ul.treeview_three').length ? $doc.find('ul.treeview_three') : $doc;
                //var oldItems = $parentItem.children('li');
                //var topItems = oldItems.filter(function (index) {
                //    return index < $selectedPage.index();
                //});
                //var bottomItems = oldItems.filter(function (index) {
                //    return index > $selectedPage.index();
                //});
                //var insertItems = createItems(data);
                //$parentItem.empty();
                //$.each(topItems, function () {
                //    $parentItem.append($(this).click(pageClick));
                //});
                //$parentItem.append($selectedPage.click(pageClick));
                //$.each(insertItems, function () {
                //    $parentItem.append($(this).click(pageClick));
                //});

                //$.each(bottomItems, function () {
                //    $parentItem.append($(this).click(pageClick));
                //});
                if ($parentItem.hasClass('default_batch')) {
                    updatePageNotClassify(data, $selectedPage, 'after');
                } else
                    createItems(data, $parentItem, $selectedPage, 'after');
                updatePageElements($parentItem);
                break;
            }
            default: {
                if (data && data.length) {
                    $doc = createNewDocElement(data.length, doctype);
                    //loading will remove after img img in documentViewer load
                    //$thumbnail.ecm_loading_show();
                    $parentItem = $doc.find('ul.treeview_three');
                    $thumbnail.append($doc);
                    //$doc.find('span.folder').ecm_loading_show();
                    //$parentItem.ecm_loading_show();
                    createItems(data, $parentItem);
                    //$.each(itemElements, function () {
                    //    $parentItem.append($(this));
                    //});
                    performIndex();
                    updatePageElements($parentItem);
                    //hide loading for show in createPage
                    //$thumbnail.ecm_loading_hide();
                } else {
                    $.messageBox({
                        message: 'Can not convert file to view'
                    });
                }
                break;
            }
        }
        $('body').ecm_loading_hide();
    }

    //params: json array
    //tao document tu json array tra ve tu server
    //function createDocument(items) {
    //    $.each(items, function (i) {
    //        var $doc = $("<div class='document'/>");
    //        var imageUrl = URL_LoadImage + "/?key=" + this.KeyCache + "&thumb=true";
    //        var dpi = "";
    //        if (this.FileType == "image")
    //            dpi = this.Resolution + " dpi";
    //        //Sau khi submit thanh cong server tra ve danh sach cac key,tuong ung voi so trang cua image
    //        //Hien thi thumbnail cac trang tren left menu
    //        var $item = $('<li><input type="hidden" class="language" value="eng"/><span class="page treeview_title" id="' + this.KeyCache + '">'
    //                + '<a href="#"><img src="' + imageUrl + '" /><span>'
    //                + '<strong class="pageNumber">' + (i + 1) + '</strong>'
    //                + '<span> ' + dpi + '</span>'
    //                + '</span></a></span></li>');
    //        $viewer = $('<input type="hidden" class="viewer" value="' + this.FileType + '"/>');
    //        $item.append($viewer);
    //        $item.data('key', this.KeyCache);
    //        $item.find('img').load(function () {
    //            createViewer.call($item.click(pageClick));
    //        });
    //        itemElements.push($item);
    //    });
    //    return itemElements;
    //}
    var draws = {};
    var currentPageId;
    var previousPageId;
    ///
    function pageClick(e) {
        currentPageId = $(this).find($('.page')).attr('id');
        //ready create documentvier
        var currentPage = $("#documentViewer").find('#page_' + currentPageId);
        //Visible = hidden ==> Page haven't finished loading
        if (loading(currentPageId))
            return false;

        if (currentPage.length > 0) {
            $("#documentViewer").children().hide();
            currentPage.parent().show();
            $("#documentViewer").scrollTo(currentPage);
        }

        ///////////////////set treeview_select class
        if (!e.ctrlKey) {
            $('.treeview_select').removeClass('treeview_select');
        }
        if (!$(this).children('.treeview_title').hasClass('treeview_select')) {
            $(this).children('.treeview_title').addClass('treeview_select');
        }
        else {
            $(this).children('.treeview_title').removeClass('treeview_select');
        }
        /////////////////////////////////////////////////////

        return false;
    }

    //Check document full loaded
    //id of page 
    function loading(id) {
        if ($("#"+id).parent().find('.viewer') == 'image' && 
            $docViewerLoading.find('#page_' + currentPageId))
            return true;
        return false;
    }

    $(document).on('click', '.treeview_three > li', pageClick);

    $('#submit').click(function () {
        if ($('.default_batch > li').length > 0) {
            alert('Please Classify Document before submit!');
            return;
        }
        data = { Documents: [] };
        var docElements = $('.treeview_second > li');
        for (i = 0; i < docElements.length; i++) {
            var id = $(docElements[i]).children('.docTypeId').val();
            var pageElements = $(docElements[i]).find('.treeview_three > li');
            var fieldValueElements = $(docElements[i]).find('div.fieldValues > input[type=hidden]');
            
            data.Documents[i] = { DocumentTypeId: id, Pages: [], FieldValues: [] };
            for (j = 0; j < pageElements.length; j++) {
                var _ImgKey = $(pageElements[j]).children('span.treeview_title').attr('id');
                var _LangCode = $(pageElements[j]).find('.language').val();

                var _annotations = {};
                var angle = 0;
                if(draws[_ImgKey] != undefined){
                    _annotations = draws[_ImgKey].getAnnotations();
                    angle = draws[_ImgKey].rotateAngle();
                }
                data.Documents[i].Pages[j] = {
                    ImgKey: _ImgKey, Annotations: _annotations, LanguageCode: _LangCode,
                    PageWidth: $("#page_" + _ImgKey).width(), PageHeight: $("#page_" + _ImgKey).height(),
                    RotateAngle: angle*-1
                };
            }
            for (k = 0; k < fieldValueElements.length; k++) {
                var _Id = $(fieldValueElements[k]).attr('class');
                var _Value = $(fieldValueElements[k]).val();
                if (_Id && _Value)
                    data.Documents[i].FieldValues[k] = { Id: _Id, Value: _Value };
                else {
                    alert("Please fill in all the required fields");
                    return;
                }
            }
        }

        $.ajax({
            url: URL_Insert,
            async: false,
            type: "POST",
            data: JSON.stringify(data),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
            },
            success: function (data, textStatus, jqXHR) {
                alert(data.Message);
                if (data.Code == 1) {
                    docElements.remove();
                    $(".current_content_fields").children().remove();
                    $(".thumbnails").click();
                    $(".button_navigation").children().prop("disable", true);
                    $("#documentViewer").children(":not(.content)").remove();
                    $("#documentViewer").children('.content').show();
                }
            }
        });
        
        return false;
    });

    function selection(e) {
        if (!e.ctrlKey) {
            $('.treeview_select').removeClass('treeview_select');
            if ($(this).hasClass('treeview_select')) {
                $(this).children('.treeview_title').removeClass('treeview_select');
            }
        }
        if (!$(this).hasClass('treeview_select')) {
            $(this).children('.treeview_title').addClass('treeview_select');
        }
        return false;
    }

    function deletePage($page) {
        var id = $page.children('.page').attr('id');
        $page.remove();
        $pageView = $('#page_' + id);
        $docView = $pageView.parent();
        $pageView.remove();
        $docElement = $page.parent();
        if (!$docView.find('div[id^="page"]').length)
            $docView.remove();
        updatePageElements($docElement);
    }

    //$doc: element to delete, the element can be a title of document or a root tag (<li>) of document
    function deleteDoc($doc) {
        var id_first = $doc.find('.page').attr('id') || $doc.parentsUntil('.treeview_second').find('.page').attr('id');
        $('#page_' + id_first).parent().remove();
        $doc.parentsUntil('.treeview_second').remove();
        $doc.remove();
        updateDocElements();
    }
    /* Hide or show children when click treeview_icon in default_batch */
    $('.treeview_first .treeview_icon').click(function () {
        if (!$('.treeview_second').is(':hidden')) {
            $('.treeview_second').slideUp(100);
            $(this).parent().removeClass('treeview_open');
        }
        else {
            $('.thumbnail').slideDown(100);
            $(this).parent().addClass('treeview_open');
        }
    });
    
    /* Function create new Document */
    function createNewDocElement(pageCounter, doctype) {
        var $wrap = $('<li></li>');
        var $id = $('<input type="hidden" class="docTypeId" value="' + doctype.DocType.Id + '"/>');
        var $lang = $('<input type="hidden" class="language" value="eng"/>');
        $wrap.append($id.add($lang));
        var $fieldValues = $("<div class='fieldValues'></div>");
        $.each(doctype.DocType.Fields, function () {
            if (this.IsSystemField == false)
                $fieldValues.append('<input type="hidden" class="' + this.Id + '" value=""/>');
        });
        $wrap.append($fieldValues);
        var $treeview_icon = $('<span class="treeview_icon glyphicon glyphicon-play"></span>');
        $folderName = $('<span class="folder treeview_title treeview_open">' +
                            '<a href="#">' +
                                '<img src="' + URL_GetIcon + "/?key=" + doctype.IconKey + '" />' +
                                '<span>' +
                                    '<strong><span class="docNumber">' + $('.treeview_second').children().length +
                                    '</span><span class="docTypeName">. ' + doctype.DocType.Name + '</span></strong>' +
                                    '<span class="pageCounter">Pages: ' + pageCounter + '</span>' +
                                '</span>' +
                            '</a>' +
                        '</span>');
        var $folder = $('<ul class="treeview_three connectedSortable"></ul>')
            .css({ 'min-height': '2px' });
        var removeIntent = false;
            
        $folder.sortable({
            revert: true,
            opacity: 0.5,
            placeholder: "ui-state-highlight",
            receive: function (e, ui) {
                //ID cua page nhan dc
                var id = ui.item.find('.page').attr('id');
                var $page = $("#page_" + id);
                //Tim document se them page nay vao
                //$(this) trong context nay la <ul> chua cac thumbnail <li>
                //Tim bat cu id nao trong !
                var id_p = $(this).find('.page').filter(':not(#' + id + ')').attr('id');
                var p = $("#page_" + id_p).parent();
                var i = ui.item.index();
                if (i == 0)
                    $page.insertBefore(p.children().first());
                else {
                    var target = p.children(':nth-child(' + i + ')');
                    $page.insertAfter(target);
                }
                $documentViewer.children().hide();
                p.show();
                $documentViewer.scrollTo($page);
            },
            update: function (e, ui) {
                updatePageElements($(this));
                var id = ui.item.find('.page').attr('id');
                var docView = $('#page_' + id).parent();
                var pageView = $("#page_" + id);
                var i = ui.item.index();
                if (i == 0)
                    docView.prepend(pageView);
                else {
                    var prevId = ui.item.prev().find('.page').attr('id');
                    var target = $("#page_" + prevId);
                    pageView.insertAfter(target);
                }
                $documentViewer.scrollTo(pageView);
                $fieldValues.removeData('cacheHTML');
                console.log($fieldValues.data('cacheHTML'));
            },
            start: function (e, ui) {
                $itemToRemove = null;
                $('body').append($recyclebin);
                $recyclebin.droppable({
                    drop: function (event, u) {
                        $recyclebin.remove();
                        $folder.sortable("option", "revert", false);
                        deletePage(u.draggable);
                        $folder.sortable("option", "revert", true);
                    },
                    over: function (event, u) {
                        $recyclebin.css('opacity', '0.3');
                    },
                    out: function (event, u) {
                        $recyclebin.css('opacity', '0.1');
                    }
                });
            },
            stop: function () {
                $recyclebin.remove();
            },
            connectWith: ".connectedSortable",
            dropOnEmpty: true
        }).disableSelection();

        $('ul.default_batch').sortable("option", "connectWith", ".connectedSortable");
        /* Hide or show Page when click treeview_icon in Document folder */
        $treeview_icon.click(function () {
            $f = $(this).parentsUntil('.treeview_second').children('.treeview_three');
            $fName = $(this).parent();
            if (!$f.is(':hidden')) {
                $f.slideUp(100);
                $fName.removeClass('treeview_open');
            }
            else {
                $f.slideDown(100);
                $fName.addClass('treeview_open');
            }
        });
        $wrap.append($folderName.prepend($treeview_icon)).append($folder);
        $wrap.click(selection);
        //$(".thumbnail").append($wrap);
        $wrap.click();
        return $wrap;
    }


    /* Function update Page number, Document number, number of Pages in a Document*/
    function updateDocElements() {
        var docElements = $('.treeview_second > li');
        for (i = 0; i < docElements.length; i++) {
            $(docElements[i]).find('.docNumber').text((i + 1));
        }
    }

    /* Update a page number in a Document element
     * @param a selector of html element, 
     *      Document consist of document title, document number, document type id (hidden), 
                page counter, page number, page id (image key)
     * @return no
     */
    function updatePageElements(docElement) {
        pages = docElement.find('.pageNumber');
        if (pages.length == 0) {
            if (!docElement.hasClass('default_batch')) {
                docElement.parentsUntil('.treeview_second').remove();
                updateDocElements();
            }
            return;
        }
        docElement.parentsUntil('.treeview_second').find('.pageCounter').text('Pages: ' + pages.length);
        //update page number
        for (i = 0; i < pages.length; i++) {
            $(pages[i]).text(i + 1);
        }
    }

    /* Function create classify a page, 
     * Name on context menu is: New document from selected page 
     */
    function classifySinglePage($itemSelector, doctype) {
        var $newDocument = createNewDocElement(1, doctype);
        var $oldDocument = $itemSelector.parent();
        $itemSelector.appendTo($newDocument.find('ul.treeview_three'));
        $thumbnail.append($newDocument);
        updatePageElements($oldDocument);
        updatePageElements($newDocument);
    }

    /*Function classify multidocument*/
    function classifyMultiPage($beginPage, doctype) {
        var begin = $beginPage.index();
        var $oldDocument = $beginPage.parent();
        ////count = (len of element) - (begin + 1) + 1
        var count = $('li', $oldDocument).length - begin;
        var $newDocument = createNewDocElement(count, doctype);
        $('li', $oldDocument).filter(function (index) {
            return index >= begin;
        }).appendTo($newDocument.find('ul.treeview_three'));
        $thumbnail.append($newDocument);
        updatePageElements($oldDocument);
        updatePageElements($newDocument);
    }

    function performIndex() {
        $('.capture_thumbnails').ecm_loading_show();
        var doc = { DocumentTypeId: 1, Pages: [] };
        var $docElement = $(".treeview_select").parentsUntil(".treeview_second").filter(":last");
        doc.DocumentTypeId = $docElement.find('.docTypeId').val();
        if (doc.DocumentTypeId !== undefined) {
            var $pageElement = $docElement.find($('ul.treeview_three > li'));
            $.each($pageElement, function () {
                var imgKey = $(this).find(".treeview_title").attr("id");
                var _LangCode = $(this).find('.language').val();
                Page = { ImgKey: imgKey, Annotations: [], LanguageCode: _LangCode };
                doc.Pages.push(Page);
            });
            var fieldValues = $docElement.find('div.fieldValues').children();
            var fieldOCRs;
            if (fieldValues.parent().data('cacheHTML') == undefined) {
                $.ajax({
                    url: URL_OCR,
                    type: "POST",
                    data: JSON.stringify(doc),
                    //async: false,
                    contentType: "application/json",
                    dataType: "json",
                    converters: {
                        'text json': true
                    },
                    error: function (e) {
                        console.log(e);
                    },
                    success: function (data) {
                        $(".current_content_fields").html(data);

                        fieldOCRs = $(".current_content_fields").find('.content_fields_input > input[type=text]');
                        $.each(fieldOCRs, function (i) {
                            $(fieldValues[i]).val($(this).val());
                            console.log($(fieldOCRs[i]).val());
                            if ($(this).val()) {
                                $(fieldOCRs[i]).parent().addClass('hasvalue');
                            } else {
                                $(fieldOCRs[i]).parent().removeClass('hasvalue');
                            }
                            $(this).change(function () {
                                $(fieldValues[i]).val($(this).val());
                            });
                        });
                        fieldValues.parent().data('cacheHTML', $(".current_content_fields").html());
                        $('.capture_thumbnails').ecm_loading_hide();
                    },
                    cache: false,
                    processData: false
                });
            } else {
                $(".current_content_fields").html(fieldValues.parent().data('cacheHTML'));
                fieldOCRs = $(".current_content_fields").find('.content_fields_input > input[type=text]');
                $.each(fieldValues, function (i) {
                    $(fieldOCRs[i]).val($(this).val());
                    console.log($(fieldOCRs[i]).val());
                    if ($(this).val()) {
                        $(fieldOCRs[i]).parent().addClass('hasvalue');
                    } else {
                        $(fieldOCRs[i]).parent().removeClass('hasvalue');
                    }
                    $(fieldOCRs[i]).change(function () {
                        $(fieldValues[i]).val($(this).val());
                    });
                });
                $('.capture_thumbnails').ecm_loading_hide();
            }
            $(document).on('click', '.content_fields_input', function () {
                if (tool) {
                    var pos = {
                        left: parseFloat($(".left", this).val()),
                        top: parseFloat($(".top", this).val()),
                        width: parseFloat($(".width", this).val()),
                        height: parseFloat($(".height", this).val())
                    }
                    var index = $(".pageIndex", this).val();
                    tool.showOCRZone(index, pos);
                }

            });
            $('.date').datepicker().on('changeDate', function () {
                $(this).change();
                $(this).parent().addClass('hasvalue');
            });
            var title = $(".treeview_select").parentsUntil(".treeview_second").filter(':last')
                            .find(".treeview_title > a > span > strong").first().html();
            var count = " ( " + ($(".treeview_second").children().length - 1) + " doc)";
            $(".current_content_header").html("<strong>" + title + "</strong>" + count);
        }
    }
    $('#index').click(function () {
        if (!$('.treeview_three').length || $('.capture_thumbnails').find('.ecm_loading_bg').length > 0)
            return false;
        performIndex.call(this);
    });

    //nhựt fix
    
    function moveDefaultBatchToFirst() {
        var def = $('ul.default_batch');
        var p = def.parent();
        if (def.index() != 0) {
            def.insertBefore(p.children().first());            
        }
    }

    function changeDocument (key, options) {
        console.log($(this));
        var oldDocument = $(this).parent();
        var _docType = searchDocType(key);
        var pageElements = oldDocument.find('ul.treeview_three');
        var newDocElement = createNewDocElement(pageElements.length, _docType);
        newDocElement.find('ul.treeview_three').replaceWith(pageElements);
        oldDocument.replaceWith(newDocElement);
        updateDocElements();
    }

    $('.navigation_back').click(function (e) {
        var $prev = $(".treeview_select").parentsUntil(".treeview_second").filter(":last").prev();
        //var id = $(".treeview_select").attr('id') || $(".treeview_select").next().find('.treeview_select').attr('id');
        if ($prev.length > 0 && $prev.index() > 0) {
            $('.navigation_next').prop('disabled', false);
            //if (loading($prev.find(id)))
            //    return false;
            $prev.find('li:first').click();
            performIndex();
            if ($(".treeview_select").parentsUntil(".treeview_second").filter(":last").prev().index() == 0)
                $(this).prop('disabled', true);
        }
    });
    $('.navigation_next').click(function (e) {
        var $next = $(".treeview_select").parentsUntil(".treeview_second").filter(":last").next();
        if ($next.length > 0) {
            $('.navigation_back').prop('disabled', false);
            $next.find('li:first').click();
            performIndex();
            if ($(".treeview_select").parentsUntil(".treeview_second").filter(":last").next().length == 0)
                $(this).prop('disabled', true);
        }
    });
    
    $('.control_zoomin').click(function () {
        //$('.select').removeClass('select');
        $(this).addClass('select');
        //draw.zoomIn();
        tool.zoomIn();
        $(this).removeClass('select')
        return false;
    });
    $('.control_resetzoom.fit_width').click(function () {
        $(this).addClass('select');
        tool.fitWidth();
        $(this).removeClass('select')
        return false;
    });
    $('.control_resetzoom.fit_height').click(function () {
        $(this).addClass('select');
        tool.fitHeight();
        $(this).removeClass('select')
        return false;
    });
    $('.control_pan').click(function () {
        $('.select').removeClass('select');
        $(this).addClass('select');
        tool.pan();
        //draw.pan();
        //draw.scrollable();
    });
    $('.control_hide').click(function () {
        //draw.toggleHidden();
        tool.hide();
    });
    $('.control_highlight').click(function () {
        $('.select').removeClass('select');
        $(this).addClass('select');        
        tool.highlight();
    });
    $('.control_redaction').click(function () {
        $('.select').removeClass('select');
        $(this).addClass('select');
        //draw.annoRedaction();
        tool.redaction();
    });
    $('.control_comment').click(function () {
        $('.select').removeClass('select');
        $(this).addClass('select');
        //draw.annoText();
        tool.text();
    });    
    $('.control_zoomout').click(function () {
        //$('.select').removeClass('select');
        //$(this).addClass('select');
        tool.zoomOut();
        return false;
    });
    $('.leftbar_lable').click(function () {
        if (tool)
            tool.removeOCRZone();
    });
    $('.control_print').click(function () {
        console.log($(".capture_content"));
        console.log(_linkCSS);
        $(".capture_content").printThis({ debug: true, loadCSS: _linkCSS });
    });
    $('.control_rotationleft').click(function () {
        //var src = draw.rotateCounterClockwise();
        //$('.treeview_select').find('img').attr('src', src);
        tool.rotateLeft();
    });
    $('.control_rotationright').click(function () {
        //var src = draw.rotateClockwise();
        //$('.treeview_select').find('img').attr('src', src);
        tool.rotateRight();
    });
    $(".control_save").click(function () {
        var $select = $('.treeview_select');
        //if class treeview_select is Page, docElement = <li>..</li>
        var $docElement = $select.parentsUntil('.treeview_second', ':last');
        //if class treeview_select is Folder, docElement = first child <li> of Folder
        //if ($select.hasClass('folder'))
        //    $docElement = $select.parent().children("li").first();

        var id = $docElement.children('.docTypeId').val();
        var pageElements = $docElement.find('.treeview_three > li');
        var fieldValueElements = $docElement.find('div.fieldValues > input[type=hidden]');

        var Document = { DocumentTypeId: id, Pages: [], FieldValues: [] };
        for (j = 0; j < pageElements.length; j++) {
            var _ImgKey = $(pageElements[j]).children('span.treeview_title').attr('id');
            var _LangCode = $(pageElements[j]).find('.language').val();

            var _annotations = {};
            var angle = 0;
            if (draws[_ImgKey] != undefined) {
                _annotations = draws[_ImgKey].getAnnotations();
                angle = draws[_ImgKey].rotateAngle();
            }
            Document.Pages[j] = {
                ImgKey: _ImgKey, Annotations: _annotations, LanguageCode: _LangCode,
                PageWidth: $("#page_" + _ImgKey).width(), PageHeight: $("#page_" + _ImgKey).height(),
                RotateAngle: angle * -1
            };
        }
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
            OK_Click: function (data) {
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
                $.ajax({
                    url: URL_SaveLocal,
                    type: "POST",
                    data: JSON.stringify(mail),
                    error: function () {
                        alert("ERROR");
                    },
                    success: function (data) {
                        //var $p = $('<iframe src="/Capture/Get?key=' + data + '"></iframe>');
                        //$("#printDiv").append($p);
                        window.open(URL_Get + "?key=" + data, "_blank");
                    },
                    contentType: "application/json"
                });
            }
        });
    });
    $('.control_mail').click(function () {
        var $select = $('.treeview_select');
        //if class treeview_select is Page, docElement = <li>..</li>
        var $docElement = $select.parentsUntil('.treeview_second', ':last');

        var id = $docElement.children('.docTypeId').val();
        var pageElements = $docElement.find('.treeview_three > li');
        var fieldValueElements = $docElement.find('div.fieldValues > input[type=hidden]');

        var Document = { DocumentTypeId: id, Pages: [], FieldValues: [] };
        for (j = 0; j < pageElements.length; j++) {
            var _ImgKey = $(pageElements[j]).children('span.treeview_title').attr('id');
            var _LangCode = $(pageElements[j]).find('.language').val();

            var _annotations = {};
            var angle = 0;
            if (draws[_ImgKey] != undefined) {
                _annotations = draws[_ImgKey].getAnnotations();
                angle = draws[_ImgKey].rotateAngle();
            }
            Document.Pages[j] = {
                ImgKey: _ImgKey, Annotations: _annotations, LanguageCode: _LangCode,
                PageWidth: $("#page_" + _ImgKey).width(), PageHeight: $("#page_" + _ImgKey).height(),
                RotateAngle: angle * -1
            };
        }
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
        cc.hide();
        mailTo.append(cc);
        mailTo.children('a').click(function () {
            if (cc.css('display') == "none")
                cc.show();
            else
                cc.hide();
            return false;
        });
        attach.add(mailBy).add(mailTo).css({ 'font-size': '15px', 'font-weight': 'bold' });
        attach.add(mailBy).add(mailTo).children('span,label').css({ 'font-size': '14px', 'font-weight': 'normal' });
        formData.append(attach).append(mailBy).append(mailTo);
        $.messageBox({
            title: 'Input mail address',
            type: 'form',
            message: null,
            formData: formData,
            width: "500px",
            OK_Click: function (data) {
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
                $.ajax({
                    url: URL_SendMail,
                    type: "POST",
                    data: JSON.stringify(mail),
                    error: function () {
                        alert("ERROR");
                    },
                    success: function (data) {
                        if (data == true)
                            $.messageBox({ type: 'notify', message: 'Your email has been send' });
                        else
                            $.messageBox({ type: 'notify', message: 'Fail to send your email!' });
                    },
                    contentType: "application/json"
                });
            }
        });

    });
    $('.control_navigationdown').click(function () {
        var $next;
        if ($('.treeview_select').hasClass('page'))
            $next = $('.treeview_select').parent().next();
        else
            $next = $('.treeview_select').parent().find('.page').eq(1).parent();
        $next.click();
    });
    $('.control_navigationup').click(function () {
        var $prev;
        if ($('.treeview_select').hasClass('page'))
            $prev = $('.treeview_select').parent().prev();
        $prev.click();
    });
    //param: id of thumbnail
    function relatives(id) {
        var p = $('#' + id).parent().parent();
        var rs = [];
        p.find('li > .treeview_title').each(function () {
            rs.push($(this).attr('id').toString());
        });
        return rs;
    }
    //manage annotationCapture instances
    function Toolbar() {
        var docs = {};
        var opt = { high: 1, text: 2, redaction: 3 };
        var current;
        this.add = function (id, d) {
            docs[id] = d;
            try {
                switch (current) {
                    case opt.high:
                        d.annoHighlight();
                        break;
                    case opt.redaction:
                        d.annoRedaction();
                        break;
                    case opt.text:
                        d.annoText();
                        break;
                }
            } catch (e) {
                console.log(e);
            }
        }
        this.highlight = function () {
            current = opt.high;
            $.each(docs, function () {
                try {
                    this.annoHighlight();
                } catch (e) {
                    console.log(e);
                }
            });
        }
        this.redaction = function () {
            current = opt.redaction;
            $.each(docs, function () {
                try {
                    this.annoRedaction();
                } catch (e) {
                    console.log(e);
                }
            });
        }
        this.text = function () {
            current = opt.text;
            $.each(docs, function () {
                try {
                    this.annoText();
                } catch (e) {
                    console.log(e);
                }
            });
        }
        this.zoomIn = function () {
            var id = currentPageId();//$('.treeview_select').attr('id').toString();
            var r = relatives(id);
            if (r.length > 0) {
                $.each(r, function () {
                    docs[this].zoomIn();
                });
            }
        }
        this.zoomOut = function () {
            var id = currentPageId();//$('.treeview_select').attr('id').toString();
            var r = relatives(id);
            if (r.length > 0) {
                $.each(r, function () {
                    docs[this].zoomOut();
                });
            }
        }
        this.fitWidth = function () {
            var id = currentPageId();//$('.treeview_select').attr('id').toString();
            var r = relatives(id);
            if (r.length > 0) {
                $.each(r, function () {
                    docs[this].fit($documentViewer.width());
                });
            }
        }
        this.fitHeight = function () {
            var id = currentPageId();//$('.treeview_select').attr('id').toString();
            var r = relatives(id);
            if (r.length > 0) {
                $.each(r, function () {
                    docs[this].fitH($documentViewer.height());
                });
            }
        }
        this.hide = function () {
            var id = currentPageId();//$('.treeview_select').attr('id').toString();
            var r = relatives(id);
            if (r.length > 0) {
                $.each(r, function () {
                    docs[this].toggleHidden();
                });
            }
        }
        this.pan = function () {
            var id = currentPageId();//$('.treeview_select').attr('id').toString();
            var r = relatives(id);
            if (r.length > 0) {
                $.each(r, function () {
                    docs[this].scrollable($("#documentViewer"));
                });
            }
        }
        this.rotateLeft = function () {
            var id = currentPageId();//$('.treeview_select').attr('id').toString();
           
            if (isSelectDoc()) {
                var r = relatives(id);
                if (r.length > 0) {
                    $.each(r, function () {
                        docs[this].rotateCounterClockwise();
                    });
                }
            } else {
                docs[id].rotateCounterClockwise();
            }
        }
        this.rotateRight = function () {
            var id = currentPageId();//$('.treeview_select').attr('id').toString();
            
            if (isSelectDoc()) {
                var r = relatives(id);
                if (r.length > 0) {
                    $.each(r, function () {
                        docs[this].rotateClockwise();
                    });
                }
            } else {
                docs[id].rotateClockwise();
            }
        }
        var curId;
        this.showOCRZone = function (index, pos) {
            var page = $(".treeview_select").parentsUntil(".treeview_second").filter(':last').children('ul').children();
            //var index = $(".pageIndex", this).val();
            var id = $(page[index]).children('.page').attr('id') || $(page[index]).attr('id');
            if (id) {
                docs[id].removePassiveAnnotation();
                docs[id].showPassiveAnnotation({
                    left: pos.left,
                    top: pos.top,
                    width: pos.width,
                    height: pos.height
                });
                curId = id;
                if (!loading(id)) {
                    $('#documentViewer').scrollTo($("#passive"));
                }
            }
        }
        this.init = function (id, annotations, angle) {
            if (docs[id]) {
                if (annotations) {
                    var anns = [];
                    $.each(annotations, function () {
                        var an = docs[id].convert(this);
                        anns.push(an);
                    });
                    docs[id].createAnnotations(anns);
                }
                if (angle) {
                    var i = angle/90;
                    if (i > 0)
                        for (j = 0; j < i; j++)
                            docs[id].rotateClockwise();
                    else
                        for (j = 0; j < -i; j++)
                            docs[id].rotateCounterClockwise();
                }
                    
            }
        }

        this.removeOCRZone = function () {
            var page = $(".treeview_select").parentsUntil(".treeview_second").filter(':last').children('ul').children();
            //var index = $(".pageIndex", this).val();
            var id = currentPageId();//$(page[curId]).children('span.treeview_select').attr('id') || "";
            if(id)
                docs[id].removePassiveAnnotation();
        }
        function currentPageId() {
            var id = $('.treeview_select').attr('id') || $('.treeview_select').parent().find('.page').attr('id');
            return id;
        }
        function isSelectDoc(){
            return $('.treeview_select').hasClass('folder');
        }
    }
});