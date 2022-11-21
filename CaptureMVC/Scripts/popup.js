(function ( $ ) {	
	$.fn.ecm_popup = function (options) {
		
		var settings = $.extend({
			// These are the defaults.
			width: 250,
			index: 9999,
			function_bt_first: "", // Function for first bt
			function_bt_second: "", // Function for second bt
			function_bt_third: "", // Function for third bt
			hide: false
			}, options);
		
		// Hide popup
		
		function disabled_tab (e) {
			if (e.keyCode == 9) {
				$(".popup_bg input:first").focus();
			}
		}
		
		if (settings.hide == true)
		{
			$(".popup_bg").remove();
			$(popup_bg).find(":button:first").unbind("click");
			$(popup_bg).find(":button:eq(1)").bind("click");
			$(popup_bg).find(":button:last").bind("click");
			$(document).unbind("keyup", disabled_tab);
		}		
		
		// Show popup
		if (settings.hide == false)
		{
			var this_popup = $(this);
			var popup_ct = $(this_popup).clone();
			var popup_bg = '<div class="popup_bg ecm_popup"><div class="popup_layer"></div></div>';
			
			//Add the popup for bottom the body tag.
			$("body").append($(popup_bg));
			$("body").children(".popup_bg").children(".popup_layer").append($(popup_ct));
			
			// Get the popup background.
			popup_bg = $(".popup_bg").first();
			
			// Set all the attributes for the popup layer.
			$(popup_bg).find(".popup_layer").css("z-index", settings.index);
			$(popup_bg).find(".popup_layer").css("width", settings.width);
			
			// Event.
			var number_bt = $(popup_bg).find(":button").length;
			$(document).bind("keyup", disabled_tab);
			
			
			if (number_bt == 1) {
				$(popup_bg).find(":button:first").bind("click", settings.function_bt_first);
			}else if (number_bt == 2) {
				$(popup_bg).find(":button:first").bind("click", settings.function_bt_first);
				$(popup_bg).find(":button:last").bind("click", settings.function_bt_second);
			}else if (number_bt == 3) {
				$(popup_bg).find(":button:first").bind("click", settings.function_bt_first);
				$(popup_bg).find(":button:eq(1)").bind("click", settings.function_bt_second);
				$(popup_bg).find(":button:last").bind("click", settings.function_bt_third);
			}			
		}
	};
})( jQuery );

