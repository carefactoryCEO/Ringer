using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Ringer.Models;
using Ringer.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Ringer.ViewModels
{
    class ChatPageViewModel : INotifyPropertyChanged
    {
        #region private members
        private App app;
        private bool isBusy = false;
        #endregion

        #region Public Properties
        // TODO: Check if app.Messages notify changes
        // else Use Dependency service
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

            // 사진 찍기
            if (action == CameraAction.TakingPhoto)
            {
                if (await CheckCameraAndStoragePermissionsAsync() && await CheckMediaLibraryPermissionAsync())
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
                            Directory = "Test",
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
                if (await CheckCameraAndStoragePermissionsAsync() && await CheckMediaLibraryPermissionAsync())
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
                            Directory = "Test"
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
                if (await CheckStoragePermissionAsync() && await CheckMediaLibraryPermissionAsync())
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
                            PhotoSize = PhotoSize.Medium,

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
                if (await CheckStoragePermissionAsync() && await CheckMediaLibraryPermissionAsync())
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

        private async Task<bool> CheckMediaLibraryPermissionAsync()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();

            if (status == PermissionStatus.Granted)
                return true;
            else
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.MediaLibrary))
                {
                    await Shell.Current.DisplayAlert("갤러리 접근 권한 요청", "사진, 동영상을 갤러리에 저장하려면 갤러리 접근 권한을 허용해야 합니다.", "확인");
                }

                status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();

                if (status == PermissionStatus.Granted)
                    return true;
                else if (status != PermissionStatus.Unknown)
                {
                    await Shell.Current.DisplayAlert("갤러리 접근 거부", "갤러리 접근을 허용하지 않았습니다.", "확인");

                    // TODO: iOS는 한 번 거부한 권한을 iOS설정에서만 변경할 수 있습니다. iOS 설정에서 갤러리 접근을 허용하시겠습니까?

                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        CrossPermissions.Current.OpenAppSettings();
                        return false;
                    }
                    
                    return false;
                }
            }

            return false;
        }

        private async Task<bool> CheckStoragePermissionAsync()
        {
            var storagePermissionStatus = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();

            if (storagePermissionStatus == PermissionStatus.Granted)
                return true;
            else
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                {
                    await Shell.Current.DisplayAlert("저장소 접근 권한 요청", "사진, 동영상을 전송하려면 저장소 접근 권한을 허용해야 합니다.", "확인");
                }

                storagePermissionStatus = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();

                if (storagePermissionStatus == PermissionStatus.Granted)
                    return true;
                else if (storagePermissionStatus != PermissionStatus.Unknown)
                {
                    await Shell.Current.DisplayAlert("저장소 접근 거부", "저장소 접근을 허용하지 않았습니다.", "확인");

                    // TODO: iOS는 한 번 거부한 권한을 iOS설정에서만 변경할 수 있습니다. iOS 설정에서 저장소 접근을 허용하시겠습니까?
                    return false;
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
                        await Shell.Current.DisplayAlert("카메라 사용", "카메라 사용 권한을 허용하지 않았습니다. 한 번 거부한 권한은 iOS설정에서만 변경할 수 있습니다.\n\n확인을 누르면 iOS 설정으로 이동합니다.", "확인");
                        CrossPermissions.Current.OpenAppSettings();
                    }
                    else
                        await Shell.Current.DisplayAlert("카메라 사용 거부", "카메라 사용 권한을 허용하지 않았습니다.", "확인");
                }
            }

            return false;
        }
        private async Task<bool> CheckCameraAndStoragePermissionsAsync() => await CheckCameraPermissionAsync() && await CheckStoragePermissionAsync() ? true : false;
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
        public string TakingPhoto { get; } = "사진 찍기";
        public string AttachingPhoto { get; } = "사진 불러오기";
        public string TakingVideo { get; } = "비디오 찍기";
        public string AttachingVideo { get; } = "비디오 불러오기";
    }
    #endregion
}