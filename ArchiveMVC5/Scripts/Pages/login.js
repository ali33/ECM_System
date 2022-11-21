function checkusername () {
	var val = $("#signinform #signinform_form_username input").val();
	if (val != "")
	{
		return true;
	}
	else
	{
		return false;
	}
}

function changecheckusername () {
	if (checkusername ()) {
		if (!$("#signinform #signinform_form_username .check").hasClass("ok")) {
			$("#signinform #signinform_form_username .check").addClass("ok");
		}
	}else{
		if ($("#signinform #signinform_form_username .check").hasClass("ok")) {
			$("#signinform #signinform_form_username .check").removeClass("ok");
		}
	}
}

$(document).ready(function(e) {
	changecheckusername();
	$("#signinform #signinform_form_username input").bind("focusin", function (e) {
		changecheckusername();
	});
	
	$("#signinform #signinform_form_username input").bind("focusout", function (e) {
		changecheckusername();
	});
	
	$("#signinform .panner_left:not(.disabled)").bind("click", function (e) {
		$("#signinform .panner_left").each(function(index, element) {
            $(element).removeClass("open");
        });
		$(this).addClass("open");
	});
});