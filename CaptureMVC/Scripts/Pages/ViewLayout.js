var bodyLayout;
$(document).ready(function () {
    bodyLayout = $('body').layout({
        applyDefaultStyles: true,
        north: {
            paneSelector: '#capture-header',
            closable: true,
            resizable: true,
            slidable: true,
            spacing_open: 1,
            size: 50,
            childOptions: {
                west: {
                    paneSelector: '#header-logo',
                    applyDefaultStyles: true,
                },
                center: {
                    paneSelector: '#header-menu',
                    applyDefaultStyles: true,
                },
                east: {
                    paneSelector: '#header-account',
                    applyDefaultStyles: true,
                }
            }
        },
        center: {
            paneSelector: '#capture-content',
        }
    });
});
