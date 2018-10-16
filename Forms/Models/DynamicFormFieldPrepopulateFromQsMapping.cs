using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Models
{
    public class DynamicFormFieldPrepopulateFromQsMapping
    {
        public string QueryStringName { get; set; }
        public string QueryStringValue { get; set; }
        public string SelectedValueOverwrite { get; set; }
    }
}
