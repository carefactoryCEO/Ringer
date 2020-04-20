using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ringer.Helpers;
using Ringer.Models;
using Ringer.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;

namespace Ringer.ViewModels
{
    public class PermissionPageViewModel
    {
        public ICommand LoadPermissionCommand { get; set; }
        public ICommand ContinueCommand { get; set; }
        public List<PermissionInfo> PermissionsList { get; set; }

        public PermissionPageViewModel()
        {
            LoadPermissionCommand = new Command(async () => await LoadPermissions());
            LoadPermissionCommand.Execute(null);

            ContinueCommand = new Command(async () => await ContinueAsync());
        }

        private async Task ContinueAsync()
        {
            foreach (var permissionInfo in PermissionsList.Where(p => !p.IsGranted))
            {
                permissionInfo.IsGranted = await permissionInfo.Permission.RequestAsync() == PermissionStatus.Granted;
            }

            await Task.Delay(20).ContinueWith(t =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage = new AppShell();
                });
            });
        }

        async Task LoadPermissions()
        {

            PermissionsList = new List<PermissionInfo>();

            if (Device.RuntimePlatform == Device.iOS)
            {
                var noti = new Notification();
                var notiInfo = new PermissionInfo()
                {
                    Permission = noti,
                    Icon = MaterialFont.Bell,
                    Title = "알림",
                    Description = "위험지역, 건강정보, 메시지 등 알림 메시지를 수신합니다.",
                    IsGranted = false
                };

                PermissionsList.Add(notiInfo);
                PermissionsList.Add(await CreatePermission(new LocationWhenInUse(), MaterialFont.MapMarker, "위치", "위치를 전송하고 주변의 공관 및 병원 등 정보를 위치 기반으로 수신합니다."));
                PermissionsList.Add(await CreatePermission(new Camera(), MaterialFont.Camera, "카메라", "사진과 비디오를 찍어 저장하고 전송합니다."));
                PermissionsList.Add(await CreatePermission(new Microphone(), MaterialFont.Microphone, "마이크", "비디오에 소리를 넣고 음성 채팅에 참여합니다."));
                PermissionsList.Add(await CreatePermission(new Photos(), MaterialFont.FolderMultipleImage, "사진첩", "사진첩에 사진과 비디오를 저장하거나 사진첩에서 불러와 전송합니다."));


                notiInfo.IsGranted = await noti.CheckStatusAsync() == PermissionStatus.Granted;
            }

            if (Xamarin.Forms.Device.RuntimePlatform == Device.Android)
            {
                PermissionsList.Add(await CreatePermission(new LocationWhenInUse(), MaterialFont.MapMarker, "위치", "위치를 전송하고 주변의 공관 및 병원 등 정보를 위치 기반으로 수신합니다."));
                PermissionsList.Add(await CreatePermission(new Camera(), MaterialFont.Camera, "카메라", "사진과 비디오를 찍어 저장하고 전송합니다."));
                PermissionsList.Add(await CreatePermission(new StorageRead(), MaterialFont.FolderMultipleImage, "저장소", "사용자의 폰 저장소에서 사진과 비디오를 전송합니다."));
            }

        }

        async Task<PermissionInfo> CreatePermission(BasePermission permission, string icon, string title, string description)
        {
            var info = new PermissionInfo()
            {
                Permission = permission,
                Icon = icon,
                Title = title,
                Description = description,
                IsGranted = await permission.CheckStatusAsync() == PermissionStatus.Granted
            };

            return info;
        }

        async Task<PermissionStatus> CheckAndRequestPermissionAsync(BasePermission permission)
        {
            var status = await permission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await permission.RequestAsync();
            }
            return status;
        }

        public class Notification : BasePlatformPermission
        {
            private INotificationPermissionService notificationPermission;

            public Notification()
            {
                notificationPermission = DependencyService.Get<INotificationPermissionService>();
            }

            public override async Task<PermissionStatus> CheckStatusAsync()
            {
                var granted = await notificationPermission.IsNotificationPermissionGranted();
                if (granted)
                    return PermissionStatus.Granted;

                return PermissionStatus.Unknown;
            }

            public override void EnsureDeclared()
            {

            }

            public override async Task<PermissionStatus> RequestAsync()
            {
                var result = await notificationPermission.RequestNotificationPermission();

                if (result)
                    return PermissionStatus.Granted;

                return PermissionStatus.Unknown;
            }
        }
    }
}
