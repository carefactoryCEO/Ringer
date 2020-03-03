using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
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
        static HttpClient _client = new HttpClient();

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
                var responseObject = JsonSerializer.Deserialize<LoginResponse>(responseString);

                return responseObject.token;
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

        public static async Task<List<RoomModel>> LoadRoomsAsync()
        {
            // header에 토큰을 넣는다.
            if (App.IsLoggedIn)
            {
                try
                {
                    if (!_client.DefaultRequestHeaders.Contains("Authorization"))
                        _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + App.Token);

                    var response = await _client.GetStringAsync(App.BaseUrl + "/auth/list");
                    //var apiResponse = await _client.GetAsync(App.BaseUrl + "/rooms");

                    Debug.WriteLine(response);

                    var rooms = JsonSerializer.Deserialize<List<RoomsResponse>>(response);

                    var roomModels = new List<RoomModel>();

                    foreach (var room in rooms)
                        roomModels.Add(new RoomModel { Id = room.Id, Title = room.Name });

                    return roomModels;


                    //if (response.StatusCode == HttpStatusCode.OK)
                    //{
                    //    var responseContent = await apiResponse.Content.ReadAsStringAsync();
                    //    var obj = JsonSerializer.Deserialize<RoomsResponse>(responseContent);

                    //    foreach (var room in obj.rooms)
                    //        Debug.WriteLine(room);
                    //}
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

            }


            // Api -> get room id list from server

            // foreach room id : get lastMessage -> lastmessage.Body, lastMessage.arrivedAt, unread count


            var result = new List<RoomModel>
            {
                new RoomModel
                {
                    Title = "신모범 43M 시카고(미국) 두통",
                    LastMessage = "어제부터 오른쪽 관자놀이가 아프더라구요.",
                    LastMessageArrivedAt = DateTime.Now.Subtract(TimeSpan.FromHours(1)),
                    UnreadMessagesCount = 5
                },
                new RoomModel
                {
                    Title = "김순용 39M 방콕(태국) 무좀",
                    LastMessage = "어렸을 때부터 오른발에 무좀이 심했어요.",
                    LastMessageArrivedAt = DateTime.Now.Subtract(TimeSpan.FromMinutes(58)),
                    UnreadMessagesCount = 4
                }
            };

            return result;
        }
    }
}
