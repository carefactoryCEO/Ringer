using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Ringer.Core;
using Ringer.Core.Data;
using Ringer.Helpers;
using Ringer.Models;
using Ringer.Services;
using Ringer.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    class ChatPageViewModel : INotifyPropertyChanged
    {
        #region private members
        private readonly IMessagingService _messagingService;
        private readonly IMessageRepository _messageRepository;
        private readonly IRESTService _restService;
        private readonly ILocalDbService _localDbService;

        DateTime birthDate;
        UserInfoType userInfoToQuery = UserInfoType.None;
        GenderType genderType;
        #endregion

        #region Constructor
        public ChatPageViewModel()
        {
            Debug.WriteLine("---------------ChatPageViewModel Ctor called--------------------");

            _messagingService = DependencyService.Resolve<IMessagingService>();
            _messageRepository = DependencyService.Resolve<IMessageRepository>();
            _restService = DependencyService.Resolve<IRESTService>();
            _localDbService = DependencyService.Resolve<ILocalDbService>();

            Messages = _messageRepository.Messages;

            SendMessageCommand = new Command(async () => await SendMessageAsync());
            GoBackCommand = new Command(async () => await Shell.Current.Navigation.PopAsync());
            //ShowVidyoCommand = new Command(async () => await Shell.Current.GoToAsync("vidyopage?vidyoRoom=fd17626e-29c8-4e2f-a5bb-3215ffe8e61a"));
            ShowVidyoCommand = new Command(async () => await Browser.OpenAsync("https://appr.tc/r/5433210"));


            CameraCommand = new Command<string>(async actionString => await ProcessCameraAction(actionString));

            // Initialize the properties for binding
            NavBarHeight = 0;
            Keyboard = Keyboard.Chat;

            //App.Token= null;
            ResetConnectionCommand = new Command(async () =>
            {
                // Reset Token
                App.Token = null;
                App.CurrentRoomId = null;
                App.UserName = null;
                App.LastServerMessageId = 0;
                userInfoToQuery = UserInfoType.None;

                // Disconnect Connection
                await _messagingService.DisconnectAsync(App.CurrentRoomId, App.UserName);

                // Clear Messages
                _messageRepository.Messages.Clear();

                // reset local db's Message table
                _messageRepository.ClearLocalDb();

                // Go Back
                await Shell.Current.Navigation.PopAsync();
            });
        }

        public async Task ExcuteLogInProcessAsync()
        {
            if (App.IsLoggedIn)
                return;

            switch (userInfoToQuery)
            {
                case UserInfoType.None:

                    // await Task.Delay(1000);
                    _messageRepository.AddLocalMessage(new Message { Body = "안녕하세요? 건강한 여행의 동반자 링거입니다.", Sender = Constants.System });
                    // await Task.Delay(1500);
                    _messageRepository.AddLocalMessage(new Message { Body = "정확한 상담을 위해 이름, 나이, 성별을 알려주세요.", Sender = Constants.System });
                    // await Task.Delay(1500);
                    _messageRepository.AddLocalMessage(new Message { Body = "한 번만 입력하면 다음부터는 링거 상담팀과 곧바로 대화할 수 있습니다. 정보 입력은 세 가지 질문에 답하는 형식으로 진행됩니다.", Sender = Constants.System });
                    // await Task.Delay(2000);
                    _messageRepository.AddLocalMessage(new Message { Body = "그럼 정보 입력을 시작하겠습니다.", Sender = Constants.System });
                    // await Task.Delay(2500);
                    _messageRepository.AddLocalMessage(new Message { Body = "이름을 입력하세요.", Sender = Constants.System });

                    userInfoToQuery = UserInfoType.Name;

                    break;

                case UserInfoType.Name:

                    App.UserName = TextToSend;
                    _messageRepository.AddLocalMessage(new Message { Body = TextToSend, Sender = App.UserName });
                    TextToSend = string.Empty;
                    // TODO: name validation here

                    // await Task.Delay(1000);

                    // name validation pass
                    _messageRepository.AddLocalMessage(new Message { Body = "생년월일 6자리와, 주민등록번호 뒷자리 1개를 입력해주세요.", Sender = Constants.System });
                    // await Task.Delay(600);

                    Keyboard = Keyboard.Numeric;
                    _messageRepository.AddLocalMessage(new Message { Body = "예를 들어 1999년 3월 20일에 태어난 여자라면 993202라고 입력하시면 됩니다.", Sender = Constants.System });

                    userInfoToQuery = UserInfoType.BirthDate;
                    break;

                case UserInfoType.BirthDate:

                    var numeric = TextToSend;
                    TextToSend = string.Empty;

                    // process gender
                    string gender = numeric.Substring(6, 1);
                    if (int.TryParse(gender, out var parsedGenderInt))
                        Debug.WriteLine(parsedGenderInt % 2 == 0 ? GenderType.Female : GenderType.Male);
                    else
                        Debug.WriteLine("Wrong Gender format");

                    genderType = parsedGenderInt % 2 == 0 ? GenderType.Female : GenderType.Male;

                    // process date of birth
                    string year = numeric.Substring(0, 2);
                    string month = numeric.Substring(2, 2);
                    string day = numeric.Substring(4, 2);

                    year = (parsedGenderInt < 3) ? "19" + year : "20" + year;

                    if (DateTime.TryParse($"{year}-{month}-{day}", out var parsedBirthdate))
                        Debug.WriteLine(parsedBirthdate);
                    else
                        Debug.WriteLine("Wrong DateTime format");


                    //year = (int.Parse(gender) < 3) ? "19" + year : "20" + year;
                    //birthDate = DateTime.Parse($"{year}-{month}-{day}");
                    //genderType = int.Parse(gender) % 2 == 0 ? GenderType.Female : GenderType.Male;

                    birthDate = parsedBirthdate;

                    _messageRepository.AddLocalMessage(new Message { Body = $"{year}년 {month}월 {day}일 {genderType}", Sender = App.UserName });

                    // await Task.Delay(500);

                    Keyboard = Keyboard.Chat;

                    _messageRepository.AddLocalMessage(new Message { Body = "조회 중입니다. 잠시만 기다려주세요.", Sender = Constants.System });

                    // Log in and get Token
                    // TODO: Add user's Location data to validate ticket
                    await _restService.LogInAsync(App.UserName, birthDate, genderType);

                    if (App.IsLoggedIn)
                    {

                        // TODO: _messageRepository.LoadMessageAsync, _messagingService.ConnectAsync를 await WhenAll()로 처리한다. 
                        Debug.WriteLine("--------------------------LoadMessage------------------------------");

                        _messageRepository.Messages.Clear();
                        await _messageRepository.LoadMessagesAsync(true).ConfigureAwait(false);

                        Messages = _messageRepository.Messages;

                        Debug.WriteLine("--------------------------LoadMessage Finished------------------------------");
                        Debug.WriteLine("--------------------------Connect------------------------------");


                        _messagingService.Init(Constants.HubUrl, App.Token);
                        await _messagingService.ConnectAsync().ConfigureAwait(false);//ExcuteLoginAsync 2초 정도 시간이 걸린다...

                        Debug.WriteLine("--------------------------Connect Finished------------------------------");
                    }

                    break;
            }
        }
        #endregion

        #region Private Methods
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(TextToSend))
                return;

            if (!App.IsLoggedIn)
            {
                await ExcuteLogInProcessAsync();
                return;
            }

            var message = new Message
            {
                RoomId = App.CurrentRoomId,
                Body = TextToSend,
                Sender = App.UserName,
                SenderId = App.UserId,
                CreatedAt = DateTime.UtcNow,
                ReceivedAt = DateTime.UtcNow
            };

            try
            {
                // local Db 저장
                var localId = await _localDbService.SaveMessageAsync(message);
                var localMessage = await _localDbService.GetMessageAsync(localId);

                // view에 표시
                _messageRepository.AddLocalMessage(localMessage);

                // send to hub
                var remoteId = await _messagingService.SendMessageToRoomAsync(App.CurrentRoomId, App.UserName, TextToSend);

                localMessage.ServerId = remoteId;

                await _localDbService.SaveMessageAsync(message: localMessage, update: true);

                // reset text
                TextToSend = string.Empty;
            }
            catch (Exception ex)
            {
                _messageRepository.AddLocalMessage(new Message { Body = $"vs.SendMessage:Send failed: {ex.Message}", Sender = Constants.System });
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

                        await _messagingService.SendMessageToRoomAsync(App.CurrentRoomId, App.UserName, $"{action}:{file.Path}");

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

                        await _messagingService.SendMessageToRoomAsync(App.CurrentRoomId, App.UserName, $"{action}:{file.Path}");

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

                        await _messagingService.SendMessageToRoomAsync(App.CurrentRoomId, App.UserName, $"{action}:{file.Path}");

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

                        await _messagingService.SendMessageToRoomAsync(App.CurrentRoomId, App.UserName, $"{action}:{file.Path}");

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

            if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                return true;
            else
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Photos))
                {
                    await Shell.Current.DisplayAlert("사진 접근 권한 요청", "사진, 동영상을 촬영하려면 사진 접근 권한을 허용해야 합니다.", "확인");
                }

                status = await CrossPermissions.Current.RequestPermissionAsync<PhotosPermission>();

                if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                    return true;
                else if (status != Plugin.Permissions.Abstractions.PermissionStatus.Unknown)
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

            if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                return true;
            else
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                    await Shell.Current.DisplayAlert("저장소 접근 권한 요청", "사진, 동영상을 저장하고 불러오려면 저장소 권한을 허용해야 합니다.", "확인");

                status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();

                if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
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

            if (cameraPermissionStatus == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                return true;
            else
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Camera))
                {
                    await Shell.Current.DisplayAlert("카메라 사용 권한 요청", "사진, 동영상을 촬영하려면 카메라 사용 권한을 허용해야 합니다.", "확인");
                }

                cameraPermissionStatus = await CrossPermissions.Current.RequestPermissionAsync<CameraPermission>();

                if (cameraPermissionStatus == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                    return true;
                else if (cameraPermissionStatus != Plugin.Permissions.Abstractions.PermissionStatus.Unknown)
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

            if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
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

                if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                    return true;
                else if (status != Plugin.Permissions.Abstractions.PermissionStatus.Unknown)
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
            if (Device.RuntimePlatform == Device.iOS)
                return await CheckCameraPermissionAsync() && await CheckPhotosPermissionsAsync();

            if (Device.RuntimePlatform == Device.Android)
                return await CheckCameraPermissionAsync() && await CheckStoragePermissionAsync();

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
        public string NavBarTitle => App.IsLoggedIn ? App.UserName : "링거 상담실";
        public ObservableCollection<Message> Messages { get; set; }
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