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
        void ReportDeviceStatus(bool isOn = false);
        Task<List<PendingMessage>> PullPendingMessages(string roomId, int lastIndex);
    }

    public class RESTService : IRESTService
    {
        private HttpClient _client;

        public RESTService()
        {
            _client = new HttpClient();
        }

        public void ReportDeviceStatus(bool isOn = false)
        {
            if (App.DeviceId == null)
                return;

            var report = JsonSerializer.Serialize(new DeviceReport
            {
                DeviceId = App.DeviceId,
                Status = isOn
            });

            Task.Run(async () =>
            {
                var response = await _client.PostAsync(Constants.ReportUrl, new StringContent(report, Encoding.UTF8, "application/json"));

                Debug.WriteLine($"[ReportDeviceStatusAsync()]IsOn:{isOn}, success:{response.IsSuccessStatusCode}, response:{response.ReasonPhrase}");
            }).ConfigureAwait(false);
        }

        public async Task<List<PendingMessage>> PullPendingMessages(string roomId, int lastIndex = 0)
        {
            var response = await _client.GetAsync($"{Constants.PendingUrl}?roomId={roomId}&lastnumber={lastIndex}");
            var responseString = await response.Content.ReadAsStringAsync();
            var pendingMessages = JsonSerializer.Deserialize<List<PendingMessage>>(responseString);

            return pendingMessages;
        }
    }
}
