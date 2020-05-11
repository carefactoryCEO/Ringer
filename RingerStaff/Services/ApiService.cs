using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ringer.Core.Data;
using RingerStaff.Models;
using Xamarin.Forms;

namespace RingerStaff.Services
{
    public static class ApiService
    {
        static readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// 서버에서 토큰을 받아온다.
        /// </summary>
        /// <param name="email">사용자 이메일</param>
        /// <param name="password">사용자 비밀번호</param>
        /// <returns>토큰. 실패시 string.Empty</returns>
        public static async Task<string> LogInAsync(string email, string password)
        {
            if (string.IsNullOrEmpty(App.DeviceId))
                return null;

            // TODO: refactor mapping
            var loginInfo = JsonSerializer.Serialize(new LoginInfo
            {
                Email = email,
                Password = password,
                DeviceId = App.DeviceId,
                DeviceType = Device.RuntimePlatform == Device.iOS ? DeviceType.iOS : DeviceType.Android
            });

            // TODO: implement Refit and Polly
            // TODO: make url constant
            HttpResponseMessage response = await _client.PostAsync(App.LoginUrl, new StringContent(loginInfo, Encoding.UTF8, "application/json"));

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                LoginResponse loginResult = JsonSerializer.Deserialize<LoginResponse>(responseString);

                App.UserId = loginResult.userId;
                App.UserName = loginResult.userName;

                return loginResult.token;
            }

            return null;
        }

        public class RoomsResponse
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public bool IsClosed { get; set; }
            public string[] Enrollments { get; set; }
        }

        public static async Task<string> GetRoomNameByIdAsync(string roomId)
        {
            try
            {
                if (!_client.DefaultRequestHeaders.Contains("Authorization"))
                    _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + App.Token);

                var url = App.BaseUrl + $"/rooms/name?roomId={roomId}";
                var result = await _client.GetStringAsync(url);

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        public static async Task<List<RoomInformation>> LoadRoomsAsync()
        {
            if (App.IsLoggedIn)
            {
                try
                {
                    if (!_client.DefaultRequestHeaders.Contains("Authorization"))
                        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + App.Token);

                    string response = await _client.GetStringAsync(App.BaseUrl + "/rooms/room-with-informations");

                    var rooms = JsonSerializer.Deserialize<List<RoomInformation>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return rooms;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            return null;
        }

        public static async Task<List<PendingMessage>> PullPendingMessagesAsync(string roomId, int lastMessageId, string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);

            try
            {
                string requestUri = $"{App.PendingUrl}?roomId={roomId}&lastId={lastMessageId}";
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

        public static async Task<List<PendingMessage>> GetSegmentedMessages(string roomId, int skip, int take)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Token);

            try
            {
                string requestUri = $"{App.AdditionalMessagesUrl}?roomId={roomId}&skip={skip}&take={take}";
                var response = await _client.GetAsync(requestUri).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException("request failed");

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
    }
}
