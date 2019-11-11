using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Ringer.Models;
using Ringer.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

            app = Application.Current as App;
            
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
                await app.SignalR.SendMessageAsync(App.Group, App.User, TextToSend);

                TextToSend = string.Empty;
            }
            catch (Exception ex)
            {
                app.AddMessage($"vs.SendMessage:Send failed: {ex.Message}", App.User);
            }
            finally
            {
                isBusy = false;
            }
        }
        private async Task<bool> CheckPermissionAsync(Permission permission)
        {
            if (await CrossPermissions.Current.CheckPermissionStatusAsync(permission) == PermissionStatus.Granted)
                return true;
            else
            {
                var permissions = await CrossPermissions.Current.RequestPermissionsAsync(permission);

                if (permissions[permission] == PermissionStatus.Granted)
                    return true;
                else
                    return false;
            }
        }
        private async Task ProcessCameraAction(string action)
        {
            if (action == "설정 열기")
            {
                CrossPermissions.Current.OpenAppSettings();
            }

            if (action == CameraAction.TakingPhoto)
            {
                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await Shell.Current.DisplayAlert("카메라 사용 불가", "사용 가능한 카메라가 없습니다 :(", "확인");
                    return;
                }

                if (await CheckPermissionAsync(Permission.Camera) && await CheckPermissionAsync(Permission.Storage))
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
                else
                {

                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        await Shell.Current.DisplayAlert("권한이 없습니다.", "사진을 전송하려면 카메라, 저장소 접근 권한을 허용해야 합니다.", "확인");
                        CrossPermissions.Current.OpenAppSettings();

                    }
                    else
                        await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera);
                }
            }

            if (action == CameraAction.TakingVideo)
            {
                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakeVideoSupported)
                {
                    await Shell.Current.DisplayAlert("카메라 사용 불가", "사용 가능한 카메라가 없습니다 :(", "확인");
                    return;
                }

                if (await CheckPermissionAsync(Permission.Camera) && await CheckPermissionAsync(Permission.Storage))
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
                else
                {
                    await Shell.Current.DisplayAlert("권한이 없습니다.", "동영상을 전송하려면 카메라, 저장소 접근 권한을 허해야 합니다.", "확인");

                    if (Device.RuntimePlatform == Device.iOS)
                        CrossPermissions.Current.OpenAppSettings();
                }
            }

            if (action == CameraAction.AttachingPhoto)
            {
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await Shell.Current.DisplayAlert("사진 불러오기 실패", "사진 접근 권한이 없습니다 :(", "확인");
                    return;
                }

                if (await CheckPermissionAsync(Permission.Storage))
                {
                    var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                    {
                        PhotoSize = PhotoSize.Medium,

                    });

                    if (file == null)
                        return;

                    app.AddMessage($"{action}:{file.Path}", "camera");

                    file.Dispose();
                }
                else
                {

                    await Shell.Current.DisplayAlert("권한이 없습니다.", "사진을 불러오려면 저장소 접근을 허용해야 합니다.", "확인");

                    if (Device.RuntimePlatform == Device.iOS)
                        CrossPermissions.Current.OpenAppSettings();
                }
            }

            if (action == CameraAction.AttachingVideo)
            {
                // ios 13 에
                if (Device.RuntimePlatform == Device.iOS)
                    return;

                if (!CrossMedia.Current.IsPickVideoSupported)
                {
                    await Shell.Current.DisplayAlert("비디오 불러오기 실패", "비디오 접근 권한이 없습니다 :(", "확인");

                    return;
                }

                if (await CheckPermissionAsync(Permission.Storage))
                {
                    var file = await CrossMedia.Current.PickVideoAsync();

                    if (file == null)
                        return;

                    app.AddMessage($"{action}:{file.Path}", "camera");

                    file.Dispose();
                }
                else
                {
                    await Shell.Current.DisplayAlert("권한이 없습니다.", "동영상ㅇ 불러오려면 저장소 접근을 허용해야 합니다.", "확인");

                    if (Device.RuntimePlatform == Device.iOS)
                        CrossPermissions.Current.OpenAppSettings();
                }
            }
        }
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