///Create plugin
(function($){
	AnnotationCapture = function(area, opt){
	    var ocrFields;
        //Vung can ve annotation
		var $area = $(area);
		var $parArea = $area.parent();
		var $img = $area.children("img");
		var $helper = $("<div class='helper'></div>");
		var x1, y1, x2, y2;
		var size = { w: 0, h: 0 };
		var rotatedAngle = 0;
		//var originalSize = { w: 0, h: 0 };
		var angle = 0;
		var styleClass = "redaction";
		var curScrollLeft = 0;
		var curScrollTop = 0;
		var scrollX1 = 0;
		var scrollY1 = 0;
		var scrolling = false;
		var hidden = false;
		var center = false;
		var drawing = false;
		var scrollable = true;
		var delta = 0.1;
		sin = Math.sin,
        cos = Math.cos,
        abs = Math.abs,
        pi = Math.PI;
		function round(x) { return x; }
		//var rotatedSize = { w: 0, h: 0 };
	    //remove padding, margin;
		$area.css("padding", 0);
		$area.css("margin", "0 10px");
		var borderWidth = ($area.outerWidth() - $area.width()) / 2;
		var borderHeight = ($area.outerHeight() - $area.height()) / 2;
		var parOuterWidth = ($parArea.outerWidth(true) - $parArea.width()) / 2;
		var parOuterHeight = ($parArea.outerHeight(true) - $parArea.height()) / 2;
		var parOfs = {
		    left: $parArea.offset().left,
		    top: $parArea.offset().top
		};
		var areaOfs = {
		    left: $area.offset().left,
		    top: $area.offset().top
		};
		//var areaOfsOrigin = {
		//    left: $area.offset().left,
		//    top: $area.offset().top
		//};
		var defaultOptions = {
		    border: '1px dashed blue',
		    display: 'none',
		    width: '0px',
		    height: '0px',
		    position: 'absolute'
		};
	    ///Zoom feature function group
		//var delta = 0.1;
		//var nZoom = 0;
		//this.zoomX = function () { return 1 + nZoom * delta };
		//var maxZoom = 5;
	    //var minZoom = -5;
		var rate = 1;
		var currentClass;
		var text;
		var endSelect = function (annotation) { };
		var docViewer;
		var load = function () { };
		this.ready = function (_load) {
		    load = _load;
		};
		if (opt.angle) {
		    angle = opt.angle
		    //rotatePage(angle);
		}
		if (opt && opt.fields)
		    ocrFields = opt.fields;
		$helper.css(defaultOptions);
		$area.append($helper);
		$img.on('dragstart', function (e) {
		    return false;
		});
		if (opt && opt.endSelect)
		    endSelect = opt.endSelect;
		if (opt.width) {
            //Fit witdth size
		    $img.width(opt.width);		    
        }
		$img.load(function () {
		    size.w = $img.width();
		    size.h = $img.height();
		    $area.width(size.w);
		    $area.height(size.h);
		    $area.mousedown(areaMouseDown);
		    $area.mousemove(areaMouseMove);
		    $area.mouseup(areaMouseUp);
		    $area.mouseleave(areaMouseLeave);
		    centering();
		    load.call(this);
		});
		function boxMouseEnter(e) {
		    if (!scrollable)
		        $(this).css("cursor", "move");
		}
		function boxMouseDown(e) {
		    focusMe(this);
		    if (!scrollable) {
		        //focusMe(this);
		        var $p = $(this).parent();
		        $p.unbind('mousedown');
		        $p.unbind('mouseup');
		    }
		}
		function boxMouseUp(e) {
		    if (!scrollable) {
		        var $p = $(this).parent();
		        $p.bind('mousedown', areaMouseDown);
		        $p.bind('mouseup', areaMouseUp);
		    }
		}
		function areaMouseDown(e) {
		    if (!docViewer)
		        docViewer = $parArea;
		    clearFocus();
		    if (scrollable) {
		        scrollX1 = e.pageX;
		        scrollY1 = e.pageY;
		        curScrollLeft = docViewer.scrollLeft();
		        curScrollTop = docViewer.scrollTop();
		        scrolling = !drawing;
		    } else if (!hidden) {
		        x1 = e.pageX - $area.offset().left;
		        y1 = e.pageY - $area.offset().top;
		        $helper.css('left', x1);
		        $helper.css('top', y1);
		        $helper.show();
		        //log("e: " + e.pageX + ":" + e.pageY +
		        //    "<br/>ofs: " + $area.offset().left + ":" + $area.offset().top +
		        //    "<br/>(x1,y1)=" + x1 + ":" + y1);
		        drawing = true;
		    }
		}
		function areaMouseMove(e) {
		    if (!docViewer)
		        docViewer = $parArea;
		    if (scrolling) {
		        var dx = scrollX1 - e.pageX;
		        var dy = scrollY1 - e.pageY;
		        docViewer.scrollLeft(curScrollLeft + dx).scrollTop(curScrollTop + dy);
		    } else
		        if (drawing && !hidden) {
		            var x2 = e.pageX - $area.offset().left;
		            var y2 = e.pageY - $area.offset().top;
		            $helper.width(x2 - x1);
		            $helper.height(y2 - y1);
		            //log("e: " + e.pageX + ":" + e.pageY +
		            //    "<br/>ofs: " + $area.offset().left + ":" + $area.offset().top +
		            //    "<br/>(x1,y1)=" + x1 + ":" + y1 +
		            //    "<br/>(x2,y2)=" + x2 + ":" + y2);
		        }
		}
		function areaMouseUp(e) {
		    drawing = false;
		    scrolling = false;
		    var w = $helper.width();
		    var h = $helper.height();
		    if (w > 1 && h > 1) {
		        var params = {
		            width: w,
		            height: h,
		            left: x1,
		            top: y1
		        }
		        var $box = createAnnotation(params, styleClass);
		        $area.append($box);
		        focusMe($box);
		        eventOnAnnotation($box);
		        endSelect($box);
		    }
		    $helper.css(defaultOptions);
		    $helper.hide();
		}
		function l(msg) {
		    console.log(msg);
		}
		function areaMouseLeave() {
		    scrolling = false;
		    drawing = false;
		}
		function createAnnotation(params, className) {
		    var $box = $("<div class='" + className + "'></div>");
		    var $params = $("<input type='hidden' class='params' value=''/>");
		    var W = $area.width(), H = $area.height(), rad = angle * pi / 180;
		    var borderWidth = ($helper.outerWidth() - $helper.width());
		    var x = params.left, y = params.top, w = params.width, h = params.height;
		    var left = round(((x * powCos(rad) * (1 + cos(rad))  		//Goc quay 360, 0
						+ y * powSin(rad) * (1 - sin(rad)) 		//Goc quay 270, -90
						+ (W - w - x) * powCos(rad) * (1 - cos(rad))	//Goc quay 180,-180
						+ (H - h - y) * powSin(rad) * (1 + sin(rad))) / 2) / rate);  //Goc quay 90,-270
		    var top = round(((y * powCos(rad) * (1 + cos(rad))  		//Goc quay 360,0
						+ (W - w - x) * powSin(rad) * (1 - sin(rad)) //Goc quay 270, -90
						+ (H - h - y) * powCos(rad) * (1 - cos(rad)) //Goc quay 180,-180
						+ x * powSin(rad) * (1 + sin(rad))) / 2) / rate);  		//Goc quay 90,-270

		    var preRotate = {
		        height: round((w * powSin(rad) + h * powCos(rad)) / rate),
		        width: round((h * powSin(rad) + w * powCos(rad)) / rate),
		        left: left,
		        top: top
		    };

		    $box.css('position', 'absolute');
		    $box.css(params);
		    $params.val(JSON.stringify(preRotate));
		    $box.append($params);
		    return $box;
		}
		function eventOnAnnotation($box) {
		    $box.mouseenter(boxMouseEnter);
		    $box.mousedown(boxMouseDown);
		    $box.mouseup(boxMouseUp);
		    $box.draggable({
		        cursor: "move",
		        containment: "parent",
		        //start: startDragOrResize,
		        //stop: stopDragOrResize
		    })
				.resizable({
				    containment: "parent",
				    handles: 'n, e, s, w, ne, se, sw, nw ',
				    //start: startDragOrResize,
				    //stop: stopDragOrResize
				});
		}
		//function posOrigin(x1, y1, w, h) {
		//    var W = $area.width(), H = $area.height(), rad = angle * pi / 180, rate = 1 + nZoom * delta;
		//    var borderWidth = ($helper.outerWidth() - $helper.width());
		//    var left = ((x1 * powCos(rad) * (1 + cos(rad))  		//Goc quay 360, 0
		//				+ y1 * powSin(rad) * (1 - sin(rad)) 		//Goc quay 270, -90
		//				+ (W - w - x1 - borderWidth) * powCos(rad) * (1 - cos(rad))	//Goc quay 180,-180
		//				+ (H - h - y1 - borderWidth) * powSin(rad) * (1 + sin(rad))) / 2) / rate;  //Goc quay 90,-270
		//    var top = ((y1 * powCos(rad) * (1 + cos(rad))  		//Goc quay 360,0
		//				+ (W - w - x1 - borderWidth) * powSin(rad) * (1 - sin(rad)) //Goc quay 270, -90
		//				+ (H - h - y1 - borderWidth) * powCos(rad) * (1 - cos(rad)) //Goc quay 180,-180
		//				+ x1 * powSin(rad) * (1 + sin(rad))) / 2) / rate;  		//Goc quay 90,-270	
		//    var pos = { left: left, top: top };
		//    return pos;
		//}
		//function sizeOrigin(w, h) {
		//    var rad = angle * pi / 180, rate = 1 + nZoom * delta;
		//    var height = round((w * powSin(rad) + h * powCos(rad)) / rate);
		//    var width = round((h * powSin(rad) + w * powCos(rad)) / rate);
		//    var size = { width: width, height: height };
		//    return size;
		//}


		//function startDragOrResize(e, ui) {
		//    $(this).data('scrollable', scrollable);
		//    scrollable = false;
		//    $(this).data('scrolling', scrolling);
		//    scrolling = false;
		//    $(this).data('drawing', drawing);
		//    drawing = false;
		//    var pos = { left: $(this).offset().left, top: $(this).offset().top };
		//    $(this).data('startX', pos.left);
		//    $(this).data('startY', pos.top);
		//}

		//function stopDragOrResize(e, ui) {
		//    var pos = { left: $(this).offset().left, top: $(this).offset().top };
		//    var size = { width: $(this).width(), height: $(this).height() };
		//    var dx = round(pos.left - $(this).data('startX'));
		//    var dy = round(pos.top - $(this).data('startY'));
		//    var params = JSON.parse($(this).children('.params').val());
		//    params.left = params.left + dx;
		//    params.top = params.top + dy;
		//    params.width = size.width;
		//    params.height = size.height;
		//    $(this).children('.params').val(JSON.stringify(params));
		//    scrollable = $(this).data('scrollable');
		//    scrolling = false;//$(this).data('scrolling');
		//    drawing = false;$(this).data('drawing');
		//}

		function rotate(angle) {
		    rotatePage(angle);
		    $area.children().filter(':not(img,.helper)')
				.each(function (index, element) {
				    var params = JSON.parse($(this).children('.params').val());
				    var aw = $area.width();
				    var ah = $area.height();
				    var w = $(this).width();
				    var h = $(this).height();
				    var m = Math.min(aw, ah),
						x = this.offsetLeft,
                        y = this.offsetTop,
                        //x = $(this).offset().left,
						//y = $(this).offset().top,
                        x0 = x - ah/2 + w / 2, y0 = y - aw/2 + h / 2,
						d = abs(ah - aw),
						rad = angle * pi / 180;
				    //var borderW = $(this).outerWidth() - $(this).width();
				    //var borderH = $(this).outerHeight() - $(this).height();
				    
				    rotatedAngle = 90;
                    //rotate round itself
				    //var l = x - ah / 2 + w / 2 + aw / 2 - h / 2;
				    //var t = x0 + ah/2 - w/2;

				    var t, l;
				    t = x;
				    l = aw - y - h;
				    //if (angle == 90 || angle == -270) {
				    //    rotatedAngle = 90;
				    //    //var x = round((m / 2 + (a - m / 2) * cos(rad) + (b - m / 2) * sin(rad) +
					//	//		 (d - params.height - borderW) * sin(rad) * (sin(rad) - 1) / 2
					//	//		 - (params.width + borderW) * cos(rad) * (cos(rad) - 1) / 2) * (1 + delta * nZoom));
				    //    //var y = round((abs(-m / 2 + (a - m / 2) * sin(rad) - (b - m / 2) * cos(rad) - d * cos(rad) * (cos(rad) - 1) / 2)
                    //    //         - (params.height + borderH) * cos(rad) * (cos(rad) - 1) / 2
				    //    //         - (params.width + borderH) * sin(rad) * (1 + sin(rad)) / 2) * (1 + delta * nZoom));
				    //    t = x ;
				    //    l = y + ah / 2;
				    //    console.log(("aw: " + aw + " ah: " + ah + " w: " + w + " h: " + h));
				    //} else
				    //    if (angle == 180 || angle == -180) {
				    //        rotatedAngle = 180;
				    //        t = x;
				    //        l = y;
				    //    } else
				    //        if (angle == 270 || angle == -90) {
				    //            rotatedAngle = 180;
				    //            t = x;
				    //            l = y;
				    //        } else {
				    //            rotatedAngle = 180;
				    //            t = x;
				    //            l = y;
				    //        }
				    //var x = round((m / 2 + (a - m / 2) * cos(rad) + (b - m / 2) * sin(rad) +
					//			 (d - h - borderW) * sin(rad) * (sin(rad) - 1) / 2
					//			 - (h + borderW) * cos(rad) * (cos(rad) - 1) / 2));
				    //var y = round((abs(-m / 2 + (a - m / 2) * sin(rad) - (b - m / 2) * cos(rad) - d * cos(rad) * (cos(rad) - 1) / 2)
					//		 - (h + borderH) * cos(rad) * (cos(rad) - 1) / 2
				    //		 - (w + borderH) * sin(rad) * (1 + sin(rad)) / 2));
				    $(element).css({
				        left: l/rate,
				        top: t/rate,
				        width: $(this).height()/rate,
				        height: $(this).width()/rate
				    });
				});
		}
		function rotatePage(angle) {
		    var w = size.w * round(Math.pow(cos(angle * pi / 180), 2))
							+ size.h * round(Math.pow(sin(angle * pi / 180), 2));
		    var h = size.h * round(Math.pow(cos(angle * pi / 180), 2))
							+ size.w * round(Math.pow(sin(angle * pi / 180), 2));
		    $area.height(h);
		    $area.width(w);
		    adjustImage();
		    var t = size.h;
		    size.h = size.w;
		    size.w = t;
		}
		function adjustImage() {
		    if (angle % 360 == -90 || angle % 360 == 270) {
		        var origin = round((size.w / 2) * rate);
		        $area.children('img').css({
		            "-webkit-transform": "rotate(270deg)",
		            "-webkit-transform-origin": "50% " + origin + "px"
		        });
		    } else if (abs(angle) % 360 == 180) {
		        $area.children('img').css({
		            "-webkit-transform": "rotate(180deg)",
		            "-webkit-transform-origin": "50% 50%"
		        });
		    } else if (angle % 360 == -270 || angle % 360 == 90) {
		        var origin = round((size.h / 2) * rate);
		        $area.children('img').css({
		            "-webkit-transform": "rotate(90deg)",
		            "-webkit-transform-origin": origin
		        });
		    } else {
		        $area.children('img').css({
		            "-webkit-transform": "rotate(0deg)",
		            "-webkit-transform-origin": "50% 50%"
		        });
		    }
		}

		function getXo(Xn) {
		    var _Xo = Xn / (1 + nZoom * delta);
		    return _Xo;
		}
		function zoomIn(_Xo) {
		    var _Xn = _Xo * (1 + (nZoom + 1) * delta);
		    if (_Xn && _Xn > 0)
		        return _Xn;
		    return _Xo;
		}
		this.zoomInX = zoomIn;
		function zoomOut(_Xo) {
		    var _Xn = _Xo * (1 + (nZoom - 1) * delta);
		    if (_Xn && _Xn > 0)
		        return _Xn;
		    return _Xo;
		}
		this.zoomOutX = zoomOut;
		function zoomElement(x, Xo) {
		    return zoomIn(Xo);
		}
		function zoomElementOut(x, Xo) {
		    return zoomOut(Xo);
		}

		function zoomInIt(pos) {
		    //Tim size so khi rate = 1
		    var orig = pos / rate;
		    return pos + orig * delta;
		}
		function zoomOutIt(pos) {
		    //Tim size so khi rate = 1
		    var orig = pos / rate;
		    return pos - orig * delta;
		}

		function performZoom() {
            //Tang moi lan 10% so voi kich thuoc ban dau
		    var w = $area.width() + size.w * delta;
		    $area.width(w);
		    var h = $area.height() + size.h * delta;
		    $area.height(h);
		    var children = $area.children(':not(img,.helper)');
		    var $img = $area.children('img');
		    if (cos(angle * pi / 180) == 0)
		        $img.width($area.height());
		    else
		        $img.width($area.width());
		    $.each(children, function (index, element) {
		        var top = this.offsetTop;
		        var left = this.offsetLeft;
		        $(this).css("left", zoomInIt(left));
		        $(this).css("top", zoomInIt(top));
		        $(this).width(zoomInIt($(this).width()));
		        $(this).height(zoomInIt($(this).height()));
		    });
		}
		function performZoomOut() {
		    //Tang moi lan 10% so voi kich thuoc ban dau
		    var w = $area.width() - size.w * delta;
		    $area.width(w);
		    var h = $area.height() - size.h * delta;
		    $area.height(h);
		    var children = $area.children(':not(img,.helper)');
		    var $img = $area.children('img');
		    if (cos(angle * pi / 180) == 0)
		        $img.width($area.height());
		    else
		        $img.width($area.width());
		    $.each(children, function (index, element) {
		        var top = $(this).offset().top - $(this).parent().offset().top;
		        var left = $(this).offset().left - $(this).parent().offset().left;
		        $(this).css("left", zoomOutIt(left));
		        $(this).css("top", zoomOutIt(top));
		        $(this).width(zoomOutIt($(this).width()));
		        $(this).height(zoomOutIt($(this).height()));
		    });
		}
		function zI() {
		    original();
		    performZoom();
		    rate = Math.round((rate + delta) * 10) / 10;
		    //nZoom++;
		    adjustImage();
		    centering();
		    console.log(rate);
		}

		function zO() {
		    original();
		    performZoomOut();
		    rate = Math.round((rate - delta) * 10) / 10;
		    //nZoom++;
		    adjustImage();
		    centering();
		    console.log(rate);
		}

		function fit(widthViewer) {
		    //if (originalSize.w > widthViewer) {
		    while (true) {
		        zO();
		        if ($area.width() < widthViewer)
		            break;
		    }
		    //}
		}

		function fitH(h) {
		    //if (originalSize.w > widthViewer) {
		    while (true) {
		        zO();
		        if ($area.height() < h)
		            break;
		    }
		    //}
		}

		function rotateClockwise() {
		    original();
		    angle += 90;
		    rotate(angle);
		    centering();
		}

		function rotateCounterClockwise() {
		    original();
		    angle -= 90;
		    rotate(angle);
		    centering();
		}

		function focusMe(element) {
		    clearFocus();
		    $(element).addClass('anno_focus');
		}
		function clearFocus() {
		    $(".anno_focus").removeClass('anno_focus');
		}
		function powCos(angle) {
		    //var radian = deg*Math.PI/180;
		    return Math.pow(cos(angle), 2);
		}
		function powSin(angle) {
		    //var radian = deg*Math.PI/180;
		    return Math.pow(sin(angle), 2);
		}
		function centering() {
		    if (!center) {
		        var left = ($area.parent().width() - $area.width()) / 2 + $area.parent().offset().left;
		        var top = ($area.parent().height() - $area.height()) / 2 + $area.parent().offset().top;
		        //$area.css({left: left, top: top, position: "absolute"});
		        center = true;
		        $area.css({ position: "relative", margin: "10px auto" });
		    }
		}
        //Dua page ve vi tri 0,0
		function original() {
		    if (center) {
		        var dx = $area.offset().left;
		        var dy = $area.offset().top;
		        $area.css({ left: 0, top: 0, position: "absolute" });
		        $(this).children(":not(img,.helper)").each(function (index, element) {
		            var x = $(this).offset().left - dx;
		            var y = $(this).offset().top - dy;
		            $(this).css({ left: x, top: y });
		        });
		        center = false;
		    }
		}
		function setDefaultClass(className) {
		    styleClass = className;
		}
	    //Public API
		this.scrollable = function (_docViewer) {
		    if (_docViewer)
		        docViewer = _docViewer;
		    scrollable = true;
		    $area.css("cursor", "move");
		}
		this.drawable = function () {
		    scrollable = false;
		    $area.css("cursor", "crosshair");
		}
		this.hide = function () {
		    $area.children(":not(img)").css("visibility", "hidden");
		    hidden = true;
		}
		this.show = function () {
		    $area.children(":not(img)").css("visibility", "visible");
		    hidden = false;
		}
		this.toggleHidden = function () {
		    if (hidden) {
		        this.show();
		    } else {
		        this.hide();
		    }
		}
		this.zoomIn = zI;
		this.zoomOut = zO;
		this.rotateClockwise = rotateClockwise;
		this.rotateCounterClockwise = rotateCounterClockwise;
		this.setDefaultClass = setDefaultClass;
		this.fit = fit;
		this.fitH = fitH;
		this.width = function () { return $area.width(); }
		this.height = function () { return $area.height(); }
		this.rotateAngle = function () { return angle; }
		this.annoText = function () {
		    this.drawable();
			this.setDefaultClass("anno_text");
			endSelect = dblClickOnAnnoText;
		}
		this.annoRedaction = function () {
		    this.drawable();
			this.setDefaultClass("anno_redaction");
			endSelect = function(a){
				
			}
		}
		this.annoHighlight = function () {
		    this.drawable();
			this.setDefaultClass("anno_highlight");
			endSelect = function(a){
				
			}
		}
		this.annoOcrZone = function (_fields) {
		    this.drawable();
			this.setDefaultClass("anno_ocr_zone");
			endSelect = dblClickOnAnnoOcr;
		}
		this.changeTo = function(anno){
			var $target = $('.anno_focus',area);
			if(!$target.length)
				return;
			var $clone = $target.clone();
			$clone.children(":not(input)").remove();
			switch(anno){
				case "text":{
					if($target.hasClass("anno_text"))
						return;
					$clone.removeClass("anno_ocr_zone anno_highlight anno_redaction");
					$clone.addClass("anno_text");
					eventOnAnnotation($clone);
					dblClickOnAnnoText($clone);
					break;
				}case "ocr_zone":{
					if($target.hasClass("anno_ocr_zone"))
						return;
					$clone.removeClass("anno_text anno_highlight anno_redaction");
					$clone.addClass("anno_ocr_zone");
					eventOnAnnotation($clone);
					dblClickOnAnnoOcr($clone);
					break;
				}
				case "highlight":{
					if($target.hasClass("anno_highligh"))
						return;
					$clone.removeClass("anno_text anno_ocr_zone anno_redaction");
					$clone.addClass("anno_highlight");
					eventOnAnnotation($clone);
					break;
				}
				case "redaction":{
					if($target.hasClass("anno_redaction"))
						return;
					$clone.removeClass("anno_text anno_ocr_zone anno_highlight");
					$clone.addClass("anno_redaction");
					eventOnAnnotation($clone);
					break;
				}
			}
			$clone.css("background-color","");
			$clone.insertBefore($target);
			$target.remove();
		}
		this.delete = function(){
			$('.anno_focus',area).remove();
		}
		function dblClickOnAnnoText($box){
		    var $text = $('<div class="anno_text_content"></div>');
		    var $editable = $("<a href='#' class='editable-button empty'>Edit...</a>");
		    
		    $editable.css({ "border-bottom": "0", "overflow": "hidden" });
			$editable.attr("data-type", "textarea");
			$box.append($text);
			$editable.editable({
				title: 'Enter comments (ctrl + enter)',
				rows: 3,
				//toggle: 'dblclick',
				display: function (value) {
                    if(!$(this).hasClass('empty'))
				        $text.text(value);
                    $(this).removeClass("editable-unsaved");                    
				},
				onblur: 'submit',
				placement: 'right',
				defaultValue: ' ',
			    savenochange: true,
				showbuttons:false
			});
			$editable.on('shown', function (e, editable) {
			    $editable.removeClass('empty');
			    editable.input.$input.val('Enter text here...');
			});

			$box.append($editable.hide());
			$box.bind('mouseenter',function () {
		        //$text.hide();
		        $editable.show();
		        var l = $box.width() / 2 - $editable.width() / 2;
		        var t = $box.height() / 2 - $editable.height() / 2;
		        $editable.css({ left: l, top: t, position: 'absolute' });
			})
			$box.bind('mouseleave',function () {
		       // $text.show();
			    $editable.hide();
			});

			$.fn.editableform.buttons = '<button type="submit" class="editable-submit">OK</button>' +
										'<button type="button" class="editable-cancel">Cancel</button>';
		}
		function dblClickOnAnnoOcr($box) {
			var $options = $('<div class="anno_ocr_zone_name"><span style="color:rgb(145, 21, 21);font-style:italic">Double click to select field!</span></div>');
			$box.css({"border-bottom":"","overflow":"hidden"});
			$box.attr("data-type","select");
			$box.editable({
				title: "Select field",
				source: ocrFields,
				toggle: "dblclick",
				display: function(value,sourceData){
					var selected = $.fn.editableutils.itemsByValue(value, sourceData);
					if(selected.length)
						$(this).children(".anno_ocr_zone_name").html("<span>"+selected[0].text+"</span>");
					$(this).removeClass("editable-unsaved");
					$(this).css("background-color", "");
					$(this).removeClass("editable-unsaved editable-click");
				},
				onblur: 'submit',
				//showbuttons: false
			});
			$box.append($options.hide());
			$box.bind('mouseenter', function () {
			    $options.show();
			})
			$box.bind('mouseleave', function () {
			    $options.hide();
			});
			$.fn.editableform.buttons = '<button type="submit" class="editable-submit">OK</button>' +
										'<button type="button" class="editable-cancel">Cancel</button>';
		}
		var create = function(params){
		    var opt = { left: 0, top: 0, width: 0, height: 0, type: "redaction" };
		    $.extend(opt, params);
            opt.left = opt.left 
			var $box = createAnnotation(opt, "anno_" + opt.type);
			switch(opt.type){
				case "ocr_zone":{
					dblClickOnAnnoOcr($box);
					if(opt.select)
						$box.children(".anno_ocr_zone_name").html("<span>"+opt.select+"</span>");
					break;
				}
				case "text":{
					dblClickOnAnnoText($box);
					break;
				}
			}
			$(area).append($box);
			eventOnAnnotation($box);
		}
		this.setFields = function(_fields){
			ocrFields = _fields;
		}
		this.createAnnotations = function(annos){
			$.each(annos, function(i,v){
				create(this);
			});
		}
		this.getAnnotationsXXX = function(){
			objs = [];
			$(area).children(":not(img,.helper,.anno_passive)").each(function (index, element) {
			    var jsObj = JSON.parse($(this).children(".params").val());
				if($(this).hasClass("anno_redaction"))
					jsObj.type = "Redaction";
				else if($(this).hasClass("anno_text")){
					jsObj.type = "Text";
					jsObj.content = $(this).children(".anno_text_content").text();
				}
				else if($(this).hasClass("anno_highlight"))
					jsObj.type = "Highlight";
				else if($(this).hasClass("anno_ocr_zone"))
					jsObj.type = "OcrZone";
				objs.push(jsObj);
            });
			return objs;
		}

		this.getAnnotations = function () {
		    objs = [];
		    var _this = this;
		    $(area).children(":not(img,.helper,.anno_passive)").each(function (index, element) {
		        var jsObj = JSON.parse($(this).children(".params").val());
		        jsObj.left = $(this).parent().offset().left + parseFloat($(this).css('left')) - $(area).offset().left;
		        jsObj.top = $(this).parent().offset().top + parseFloat($(this).css('top')) - $(area).offset().top;
		        jsObj.width =  $(this).width();
		        jsObj.height = $(this).height();

		        if ($(this).hasClass("anno_redaction"))
		            jsObj.type = "Redaction";
		        else if ($(this).hasClass("anno_text")) {
		            jsObj.type = "Text";
		            jsObj.content = $(this).children(".anno_text_content").text();
		        }
		        else if ($(this).hasClass("anno_highlight"))
		            jsObj.type = "Highlight";
		        else if ($(this).hasClass("anno_ocr_zone"))
		            jsObj.type = "OcrZone";
		        objs.push(jsObj);
		    });
		    return objs;
		}
		this.getAnnotationsOriginSize = function () {
		    objs = [];
		    var _this = this;
		    $(area).children(":not(img,.helper,.anno_passive)").each(function (index, element) {
		        var jsObj = JSON.parse($(this).children(".params").val());
		        jsObj.left = ($(this).parent().offset().left +
                                parseFloat($(this).css('left')) -
                                $(area).offset().left) / _this.zoomX();
		        jsObj.top = ($(this).parent().offset().top +
                                parseFloat($(this).css('top')) -
                                $(area).offset().top) / _this.zoomX();
		        jsObj.width = $(this).width() / _this.zoomX();
		        jsObj.height = $(this).height() / _this.zoomX();

		        if ($(this).hasClass("anno_redaction"))
		            jsObj.type = "Redaction";
		        else if ($(this).hasClass("anno_text")) {
		            jsObj.type = "Text";
		            jsObj.content = $(this).children(".anno_text_content").text();
		        }
		        else if ($(this).hasClass("anno_highlight"))
		            jsObj.type = "Highlight";
		        else if ($(this).hasClass("anno_ocr_zone"))
		            jsObj.type = "OcrZone";
		        objs.push(jsObj);
		    });
		    return objs;
		}
		this.disableDblClickOnAnnotation = function () {
		    $(".anno_text, .anno_ocr_zone").editable('disable');
		}
		this.showPassiveAnnotation = function (pos) {
		    var opt = { left: 0, top: 0, width: 0, height: 0};
		    $.extend(opt, pos);
		    opt.left *= this.zoomX();
		    opt.top *= this.zoomX();
		    opt.width *= this.zoomX();
		    opt.height *= this.zoomX();
		    var $box = createAnnotation(opt, "anno_passive");
		    $box.attr("id", "passive");
		    $(area).append($box);
		}
		this.removePassiveAnnotation = function () {
		    $("#passive").remove();
		}
		var _this = this;
		$(document).on('keyup', function (e) {
		    if (e.keyCode == 46)
		        _this.delete();
		});
		return this;
	}
	$.fn.annotation = function(options){
		this.each(function(){
			defaultOption = {
				
			}
			$(this).data("annotation",new Annotation(this));
		});
		return $(this).data("annotation");
	}
	$.fn.annotationCapture = function(options){
		this.each(function(){
			if(options && options.image){
				$(this).append("<img src='" + options.image + "'/>");
			}

			$(this).data("annotation",new AnnotationCapture(this,options));
		});
		return $(this).data("annotation");
	}
}(jQuery));