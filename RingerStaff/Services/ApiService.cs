using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ringer.Core.Data;
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

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<ResponseJson>(responseString);

                return responseObject.token;
            }

            return null;
        }
    }
}
