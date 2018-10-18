using Forms.Controllers;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Models
{
    public interface IFormsStore
    {
        DynamicForm Find(string id);
    }

    class InMemFormsStore : IFormsStore
    {
        private readonly Dictionary<string, DynamicForm> dict;

        public InMemFormsStore(IOptions<ReCaptchaSettings> captchaSettings)
        {
            dict = new Dictionary<string, DynamicForm>();

            dict.Add(
                "footerForm1",
                new DynamicForm
                {
                    FormId = "footerForm1",
                    ApiControllerPath = "/api/forms", // url to the re-captcha server-side validation endpoint
                    FormTitle = "<div class='text-center'><h2>Form 1</h2><p>this form was built declaratively</p></div>",
                    EmailTo = "jalva01@gmail.com",
                    EmailSubject = "Testing dynamic forms",
                    EmailIntro = "Dynamic forms in .Net Core",
                    AlignReCaptchaWithSubmitButton = true,
                    SubmitButtonAlignement = AlignmentEnum.right,
                    CaptchaAlignment = AlignmentEnum.left,
                    SubmissionType = SubmissionTypeEnum.None,

                    Fields = new List<DynamicFormField>
                    {
                        new DynamicFormField
                        {
                            Type = DynamicFormFieldType.text,
                            Label = "First Name",
                            NameAttribute = "first-name",

                            ValidationAttributes = new Dictionary<string, string>()
                            {
                                {"required", "y" }
                            },

                            XsWidth = DynamicFormFieldSizeEnum.w100prcnt,
                            SmWidth = DynamicFormFieldSizeEnum.w50prcnt
                        },

                        new DynamicFormField
                        {
                            Type = DynamicFormFieldType.text,
                            Label = "Last Name",
                            NameAttribute = "last-name",

                            ValidationAttributes = new Dictionary<string, string>()
                            {
                                {"required", "y" }
                            },

                            XsWidth = DynamicFormFieldSizeEnum.w100prcnt,
                            SmWidth = DynamicFormFieldSizeEnum.w50prcnt
                        },

                        new DynamicFormField
                        {
                            Type = DynamicFormFieldType.email,
                            Label = "Email",
                            NameAttribute = "email",

                            ValidationAttributes = new Dictionary<string, string>()
                            {
                                {"required", "y" },
                                {"pattern", @"[^\s@]+@[^\s@]+\.[^\s@]+" }
                            },

                            XsWidth = DynamicFormFieldSizeEnum.w100prcnt,
                            SmWidth = DynamicFormFieldSizeEnum.w50prcnt
                        },

                        new DynamicFormField
                        {
                            Type = DynamicFormFieldType.text,
                            Label = "Company",
                            NameAttribute = "company",

                            ValidationAttributes = new Dictionary<string, string>()
                            {
                                {"required", "y" }
                            },

                            XsWidth = DynamicFormFieldSizeEnum.w100prcnt,
                            SmWidth = DynamicFormFieldSizeEnum.w50prcnt
                        },

                        new DynamicFormField
                        {
                            Type = DynamicFormFieldType.select,
                            Label = "Product of Interest",
                            NameAttribute = "product",

                            ValidationAttributes = new Dictionary<string, string>()
                            {
                                {"required", "y" }
                            },

                            XsWidth = DynamicFormFieldSizeEnum.w100prcnt,
                            SmWidth = DynamicFormFieldSizeEnum.w50prcnt,

                            Options = new List<DynamicFormFieldOption>
                            {
                                new DynamicFormFieldOption{ Title = "Select Option 1", Value="1" },
                                new DynamicFormFieldOption{ Title = "Select Option 2", Value="2" },
                                new DynamicFormFieldOption{ Title = "Select Option 3", Value="3" }
                            },

                            HideMappings = new List<DynamicFormFieldHideMapping>
                            {
                                new DynamicFormFieldHideMapping
                                {
                                    HideControlsWithNames = "last-name",
                                    SelectedValues = "2"
                                }
                            }
                        },

                        new DynamicFormField
                        {
                            Type = DynamicFormFieldType.radio,
                            Label = "Radio Group",
                            NameAttribute = "radio-group",

                            ValidationAttributes = new Dictionary<string, string>()
                            {
                                {"required", "y" }
                            },

                            XsWidth = DynamicFormFieldSizeEnum.w100prcnt,
                            SmWidth = DynamicFormFieldSizeEnum.w50prcnt,

                            Options = new List<DynamicFormFieldOption>
                            {
                                new DynamicFormFieldOption{ Title = "Option 1", Value="1" },
                                new DynamicFormFieldOption{ Title = "Option 2", Value="2" },
                                new DynamicFormFieldOption{ Title = "Option 3", Value="3" }
                            }
                        },
                    }
                }
            );
        }
        
        

        public DynamicForm Find(string id)
        {
            if (dict.ContainsKey(id))
            {
                var form = dict[id];
                return form;
            }
            return null;
        }
    }
}
