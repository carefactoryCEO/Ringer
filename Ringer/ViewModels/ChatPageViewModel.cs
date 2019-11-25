using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Ringer.Core;
using Ringer.Core.Models;
using Ringer.Helpers;
using Ringer.Models;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    class ChatPageViewModel : INotifyPropertyChanged
    {
        #region private members
        MessagingService messagingService;
        string room = "Xamarin";
        string name;
        DateTime birthDate;
        UserInfoType userInfoToQuery = UserInfoType.None;
        GenderType gender;
        #endregion

        #region Constructor
        public ChatPageViewModel()
        {
            messagingService = DependencyService.Resolve<MessagingService>();

            SendMessageCommand = new Command(async () => await SendMessageAsync());

            GoBackCommand = new Command(async () => await Shell.Current.Navigation.PopAsync());
            ShowVidyoCommand = new Command(async () => await Shell.Current.GoToAsync("vidyopage"));
            CameraCommand = new Command<string>(async actionString => await ProcessCameraAction(actionString));

            //Settings.Token = null;
        }

        public async Task CheckLogInAsync()
        {
            if (!Settings.IsLoggedIn)
            {
                await Task.Delay(2000);

                messagingService.AddLocalMessage("안녕하세요? 건강한 여행의 동반자 링거입니다.", Settings.System);
                await Task.Delay(1500);
                messagingService.AddLocalMessage("정확한 상담을 위해 이름, 나이, 성별을 알려주세요.", Settings.System);
                await Task.Delay(1500);
                messagingService.AddLocalMessage("한 번만 입력하면 다음부터는 링거 상담팀과 곧바로 대화할 수 있습니다. 정보 입력은 세 가지 질문에 답하는 형식으로 진행됩니다.", Settings.System);
                await Task.Delay(2000);
                messagingService.AddLocalMessage("그럼 정보 입력을 시작하겠습니다.", Settings.System);
                await Task.Delay(2500);
                messagingService.AddLocalMessage("이름을 입력하세요.", Settings.System);

                userInfoToQuery = UserInfoType.Name;
            }
            else
            {
                messagingService.Init(Settings.HubUrl, Settings.Token);
                await messagingService.ConnectAsync();
                // TODO: Room name 로직 확정
                await messagingService.JoinRoomAsync(room, Settings.Name);

                Debug.WriteLine(messagingService.HubConnection.ConnectionId);
            }
        }
        #endregion

        #region Private Methods
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(TextToSend))
                return;

            if (!Settings.IsLoggedIn)
            {
                switch (userInfoToQuery)
                {
                    case UserInfoType.Name:

                        Settings.Name = TextToSend;
                        messagingService.AddLocalMessage(TextToSend, Settings.Name);
                        // TODO: name validation here

                        // name validation pass
                        TextToSend = string.Empty;
                        await Task.Delay(2000);

                        messagingService.AddLocalMessage("생년월일을 yy-mm-dd 형식으로 입력하세요. 예를들어 1995년 3월 15일이 생일이라면 95-03-15라고 입력하세요.", Settings.System);
                        await Task.Delay(2000);
                        messagingService.AddLocalMessage("여행사를 통해 링거에 가입할 때 입력한 것과 일치해야 합니다. 여권, 주민등록증에 기재된 생년월일을 입력하시는 게 가장 좋습니다.", Settings.System);
                        userInfoToQuery = UserInfoType.BirthDate;
                        break;

                    case UserInfoType.BirthDate:

                        messagingService.AddLocalMessage(TextToSend, Settings.Name);
                        // TODO: birthDate validation (format, range)

                        // birthDate validation pass
                        birthDate = DateTime.Parse(TextToSend);
                        TextToSend = string.Empty;
                        await Task.Delay(1600);

                        messagingService.AddLocalMessage("성별을 입력하세요. 여자는 여자, 남자는 남자라고 쓰시면 됩니다.", Settings.System);
                        userInfoToQuery = UserInfoType.Gender;
                        break;

                    case UserInfoType.Gender:

                        messagingService.AddLocalMessage(TextToSend, Settings.Name);

                        // TODO: Gender validation

                        // Gender validation pass
                        gender = TextToSend == "여자" ? GenderType.Female : GenderType.Male;
                        TextToSend = string.Empty;

                        await Task.Delay(1400);

                        messagingService.AddLocalMessage("조회 중입니다. 잠시만 기다려주세요.", Settings.System);

                        // TODO: Get Token!
                        HttpClient client = new HttpClient();

                        var loginInfo = JsonSerializer.Serialize(new LoginInfo
                        {
                            Name = Settings.Name,
                            BirthDate = birthDate,
                            Gender = gender,
                        });

                        // get Token
                        HttpResponseMessage response = await client.PostAsync(Settings.TokenUrl, new StringContent(loginInfo, Encoding.UTF8, "application/json"));
                        var token = await response.Content.ReadAsStringAsync();

                        // TODO: token 발급되었는지 확인
                        // TODO: token 발급되지 않았으면 처음부터 다시? 손쉽게 오타 부분만 고칠 수 있는 UI 제공

                        Settings.Token = token;

                        //messagingService.AddLocalMessage($"로그인토큰: {Settings.Token}", Settings.System);


                        // Messaging Service Initialize
                        messagingService.Init(Settings.HubUrl, Settings.Token);
                        await messagingService.ConnectAsync();
                        await messagingService.JoinRoomAsync(room, Settings.Name);

                        break;

                    default:
                        break;
                }

                if (Settings.IsLoggedIn && messagingService.IsConnected)
                {
                    //messagingService.AddLocalMessage($"커넥션 id: {messagingService?.HubConnection?.ConnectionId}", Settings.System);

                    messagingService.AddLocalMessage($"{Settings.Name}님 확인되었습니다. 이제 링거 상담팀과 대화하실 수 있습니다.", Settings.System);
                    await Task.Delay(2000);

                    messagingService.AddLocalMessage($"AA가 궁금하면 aa를 BB가 궁금하면 bb를 채팅창에 입력하세요. 링거 데이터베이스에 저장된 정보를 바로 알려드리고, 상담팀이 확인한 후 더 자세히 알려드리겠습니다.", Settings.System);
                }

                return;
            }


            try
            {
                await messagingService.SendMessageToRoomAsync(room, Settings.Name, TextToSend);

                TextToSend = string.Empty;
            }
            catch (Exception ex)
            {
                messagingService.AddLocalMessage($"vs.SendMessage:Send failed: {ex.Message}", Settings.System);
            }
        }

        private async Task ProcessCameraAction(string action)
        {
            if (action == "설정 열기")
            {
                CrossPermissions.Current.OpenAppSettings();
            }

            #region taking photo
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

                        await messagingService.SendMessageToRoomAsync(room, name, $"{action}:{file.Path}");

                        file.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);

                    }
                }
            }
            #endregion

            #region taking video
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

                        await messagingService.SendMessageToRoomAsync(room, name, $"{action}:{file.Path}");

                        file.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            #endregion

            #region attaching photo
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

                        await messagingService.SendMessageToRoomAsync(room, name, $"{action}:{file.Path}");

                        file.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            #endregion

            #region attaching video            
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

                        await messagingService.SendMessageToRoomAsync(room, name, $"{action}:{file.Path}");

                        file.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            #endregion
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
        private async Task<bool> CheckStoragePermissionAsync()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();

            if (status == PermissionStatus.Granted)
                return true;
            else
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                    await Shell.Current.DisplayAlert("저장소 접근 권한 요청", "사진, 동영상을 저장하고 불러오려면 저장소 권한을 허용해야 합니다.", "확인");

                status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();

                if (status == PermissionStatus.Granted)
                    return true;

                return false;
            }
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
        private async Task<bool> TakingPhotoPermittedAsync()
        {
            if (Device.RuntimePlatform == Device.iOS)
                return await CheckCameraPermissionAsync() && await CheckPhotosPermissionsAsync();

            else if (Device.RuntimePlatform == Device.Android)
                return await CheckCameraPermissionAsync() && await CheckStoragePermissionAsync();

            else
                return false;
        }
        private async Task<bool> TakingVideoPermittedAsync()
        {
            if (Device.RuntimePlatform == Device.iOS)
                return await CheckCameraPermissionAsync() && await CheckPhotosPermissionsAsync() && await CheckMicPermissionAsync();

            else if (Device.RuntimePlatform == Device.Android)
                return await CheckCameraPermissionAsync() && await CheckStoragePermissionAsync();

            else
                return false;
        }
        #endregion

        #region Public Properties
        public string TextToSend { get; set; }
        public CameraAction CameraAction { get; } = new CameraAction();
        public double NavBarHeight { get; set; } = 0;
        public string NavBarTitle => "링거 상담실";
        public ObservableCollection<Message> Messages => messagingService.Messages;
        #endregion

        #region public Commands
        public ICommand SendMessageCommand { get; }
        public ICommand CameraCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand ShowVidyoCommand { get; }
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