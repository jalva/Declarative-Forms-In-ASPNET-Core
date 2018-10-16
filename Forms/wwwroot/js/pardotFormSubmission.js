// this is a custom form submission action that creates an iframe and submits the serialized form to Pardot via this iframe 

import { parseQueryString } from "./utils";
import { RegExp } from "core-js/library/web/timers";

function pardotFormSubmission(form, model, successCallback, errorCallback) {

    if (!model.handlerUrl) {
        model.errors = "Pardot handler url missing"
        errorCallback();
        return;
    }

    var iframe = document.createElement('iframe');
    iframe.setAttribute('width', '1');
    iframe.setAttribute('height', '1');
    iframe.setAttribute('style', 'width:1;height:1;overflow:hidden');
    document.body.appendChild(iframe);
    iframe.addEventListener('load', function (event) {
        console.log("iFrame has loaded, going to access its contents");
        try {
            var iframeUrl = this.contentWindow.location.href;
            iframe.remove();
            var qs = parseQueryString(iframeUrl);
            if (qs.hasOwnProperty('errors') && qs.errors == 'true') {
                // there were validation errors from Pardot
                // parse the errors
                var errors = qs.errorMessage.replace(new RegExp(/\~\~\~/, 'g'), '<br/>'); // regex that operates on the Pardot custom error formatting
                throw new Error(errors);
            }
            else {
                console.log('pardot submission successful');
                successCallback();
            }
        }
        catch (e) {
            console.log(e.message);
            model.errors = 'iFrame submission error: ' + e.message;
            errorCallback();
        }
    });

    iframe.setAttribute('src', model.handlerUrl);
}

export {pardotFormSubmission}