///Create plugin
(function($){
	Annotation = function(area,opt){
		var $area = $(area);
		var $parArea = $area.parent();
		var $img = $area.children("img");
        var $helper = $("<div class='helper'></div>");
		var x1, y1,x2,y2;
		var originalSize = { w: 0, h: 0};
		var angle = 0;
		var styleClass = "redaction";
		var scrollable = true;
		var curScrollLeft = 0;
		var curScrollTop = 0;
		var scrollX1 = 0;
		var scrollY1 = 0;
		var scrolling = false;
		var hidden = false;
		var deg = 0;
		    sin = Math.sin,
			cos = Math.cos,
			abs = Math.abs,
			//round = Math.round,
			pi = Math.PI;
		function round(x) { return x; }
		var rotatedSize = { w: 0, h: 0 };
		//do not support padding;
		$area.css("padding",0);
		$area.css("margin","0 0");
		var borderWidth = ($area.outerWidth() - $area.width()) / 2;
		var borderHeight = ($area.outerHeight() - $area.height()) / 2;
		var parOuterWidth =($parArea.outerWidth(true) - $parArea.width()) / 2;
		var parOuterHeight =($parArea.outerHeight(true) - $parArea.height()) / 2;
		var parOfs = {left: $parArea.offset().left, 
						top: $parArea.offset().top 	};
		var areaOfs = {left: $area.offset().left, 
						top: $area.offset().top };
		var areaOfsOrigin = {left: $area.offset().left, 
							top: $area.offset().top };
		var moving = false;
		var defaultOptions = {
				border: '1px dashed blue',
				display: 'none',
				width: '0px',
				height: '0px',
				position: 'absolute'
		};
		var center = false;
	    ///Zoom feature function group
		var delta = 0.1;
		var nZoom = 0;
		this.zoomX = function () { return 1 + nZoom * delta};
		var maxZoom = 5;
		var minZoom = -5;
		var currentClass;
		var text;
		this.load = function () { };
		
		$helper.css(defaultOptions);
		$area.append($helper);
		$img.on('dragstart',function(e){
			return false;
		});
		endSelect = function(annotation){};
		if(opt && opt.endSelect)
			endSelect = opt.endSelect;
		$img.load(function(){
			rotatedSize.w = originalSize.w = $img.width();
			rotatedSize.h = originalSize.h = $img.height();
			$area.width(originalSize.w);
			$area.height(originalSize.h);
			$area.css("padding",0);
			$area.mousedown(areaMouseDown);
			$area.mousemove(areaMouseMove);
			$area.mouseup(areaMouseUp);
			//$area.mouseleave(areaMouseLeave);
			centering();
		    //this.load();
			fit($area.parent().width());
		});
		function boxMouseEnter(e) {
            if(!scrollable)
		        $(this).css("cursor", "move");
		}
		function boxMouseDown(e) {
		    if (!scrollable) {
		        focusMe(this);
		        var $p = $(this).parent();
		        $p.unbind('mousedown');
		        $p.unbind('mouseup');
		    }
		}
		function boxMouseOut(e) {
		    if (!scrollable) {
		        var $p = $(this).parent();
		        $p.bind('mousedown', areaMouseDown);
		        $p.bind('mouseup', areaMouseUp);
		    }
		}
		function areaMouseDown(e){
		    clearFocus();
		    if (scrollable) {
		        scrollX1 = e.pageX;
		        scrollY1 = e.pageY;
		        curScrollLeft = $parArea.scrollLeft();
		        curScrollTop = $parArea.scrollTop();
		        scrolling = !moving;
		    } else if(!hidden) {
		        x1 = e.pageX - $area.offset().left;
		        y1 = e.pageY - $area.offset().top;
		        $helper.css('left', x1);
		        $helper.css('top', y1);
		        $helper.show();
		        //log("e: " + e.pageX + ":" + e.pageY +
                //    "<br/>ofs: " + $area.offset().left + ":" + $area.offset().top +
                //    "<br/>(x1,y1)=" + x1 + ":" + y1);
		        moving = true;
		    }
		}
		function areaMouseMove(e) {
		    if (scrolling) {
		        var dx = scrollX1 - e.pageX;
		        var dy = scrollY1 - e.pageY;
		        $parArea.scrollLeft(curScrollLeft + dx).scrollTop(curScrollTop + dy);
		    } else 
		    if (moving && !hidden) {
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
		    moving = false;
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
		function areaMouseLeave() {
		    scrolling = false;
		    moving = false;
		}
		createAnnotation = function(params, className){
			var $box = $("<div class='" + className + "'></div>");
			var $params =$("<input type='hidden' class='params' value=''/>");
			var W = $area.width(), H = $area.height(), rad = angle*pi/180, rate = 1 + nZoom*delta;
			var borderWidth = ($helper.outerWidth() - $helper.width());
			var x = params.left, y = params.top, w = params.width, h = params.height;
			var left = round(((  x*powCos(rad)*(1+cos(rad))  		//Goc quay 360, 0
						+ y*powSin(rad)*(1-sin(rad)) 		//Goc quay 270, -90
						+ (W-w-x)*powCos(rad)*(1-cos(rad))	//Goc quay 180,-180
						+ (H-h-y)*powSin(rad)*(1+sin(rad)))/2)/rate);  //Goc quay 90,-270
			var top = round(((  y*powCos(rad)*(1+cos(rad))  		//Goc quay 360,0
						+ (W-w-x)*powSin(rad)*(1-sin(rad)) //Goc quay 270, -90
						+ (H-h-y)*powCos(rad)*(1-cos(rad)) //Goc quay 180,-180
						+ x*powSin(rad)*(1+sin(rad)))/2)/rate);  		//Goc quay 90,-270
						
			var preRotate = {
					height: round((w*powSin(rad) + h*powCos(rad))/rate),
					width: round((h*powSin(rad) + w*powCos(rad))/rate),
					left: left ,
					top: top 
				};
					
			$box.css('position', 'absolute');
			$box.css(params);			
			$params.val(JSON.stringify(preRotate));
			$box.append($params);
			return $box;
		}
		eventOnAnnotation = function ($box) {
		    $box.mouseenter(boxMouseEnter);
			$box.mousedown(boxMouseDown);
			$box.mouseout(boxMouseOut);
			$box.draggable({ 
					cursor: "move", 
					containment: "parent",
					start: startDragOrResize,
					stop: stopDragOrResize
				})
				.resizable({
					containment: "parent", 
					handles: 'n, e, s, w, ne, se, sw, nw ',
					start: startDragOrResize,
					stop: stopDragOrResize
				});
		}
		function posOrigin(x1,y1,w,h){
			var W = $area.width(), H = $area.height(), rad = angle*pi/180, rate = 1 + nZoom*delta;
			var borderWidth = ($helper.outerWidth() - $helper.width());
			var left =((  x1*powCos(rad)*(1+cos(rad))  		//Goc quay 360, 0
						+ y1*powSin(rad)*(1-sin(rad)) 		//Goc quay 270, -90
						+ (W-w-x1 - borderWidth)*powCos(rad)*(1-cos(rad))	//Goc quay 180,-180
						+ (H-h-y1 - borderWidth)*powSin(rad)*(1+sin(rad)))/2)/rate;  //Goc quay 90,-270
			var top =((  y1*powCos(rad)*(1+cos(rad))  		//Goc quay 360,0
						+ (W-w-x1 - borderWidth)*powSin(rad)*(1-sin(rad)) //Goc quay 270, -90
						+ (H-h-y1 - borderWidth)*powCos(rad)*(1-cos(rad)) //Goc quay 180,-180
						+ x1*powSin(rad)*(1+sin(rad)))/2)/rate;  		//Goc quay 90,-270	
			var pos = {left: left, top: top};
			return pos;
		}
		function sizeOrigin(w,h){
			var rad = angle*pi/180, rate = 1 + nZoom*delta;
			var height = round((w*powSin(rad) + h*powCos(rad))/rate);
			var width = round((h*powSin(rad) + w*powCos(rad))/rate);	
			var size = {width: width, height: height};
			return size;
		}
		function startDragOrResize(e, ui){
			var pos = posOrigin($(this).offset().left,$(this).offset().top,$(this).width(),$(this).height());
			$(this).data('startX',pos.left);
			$(this).data('startY',pos.top);
		}
		
		function stopDragOrResize(e, ui){
			var pos = posOrigin($(this).offset().left,$(this).offset().top,$(this).width(),$(this).height());
			var size = sizeOrigin($(this).width(),$(this).height());
			var dx = round(pos.left - $(this).data('startX'));
			var dy = round(pos.top - $(this).data('startY'));
			var params = JSON.parse($(this).children('.params').val());
			params.left = params.left + dx;
			params.top = params.top + dy;
			params.width = size.width;
			params.height = size.height;
			log(JSON.stringify(params));
			$(this).children('.params').val(JSON.stringify(params));		
		}
		
		function rotate(angle){
			rotatedSize.w = originalSize.w*round(Math.pow(cos(angle*pi/180),2)) 
							+ originalSize.h*round(Math.pow(sin(angle*pi/180),2));
			rotatedSize.h = originalSize.h*round(Math.pow(cos(angle*pi/180),2)) 
							+ originalSize.w*round(Math.pow(sin(angle*pi/180),2));
			$area.height(rotatedSize.h * (1 + delta*nZoom));
			$area.width(rotatedSize.w * (1 + delta*nZoom));
			adjustImage();
			$area.children().filter(':not(img,.helper)')
				.each(function(index, element) {
					var params = JSON.parse($(this).children('.params').val());
					var m = Math.min(originalSize.w,originalSize.h),		
						a = params.left,
						b = params.top,
						d = abs(originalSize.h - originalSize.w),
						rad = angle*pi/180;
					var borderW =$(this).outerWidth() - $(this).width();
					var borderH =$(this).outerHeight() - $(this).height();
					var x = round((m/2 + (a - m/2)*cos(rad) + (b- m/2)*sin(rad) + 
								 (d - params.height - borderW)*sin(rad)*(sin(rad) - 1)/2
								 -(params.width + borderW)*cos(rad)*(cos(rad) - 1)/2)*(1 + delta*nZoom));
					var y = round((abs(-m/2 + (a - m/2)*sin(rad) - (b- m/2)*cos(rad) - d*cos(rad)*(cos(rad) - 1)/2)
							 - (params.height + borderH)*cos(rad)*(cos(rad) - 1)/2
							 - (params.width + borderH)*sin(rad)*(1 + sin(rad))/2)*(1 + delta*nZoom));
                	$(element).css({
						left: x,
						top: y,
						width: $(this).height(),
						height: $(this).width()
					});
            	});
		}
		function adjustImage(){
			if(angle%360 == 90 || angle%360 == -270){
				var origin = round((originalSize.w/2)*(1+delta*nZoom));
				$area.children('img').css({
					"-webkit-transform" :"rotate(270deg)",
					"-webkit-transform-origin": "50% " + origin + "px"
				});
			}else if(abs(angle)%360 == 180){
				$area.children('img').css({
					"-webkit-transform" :"rotate(180deg)",
					"-webkit-transform-origin": "50% 50%"
				});
			}else if(angle%360 == 270 || angle%360 == -90){
				var origin = round((originalSize.h/2)*(1+delta*nZoom));
				$area.children('img').css({
					"-webkit-transform" :"rotate(90deg)",
					"-webkit-transform-origin": origin
				});
			}else{
				$area.children('img').css({
					"-webkit-transform" :"rotate(0deg)",
					"-webkit-transform-origin": "50% 50%"
				});
			}
		}
		
		function getXo(Xn){
			var _Xo = Xn/(1+nZoom*delta);
		  	return _Xo;
		}
		function zoomIn(_Xo){
		  	var _Xn = _Xo*(1+(nZoom+1)*delta);
		  	if(_Xn && _Xn>0)
				return _Xn;
		  	return _Xo;
		}
		this.zoomInX = zoomIn;
		function zoomOut(_Xo){
		  	var _Xn = _Xo*(1+(nZoom-1)*delta);
		  	if(_Xn && _Xn>0)
				return _Xn;
		  	return _Xo;
		}
		this.zoomOutX = zoomOut;
		function zoomElement(x,Xo){
			return zoomIn(Xo);
		}
		function zoomElementOut(x,Xo){
			return zoomOut(Xo);
		}
		function performZoom(){
			var w = zoomElement($area.width(), rotatedSize.w);
			$area.width(w);
			var h = zoomElement($area.height(), rotatedSize.h);
			$area.height(h);
			var children = $area.children();
			$.each(children, function(index, element) {
				if($(element).prop("tagName") == "IMG" ){
					if(Math.round(cos(angle*pi/180))==0)
						$(this).width($(this).parent().height());
					else
						$(this).width($(this).parent().width());
				}
				else 
					if($(element).attr("class")!="helper"){
						var params = JSON.parse($(element).children('.params').val());
						var bW = ($(this).outerWidth() - $(this).width())/2;
						var bH = ($(this).outerHeight() - $(this).height())/2;
						var xc = $(this).offset().left - $(this).parent().offset().left;
						var rotatedW = params.width*round(Math.pow(cos(angle*pi/180),2)) 
									+ params.height*round(Math.pow(sin(angle*pi/180),2));
						var rotatedH = params.height*round(Math.pow(cos(angle*pi/180),2)) 
									+ params.width*round(Math.pow(sin(angle*pi/180),2));
						var x0 = params.left;
//						if(angle%360 == -90 || angle%360 == 270)
							x0 = getXo(xc);
						var xn = zoomElement(xc, x0);
						
						
						var yc = $(this).offset().top - $(this).parent().offset().top;
						var y0 = params.top;
						y0 = getXo(yc);
						var yn = zoomElement(yc,y0);
						
						
						var w = $(element).width();
						var wn = zoomElement(w, rotatedW);
						
						var h = $(element).height();
						var hn = zoomElement(h,rotatedH);

						$(element).css("left",xn);
						$(element).css("top",yn);
						$(element).width(wn);
						$(element).height(hn);
						
						
					}
            });
		}
		function performZoomOut(){
		    var w = zoomElementOut($area.width(), rotatedSize.w);
		    $area.width(w);
		    var h = zoomElementOut($area.height(), rotatedSize.h);
		    $area.height(h);
		    var children = $area.children();
			$.each(children, function(index, element) {
				if($(element).prop("tagName") == "IMG" ){
					if(Math.round(cos(angle*pi/180))==0)
						$(this).width($(this).parent().height());
					else
						$(this).width($(this).parent().width());
				}
				else 
					if($(element).attr("class")!="helper"){
						var params = JSON.parse($(element).children('.params').val());
						
						var xc = $(this).offset().left - $(this).parent().offset().left;
						var rotatedW = params.width*round(Math.pow(cos(angle*pi/180),2)) 
									+ params.height*round(Math.pow(sin(angle*pi/180),2));
						var rotatedH = params.height*round(Math.pow(cos(angle*pi/180),2)) 
									+ params.width*round(Math.pow(sin(angle*pi/180),2));
						var x0 = params.left;
							x0 = getXo(xc);
						var xn = zoomElementOut(xc, x0);
						
						var yc = $(element).offset().top - $(this).parent().offset().top;
						var y0 = params.top;
							y0 = getXo(yc);
						var yn = zoomElementOut(yc,y0);
						
						var w = $(element).width();
						var wn = zoomElementOut(w, rotatedW);
						
						var h = $(element).height();
						var hn = zoomElementOut(h,rotatedH);
						$(element).css("left", round(xn));
						$(element).css("top",round(yn));
						$(element).width(wn);
						$(element).height(hn);
					}
            });
		}
		function zI(){
			original();
			performZoom();
			nZoom++;
			adjustImage();
			centering();
		}
		
		function zO(){
			if($area.width() > $area.parent().width()){
				original();
				performZoomOut();
				nZoom--;
				adjustImage();
				centering();
			}
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

		function rotateClockwise(){
			original();
			angle-=90;
            rotate(angle);
			centering();
		}
		
		function rotateCounterClockwise(){
			original();
			angle+=90;
            rotate(angle);
			centering();
		}
		
		//Utility
		function log(txt){
			$('#console').html(txt);
		}
		function focusMe(element){
			clearFocus();
			$(element).addClass('anno_focus');
		}
		function clearFocus(){
			$(".anno_focus").removeClass('anno_focus');
		}
		function powCos(angle){
			//var radian = deg*Math.PI/180;
			return Math.pow(cos(angle),2);
		}
		function powSin(angle){
			//var radian = deg*Math.PI/180;
			return Math.pow(sin(angle),2);
		}
		function centering(){
			if(!center){
				var left = ($area.parent().width() - $area.width())/2 + $area.parent().offset().left;
				var top = ($area.parent().height() - $area.height())/2 + $area.parent().offset().top;
				//$area.css({left: left, top: top, position: "absolute"});
				center = true;
				$area.css({position: "relative", margin:"0 auto"});
			}
		}
		function original(){
			if(center){
				var dx = $area.offset().left;
				var dy = $area.offset().top;
				$area.css({left: 0, top: 0, position: "absolute"});
				$(this).children(":not(img,.helper)").each(function(index, element) {
					var x = $(this).offset().left - dx;
					var y = $(this).offset().top - dy;
					$(this).css({left:x,top:y});
				});
				center = false;
			}
		}
		function setDefaultClass(className){
			styleClass = className;
		}
	    //Public API
		this.scrollable = function(){
		    scrollable = true;
		    $area.css("cursor", "move");
		}
		this.drawable = function () {
		    scrollable = false;
		    $area.css("cursor", "crosshair");
		}
		this.hide = function () {
		    $area.children(":not(img)").css("visibility","hidden");
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
		this.width = function () { return $area.width(); }
		this.height = function () { return $area.height(); }
		this.rotateAngle = function () { return angle; }
		return this;
	}
	
	function AnnotationCapture(area,opt){
		var styleOCR = {
			"border": "1px solid green",
			"background-color": "#CC3"
		}
		var ocrFields;
		if(opt && opt.fields)
			ocrFields = opt.fields;
		Annotation.call(this,area);
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
		    //var $text = $('<div class="anno_text_content"></div>');
		    //var $editable = $("<a href='#' class='editable-button empty'>Select...</a>");

		    //$editable.css({ "border-bottom": "0", "overflow": "hidden" });
		    //$editable.attr("data-type", "select");
		    //$box.append($text);
		    //$editable.editable({
		    //    title: "Select field",
		    //    display: function (value) {
		    //        if (!$(this).hasClass('empty'))
		    //            $text.text(value);
		    //        $(this).removeClass("editable-unsaved");
		    //    },
		    //    source: ocrFields,
		    //    onblur: 'submit',
		    //    defaultValue: ' ',
		    //    savenochange: true,
		    //    //showbuttons: false
		    //});
		    //$editable.on('shown', function (e, editable) {
		    //    $editable.removeClass('empty');
		    //    editable.input.$input.val('Enter text here...');
		    //});

		    //$box.append($editable.hide());
		    //$box.bind('mouseenter', function () {
		    //    //$text.hide();
		    //    $editable.show();
		    //    var l = $box.width() / 2 - $editable.width() / 2;
		    //    var t = $box.height() / 2 - $editable.height() / 2;
		    //    $editable.css({ left: l, top: t, position: 'absolute' });
		    //})
		    //$box.bind('mouseleave', function () {
		    //    // $text.show();
		    //    $editable.hide();
		    //});

		    //$.fn.editableform.buttons = '<button type="submit" class="editable-submit">OK</button>' +
			//							'<button type="button" class="editable-cancel">Cancel</button>';
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
			$box.append($options);
			$.fn.editableform.buttons = '<button type="submit" class="editable-submit">OK</button>' +
										'<button type="button" class="editable-cancel">Cancel</button>';
		}
		create = function(params){
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
		        jsObj.left = /*$(this).offset().left*/ $(this).parent().offset().left + parseFloat($(this).css('left')) - $(area).offset().left;//_this.zoomX();
		        jsObj.top = /*$(this).offset().top*/ $(this).parent().offset().top + parseFloat($(this).css('top')) - $(area).offset().top;//_this.zoomX();
		        jsObj.width =  $(this).width();//_this.zoomX();
		        jsObj.height = $(this).height();//_this.zoomX();

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
	}
	//AnnotationCapture.prototype = new Annotation();
	//AnnotationCapture.constructor = AnnotationCapture;
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





    /////////////Include more
    /////////////jquery-scrollTo.js
	var $scrollTo = $.scrollTo = function (target, duration, settings) {
	    $(window).scrollTo(target, duration, settings);
	};

	$scrollTo.defaults = {
	    axis: 'xy',
	    duration: parseFloat($.fn.jquery) >= 1.3 ? 0 : 1,
	    limit: true
	};

    // Returns the element that needs to be animated to scroll the window.
    // Kept for backwards compatibility (specially for localScroll & serialScroll)
	$scrollTo.window = function (scope) {
	    return $(window)._scrollable();
	};

    // Hack, hack, hack :)
    // Returns the real elements to scroll (supports window/iframes, documents and regular nodes)
	$.fn._scrollable = function () {
	    return this.map(function () {
	        var elem = this,
				isWin = !elem.nodeName || $.inArray(elem.nodeName.toLowerCase(), ['iframe', '#document', 'html', 'body']) != -1;

	        if (!isWin)
	            return elem;

	        var doc = (elem.contentWindow || elem).document || elem.ownerDocument || elem;

	        return /webkit/i.test(navigator.userAgent) || doc.compatMode == 'BackCompat' ?
				doc.body :
				doc.documentElement;
	    });
	};

	$.fn.scrollTo = function (target, duration, settings) {
	    if (typeof duration == 'object') {
	        settings = duration;
	        duration = 0;
	    }
	    if (typeof settings == 'function')
	        settings = { onAfter: settings };

	    if (target == 'max')
	        target = 9e9;

	    settings = $.extend({}, $scrollTo.defaults, settings);
	    // Speed is still recognized for backwards compatibility
	    duration = duration || settings.duration;
	    // Make sure the settings are given right
	    settings.queue = settings.queue && settings.axis.length > 1;

	    if (settings.queue)
	        // Let's keep the overall duration
	        duration /= 2;
	    settings.offset = both(settings.offset);
	    settings.over = both(settings.over);

	    return this._scrollable().each(function () {
	        // Null target yields nothing, just like jQuery does
	        if (target == null) return;

	        var elem = this,
				$elem = $(elem),
				targ = target, toff, attr = {},
				win = $elem.is('html,body');

	        switch (typeof targ) {
	            // A number will pass the regex
	            case 'number':
	            case 'string':
	                if (/^([+-]=)?\d+(\.\d+)?(px|%)?$/.test(targ)) {
	                    targ = both(targ);
	                    // We are done
	                    break;
	                }
	                // Relative selector, no break!
	                targ = $(targ, this);
	                if (!targ.length) return;
	            case 'object':
	                // DOMElement / jQuery
	                if (targ.is || targ.style)
	                    // Get the real position of the target 
	                    toff = (targ = $(targ)).offset();
	        }
	        $.each(settings.axis.split(''), function (i, axis) {
	            var Pos = axis == 'x' ? 'Left' : 'Top',
					pos = Pos.toLowerCase(),
					key = 'scroll' + Pos,
					old = elem[key],
					max = $scrollTo.max(elem, axis);

	            if (toff) {// jQuery / DOMElement
	                attr[key] = toff[pos] + (win ? 0 : old - $elem.offset()[pos]);

	                // If it's a dom element, reduce the margin
	                if (settings.margin) {
	                    attr[key] -= parseInt(targ.css('margin' + Pos)) || 0;
	                    attr[key] -= parseInt(targ.css('border' + Pos + 'Width')) || 0;
	                }

	                attr[key] += settings.offset[pos] || 0;

	                if (settings.over[pos])
	                    // Scroll to a fraction of its width/height
	                    attr[key] += targ[axis == 'x' ? 'width' : 'height']() * settings.over[pos];
	            } else {
	                var val = targ[pos];
	                // Handle percentage values
	                attr[key] = val.slice && val.slice(-1) == '%' ?
						parseFloat(val) / 100 * max
						: val;
	            }

	            // Number or 'number'
	            if (settings.limit && /^\d+$/.test(attr[key]))
	                // Check the limits
	                attr[key] = attr[key] <= 0 ? 0 : Math.min(attr[key], max);

	            // Queueing axes
	            if (!i && settings.queue) {
	                // Don't waste time animating, if there's no need.
	                if (old != attr[key])
	                    // Intermediate animation
	                    animate(settings.onAfterFirst);
	                // Don't animate this axis again in the next iteration.
	                delete attr[key];
	            }
	        });

	        animate(settings.onAfter);

	        function animate(callback) {
	            $elem.animate(attr, duration, settings.easing, callback && function () {
	                callback.call(this, target, settings);
	            });
	        };

	    }).end();
	};

    // Max scrolling position, works on quirks mode
    // It only fails (not too badly) on IE, quirks mode.
	$scrollTo.max = function (elem, axis) {
	    var Dim = axis == 'x' ? 'Width' : 'Height',
			scroll = 'scroll' + Dim;

	    if (!$(elem).is('html,body'))
	        return elem[scroll] - $(elem)[Dim.toLowerCase()]();

	    var size = 'client' + Dim,
			html = elem.ownerDocument.documentElement,
			body = elem.ownerDocument.body;

	    return Math.max(html[scroll], body[scroll])
			 - Math.min(html[size], body[size]);
	};

	function both(val) {
	    return typeof val == 'object' ? val : { top: val, left: val };
	};
}(jQuery));