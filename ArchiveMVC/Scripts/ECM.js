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