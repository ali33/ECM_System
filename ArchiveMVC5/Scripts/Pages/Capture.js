//##################################################################
//# Copyright (C) 2008-2013, MIA Solution. All Rights Reserved. #
//# 
//# History: 
//# Date Time   Updater     Comment 
//# 23/08/2013  ThoDinh       Tạo mới 
//##################################################################
var insOption;
var pageCurrent;
var docTypeName;
var documentId;
var fields = {};
var $thumbnail, $toolbar, $documentViewer, $docViewerLoading, $classifyLaterFolder, $docTypeId;
var $selectedPage;
var $selectedFolder;
var draw;
//danh sách toolbar theo id của doctype
var toolbarElement = [];
var draws = {};
var thumbs = {};
var currentPageId;
var previousPageId;
//Table field value
var tableFieldValues = [];
var thumbAngle = 0;
var delta = 0.1;
var nZoom = 0;

//Annotation array when view loaded
var rotate = {};
//Khai bao Enum
var Options = {
    Import: 0,
    InsertBefore: 1,
    InsertAfter: 2,
    Replace: { Import: 31, Scan: 32 },
    NewFromSelected: 4,
    NewStartingHer: 5,
    ClassifyLater: 6,
    Exist: 7
};

var dataType = {
    String : 0,
    Integer : 1,
    Decimal : 2,
    Picklist : 3,
    Boolean : 4,
    Date : 5,
    Folder : 6,
    Table : 7
};

var page_selected_css = {
    border: '2px solid rgb(255, 106, 0)'
};

var load_tool_bar = false;
var isEditOcrAndDoc = false;

var tool = new Toolbar();

function moveDefaultBatchToFirst() {
    var def = $('ul.default_batch');
    var p = def.parent();
    if (def.index() != 0) {
        def.insertBefore(p.children().first());
    }
}

function changeDocument(key, options) {
    console.log($(this));
    var oldDocument = $(this).parent();
    var _docType = searchDocType(key);
    var pageElements = oldDocument.find('ul.treeview_three');
    var newDocElement = createNewDocElement(pageElements.length, _docType);
    newDocElement.find('ul.treeview_three').replaceWith(pageElements);
    oldDocument.replaceWith(newDocElement);
    updateDocElements();
}
/*
    Tao cac Page (Thumbnail va View)
    @params
        items: danh sach cac page (List<CacheFileResult>)
        $docOnViewer: element de add page
        $parent: element se add cac thumbnail items
        [$pos]: element se dc append
        [opt]: lua chon append 'before' or 'after' or 'replace'
*/
function createItems(items, $parent, $pos, opt, annotations, rotate) {
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
    $(".treeview_three").find("li").removeClass("active");

    $.each(items, function (i) {
        var id = this.KeyCache;
        var Annotations = this.Annotations;
        var rotateAngle = this.RotateAngle;
        var $item = createThumbnail(this, i);

        if (rotateAngle == undefined) {
            rotateAngle = 0;
        }

        thumbs[id] = $item.thumbnail(rotateAngle);

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
        $item.click(pageClick)
        itemElements.push($item);
        ///////////////Create document viewer

        if (this.FileType != "image") {
            var URL_Text = URL_Get_Document + "/?key=" + this.KeyCache;
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
            var $page = $("<div id='" + pageID + "' class='content_page'></div>");
            var imageUrl = URL_LoadImage + "?key=" + this.KeyCache;
            if ($target) {
                if (opt == 'before' && i == 0)
                    $page.insertBefore($target);
                else
                    $page.insertAfter($target);
                $target = $page;
            } else {
                $document.append($page);
            }
            draws[this.KeyCache] = $page.annotationCapture({ image: imageUrl, width: ($documentViewer.width() - 50) });
            add(this.KeyCache, draws[this.KeyCache]);
            $page.click(pageElementClick);

            $page.find('img').load(function () {
                countLoaded++;
                if (countLoaded == items.length) {
                    if ($pageToRemove)
                        $pageToRemove.remove();
                    $document.attr('data-loading', false);
                    $documentViewer.children().hide();
                    $documentViewer.append($document);
                    $docViewerLoading.find($document).remove();
                    $("#documentViewer").scrollTo($document.children().first());
                }
                $item.find('.treeview_title').ecm_loading_hide();
                if(Annotations)
                    tool.init(id, Annotations, rotateAngle);
            });

            if (i == 0) {
                $page.addClass("active");
            }
        }

        if (i == 0) {
            $item.find(".page").parent().addClass("active");
        }
    });
    if (opt == 'replace') {
        $pos.next().find(".page").parent().addClass("active");
        $pos.remove();
        $pageToRemove.next().find(".content_page").addClass("active");
        $pageToRemove.remove();
    }
    return itemElements;
}

function createThumbnail(item, i) {
    //////////Create Thumbnail on left side bar
    var imageUrl = URL_LoadImage + "/?key=" + item.KeyCache + "&thumb=true";
    var dpi = "";
    if (item.FileType == "image")
        dpi = item.Resolution + " dpi";
    //Sau khi submit thanh cong server tra ve danh sach cac key,tuong ung voi so trang cua image
    //Hien thi thumbnail cac trang tren left menu
    var $item = $('<li id="' + item.KeyCache + '">'
                + '<input type="hidden" class="language" value="eng"/>'
                + '<span class="page treeview_title" id="' + item.KeyCache + '">'
                    + '<a>'
                            + '<img src="' + imageUrl + '" />'
                        + '<span>'
                            + '<h1 class="pageNumber"><strong>' + (i + 1) + '</strong></h1>'
                            + '<span>' + dpi + '</span>'
                        +'</span>'
                + '</a>'
            +'</li>');
    $viewer = $('<input type="hidden" class="viewer" value="' + item.FileType + '"/>');
    $item.append($viewer);
    $item.data('key', item.KeyCache);
    return $item;
}

function updatePageNotClassify(items, $pos, opt) {
    //$target is a page to replace or insert aftter or insert before
    var $target;
    //$targetitem is a thumbnail item to replace or insert aftter or insert before
    var $targetItem;
    var $pageToRemove;
    if ($pos) {
        var id = $pos.find('.page').attr('id');
        $target = $('#page_' + id);documentViewer
        $targetItem = $pos;
        $target.parent().remove();
        if (opt == 'replace')
            $pageToRemove = $target;
    }
    //$docViewerLoading.children('.content').hide();
    $.each(items, function (i) {
        var $document = $("<div class='document' data-loading='true'/>");
        $docViewerLoading.append($document);
        var $item = createThumbnail(this,i);

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
        $item.click(pageClick)
        ///////////////Create document viewer

        if (this.FileType != "image") {
            var URL_Text = URL_Get_Document + "/?key=" + this.KeyCache;
            var pageID = "page_" + this.KeyCache;
            var $page = $("<div id='" + pageID + "/>");
            $page.css({
                width: '100%', height: '100%', overflow: 'hidden',
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
            var $page = $("<div id='" + pageID + "' class='content_page></div>");
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

function createDocument(data, insOption, docId, documentTypeName, tempId) {

    if (data == null)
    {
        return;
    }

    var set,
    $parentItem;
    if (documentTypeName)
        docTypeName = documentTypeName;
    doctype = searchDocType(docTypeName);
    var $doc;
    var $parentItem;
    documentId = docId;
    isEditOcrAndDoc = false;

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

            updatePageElements($parentItem);
            break;
        }
        case Options.Replace.Import: {
            $doc = $selectedFolder;
            $parentItem = $doc.find('ul.treeview_three').length ? $doc.find('ul.treeview_three') : $doc;
            isEditOcrAndDoc = true;
            if ($parentItem.hasClass('default_batch')) {
                updatePageNotClassify(data, $selectedPage, 'replace');
            } else {
                createItems(data, $parentItem, $selectedPage, 'replace');
            }
            performIndex();
            updatePageElements($parentItem);
            break;
        }
        case Options.InsertBefore: {
            $doc = $selectedFolder;
            $parentItem = $doc.find('ul.treeview_three').length ? $doc.find('ul.treeview_three') : $doc;
            isEditOcrAndDoc = true;
            if ($parentItem.hasClass('default_batch')) {
                updatePageNotClassify(data, $selectedPage, 'before');
            } else {
                createItems(data, $parentItem, $selectedPage, 'before');
            }
            performIndex();
            updatePageElements($parentItem);
            break;
        }
        case Options.InsertAfter: {
            $doc = $selectedFolder;
            $parentItem = $doc.find('ul.treeview_three').length ? $doc.find('ul.treeview_three') : $doc;
            isEditOcrAndDoc = true;
            if ($parentItem.hasClass('default_batch')) {
                updatePageNotClassify(data, $selectedPage, 'after');
            } else {
                createItems(data, $parentItem, $selectedPage, 'after');
            }
            performIndex();
            updatePageElements($parentItem);
            break;
        }
        case Options.Exist: {
            $doc = createNewDocElement(data.length, doctype, docId, tempId);
            $parentItem = $doc.find('ul.treeview_three');
            $thumbnail.append($doc);
            createItems(data, $parentItem, null, null);
            performIndex();
            break;
        }
        default: {
            if (data && data.length) {
                $doc = createNewDocElement(data.length, doctype, docId, tempId);
                $parentItem = $doc.find('ul.treeview_three');
                $thumbnail.append($doc);
                createItems(data, $parentItem);
                performIndex();
                updatePageElements($parentItem);
            } else {
                $.EcmAlert("Can not convert file to view", 'WARNING', BootstrapDialog.TYPE_WARNING);
            }
            break;
        }
    }
    load_tool_bar = false;
    if (data[0].FileType == "image") {
        if (doctype) {
            LoadToolbar(doctype.DocType.Id);
        }
        else {
            LoadToolbar(null);
        }

        load_tool_bar = true;
    }


    $('body').ecm_loading_hide();
}

function LoadToolbar(docTypeId) {
    if (docTypeId == undefined) {
        getToolbar('');
    }
    else {
        if (toolbarElement[docTypeId]) {
            $toolbar.html(toolbarElement[docTypeId]);
        }
        else {
            getToolbar(docTypeId);
        }
    }
}

function getToolbar(docTypeId) {
    $.get(URL_Toolbar + "?id=" + docTypeId, function (data) {
        $toolbar.html(data);

        if (docTypeId != '') {
            toolbarElement[docTypeId] = data;
        }
    });

}

function searchDocType(key) {
    rs = $.grep(docType, function (item) {
        return item.DocType.Name == key;
    });
    if (rs.length > 0)
        return rs[0];
}

function getObjects(obj, key, val) {
    var objects = [];
    for (var i in obj) {
        if (!obj.hasOwnProperty(i)) continue;
        if (typeof obj[i] == 'object') {
            objects = objects.concat(getObjects(obj[i], key, val));
        } else if (i == key && obj[key] == val) {
            objects.push(obj);
        }
    }
    return objects;
}

function relatives(id) {
    var p = $('#' + id).parent().parent();
    var rs = [];
    p.find('li > .treeview_title').each(function () {
        rs.push($(this).attr('id').toString());
    });
    return rs;
}


var docs = {};
var opt = { high: 1, text: 2, redaction: 3 };
var current;

function add (id, d) {
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

//manage annotationCapture instances
function Toolbar() {
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
        var id = currentPageId();
        var r = relatives(id);
        if (r.length > 0) {
            $.each(r, function () {
                docs[this].zoomIn();
            });
        }
    }
    this.zoomOut = function () {
        var id = currentPageId();
        var r = relatives(id);
        if (r.length > 0) {
            $.each(r, function () {
                docs[this].zoomOut();
            });
        }
    }
    this.fitWidth = function () {
        var id = currentPageId();
        var r = relatives(id);
        if (r.length > 0) {
            $.each(r, function () {
                docs[this].fit($documentViewer.width());
            });
        }
    }
    this.fitHeight = function () {
        var id = currentPageId();
        var r = relatives(id);
        if (r.length > 0) {
            $.each(r, function () {
                docs[this].fitH($documentViewer.height());
            });
        }
    }
    this.hide = function () {
        var id = currentPageId();
        var r = relatives(id);
        if (r.length > 0) {
            $.each(r, function () {
                docs[this].toggleHidden();
            });
        }
    }
    this.pan = function () {
        var id = currentPageId();
        var r = relatives(id);
        if (r.length > 0) {
            $.each(r, function () {
                docs[this].scrollable($("#documentViewer"));
            });
        }
        //enableSelectedMode(false,this);
    }
    this.rotateLeft = function () {
        var id = currentPageId();
        var $docElements = $(".page").parent(".active");
        docs[id].rotateCounterClockwise();
        thumbs[id].rotateThumbLeft();
    }
    this.rotateRight = function () {
        var id = currentPageId();
        docs[id].rotateClockwise();
        thumbs[id].rotateThumbRight();
    }

    var curId;
    this.showOCRZone = function (index, pos, fieldId) {
        var page = $(".page").parent(".active").parentsUntil(".treeview_second").filter(':last').children('ul').children();
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
                var src = $("#page_" + curId).children('img').attr('src');
                var croppedSrc;
                var para = new Array();
                
                para.push(src.substring(src.indexOf('=') + 1));
                para.push(pos.top);
                para.push(pos.left);
                para.push(pos.width);
                para.push(pos.height);
                para.push(fieldId);

                $.ajax({
                    url: URL_CROPPED_IMAGE,
                        type: "POST",
                        data: JSON.stringify(para),
                        async: false,
                        contentType: "application/json",
                        dataType: "json",
                        tranditional:true,
                        error: function (e) {
                            console.log(e);
                        },
                        success: function (data) {
                            var $croppedImage = $("<img src='" + data + "' atl=''/>");
                            $("#croppedImage").empty().append($croppedImage);
                            $croppedImage.css({
                                width: pos.width, height: pos.height, 'margin-left':'auto','margin-right':'auto'
                            });
                            $("#croppedImage").css({visibility: 'visible' }).show();
                            
                            //Scroll snipped image
                            var $container = $("#croppedImage");
                            var $img = $("#croppedImage img");
                            var cHeight = $container.height();
                            var cWidth = $container.width();
                            var iHeight = $img.height();
                            var iWidth = $img.width();

                            //var top = (iHeight - cHeight) / 2;
                            //var left = (iWidth - cWidth) / 2;

                            $container.scrollLeft(0);
                            $container.scrollTop(0);

                            //end scroll snipped image

                            //scroll snipped image
                            var clicking = false;
                            var previousX;
                            var previousY;

                            $("#croppedImage").mousedown(function (e) {
                                e.preventDefault();
                                previousX = e.clientX;
                                previousY = e.clientY;
                                clicking = true;
                            });

                            $(document).mouseup(function () {

                                clicking = false;
                            });

                            $("#croppedImage").mousemove(function (e) {

                                if (clicking) {
                                    e.preventDefault();
                                    var directionX = (previousX - e.clientX) > 0 ? 1 : -1;
                                    var directionY = (previousY - e.clientY) > 0 ? 1 : -1;
                                    //$("#scroll").scrollLeft($("#scroll").scrollLeft() + 10 * directionX);
                                    //$("#scroll").scrollTop($("#scroll").scrollTop() + 10 * directionY);
                                    $("#croppedImage").scrollLeft($("#croppedImage").scrollLeft() + (previousX - e.clientX));
                                    $("#croppedImage").scrollTop($("#croppedImage").scrollTop() + (previousY - e.clientY));
                                    previousX = e.clientX;
                                    previousY = e.clientY;
                                }
                            });



                            $("#croppedImage").mouseleave(function (e) {
                                clicking = false;
                            });
                        },
                    });

                //$("#croppedImage").children('img').attr("src", src);
                //$("#croppedImage").children('img').css({
                //    'margin-left': -left, 'margin-top': -top,
                //    width: imgWidth, height: imgHeight
                //});
                //$("#croppedImage").css({ width: width, height: height, visibility: 'visible' }).show();
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
                var i = angle / 90;
                if (i > 0)
                    for (j = 0; j < i; j++)
                        docs[id].rotateClockwise();
                else
                    for (j = 0; j < -i; j++)
                        docs[id].rotateCounterClockwise();
            }

        }
    }
    this.select = function () {
        var id = currentPageId();
        var r = relatives(id);
        if (r.length > 0) {
            $.each(r, function () {
                docs[this].selected($("#documentViewer"));
            });
        }
        //enableSelectedMode(true, this);
    }
    this.removeOCRZone = function () {
        var id = currentPageId();

        if (id) {
            docs[id].removePassiveAnnotation();
            $("#croppedImage").children('img').remove();
            $("#croppedImage").css({ visibility: 'collapse' }).show();
        }

    }

    function currentPageId() {
        var id = $(".page").parent(".active").attr('id');
        return id;
    }
    function isSelectDoc(id) {
        return $('.treeview_select').hasClass('folder');
    }
}

/* 
    Function create new Document
    Tao ra Document Thumnails
    @params
        pageCounter: so luong page
        docType: DocumentTypeModel
        docId: Neu View Document thi co docId
*/
function createNewDocElement(pageCounter, doctype, docId, tempId) {
    if ((docId == undefined || docId == '') && tempId == undefined) {
        tempId = guid();
    }

    var $wrap = $('<li data-doc_type_id="' + doctype.DocType.Id + '" data-doc-id = "' + docId + '" data-temp_id="' + tempId + '" class="active"></li>');
    var $id = $('<input type="hidden" class="docTypeId" value="' + doctype.DocType.Id + '"/>');
    var $lang = $('<input type="hidden" class="language" value="eng"/>');
    $wrap.append($id.add($lang));
    var $fieldValues = $("<div class='fieldValues'></div>");
    $.each(doctype.DocType.Fields, function () {
        var $fieldValueElement = $("<div data-field_id='" + this.Id + "' class='fieldValueElement'></div>");
        if (this.IsSystemField == false) {
            if (this.Children != null && this.Children.length > 0) {
                var $tableFieldValue = $("<div class='tableFieldValue'></div>");
                $fieldValueElement.append($tableFieldValue);
            }

            $fieldValueElement.append('<input type="hidden" data-id="' + this.Id + '" value=""/>');
            $fieldValues.append($fieldValueElement);
        }
    });
    $wrap.append($fieldValues);

    var $folderContainer = $('<div class="col-md-12 folder">' +
                                '<div class="col-md-5"><img src="' + URL_Folder + '" /></div>' +
                                '<div class="col-md-7">' +
                                    '<div><h3>' + $('.treeview_second').children().length + '.' + doctype.DocType.Name + '</h3></div>' +
                                    '<div class="pageCount"><h5>Pages: ' + pageCounter + '</h5></div>' +
                                '</div>'+
                            '</div>');

    var $folder = $('<ul class="treeview-menu treeview_three connectedSortable"></ul>');
    var removeIntent = false;

    $folder.sortable({
        revert: true,
        helper: 'clone',
        opacity: 0.5,
        placeholder: "ui-state-highlight",
        receive: function (e, ui) {
            var docTypeId = ui.item.parent().parent().data('doc_type_id');
            var allowedReOrderPage = global_Page == "Capture" || permission.documentType[docTypeId]['AllowedReOrderPage'];

            if (allowedReOrderPage) {
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
            }
            return false;
        },
        update: function (e, ui) {
            var docTypeId = ui.item.parent().parent().data('doc_type_id');
            var allowedReOrderPage;
            if (global_Page == "Capture") {
                allowedReOrderPage = global_Page == "Capture" || permission.documentType[docTypeId]['AllowedReOrderPage'];
            }
            else {
                allowedReOrderPage = docTypeId != undefined && permission.documentType[docTypeId]['AllowedReOrderPage'];
            }

            if (allowedReOrderPage) {
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
            } else {
                return false;
            }
        },
        start: function (e, ui) {
            $itemToRemove = null;
            $('body').append($recyclebin);
            $recyclebin.droppable({
                drop: function (event, u) {
                    var docTypeId = u.draggable.parent().parent().data('doc_type_id');
                    var allowedDelete = global_Page == "Capture" || permission.documentType[docTypeId]['AllowedDeletePage'];

                    if (allowedDelete) {
                        $recyclebin.remove();
                        $folder.sortable("option", "revert", false);
                        deletePage(u.draggable);
                        $folder.sortable("option", "revert", true);
                    } else {
                        $.EcmAlert("You do not have permission to delete page!", 'WARNING', BootstrapDialog.TYPE_WARNING);
                    }
                },
                over: function (event, u) {
                    $recyclebin.css('opacity', '0.5');
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
    $folderContainer.click(function () {
        $f = $(this).parentsUntil('.treeview_second').children('.treeview_three');
        $fName = $(this).parent();
        if ($fName.hasClass('active')) {
            $f.slideUp(100);
            $fName.removeClass('active');
        }
        else {
            $f.slideDown(100);
            $fName.addClass('active');
        }
    });

    $wrap.append($folderContainer).append($folder);
    $wrap.click(selection);
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
    $('.current_content').ecm_loading_show();

    var $docElement = $(".page").parent(".active").parentsUntil(".treeview_second").filter(":last");
    var tempid = $docElement.data('temp_id');
    var doc = { DocumentTypeId: 1, Pages: [], TempId:tempid };
    doc.DocumentTypeId = $docElement.find('.docTypeId').val();
    if (doc.DocumentTypeId !== undefined) {
        var $pageElement = $docElement.find($('ul.treeview_three > li'));
        $.each($pageElement, function () {
            var imgKey = $(this).find(".treeview_title").attr("id");
            var _LangCode = $(this).find('.language').val();
            var Page = { ImgKey: imgKey, Annotations: [], LanguageCode: _LangCode };
            doc.Pages.push(Page);
        });

        var fieldValues = $docElement.find('div.fieldValues').children();
        var fieldOCRs;
        var fieldNames;
        if (isEditOcrAndDoc) {
            fieldValues.parent().removeData("cacheHTML");
            $(".current_content_fields").find(".content_fields").remove();
            $(".current_content_fields").parent().find("#croppedImage").hide();
            $(".current_content_fields").parent().find("#croppedImage").find("img").remove();
        }

        if (fieldValues.parent().data("cacheHTML") == undefined) {
            //For import
            var data_ocr = {
                doc: doc
            };
            //For replace -> import
            // and for insertBefore
            if (isEditOcrAndDoc) {
                data_ocr = {
                    doc: doc,
                    isEdit: isEditOcrAndDoc
                };
            }
            //End for replace -> import
            // and for insertBefore
            //if (global_Page == "Capture") {

            //TODO: Assign TempDocId
            JsonHelper.helper.post(URL_OCR, JSON.stringify(data_ocr), function (data) {
                $(".current_content_fields").html(data);
                fieldOCRs = $(".current_content_fields").find('.content_fields_input > input[type="text"], select, input[type="checkbox"]');
                fieldNames = $(".current_content_fields").find('.label-control');

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
                isEditOcrAndDoc = false;
                $('.current_content').ecm_loading_hide();
            }, function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR);
            });
        } else {
            fieldValues = $docElement.find('div.fieldValues').children();

            $(".current_content_fields").html(fieldValues.parent().data('cacheHTML'));
            fieldOCRs = $(".current_content_fields").find('.content_fields_input > input[type="text"], select, input[type="checkbox"]');

            $.each(fieldValues, function (i) {
                var $fieldValue = $(fieldValues[i]).find("input[type=hidden]");

                $(fieldOCRs[i]).val($(this).val());

                if ($(fieldOCRs[i]).is("input[type=checkbox]")) {
                    if ($(fieldOCRs[i]).val() == "on") {
                        $(fieldOCRs[i]).prop("checked", true);
                    }
                    else {
                        $(fieldOCRs[i]).prop("checked", false);
                    }
                }

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
        }

        $('.current_content').ecm_loading_hide();

        $(document).on('click', '.content_fields_input', function () {
            if (tool && global_Page == "Capture") {
                var id = $(".id", this).val();
                var pos = {
                    left: parseFloat($(".left", this).val()),
                    top: parseFloat($(".top", this).val()),
                    width: parseFloat($(".width", this).val()),
                    height: parseFloat($(".height", this).val())
                }
                var index = $(".pageIndex", this).val();

                if (index != null) {
                    tool.showOCRZone(index, pos, id);
                }
                else {
                    tool.removeOCRZone();
                }
            }

        });
        $('.date').datepicker().on('changeDate', function () {
            $(this).change();
            $(this).parent().addClass('hasvalue');
        });

        $('.lookup').on('input', function () {
            var valueText = $(this).val();
            var fieldId = $(this).parent().find('div').attr('id');
            var lookupField = GetField(fieldId);
            var auto = $(this).parent().find('div');
            var top = $(this).offset().top - $(this).height();
            var left = $(this).offset().left + $(this).width();

            $(document).mouseup(function (e) {
                if (!auto.is(e.target) // if the target of the click isn't the container...
                    && auto.has(e.target).length === 0) // ... nor a descendant of the container
                {
                    auto.hide();
                }
            });

            $.ajax({
                type: "POST",
                url: URL_Lookup,
                data: JSON.stringify({ LookupInfo: lookupField.LookupInfo, Text: valueText }),
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data != '') {
                        auto.empty();
                        auto.append(data);
                        autoFixTableHeader('tableLookup');
                        //auto.css({top: top, left:left});
                        auto.show();

                        $(".lookupRow").hover(function () {
                            var maps = $(this).find('td');

                            for (var i = 0; i < maps.length; i++) {
                                var name = $(maps[i]).find('input[type=hidden]').val();
                                var value = $(maps[i]).text();

                                var fieldNames = $(".current_content_fields").find('.name');
                                for (var j = 0; j < fieldNames.length; j++) {
                                    if (name == $(fieldNames[j]).val() && name != lookupField.Name) {
                                        $(fieldOCRs[j]).val(value);
                                    }
                                }
                            }
                        });

                        $(".lookupRow").click(function () {
                            var maps = $(this).find('td');
                            for (var i = 0; i < maps.length; i++) {
                                var name = $(maps[i]).find('input[type=hidden]').val();
                                var value = $(maps[i]).text();

                                var fieldNames = $(".current_content_fields").find('.name');
                                for (var j = 0; j < fieldNames.length; j++) {
                                    if (name == $(fieldNames[j]).val()) {
                                        $(fieldOCRs[j]).val(value);
                                    }
                                }
                            }

                            auto.hide();
                        });

                        $(".lookupRow").keydown(function (e) {
                            var event = e;
                            var maps = $(this).find('td');
                            for (var i = 0; i < maps.length; i++) {
                                var name = $(maps[i]).find('input[type=hidden]').val();
                                var value = $(maps[i]).text();

                                var fieldNames = $(".current_content_fields").find('.name');
                                for (var j = 0; j < fieldNames.length; j++) {
                                    if (name == $(fieldNames[j]).val()) {
                                        $(fieldOCRs[j]).val(value);
                                    }
                                }
                            }

                            auto.hide();
                        });
                    }
                    else {
                        auto.empty();
                        auto.hide();
                    }
                },
                error: function (err) {
                    $.EcmAlert(err, "WARNNING", BootstrapDialog.TYPE_WARNING);
                }
            });
        });

        //$('.select').on('change', function () {
        //    $(this).val();
        //});

        var title = $(".treeview_title").parent(".active").parentsUntil(".treeview_second").filter(':last').find(".folder > .col-md-7 > div > h3").text();

        var documents = $(".treeview_second").children().length - 1;
        var count = " ( " + (documents) + " doc)";

        $(".current_content_header").html("<strong>" + title + "</strong>" + count);

        if (documents != null && documents > 1) {
            $("#navigation_index").css({ visibility: 'visible' }).show();
        }
        else {
            $("#navigation_index").css({ visibility: 'collapse' }).show();
        }


    }
}

function ShowLookupData(lookupInfo, text, content) {
    JsonHelper.helper.post(
        URL_Lookup,
        JSON.stringify({ LookupInfo: lookupInfo, Text: text }),
        LoadLookupTestSuccess,
        LoadLookupTestError);
}

function LoadLookupTestSuccess(data) {
    var resultContent = $('#lookup_data_result');
    resultContent.empty();
    resultContent.append(data);
}

function LoadLookupTestError(err) {
    $.EcmAlert(err, "WARNNING", BootstrapDialog.TYPE_WARNING);
}

function selection(e) {
    if (!e.ctrlKey) {
        //$('.treeview_select').removeClass('treeview_select');
        //$(this).children('.treeview_title').addClass('treeview_select');
        $(this).find('li').first().click();
    } else {
        //$('.treeview_select').removeClass('treeview_select');
        //$(this).children('.treeview_title').addClass('treeview_select');
        $(this).find('li').first().click();
        //var $this_title = $(this).children('.treeview_title');
        //if ($this_title.hasClass("treeview_select")) {
        //    $this_title.removeClass("treeview_select");
        //} else {
        //    $this_title.addClass("treeview_select");
        //}
    }
    return false;
}

function deletePage($page) {

    var lst_page_select = $page.parent().find("li.active");
    var haveDeleted = lst_page_select.length > 0;
    $.each(lst_page_select, function () {
        var $this = $(this);
        //var selected = $(this).children('.page').parent().hasClass("active");
        //if (selected) {
            var id = $this.children('.page').attr('id');
            var page_sup_parent = $this.parent().parent();
            $this.remove();
            $pageView = $('#page_' + id);
            $docView = $pageView.parent();
            $pageView.remove();
            $docElement = $page.parent();
            if (!$docView.find('div[id^="page"]').length)
                $docView.remove();
            updatePageElements($docElement);
            if (page_sup_parent.find(".page.treeview_title").length == 0) {
                page_sup_parent.find(".folder.treeview_title").remove();
            }
            //recheck
            if ($(".treeview_three").find("li").length == 0) {
                if ($toolbar)
                    $toolbar.children().remove();
            }
            isEditOcrAndDoc = true;
            performIndex();
        //}
    });
    if (!haveDeleted) {
        $.EcmAlert("Please select least one page!", 'WARNING', BootstrapDialog.TYPE_WARNING);
    }
}
//$doc: element to delete, the element can be a title of document or a root tag (<li>) of document
function deleteDoc($doc) {
    var id_first = $doc.find('.page').attr('id') || $doc.parentsUntil('.treeview_second').find('.page').attr('id');
    $('#page_' + id_first).parent().remove();
    $doc.parentsUntil('.treeview_second').remove();
    $doc.remove();
    updateDocElements();
    $toolbar.children().remove();
}

function pageClick(e) {
    currentPageId = $(this).find($('.page')).attr('id');
    var $currentPage = $("#documentViewer").find('#page_' + currentPageId);
    if (loading(currentPageId))
        return false;
    var docTypeId = $('#' + currentPageId).parentsUntil('.treeview_second').last().find('.docTypeId').val();
    if (load_tool_bar) {
        LoadToolbar(docTypeId);
    }
    if ($currentPage.length > 0) {
        $("#documentViewer").children().hide();
        $currentPage.parent().show();
        $("#documentViewer").scrollTo($currentPage);
        $currentPage.click();

    }

    if (!e.ctrlKey) {
        $(this).parent().children().removeClass('active');
        $(this).addClass('active');
    }

    return false;
}

function pageElementClick(e) {
    var $page = $(this).parent().find(".active");

    $.each($page, function (i, item) {
        $(item).removeClass("active");
    });

    $(this).addClass("active");

    var pageId = $(this).attr("id").replace("page_", "");
    var $thumbPage = $(".treeview_three > li.active");
    var $selectThumbPage = $(".treeview_three > li#" + pageId);

    $thumbPage.removeClass("active");
    $selectThumbPage.addClass("active");

    $thumbnail.scrollTo($selectedPage);
}

$('.content_page').mousedown(function (event) {
    switch (event.which) {
        case 3:
            pageElementClick(event);
            break;
        default:
            $.EcmAlert("You have a strange Mouse!", "WARNNING", BootstrapDialog.TYPE_WARNING);
    }
});
//Check document full loaded
//id of page 
function loading(id) {
    if ($("#" + id).parent().find('.viewer') == 'image' &&
        $docViewerLoading.find('#page_' + currentPageId))
        return true;
    return false;
}


function toobarClick() {
    $(document).on('click', '.control_zoomin', function () {
        //$(document).on('click','.select').removeClass('select');
        $(this).addClass('select');
        //draw.zoomIn();
        tool.zoomIn();
        $(this).removeClass('select');
        return false;
    });
    $(document).on('click', '.control_fitwidth', function () {
        $(this).addClass('select');
        tool.fitWidth();
        $(this).removeClass('select');
        return false;
    });
    $(document).on('click', '.control_fitheight', function () {
        $(this).addClass('select');
        tool.fitHeight();
        $(this).removeClass('select');
        return false;
    });
    $(document).on('click', '.control_pan', function () {
        $('.select').removeClass('select active');
        $(this).addClass('select active');
        tool.pan();
    });
    $(document).on('click', '.control_select', function () {
        $('.select').removeClass('select active');
        $(this).addClass('select active');
        tool.select();
    });

    $(document).on('click', '.control_hide', function () {
        tool.hide();
    });

    $(document).on('click', '.control_highlight', function () {
        $('.select').removeClass('select active');
        $(this).addClass('select active');
        tool.highlight();
    });

    $(document).on('click', '.control_redaction', function () {
        $('.select').removeClass('select active');
        $(this).addClass('select active');
        tool.redaction();
    });

    $(document).on('click', '.control_comment', function () {
        $('.select').removeClass('select active');
        $(this).addClass('select active');
        tool.text();
    });

    $(document).on('click', '.control_zoomout', function () {
        tool.zoomOut();
        return false;
    });

    $(document).on('click', '.leftbar_lable', function () {
        if (tool)
            tool.removeOCRZone();
    });

    $(document).on('click', '.control_print', function () {
        console.log($(".documentViewer"));
        console.log(_linkCSS);
        //$(".documentViewer").printThis({ debug: true, loadCSS: _linkCSS });
        $(".documentViewer").printThis({
            debug: false,               
            importCSS: true,            
            importStyle: false,         
            printContainer: true,       
            loadCSS: _linkCSS,
            pageTitle: "",              
            removeInline: false,        
            printDelay: 333,            
            header: null,               
            formValues: true            
        });

    });

    $(document).on('click', '.control_rotationleft', function (id) {
        tool.rotateLeft();
    });

    $(document).on('click', '.control_rotationright', function (id) {
        tool.rotateRight();
    });

    $(document).on('click', ".control_save", function () {
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
        var formData = $("<div class='box box-info save_or_mail_option'>" +
                            "<div class='box-header with-border'>Save information</div>" +
                            "<div class='box-body'></div>" +
                        "</div>");

        var attach = $("<div class='form-group'><h4>Attachment File Options</h4></div>" +
                "<div class='form-group'>" +
                    "<input type='radio' name='format' value='pdf' checked='checked' id='mail_opt_pdf' class='col-md-1'/>" +
                    "<label for='mail_opt_pdf' class='label-control col-md-5'>Email attachment as PDF</label>" +
                    "<input type='radio' name='format' value='tiff'/ id='mail_opt_tiff' class='col-md-1'/>" +
                    "<label for='mail_opt_tiff' class='label-control col-md-5'>Email attachment as TIFF</label>" +
                "</div>" +
            "</div>" +
            "<div class='form-group'><h4>Page range</h4></div>" +
                "<div class='form-group'>" +
                    "<input type='radio' name='pagerange' value='all' id='mail_opt_all' checked='checked' class='col-md-1'/>" +
                    "<label for='mail_opt_all' class='label-control col-md-3'>All</label>" +
                    "<input type='radio' name='pagerange' value='pages' id='mail_opt_pages' class='col-md-1'/>" +
                    "<label for='mail_opt_pages' class='label-control col-md-3'>Pages</label>" +
                "</div>" +
                "<div class='form-group'>" +
                    "<input type='text' name='listofpage' class='form-control input-sm' />" +
                "</div>");

        formData.find('.box-body').append(attach);
        BootstrapDialog.show({
            title: 'Save option',
            message: formData,
            buttons: [{
                label: 'OK',
                cssClass: 'btn-success',
                icon: 'glyphicon glyphicon-ok',
                action: function (dialogRef) {
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
                    JsonHelper.helper.post(URL_SaveLocal, JSON.stringify(mail), function (data) {
                        $('section.body').ecm_loading_hide();
                        window.open(URL_Get + "?key=" + data, "_blank");
                    });
                    dialogRef.close();
                }
            },
            {
                label: 'Cancel',
                action: function (dialogRef) {
                    dialogRef.close();
                }
            }]
        });
    });

    $(document).on('click', '.control_mail', function () {
        var $select = $('.treeview_select');
        //if class treeview_select is Page, docElement = <li>..</li>
        var $docElement = $select.parentsUntil('.treeview_second', ':last');

        var id = $docElement.children('.docTypeId').val();
        var pageElements = $docElement.find('.treeview_three > li');
        var fieldValueElements = $docElement.find('div.fieldValues > input[type=hidden]');
        var _DocumentId = $docElement.data('doc-id');
        var Document = {
            DocumentTypeId: id,
            Pages: [], FieldValues: [],
            DocumentId: _DocumentId
        };
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
        createMailForm(Document);
    });

    function createMailForm(Document) {
        var formData = $("<div class='box box-info save_or_mail_option'>" +
                    "<div class='box-header with-border'>Sent mail information</div>" +
                    "<div class='box-body'></div>" +
                "</div>");
        var attach = $("<div class='form-group'><h4>Attachment File Options</h4></div>" +
                        "<div class='form-group'>" +
                            "<input type='radio' name='format' value='pdf' checked='checked' id='mail_opt_pdf' class='col-md-1'/>" +
                            "<label for='mail_opt_pdf' class='label-control col-md-5'>Email attachment as PDF</label>" +
                            "<input type='radio' name='format' value='tiff'/ id='mail_opt_tiff' class='col-md-1'/>" +
                            "<label for='mail_opt_tiff' class='label-control col-md-5'>Email attachment as TIFF</label>" +
                        "</div>" +
                    "</div>" +
                    "<div class='form-group'><h4>Page range</h4></div>" +
                        "<div class='form-group'>" +
                            "<input type='radio' name='pagerange' value='all' id='mail_opt_all' checked='checked' class='col-md-1'/>" +
                            "<label for='mail_opt_all' class='label-control col-md-3'>All</label>" +
                            "<input type='radio' name='pagerange' value='pages' id='mail_opt_pages' class='col-md-1'/>" +
                            "<label for='mail_opt_pages' class='label-control col-md-3'>Pages</label>" +
                        "</div>" +
                        "<div class='form-group'>" +
                            "<input type='text' name='listofpage' class='form-control input-sm' />" +
                        "</div>");
        var mailBy = $("<div class='form-group'><h4>Send by</h4></div>" +
                            "<div class='form-group'>" +
                                "<input type='radio' name='sendby' value='server' checked='checked' id='mail_opt_server' class='col-md-1'/>" +
                                "<label for='mail_opt_server' class='label-control col-md-5'>Send email immediately</label>" +
                                "<input type='radio' name='sendby' value='client' id='mail_opt_client' class='col-md-1'/>" +
                                "<label for='mail_opt_client' class='label-control col-md-5'>Compose email</label>" +
                        "</div>");
        var mailTo = $("<div class='form-group'> " +
                            "<h4>Send mmail information</h4>" +
                        "</div>" +
                        "<div class='form-group'>" +
                            "<label class='label-control'>To email </label>" +
                            "<input type='text' name='mailTo' class='form-control input-sm'/>" +
                            "<a href='#'>Add Cc, Bcc email address</a><br/>" +
                        "</div>" +
                    "</div>");
        var cc = $("<div class='form-group'><label class='label-control'>CC </label><input class='form-control input-sm' type='text' name='cc'/></div>" +
                 "<div class='form-group'><label class='label-control'>BCC </label><input class='form-control input-sm' type='text' name='bcc'/></div>");
        //#endregion
        //#region create style
        mailBy.children("input[name=sendby]:radio").change(function () {
            var checkVal = $(this).val();
            if (checkVal == "server") {
                mailTo.show();
            }
            else {
                mailTo.hide();
            }
        });

        cc.hide();
        mailTo.append(cc);
        mailTo.children('a').click(function () {
            if (cc.is(":visible"))
                cc.show();
            else
                cc.hide();
            return false;
        });
        formData.find('.box-body').append(attach).append(mailBy).append(mailTo);
        BootstrapDialog.show({
            title: "Mail options",
            message: formData,
            buttons: [{
                label: 'OK',
                cssClass: 'btn-success',
                icon: 'glyphicon glyphicon-ok',
                action: function (dialogRef) {
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
                    mail['sendby'] = $('input[name="sendby"]:checked', form).val();

                    if ($('input[name="mailTo"]', form).val() == "" && $('input[name="sendby"]:checked', form).val() == "server") {
                        $.EcmAlert("Mail to is required", "INFO", BootstrapDialog.TYPE_INFO);
                        return false;
                    }

                    mail['Document'] = Document;

                    if ($('input[name="sendby"]:checked', form).val() == 'client') {
                        var maildiv = $("#mail_div");
                        var divcc = $("#div_cc");
                        var divbcc = $("#div_bcc");
                        var btncc = $("#show_cc");
                        var btnbcc = $("#show_bcc");

                        divcc.hide();
                        divbcc.hide();
                        maildiv.children('input').val();
                        maildiv.find("#mail_body").val();

                        btncc.click(function () {
                            if (divcc.is(":visible")) {
                                divcc.hide();
                                $(this).empty().text("Show cc");
                            }
                            else {
                                divcc.show();
                                $(this).empty().text("Hide cc");
                            }
                        });

                        btnbcc.click(function () {
                            if (divbcc.is(":visible")) {
                                divbcc.hide()
                                $(this).empty().text("Show bcc");
                            }
                            else {
                                divbcc.show();
                                $(this).empty().text("Hide bcc");
                            }
                        });

                        $("#btn_send").click(function () {
                            $("#dialog_send_mail").ecm_loading_show();

                            mail['mailTo'] = $("#mail_div").find("#mail_to").val();

                            if ($("#mail_div").find("#mail_cc").val() != "") {
                                mail['cc'] = $("#mail_div").find("#mail_cc").val();
                            }

                            if ($("#mail_div").find("#mail_bcc").val() != "") {
                                mail['bcc'] = $("#mail_div").find("#mail_bcc").val();
                            }

                            mail['subject'] = $("#mail_div").find("#mail_subject").val();
                            mail['body'] = $("#mail_div").find("#mail_body").val();

                            JsonHelper.helper.post(URL_SendMail, JSON.stringify(mail), function (data) {
                                $.EcmAlert(data, "INFO", BootstrapDialog.TYPE_INFO);
                            }, function (jqXHR, textStatus, errorThrown) {
                                $.EcmAlert("Failed to send your email!", "INFO", BootstrapDialog.TYPE_INFO);
                            }, false);

                            $("#dialog_send_mail").ecm_loading_hide();
                            $("#dialog_send_mail").modal("hide");
                        });

                        $("#dialog_send_mail").modal("show");
                        dialogRef.close();
                    }
                    else {
                        mail['mailTo'] = $('input[name="mailTo"]', form).val();

                        if ($('input[name="cc"]', form).val() != "") {
                            mail['cc'] = $('input[name="cc"]', form).val();
                        }

                        if ($('input[name="bcc"]', form).val() != "") {
                            mail['bcc'] = $('input[name="bcc"]', form).val();
                        }
                        dialogRef.getModal().ecm_loading_show();

                        JsonHelper.helper.post(URL_SendMail, JSON.stringify(mail), function (data) {
                            dialogRef.getModal().ecm_loading_hide();
                            $.EcmAlert(data, "INFO", BootstrapDialog.TYPE_INFO);
                        }, function (jqXHR, textStatus, errorThrown) {
                            $.EcmAlert("Failed to send your email!", "INFO", BootstrapDialog.TYPE_INFO);
                            dialogRef.getModal().ecm_loading_hide();
                        }, false);

                        dialogRef.close();
                    }
                }
            }, {
                label: 'Cancel',
                action: function (dialogRef) {
                    dialogRef.close();
                }
            }]
        });
    }
}

$(document).ready(function () {
    $thumbnail = $('.thumb');
    $toolbar = $('.capture_content_menu');
    $documentViewer = $("#documentViewer");
    $docViewerLoading = $("<div id='docViewerTemp'>");
    $classifyLaterFolder = $('<ul class="default_batch connectedSortable"></ul>').css({ 'min-height': '2px' });
    $docTypeId = $("#docTypeId");
    $thumbnail.append($classifyLaterFolder);
    _top = ($('body').scrollTop() + $('body').height() - 300) / 2;
    _left = ($('body').scrollTop() + $('body').width() - 300) / 2;
    $recyclebin = $('<img src="' + URL_Recycle + '"/>').css({
        position: 'absolute', top: _top, left: _left, height: 300, width: 300, opacity: '0.1', zIndex: 2
    });
    $itemToRemove = null;
    //$('body').ecm_loading_show();
    createContextMenu(docType, permission);
    //$(".date").datepicker();

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
                    $documentViewer.children(':not(.content)').hide();
                    $documentViewer.children('.content').show();
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
    //Drag drop on  folder title
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
                    var docTypeId = u.draggable.data('doc_type_id');
                    var allowedDelete = global_Page == "Capture" || permission.documentType[docTypeId]['AllowedDeletePage'];
                    
                    if(allowedDelete){
                        $recyclebin.remove();
                        deleteDoc(u.draggable);
                        //u.draggable.remove();
                        updateDocElements();
                        //$('#documentViewer').children(':not(.content)').hide();
                        $documentViewer.children('.content').show();
                    } else {
                        $.EcmAlert("You do not have permission to delete page!", 'WARNING', BootstrapDialog.TYPE_WARNING);
                    }
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

    //Su kien chon file xong va file co su thay doi,
    $("#filePath").change(function () {
        if ($("#filePath").val() == '') {
            return;
        }

        $('body').ecm_loading_show();
        //Goi ham submit de upload image len server
        options = {
            url: URL_PostImage,
            dataType: "json",
            success: function (data) {
                createDocument(data, insOption);
            },
            error: showError,
        };
        $('#formUpload').ajaxSubmit(options);
    });

    $("#filePath").click(function () {
        $(this).val("");
    });

    function showError(jqXHR, textStatus, errorThrown ) {
        $.EcmAlert('Upload fail.', "WARNNING", BootstrapDialog.TYPE_WARNING);
        console.log(textStatus);
        console.log(errorThrown);
        console.log(jqXHR);
        $('body').ecm_loading_hide();
    }
    
    $docViewerLoading.css({
        visibility: 'hidden',
        left: $('body').offset().left + $('body').width(),
        top: 0,//$('body').offset().top + $('body').height(),
        width: $documentViewer.width(),
        height: $documentViewer.height(),
        zIndex: -1,
        position: 'absolute'
    });
    $('body').append($docViewerLoading);

    $(document).on('click', '.treeview_three > li', pageClick);

    $('#submit').click(function () {
        if ($('.default_batch > li').length > 0) {
            $.EcmAlert('Please Classify Document before submit!', "WARNNING", BootstrapDialog.TYPE_WARNING);
            return;
        }
        var fillRequired = true;

        $('div.content_fields_input .validate').each(function () {
            $(this).parent().removeClass("has-error");
            $(this).prop("title", "");
            if (!$(this).val()) {
                fillRequired = false;
                $(this).parent().addClass("has-error");
                $(this).prop("title", "Field is required!");
                return false;
            }
        });

        if (!fillRequired) {
            //$.EcmAlert("Please fill in all the required fields", "WARNNING", BootstrapDialog.TYPE_WARNING);
            return;
        }
        var docElements = $('.treeview_second > li');

        data = { Documents: [] };
        var docElements = $('.treeview_second > li');
        for (i = 0; i < docElements.length; i++) {
            var docTypeId = $(docElements[i]).children('.docTypeId').val();
            var id = $(docElements[i]).data('doc-id');
            var pageElements = $(docElements[i]).find('.treeview_three > li');
            var fieldValueElements = $(docElements[i]).find('div.fieldValues > div.fieldValueElement');
            
            data.Documents[i] = {
                DocumentTypeId: docTypeId,
                DocumentId: id,
                Pages: [], FieldValues: []
            };

            for (j = 0; j < pageElements.length; j++) {
                var _ImgKey = $(pageElements[j]).children('span.treeview_title').attr('id');
                var _LangCode = $(pageElements[j]).find('.language').val();
                var _PageId = $(pageElements[j]).data('page-id');
                var _annotations = {};
                var angle = 0;
                if(draws[_ImgKey] != undefined){
                    _annotations = draws[_ImgKey].getAnnotations();
                    angle = draws[_ImgKey].rotateAngle();
                }
                data.Documents[i].Pages[j] = {
                    ImgKey: _ImgKey, Annotations: _annotations, LanguageCode: _LangCode,
                    PageWidth: $("#page_" + _ImgKey).width(), PageHeight: $("#page_" + _ImgKey).height(),
                    RotateAngle: angle * -1,
                    PageId: _PageId
                };
            }

            for (k = 0; k < fieldValueElements.length; k++) {
                var _Id = $(fieldValueElements[k]).attr('data-field_id');
                var _Value = $(fieldValueElements[k]).val();

                if (_Id) {
                    data.Documents[i].FieldValues[k] = { Id: _Id.toString(), Value: _Value, TableFieldValues: [] };
                }

                var $tableValues = $(fieldValueElements[k]).find('div.tableFieldValue > div.rowIndex');

                if ($tableValues != undefined && $tableValues.length > 0) {

                    $.each($tableValues, function (n) {
                        var rowIndex = $(this).data('row_id');
                        var $tableFieldEement = $(this).find("input[type=hidden]");

                        $.each($tableFieldEement, function (m) {
                            var tableValue = $(this).val();
                            var tableValueId = $(this).data('column_id');

                            data.Documents[i].FieldValues[k].TableFieldValues.push({ FieldId: tableValueId, RowIndex: rowIndex, Value: tableValue });
                        });
                    });
                }

            }
        }
        $("section.body").ecm_loading_show();
        JsonHelper.helper.post(URL_Submit, JSON.stringify(data), function (data, textStatus, jqXHR) {
            $("section.body").ecm_loading_hide();
            $.EcmAlert(data.Message, "WARNNING", BootstrapDialog.TYPE_WARNING);
            if (data.Code == 1 && global_Page == "Capture") {
                docElements.remove();
                $(".current_content_fields").children().remove();
                $(".current_content_header").children().remove().empty();
                $("#croppedImage").children().remove().hide();
                $(".thumb").children().remove();
                $(".button_navigation").children().prop("disable", true);
                $documentViewer.children(":not(.content)").remove();
                $documentViewer.children('.content').show();
                $toolbar.children().remove();
            }
        }, function () { $("section.body").ecm_loading_hide(); });     
        return false;
    });

    /* Hide or show children when click treeview_icon in default_batch */
    $('.treeview_first .treeview_icon').click(function () {
        if (!$('.treeview_second').is(':hidden')) {
            $('.treeview_second').slideUp(100);
            $(this).parent().removeClass('treeview_open');
        }
        else {
            $('.thumb').slideDown(100);
            $(this).parent().addClass('treeview_open');
        }
    });
    
    $('#index').click(function () {
        if (!isEditOcrAndDoc) {
            if (!$('.treeview_three').length || $('.capture_thumbnails').find('.ecm_loading_bg').length > 0)
                return false;
        }
        performIndex.call(this);
    });

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

    toobarClick();

    $(".import_documentType").click(function () {
        docTypeName = $(this).text();
        var _docTypeId = $(this).attr("id");
        importClassified(docTypeName);
    });

    $("#import_none_classify").click(function () {
        docTypeName = null;
        importfile();
    });

    $("#scan_none_classify").click(function () {
        //docTypeName = null;
    });

    $(".scan_documentType").click(function () {
        //docTypeName = null;
        scan($(this).text());
    });

    $("#camera_none_classify").click(function () {
        docTypeName = null;
        showWebcam();
    });

    $(".camera_documentType").click(function () {
        docTypeName = $(this).text();
        showWebcam();
    });

    $(".page_per_content").click(function () {
        var per = $(this).data("page-per-doc");
        $("#current_page_per_doc").val(per);
        $(this).parent().find("a > i").removeClass().addClass("fa fa-square-o");
        $(this).find("a > i").removeClass().addClass("fa fa-check-square-o");
    });

    //script cam
    //$("#webcam").scriptcam({
    //    fileReady: fileReady,
    //    cornerRadius: 20,
    //    cornerColor: 'e3e5e2',
    //    onError: onError,
    //    promptWillShow: promptWillShow,
    //    showMicrophoneErrors: false,
    //    onWebcamReady: onWebcamReady,
    //    setVolume: setVolume,
    //    timeLeft: timeLeft,
    //    fileName: 'demofilename',
    //    connected: showRecord,
    //    path: '/plugins/webcam/'
    //});
    //setVolume(0);
    //$("#slider").slider({ animate: true, min: 0, max: 100, value: 50, orientation: 'vertical', disabled: true });
    //$("#slider").bind("slidechange", function (event, ui) {
    //    $.scriptcam.changeVolume($("#slider").slider("option", "value"));
    //});
});

///////////////////////////////////////////////////////////////////
function importfile() {
    insOption = Options.ClassifyLater;
    $("#formUpload input[name=docTypeId]").val();
    $("#filePath").click();
}

function importClassified(key) {
    var _docType = searchDocType(key);
    if (_docType) {
        insOption = Options.Import;
        docTypeName = key;
        $("#formUpload input[name=docTypeId]").val(_docType.DocType.Id);
        $("#filePath").click();
    }
}

function scan(key) {
    key = key.replace("scan-", "");
    _docType = searchDocType(key);
    if (_docType) {
        insOption = Options.Import;
        docTypeName = key;
        try {
            var obj;
            if (window.chrome)
                obj = document.TwainX;
                //obj = new ActiveXObject("TwainActiveX.TwainActiveX");
            else
                obj = new ActiveXObject("TwainActiveX.TwainActiveX");
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

/////////////////////////////////////////////////////
var pos = 0, ctx = null, saveCB, image = [];
//var cameraWidth = 600, cameraHeight = 450;
var cameraWidth = 320, cameraHeight = 240;
var canvas = document.createElement("canvas");
canvas.setAttribute('width', cameraWidth);
canvas.setAttribute('height', cameraHeight);
ctx = canvas.getContext("2d");
image = ctx.getImageData(0, 0, cameraWidth, cameraHeight);
saveCB = function (data) {
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
    if (pos >= 4 * cameraWidth * cameraHeight) {
        ctx.putImageData(img, 0, 0);
        // $('body').ecm_loading_show();

        $.post(URL_PostSingleImage, { fileUpload: canvas.toDataURL("image/png"), isFromCamera: true }, function (data) {
            //docTypeName = "Test CV"
            if(docTypeName != null)
                createDocument(data, Options.Import);
            else
                createDocument(data, Options.ClassifyLater);
            //$('body').ecm_loading_hide();
        });
        pos = 0;
    }
};
///////////////////////////////////////////////////////////////////

function showWebcam() {
    var $camera = $("#camera_dialog").find('#camera');
    $camera.webcam({
        width: 320,
        height: 240,
        mode: "callback",
        swffile: URL_Camera,
        onTick: function () { },
        onSave: saveCB,
        onCapture: captureWebcam,
        debug: function () { },
        onLoad: function () { }
    });

    $("#camera_dialog").modal("show");

}

$(document).on("click", "#btnok_camera", function () {
    webcam.capture();
});

$(document).on("click", "#btncancel_camera", function () {
    $("#camera_dialog").find('#camera').children().remove();
});

function captureWebcam() {
    webcam.save();
}

//script cam
//function showRecord() {
//    $("#recordStartButton").attr("disabled", false);
//}
//function startRecording() {
//    $("#recordStartButton").attr("disabled", true);
//    $("#recordStopButton").attr("disabled", false);
//    $("#recordPauseResumeButton").attr("disabled", false);
//    $.scriptcam.startRecording();
//}
//function closeCamera() {
//    $("#slider").slider("option", "disabled", true);
//    $("#recordPauseResumeButton").attr("disabled", true);
//    $("#recordStopButton").attr("disabled", true);
//    $.scriptcam.closeCamera();
//    $('#message').html('Please wait for the file conversion to finish...');
//}
//function pauseResumeCamera() {
//    if ($("#recordPauseResumeButton").html() == 'Pause Recording') {
//        $("#recordPauseResumeButton").html("Resume Recording");
//        $.scriptcam.pauseRecording();
//    }
//    else {
//        $("#recordPauseResumeButton").html("Pause Recording");
//        $.scriptcam.resumeRecording();
//    }
//}
//function fileReady(fileName) {
//    $('#recorder').hide();
//    $('#message').html('This file is now dowloadable for five minutes over <a href=' + fileName + '">here</a>.');
//    var fileNameNoExtension = fileName.replace(".mp4", "");
//    jwplayer("mediaplayer").setup({
//        width: 320,
//        height: 240,
//        file: fileName,
//        image: fileNameNoExtension + "_0000.jpg"
//    });
//    $('#mediaplayer').show();
//}
//function onError(errorId, errorMsg) {
//    alert(errorMsg);
//}
//function onWebcamReady(cameraNames, camera, microphoneNames, microphone, volume) {
//    $("#slider").slider("option", "disabled", false);
//    $("#slider").slider("option", "value", volume);
//    $.each(cameraNames, function (index, text) {
//        $('#cameraNames').append($('<option></option>').val(index).html(text))
//    });
//    $('#cameraNames').val(camera);
//    $.each(microphoneNames, function (index, text) {
//        $('#microphoneNames').append($('<option></option>').val(index).html(text))
//    });
//    $('#microphoneNames').val(microphone);
//}
//function promptWillShow() {
//    alert('A security dialog will be shown. Please click on ALLOW.');
//}
//function setVolume(value) {
//    value = parseInt(32 * value / 100) + 1;
//    for (var i = 1; i < value; i++) {
//        $('#LedBar' + i).css('visibility', 'visible');
//    }
//    for (i = value; i < 33; i++) {
//        $('#LedBar' + i).css('visibility', 'hidden');
//    }
//}
//function timeLeft(value) {
//    $('#timeLeft').val(value);
//}
//function changeCamera() {
//    $.scriptcam.changeCamera($('#cameraNames').val());
//}
//function changeMicrophone() {
//    $.scriptcam.changeMicrophone($('#microphoneNames').val());
//}


$(document).on('click', '.table_field_value', function () {

    $(".content_fields").ecm_loading_show();

    var fieldId = $(".id", $(this).parent()).val();
    var $docElement = $(".treeview_select").parentsUntil(".treeview_second").filter(":last");
    var $fieldValueElement = $($docElement).find('div.fieldValues > div.fieldValueElement[data-field_id=' + fieldId + ']');
    var $tableFieldValueElement = $($fieldValueElement).find('div.tableFieldValue');
    var dialogContainer = $('#tableFieldValue');

    if ($tableFieldValueElement.find("input[type=hidden]").length == 0) {
        LoadTableValue(fieldId);
    }
    else {
        var docType = searchDocType(docTypeName);
        var $field = getObjects(docType.DocType.Fields, "Id", fieldId)[0];

        var colCount = $field.Children.length;
        var $tableValue = $("<table class='table tableValue'></table>");
        var $tableValueHeader = $("<thead><tr></tr></thead>");
        var $tableValueBody = $("<tbody></tbody>");

        var $tableValueFooter = $("<tfoot><tr class='fakeRow'><td colspan='" + colCount + "' onclick='addTableRow(" + JSON.stringify($field.Children) + ");'><span class='textButton hand'>Add more row</span></td></tr></tfoot>");

        //build table value header
        var header = '<td></td>';

        $.each($field.Children, function (i, column) {
            header += "<td>" + column.ColumnName + "</td>";
        });

        $tableValueHeader.append(header);
        $tableValue.append($tableValueHeader);

        //build table value body

        $.each($tableFieldValueElement, function (i, item) {
            var $tableRowsData ;
            var $tableRows = $(item).find('div.rowIndex');

            $.each($tableRows, function (j, row) {
                var $values = $(row).find('input[type=hidden]');
                $tableRowsData = $('<tr><td class="removeRow"><i class="glyphicon glyphicon-trash vcenter hand"></i></td></tr>');
                var tableData = '';

                $.each($values, function (k, valueData) {
                    var $columnField = getObjects($field.Children, 'Id', $(valueData).data('column_id'))[0];
                    var textClass = "";

                    if ($columnField.DataType == 5) {
                        textClass = "dateField";
                    }
                    else if ($columnField.DataType == 1) {
                        textClass = "allownumericwithoutdecimal";
                    }
                    else if ($columnField.DataType == 2) {
                        textClass = "allownumericwithdecimal";
                    }

                    tableData += "<td>" +
                                    "<div class='content_fields_input'>" +
                                        "<input type='text' value='" + $(valueData).val() + "' class='form-control input-sm " + textClass + "'/>" +
                                        "<input type='hidden' value='" + $columnField.Id + "' id='tableFieldId'/></div></td>";
                });

                $tableRowsData.append(tableData);
                $tableValueBody.append($tableRowsData);
            });

        });

        $tableValue.append($tableValueBody);
        $tableValue.append($tableValueFooter);

        dialogContainer.empty().append($tableValue);
        $('.content_fields').ecm_loading_hide();
    }

    var fieldIdInput = $("#tableViewDialog").find("#FieldId");

    if (fieldIdInput != undefined) {
        fieldIdInput.val(fieldId);
    }
    $("#tableViewDialog").modal("show");
});

$(document).on('click', '.removeRow', function () {
   // var $body = $('.tableValue > tbody');
    var $row = $(this).parent('tr');

    $row.remove();

    autoFixTableHeader('tableValue');
});


function addTableRow(children) {

    var addedRowStart = "<tr><td class='removeRow'><i class='glyphicon glyphicon-trash vcenter hand'></i></td>";
    var addRowEnd = "</tr>";
    var addedCol = "";

    $.each(children, function (i, item) {
        var columnName = item.ColumnName;
        var className = '';
        var defaultValue = '';

        if (item.DefaultValue != null) {
            defaultValue = item.DefaultValue;
        }

        if (item.DataType == dataType.Date) {
            className = 'dateField';
            if (item.UseCurrentDate) {
                //defaultValue = $.now();
            }
        }

        addedCol += "<td>" +
                        "<div class='content_fields_input'>" +
                            "<input type='text' class='form-control input-sm " + className + "' value='" + defaultValue + "' data-id='" + item.FieldId + "' /><input type='hidden' value='" + item.FieldId + "' id='tableFieldId'/>" +
                        "</div>" +
                    "</td>";
    });

    var row = addedRowStart + addedCol + addRowEnd;
    jQuery(".tableValue tbody").append(row);

    autoFixTableHeader('tableValue');
}


function dialogTableValueOk() {
    var $dialog = $('#tableViewDialog');
    var fieldId = $dialog.find("#FieldId").val();

    var $docElement = $(".treeview_select").parentsUntil(".treeview_second").filter(":last");
    var $fieldValueElement = $($docElement).find('div.fieldValues > div.fieldValueElement[data-field_id=' + fieldId + ']');
    var $tableFieldValueElement = $($fieldValueElement).find('div.tableFieldValue');
    var tableRows = $dialog.find("tbody > tr");
    var rowIndex = 0;

    $tableFieldValueElement.empty();

    $.each(tableRows, function (i, item) {
        //var columnGuidId = $(item).attr("data-id");
        var values = $(item).find("td > div");
        var $tableRow = $('<div data-row_id="' + rowIndex + '" class="rowIndex"></div>');

        $.each(values, function (j, value) {
            var fieldValue = $(value).find("input[type=text]").val();
            var fieldId = $(value).find("#tableFieldId").val();

            var tableFieldValue = '<input type="hidden" value="' + fieldValue + '" data-column_id="' + fieldId + '">';
            $tableRow.append(tableFieldValue);
        });

        $tableFieldValueElement.append($tableRow);
        rowIndex++;
    });

    $('#tableViewDialog').modal("hide");
    $('#tableFieldValue').empty();
}

function dialogTableValueCancel() {
    $(this).dialog("close");
    $('#tableFieldValue').empty();
}

function dialogTableValueOpen() {
    $('.dateField').datepicker().on('changeDate', function () {
        $(this).change();
        $(this).parent().addClass('hasvalue');
    });

    //$('.tableValue').fixedHeader();
    autoFixTableHeader('tableValue');
}

function LoadTableValue(fieldId) {

    ///Get Ajax
    var para = {};

    if (global_Page == 'Capture') {
        var docTypeId = $(".docTypeId", $(".table_field_value").parent()).val();
        para = {
            DocumentTypeId: docTypeId,
            FieldId: fieldId
        }

    }
    else {
        para = {
            DocumentId: documentId,
            FieldId: fieldId
        }
    }
    $.ajax({
        type: "POST",
        url: URL_LoadTableValue,
        data: JSON.stringify(para),
        contentType: "application/json; charset=utf-8",
        async: false,
        success: LoadTableValue_Success,
        error: LoadTableValue_Error
    });
}

function LoadTableValue_Success(data) {
    var tableValueElements = $(data).find('tbody > tr > input[type=text]');

    $.each(tableValueElements, function (i, item) {
        var value = item.val()
    });

    var dialogContainer = $('#tableFieldValue');
    dialogContainer.empty().append(data);
    $('.content_fields').ecm_loading_hide();

}

function LoadTableValue_Error(jqXHR, textStatus, errorThrown) {
    console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
}

$(document).on('click', '#btSaveTableValue', function (e) {
    dialogTableValueOk();
});

//$(document).on("keypress",".allownumericwithdecimal", function (event) {
//    //this.value = this.value.replace(/[^0-9\.]/g,'');
//    $(this).val($(this).val().replace(/[^0-9\.]/g, ''));
//    if ((event.which != 46 || $(this).val().indexOf('.') != -1) && (event.which < 48 || event.which > 57)) {
//        event.preventDefault();
//    }
//});

//$(document).on("keypress",".allownumericwithoutdecimal", function (event) {
//    $(this).val($(this).val().replace(/[^\d].+/, ""));
//    if ((event.which < 48 || event.which > 57)) {
//        event.preventDefault();
//    }
//});

function autoFixTableHeader(className) {
    var $table = $('.' + className +'');
    var $header = $('.'+ className +' thead');
    var $headerColumns = $header.find('tr th');

    var $body = $('.'+ className +' tbody');
    var $bodyRows = $body.find('tr');
    var $bodyColumns = $($bodyRows[0]).find('td');

    $.each($bodyColumns, function (i, item) {
        if ($($headerColumns[i]).width() < $(item).width()) {
            $($headerColumns[i]).width($(item).width());
        }
        else {
            $(item).width($($headerColumns[i]).width());
        }
    });
}

function GetField(id)
{
    for(var i=0; i < doctype.DocType.Fields.length; i++)
    {
        if (id == doctype.DocType.Fields[i].Id)
        {
            return doctype.DocType.Fields[i];
        }
    }
}