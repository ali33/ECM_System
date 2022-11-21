$("#formUpload").submit(function () {
    var formData = new FormData($(this)[0]);

    $.ajax({
        url: $(this).attr("action"),
        type: 'POST',
        data: formData,
        async: false,
        dataType: "json",
        success: function (data) {
            alert(data);
            //location.reload();
            $('img').attr("src","http://localhost:24091/ImageProcessing/ID/?key="+ data[0].B+"&rote=0");
        },
        cache: false,
        contentType: false,
        processData: false
    });
    return false;
});
function uploadFile() {
    return;
    alert("Started");
    $("#uploadProgress").show();
    $.ajax({
        url: "/Home/UploadTest",
        type: "POST",
        data: $(this).serialize(),
        async: false,
        cache: false,
        beforeSend: function () {
            $("#uploadProgress").show()
        },
        complete: function () {
            $("#uploadProgress").html("Upload completed");
        },
        success: function (msg) {

            if (msg == "ok")
                $("#uploadProgress").hide();
            else
                alert("Error while uploading");

        }
    });
    return false;
}

$(document).ready(function () {

    $(document).on('mouseenter', ".hastooltip", function () {
        var $this = $(this);
        if (this.offsetWidth < this.scrollWidth && !$this.attr('title')) {

            //$this.tooltip({
            //    title: $this.text(),
            //    placement: "bottom"
            //});
            var options = {
                selector: '',
                title: $this.text(),
                container: 'body',
                placement: function (context, source) {
                    var position = $(source).position();

                    if (position.left < 200) {
                        return "right";
                    }

                    if (position.top < 200) {
                        return "bottom";
                    }

                    else {
                        return "left";
                    }
                }, trigger: "hover"
            };
            $this.tooltip(options);
        }
    });

    $('[data-toggle="popover"]').popover({ html: true });

    $('body').on('click', '.close', function (e) {
        $('[data-toggle="popover"]').popover('hide');
    });

    $('body').on('click', '.noDelete', function (e) {
        $('[data-toggle="popover"]').popover('hide');
    });

    $('body').on('click', function (e) {
        //did not click a popover toggle, or icon in popover toggle, or popover
        if ($(e.target).data('toggle') !== 'popover'
            && $(e.target).parents('[data-toggle="popover"]').length === 0
            && $(e.target).parents('.popover.in').length === 0) {
            $('[data-toggle="popover"]').popover('hide');
        }
    });

    $(document).on("click", ".menu > li", function (e) {
        $(".menu > li").find(".treeview-menu").css({ display: 'none' });
        $(".menu > li").removeClass('active');
        $(this).addClass('active');
        $(this).find(".treeview-menu").css({ display: 'block' });
    });

    $(document).on("click", ".menu > li > .treeview-menu > li", function (e) {
        $(".menu > li > .treeview-menu > li").removeClass('active');
        $(this).addClass('active');
        $(this).find(".treeview-menu").css({ display: 'block' });
    });

    $(document).on("change", ".checkall", function () {
        if ($(this).is(":checked")) {
            $("tbody tr td input:checkbox").each(function () {
                $(this).prop('checked', true);
            });
        } else {
            $("tbody tr td input:checkbox").each(function () {
                $(this).prop('checked', false);
            });
        }
    });

    $(document).on("change", ".item-check", function () {
        var allChecked = $(this).is(":checked");

            $("tbody tr td input:checkbox").each(function () {
                allChecked = allChecked && $(this).is(':checked');
            });

            if (allChecked) {
                $(".checkall").prop('checked', true);
            }
            else {
                $(".checkall").prop('checked', false);
            }
    });

    $(document).on("input","input.validate", function () {
        if ($(this).val() != "" && $(this).parents().hasClass("has-error")) {
            $(this).parents().removeClass("has-error");
        }
    });

    $("button").on('dblclick', function (event) {
        event.preventDefault();
    });
});

$(document).on("keypress", ".allownumericwithdecimal", function (event) {
    //this.value = this.value.replace(/[^0-9\.]/g,'');
    $(this).val($(this).val().replace(/[^0-9\.]/g, ''));
    if ((event.which != 46 || $(this).val().indexOf('.') != -1) && (event.which < 48 || event.which > 57)) {
        event.preventDefault();
    }
});

$(document).on("keypress", ".allownumericwithoutdecimal", function (event) {
    $(this).val($(this).val().replace(/[^\d].+/, ""));
    if ((event.which < 48 || event.which > 57)) {
        event.preventDefault();
    }
});

