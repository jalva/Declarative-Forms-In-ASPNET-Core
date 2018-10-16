using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Forms.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Forms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormsController : Controller
    {
        private readonly IFormsStore _formsStore;
        private readonly IReCaptchaValidator _reCaptchaValidator;
        private readonly IEmailService _emailService;
        private readonly IOptions<EmailSettings> _emailSettings;

        public FormsController(IFormsStore formsStore, IReCaptchaValidator reCaptchaValidator, IEmailService emailService, IOptions<EmailSettings> emailSettings)
        {
            _formsStore = formsStore;
            _reCaptchaValidator = reCaptchaValidator;
            _emailService = emailService;
            _emailSettings = emailSettings;
        }
        
        /// <summary>
        /// Handles server-side google re-captcha validation, which is an API call made during the form submission.
        /// </summary>
        /// <param name="model">Form id and google recaptcha response which needs to be validated</param>
        /// <returns>If form has defined a handler url, pass it to the ajax call so that appropriate form submission handler can use it to submit the form</returns>
        [HttpPost]
        [Route("ValidateRecaptcha")]
        public string ValidateRecaptcha(FormModel model)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "mMissing arguments";
            }

            // remove recaptcha from form
            var formDict = HttpUtility.ParseQueryString(model.SerializedForm);
            model.SerializedForm = string.Join("&", formDict.AllKeys.Where(k => k != "g-recaptcha-response").Select(k => k + "=" + /*HttpUtility.UrlEncode(*/formDict[k]/*)*/));

            // turn on TLS 1.1 and 1.2 without affecting other protocols
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var errors = "";
            DynamicForm form = null;

            try
            {
                // retrieve the form from datastore
                form = _formsStore.Find(model.FormId);

                if(form == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return "Form not found, id: " + model.FormId;
                }
                

                // validate recaptcha
                var success = _reCaptchaValidator.Validate(model.ReCaptchaResponse, Request.HttpContext.Connection.RemoteIpAddress.ToString(), out errors);

                if (!success)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return "ReCaptcha is invalid. Error: " + errors;
                }

            }
            catch(Exception ex)
            {
                errors = "Error validating recaptcha: " + ex.Message;
            }

            if (!string.IsNullOrEmpty(errors))
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return errors;
            }

            return form.SubmissionHandlerUrl;
        }

        
        [HttpPost]
        [Route("SendEmail")]
        public string SendEmail(FormModel model)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "Missing arguments";
            }
            
            try
            {
                // remove recaptcha from form
                var formDict = HttpUtility.ParseQueryString(model.SerializedForm);
                model.SerializedForm = string.Join("&", formDict.AllKeys.Where(k => k != "g-recaptcha-response").Select(k => k + "=" + /*HttpUtility.UrlEncode(*/formDict[k]/*)*/));

                var form = _formsStore.Find(model.FormId);

                HandleEmails(model, form);
                 
                return "";
            }
            catch(Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return "Exception encoutered when sending email: " + ex.Message;
            }
        }

        
        private void HandleEmails(FormModel model, DynamicForm form)
        {
            var to = form.EmailTo;
            var cc = form.EmailCc;
            var bcc = form.EmailBcc;
            var subject = form.EmailSubject;
            var intro = form.EmailIntro;
            string body;

            var formDict = HttpUtility.ParseQueryString(model.SerializedForm);
            var parsedForm = "";
            for(var i = 0; i < formDict.Keys.Count; i++)
            {
                var key = formDict.Keys[i];
                parsedForm += "<br/><b>" + key + ": </b>" + formDict[key];
            }

            body = parsedForm;

            if (!string.IsNullOrEmpty(model.Errors))
            {
                // send error email
                model.Errors = model.Errors + body;
                _emailService.SendEmail(to, cc, bcc, subject + " (with Errors)", intro, body);
                return;
            }
            
            if(string.IsNullOrEmpty(model.Errors))
            {
                // handle regular form email and email mappings
                var formCols = form.Columns;
                var looseFields = form.Fields;

                // handle fields in columns
                for(var i=0;i<formCols.Count; i++)
                {
                    var col = formCols[i];
                    var children = col.Fields;
                    looseFields.AddRange(children);
                }

                // handle loose fields
                var hasEmailMappings = false;
                for(var i=0;i<looseFields.Count; i++)
                {
                    var formField = looseFields[i];
                    var emailMappings = formField.EmailMappings;

                    hasEmailMappings = hasEmailMappings || emailMappings.Count > 0;

                    for(var i2 = 0; i2 < emailMappings.Count; i2++)
                    {
                        var mapping = emailMappings[i2];
                        var mappingVals = mapping.ValuesSelected.Split(',');
                        var formVals = formDict[formField.NameAttribute].Split(',');

                        foreach(var mappingVal in mappingVals)
                        {
                            foreach(var formVal in formVals)
                            {
                                if(mappingVal.Equals(formVal, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    _emailService.SendEmail(mapping.EmailTo, mapping.EmailCc, mapping.EmailBcc, mapping.Subject, mapping.Intro, body);

                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                to = _emailSettings.Value.ErrorEmailTo;
                cc = "";
                bcc = "";
                intro = model.Errors;
            }

            // send form email
            _emailService.SendEmail(to, cc, bcc, subject, intro, body);
        }
    }
}
