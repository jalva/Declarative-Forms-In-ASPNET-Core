using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Models
{
    public class DynamicFormField
    {
        public DynamicFormField()
        {
            ValidationAttributes = new Dictionary<string, string>();
            EmailMappings = new List<DynamicFormEmailMapping>();
            Options = new List<DynamicFormFieldOption>();
        }

        public bool HideControl { get; set; }
        public string Label { get; set; }
        public string NameAttribute { get; set; }
        public string Placeholder { get; set; }
        public int TabIndex { get; set; }
        public DynamicFormFieldType Type { get; set; }
        public Dictionary<string,string> ValidationAttributes { get; set; }
        public string ValueAttribute { get; set; }
        public DynamicFormFieldSizeEnum XsWidth { get; set; }
        public DynamicFormFieldSizeEnum SmWidth { get; set; }
        public DynamicFormFieldSizeEnum MdWidth { get; set; }
        public DynamicFormFieldSizeEnum LgWidth { get; set; }

        /// <summary>
        /// Applies to drop downs (select), radio groups and checkbox groups
        /// </summary>
        public bool RenderVertically { get; set; }

        public List<DynamicFormEmailMapping> EmailMappings;

        public List<DynamicFormFieldPrepopulateFromQsMapping> PrepopulateFromQsMappings;
        public List<DynamicFormFieldPrepopulateFromUrlMapping> PrepopulateFromUrlMappings;
        public List<DynamicFormFieldPrepopulateFromCookieMapping> PrepopulateFromCookieMappings;

        public List<DynamicFormFieldHideMapping> HideMappings;
        public List<DynamicFormFieldShowMapping> ShowMappings;

        public List<DynamicFormFieldOption> Options;


        public string GetResponsiveSizesClasses()
        {
            Func<DynamicFormFieldSizeEnum, string, string> getClass = (size, prefix) =>
            {
                switch (size) {
                    case DynamicFormFieldSizeEnum.w100prcnt:
                        return "col-" + prefix + "-12";
                    case DynamicFormFieldSizeEnum.w50prcnt:
                        return "col-" + prefix + "-6";
                    case DynamicFormFieldSizeEnum.w33prcnt:
                        return "col-" + prefix + "-4";
                    case DynamicFormFieldSizeEnum.w25prcnt:
                        return "col-" + prefix + "-3";
                    case DynamicFormFieldSizeEnum.w16prcnt:
                        return "col-" + prefix + "-2";
                    case DynamicFormFieldSizeEnum.w8prcnt:
                        return "col-" + prefix + "-1";
                    default:
                        return "";
                }
            };

            var xsClass = getClass(XsWidth, "xs");
            var smClass = getClass(SmWidth, "sm");
            var mdClass = getClass(MdWidth, "md");
            var lgClass = getClass(LgWidth, "lg");

            return string.Join(" ", new[] { xsClass, smClass, mdClass, lgClass });

        }

        public string GetValidationAttrs()
        {
            var attrs = this.ValidationAttributes.Keys.Select(name => name + "=\"" + this.ValidationAttributes[name] + "\"").ToArray();
            var str = string.Join(" ", attrs);
            return str;
        }
    }

    public enum DynamicFormFieldSizeEnum
    {
        None,
        w100prcnt,
        w50prcnt,
        w33prcnt,
        w25prcnt,
        w16prcnt,
        w8prcnt
    }

    public enum DynamicFormFieldType
    {
        checkbox,
        checkboxlist,
        email,
        file,
        hidden,
        number,
        radio,
        select,
        tel,
        text,
        textarea,
        url
    }
}
