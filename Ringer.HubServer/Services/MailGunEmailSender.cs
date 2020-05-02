using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ringer.HubServer.Services
{
    public interface IEmailSender
    {
        string GetRandomString(int length = 8, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*");
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
        public string GetRandomString(int length = 8, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*")
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "length cannot be less than zero.");
            if (string.IsNullOrEmpty(allowedChars)) throw new ArgumentException("allowedChars may not be empty.");

            const int byteSize = 0x100;
            var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
            if (byteSize < allowedCharSet.Length) throw new ArgumentException(String.Format("allowedChars may contain no more than {0} characters.", byteSize));

            // Guid.NewGuid and System.Random are not particularly random. By using a
            // cryptographically-secure random number generator, the caller is always
            // protected, regardless of use.
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var result = new StringBuilder();
                var buf = new byte[128];
                while (result.Length < length)
                {
                    rng.GetBytes(buf);
                    for (var i = 0; i < buf.Length && result.Length < length; ++i)
                    {
                        // Divide the byte into allowedCharSet-sized groups. If the
                        // random value falls into the last group and the last group is
                        // too small to choose from the entire allowedCharSet, ignore
                        // the value in order to avoid biasing the result.
                        var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i]) continue;
                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }
                return result.ToString();
            }
        }
    }
}