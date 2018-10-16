using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Forms.Models
{
    public class DynamicForm : ICloneable
    {
        public DynamicForm()
        {
            Columns = new List<DynamicFormColumn>();
            Fields = new List<DynamicFormField>();
        }

        public string FormId { get; set; }
        public string ApiControllerPath { get; set; }
        public string EmailBcc { get; set; }
        public string EmailCc { get; set; }
        public string EmailIntro { get; set; }
        public string EmailSubject { get; set; }
        public string EmailTo { get; set; }
        public bool SkipDefaultEmailWhenEmailMappingsPresent { get; set; }
        public bool AlignReCaptchaWithSubmitButton { get; set; }
        public string BakcgroundImage { get; set; }
        public AlignmentEnum CaptchaAlignment { get; set; }
        public string FormDisclaimer { get; set; }
        public string FormTitle { get; set; }
        public bool HideRecaptcha { get; set; }
        public string ReCaptchaKey { get; set; }
        public bool DisableServerSideRecaptcha { get; set; }
        public string SubmissionHandlerUrl { get; set; }
        public string SubmitButtonText { get; set; }
        public AlignmentEnum SubmitButtonAlignement { get; set; }
        public string SuccessRedirectPage { get; set; }
        public string SuccessMessage { get; set; }
        public string SubmissionType { get; set; }

        public List<DynamicFormColumn> Columns { get; set; }
        public List<DynamicFormField> Fields { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public enum AlignmentEnum
    {
        left,
        center,
        right
    }
}
