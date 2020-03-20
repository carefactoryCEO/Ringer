using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ringer.HubServer.Services
{
    public interface IPushService
    {
        Task<bool> Push(string title, string message, Dictionary<string, string> customData);
    }

    public class PushService : IPushService
    {
        Receiver receiver = new Receiver();

        public PushService(Dictionary<string, string> dicInstallIdPlatform)
        {
            //Simply get all the Install IDs for the receipient with the platform name as the value
            foreach (string key in dicInstallIdPlatform.Keys)
            {
                switch (dicInstallIdPlatform[key])
                {
                    case "Android":
                        receiver.AndroidDevices.Add(key);

                        break;

                    case "iOS":
                        receiver.IOSDevices.Add(key);

                        break;
                }
            }
        }

        public class Constants
        {
            public const string Url = "https://api.appcenter.ms/v0.1/apps";
            public const string ApiKeyName = "X-API-Token";
            public const string FullAccessToken = "aaafc1f9b35195b30c8985152cb6f4a5acd4b93a";

            public const string DeviceTarget = "devices_target";
            public class Apis { public const string Notification = "push/notifications"; }

            public const string AppNameAndroid = "ringer.android";
            public const string AppNameiOS = "ringer.ios";

            public const string Organization = "mbshin-carefactory.co.kr";
        }

        [JsonObject]
        public class PushContent
        {
            [JsonProperty("notification_target")]
            public Target Target { get; set; }

            [JsonProperty("notification_content")]
            public Content Content { get; set; }
        }

        [JsonObject]
        public class Content
        {
            public Content()
            {
                Name = "default";   //By default cannot be empty, must have at least 3 characters
            }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("custom_data")]
            public IDictionary<string, string> CustomData { get; set; }
        }

        [JsonObject]
        public class Target
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("devices")]
            public IEnumerable Devices { get; set; }
        }

        public class Receiver
        {
            public Receiver()
            {
                IOSDevices = new List<string>();
                AndroidDevices = new List<string>();
            }

            public List<string> IOSDevices { get; set; }
            public List<string> AndroidDevices { get; set; }
        }

        public async Task<bool> Push(string title, string message, Dictionary<string, string> customData = default)
        {
            try
            {
                if (title.Length > 100)
                    title = title.Substring(0, 95) + "...";

                if (message.Length > 100)
                    message = message.Substring(0, 95) + "...";

                if (!receiver.IOSDevices.Any() && !receiver.AndroidDevices.Any())
                    return false; //No devices to send

                //To make sure in Android, title and message is retain when click from notification. Else it's lost when app is in background
                if (customData == null)
                    customData = new Dictionary<string, string>();

                if (!customData.ContainsKey("Title"))
                    customData.Add("Title", title);

                if (!customData.ContainsKey("Message"))
                    customData.Add("Message", message);

                //custom data cannot exceed 100 char 
                foreach (string key in customData.Keys)
                {
                    if (customData[key].Length > 100)
                    {
                        customData[key] = customData[key].Substring(0, 95) + "...";
                    }
                }

                var push = new PushContent
                {
                    Content = new Content
                    {
                        Title = title,
                        Body = message,
                        CustomData = customData
                    },
                    Target = new Target
                    {
                        Type = Constants.DeviceTarget
                    }
                };

                HttpClient httpClient = new HttpClient();

                //Set the content header to json and inject the token
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, Constants.FullAccessToken);

                //Needed to solve SSL/TLS issue when 
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                if (receiver.IOSDevices.Any())
                {
                    //receiver.IOSDevices.Add("77a6557d-f528-412d-b9d2-a5435b456c3e");
                    //receiver.IOSDevices.Add("43a4181d-160a-44de-ad18-4514e709e78e");
                    push.Target.Devices = receiver.IOSDevices;

                    string content = JsonConvert.SerializeObject(push);

                    HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                    string URL = $"{Constants.Url}/{Constants.Organization}/{Constants.AppNameiOS}/{Constants.Apis.Notification}";

                    var result = await httpClient.PostAsync(URL, httpContent);
                }

                if (receiver.AndroidDevices.Any())
                {
                    //receiver.AndroidDevices.Add("3b49b6a6-724a-4a70-9114-0bded38dbea6");
                    //receiver.AndroidDevices.Add("da67bf91-c6c6-485a-9002-08279505cb25");
                    push.Target.Devices = receiver.AndroidDevices;

                    string content = JsonConvert.SerializeObject(push);

                    HttpContent httpContent = new StringContent(content, Encoding.UTF8, "application/json");

                    string URL = $"{Constants.Url}/{Constants.Organization}/{Constants.AppNameAndroid}/{Constants.Apis.Notification}";

                    var result = await httpClient.PostAsync(URL, httpContent);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                return false;
            }
        }
    }
}
