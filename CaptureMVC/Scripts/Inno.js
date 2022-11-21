var Inno = [];

Inno.helper = new function () {

    this.get = function (url, data, panel, syncOject, success, error) {

        // Get the default error if not specify
        error = error || this.defaultErrorHandler;

        $.ajax({
            type: "GET",
            url: url,
            data: data,
            contentType: "application/json; charset=utf-8",
            cache: false,
            success: function (data) {
                if (data.IsTimeOut && data.IsTimeOut == true) {
                    //AppDomainAppVirtualPath declare in _Layout.cshtml
                    window.location.href = AppDomainAppVirtualPath + "?ReturnUrl=" + data.Detail;
                }
                else {
                    success.call(this, data);
                    if (syncOject.syncCount == 1) {
                        if (null != panel) {
                            for (var i = 0; i < panel.length; i++) {
                                panel[i].ecm_loading_hide()
                            }
                        }
                    }
                    
                    syncOject.syncCount = syncOject.syncCount - 1;
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                error.call(this, jqXHR, textStatus, errorThrown);
                if (syncOject.syncCount == 1) {
                    if (null != panel) {
                        for (var i = 0; i < panel.length; i++) {
                            panel[i].ecm_loading_hide()
                        }
                    }
                }

                syncOject.syncCount = syncOject.syncCount - 1;
            }
        });
    };

    this.post = function (url, data, panel, syncOject, success, error) {

        // Get the default error if not specify
        error = error || this.defaultErrorHandler;

        $.ajax({
            type: "POST",
            url: url,
            data: data,
            contentType: "application/json; charset=utf-8",
            cache: false,
            success: function (data) {
                if (data.IsTimeOut && data.IsTimeOut == true) {
                    //AppDomainAppVirtualPath declare in _Layout.cshtml
                    window.location.href = AppDomainAppVirtualPath + "?ReturnUrl=" + data.Detail;
                }
                else {
                    success.call(this, data);
                    if (syncOject.syncCount == 1) {
                        if (null != panel) {
                            for (var i = 0; i < panel.length; i++) {
                                panel[i].ecm_loading_hide()
                            }
                        }
                    }

                    syncOject.syncCount = syncOject.syncCount - 1;
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                error.call(this, jqXHR, textStatus, errorThrown);
                success.call(this, data);
                if (syncOject.syncCount == 1) {
                    if (null != panel) {
                        for (var i = 0; i < panel.length; i++) {
                            panel[i].ecm_loading_hide()
                        }
                    }
                }

                syncOject.syncCount = syncOject.syncCount - 1;
            }
        });
    };

    // Handler for default error
    this.defaultErrorHandler = function (jqXHR, textStatus, errorThrown) {
        alert(DEFAULT_ERROR_MESSAGE);
        console.log(jqXHR + "-" + textStatus + "-" + errorThrown);
    };
};
