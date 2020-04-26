using System;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Ringer.Helpers
{
    public static class Constants
    {
        public static readonly string AppCenterAndroid = "android=776e5a61-2f89-48c3-95b6-5fa3dde1c708;";
        public static readonly string AppCenteriOS = "ios=b1b4c859-3d1a-4f7c-bf34-b4e45a2aad65";
        public static readonly string BaseUrl = DeviceInfo.DeviceType == DeviceType.Physical ? "https://ringerhub.azurewebsites.net" : Device.RuntimePlatform == Device.iOS ? "http://localhost:5000" : "http://10.0.2.2:5000";
        public static readonly string PendingUrl = BaseUrl + "/message/pending";
        public static readonly string HubUrl = BaseUrl + "/hubs/chat";
        public static readonly string LoginUrl = BaseUrl + "/auth/login";
        public static readonly string RegisterConsumerUrl = BaseUrl + "/auth/register-consumer";
        public static readonly string LoginConsumerUrl = BaseUrl + "/auth/login-consumer";
        public static readonly string DeviceCheckUrl = BaseUrl + "/auth/check-device-activity";

        public static readonly string InformationUrl = BaseUrl + "/information";
        public static readonly string ConsulateUrl = InformationUrl + "/consulates";
        public static readonly string DbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ringer.db3");
        public static readonly string System = "링거";

        // Camera action
        public static readonly string Title = "작업을 선택하세요.";
        public static readonly string Cancle = "취소";
        public static readonly string Destruction = "파파괴";
        public static readonly string TakingPhoto = "사진 촬영";
        public static readonly string AttachingPhoto = "사진 불러오기";
        public static readonly string TakingVideo = "동영상 촬영";
        public static readonly string AttachingVideo = "동영상 불러오기";

        public static readonly string BlobContainerName = "ringer";
        public static readonly string BlobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=ringerstoragekr;AccountKey=D3egnQI+dAWuKGMEjR10TaSe+2VtxHP2PMCivX6AlyvKEYBdaLZgxb8U7U0QQooRG5NJa/iQkjMFJ452Em+QLg==;EndpointSuffix=core.windows.net";

        // Message
        public static readonly int MessageCount = 100;
        public static readonly string ImagePlaceholder = "https://ringerstoragekr.blob.core.windows.net/ringer/ringer-placeholder.jpg";

        public static readonly string DisconnectedString = "Disconnected";

        public static readonly string PushNotificationString = "pushNotification";
        public static readonly string LocalNotificationString = "localNotification";
        public static readonly string ChatPageUriFromLocalNotification = $"//MapPage/ChatPage?from={LocalNotificationString}";
        public static readonly string ChatPageUriFromPushNotification = $"//MapPage/ChatPage?from={PushNotificationString}";

        public static readonly string RingerPhoneNumber = "+82-10-2922-7428";
        public static readonly string RingerEmergencyPhoneNumber = "+82-10-2922-7428";

        public static readonly string FootPrintUrl = InformationUrl + "/foot-print";
        public static readonly string TermsUrl = BaseUrl + "/auth/terms";

    }
}
