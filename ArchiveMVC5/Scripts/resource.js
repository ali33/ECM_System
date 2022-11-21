
var RMenuCapture = {
    en: {
        'import'    : 'Import',
        'scan'      : 'Scan',
        'camera'      : 'Camera',
        'delete'    : 'Delete',
        'classifyLater': 'Classify later',
        'newDocFromPage': 'New document from selected pages',
        'contentLang': 'Content language setting',
        'newDocHere': 'New document starting here',
        'replace': 'Replace',
        'insertBefore': 'Insert before',
        'insertAfter': 'Insert after',
        'eng': 'English',
        'vie': 'Vietnamese',
        'rotateLeft': 'Rotate left',
        'rotateRight': 'Rotate right',
        'index': 'Index',
        'changeContent' : 'Change content type',
        'append': 'Append',
        print: 'Print',
        mail: 'Send email',
        save: 'Save',
        hide: 'Hide all annotation',
        highlight: 'High light',
        redaction: 'Redaction',
        note: "Add text",
        previous: "Previous",
        next: "Next",
        zoomIn: "Zoom In",
        zoomOut: "Zoom Out",
        fitViewer: "Fit Viewer",
        fitWidth: "Fit Width",
        fitHeight: "Fit Height"
    },
    vi: {
        'import': 'Nhập',
        'scan': 'Quét',
        'camera':'Camera',
        'delete': 'Xóa'
    }
}
var command = {
    'imp': 'import',
    'sca': 'scan',
    'cam': 'cam',
    'del': 'delete',
    'classifyLater': 'classifyLater'
}

RMenuCapture = RMenuCapture['en'];

var guid = (function () {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
                   .toString(16)
                   .substring(1);
    }
    return function () {
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
               s4() + '-' + s4() + s4() + s4();
    };
})();
