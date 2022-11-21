(function ( $ ) {
	$.fn.ecm_loading_show = function (options) {
		var tag_parent = this;
		var settings = $.extend({
			// These are the defaults.
		}, options);
				
		var load_bg = '<div class="ecm_loading_bg"><div class="load"><img src="/Images/loading/preloader-black.gif" alt="" /></div></div>';
		$(".wrapper").addClass("disabled");
		// Add loader in body
		
		$(tag_parent).append($(load_bg));
		$(tag_parent).children(".ecm_loading_bg").children(".load").css("top", $(tag_parent).children(".ecm_loading_bg").outerHeight(true)/2 - 16);
		$(tag_parent).children(".ecm_loading_bg").children(".load").css("left", $(tag_parent).children(".ecm_loading_bg").outerWidth(true)/2 - 16);
		//$(tag_parent).live("scroll", function (e) { alert("a"); });
		$(load_bg).find('img').css({ width: '16', height: '16' });
	}
	
	$.fn.ecm_loading_hide = function (options) {
		var tag_parent = this;
		var settings = $.extend({
			// These are the defaults.
		}, options);
		$(".wrapper").removeClass("disabled");

		$(tag_parent).children(".ecm_loading_bg").remove();
	}
}) ( jQuery );
