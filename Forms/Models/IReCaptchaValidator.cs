using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Forms.Models
{
    public interface IReCaptchaValidator
    {
        bool Validate(string reCaptchaResponse, string remoteIp, out string errors);
    }

    class GReCaptchaValidator : IReCaptchaValidator
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }


        private IOptions<ReCaptchaSettings> _reCaptchaSettings;

        public GReCaptchaValidator(IOptions<ReCaptchaSettings> reCaptchaSettings)
        {
            _reCaptchaSettings = reCaptchaSettings;
        }

        public bool Validate(string reCaptchaResponse, string remoteIp, out string errors)
        { 
            var secretKey = _reCaptchaSettings.Value.RecaptchaPrivateKey;
            var url = "https://www.google.com/recaptcha/api/siteverify";
            var client = new WebClient();
            var googleReply = client.DownloadString(string.Format(url + "?secret={0}&response={1}", secretKey, reCaptchaResponse));
            var captchaResponse = JsonConvert.DeserializeObject<GReCaptchaValidator>(googleReply);
            errors = "";
            if (!captchaResponse.Success)
            {
                foreach (var err in captchaResponse.ErrorCodes)
                    errors += err + "; ";
            }
            return string.IsNullOrEmpty(errors);
        }
    }
}
