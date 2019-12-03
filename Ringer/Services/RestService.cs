using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ringer.Core.Data;
using Ringer.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;
using EssentialDeviceType = Xamarin.Essentials.DeviceType;

namespace Ringer.Services
{
    public interface IRESTService
    {
        Task ReportDeviceStatusAsync(bool isOn = false);
        Task<List<PendingMessage>> PullPendingMessages(string roomId, int lastIndex);
    }

    public class RESTService : IRESTService
    {
        private HttpClient _client;

        public RESTService()
        {
            _client = new HttpClient();
        }

        public async Task ReportDeviceStatusAsync(bool isOn = false)
        {
            if (App.DeviceId == null)
                return;

            _client = new HttpClient();

            var report = JsonSerializer.Serialize(new DeviceReport
            {
                DeviceId = App.DeviceId,
                Status = isOn
            });

            var response = await _client.PostAsync(Constants.ReportUrl, new StringContent(report, Encoding.UTF8, "application/json"));
            Debug.WriteLine($"{response.IsSuccessStatusCode}:{response.ReasonPhrase}");
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
