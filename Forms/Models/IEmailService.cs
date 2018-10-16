using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Forms.Models
{
    public interface IEmailService
    {
        Task SendEmail(string to, string cc, string bcc, string subject, string intro, string body, string from=null);
    }

    class SendGridEmailService : IEmailService
    {
        private IOptions<SendGridSettings> _sendGridSettings;
        private IOptions<EmailSettings> _emailSettings;

        public SendGridEmailService(IOptions<SendGridSettings> sendGridSettings, IOptions<EmailSettings> emailSettings)
        {
            _sendGridSettings = sendGridSettings;
            _emailSettings = emailSettings;
        }

        public async Task SendEmail(string to, string cc, string bcc, string subject, string intro, string body, string from=null)
        {
            if (string.IsNullOrEmpty(from))
                from = _emailSettings.Value.FromEmail;

            var myMessage = new SendGridMessage();

            // Add the message properties.
            myMessage.From = new EmailAddress(from);


            myMessage.AddTo(new EmailAddress(to));

            if (!string.IsNullOrEmpty(cc))
            {
                foreach (var _cc in cc.Split(','))
                    myMessage.AddCc(new EmailAddress(_cc));
            }

            if (!string.IsNullOrEmpty(bcc))
            {
                foreach (var _bcc in bcc.Split(','))
                    myMessage.AddBcc(new EmailAddress(_bcc));
            }

            myMessage.Subject = subject;

            //Add the HTML and Text bodies
            myMessage.HtmlContent = body;
            //myMessage.Text = "Hello World plain text!";

            // Create a Web transport, using API Key
            var key = System.Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            if (string.IsNullOrEmpty(key))
                key = _sendGridSettings.Value.SendGridApiKey;


            var client = new SendGridClient(key);
            var response = await client.SendEmailAsync(myMessage);
        }
    }
}
