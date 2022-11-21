(function ( $ ) {
	$.fn.ecm_loading_show = function (options) {
		var tag_parent = this;
		var settings = $.extend({
			// These are the defaults.
		}, options);
				
		var load_bg = '<div class="ecm_loading_bg"><div class="load"><img src="Images/loading/preloader-black.gif" /></div></div>';
		
		// Add loader in body
		
		$(tag_parent).append($(load_bg));
		$(tag_parent).children(".ecm_loading_bg").children(".load").css("top", $(tag_parent).children(".ecm_loading_bg").outerHeight(true)/2 - 16);
		$(tag_parent).children(".ecm_loading_bg").children(".load").css("left", $(tag_parent).children(".ecm_loading_bg").outerWidth(true)/2 - 16);
		$(tag_parent).live("scroll", function (e) { alert("a"); });
		//$(load_bg).find('img').css({ width: '16', height: '16' });
	}
	
	$.fn.ecm_loading_hide = function (options) {
		var tag_parent = this;
		var settings = $.extend({
			// These are the defaults.
		}, options);

		$(tag_parent).children(".ecm_loading_bg").remove();
	}
})(jQuery);

(function ($) {
    $.fn.ecm_disable = function () {
        var target = $(this);
        target.children('.ecm_loading_bg').remove();
        target.append($('<div class="ecm_loading_bg"><div class="load"></div></div>'));
    }

    $.fn.ecm_enable = function () {
        $(this).children(".ecm_loading_bg").remove();
    }
})(jQuery);

(function ($) {
    $.fn.ecm_in_active = function () {
        var target = $(this);
        target.children('.ecm_loading_bg').remove();
        target.append($('<div class="ecm_loading_bg in-active"><div class="load"></div></div>'));
    }

    $.fn.ecm_active = function () {
        $(this).children(".ecm_loading_bg").remove();
    }
})(jQuery);