using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ringer.Core.Data;
using Ringer.Helpers;
using Xamarin.Forms;

namespace Ringer.Services
{
    public interface IRESTService
    {
        void ReportDeviceStatus(bool isOn);
        Task ReportDeviceStatusDebouncedAsync(bool isOn, int debounceMilliSeconds = 50);
        Task<List<PendingMessage>> PullPendingMessagesAsync();
        Task LogInAsync(string name, DateTime birthDate, GenderType genderType);
    }

    public class RESTService : IRESTService
    {
        private HttpClient _client;
        private CancellationTokenSource _cts;

        public RESTService()
        {
            _client = new HttpClient();
            _cts = new CancellationTokenSource();
        }

        public async Task ReportDeviceStatusDebouncedAsync(bool isOn, int debounceMilliSeconds)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var thisToken = _cts.Token;

            await Task.Delay(debounceMilliSeconds);

            if (!thisToken.IsCancellationRequested)
            {
                if (App.DeviceId == null || App.DeviceIsOn == isOn)
                    return;

                App.DeviceIsOn = isOn;

                var report = JsonSerializer.Serialize(new DeviceReport
                {
                    DeviceId = App.DeviceId,
                    Status = isOn
                });

                var response = await _client.PostAsync(Constants.ReportUrl, new StringContent(report, Encoding.UTF8, "application/json")).ConfigureAwait(false);

                Debug.WriteLine($"[ReportDeviceStatusAsync()]IsOn:{isOn}, success:{response.IsSuccessStatusCode}, response:{response.ReasonPhrase}");
            }
        }

        public void ReportDeviceStatus(bool isOn)
        {
            if (App.DeviceId == null || App.DeviceIsOn == isOn)
                return;

            App.DeviceIsOn = isOn;

            var report = JsonSerializer.Serialize(new DeviceReport
            {
                DeviceId = App.DeviceId,
                Status = isOn
            });

            Task.Run(async () =>
            {
                var response = await _client.PostAsync(Constants.ReportUrl, new StringContent(report, Encoding.UTF8, "application/json")).ConfigureAwait(false);

                Debug.WriteLine($"[ReportDeviceStatusAsync()]IsOn:{isOn}, success:{response.IsSuccessStatusCode}, response:{response.ReasonPhrase}");

            }).ConfigureAwait(false);
        }

        public async Task<List<PendingMessage>> PullPendingMessagesAsync()
        {
            var response = await _client.GetAsync($"{Constants.PendingUrl}?roomId={App.CurrentRoomId}&lastnumber={App.LastMessageId}").ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync();
            var pendingMessages = JsonSerializer.Deserialize<List<PendingMessage>>(responseString);

            return pendingMessages;
        }

        public async Task LogInAsync(string name, DateTime birthDate, GenderType genderType)
        {

            var loginInfo = JsonSerializer.Serialize(new LoginInfo
            {
                Name = App.UserName,
                BirthDate = birthDate,
                Gender = genderType,
                DeviceId = App.DeviceId,
                DeviceType = Device.RuntimePlatform == Device.iOS ? DeviceType.iOS : DeviceType.Android
            });

            Debug.WriteLine(loginInfo);

            HttpResponseMessage response = await _client.PostAsync(Constants.LoginUrl, new StringContent(loginInfo, Encoding.UTF8, "application/json"));

            // 로그인 실패
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                Debug.WriteLine(await response.Content.ReadAsStringAsync());

            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<ResponseJson>(responseString);

            // TODO: token 발급되었는지 확인
            // TODO: token 발급되지 않았으면 처음부터 다시? 손쉽게 오타 부분만 고칠 수 있는 UI 제공
            App.Token = responseObject.token;
            App.CurrentRoomId = responseObject.roomId;

            Debug.WriteLine(App.Token);
            Debug.WriteLine(App.CurrentRoomId);
        }
    }
}
