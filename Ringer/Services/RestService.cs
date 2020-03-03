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
        Task<List<PendingMessage>> PullPendingMessagesAsync();
        Task LogInAsync(string name, DateTime birthDate, GenderType genderType);
    }

    public class RESTService : IRESTService
    {
        private readonly HttpClient _client;

        public RESTService()
        {
            _client = new HttpClient();
        }

        public async Task<List<PendingMessage>> PullPendingMessagesAsync()
        {
            if (App.IsLoggedIn)
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);
            else
                return new List<PendingMessage>();

            try
            {
                string requestUri = $"{Constants.PendingUrl}?roomId={App.CurrentRoomId}&lastnumber={App.LastServerMessageId}";
                var response = await _client.GetAsync(requestUri).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException();

                // if token expired -> response.StatusCode
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Debug.WriteLine(response.Headers.WwwAuthenticate.ToString());

                    return new List<PendingMessage>();
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var pendingMessages = JsonSerializer.Deserialize<List<PendingMessage>>(responseString);

                return pendingMessages;
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Shell.Current.DisplayAlert("이럴수가", ex.Message, "닫기");
                });

                return new List<PendingMessage>();
            }
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
            var responseObject = JsonSerializer.Deserialize<LoginResponse>(responseString);

            // TODO: token 발급되었는지 확인
            // TODO: token 발급되지 않았으면 처음부터 다시? 손쉽게 오타 부분만 고칠 수 있는 UI 제공
            App.Token = responseObject.token;
            App.CurrentRoomId = responseObject.roomId;
            App.UserId = responseObject.userId;

            Debug.WriteLine(App.Token);
            Debug.WriteLine(App.CurrentRoomId);
            Debug.WriteLine(App.UserId);
        }
    }
}
