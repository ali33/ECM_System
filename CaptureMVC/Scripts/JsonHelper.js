

var JsonHelper = [];

JsonHelper.helper = new function () {

    this.defaultErrorMessage = 'Có lỗi trong quá trình xử lý. Vui lòng thực hiện lại thao tác.';

    // Hiển thị thông tin ra console để debug
    this.debug = function (data) {
        console.debug(data);
        return this;
    }


    // Thay cho hàm $.post, thêm xử lý lỗi mặc định.
    // Hiển thị thông báo báo khi có exception
    // 'Có lỗi trong quá trình xử lý. Vui lòng thực hiện lại thao tác.'
    this.post = function (url, data, success, error) {

        // Hàm xử lý lỗi mặc định cho post
        error = error || this.defaultErrorHandler;

        $.ajax({
            type: "POST",
            url: url,
            data: data,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.IsTimeOut && data.IsTimeOut == true)
                    //AppDomainAppVirtualPath declare in _Layout.cshtml
                    window.location.href = AppDomainAppVirtualPath + "?ReturnUrl=" + data.Detail;
                else
                    success.call(this, data);
            },
            error: function (xhr, b, c) {
                error.call(this, xhr, b, c);
                //AppDomainAppVirtualPath declare in _Layout.cshtml
                //window.location.href = AppDomainAppVirtualPath + "?ReturnUrl=" + window.location.pathname;
            }
        });
    }

    // Xử lý lỗi mặc định
    this.defaultErrorHandler = function () {
        var msg = this.defaultErrorMessage
            || 'Có lỗi trong quá trình xử lý. Vui lòng thực hiện lại thao tác.';
        alert(msg);
    }
};
