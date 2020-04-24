using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ringer.HubServer.Services
{
    public interface IEmailSender
    {
        Task SendMail(string to, string subject, string body);
    }

    public class MailConfigSection
    {
        public string From { get; set; }

        public string MailgunKey { get; set; }

        public string Domain { get; set; }
    }

    public class MailgunEmailSender : IEmailSender
    {
        private readonly HttpClient mailgunHttpClient;
        private readonly MailConfigSection mailConfigSection;
        private readonly ILogger<MailgunEmailSender> logger;

        public MailgunEmailSender(HttpClient mailgunHttpClient,
            MailConfigSection mailConfigSection,
            ILogger<MailgunEmailSender> logger)
        {
            this.mailgunHttpClient = mailgunHttpClient;
            this.mailConfigSection = mailConfigSection;
            this.logger = logger;
        }
        public async Task SendMail(string to, string subject, string body)
        {
            Dictionary<string, string> form = new Dictionary<string, string>
            {
                ["from"] = mailConfigSection.From,
                ["to"] = to,
                ["subject"] = subject,
                ["html"] = body,
            };

            HttpResponseMessage response = await mailgunHttpClient.PostAsync($"v3/{mailConfigSection.Domain}/messages", new FormUrlEncodedContent(form));

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Error when trying to send mail. mailFrom: {mailFrom}, emailTo: {emailTo}, body: {body}, subject: {subject}, response: {@response}", mailConfigSection.From, to, body, subject, response);
            }
        }
    }
}
