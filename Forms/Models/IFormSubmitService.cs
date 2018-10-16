using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Forms.Models
{
    public interface IFormSubmitService
    {
        bool SubmitForm(FormModel model, DynamicForm form, out string errors);
    }

    class PardotFormSubmitService : IFormSubmitService
    {
        public bool SubmitForm(FormModel model, DynamicForm form, out string errors)
        {
            var url = form.SubmissionHandlerUrl + "?" + model.SerializedForm;
            var uri = new Uri(url);
            var request = HttpWebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            var webResponse = request.GetResponse();
            var responseText = new StreamReader(webResponse.GetResponseStream()).ReadToEnd();
            errors = "";
            if (responseText.Contains("error"))
            {
                Regex rx = new Regex(@"~~~(.*)~~~");
                MatchCollection matches = rx.Matches(responseText);
                Match match = matches.Count > 0 ? matches[0] : null;
                if (match != null)
                    errors = match.Value.Replace("~~~", "<br/>");
            }

            return string.IsNullOrEmpty(errors);
        }
    }
}
