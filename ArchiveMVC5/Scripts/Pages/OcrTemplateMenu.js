var style2 = {
    height: 30, fontSize: 11, menuWidth: 200
};
var style1 = {
    height: 30, fontSize: 11, menuWidth: 250
};
var styleCam = {
    height: 30, fontSize: 11, menuWidth: 150
};
var styleScan = {
    height: 30, fontSize: 11, menuWidth: 250
};

function createContextMenu() {
    createContextMenuFolder();
    createContextMenuPage();
}

function createContextMenuPage() {
    //#region Items an Options for context menu
    //Items for Context Menu
    var itemsOfPage = {
        'delete': {
            name: RMenuCapture['delete']
        },
        'replace': {
            name: RMenuCapture['replace'],
            items: {
                "reScan": { name: RMenuCapture['scan'], iconUrl: URL_ScanIcon },
                "reImport": {
                    name: RMenuCapture['import'], iconUrl: URL_ImportIcon
                },
                "reCamera": { name: RMenuCapture['camera'], iconUrl: URL_CameraIcon }
            },
            style: style2
        },
        'sep1': '----',
        'insBefore': {
            name: RMenuCapture['insertBefore'],
            items: {
                "insBefore_Scan": { name: RMenuCapture['scan'], iconUrl: URL_ScanIcon },
                "insBefore_Import": {
                    name: RMenuCapture['import'], iconUrl: URL_ImportIcon
                },
                "insBefore_Camera": { name: RMenuCapture['camera'], iconUrl: URL_CameraIcon }
            },
            style: style2
        },
        'insAfter': {
            name: RMenuCapture['insertAfter'],
            items: {
                "insAfter_Scan": { name: RMenuCapture['scan'], iconUrl: URL_ScanIcon },
                "insAfter_Import": {
                    name: RMenuCapture['import'], iconUrl: URL_ImportIcon
                },
                "insAfter_Camera": {name: RMenuCapture['camera'], iconUrl: URL_CameraIcon}
            },
            style: style2
        },
        'rotationright': {
            name: RMenuCapture['rotateRight']
        },
        'rotationleft': {
            name: RMenuCapture['rotateLeft']
        },
    };

    //Context menu options
    var menuOptPage = {
        selector: "div.ocr_pages li",
        items: itemsOfPage,
        style: style1,
        callback: callback_ContextMenuPage,
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
            }
        }
    };

    //$.contextMenu(menuOptPage);
    //#endregion
}

function createContextMenuFolder() {
    //#region Items an Options for context menu
    //Items for Context Menu
    var itemsOfFolder = {
        "scan": {
            name: RMenuCapture['scan'], iconUrl: URL_ScanIcon,
            style: style2
        },
        "import": {
            name: RMenuCapture['import'], iconUrl: URL_ImportIcon,
            style: style2
        },
        "camera": {
            name: RMenuCapture['camera'], iconUrl: URL_CameraIcon,
            style: styleScan
        },
        "sep": "-----",
        "delete": {
            name: RMenuCapture['delete'], icon: "none"
        }
    };

    //Context menu options
    var menuOptPage = {
        selector: "div.ocrtemplate_root",
        items: itemsOfFolder,
        style: style1,
        callback: callback_ContextMenuFolder,
        events: {
            show: function (opt) {
                var $this = this;
                var value = $this.data();
                if (value['vie'] == value['eng'])
                    $.contextMenu.setInputValues(opt, { key: value.key, 'eng': true, 'vie': false });
                else
                    $.contextMenu.setInputValues(opt, $this.data());
            },
            hide: function (opt) {
                var $this = this;
                $selectedPage = $this;
            }
        }
    };

    //$.contextMenu(menuOptPage);
    //#endregion
}
function callback_ContextMenuFolder(key, options) {
    switch (key) {
        case 'import':
            {
                var docTypeId = $('.doc_type_id').val();
                if (docTypeId) {
                    insOption = Options.Import;
                    //docTypeName = key;
                    $("#formUpload input[name=docTypeId]").val(docTypeId);
                    $("#filePath").click();
                } else {

                }
                break;
            }
        case 'scan':
            {
                insOption = Options.Import;
                Scan();
                break;
            }
        case 'camera':
            {
                insOption = Options.Import;
                showWebcam();
                break;
            }
    }
}

function callback_ContextMenuPage(key, options) {
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
        case 'insBefore_Scan': {
            $(this).click();
            $selectedPage = $(this);
            $selectedFolder = $selectedPage.parentsUntil('.treeview_second').filter('.treeview_second > li');
            insOption = Options.InsertBefore;
            Scan();
            break;
        }
        case 'insBefore_Camera': {
            $(this).click();
            $selectedPage = $(this);
            $selectedFolder = $selectedPage.parentsUntil('.treeview_second').filter('.treeview_second > li');
            insOption = Options.InsertBefore;
            showWebcam();
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
        case 'insAfter_Scan':{
            $(this).click();
            $selectedPage = $(this);
            $selectedFolder = $selectedPage.parentsUntil('.treeview_second').filter('.treeview_second > li');
            insOption = Options.InsertAfter;
            Scan();
        }
        case 'insAfter_Camera':{
            $(this).click();
            $selectedPage = $(this);
            $selectedFolder = $selectedPage.parentsUntil('.treeview_second').filter('.treeview_second > li');
            insOption = Options.InsertAfter;
            showWebcam();
        }
        case 'rotationright': {
            //$(this).click();
            $selectedPage = $(this);
            var currentPageid = $(this).find(".page").attr("id");
            draws[currentPageid].rotateClockwise();
            thumbs[currentPageid].rotateThumbRight();

            //$.each($selectedPage.parent().find(".treeview_select"), function (i, item) {
            //    var pageid = item.id;

            //    if (pageid != currentPageid) {
            //        draws[pageid].rotateClockwise();
            //        //thumbs[pageid].rotateThumbRight();
            //    }
            //});

            break;
        }
        case 'rotationleft': {
            //$(this).click();
            $selectedPage = $(this);

            var currentPageid = $(this).find(".page").attr("id");
            draws[currentPageid].rotateCounterClockwise();
            //thumbs[currentPageid].rotateThumbLeft();

            //$.each($selectedPage.parent().find(".treeview_select"), function (i, item) {
            //    var pageid = item.id;

            //    if (pageid != currentPageid) {
            //        draws[pageid].rotateCounterClockwise();
            //        thumbs[pageid].rotateThumbLeft();
            //    }
            //});

            break;
            //$(".control_" + key).click();
        }
        default: {
        }
    }
}

function Scan() {
    try {
        var obj;
        if ($.browser.chrome)
            obj = document.TwainX;
            //obj = new ActiveXObject("TwainActiveX.TwainActiveX");
        else
            obj = new ActiveXObject("TwainActiveX.TwainActiveX");
        if (obj) {
            var s = obj.Scan(true, "http://" + document.location.host + "/" + URL_PostImage);
            var pages = JSON.parse(s);
            importTemplate(pages, insOption);
        }
    }
    catch (e) {
        console.log(e);
    }
}