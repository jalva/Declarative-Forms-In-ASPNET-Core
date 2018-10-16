using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Models
{
    public class DynamicFormEmailMapping
    {
        public string EmailBcc { get; set; }
        public string EmailCc { get; set; }
        public string EmailTo { get; set; }
        public string Intro { get; set; }
        public string Subject { get; set; }
        public string ValuesSelected { get; set; }
    }
}
