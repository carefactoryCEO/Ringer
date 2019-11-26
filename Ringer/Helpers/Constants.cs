using Xamarin.Essentials;

namespace Ringer.Helpers
{
    public static class Constants
    {
        public static readonly string AppCenterAndroid = "android=776e5a61-2f89-48c3-95b6-5fa3dde1c708;";
        public static readonly string AppCenteriOS = "ios=b1b4c859-3d1a-4f7c-bf34-b4e45a2aad65";
        public static readonly string HubUrl = DeviceInfo.DeviceType == DeviceType.Physical ? "https://ringerhub.azurewebsites.net/hubs/chat" : "http://localhost:5000/hubs/chat";
        public static readonly string TokenUrl = DeviceInfo.DeviceType == DeviceType.Physical ? "https://ringerhub.azurewebsites.net/auth/login" : "http://localhost:5000/auth/login";
        public static readonly string System = "system";
        public static readonly string ChattingRoom = "Xamarin";

        public static string DeviceId
        {
            get => Preferences.Get(nameof(DeviceId), null);
            set => Preferences.Set(nameof(DeviceId), value);
        }

        public static string Token
        {
            get => Preferences.Get(nameof(Token), null);
            set => Preferences.Set(nameof(Token), value);
        }

        public static string UserName
        {
            get => Preferences.Get(nameof(UserName), null);
            set => Preferences.Set(nameof(UserName), value);
        }
    }
}
