$(document).ready(function(e) {
	/* Sign in pages control */
	$("form.formsignin").live("submit", function (e){
		e.preventDefault();
		$(".signin_error_box").css("display", "block");
	});
	/* Search archive*/
	$(".body.archive").find(".documenttype").find(".documenttype_header .show_bt").live("click", function (e) {
		var bt = this;
		$(bt).toggleClass("type_collapse");
		$(bt).toggleClass("type_show");
		$(bt).parents(".documenttype").toggleClass("type_collapse");
		$(bt).parents(".documenttype").toggleClass("type_show");
		$(".search").toggleClass("show_left");
	});
	
	$(".body.archive").find(".searchqueryform").find("form").live("click", function (e) {
	    e.preventDefault();
	});
	
	$(".body.archive").find(".search").find(".query_control_button").find("span.show_bt").live("click", function (e) {
	    var bt = this;
	    $(bt).toggleClass("query_show");
	    $(bt).toggleClass("query_collapse");
	    $(bt).parents(".search_query").toggleClass("query_collapse");
	    $(bt).parents(".search_query").toggleClass("query_show");
		$("div.result_query").toggleClass("max_size");
		$("div.result_query").toggleClass("min_size");
	});
	/* Capture */
	$(".body.archive").find(".capture_sitebar").find(".show_bt").live("click", function (e) {
		var bt = this;
		$(bt).toggleClass("capture_show");
		$(bt).toggleClass("capture_collapse");
		$(bt).parents(".capture_sitebar").toggleClass("type_show");
		$(bt).parents(".capture_sitebar").toggleClass("type_collapse");
		$(".capture").toggleClass("show_left");
	});	
	
	$(".body.archive").find(".capture_sitebar").find(".capture_header_feature").find("div.span_show").live("click", function (e) {
		var bt = this;
		$(bt).focus();
	});
	
	/* show drowdown menu capture_header*/
	
	$(".body.archive").find(".capture_sitebar").find(".capture_header_feature").find("div.span_show").live("focus", function (e) {
		var bt = this;
		
		var clearDropdown = function(selector) {
			$(bt).parents(".capture_header").find(".capture-dropdown-menu").each(function(index, element) {
				if ( $(this) != $(selector) && $(this).hasClass("open") ){
					$(this).removeClass("open");
				}
			});
		};
		
	 	clearDropdown(bt);
		
		$(bt).find(".capture-dropdown-menu").addClass("open");
	});
	
	/* hide drowdown menu capture_header*/
	
	$(".body.archive").find(".capture_sitebar").find(".capture_header_feature").live("mouseleave", function (e) {
		var bt = this;
		$(this).find(".capture-dropdown-menu.open").removeClass("open");
	});
})


