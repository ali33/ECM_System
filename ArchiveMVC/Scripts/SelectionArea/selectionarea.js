//################################################################ 
//# Copyright (C) 2008-2013, Innori Solution. All Rights Reserved. 
//# 
//# History: 
//# Date Time   Updater     Comment 
//# 01/08/2013  ThoDinh     Thêm mới
//# 23/08/2013  ThoDinh     Cập nhật:thêm hàm loadImage
//# 27/08/2013   ThoDinh     Cap nhat function $.imgAreaSelect thanh imgAreaSelect, 
//#                         chuyen vao ben trong function $.selectionArea
//# 13/09/2013   ThoDinh     Cap nhat context menu cho cac Annotations
//################################################################
(function($) {
    
    /**
     * Hàm lấy giá trị width của scrollbar
     * @returns {unresolved}
     */
    function scrollbarWidth() {
        var outer = document.createElement("div");
        outer.style.visibility = "hidden";
        outer.style.width = "100px";
        document.body.appendChild(outer);

        var widthNoScroll = outer.offsetWidth;
        // force scrollbars
        outer.style.overflow = "scroll";

        // add innerdiv
        var inner = document.createElement("div");
        inner.style.width = "100%";
        outer.appendChild(inner);        

        var widthWithScroll = inner.offsetWidth;

        // remove divs
        outer.parentNode.removeChild(outer);

        return widthNoScroll - widthWithScroll;
    }

    function GetURLParameter(sParam, url) {
        var sPageURL = url.substring(url.indexOf('?') + 1);
        var sURLVariables = sPageURL.split('&');
        for (var i = 0; i < sURLVariables.length; i++) {
            var sParameterName = sURLVariables[i].split('=');
            if (sParameterName[0] == sParam) {
                return sParameterName[1];
            }
        }
    }

    function SetURLParameter(sParam, value, url) {
        var newURL = url.substr(0, url.indexOf('?')) + "?";
        var sPageURL = url.substring(url.indexOf('?') + 1);
        var sURLVariables = sPageURL.split('&');
        for (var i = 0; i < sURLVariables.length; i++) {
            var sParameterName = sURLVariables[i].split('=');
            if (sParameterName[0] == sParam) {
                newURL += sParameterName[0] + "=" + value;
            } else
                newURL += sParameterName[0] + "=" + sParameterName[1];
            if ((i + 1) < sURLVariables.length)
                newURL += "&";
        }
        return newURL;
    }
    var
         /* Định nghĩa loại hiện tại của rectangle*/
         //globalType,
         /* Định nghĩa các loại rectangle*/
         types = ["OCRZone", "Redaction", "Highlight", "Text"],
         /* Số lượng rectangle, đồng thời là id của rectangle kế tiếp*/
         autoId = 0,
        /* Rectangle đang được focus*/
        currentRect,
        // delta của zoom
        deltaZoom = 0.1,
        //giá trị của width, height image ban đầu
        originalWidth,
        originalHeight,
        // hệ số zoom
        zoom = 1,
        // hệ số zoom trước đó
        preZoom,
        pan = false,
        listField = "",
        arrayListField;
    /**
     * Lớp SelectionArea cho phép chọn vùng bằng các rectangle 
     *      dựa vào thư viện imgAreaSelect
     * @param {type} area là đối tượng HTML, vùng cần chọn
     * @returns {unresolved}
     */
    $.selectionArea = function (area, onSelectEnd) {
        
        var
           /* Mảng chứa các rectangle*/
           rects = [],
           globalType,
           /* Cho phép tiếp tục vẽ hay không!*/
           enable = true;
        var
            /* area la nội dung html, $area là selector của vùng area*/
            $area = $(area),
            $p = $area.parent();        

        /* Enum options context menu on Annotations */
        var optMenu = {
            Admin: 0, Archive: 1
        };
        this.optMenu = optMenu;
        function createCanvas(canvas) {
            rects[rects.length] = new imgAreaSelect(canvas, {
                enable: true,
                parent: $(canvas).parent(),
                onSelectEnd: onSelectEnd ? onSelectEnd : function () { }
            });
            autoId++;
        }
        
        /** @author Tho
         * Tao context menu cho rectangle
         * @returns {undefined}
         */
        function registerContextMenu() {
            $.contextMenu({
                selector: '.Redaction,.OCRZone,.Highlight,.Text',
                callback: function (key, options) {
                    if (key == "delete") {
                        currentRect.remove();
                        
                    } else if (key == "quit") {
                        console.log("quit");
                    } else {
                        console.log(currentRect);
                        currentRect.setType(key);
                    }
                },
                items: {
                    "OCRZone": { name: "Text" },
                    "Redaction": { name: "Redaction" },
                    "Highlight": { name: "High light" },
                    "Text": { name: "Comment" },
                    "sep1": "---------",
                    "delete": { name: "Delete" }
                },
                style: { fontSize: 14, height: 30 }
            });
        };
        
        this.setAnnotationContextMenu = function (context) {
            var _item = {};
            if (context == optMenu.Admin) {
                _item = {
                    "OCRZone": { name: "Text" },
                    "Redaction": { name: "Redaction" },
                    "Highlight": { name: "High light" },
                    "Text": { name: "Comment" },
                    "sep1": "---------",
                    "delete": { name: "Delete" }
                }
            }
            else {
                _item = {
                    "Redaction": { name: "Redaction" },
                    "Highlight": { name: "High light" },
                    "Text": { name: "Comment" },
                    "sep1": "---------",
                    "delete": { name: "Delete" }
                }
            }
            $.contextMenu({
                selector: '.Redaction,.OCRZone,.Highlight,.Text',
                callback: function (key, options) {
                    if (key == "delete") {
                        currentRect.remove();
                    } else {
                        currentRect.setType(key);
                    }
                },
                items: _item,
                style: { fontSize: 14, height: 30 }
            });
        }

        /*
         * Lấy danh sách các rectangle dưới dạng json,
         * @returns {@exp;JSON@call;stringify}
         */
        function getRectsAsJSON(){
            var objs={"rects":[]};
            for (i = 0; i < rects.length; i++) {
                if (!rects[i].isEmpty())
                    objs.rects.push(rects[i].getOriginalSetting());
            }
            objs.listField = arrayListField;
            objs.page = { 'origWidth': originalWidth, 'origHeight': originalHeight };
            return JSON.stringify(objs);
        };
        

        function getRectsAsArray() {
            var objs = [];
            for (i = 0; i < rects.length; i++) {
                if (!rects[i].isEmpty())
                    objs.push(rects[i].getOriginalSetting2());
            }
            return objs;
        };

        this.getAnnotations = getRectsAsArray;
        var ocr;
        this.displayOCRZone = function (setting) {
            //if (ocr != undefined)
            //    ocr.remove();
            x2 = parseFloat(setting.left) + parseFloat(setting.width)
            y2 = parseFloat(setting.top) + parseFloat(setting.height);
            if (ocr)
                ocr.remove();
            ocr = new imgAreaSelect(area, {
                enable: false,
                x1: setting.left * zoom,
                y1: setting.top * zoom,
                x2: x2 * zoom,
                y2: y2 * zoom,
                type: types[0],
                field: setting.field,
                parent: $(area).parent()
            });
            $('.empty').remove();
        }
        this.clearOCRZone = function () {
            if (ocr)
                ocr.remove();
            createCanvas(area);
        }
        /*
         * Vẽ lại các rectangle từ một json
         * @param {type} jsonRects
         */
        function setRectsFromJSON(jsonRects) {
            removeAll();
            if (jsonRects != undefined) {
                if (jsonRects.listField != undefined)
                    this.setList(jsonRects.listField);
                rects = [];
                autoId = 0;
                if (jsonRects != undefined) {
                    for (i = 0; i < jsonRects.rects.length; i++) {
                        //$area.remove();
                        //$p.append($area);
                        restoreCanvas(jsonRects.rects[i]);
                    }
                    //rects[jsonRects.rects.length].unbind();
                }
            }
            if (globalType) {
                createCanvas(area);
            }
        };
        createCanvas(area);
        /*
         * Vẽ một rectangle từ tham số setting
         * @param {type} setting
         */
        function restoreCanvas(setting) {
            rects[rects.length] = new imgAreaSelect(area,{
                enable:true,
                x1: setting.selection.x1 * zoom,
                y1: setting.selection.y1 * zoom,
                x2: setting.selection.x2 * zoom,
                y2: setting.selection.y2 * zoom,
                type:setting.type,
                field:setting.field,
                text:setting.text,
                parent:$(area).parent()
            });
            autoId++;
        }
        
        /*
         * Xóa các rectangle hiện tại 
         * và rectangle empty (rectangle đang chuẩn bị vẽ)
         */
        function removeAll(){
            //for (i = 0; i < rects.length; i++) {
            while (rects.length) {
                rects[0].remove();
            }
        };
        this.removeAll = removeAll;
        /*
         * Gán kiểu hiện tại cho các rectangle, khi vẽ các rectangle tiếp theo, 
         * type mặc định sẽ là type này
         * 
         * @param _type: kiểu số = {0,1,2,3} 
         *          hoặc chuỗi = {text,annotation,highlight,comment}
         */
        function setGlobalType(_type){
            enable = true;
            pan = false;
            $area.css('cursor','default');
            if($.isNumeric(_type))
                globalType = types[_type];
            else
                globalType = _type;
        }
        this.getCurrentType = function () {
            return globalType;
        }
        this.setList = function(list){
            arrayListField = list;
            listField = '<option></option>';
            $.each(list,function(){
                listField += "<option>" + this + "</option>";
            });
            $.each(rects,function(){
                this.resetSelection();    
            });
        };
        // Public API
        this.enable = function(){
            enable = true;
        };
        this.disable = function(){
            enable = false;
        };
        this.setGlobalType = setGlobalType;
        this.getRectsAsJSON = getRectsAsJSON;
        this.setRectsFromJSON = setRectsFromJSON;
        var isHide = false;
        this.hideOrShow = function(){
            if(!isHide){
                for (i=0; i<rects.length; i++){
                    rects[i].hide();
                }
                isHide = true;
            }else{
                for (i=0; i<rects.length; i++){
                    rects[i].show();
                }
                isHide = false;
            }
        };
        var rotate = 0;
        this.rotateClockwise = function (){
            rotate++;
            rotate = (rotate % 4);
            var urlSrc = $area.attr("src");
            
            var src = rotate == 1 || rotate == -3 ? SetURLParameter('rote', "1", urlSrc) :
                      rotate == 2 || rotate == -2 ? SetURLParameter('rote', "2", urlSrc) :
                      rotate == 3 || rotate == -1 ? SetURLParameter('rote', "3", urlSrc) :
                                                    SetURLParameter('rote', "0", urlSrc);
            $area.attr("src", src);
			if(rotate%2==0)
				$area.css({width:originalWidth*zoom,height:originalHeight*zoom});
			else
				$area.css({width:originalHeight*zoom,height:originalWidth*zoom});
            for (i=0;i<rects.length;i++)
                rects[i].rotate(90);
            if(ocr)
                ocr.rotate(90);
            return src;
		};
        this.rotateCounterClockwise = function () {
            rotate--;
            rotate = (rotate % 4);
            var urlSrc = $area.attr("src");
            
            var src = rotate == 1 || rotate == -3 ? SetURLParameter('rote', "1", urlSrc) :
                      rotate == 2 || rotate == -2 ? SetURLParameter('rote', "2", urlSrc) :
                      rotate == 3 || rotate == -1 ? SetURLParameter('rote', "3", urlSrc) :
                                                    SetURLParameter('rote', "0", urlSrc);
            console.log(src);
            $area.attr("src", src);
            if (rotate % 2 == 0)
                $area.css({ width: originalWidth * zoom, height: originalHeight * zoom });
            else
                $area.css({ width: originalHeight * zoom, height: originalWidth * zoom });
            for (i = 0; i < rects.length; i++)
                rects[i].rotate(-90);
            if (ocr)
                ocr.rotate(-90);
            this.log();
            return src;
        };
        var w, h;
        this.log = function () {
            console.log("deltaZoom:[" + deltaZoom + "] - originalWidth:[" + originalWidth + "] - originalHeight:[" + originalHeight + "]-zoom:[" + zoom + "]-rotate:[" + (rotate) + "]");
        }
        this.zoomIn = function(){
            preZoom = zoom;
            zoom = Number(parseFloat(zoom + deltaZoom).toFixed(1));
            //Fix zoom width rotate
            //Code Ole:$area.css({width:originalWidth*zoom,height:originalHeight*zoom});
            if (rotate % 2 == 0)
                $area.css({ width: originalWidth * zoom, height: originalHeight * zoom });
            else
                $area.css({ width: originalHeight * zoom, height: originalWidth * zoom });
            //EndFix zoom width rotate
            for (i=0;i<rects.length;i++){
                rects[i].zoom();
            }
            if (ocr)
                ocr.zoom();
            this.log();
            
        };
        this.zoomOut = function(){
            preZoom = zoom;
            zoom = Number(parseFloat(zoom - deltaZoom).toFixed(1));
            //Fix zoom width rotate
            //Code Ole:$area.css({width:originalWidth*zoom,height:originalHeight*zoom});
            if (rotate % 2 == 0)
                $area.css({ width: originalWidth * zoom, height: originalHeight * zoom });
            else
                $area.css({ width: originalHeight * zoom, height: originalWidth * zoom });
            //EndFix zoom width rotate
            for (i=0;i<rects.length;i++){
                rects[i].zoom();
            }
            if (ocr)
                ocr.zoom();
            this.log();
        };
        this.pan = function(){
            $area.css('cursor','hand');
            enable = false;
            pan = true;
        };
        this.loadImage = function (url) {
            $(area).attr('src',url);
        }
        ////////////////////////////////////////////////////////////////////////////////////////////


        /*********************Tao rectangle****************************************/
        /* Lớp imgAreaSelect dùng để vẽ các rectangle
         * @param img 
         *      Thẻ HTML, vùng để vẽ các rectangle
         * @param options 
         *      Các options
         */
        function imgAreaSelect(img, options) {
            var
                /* jQuery object representing the image */
                $img = $(img),
                /* Has the image finished loading? */
                imgLoaded,
                /* Plugin elements */
                /* Container box */
                $box = div(),
                /* Selection area */
                $area = div(),
                /* Border (four divs) */
                $border = div().add(div()).add(div()).add(div()),
                /* Outer area (four divs) */
                $outer = div().add(div()).add(div()).add(div()),
                /* Handles (empty by default, initialized in setOptions()) */
                $handles = $([]),
                /*
                 * Additional element to work around a cursor problem in Opera
                 * (explained later)
                 */
                $areaOpera,
                /* Image position (relative to viewport) */
                left, top,
                /* Image offset (as returned by .offset()) */
                imgOfs = { left: 0, top: 0 },
                /* Image dimensions (as returned by .width() and .height()) */
                imgWidth, imgHeight,
                /*
                 * jQuery object representing the parent element that the plugin
                 * elements are appended to
                 */
                $parent,
                /* Parent element offset (as returned by .offset()) */
                parOfs = { left: 0, top: 0 },
                /* Base z-index for plugin elements */
                zIndex = 0,
                /* Plugin elements position */
                position = 'absolute',
                /* X/Y coordinates of the starting point for move/resize operations */
                startX, startY,
                /* Horizontal and vertical scaling factors */
                scaleX, scaleY,
                /* Current resize mode ("nw", "se", etc.) */
                resize,
                /* Selection area constraints */
                minWidth, minHeight, maxWidth, maxHeight,
                /* As, minHeight, maxWidth, maxHeight,pect ratio to maintain (floating point number) */
                aspectRatio,
                /* Are the plugin elements currently displayed? */
                shown,
                /* Current selection (relative to parent element) */
                x1, y1, x2, y2,
                /* Current selection (relative to scaled image) */
                selection = { x1: 0, y1: 0, x2: 0, y2: 0, width: 0, height: 0 },
                /* Document element */
                docElem = document.documentElement,
                /* User agent */
                ua = navigator.userAgent,
                /* Various helper variables used throughout the code */
                $p, d, i, o, w, h, adjusted;
            //Tho
            var id, classType, field, text,
                $input = $('<textarea class="editor"/>'),
                $barInput = $('<div class="bar-editor"></div>'),
                $combox = $("<select class='combox'>").append($(listField));
            function inputMouseEvent() {
                $input.bind("mousedown", function () {
                    focusMe();
                });
                $input.bind('input propertychange', function () {
                    text = $input.val();
                });
                $input.bind("mousemove", function () {
                    $box.unbind('mousedown', areaMouseDown);
                });
                $input.bind("mouseleave", function () {
                    $box.bind('mousedown', areaMouseDown);
                });
            }
            this.box = $box;

            function comboxMouseEvent() {
                $combox.bind('mousemove', function () {
                    $box.unbind('mousedown', areaMouseDown);
                });
                $combox.bind("mouseleave", function () {
                    $box.bind('mousedown', areaMouseDown);

                });
                $combox.bind("mousedown", function () {
                    $combox.css('opacity', '1');
                });
                $combox.find('option').bind('click', function () {
                    $combox.css('opacity', '0.8');
                });
                $combox.change(function () {
                    var selected = $combox.find("option:selected");
                    field = selected.val();
                    console.log(checkFieldError());
                    $combox.css('opacity', '0.8');
                });
                $box.bind('mouseleave', function () {
                    //            if(!hideField){
                    $combox.css('display', 'none');
                    //            }
                    $combox.css('opacity', '0.8');
                });
            }
            var abs = Math.abs,
                max = Math.max,
                min = Math.min,
                round = Math.round;
            text = options.text;
            /**
             * Create a new HTML div element
             * 
             * @return A jQuery object representing the new element
             */
            function div() {
                return $('<div/>');
            }
            /** @author Tho
             * Đổi type cho rectangle
             * @param {type} type
             */
            setType = function (_type) {
                classType = _type;
                $box.attr("class", _type);
                if (classType == types[3]) {
                    $input.val(text);
                    $box.append($barInput);
                    $box.append($input);
                    $combox.remove();
                    inputMouseEvent();
                    $input.width(w = selection.width).height(h = selection.height);
                    field = "";
                }
                else if (classType == types[0]) {
                    $barInput.remove();
                    $input.remove();
                    text = "";
                    $box.append($combox);
                    comboxMouseEvent();
                    $combox.width(w = selection.width - 3);
                }
                else {
                    $barInput.remove();
                    $input.remove();
                    $combox.remove();
                    text = "";
                    field = "";
                }
            };

            if (options.type != undefined) {
                setType(options.type);
                if (options.type == types[0] && options.field != undefined) {
                    field = options.field;
                    $combox.val(options.field);
                }
            } else {
                setType("empty");
            }

            id = autoId;
            
            //$box.attr("id", id);
            /*
             * Translate selection coordinates (relative to scaled image) to viewport
             * coordinates (relative to parent element)
             */

            /**
             * Translate selection X to viewport X
             * 
             * @param x
             *            Selection X
             * @return Viewport X
             */
            function viewX(x) {
                return x + imgOfs.left - parOfs.left;
            }

            /**
             * Translate selection Y to viewport Y
             * 
             * @param y
             *            Selection Y
             * @return Viewport Y
             */
            function viewY(y) {
                return y + imgOfs.top - parOfs.top;
            }

            /*
             * Translate viewport coordinates to selection coordinates
             */

            /**
             * Translate viewport X to selection X
             * 
             * @param x
             *            Viewport X
             * @return Selection X
             */
            function selX(x) {
                return x - imgOfs.left + parOfs.left;
            }

            /**
             * Translate viewport Y to selection Y
             * 
             * @param y
             *            Viewport Y
             * @return Selection Y
             */
            function selY(y) {
                return y - imgOfs.top + parOfs.top;
            }

            /*
             * Translate event coordinates (relative to document) to viewport
             * coordinates
             */

            /**
             * Get event X and translate it to viewport X
             * 
             * @param event
             *            The event object
             * @return Viewport X
             */
            function evX(event) {
                return event.pageX - parOfs.left;
            }

            /**
             * Get event Y and translate it to viewport Y
             * 
             * @param event
             *            The event object
             * @return Viewport Y
             */
            function evY(event) {
                return event.pageY - parOfs.top;
            }

            /**
             * Get the current selection
             * 
             * @param noScale
             *            If set to <code>true</code>, scaling is not applied to the
             *            returned selection
             * @return Selection object
             */
            function getSelection(noScale) {
                var sx = noScale || scaleX, sy = noScale || scaleY;

                return {
                    x1: round(selection.x1 * sx),
                    y1: round(selection.y1 * sy),
                    x2: round(selection.x2 * sx),
                    y2: round(selection.y2 * sy),
                    width: round(selection.x2 * sx) - round(selection.x1 * sx),
                    height: round(selection.y2 * sy) - round(selection.y1 * sy)
                };
            }

            /**
             * Set the current selection
             * 
             * @param x1
             *            X coordinate of the upper left corner of the selection area
             * @param y1
             *            Y coordinate of the upper left corner of the selection area
             * @param x2
             *            X coordinate of the lower right corner of the selection area
             * @param y2
             *            Y coordinate of the lower right corner of the selection area
             * @param noScale
             *            If set to <code>true</code>, scaling is not applied to the
             *            new selection
             */
            function setSelection(x1, y1, x2, y2, noScale) {
                var sx = noScale || scaleX, sy = noScale || scaleY;

                selection = {
                    x1: round(x1 / sx || 0),
                    y1: round(y1 / sy || 0),
                    x2: round(x2 / sx || 0),
                    y2: round(y2 / sy || 0)
                };
                //Tho 
                selection.width = selection.x2 - selection.x1;
                selection.height = selection.y2 - selection.y1;
            }

            /**
             * Recalculate image and parent offsets
             */
            function adjust(rotate) {
                /*
                 * Do not adjust if image has not yet loaded or if width is not a
                 * positive number. The latter might happen when imgAreaSelect is put
                 * on a parent element which is then hidden.
                 */
                if (!imgLoaded || !$img.width())
                    return;

                /*
                 * Get image offset. The .offset() method returns float values, so they
                 * need to be rounded.
                 */
                imgOfs = { left: round($img.offset().left), top: round($img.offset().top) };
                //            console.log(imgOfs);
                /* Get image dimensions */
                imgWidth = $img.innerWidth();
                imgHeight = $img.innerHeight();

                imgOfs.top += ($img.outerHeight() - imgHeight) >> 1;
                imgOfs.left += ($img.outerWidth() - imgWidth) >> 1;
                if (rotate % 360 === 90) {
                    imgOfs = { left: round($img.offset().top), top: round($img.offset().left) };
                    imgWidth = $img.innerHeight();
                    imgHeight = $img.innerWidth();

                    imgOfs.top += ($img.outerWidth() - imgWidth) >> 1;
                    imgOfs.left += ($img.outerHeight() - imgHeight) >> 1;
                }
                /* Set minimum and maximum selection area dimensions */
                minWidth = round(options.minWidth / scaleX) || 0;
                minHeight = round(options.minHeight / scaleY) || 0;
                maxWidth = round(min(options.maxWidth / scaleX || 1 << 24, imgWidth));
                maxHeight = round(min(options.maxHeight / scaleY || 1 << 24, imgHeight));

                /*
                 * Workaround for jQuery 1.3.2 incorrect offset calculation, originally
                 * observed in Safari 3. Firefox 2 is also affected.
                 */
                if ($().jquery == '1.3.2' && position == 'fixed' &&
                    !docElem['getBoundingClientRect']) {
                    imgOfs.top += max(document.body.scrollTop, docElem.scrollTop);
                    imgOfs.left += max(document.body.scrollLeft, docElem.scrollLeft);
                }

                /* Determine parent element offset */
                parOfs = /absolute|relative/.test($parent.css('position')) ?
                {
                    left: round($parent.offset().left) - $parent.scrollLeft(),
                    top: round($parent.offset().top) - $parent.scrollTop()
                } :
                    position == 'fixed' ?
                    { left: $(document).scrollLeft(), top: $(document).scrollTop() } :
                    { left: 0, top: 0 };
                if (rotate % 360 === 90) {
                    parOfs = /absolute|relative/.test($parent.css('position')) ?
                {
                    left: round($parent.offset().top) - $parent.scrollTop(),
                    top: round($parent.offset().left) - $parent.scrollLeft()
                } :
                    position == 'fixed' ?
                    { left: $(document).scrollTop(), top: $(document).scrollLeft() } :
                    { left: 0, top: 0 };
                    console.log("90sss");
                }
                left = viewX(0);
                top = viewY(0);

                /*
                 * Check if selection area is within image boundaries, adjust if
                 * necessary
                 */
                if (selection.x2 > imgWidth || selection.y2 > imgHeight)
                    doResize();
            }

            /**
             * Update plugin elements
             * 
             * @param resetKeyPress
             *            If set to <code>false</code>, this instance's keypress
             *            event handler is not activated
             */
            function update(resetKeyPress) {
                /* If plugin elements are hidden, do nothing */
                if (!shown) return;

                /*
                 * Set the position and size of the container box and the selection area
                 * inside it
                 */
                $box.css({ left: viewX(selection.x1), top: viewY(selection.y1) })
                    .add($area).width(w = selection.width).height(h = selection.height);
                //Nếu classType là comment-area,h=height-23,23 la chieu cao cua phan bar
                if (classType == types[3])
                    $input.width(w = selection.width - 3).height(h = selection.height - 23);
                else
                    $combox.width(w = selection.width - 3);
                /*
                 * Reset the position of selection area, borders, and handles (IE6/IE7
                 * position them incorrectly if we don't do this)
                 */
                //Tho
                //$area.add($border).add($handles).css({ left: 0, top: 0 });
                $area.add($border).add($handles).css({ left: 0, top: 0 });
                /* Set border dimensions */
                $border
                    .width(max(w - $border.outerWidth() + $border.innerWidth(), 0))
                    .height(max(h - $border.outerHeight() + $border.innerHeight(), 0));
                //Tho: Bỏ outer và handles

                if (resetKeyPress !== false) {
                    /*
                     * Need to reset the document keypress event handler -- unbind the
                     * current handler
                     */
                    if (imgAreaSelect.onKeyPress != docKeyPress)
                        $(document).unbind(imgAreaSelect.keyPress,
                            imgAreaSelect.onKeyPress);

                    if (options.keys)
                        /*
                         * Set the document keypress event handler to this instance's
                         * docKeyPress() function
                         */
                        $(document)[imgAreaSelect.keyPress](
                            imgAreaSelect.onKeyPress = docKeyPress);
                }

                /*
                 * Internet Explorer displays 1px-wide dashed borders incorrectly by
                 * filling the spaces between dashes with white. Toggling the margin
                 * property between 0 and "auto" fixes this in IE6 and IE7 (IE8 is still
                 * broken). This workaround is not perfect, as it requires setTimeout()
                 * and thus causes the border to flicker a bit, but I haven't found a
                 * better solution.
                 * 
                 * Note: This only happens with CSS borders, set with the borderWidth,
                 * borderOpacity, borderColor1, and borderColor2 options (which are now
                 * deprecated). Borders created with GIF background images are fine.
                 */
                if (msie && $border.outerWidth() - $border.innerWidth() == 2) {
                    $border.css('margin', 0);
                    setTimeout(function () { $border.css('margin', 'auto'); }, 0);
                }
            }

            /**
             * Do the complete update sequence: recalculate offsets, update the
             * elements, and set the correct values of x1, y1, x2, and y2.
             * 
             * @param resetKeyPress
             *            If set to <code>false</code>, this instance's keypress
             *            event handler is not activated
             */
            function doUpdate(resetKeyPress) {
                adjust();
                update(resetKeyPress);
                x1 = viewX(selection.x1); y1 = viewY(selection.y1);
                x2 = viewX(selection.x2); y2 = viewY(selection.y2);
            }

            /**
             * Hide or fade out an element (or multiple elements)
             * 
             * @param $elem
             *            A jQuery object containing the element(s) to hide/fade out
             * @param fn
             *            Callback function to be called when fadeOut() completes
             */
            function hide($elem, fn) {
                options.fadeSpeed ? $elem.fadeOut(options.fadeSpeed, fn) : $elem.hide();
            }

            /**
             * Selection area mousemove event handler
             * 
             * @param event
             *            The event object
             */
            function areaMouseMove(event) {

                var x = selX(evX(event)) - selection.x1,
                    y = selY(evY(event)) - selection.y1;
                //                console.log(adjusted);
                if (!adjusted) {
                    adjust();
                    adjusted = true;

                    $box.one('mouseout', function () { adjusted = false; });
                }

                /* Clear the resize mode */
                resize = '';
                if (options.resizable) {
                    /*
                     * Check if the mouse pointer is over the resize margin area and set
                     * the resize mode accordingly
                     */
                    if (y <= options.resizeMargin)
                        resize = 'n';
                    else if (y >= selection.height - options.resizeMargin)
                        resize = 's';
                    if (x <= options.resizeMargin)
                        resize += 'w';
                    else if (x >= selection.width - options.resizeMargin)
                        resize += 'e';
                }
                if (!resize) {
                    if (classType === types[0]) {
                        $combox.css('display', 'block');
                    }
                }
                $box.css('cursor', resize ? resize + '-resize' :
                    options.movable ? 'move' : '');
                if ($areaOpera)
                    $areaOpera.toggle();
            }

            /**
             * Document mouseup event handler
             * 
             * @param event
             *            The event object
             */
            function docMouseUp(event) {
                /* Set back the default cursor */
                $('body').css('cursor', '');
                /*
                 * If autoHide is enabled, or if the selection has zero width/height,
                 * hide the selection and the outer area
                 * Neu autoHide hoac width/height =0 thi remove
                 */
                if (options.autoHide || selection.width * selection.height == 0) {
                    //hide($box.add($outer), function () { $(this).hide(); });
                    //Tho
                    $box.add($outer).remove();
                }
                $(document).unbind('mousemove', selectingMouseMove);
                $box.mousemove(areaMouseMove);

                options.onSelectEnd(img, getSelection());

                /** @author Tho
                 * Xóa div của img sau đó lại thêm $img vào để tạo một vùng để vẽ mới
                 * gọi hàm createCanvas cho $img mới để nó gán imgAreaSelect cho $img mới
                 */
                if (!resize) {
                    unbind();
                    
                    //$img.remove();
                    //$parent.append($img);
                    createCanvas(img);
                }
            }
            function unbind() {
                $img.remove();
                $parent.append($img);
            }
            this.unbind = unbind;
            //Tho
            $parent = $img.parent();
            //        console.log($parent.width());
            //        $parent.css({width:imgWidth,height:imgHeight});
            /**
             * Selection area mousedown event handler
             * 
             * @param event
             *            The event object
             * @return false
             */
            function areaMouseDown(event) {
                /** @author Tho
                 * Thêm sự kiện nhấn chuột phải: 
                 *      focus rectangle được nhấn chuột phải 
                 *      và gán currentRect = rect đó
                 */
                if (event.which == 3) {
                    focusMe();
                    console.log("Right click");
                    return false;
                }
                if (event.which != 1) return false;
                adjust();

                if (resize) {
                    console.log("Area Mouse Down Resize");
                    /* Resize mode is in effect */
                    $('body').css('cursor', resize + '-resize');

                    x1 = viewX(selection[/w/.test(resize) ? 'x2' : 'x1']);
                    y1 = viewY(selection[/n/.test(resize) ? 'y2' : 'y1']);

                    $(document).mousemove(selectingMouseMove)
                        .one('mouseup', docMouseUp);
                    $box.unbind('mousemove', areaMouseMove);
                }
                else if (options.movable) {
                    console.log("Area Mouse Down Movable");
                    
                    focusMe();
                    //left + selection.x1 - event.pageX + parent.left
                    startX = left + selection.x1 - evX(event);
                    startY = top + selection.y1 - evY(event);

                    $box.unbind('mousemove', areaMouseMove);

                    $(document).mousemove(movingMouseMove)
                        .one('mouseup', function () {
                            options.onSelectEnd(img, getSelection());

                            $(document).unbind('mousemove', movingMouseMove);
                            $box.mousemove(areaMouseMove);
                        });
                }
                else
                    $img.mousedown(event);

                return false;
            }

            /**
             * Adjust the x2/y2 coordinates to maintain aspect ratio (if defined)
             * 
             * @param xFirst
             *            If set to <code>true</code>, calculate x2 first. Otherwise,
             *            calculate y2 first.
             */
            function fixAspectRatio(xFirst) {
                if (aspectRatio)
                    if (xFirst) {
                        x2 = max(left, min(left + imgWidth,
                            x1 + abs(y2 - y1) * aspectRatio * (x2 > x1 || -1)));
                        y2 = round(max(top, min(top + imgHeight,
                            y1 + abs(x2 - x1) / aspectRatio * (y2 > y1 || -1))));
                        x2 = round(x2);
                    }
                    else {
                        y2 = max(top, min(top + imgHeight,
                            y1 + abs(x2 - x1) / aspectRatio * (y2 > y1 || -1)));
                        x2 = round(max(left, min(left + imgWidth,
                            x1 + abs(y2 - y1) * aspectRatio * (x2 > x1 || -1))));
                        y2 = round(y2);
                    }
            }

            /**
             * Resize the selection area respecting the minimum/maximum dimensions and
             * aspect ratio
             */
            function doResize() {
                /*
                 * Make sure the top left corner of the selection area stays within
                 * image boundaries (it might not if the image source was dynamically
                 * changed).
                 */
                x1 = min(x1, left + imgWidth);
                y1 = min(y1, top + imgHeight);

                if (abs(x2 - x1) < minWidth) {
                    /* Selection width is smaller than minWidth */
                    x2 = x1 - minWidth * (x2 < x1 || -1);

                    if (x2 < left)
                        x1 = left + minWidth;
                    else if (x2 > left + imgWidth)
                        x1 = left + imgWidth - minWidth;
                }

                if (abs(y2 - y1) < minHeight) {
                    /* Selection height is smaller than minHeight */
                    y2 = y1 - minHeight * (y2 < y1 || -1);

                    if (y2 < top)
                        y1 = top + minHeight;
                    else if (y2 > top + imgHeight)
                        y1 = top + imgHeight - minHeight;
                }
                /**
                 * Gioi han khi ve ra ngoai vung cua parent
                 * @author Tho
                 */
                //            x2 = max(left, min(x2, left + imgWidth,
                //                               $parent.width() + $parent.offset().left - scrollbarWidth()));
                //            y2 = max(top, min(y2, top + imgHeight,
                //                               $parent.height() + $parent.offset().top - scrollbarWidth()));
                //            if(x2 < $parent.offset().left){
                //                x2 = $parent.offset().left;
                //            }
                //            if(y2 < $parent.offset().top){
                //                y2 = $parent.offset().top;
                //            }
                x2 = max(left, min(x2, left + imgWidth));
                y2 = max(top, min(y2, top + imgHeight));

                //            if(x2 > $parent.width() - scrollbarWidth()){
                //                $parent.scrollLeft(x2 - ($parent.width() - scrollbarWidth()));
                //            }
                //            if(x1 < (imgWidth - $parent.width())){
                //                $parent.scrollLeft(x1);
                //            }
                //            if(y2 > $parent.height() - scrollbarWidth()){
                //                $parent.scrollTop(y2 - ($parent.height() - scrollbarWidth()));
                //            }
                //            if(y1 < (imgHeight - $parent.height())){
                //                $parent.scrollTop(y1);
                //            }
                fixAspectRatio(abs(x2 - x1) < abs(y2 - y1) * aspectRatio);

                if (abs(x2 - x1) > maxWidth) {
                    /* Selection width is greater than maxWidth */
                    x2 = x1 - maxWidth * (x2 < x1 || -1);
                    fixAspectRatio();
                }

                if (abs(y2 - y1) > maxHeight) {
                    /* Selection height is greater than maxHeight */
                    y2 = y1 - maxHeight * (y2 < y1 || -1);
                    fixAspectRatio(true);
                }

                selection = {
                    x1: selX(min(x1, x2)), x2: selX(max(x1, x2)),
                    y1: selY(min(y1, y2)), y2: selY(max(y1, y2)),
                    width: abs(x2 - x1), height: abs(y2 - y1)
                };

                update();
                //            if(x2>$parent.offset().left+$parent.width())
                options.onSelectChange(img, getSelection());
            }

            /**
             * Mousemove event handler triggered when the user is selecting an area
             * 
             * @param event
             *            The event object
             * @return false
             */
            function selectingMouseMove(event) {
                x2 = /w|e|^$/.test(resize) || aspectRatio ? evX(event) : viewX(selection.x2);
                y2 = /n|s|^$/.test(resize) || aspectRatio ? evY(event) : viewY(selection.y2);

                doResize();

                return false;
            }

            /**
             * Move the selection area
             * 
             * @param newX1
             *            New viewport X1
             * @param newY1
             *            New viewport Y1
             */
            function doMove(newX1, newY1) {
                x2 = (x1 = newX1) + selection.width;
                y2 = (y1 = newY1) + selection.height;

                $.extend(selection, {
                    x1: selX(x1), y1: selY(y1), x2: selX(x2),
                    y2: selY(y2)
                });

                update();

                options.onSelectChange(img, getSelection());
            }

            /**
             * Mousemove event handler triggered when the selection area is being moved
             * 
             * @param event
             *            The event object
             * @return false
             */
            function movingMouseMove(event) {
                /**
                 * Sửa lại x1,y1 để không cho phép move ra khỏi vùng của parent
                 * @author Tho
                 */
                //            x1 = max($parent.offset().left, 
                //                    min(startX + evX(event), 
                //                        $parent.offset().left + $parent.width() 
                //                                - selection.width - scrollbarWidth(),
                //                        $parent.offset().left + imgWidth - selection.width));
                //            y1 = max($parent.offset().top, 
                //                    min(startY + evY(event), 
                //                        $parent.offset().top + $parent.height() 
                //                                - selection.height - scrollbarWidth(),
                //                        $parent.offset().top + imgHeight - selection.height));

                x1 = max(left, min(startX + evX(event), left + imgWidth - selection.width));
                y1 = max(top, min(startY + evY(event), top + imgHeight - selection.height));
                //            xx = x2 + ":" + ($parent.width() - scrollbarWidth());
                //            console.log(xx);
                //            if(x2 > $parent.width() - scrollbarWidth()){
                //                $parent.scrollLeft(x2 - ($parent.width() - scrollbarWidth()));
                //            }
                //            if(x1 < (imgWidth - $parent.width())){
                //                $parent.scrollLeft(x1);
                //            }
                //            if(y2 > $parent.height() - scrollbarWidth()){
                //                $parent.scrollTop(y2 - ($parent.height() - scrollbarWidth()));
                //            }
                //            if(y1 < (imgHeight - $parent.height())){
                //                $parent.scrollTop(y1);
                //            }
                doMove(x1, y1);
                event.preventDefault();
                return false;
            }

            /**
             * Start selection
             */
            function startSelection() {
                /** @author Tho
                 * Đặt class cho box laf globalType;
                 */
                if (globalType !== undefined)
                    setType(globalType);
                focusMe();
                //            if(classType == types[3]){
                //                $box.append($barInput);
                //                $box.append($input);
                //            }

                $(document).unbind('mousemove', startSelection);
                adjust();

                x2 = x1;
                y2 = y1;
                doResize();

                resize = '';

                if (!$outer.is(':visible'))
                    /* Show the plugin elements */
                    //Tho : Remove outer
                    //$box.add($outer).hide().fadeIn(options.fadeSpeed||0);
                    $box.hide().fadeIn(options.fadeSpeed || 0);

                shown = true;

                $(document).unbind('mouseup', cancelSelection)
                    .mousemove(selectingMouseMove).one('mouseup', docMouseUp);
                $box.unbind('mousemove', areaMouseMove);

                options.onSelectStart(img, getSelection());
            }

            /**
             * Cancel selection
             */
            function cancelSelection() {
                $(document).unbind('mousemove', startSelection)
                    .unbind('mouseup', cancelSelection);
                hide($box.add($outer));

                setSelection(selX(x1), selY(y1), selX(x1), selY(y1));

                /* If this is an API call, callback functions should not be triggered */
                if (!(this instanceof imgAreaSelect)) {
                    options.onSelectChange(img, getSelection());
                    options.onSelectEnd(img, getSelection());
                }
            }

            var dLeft, dTop;
            function dragEndScroll(e) {
                console.log("end");
                $(document).unbind('mousemove', dragScrolling)
                    .unbind('mouseup', dragEndScroll);
            }

            function dragStartScroll(e) {
                $(document).unbind('mousemove', dragStartScroll);
                //            console.log($parent.scrollTop());
                dLeft = $parent.scrollLeft();
                dTop = $parent.scrollTop();
                $(document).mousemove(dragScrolling).one('mouseup', docMouseUp);
            }

            function dragScrolling(e) {
                x2 = e.pageX;
                y2 = e.pageY;
                console.log(($parent.scrollTop() + ":" + (y1 - y2)));
                //            $parent.scrollLeft($parent.scrollLeft() + x1-x2).scrollTop($parent.scrollTop() + y1-y2);
                $parent.scrollLeft(dLeft + x1 - x2).scrollTop(dTop + y1 - y2);

            }


            /**
             * Image mousedown event handler
             * 
             * @param event
             *            The event object
             * @return false
             */
            function imgMouseDown(event) {
                /** @author Tho
                 * Thên kiểm tra điều kiện enable, 
                 *      phân biệt enable với options.enable,
                 *      enable là biến global
                 *      options.enable là options của imgAreaSelect
                 */
                //            removeAllFocus();
                if (enable) {

                    /* Ignore the event if animation is in progress */
                    if (event.which != 1 || $outer.is(':animated')) return false;

                    adjust();
                    startX = x1 = evX(event);
                    startY = y1 = evY(event);

                    /* Selection will start when the mouse is moved */
                    $(document).mousemove(startSelection).mouseup(cancelSelection);

                    return false;
                }
                if (pan == true) {
                    x1 = event.pageX;
                    y1 = event.pageY;
                    console.log((x1 + ":" + y1));
                    $(document).mousemove(dragStartScroll).mouseup(dragEndScroll);
                    return false;
                }
            }

            /**
             * Window resize event handler
             */
            function windowResize() {
                doUpdate(false);
            }

            /**
             * Image load event handler. This is the final part of the initialization
             * process.
             */
            function imgLoad() {
                imgLoaded = true;
                if (originalWidth === undefined) {
                    originalWidth = $img.width();
                    originalHeight = $img.height();
                }
                /* Set options */
                setOptions(options = $.extend({
                    classPrefix: 'imgareaselect',
                    movable: true,
                    parent: 'body',
                    resizable: true,
                    resizeMargin: 10,
                    onInit: function () { },
                    onSelectStart: function () { },
                    onSelectChange: function () { },
                    onSelectEnd: function () { }
                }, options));
                //Tho
                //$box.add($outer).css({ visibility: '' });
                $box.css({ visibility: '' });
                if (options.show) {
                    shown = true;
                    adjust();
                    update();
                    //Tho
                    //$box.add($outer).hide().fadeIn(options.fadeSpeed||0);
                    $box.hide().fadeIn(options.fadeSpeed || 0);
                }

                /*
                 * Call the onInit callback. The setTimeout() call is used to ensure
                 * that the plugin has been fully initialized and the object instance is
                 * available (so that it can be obtained in the callback).
                 */
                setTimeout(function () { options.onInit(img, getSelection()); }, 0);
            }

            /**
             * Document keypress event handler
             * 
             * @param event
             *            The event object
             * @return false
             */
            var docKeyPress = function (event) {
                var k = options.keys, d, t, key = event.keyCode;

                d = !isNaN(k.alt) && (event.altKey || event.originalEvent.altKey) ? k.alt :
                    !isNaN(k.ctrl) && event.ctrlKey ? k.ctrl :
                    !isNaN(k.shift) && event.shiftKey ? k.shift :
                    !isNaN(k.arrows) ? k.arrows : 10;

                if (k.arrows == 'resize' || (k.shift == 'resize' && event.shiftKey) ||
                    (k.ctrl == 'resize' && event.ctrlKey) ||
                    (k.alt == 'resize' && (event.altKey || event.originalEvent.altKey))) {
                    /* Resize selection */

                    switch (key) {
                        case 37:
                            /* Left */
                            d = -d;
                        case 39:
                            /* Right */
                            t = max(x1, x2);
                            x1 = min(x1, x2);
                            x2 = max(t + d, x1);
                            fixAspectRatio();
                            break;
                        case 38:
                            /* Up */
                            d = -d;
                        case 40:
                            /* Down */
                            t = max(y1, y2);
                            y1 = min(y1, y2);
                            y2 = max(t + d, y1);
                            fixAspectRatio(true);
                            break;
                        default:
                            return;
                    }

                    doResize();
                }
                else {
                    /* Move selection */

                    x1 = min(x1, x2);
                    y1 = min(y1, y2);

                    switch (key) {
                        case 37:
                            /* Left */
                            doMove(max(x1 - d, left), y1);
                            break;
                        case 38:
                            /* Up */
                            doMove(x1, max(y1 - d, top));
                            break;
                        case 39:
                            /* Right */
                            doMove(x1 + min(d, imgWidth - selX(x2)), y1);
                            break;
                        case 40:
                            /* Down */
                            doMove(x1, y1 + min(d, imgHeight - selY(y2)));
                            break;
                        default:
                            return;
                    }
                }

                return false;
            };

            /**
             * Apply style options to plugin element (or multiple elements)
             * 
             * @param $elem
             *            A jQuery object representing the element(s) to style
             * @param props
             *            An object that maps option names to corresponding CSS
             *            properties
             */
            function styleOptions($elem, props) {
                for (var option in props)
                    if (options[option] !== undefined)
                        $elem.css(props[option], options[option]);
            }

            /**
             * Set plugin options
             * 
             * @param newOptions
             *            The new options object
             */
            function setOptions(newOptions) {
                if (newOptions.parent)
                    //Tho
                    //($parent = $(newOptions.parent)).append($box.add($outer));
                    ($parent = $(newOptions.parent)).append($box);

                /* Merge the new options with the existing ones */
                $.extend(options, newOptions);

                adjust();

                /* Calculate scale factors */
                scaleX = options.imageWidth / imgWidth || 1;
                scaleY = options.imageHeight / imgHeight || 1;

                /* Set selection */
                if (newOptions.x1 != null) {
                    setSelection(newOptions.x1, newOptions.y1, newOptions.x2,
                        newOptions.y2);
                    newOptions.show = !newOptions.hide;
                    
                    
                }

                if (newOptions.keys)
                    /* Enable keyboard support */
                    options.keys = $.extend({ shift: 1, ctrl: 'resize' },
                        newOptions.keys);

                /* Add classes to plugin elements */
                $area.addClass(options.classPrefix + '-selection');
                //Tho Bỏ các $border

                /* Apply style options */
                styleOptions($area, {
                    selectionColor: 'background-color',
                    selectionOpacity: 'opacity'
                });

                /* Append all the selection area elements to the container box */
                //$box.append($area.add($border).add($areaOpera)).append($handles);
                $box.append($area.add($areaOpera)).append($handles);

                if (msie) {
                    if (o = ($border.css('filter') || '').match(/opacity=(\d+)/))
                        $border.css('opacity', o[1] / 100);
                }

                if (newOptions.hide)
                    hide($box.add($outer));
                else if (newOptions.show && imgLoaded) {
                    shown = true;
                    $box.add($outer).fadeIn(options.fadeSpeed || 0);
                    doUpdate();
                }

                /* Calculate the aspect ratio factor */
                aspectRatio = (d = (options.aspectRatio || '').split(/:/))[0] / d[1];

                $img.add($outer).unbind('mousedown', imgMouseDown);

                if (options.disable || options.enable === false) {
                    /* Disable the plugin */
                    $box.unbind('mousemove', areaMouseMove).unbind('mousedown', areaMouseDown);
                    $(window).unbind('resize', windowResize);
                }
                else {
                    if (options.enable || options.disable === false) {
                        /* Enable the plugin */
                        if (options.resizable || options.movable)
                            $box.mousemove(areaMouseMove).mousedown(areaMouseDown);

                        $(window).resize(windowResize);
                    }

                    if (!options.persistent)
                        $img.add($outer).mousedown(imgMouseDown);
                }
                if (newOptions.x1!=null) {
                    $img.remove();
                    $parent.append($img);
                }
                options.enable = options.disable = undefined;
            }

            this.clear = function () {
                /*
                 * Call setOptions with { disable: true } to unbind the event handlers
                 */
                $box.remove();
            };
            /*
             * Public API
             */

            /**
             * Get current options
             * 
             * @return An object containing the set of options currently in use
             */
            this.getOptions = function () { return options; };

            /**
             * Set plugin options
             * 
             * @param newOptions
             *            The new options object
             */
            this.setOptions = setOptions;

            /**
             * Get the current selection
             * 
             * @param noScale
             *            If set to <code>true</code>, scaling is not applied to the
             *            returned selection
             * @return Selection object
             */
            this.getSelection = getSelection;

            /**
             * Set the current selection
             * 
             * @param x1
             *            X coordinate of the upper left corner of the selection area
             * @param y1
             *            Y coordinate of the upper left corner of the selection area
             * @param x2
             *            X coordinate of the lower right corner of the selection area
             * @param y2
             *            Y coordinate of the lower right corner of the selection area
             * @param noScale
             *            If set to <code>true</code>, scaling is not applied to the
             *            new selection
             */
            this.setSelection = setSelection;

            /** @author Tho
             * Lấy các tham số cho rectangle để lưu lại
             * @returns {obj}
             */
            this.getSetting = function () {
                var obj = {};
                obj.id = id;
                obj.selection = {
                    x1: selection.x1,
                    y1: selection.y1,
                    x2: selection.x2,
                    y2: selection.y2,
                    width: selection.x2 - selection.x1,
                    height: selection.y2 - selection.y1
                };
                obj.type = classType;
                obj.field = field;
                if (classType === types[3])
                    obj.text = text;
                return obj;
            };

            this.setType = setType;
            /**
             * Cancel selection
             */
            this.cancelSelection = cancelSelection;

            /**
             * Update plugin elements
             * 
             * @param resetKeyPress
             *            If set to <code>false</code>, this instance's keypress
             *            event handler is not activated
             */
            this.update = doUpdate;

            /* Do the dreaded browser detection */
            var msie = (/msie ([\w.]+)/i.exec(ua) || [])[1],
                opera = /opera/i.test(ua),
                safari = /webkit/i.test(ua) && !/chrome/i.test(ua);

            /* 
             * Traverse the image's parent elements (up to <body>) and find the
             * highest z-index
             */
            $p = $img;

            while ($p.length) {
                zIndex = max(zIndex,
                    !isNaN($p.css('z-index')) ? $p.css('z-index') : zIndex);
                /* Also check if any of the ancestor elements has fixed position */
                if ($p.css('position') == 'fixed')
                    position = 'fixed';

                $p = $p.parent(':not(body)');
            }

            /*
             * If z-index is given as an option, it overrides the one found by the
             * above loop
             */
            zIndex = options.zIndex || zIndex;

            if (msie)
                $img.attr('unselectable', 'on');

            /*
             * In MSIE and WebKit, we need to use the keydown event instead of keypress
             */
            imgAreaSelect.keyPress = msie || safari ? 'keydown' : 'keypress';

            /*
             * There is a bug affecting the CSS cursor property in Opera (observed in
             * versions up to 10.00) that prevents the cursor from being updated unless
             * the mouse leaves and enters the element again. To trigger the mouseover
             * event, we're adding an additional div to $box and we're going to toggle
             * it when mouse moves inside the selection area.
             */
            if (opera)
                $areaOpera = div().css({
                    width: '100%', height: '100%',
                    position: 'absolute', zIndex: zIndex + 2 || 2
                });

            /*
             * We initially set visibility to "hidden" as a workaround for a weird
             * behaviour observed in Google Chrome 1.0.154.53 (on Windows XP). Normally
             * we would just set display to "none", but, for some reason, if we do so
             * then Chrome refuses to later display the element with .show() or
             * .fadeIn().
             */
            $box.add($outer).css({
                visibility: 'hidden', position: position,
                overflow: 'hidden', zIndex: zIndex || '0'
            });
            //$box.css({ zIndex: zIndex + 2 || 2 });
            $area.add($border).css({ position: 'absolute', fontSize: 0 });

            //if(options.id){
            //    id=options.id;
            //}
            //else{
                //id = autoId;
            //}
            //$box.attr("id",id);

            /*
             * If the image has been fully loaded, or if it is not really an image (eg.
             * a div), call imgLoad() immediately; otherwise, bind it to be called once
             * on image load event.
             */
            img.complete || img.readyState == 'complete' || !$img.is('img') ?
                imgLoad() : $img.one('load', imgLoad);

            /* 
             * MSIE 9.0 doesn't always fire the image load event -- resetting the src
             * attribute seems to trigger it. The check is for version 7 and above to
             * accommodate for MSIE 9 running in compatibility mode.
             */
            if (!imgLoaded && msie && msie >= 7)
                img.src = img.src;

            ///////////////////////////
            var cur;
            /** @author Tho
             * Xóa các rectangle khác đang được focus 
             * và gán focus cho rectangle hiện tại
             */
            function focusMe() {
                /* Xóa các vùng focus khác */
                $(".focus").removeClass("focus");
                /* đặt focus cho rectangle này*/
                $box.addClass("focus");
                //currentRect = cur;
                for (i = 0; i < rects.length; i++)
                {
                    if (rects[i].getId() == id) {
                        currentRect = rects[i];
                    }
                }
                //            $x = "focus " + id;
                //            console.log($x);
            }

            /**
             * Remove plugin completely
             */
            this.remove = function () {
                /*
                 * Call setOptions with { disable: true } to unbind the event handlers
                 */
                //setOptions({ disable: true });
                
                $box.remove();
                for (i = 0; i < rects.length; i++) {
                    if (rects[i].getId() === id) {
                        rects.splice(i,1);
                    }
                }

            };
            function removeAllFocus() {
                $(".focus").removeClass("focus");
                currentRect = null;
                console.log(currentRect);
            }
            /**
             * Kiểm tra xem rectangle có rỗng hay không!
             * rectangle khi khởi tạo sẽ tạo ra một thẻ div có class là empty
             * @author Tho
             */
            isEmpty = function () {
                return classType == "empty";
            };
            this.isEmpty = isEmpty;
            this.getId = function () {
                return id;
            };
            this.setFieldName = function (name) {
                field = name;
            };
            this.hide = function () {
                $box.hide();
            };
            this.show = function () {
                $box.show();
            };
            //        var scrollX = 0, scrollY = 0;
            $parent.scroll(function (e) {
                if (classType != "empty") {
                    adjust();
                    update();
                }
            });
            this.rotate = function (_degree) {
                var h = imgWidth, w = imgHeight,
                        y1 = selection.y1,
                        y2 = selection.y2,
                        x1 = selection.x1,
                        x2 = selection.x2,
                        ww = selection.width,
                        hh = selection.height;


                /*
                if(x2>0 && y2>0){
                    var xx = "x1="+x1+",y="+y1+",w="+ww+",h="+hh+",h="+h+",w="+w;
                    
                    var x = x1 +ww/2 - w/2 , 
                        y = y1 +hh/2 - h/2 ;
                        if(_degree>0)
                        {
                            //x = x - (h-w)/2, // tọa độ theo tâm mới
                            //y = y - (h-w)/2;
                        }
                        else{
                            //x = x + (h-w)/2, // tọa độ theo tâm mới
                            //y = y + (h-w)/2;
                        }
                        x = x - (h-w)/2; // tọa độ theo tâm mới
                        y = y - (h-w)/2;
                    var x_xoay = -1 * y,//x*Math.cos(Math.PI*_degree/180.0)-y*Math.sin(Math.PI*_degree/180.0),//
                        y_xoay = x;//x*Math.sin(Math.PI*_degree/180.0)+y*Math.cos(Math.PI*_degree/180.0);//
                    x = x_xoay + w/2 - hh/2;
                    y = y_xoay + h/2 - ww/2;
                    x = x ;
                    y = y ;
                    selection.x1= x;//-1*(y1 + hh/2 - h/2) + w/2 - ww/2;
                    selection.y1= y;//1*(x1 + ww/22  - w/2) +h/2 - hh/2;
                    var temp =selection.width;
                    selection.width =  selection.height;
                    selection.height=temp;
                    console.log(_degree+"rotate:"+xx);
                    xx = "=>x1="+x+",y="+y+",w="+hh+",h="+ww+",h="+h+",w="+w;
                    console.log("rotate:"+xx);
                selection.rotateangle += _degree;*/
                if (ww > 0 && hh > 0) {
                    var xx = "x1=" + x1 + ",y=" + y1 + ",w=" + ww + ",h=" + hh + ",h=" + h + ",w=" + w;
                    //LeftBottom
                    console.log("-----");
                    /* công thức đúng
                    var xo = x1-h/2;						
                    var yo = y1-w/2;
                    console.log("xo:"+xo);
                    console.log("yo:"+yo);
                    var x = Math.cos((1*_degree * Math.PI) / 180) * xo - Math.sin((1*_degree * Math.PI) / 180) * yo; 
                    var y = Math.sin((1*_degree * Math.PI) / 180) * xo + Math.cos((1*_degree * Math.PI) / 180) * yo;					
                    
                    x = x + (w)/2;
                    y = y + (h)/2;
                    console.log("x:"+x);
                    console.log("y:"+y);
                    
                    selection.x1= x;
                    selection.y1= y;
                    */
                    // w = 3;
                    // h = 2;
                    // x1 = 0;
                    // y1 = 0;
                    console.log("x:" + x1);
                    console.log("y:" + y1);
                    var xo = x1 - h / 2 + ww / 2;

                    var yo = y1 - w / 2 + hh / 2;

                    //_degree = -90;
                    console.log("xo:" + xo);
                    console.log("yo:" + yo);
                    var x = Math.cos((1 * _degree * Math.PI) / 180) * xo - Math.sin((1 * _degree * Math.PI) / 180) * yo;
                    var y = Math.sin((1 * _degree * Math.PI) / 180) * xo + Math.cos((1 * _degree * Math.PI) / 180) * yo;


                    x = x + (w) / 2 - hh / 2;
                    y = y + (h) / 2 - ww / 2;
                    console.log("x:" + x);
                    console.log("y:" + y);
                    console.log("-------");

                    selection.x1 = x;
                    selection.y1 = y;
                    var temp = selection.width;
                    selection.width = selection.height;
                    selection.height = temp;

                    selection.rotateangle += _degree;
                }
                adjust();
                update();
                adjust();
            };
            //this.rotateClockwise = function (_deg) {
            //    if (classType != "empty") {
            //        var w = imgWidth, h = imgHeight,
            //            xx1 = h - selection.y1, yy1 = selection.x1,
            //            xx2 = h - selection.y2, yy2 = selection.x2;
            //        if (_deg % 360 === 90) {
            //            setSelection(xx2, yy1, xx1, yy2);
            //            xx = "x1=" + xx1 + ",y1=" + yy1 + ",x2=" + xx2 + ",y2=" + yy2 + ",h=" + h;
            //            console.log(xx);
            //            xx = "x1=" + selection.x1 + ",y1=" + selection.y1 + ",x2=" + selection.x2 + ",y2=" + selection.y2 + ",h=" + h;
            //            console.log(xx);
            //            adjust(90);
            //        }
            //        else if (_deg % 360 === 180) {
            //            xx1 = h - selection.y1, yy1 = selection.x1,
            //            xx2 = h - selection.y2, yy2 = selection.x2;
            //            setSelection(xx2, yy1, xx1, yy2);
            //            xx = "x1=" + xx1 + ",y1=" + yy1 + ",x2=" + xx2 + ",y2=" + yy2 + ",h=" + h;
            //            console.log(xx);
            //            xx = "x1=" + selection.x1 + ",y1=" + selection.y1 + ",x2=" + selection.x2 + ",y2=" + selection.y2 + ",h=" + h;
            //            console.log(xx);
            //            adjust(0);
            //        }
            //        else if (_deg % 360 === 270) {
            //            setSelection(xx2, yy1, xx1, yy2);
            //            console.log(_deg);
            //            adjust(90);
            //        }
            //        else {
            //            setSelection(x1, y1, x2, y2);
            //            adjust(0);
            //            console.log(_deg);
            //        }
            //        xx = "x1=" + x1 + ",y1=" + y1 + ",x2=" + x2 + ",y2=" + y2 + ",h=" + h;
            //        console.log(getSelection());
            //
            //        update();
            //    }
            //};
            var isZoom = false;
            this.zoom = function () {
                if (classType !== "empty") {
                    x1 = selection.x1 / preZoom; y1 = selection.y1 / preZoom;
                    x2 = selection.x2 / preZoom; y2 = selection.y2 / preZoom;
                    x1 = round(x1); y1 = round(y1); x2 = round(x2); y2 = round(y2);
                    setSelection(x1 * zoom, y1 * zoom, x2 * zoom, y2 * zoom);
                    adjust();
                    update();
                    isZoom = true;
                }
            };
            this.getOriginalSetting = function () {
                var obj = {};
                //obj.id = id;
                if (isZoom) {
                    obj.selection = {
                        x1: x1,
                        y1: y1,
                        x2: x2,
                        y2: y2,
                        width: x2 - x1,
                        height: y2 - y1
                    };
                } else {
                    obj.selection = {
                        x1: round(selection.x1 / zoom),
                        y1: round(selection.y1 / zoom),
                        x2: round(selection.x2 / zoom),
                        y2: round(selection.y2 / zoom),
                        width: round(selection.x2 / zoom) - round(selection.x1 / zoom),
                        height: round(selection.y2 / zoom) - round(selection.y1 / zoom)
                    };
                }
                obj.type = classType;
                obj.field = field;
                if (classType === types[3])
                    obj.text = text;
                return obj;
            };

            this.getOriginalSetting2 = function () {
                var obj = {};
                //obj.id = id;
                if (isZoom) {
                    obj = {
                        Left: x1,
                        Top: y1,
                        Width: x2 - x1,
                        Height: y2 - y1
                    };
                } else {
                    obj = {
                        Left: round(selection.x1 / zoom),
                        Top: round(selection.y1 / zoom),
                        Width: round(selection.x2 / zoom) - round(selection.x1 / zoom),
                        Height: round(selection.y2 / zoom) - round(selection.y1 / zoom)
                    };
                }
                obj.Type = classType;
                obj.field = field;
                if (classType === types[3])
                    obj.Content = text;
                return obj;
            };

            this.setField = function (_fieldName) {
                field = _fieldName;
            };
            this.resetSelection = function () {
                $combox.empty();
                $combox.append(listField);
            };
            this.getFieldName = function () {
                return field;
            };
            /** @author Tho
             * Context Menu cho cac rectangle
             */
            
            cur = this;
            return this;
        };
        //////////////////////////////////////////////////////////////////////////////////////////////

    };
       
    /**********************************************************************/
    $.fn.selectionArea = function(options,i){
        this.each(function () {
            //if (!$(this).data("selectionArea")) {
                autoId = 0;
                //content = $('<img class="content"/>');
                //$(this).append(content);
                $(this).data("selectionArea", new $.selectionArea(this));
            //}
        });
        return $(this).data('selectionArea');
    };
    //$.selectionArea = function (options, i) {
    //    this.each(function () {
    //        if (!$(this).data("selectionArea")) {
    //            autoId = 0;
    //            content = $('<img class="content"/>');
    //            $(this).append(content);
    //            $(this).data("selectionArea", new $.selectionArea(content));
    //        }
    //    });
    //    return $(this).data('selectionArea');
    //};
    ////////////////////////////////////////////////////////////////////////////
}(jQuery));
