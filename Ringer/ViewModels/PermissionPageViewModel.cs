using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ringer.Helpers;
using Ringer.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;

namespace Ringer.ViewModels
{
    public class PermissionPageViewModel
    {
        public ICommand OnSelectPermissionChangeCommand { get; set; }
        public ICommand GoHomeCommand { get; set; }
        public ICommand LoadPermissionCommand { get; set; }
        public List<PermissionInfo> PermissionsList { get; set; }
        public PermissionInfo PermissionSelected { get; set; }

        public PermissionPageViewModel()
        {
            LoadPermissionCommand = new Command(async () => await LoadPermissions());
            LoadPermissionCommand.Execute(null);

            OnSelectPermissionChangeCommand = new Command(async () =>
            {
                if (PermissionSelected != null)
                {
                    PermissionSelected.IsGranted = await CheckAndRequestPermissionAsync(PermissionSelected.Permission) == PermissionStatus.Granted;
                }
            });

            GoHomeCommand = new Command(async () =>
            {
                await App.Current.MainPage.DisplayAlert("Hey", "Welcome to my App", "Ok");
            });
        }

        async Task LoadPermissions()
        {
            PermissionsList = new List<PermissionInfo>()
            {
                { await CreatePermission(new LocationWhenInUse(), MaterialFont.MapMarker, "위치(필수)", "위치를 전송하고 주변의 공관 및 병원 등 정보를 위치 기반으로 수신합니다.") },
                { await CreatePermission(new Camera(), MaterialFont.Camera, "카메라", "사진과 비디오를 찍어 저장하고 전송합니다.")},

            };

            if (Xamarin.Forms.Device.RuntimePlatform == Device.iOS)
            {
                PermissionsList.Add(await CreatePermission(new Microphone(), MaterialFont.Microphone, "마이크", "비디오에 소리를 넣고 음성 채팅에 참여합니다."));
                PermissionsList.Add(await CreatePermission(new Photos(), MaterialFont.FolderMultipleImage, "사진첩", "사진첩에 사진과 비디오를 저장하거나 사진첩에서 불러와 전송합니다."));
            }

            if (Xamarin.Forms.Device.RuntimePlatform == Device.Android)
            {
                PermissionsList.Add(await CreatePermission(new StorageRead(), MaterialFont.FolderMultipleImage, "저장소", "사용자의 폰 저장소에서 사진과 비디오를 전송합니다."));
            }
        }

        async Task<PermissionInfo> CreatePermission(BasePermission permission, string icon, string title, string description)
        {
            return new PermissionInfo()
            {
                Permission = permission,
                Icon = icon,
                Title = title,
                Description = description,
                IsGranted = await permission.CheckStatusAsync() == PermissionStatus.Granted
            };
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
    }
}
