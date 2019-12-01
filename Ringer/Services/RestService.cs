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
    }

    public class RESTService : IRESTService
    {
        public async Task<string> ReportDeviceStatusAsync(string deviceId, bool isOn)
        {
            HttpClient client = new HttpClient();

            var report = JsonSerializer.Serialize(new DeviceReport
            {
                DeviceId = deviceId,
                Status = isOn
            });

            HttpResponseMessage response = await client.PostAsync(Constants.ReportUrl, new StringContent(report, Encoding.UTF8, "application/json"));

            Debug.WriteLine(response.ReasonPhrase);

            return response.ReasonPhrase;
        }
    }
}
