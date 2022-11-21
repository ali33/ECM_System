var windows_height = 0;

var resize_vetical = function ()
	{
		var window_height = $(window).height();
		var header_height = $("header.archive_header").outerHeight(true);
		var border = 0;
		if ($("section.body").css("border-top-width").substring(0, $("section.body").css("border-top-width").search("px")) > 0 )
		{
			border += $("section.body").css("border-top-width");
		}
		if ($("section.body").css("border-bottom-width").substring(0,$("section.body").css("border-bottom-width").search("px"))  > 0)
		{
			border += $("section.body").css("border-bottom-width");
		}
		$("section.body").height(window_height - header_height - border -5 );
	}

var resize_horizontal = function()
	{
		var window_width = $(window).width();
		var left_width = $("div.left_column").outerWidth(true);
		var menu_width = $("div.capture_content_menu").outerWidth(true);
		$("div.right_column").attr("left", left_width);
		$("div.right_column").width(window_width - left_width - menu_width);
	}

var resize_event = function ()
	{
		resize_vetical();
		resize_horizontal();
	}

var resize_vetical_document_list = function ()
	{
		var document_type_height = $(".document_type").height();
		var document_type_header_hight = $(".document_type > .document_type_header").outerHeight(true);
		$(".document_type > .document_type_content").height(document_type_height - document_type_header_hight);
	}

var resize_vetical_search = function ()
	{
		var search_height = $(".search").height();
		var search_query_height = $(".search > .search_query").outerHeight(true);
		$(".search > .result_query").height(search_height - search_query_height);
	}

	var disable_tabpress = function (e)
	{
		if( e.which == 9) {
			e.preventDefault();
		}
	}
	
	var resize_vetical_main_content = function (e)
	{
		var left_hight = $("div.left_column").height();
		var left_capture_header_height = $("div.left_column").find(".capture_header").height();
		var left_capture_button_mainbar_hight = $("div.left_column").find(".button_mainbar").outerHeight();
		$("div.left_column").find(".capture_main_content").height(left_hight - left_capture_header_height - left_capture_button_mainbar_hight);
	}

    //Loc Ngo

	function getBrowserName(){
	    var Browser = navigator.userAgent;
	    if (Browser.indexOf('MSIE') >= 0){
	        Browser = 'MSIE';
	    }else if (Browser.indexOf('Firefox') >= 0){
	        Browser = 'Firefox';
	    }else if (Browser.indexOf('Chrome') >= 0){
	        Browser = 'Chrome';
	    }else if (Browser.indexOf('Safari') >= 0){
	        Browser = 'Safari';
	    }else if (Browser.indexOf('Opera') >= 0){
	        Browser = 'Opera';
	    }else{
	        Browser = 'UNKNOWN';
	    }
	    return Browser;
	}

	var resize_vetical_second_main_admin = function (e) {
	    var left_height = $(".left_column_container_content").height();
	    if (left_height == null)
	        left_height = 0;
	    var title_height = $(".left_column_container_content").find("div.archive_admin_title").height();
	    if (title_height == null)
	        title_height = 0;
	    var other_height = $(".left_column_container_content").find("div.archive_admin_other").height();
	    if (other_height == null)
	        other_height = 0;
	    var button_height = $(".left_column_container_content").find("div.archive_button_content").height();
	    if (button_height == null)
	        button_height = 10;
	    var orc_button_height = $(".left_column_container_content").find("div.archive_button_content_orc").height();
	    if (orc_button_height == null)
	        orc_button_height = 0;
	    $(".left_column_container_content").find("div.archive_admin_content").height(left_height - title_height - other_height - button_height - orc_button_height);
	}

	var resize_vetical_properties_content = function (e) {
	    var right_height = $(".right_column_container_content").height();
	    if (right_height == null)
	        right_height = 0;
	    var title_height = $(".right_column_container_content").find("div.right_properties_title").height();
	    if (title_height == null)
	        title_height = 0;
	    var other_height = $(".right_column_container_content").find("div.right_properties_other").height();
	    if (other_height == null)
	        other_height = 0;
	    var button_height = $(".right_column_container_content").find("div.right_properties_button").height();
	    if (button_height == null)
	        button_height = 10;
	    $(".right_column_container_content").find("div.right_properties_content").height(right_height - title_height - other_height - button_height);
	}
    
	var resize_vetical_properties_top_down_height = function (e) {
	    var $sub_properties = $(".sub_properties");
	    var sub_properties_height = $sub_properties.height();
	    var sub_properties_top_height = $sub_properties.find(".sub_properties_top").height();
	    if (sub_properties_top_height == null) {
	        sub_properties_top_height = 0;
	    } else {
	        sub_properties_top_height = sub_properties_top_height + 30;
	    }
	    var sub_properties_between_height = $sub_properties.find(".sub_properties_between").height();
	    if (sub_properties_between_height == null) {
	        sub_properties_between_height = 0;
	    } else {
	        sub_properties_between_height = sub_properties_between_height + 15;
	    }
	    var properties_top_height = sub_properties_top_height + sub_properties_between_height;

	    $sub_properties.find(".down_height").height(sub_properties_height - properties_top_height - 20);
	}

	var resize_vetical_bewteen_top_down_height = function (e) {
	    var $between_and_right = $(".between_and_right");
	    var between_and_right_height = $between_and_right.height();
	    var between_top_height = $between_and_right.find(".top_height").height();
	    $between_and_right.find(".down_height").height(between_and_right_height - between_top_height);
	}

	var resize_vetical_properties_width = function (e) {
	    var windows_width = $(window).width();
	    var left_menu_width = $(".archive_admin_menu").width();
	    var between_sub_menu = $(".admin_sub_menu").width();
	    $(".sub_properties").width(windows_width - left_menu_width - between_sub_menu);
	}
    
	var reszie_vetical_fieldset = function (e) {
	    var $fieldset_top = $(".down_height").find(".sub_properties_mutli_data").find(".parent_fieldset");
	    var $fieldset_down = $(".top_height").find(".between_content").find(".parent_fieldset");
	    var properties_size = $(".sub_properties").width();
	    $fieldset_top.width(properties_size - 50);
	    $fieldset_down.width(properties_size - 50);
	}

	var resize_vertical_between_fieldset = function (e) {
	    var $top_height = $(".between_and_right").find(".top_height").height();
	    var $between_height = $(".between_and_right").height();
	    $(".between_and_right").find(".down_height").height($between_height - $top_height);

	    var $fieldset_top = $(".top_height").find(".parent_fieldset");
	    var $fieldset_down = $(".down_height").find(".between_multi_data").find(".parent_fieldset");
	    var $between_width = $(".between_and_right").width();
	    var $down_height = $(".between_and_right").find(".down_height").height();
	    $fieldset_down.height($down_height - 25 - 30);
	    $fieldset_top.width($between_width - 50);
	    $fieldset_down.width($between_width - 50);
	}

	var resize_vetical_windows_height = function(e){
	    var tmp = $(window).height();
	    if (tmp > windows_height)
	        $(window).height(windows_height);
	}

	var resize_vertical_multi_data = function (e) {
	    var $fieldset = $(".sub_properties").find(".down_height").find(".sub_properties_mutli_data").find(".parent_fieldset");
	    var down_height = $(".sub_properties").find(".down_height").height();
        //set fieldset height
	    $fieldset.height(down_height - 20 - 30);
	    var fieldset_height = $fieldset.height();
	    var button_height = $fieldset.find(".multi_button").height() + 20;
	    $fieldset.find(".multi_data").height(fieldset_height - button_height);
	}

	var sub_menu_height = function (e) {
	    var total_height = $(".admin_sub_menu").height();
	    var other_height = $(".admin_sub_menu").find(".admin_sub_other").height();
	    if (other_height == null)
	        other_height = 0;
	    var button_height = $(".admin_sub_menu_button").height() + 10;
	    $(".admin_sub_menu").find(".admin_sub_menu_items").height(total_height - button_height - other_height - 10);
	}

	var setButtonWidthInFirefox = function () {
	    if (getBrowserName() == 'Firefox') {
	        $(".archive_admin_container").find(".archive_admin_button").width(80);
	        $(".archive_admin_container").find(".properties_button").width(60);
	    }
	}

    //End Loc Ngo

	$(document).ready(function (e) {

	    windows_height = $(window).height();
	    resize_event();
	    resize_vetical_document_list();
	    resize_vetical_search();
	    resize_vetical_main_content();
	    resize_vetical_properties_width();

	    $(window).resize(function(e) {
		    resize_event();
		    resize_vetical_document_list();
		    resize_vetical_search();
		    resize_vetical_main_content();
		    resize_vetical_properties_top_down_height();
		    resize_vetical_properties_width();
		    sub_menu_height();
		    reszie_vetical_fieldset();
		    resize_vertical_multi_data();
		    resize_vertical_between_fieldset();
	    })
	
    //Resize second menu of archive admin
	$(document).on("load", ".left_column_container_content", resize_vetical_second_main_admin);

	/* effect for login form */
		// This is event to close error box on login form
	//$(".signin_error_box a.close").live("click", function (e) {
	//	e.preventDefault();
	//	$(this).parents(".signin_error_box").css("display", "none");
	//});
		
	
	/* resize nav search */
	$(".body_search > .document_type > .nav_resize").live("click", function (e) {
		if ($(".body_search > .document_type").outerWidth(true) < 280) {
			$(".body_search > .document_type").addClass("open").removeClass("collapse");
			$(".body_search > .search").width($(".body_search > .search").width() - 248);
		}else{
			$(".body_search > .document_type").addClass("collapse").removeClass("open");
			$(".body_search > .search").width($(".body_search > .search").width() + 248);
		}
	});
	
	/* resize nav capture */
	$(".body_capture > .capture_sitebar > .nav_resize").live("click", function (e) {
		if ($(".body_capture > .capture_sitebar").outerWidth(true) < 340) {
			$(".body_capture > .capture_sitebar").addClass("open").removeClass("collapse");
			$(".body_capture > .capture").width($(".body_capture > .capture").width() - 308);
		}else{
			$(".body_capture > .capture_sitebar").addClass("collapse").removeClass("open");
			$(".body_capture > .capture").width($(".body_capture > .capture").width() + 308);
		}
	});
	
	
	/* resize query search */
	
	$(".body_search > .search > .search_query .que_resize").live("click", function (e) {
		var bt =  this;
		if($(bt).parents(".search_query").hasClass("open")){
			$(bt).parents(".search_query").addClass("collapse").removeClass("open");
			$(".search > .result_query").height($(".body_search > .search").height() - $(".search > .search_query").height());
		}else{
			$(bt).parents(".search_query").addClass("open").removeClass("collapse");
			$(".search > .result_query").height($(".body_search > .search").height() - $(".search > .search_query").height());
		}
	});
	
	/* resize result */
	
	$(".body_search .result_query .result_resize").live("click", function (e) {
		var bt = this;
		if($(bt).parents(".datagript_result > div").hasClass("collapse")){
			$(bt).parents(".datagript_result > div").removeClass("collapse");
		}else{
			$(bt).parents(".datagript_result > div").addClass("collapse");
		}
	});
	
	
	
	/* input control focus */	
		// This is evernt focus on textbox to add class "focus" to div parent
	$(".input-control > input[type='text']").live("focusin", function (e) {
		$(this).parent(".input-control").addClass("focus");
	});
	
	$(".input-control > input[type='text']").live("focusout", function (e) {
		$(this).parent(".input-control").removeClass("focus");
	});
	
	/* Focus textarea */
	
	$(".input-control > textarea").live("focusin", function (e) {
		$(this).parent(".input-control").addClass("focus");
	});
	
	$(".input-control > textarea").live("focusout", function (e) {
		$(this).parent(".input-control").removeClass("focus");
	});

	/* input control number */
	$(".input-control.number > input[type='text']").keydown(function (e) {
		// Allow: backspace, delete, tab, escape, and enter
        if ( event.keyCode == 46 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 || event.keyCode == 13 || 
             // Allow: Ctrl+A
            (event.keyCode == 65 && event.ctrlKey === true) || 
             // Allow: home, end, left, right
            (event.keyCode >= 35 && event.keyCode <= 39)) {
                 // let it happen, don't do anything
                 return;
        }
        else {
            // Ensure that it is a number and stop the keypress
            if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105 )) {
                event.preventDefault(); 
            }   
        }
	});
	
	/* button close */
	
	$(".input-control > button.close").live("click", function (e) {
		e.preventDefault();
		var bt =  this;
		$(bt).parent(".input-control").find("input[type='text']").val("");
	});
	
	/* datepicker */
	
		// This event to show datepicker when textbox focus
		
		// This is action when user click delete button on new fields in query.
	$(".layout_table button.delete-new").live("click", function (e) {
		e.preventDefault();
		var bt = $(this);
		$(bt).parents("div.new").remove();
	});
	
	/* drag bar */
	
	///THODINH: UPDATE
	/* leftbar lable */
	
	$(".capture_leftbar > .leftbar_lable").live("click", function (e) {
		$(".capture_leftbar > .leftbar_lable").removeClass("leftbar_lable_focus");
		$(this).addClass("leftbar_lable_focus");
		//$(".capture_mainbar .capture_main_content").toggleClass("thumbnails indexs");
		if ($(this).hasClass('indexs')) {
		    $(".capture_mainbar .capture_main_content").removeClass("thumbnails");
		    $(".capture_mainbar .capture_main_content").addClass("indexs");
		    $('#croppedImage').show(400, function () {
		        if ($("img", this).attr("src") == "#")
		            $(this).css("visibility", "hidden");
		    });
		}
		else {
		    $(".capture_mainbar .capture_main_content").removeClass("indexs");
		    $(".capture_mainbar .capture_main_content").addClass("thumbnails");
		    $('#croppedImage').hide();
		}
	});
	//$(".capture_leftbar > .leftbar_lable").click(function (e) {

	//});
	/////////////END UPDATE
	/* pagination */
	
		// This event active the pagination link.
	
	$(".pagination a").live("click", function (e) {
		e.preventDefault();
		if (!$(this).parent("li").hasClass("disabled")) {
			$(this).parents(".pagination").children("li").removeClass("active");
			$(this).parent("li").addClass("active");
		}
	});
	
	
	/* current_content_fields */
		// This event and and remove class "hasvalue" for text box.
		
	$(".content_fields.mandatory .input-control input").live("keyup",function (e) {
		if ($(this).val() == ""){
			$(this).parents().removeClass("hasvalue");
		}else{
			$(this).parents().addClass("hasvalue");
		}
	});
	
});
