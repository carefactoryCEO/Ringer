using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ringer.Core.Data;
using Ringer.Helpers;
using Xamarin.Forms;

namespace Ringer.Services
{
    public interface IRESTService
    {
        Task<List<PendingMessage>> PullPendingMessagesAsync(string roomId, int lastMessageId, string token);
        Task LogInAsync(string name, DateTime birthDate, GenderType genderType);
    }

    public class RESTService : IRESTService
    {
        private readonly HttpClient _client;

        public RESTService()
        {
            _client = new HttpClient();
        }

        public async Task<List<PendingMessage>> PullPendingMessagesAsync(string roomId, int lastMessageId, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);

            try
            {
                string requestUri = $"{Constants.PendingUrl}?roomId={roomId}&lastId={lastMessageId}";
                var response = await _client.GetAsync(requestUri).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException("request failed");

                // TODO if token expired -> response.StatusCode

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new HttpRequestException("unauthorized");

                var responseString = await response.Content.ReadAsStringAsync();
                var pendingMessages = JsonSerializer.Deserialize<List<PendingMessage>>(responseString);

                return pendingMessages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task LogInAsync(string name, DateTime birthDate, GenderType genderType)
        {
            LoginInfo loginInfo = new LoginInfo
            {
                Name = name,
                BirthDate = birthDate,
                Gender = genderType,
                DeviceId = App.DeviceId,
                DeviceType = Device.RuntimePlatform == Device.iOS ? DeviceType.iOS : DeviceType.Android
            };

            var loginInfoJson = JsonSerializer.Serialize(loginInfo);

            HttpResponseMessage response = await _client.PostAsync(Constants.LoginUrl, new StringContent(loginInfoJson, Encoding.UTF8, "application/json"));

            // 로그인 실패
            if (response.StatusCode != HttpStatusCode.OK)
                Debug.WriteLine(await response.Content.ReadAsStringAsync());

            var responseJson = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseJson);

            // TODO: token 발급되었는지 확인
            // TODO: token 발급되지 않았으면 처음부터 다시? 손쉽게 오타 부분만 고칠 수 있는 UI 제공
            App.Token = loginResponse.token;
            App.RoomId = loginResponse.roomId;
            App.UserId = loginResponse.userId;

            Utilities.Trace(App.Token);
            Utilities.Trace(App.RoomId);
            Utilities.Trace(App.UserId.ToString());
        }
    }
}
