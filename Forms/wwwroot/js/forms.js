import { formSubmissionFactory } from './formSubmissionFactory.js';
import { serializeForm, readCookie, parseQueryString } from './utils';

if (!HTMLFormElement.prototype.reportValidity) {
    HTMLFormElement.prototype.reportValidity = function () {
        var submitButtons = this.querySelectorAll("button, input[type=submit]");
        for (var i = 0; i < submitButtons.length; i++) {
            // Filter out <button type="button">, as querySelectorAll can't
            // handle :not filtering
            if (submitButtons[i].type === "submit") {
                submitButtons[i].click();
                return;
            }
        }
    }
}

function initDynamicForm(formEl) {
    initValidation(formEl);
    initSubmission(formEl);
    initMappings(formEl);
    console.log('Initialized dynamic form: ' + formEl.getAttribute('id'))
}

function initValidation(formEl) {
    formEl.setAttribute('novalidate', 'novalidate');
    var inputs = formEl.querySelectorAll('input:not([type="submit"]), select, textarea');
    function validate() {
        var isValid = this.checkValidity();
        if (isValid) {
            this.classList.remove("error");
            this.classList.add("valid");
        }
        else {
            this.classList.remove("valid");
            this.classList.add("error");
        }
    }

    for (var i = 0; i < inputs.length; i++) {
        let input = inputs[i];
        input.addEventListener(
            "invalid",
            function (event) {
                input.classList.add("error");
            },
            false
        );

        input.addEventListener("blur", validate);
        input.addEventListener("change", validate);
    }

    // handle validation for custom checkbox list field
    var cblHidden = formEl.querySelectorAll(".checkbox-list-wrapper input[type=hidden]");

    for (var i = 0; i < cblHidden.length; i++) {
        var h = cblHidden[i];
        var parent = h.closest('.checkbox-list-wrapper');
        var cblCheckboxes = parent.querySelectorAll(".checkbox-list-wrapper[required] input[type=checkbox]");
        for (var i2 = 0; i2 < cblCheckboxes.length; i2++) {
            var cb = cblCheckboxes[i2];
            cb.addEventListener('change', function (e) {
                h.value = '';
                if (cb.checked) {
                    console.log('checkbox-list cb checked, value: ' + this.value);
                    cblHidden.val(cblHidden.value + (cblHidden.value ? ',' : '') + this.value);
                }
            });
        }
        var firstCbx = cblCheckboxes[0];
        if (cblHidden.value = cblHidden.value.replace(/\,/, '').trim() == "") {
            firstCbx.setAttribute('required', '');
            firstCbx.setCustomValidity('Please select a value.');
        }
        else {
            firstCbx.removeAttribute('required');
            firstCbx.setCustomValidity('');
        }
    }
}


function initSubmission(form) {
    var formMsgLbl = form.querySelector('.form-message-lbl');
    var submitBttn = form.querySelector('input[type=submit]');

    form.addEventListener('submit', function (e) {
        console.log('onsubmit event');
        e.preventDefault();
        this.reportValidity();

        var noCaptcha = form.dataset['noCaptcha'];

        // run default behavior (submitting) in case of validation success
        if (this.checkValidity()) {
            console.log('form fields are valid');
            
            var captchaValid = noCaptcha && noCaptcha !== "false";

            if (!captchaValid) {
                var recaptchaResponse = getRecaptchaResponse(form);
                if (!recaptchaResponse) {
                    var captchaErrorLbl = form.querySelector('.recaptcha-error-lbl');
                    captchaErrorLbl.innerHTML = 'Please validate re-captcha';
                }
                else {
                    captchaValid = true;
                }
            }

            if (captchaValid) {

                // display submission status
                formMsgLbl.innerHTML = '<p class="bg-info">Submitting form, please wait.</p>';

                // disable submit button
                submitBttn.disabled = true;


                var submissionType = form.dataset['submissionType'];
                var formSubmission = null;
                if (submissionType) {
                    formSubmission = formSubmissionFactory.getSubmission(submissionType);
                }


                // build model to send to server (both for recaptcha validation as well as sendEmail)
                var formActionUrl = form.getAttribute('action');
                var hideCaptcha = form.dataset['noCaptcha'] && form.dataset['noCaptcha'] !== 'false';
                var pageUrl = location.href;
                var formId = form.dataset['formId'];
                var captchaResponse = getRecaptchaResponse(form);
                var serializedForm = serializeForm(form);
                var sendEmailUrl = form.dataset['sendEmailUrl'];

                var model = {
                    formId: formId,
                    reCaptchaResponse: getRecaptchaResponse(form),
                    serializedForm: serializedForm,
                    pageUrl: pageUrl,
                    sendEmailUrl: sendEmailUrl
                };


                function errorCallback() {
                    console.log(model.errors);

                    formMsgLbl.innerHTML = `<p class="bg-danger">${ model.errors}</p>`;

                    submitBttn.disabled = false;

                    // send error email
                    sendEmail(model);
                }

                function successCallback() {
                    console.log('Form submitted successfully');
                    form.reset();
                    resetRecaptcha(form);

                    var redirectUrl = form.dataset['redirectUrl'];

                    if (!redirectUrl) {
                        formMsgLbl.innerHTML = '<p class="bg-success">' + (form.dataset['successMessage'] || 'Form successfully submitted. Thank you.') + '</p>';
                        submitBttn.disabled = false;
                    }

                    // send email
                    sendEmail(model);

                    if (redirectUrl)
                        window.location = redirectUrl;
                }

                console.log('form action: ' + form.getAttribute('action'));
                console.log('no captcha: ' + noCaptcha);

                if (!noCaptcha || noCaptcha === 'false') {
                    console.log('going to server-side validate recaptcha');


                    // validate re-captcha server-side
                    console.log('going to fetch ' + formActionUrl);
                    fetch(
                        formActionUrl,
                        {
                            method: 'post',
                            headers: {
                                "Content-Type": 'application/json'
                            },
                            dataType: 'text',
                            body: JSON.stringify(model)
                        }
                    ).then(function (response) {

                        if(!response.ok) {
                            var errors = response.responseText;
                            model.errors = 'Server-side recaptcha validation errors: ' + errors;

                            errorCallback();
                            return;
                        }

                        var handlerUrl = response.responseText;
                        console.log('server-side recaptcha validation successfull, received handler url: ' + response.responseText);
                        if (handlerUrl) {
                            model.handlerUrl = handlerUrl;
                        }

                        
                        if (formSubmission) {
                            // if custom form submission function exists, invoke it
                            console.log('going to submit form with submission-type: ' + submissionType);
                            formSubmission(form, model, successCallback, errorCallback);
                        }
                        else {
                            successCallback();
                        }
                    });
                }
                else {
                    if (formSubmission) {
                        // if form submission function exists, invoke it
                        console.log('going to submit form with submission-type: ' + submissionType);
                        formSubmission(form, model, successCallback, errorCallback);
                    }
                    else {
                        successCallback();
                    }
                }
            }
            else {
                // captcha invalid
            }
        }
        else {
            // form not valid
            console.log('form is invalid');
        }

    });
}

function initMappings(form) {
    var qs = parseQueryString();

    function prepopulateMappingsHandler(mapping, el, fieldType) {
        console.log('handling prepopulate from qs mapping, qsName: ' + mapping.qsName + ', qsVal: ' + mapping.qsValue + ', url: ' + mapping.urlSegment + ', val: ' + mapping.selectedVals);
        var val = "";
        if (mapping.qsName && qs[mapping.qsName]) {
            console.log('found qs: ' + mapping.qsName);
            if (!mapping.qsValue) {
                val = qs[mapping.qsName];
            }
            else if (qs[mapping.qsName] == mapping.qsValue) {
                val = mapping.selectedVals;
            }
        }
        else if (mapping.urlSegment && location.pathname.indexOf(mapping.urlSegment) > -1) {
            console.log('handling url segment: ' + mapping.urlSegment);
            val = mapping.selectedVals;
        }
        else if (mapping.cookieName && readCookie(mapping.cookieName)) {
            val = mapping.selectedVals;
        }
        else {
            return;
        }

        if (fieldType == "checkboxlist") {
            if (val) {
                var tokens = val.split(',');
                for (var i = 0; i < tokens.length; i++) {
                    var cb = el.parentNode.query("input[type=checkbox][value='" + tokens[i] + "']");
                    console.log('matched ' + cb.length + ' checkboxes');
                    cb.checked = true;
                }
            }
        }
        else if (fieldType == 'checkbox') {
            cb.checked = true;
        }
        else {
            el.value = val;
        }

        console.log('mapped value: ' + val + ' for field type: ' + fieldType + '; for el: ' + el);
    }

    function showHideMappingsHandler(mapping, el, fieldType) {
        console.log('handling show/hide mapping, selectedVals: ' + mapping.selectedVals + ', controls to hide: ' + mapping.controlsToHide + ', controls to show: ' + mapping.controlsToShow);
        function onChangeCallback(x, y) {
            var selVal = el.value;

            if (mapping.controlsToHide) {
                var controlsToHideTokens = mapping.controlsToHide.split(',');
                var selectedValsTokens = mapping.selectedVals.split(',');
                for (var i = 0; i < selectedValsTokens.length; i++) {
                    var control;
                    for (var i2 = 0; i2 < controlsToHideTokens.length; i2++) {
                        console.log('hiding control: ' + "[name='" + controlsToHideTokens[i2] + "']");
                        control = form.querySelector("[name='" + controlsToHideTokens[i2] + "']");

                        if (selVal.indexOf(selectedValsTokens[i]) > -1) {
                            control.parentNode.style.display = 'none';

                        }
                        else {
                            control.parentNode.style.display = 'block';

                        }
                    }
                }
            }
            if (mapping.controlsToShow) {
                console.log('--inside controls to show');
                var controlsToShowTokens = mapping.controlsToShow.split(',');
                var selectedValsTokens = mapping.selectedVals.split(',');
                for (var i = 0; i < selectedValsTokens.length; i++) {
                    var control;
                    for (var i2 = 0; i2 < controlsToShowTokens.length; i2++) {
                        console.log('showing controls: ' + "[name='" + controlsToShowTokens[i2] + "']");
                        control = form.querySelector("[name='" + controlsToShowTokens[i2] + "']");

                        console.log('selVal: ' + selVal + "-> mapping selVal: " + selectedValsTokens[i]);
                        if (selVal.indexOf(selectedValsTokens[i]) > -1) {
                            control.parentNode.style.display = 'block';

                        }
                        else {
                            control.parentNode.style.display = 'none';
                        }
                    }
                }
            }
        }

        el.addEventListener('change', onChangeCallback);
    }

    handleFieldMappings("data-has-prepopulate-qs-mappings", form, prepopulateMappingsHandler);
    handleFieldMappings("data-has-prepopulate-url-mappings", form, prepopulateMappingsHandler);
    handleFieldMappings("data-has-prepopulate-cookie-mappings", form, prepopulateMappingsHandler);

    handleFieldMappings("data-has-hide-mappings", form, showHideMappingsHandler);
    handleFieldMappings("data-has-show-mappings", form, showHideMappingsHandler);
}


function handleFieldMappings(mappingType, form, handler) {
    var controls = form.querySelectorAll('[' + mappingType + '=yes]');
    console.log('controls with ' + mappingType + ': ' + controls.length);
    for (var i = 0; i < controls.length; i++) {
        var el = controls[i];
        var fieldType = el.getAttribute('data-type');
        if (!fieldType) {
            fieldType = el.getAttribute('type');
        }
        var jsonStr = el.getAttribute(mappingType.replace('-has', ''));
        console.log('jsonStr: ' + jsonStr);
        var mappingsList = JSON.parse(jsonStr);
        for (var i = 0; i < mappingsList.length; i++) {
            var mappingObj = mappingsList[i];
            handler(mappingObj, el, fieldType);
        }
    }
}


function getRecaptchaResponse(form) {
    var recaptchaId = form.dataset['recaptchaId'];
    var reCaptchaResponse = recaptchaId ? grecaptcha.getResponse(recaptchaId) : grecaptcha.getResponse();
    console.log('recaptchaId: ' + recaptchaId + ' response: ' + reCaptchaResponse);
    return reCaptchaResponse;
}

function resetRecaptcha(form) {
    var recaptchaId = form.dataset['recaptchaId'];
    if (recaptchaId)
        grecaptcha.reset(recaptchaId);
    else
        grecaptcha.reset();
}


function sendEmail(model) {
    if (model.errors)
        model.errors = model.errors.replace('<br/>', '||n||').replace('<br />', '||b||').replace('<br', '||b||').replace('/>', '');

    var postData = JSON.stringify(model);
    console.log('going to send emails, data: ' + postData);

    fetch(model.sendEmailUrl, {
        method: 'post',
        headers: { 'Content-Type': 'application/json' },
        body: postData
    }).then(function (response) {
        console.log('Emails sent ' + (response.ok ? 'successfully' : 'unsuccessfully'));
    });
}


export { initDynamicForm }