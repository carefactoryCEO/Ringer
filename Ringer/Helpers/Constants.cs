using Xamarin.Essentials;
using Xamarin.Forms;

namespace Ringer.Helpers
{
    public static class Constants
    {
        public static readonly string AppCenterAndroid = "android=776e5a61-2f89-48c3-95b6-5fa3dde1c708;";
        public static readonly string AppCenteriOS = "ios=b1b4c859-3d1a-4f7c-bf34-b4e45a2aad65";
        public static readonly string HubUrl = DeviceInfo.DeviceType == DeviceType.Physical ? "https://ringerhub.azurewebsites.net/hubs/chat" : Device.RuntimePlatform == Device.iOS ? "http://localhost:5000/hubs/chat" : "http://10.0.2.2:5000/hubs/chat";
        public static readonly string LoginUrl = DeviceInfo.DeviceType == DeviceType.Physical ? "https://ringerhub.azurewebsites.net/auth/login" : Device.RuntimePlatform == Device.iOS ? "http://localhost:5000/auth/login" : "http://10.0.2.2:5000/auth/login";
        public static readonly string System = "system";
        public static readonly string ChattingRoom = "Xamarin";

        // Camera action
        public static readonly string Title = "작업을 선택하세요.";
        public static readonly string Cancle = "취소";
        public static readonly string Destruction = "파파괴";
        public static readonly string TakingPhoto = "사진 촬영";
        public static readonly string AttachingPhoto = "사진 불러오기";
        public static readonly string TakingVideo = "동영상 촬영";
        public static readonly string AttachingVideo = "동영상 불러오기";


    }
}
