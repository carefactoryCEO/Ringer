using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Ringer.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    class ChatPageViewModel : INotifyPropertyChanged
    {
        #region private members
        private App app;
        private bool isBusy = false;
        #endregion

        #region Public Properties
        public ObservableCollection<Message> Messages => app.Messages;
        public string TextToSend { get; set; }
        public CameraAction CameraAction { get; } = new CameraAction();
        public double NavBarHeight { get; set; } = 0;
        public string NavBarTitle => "링거 상담실";
        #endregion

        #region Commands
        public ICommand SendMessageCommand { get; }
        public ICommand CameraCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand ShowVidyoCommand { get; }
        #endregion

        #region Constructor
        public ChatPageViewModel()
        {
            if (DesignMode.IsDesignModeEnabled)
                return;

            app = Xamarin.Forms.Application.Current as App;
            
            SendMessageCommand  = new Command(async () => await SendMessage());
            GoBackCommand       = new Command(async () => await Shell.Current.Navigation.PopAsync());
            ShowVidyoCommand    = new Command(async () => {
                await Shell.Current.GoToAsync("vidyopage");
                });
            CameraCommand       = new Command<string>(async actionString => await ProcessCameraAction(actionString));
        }
        #endregion

        #region Private Methods
        private async Task SendMessage()
        {
            if (string.IsNullOrEmpty(TextToSend) || isBusy)
                return;

            if (!app.IsSignalRConnected)
            {
                await Shell.Current.DisplayAlert("Not connected", "Please connect to the server and try again.", "OK");
                return;
            }

            try
            {
                isBusy = true;
                await app.SignalR.SendMessageAsync((string)app.Properties["Group"], (string)app.Properties["User"], TextToSend);

                TextToSend = string.Empty;
            }
            catch (Exception ex)
            {
                app.AddMessage($"vs.SendMessage:Send failed: {ex.Message}", String.Empty);
            }
            finally
            {
                isBusy = false;
            }
        }

        private async Task ProcessCameraAction(string action)
        {
            if (action == "설정 열기")
            {
                CrossPermissions.Current.OpenAppSettings();
            }

            // 사진 촬영
            if (action == CameraAction.TakingPhoto)
            {
                if (await TakingPhotoPermittedAsync())
                {
                    if (!CrossMedia.Current.IsTakePhotoSupported)
                    {
                        await Shell.Current.DisplayAlert("사진촬영 불가", "촬영 가능한 카메라가 없습니다 :(", "확인");
                        return;
                    }

                    try
                    {
                        var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                        {
                            Directory = "RingerPhoto",
                            SaveToAlbum = true,
                            CompressionQuality = 75,
                            CustomPhotoSize = 50,
                            PhotoSize = PhotoSize.MaxWidthHeight,
                            MaxWidthHeight = 2000,
                            DefaultCamera = CameraDevice.Rear
                        });

                        if (file == null)
                            return;

                        app.AddMessage($"{action}:{file.Path}", "camera");

                        file.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);

                    }
                }
            }

            // 비디오 찍기
            if (action == CameraAction.TakingVideo)
            {
                if (await TakingVideoPermittedAsync())
                {
                    if (!CrossMedia.Current.IsTakeVideoSupported)
                    {
                        await Shell.Current.DisplayAlert("동영상 촬영 불가", "촬영 가능한 카메라가 없습니다 :(", "확인");
                        return;
                    }

                    try
                    {
                        var file = await CrossMedia.Current.TakeVideoAsync(new StoreVideoOptions
                        {
                            Name = "VIDEO-" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".mp4",
                            Directory = "RingerVideo",
                            SaveToAlbum = true
                        });

                        if (file == null)
                            return;

                        app.AddMessage($"{action}:{file.Path}", "camera");

                        file.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            // 사진 불러오기
            if (action == CameraAction.AttachingPhoto)
            {
                if (AttachingPhotoPermitted())
                {
                    try
                    {
                        if (!CrossMedia.Current.IsPickPhotoSupported)
                        {
                            await Shell.Current.DisplayAlert("사진 불러오기 실패", "사진 불러오기가 지원되지 않는 기기입니다. :(", "확인");
                            return;
                        }

                        var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                        {
                            PhotoSize = PhotoSize.Medium
                        });

                        if (file == null)
                            return;

                        app.AddMessage($"{action}:{file.Path}", "camera");

                        file.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            // 비디오 불러오기
            if (action == CameraAction.AttachingVideo)
            {
                if (AttachingVideoPermitted())
                {
                    if (!CrossMedia.Current.IsPickVideoSupported)
                    {
                        await Shell.Current.DisplayAlert("비디오 불러오기 실패", "비디오 접근 권한이 없습니다 :(", "확인");

                        return;
                    }

                    try
                    {
                        var file = await CrossMedia.Current.PickVideoAsync();

                        if (file == null)
                            return;

                        app.AddMessage($"{action}:{file.Path}", "camera");

                        file.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }

        private async Task<bool> CheckPhotosPermissionsAsync()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync<PhotosPermission>();

            if (status == PermissionStatus.Granted)
                return true;
            else
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Photos))
                {
                    await Shell.Current.DisplayAlert("사진 접근 권한 요청", "사진, 동영상을 촬영하려면 사진 접근 권한을 허용해야 합니다.", "확인");
                }

                status = await CrossPermissions.Current.RequestPermissionAsync<PhotosPermission>();

                if (status == PermissionStatus.Granted)
                    return true;
                else if (status != PermissionStatus.Unknown)
                {
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        bool goSetting = await Shell.Current.DisplayAlert("권한이 필요합니다.", "사진 접근 권한을 허용하지 않았습니다. 한 번 거부한 권한은 iOS설정에서만 변경할 수 있습니다.", "iOS설정 가기", "확인");

                        if (goSetting)
                            CrossPermissions.Current.OpenAppSettings();
                    }
                }
            }

            return false;
        }
        private async Task<bool> CheckCameraPermissionAsync()
        {
            // camera availability check
            if (!CrossMedia.Current.IsCameraAvailable)
            {
                await Shell.Current.DisplayAlert("카메라 사용 불가", "사용 가능한 카메라가 없습니다 :(", "확인");
                return false;
            }

            // camera permission check
            var cameraPermissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync<CameraPermission>();

            if (cameraPermissionStatus == PermissionStatus.Granted)
                return true;
            else
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Camera))
                {
                    await Shell.Current.DisplayAlert("카메라 사용 권한 요청", "사진, 동영상을 촬영하려면 카메라 사용 권한을 허용해야 합니다.", "확인");
                }

                cameraPermissionStatus = await CrossPermissions.Current.RequestPermissionAsync<CameraPermission>();

                if (cameraPermissionStatus == PermissionStatus.Granted)
                    return true;
                else if (cameraPermissionStatus != PermissionStatus.Unknown)
                {
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        bool goSetting = await Shell.Current.DisplayAlert("권한이 필요합니다.", "카메라 사용 권한을 허용하지 않았습니다. 한 번 거부한 권한은 iOS설정에서만 변경할 수 있습니다.", "iOS설정 가기", "확인");

                        if (goSetting)
                            CrossPermissions.Current.OpenAppSettings();
                    }
                }
            }

            return false;
        }
        private async Task<bool> CheckMicPermissionAsync()
        {
            if (Device.RuntimePlatform == Device.Android)
                return true;

            var status = await CrossPermissions.Current.CheckPermissionStatusAsync<MicrophonePermission>();

            Debug.WriteLine(status.ToString());

            if (status == PermissionStatus.Granted)
                return true;
            else
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Microphone))
                {
                    await Shell.Current.DisplayAlert("마이크 사용 권한 요청", "동영상을 촬영하려면 마이크 사용 권한을 허용해야 합니다.", "확인");

                    Debug.WriteLine("should show");
                }

                status = await CrossPermissions.Current.RequestPermissionAsync<MicrophonePermission>();

                Debug.WriteLine(status.ToString());

                if (status == PermissionStatus.Granted)
                    return true;
                else if (status != PermissionStatus.Unknown)
                {
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        bool goSetting = await Shell.Current.DisplayAlert("권한이 필요합니다.", "마이크 사용 권한을 허용하지 않았습니다. 한 번 거부한 권한은 iOS설정에서만 변경할 수 있습니다.", "iOS설정 가기", "확인");

                        if (goSetting)
                            CrossPermissions.Current.OpenAppSettings();
                    }
                }
            }

            Debug.WriteLine("no mic permission");

            return false;
        }

        private bool AttachingPhotoPermitted() => true;
        private bool AttachingVideoPermitted() => true;
        private async Task<bool> TakingPhotoPermittedAsync() => await CheckCameraPermissionAsync() && await CheckPhotosPermissionsAsync();
        private async Task<bool> TakingVideoPermittedAsync() => await CheckCameraPermissionAsync() && await CheckPhotosPermissionsAsync() && await CheckMicPermissionAsync();
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }

    #region CamaraAction Class
    class CameraAction
    {
        public string Title { get; } = "작업을 선택하세요.";
        public string Cancle { get; } = "취소";
        public string Destruction { get; } = "파파괴";
        public string TakingPhoto { get; } = "사진 촬영";
        public string AttachingPhoto { get; } = "사진 불러오기";
        public string TakingVideo { get; } = "동영상 촬영";
        public string AttachingVideo { get; } = "동영상 불러오기";
    }
    #endregion
}