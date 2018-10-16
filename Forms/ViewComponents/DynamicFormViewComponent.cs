using Forms.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.ViewComponents
{
    public class DynamicFormViewComponent : ViewComponent
    {
        private readonly IFormsStore _formsStore;
        private readonly IOptions<ReCaptchaSettings> _captchaSettings;

        public DynamicFormViewComponent(IFormsStore formsStore, IOptions<ReCaptchaSettings> captchaSettings)
        {
            _formsStore = formsStore;
            _captchaSettings = captchaSettings;
        }

        public IViewComponentResult Invoke(string formId, DynamicForm formOverwrites)
        {
            DynamicForm form = formOverwrites;

            if (!string.IsNullOrEmpty(formId))
                form = _formsStore.Find(formId);

            form.ReCaptchaKey = _captchaSettings.Value.RecaptchaPublicKey; 

            if(formOverwrites != null && !string.IsNullOrEmpty(formId))
            {
                form = (DynamicForm)form.Clone();

                if (!string.IsNullOrEmpty(formOverwrites.FormTitle))
                    form.FormTitle = formOverwrites.FormTitle;
                if (!string.IsNullOrEmpty(formOverwrites.SubmitButtonText))
                    form.SubmitButtonText = formOverwrites.SubmitButtonText;
                if (!string.IsNullOrEmpty(formOverwrites.SuccessMessage))
                    form.SuccessMessage = formOverwrites.SuccessMessage;
                if (!string.IsNullOrEmpty(formOverwrites.SubmissionHandlerUrl))
                    form.SubmissionHandlerUrl = formOverwrites.SubmissionHandlerUrl;
                if (!string.IsNullOrEmpty(formOverwrites.SuccessRedirectPage))
                    form.SuccessRedirectPage = formOverwrites.SuccessRedirectPage;
                if (!string.IsNullOrEmpty(formOverwrites.EmailTo))
                    form.EmailTo = formOverwrites.EmailTo;
                if (!string.IsNullOrEmpty(formOverwrites.EmailSubject))
                    form.EmailSubject = formOverwrites.EmailSubject;

            }

            return View(form);
        }
    }
}
