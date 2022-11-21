

var JsonHelper = [];

JsonHelper.helper = new function () {

    this.defaultErrorMessage = 'Process error. Please try again!';

    this.debug = function (data) {
        console.debug(data);
        return this;
    }


    this.post = function (url, data, success, error, async) {

        error = error || this.defaultErrorHandler;
        var asyncData = false;

        if (async == !undefined) {
            asyncData = async;
        }

        $.ajax({
            type: "POST",
            url: url,
            data: data,
            async: asyncData,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.IsTimeOut && data.IsTimeOut == true)
                    window.location.href = AppDomainAppVirtualPath + "?ReturnUrl=" + data.Detail;
                else
                    success.call(this, data);
            },
            error: function (xhr, b, c) {
                error.call(this, xhr, b, c);
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
