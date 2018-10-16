using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Models
{
    public class FormModel
    {
        public string ReCaptchaResponse { get; set; }
        [Required]
        public string SerializedForm { get; set; }
        [Required]
        public string PageUrl { get; set; }
        [Required]
        public string FormId { get; set; }
        public string Errors { get; set; }

    }
}
