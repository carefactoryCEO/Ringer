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
using Ringer.Core.Data;
using Ringer.Core.Models;
using Ringer.Helpers;
using Ringer.Models;
using Xamarin.Forms;
using XFDevice = Xamarin.Forms.Device;

namespace Ringer.ViewModels
{
    class ChatPageViewModel : INotifyPropertyChanged
    {
        #region private members
        MessagingService _messagingService;
        IMessageRepository _messageRepository;
        DateTime birthDate;
        UserInfoType userInfoToQuery = UserInfoType.None;
        GenderType genderType;
        #endregion

        public bool IsLoggedIn => App.Token != null;

        #region Constructor
        public ChatPageViewModel()
        {
            _messagingService = DependencyService.Resolve<MessagingService>();
            _messageRepository = DependencyService.Resolve<IMessageRepository>();

            SendMessageCommand = new Command(async () => await SendMessageAsync());
            GoBackCommand = new Command(async () => await Shell.Current.Navigation.PopAsync());
            ShowVidyoCommand = new Command(async () => await Shell.Current.GoToAsync("vidyopage"));
            CameraCommand = new Command<string>(async actionString => await ProcessCameraAction(actionString));

            // Initialize the properties for binding
            NavBarHeight = 0;
            Keyboard = Keyboard.Chat;

            //App.Token= null;
            ResetConnectionCommand = new Command(async () =>
            {
                // Reset Token
                App.Token = null;

                // Disconnect Connection
                if (_messagingService.IsConnected)
                    await _messagingService.DisconnectAsync(App.RoomName, App.UserName);

                // Clear Messages
                _messageRepository.Messages.Clear();

                // Go Back
                await Shell.Current.Navigation.PopAsync();
            });

        }

        public async Task CheckLogInAsync()
        {
            App.Trace(App.Token);
            App.Trace(App.UserName);

            if (!IsLoggedIn)
            {
                // await Task.Delay(1000);

                _messageRepository.AddLocalMessage(new Message { Content = "안녕하세요? 건강한 여행의 동반자 링거입니다.", Sender = Constants.System });
                // await Task.Delay(1500);
                _messageRepository.AddLocalMessage(new Message { Content = "정확한 상담을 위해 이름, 나이, 성별을 알려주세요.", Sender = Constants.System });
                // await Task.Delay(1500);
                _messageRepository.AddLocalMessage(new Message { Content = "한 번만 입력하면 다음부터는 링거 상담팀과 곧바로 대화할 수 있습니다. 정보 입력은 세 가지 질문에 답하는 형식으로 진행됩니다.", Sender = Constants.System });
                // await Task.Delay(2000);
                _messageRepository.AddLocalMessage(new Message { Content = "그럼 정보 입력을 시작하겠습니다.", Sender = Constants.System });
                // await Task.Delay(2500);
                _messageRepository.AddLocalMessage(new Message { Content = "이름을 입력하세요.", Sender = Constants.System });

                userInfoToQuery = UserInfoType.Name;
            }
            else
            {
                // TODO: Token이 valid한지 체크한다.



                await _messagingService.Init(Constants.HubUrl, App.Token);
                await _messagingService.ConnectAsync(App.RoomName, App.UserName);

                App.Trace(_messagingService.ConnectionId);
            }
        }
        #endregion

        #region Private Methods
        private async Task SendMessageAsync()
        {
            App.Trace(App.Token);
            App.Trace(App.UserName);

            if (string.IsNullOrEmpty(TextToSend))
                return;

            if (!IsLoggedIn)
            {
                switch (userInfoToQuery)
                {
                    case UserInfoType.Name:

                        App.UserName = TextToSend;
                        _messageRepository.AddLocalMessage(new Message { Content = TextToSend, Sender = App.UserName });
                        // TODO: name validation here

                        // name validation pass
                        TextToSend = string.Empty;
                        // await Task.Delay(1000);

                        _messageRepository.AddLocalMessage(new Message { Content = "생년월일 6자리와, 주민등록번호 뒷자리 1개를 입력해주세요.", Sender = Constants.System });
                        // await Task.Delay(600);

                        Keyboard = Keyboard.Numeric;
                        _messageRepository.AddLocalMessage(new Message { Content = "예를 들어 1999년 3월 20일에 태어난 여자라면 993202라고 입력하시면 됩니다.", Sender = Constants.System });

                        userInfoToQuery = UserInfoType.BirthDate;
                        break;

                    case UserInfoType.BirthDate:

                        var numeric = TextToSend;
                        TextToSend = string.Empty;

                        string year = numeric.Substring(0, 2);
                        string month = numeric.Substring(2, 2);
                        string day = numeric.Substring(4, 2);
                        string gender = numeric.Substring(6, 1);

                        year = (int.Parse(gender) < 3) ? "19" + year : "20" + year;

                        birthDate = DateTime.Parse($"{year}-{month}-{day}");
                        genderType = int.Parse(gender) % 2 == 0 ? GenderType.Female : GenderType.Male;

                        _messageRepository.AddLocalMessage(new Message { Content = $"{year}년 {month}월 {day}일 {genderType}", Sender = App.UserName });

                        // await Task.Delay(500);

                        Keyboard = Keyboard.Chat;

                        _messageRepository.AddLocalMessage(new Message { Content = "조회 중입니다. 잠시만 기다려주세요.", Sender = Constants.System });

                        // await Task.Delay(500);

                        // TODO: Get Token!
                        HttpClient client = new HttpClient();

                        /*
                         *  public class LoginInfo
                            {
                                public string LoginType { get; set; }

                                public string Email { get; set; }
                                public string Password { get; set; }

                                public string Name { get; set; }
                                public DateTime BirthDate { get; set; }
                                public GenderType Gender { get; set; }

                                public DeviceType DeviceType { get; set; }
                                public string DeviceId { get; set; }
                            }
                         */

                        var loginInfo = JsonSerializer.Serialize(new LoginInfo
                        {
                            Name = App.UserName,
                            BirthDate = birthDate,
                            Gender = genderType,
                            DeviceId = App.DeviceId,
                            DeviceType = XFDevice.RuntimePlatform == XFDevice.iOS ? DeviceType.iOS : DeviceType.Android
                        });

                        // get Token
                        HttpResponseMessage response = await client.PostAsync(Constants.TokenUrl, new StringContent(loginInfo, Encoding.UTF8, "application/json"));

                        /**
                         * 
                         * 서버에서 일치 정보 찾음
                         * response.StatusCode : System.Net.HttpStatusCode.OK
                         * response
                         * {
                         *      StatusCode: 200, 
                         *      ReasonPhrase: 'OK', 
                         *      Version: 1.1, 
                         *      Content: System.Net.Http.NSUrlSessionHandler+NSUrlSessionDataTaskStreamContent, 
                         *      Headers:{Transfer-Encoding: IdentityServer: KestrelDate: Tue, 26 Nov 2019 12:35:50 GMTContent-Type: text/plain; charset=utf-8}
                         *  }
                         *  
                         *  
                         * 서버에 일치 정보 없음
                         * response.StatusCode : System.Net.HttpStatusCode.NotFound
                         * response
                         * {
                         *      StatusCode: 404, 
                         *      ReasonPhrase: 'Not Found', 
                         *      Version: 1.1, 
                         *      Content: System.Net.Http.NSUrlSessionHandler+NSUrlSessionDataTaskStreamContent, 
                         *      Headers:{Transfer-Encoding: IdentityServer: KestrelDate: 2019-11-26 오후 12:25:53 +00:00Content-Type: text/plain; charset=utf-8}
                         *  }
                         *  
                         **/

                        var token = await response.Content.ReadAsStringAsync();

                        // TODO: token 발급되었는지 확인
                        // TODO: token 발급되지 않았으면 처음부터 다시? 손쉽게 오타 부분만 고칠 수 있는 UI 제공

                        App.Token = token;

                        //messagingService.AddLocalMessage($"로그인토큰: {App.Token}", Constants.System);


                        // Messaging Service Initialize
                        // 
                        await _messagingService.Init(Constants.HubUrl, App.Token);
                        await _messagingService.ConnectAsync(App.RoomName, App.UserName);

                        break;

                    default:
                        break;
                }

                if (IsLoggedIn && _messagingService.IsConnected)
                {
                    //messagingService.AddLocalMessage($"커넥션 id: {messagingService?.HubConnection?.ConnectionId}", Constants.System);

                    _messageRepository.AddLocalMessage(new Message { Content = $"{App.UserName}님 확인되었습니다. 이제 링거 상담팀과 대화하실 수 있습니다.", Sender = Constants.System });
                    //// await Task.Delay(2000);

                    //messagingService.AddLocalMessage($"AA가 궁금하면 aa를 BB가 궁금하면 bb를 채팅창에 입력하세요. 링거 데이터베이스에 저장된 정보를 바로 알려드리고, 상담팀이 확인한 후 더 자세히 알려드리겠습니다.", Constants.System);
                }

                return;
            }


            try
            {
                await _messagingService.SendMessageToRoomAsync(App.RoomName, App.UserName, TextToSend);

                TextToSend = string.Empty;
            }
            catch (Exception ex)
            {
                _messageRepository.AddLocalMessage(new Message { Content = $"vs.SendMessage:Send failed: {ex.Message}", Sender = Constants.System });
            }
        }

        private async Task ProcessCameraAction(string action)
        {
            if (action == "설정 열기")
            {
                CrossPermissions.Current.OpenAppSettings();
            }

            #region taking photo
            if (action == Constants.TakingPhoto)
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

                        await _messagingService.SendMessageToRoomAsync(App.RoomName, App.UserName, $"{action}:{file.Path}");

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
            if (action == Constants.TakingVideo)
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

                        await _messagingService.SendMessageToRoomAsync(App.RoomName, App.UserName, $"{action}:{file.Path}");

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
            if (action == Constants.AttachingPhoto)
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

                        await _messagingService.SendMessageToRoomAsync(App.UserName, App.UserName, $"{action}:{file.Path}");

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
            if (action == Constants.AttachingVideo)
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

                        await _messagingService.SendMessageToRoomAsync(App.RoomName, App.UserName, $"{action}:{file.Path}");

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
                    if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
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
                    if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
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
            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
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
                    if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
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
            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
                return await CheckCameraPermissionAsync() && await CheckPhotosPermissionsAsync();

            else if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
                return await CheckCameraPermissionAsync() && await CheckStoragePermissionAsync();

            else
                return false;
        }
        private async Task<bool> TakingVideoPermittedAsync()
        {
            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
                return await CheckCameraPermissionAsync() && await CheckPhotosPermissionsAsync() && await CheckMicPermissionAsync();

            else if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
                return await CheckCameraPermissionAsync() && await CheckStoragePermissionAsync();

            else
                return false;
        }
        #endregion

        #region Public Properties
        public string TextToSend { get; set; }
        public Keyboard Keyboard { get; set; }
        public double NavBarHeight { get; set; }
        public string NavBarTitle => "링거 상담실";
        public ObservableCollection<Message> Messages => _messageRepository.Messages;
        #endregion

        #region public Commands
        public ICommand SendMessageCommand { get; }
        public ICommand CameraCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand ShowVidyoCommand { get; }
        public ICommand ResetConnectionCommand { get; }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}