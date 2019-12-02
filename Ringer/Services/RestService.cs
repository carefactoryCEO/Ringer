using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ringer.Core.Data;
using Ringer.Helpers;

namespace Ringer.Services
{
    public interface IRESTService
    {
        Task<string> ReportDeviceStatusAsync(string deviceId, bool isOn);
        Task<List<PendingMessage>> PullPendingMessages(string roomId, int lastIndex);
    }


    public class RESTService : IRESTService
    {
        private HttpClient _client;

        public RESTService()
        {
            _client = new HttpClient();
        }

        public async Task<string> ReportDeviceStatusAsync(string deviceId, bool isOn)
        {
            _client = new HttpClient();

            var report = JsonSerializer.Serialize(new DeviceReport
            {
                DeviceId = deviceId,
                Status = isOn
            });

            HttpResponseMessage response = await _client.PostAsync(Constants.ReportUrl, new StringContent(report, Encoding.UTF8, "application/json"));

            Debug.WriteLine(response.ReasonPhrase);

            return response.ReasonPhrase;
        }

        public async Task<List<PendingMessage>> PullPendingMessages(string roomId, int lastIndex)
        {
            HttpResponseMessage response = await _client.GetAsync($"https://ringerhub.azurewebsites.net/Message/pending?roomId={roomId}&lastnumber={lastIndex}");

            var responseString = await response.Content.ReadAsStringAsync();

            var pendingMessages = JsonSerializer.Deserialize<List<PendingMessage>>(responseString);

            return pendingMessages;
        }
    }
}
