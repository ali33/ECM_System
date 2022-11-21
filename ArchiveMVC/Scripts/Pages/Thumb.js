(function ($) {
    Thumb = function (defaultAngle, thumb) {
        var angle = 0;

        angle = parseInt(defaultAngle);
        var delta = 0.1;
        var nZoom = 0;
        var $thumb = $(thumb);
        var $img = $thumb.find("img");

        var load = function () { };
        this.ready = function (_load) {
            load = _load;
        };

        $img.load(function () {
            rotate(angle);
            load.call(this);
        });

        function createThumb(item, index) {
            var imageUrl = URL_LoadImage + "/?key=" + item.KeyCache + "&thumb=true";
            var dpi = "";
            if (item.FileType == "image")
                dpi = item.Resolution + " dpi";
            //Sau khi submit thanh cong server tra ve danh sach cac key,tuong ung voi so trang cua image
            //Hien thi thumbnail cac trang tren left menu
            var $item = $('<li data-page-id="' + (item.PageId ? item.PageId : "")
                    + '"><input type="hidden" class="language" value="eng"/>'
                    + '<span class="page treeview_title" id="' + item.KeyCache + '">'
                    + '<a href="#"><img src="' + imageUrl + '" /><span>'
                    + '<strong class="pageNumber">' + (index + 1) + '</strong>'
                    + '<span> ' + dpi + '</span>'
                    + '</span></a></span></li>');
            $viewer = $('<input type="hidden" class="viewer" value="' + item.FileType + '"/>');
            $item.append($viewer);
            $item.data('key', item.KeyCache);
            return $item;
        }

        function rotateThumbLeft() {
            angle -= 90;
            adjustImage($thumb.find("img"));
        }

        function rotateThumbRight() {
            angle += 90;
            adjustImage($thumb.find("img"));
        }

        function rotate(rotateAngle) {
            angle = rotateAngle;
            adjustImage($thumb.find("img"));
        }

        function adjustImage($thumb) {
            if (angle % 360 == 90 || angle % 360 == -270) {
                var origin = round((originalSize.w / 2) * (1 + delta * nZoom));
                $thumb.children('img').css({
                    "transform": "rotate(270deg)",
                    "-ms-transform": "rotate(270deg)",
                    "-moz-transform": "rotate(270deg)",
                    "-webkit-transform": "rotate(270deg)",

                    "-webkit-transform-origin": "50% " + origin + "px",
                    "transform-origin": "50% " + origin + "px",
                    "-ms-transform-origin": "50% " + origin + "px",
                    "-moz-transform-origin": "50% " + origin + "px"
                });
            } else if (abs(angle) % 360 == 180) {
                $thumb.children('img').css({
                    "transform": "rotate(180deg)",
                    "-moz-transform": "rotate(180deg)",
                    "-ms-transform": "rotate(180deg)",
                    "-webkit-transform": "rotate(180deg)",
                    "-webkit-transform-origin": "50% 50%",
                    "transform-origin": "50% 50%",
                    "-ms-transform-origin": "50% 50%",
                    "-moz-transform-origin": "50% 50%"
                });
            } else if (angle % 360 == 270 || angle % 360 == -90) {
                var origin = round((originalSize.h / 2) * (1 + delta * nZoom));
                $thumb.children('img').css({
                    "transform": "rotate(90deg)",
                    "-moz-transform": "rotate(90deg)",
                    "-ms-transform": "rotate(90deg)",
                    "-webkit-transform": "rotate(90deg)",
                    "-webkit-transform-origin": origin,
                    "transform-origin": origin,
                    "-ms-transform-origin": origin,
                    "-moz-transform-origin": origin
                });
            } else {
                $thumb.children('img').css({
                    "transform": "rotate(0deg)",
                    "-moz-transform": "rotate(0deg)",
                    "-ms-transform": "rotate(0deg)",
                    "-webkit-transform": "rotate(0deg)",
                    "-webkit-transform-origin": "50% 50%",
                    "transform-origin": "50% 50%",
                    "-ms-transform-origin": "50% 50%",
                    "-moz-transform-origin": "50% 50%"
                });
            }
        }

        function adjustImage(content) {
            if (angle % 360 == 90 || angle % 360 == -270) {
                var origin = round((content.w / 2) * (1 + delta * nZoom));
                content.css({
                    "transform": "rotate(270deg)",
                    "-ms-transform": "rotate(270deg)",
                    "-moz-transform": "rotate(270deg)",
                    "-webkit-transform": "rotate(270deg)",

                    "-webkit-transform-origin": "50% " + origin + "px",
                    "transform-origin": "50% " + origin + "px",
                    "-ms-transform-origin": "50% " + origin + "px",
                    "-moz-transform-origin": "50% " + origin + "px"
                });
            } else if (abs(angle) % 360 == 180) {
                content.css({
                    "transform": "rotate(180deg)",
                    "-moz-transform": "rotate(180deg)",
                    "-ms-transform": "rotate(180deg)",
                    "-webkit-transform": "rotate(180deg)",
                    "-webkit-transform-origin": "50% 50%",
                    "transform-origin": "50% 50%",
                    "-ms-transform-origin": "50% 50%",
                    "-moz-transform-origin": "50% 50%"
                });
            } else if (angle % 360 == 270 || angle % 360 == -90) {
                var origin = round((content.h / 2) * (1 + delta * nZoom));
                content.css({
                    "transform": "rotate(90deg)",
                    "-moz-transform": "rotate(90deg)",
                    "-ms-transform": "rotate(90deg)",
                    "-webkit-transform": "rotate(90deg)",
                    "-webkit-transform-origin": origin,
                    "transform-origin": origin,
                    "-ms-transform-origin": origin,
                    "-moz-transform-origin": origin
                });
            } else {
                content.css({
                    "transform": "rotate(0deg)",
                    "-moz-transform": "rotate(0deg)",
                    "-ms-transform": "rotate(0deg)",
                    "-webkit-transform": "rotate(0deg)",
                    "-webkit-transform-origin": "50% 50%",
                    "transform-origin": "50% 50%",
                    "-ms-transform-origin": "50% 50%",
                    "-moz-transform-origin": "50% 50%"
                });
            }
        }

        function round(x) { return x; }

        this.rotateThumbLeft = rotateThumbLeft;
        this.rotateThumbRight = rotateThumbRight;
        this.rotate = rotate;
        return this;
    }

    //AnnotationCapture.prototype = new Annotation();
    //AnnotationCapture.constructor = AnnotationCapture;
    $.fn.thumbnail = function (angle) {
        this.each(function () {
            
            $(this).data("thumb", new Thumb(angle, this));
        });
        return $(this).data("thumb");
    }
}(jQuery));