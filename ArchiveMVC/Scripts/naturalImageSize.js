(function ($) {
    function img(url) {
        var i = new Image;
        i.src = url;
        return i;
    }

    if ('naturalWidth' in (new Image)) {
        $.fn.naturalWidth = function () { return this[0].naturalWidth; };
        $.fn.naturalHeight = function () { return this[0].naturalHeight; };
        return;
    }
    $.fn.naturalWidth = function () { return img(this.src).width; };
    $.fn.naturalHeight = function () { return img(this.src).height; };
})(jQuery);