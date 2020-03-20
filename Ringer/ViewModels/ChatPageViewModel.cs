using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Ringer.Core;
using Ringer.Core.Data;
using Ringer.Helpers;
using Ringer.Models;
using Ringer.Services;
using Ringer.Types;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    class ChatPageViewModel : INotifyPropertyChanged
    {
        #region private members
        private readonly IMessaging _messaging;
        private readonly IRESTService _restService;
        private readonly BlobContainerClient _blobContainer;
        private UserInfoType _userInfoToQuery = UserInfoType.None;
        private DateTime _birthDate;
        private GenderType _genderType;
        #endregion

        #region Public Properties
        public ObservableCollection<MessageModel> Messages { get; set; }
        public bool IsBusy { get; set; }
        public string TextToSend { get; set; }
        public Keyboard Keyboard { get; set; } = Keyboard.Chat;
        public double NavBarHeight { get; set; } = 0;
        public Thickness BottomPadding { get; set; }
        public string NavBarTitle => App.IsLoggedIn ? App.UserName : "링거 상담실";
        #endregion

        #region Constructor
        public ChatPageViewModel()
        {
            _blobContainer = new BlobContainerClient(Constants.BlobStorageConnectionString, Constants.BlobContainerName);

            _messaging = DependencyService.Resolve<IMessaging>();
            _restService = DependencyService.Resolve<IRESTService>();

            _messaging.MessageAdded += MessageRepository_MessageAdded;
            _messaging.MessageUpdated += MessageRepository_MessageUpdated;
            _messaging.FetchingStateChanged += Messaging_FetchingStateChanged;
            _messaging.MessagesFetched += _messaging_MessagesFetched;

            SendMessageCommand = new Command(async () => await SendMessageAsync());
            TakingPhotoCommand = new Command(async () => await TakePhotoAsync());
            TakingVideoCommand = new Command(async () => await TakeVideoAsync());
            GalleryPhotoCommand = new Command(async () => await GalleryPhotoAsync());
            GalleryVideoCommand = new Command(async () => await GalleryVideoAsync());
            ResetConnectionCommand = new Command(async () => await ResetConnection());
            LoadBufferCommand = new Command(() => LoadBufferMessages());
            RefreshCommand = new Command(() =>
            {
                Messages = new ObservableCollection<MessageModel>(_messaging.Messages);
                MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());
            });

            Messages = new ObservableCollection<MessageModel>(_messaging.Messages);

            _messaging.BufferMessages();
        }

        public async Task EnsureMessageLoaded()
        {
            if (Messages.Any())
                return;

            await _messaging.InitMessagesAsync();
            Messages = new ObservableCollection<MessageModel>(_messaging.Messages);
        }

        private void _messaging_MessagesFetched(object sender, MessageModel[] fetchedMessages)
        {
            if (fetchedMessages.Any())
            {
                foreach (var message in fetchedMessages)
                    Messages.Add(message);

                MessagingCenter.Send(this, "MessageAdded", (object)fetchedMessages.Last());
            }
        }

        private void Messaging_FetchingStateChanged(object sender, FetchingState state)
        {
            IsBusy = (state == FetchingState.Fetching) ? true : false;
        }
        #endregion

        #region Private Methods
        private void LoadBufferMessages()
        {
            IsBusy = true;

            if (Messages.Count == _messaging.Messages.Count)
            {
                IsBusy = false;
                return;
            }

            var target = Messages.Cast<object>().First();

            Messages = new ObservableCollection<MessageModel>(_messaging.Messages);

            MessagingCenter.Send(this, "MessageLoaded", target);

            IsBusy = false;

            _messaging.BufferMessages();
        }
        private async Task SendMessageAsync()
        {
            if (string.IsNullOrEmpty(TextToSend))
                return;

            if (!App.IsLoggedIn)
            {
                await ExcuteLogInProcessAsync();
                return;
            }

            var message = new MessageModel
            {
                RoomId = App.RoomId,
                ServerId = -1,
                Body = TextToSend,
                Sender = App.UserName,
                SenderId = App.UserId,
                CreatedAt = DateTime.UtcNow,
                ReceivedAt = DateTime.UtcNow,
                MessageTypes = MessageTypes.Leading | MessageTypes.Trailing | MessageTypes.Text | MessageTypes.Outgoing
            };

            // reset text
            TextToSend = string.Empty;

            // 저장
            await _messaging.AddMessageAsync(message).ConfigureAwait(false);

            // 전송
            await _messaging.SendMessageToRoomAsync(message.RoomId, message.Sender, message.Body).ConfigureAwait(false);
        }
        private async Task ResetConnection()
        {
            // Reset Token
            App.Token = null;
            App.RoomId = null;
            App.UserName = null;
            App.LastServerMessageId = 0;
            _userInfoToQuery = UserInfoType.None;

            // Disconnect Connection
            await _messaging.DisconnectAsync();

            Messages.Clear();

            // reset local db's Message table
            _messaging.ClearLocalDb();

            // Go Back
            await Shell.Current.Navigation.PopAsync();
        }
        private void MessageRepository_MessageUpdated(object sender, MessageModel updatedMessage)
        {
            var targetMessage = Messages.LastOrDefault(t => t.Id == updatedMessage.Id);

            if (targetMessage != null)
            {
                targetMessage.MessageTypes = updatedMessage.MessageTypes;
                MessagingCenter.Send(this, "MessageAdded", (object)targetMessage);
            }
        }
        private void MessageRepository_MessageAdded(object sender, MessageModel newMessage)
        {
            Messages.Add(newMessage);
            MessagingCenter.Send(this, "MessageAdded", (object)newMessage);
        }
        #region camera actions
        private async Task TakePhotoAsync()
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
                    App.IsCameraActivated = true;
                    // Taking picture
                    using MediaFile mediaFile = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                    {
                        Directory = "RingerPhoto",
                        SaveToAlbum = true,
                        SaveMetaData = true,
                        RotateImage = true,
                        CompressionQuality = 75,
                        CustomPhotoSize = 50,
                        PhotoSize = PhotoSize.MaxWidthHeight,
                        MaxWidthHeight = 1000,
                        DefaultCamera = CameraDevice.Rear,
                        AllowCropping = false
                    });

                    if (mediaFile == null)
                        return;

                    IsBusy = true;

                    var fileName = $"image-{App.UserId}-{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")}.jpg";
                    BlobClient blobClient = _blobContainer.GetBlobClient(fileName);

                    var message = new MessageModel
                    {
                        RoomId = App.RoomId,
                        ServerId = -1,
                        Body = blobClient.Uri.ToString(),
                        Sender = App.UserName,
                        SenderId = App.UserId,
                        CreatedAt = DateTime.UtcNow,
                        ReceivedAt = DateTime.UtcNow,
                        MessageTypes = MessageTypes.Outgoing | MessageTypes.Image | MessageTypes.Leading | MessageTypes.Trailing,
                    };

                    await Task.WhenAll(new Task[]
                    {
                            // Upload to azure blob storage
                            blobClient.UploadAsync(mediaFile.GetStream(), httpHeaders: new BlobHttpHeaders { ContentType = "image/jpeg" }),
                            // Display image message to view locally
                            _messaging.AddMessageAsync(message)
                    }).ConfigureAwait(false);

                    //mediaFile.Dispose();

                    IsBusy = false;

                    // Send image message to server
                    await _messaging.SendMessageToRoomAsync(message.RoomId, message.Sender, message.Body).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    MessagingCenter.Send(this, "CameraActionCompleted", "completed");
                    App.IsCameraActivated = false;
                }
            }
        }
        private async Task GalleryPhotoAsync()
        {
            if (AttachingPhotoPermitted())
            {
                try
                {
                    App.IsCameraActivated = true;

                    if (!CrossMedia.Current.IsPickPhotoSupported)
                    {
                        await Shell.Current.DisplayAlert("사진 불러오기 실패", "사진 불러오기가 지원되지 않는 기기입니다. :(", "확인");
                        return;
                    }

                    var mediaFile = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                    {
                        PhotoSize = PhotoSize.Medium
                    });


                    if (mediaFile == null)
                        return;

                    IsBusy = true;

                    var fileName = $"image-{App.UserId}-{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")}.jpg";
                    BlobClient blobClient = _blobContainer.GetBlobClient(fileName);

                    var message = new MessageModel
                    {
                        RoomId = App.RoomId,
                        ServerId = -1,
                        Body = blobClient.Uri.ToString(),
                        Sender = App.UserName,
                        SenderId = App.UserId,
                        CreatedAt = DateTime.UtcNow,
                        ReceivedAt = DateTime.UtcNow,
                        MessageTypes = MessageTypes.Outgoing | MessageTypes.Image | MessageTypes.Leading | MessageTypes.Trailing,
                    };

                    await Task.WhenAll(new Task[]
                    {
                            // upload to azure blob storage
                            blobClient.UploadAsync(mediaFile.GetStream(), httpHeaders: new BlobHttpHeaders { ContentType = "image/jpeg" }),
                            // save message locally
                            _messaging.AddMessageAsync(message)

                    }).ConfigureAwait(false);

                    mediaFile.Dispose();

                    IsBusy = false;
                    // send image message
                    await _messaging.SendMessageToRoomAsync(message.RoomId, message.Sender, message.Body).ConfigureAwait(false);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    MessagingCenter.Send(this, "CameraActionCompleted", "completed");
                    App.IsCameraActivated = false;
                }
            }
        }
        private async Task TakeVideoAsync()
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
                    App.IsCameraActivated = true;
                    // Taking Video
                    MediaFile mediaFile = await CrossMedia.Current.TakeVideoAsync(new StoreVideoOptions
                    {
                        DesiredLength = TimeSpan.FromMinutes(2.0d),
                        DefaultCamera = CameraDevice.Rear,
                        Directory = "RingerVideo",
                        SaveToAlbum = true,
                        Quality = VideoQuality.Medium
                    }); ;

                    if (mediaFile == null)
                        return;

                    IsBusy = true;

                    var fileName = $"video-{App.UserId}-{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")}.mp4";
                    BlobClient blobClient = _blobContainer.GetBlobClient(fileName);
                    var message = new MessageModel
                    {
                        RoomId = App.RoomId,
                        ServerId = -1,
                        Body = blobClient.Uri.ToString(),
                        Sender = App.UserName,
                        SenderId = App.UserId,
                        CreatedAt = DateTime.UtcNow,
                        ReceivedAt = DateTime.UtcNow,
                        MessageTypes = MessageTypes.Outgoing | MessageTypes.Video | MessageTypes.Leading | MessageTypes.Trailing,
                    };

                    await Task.WhenAll(new Task[]
                    {
                            // Upload to Azure blob storage
                            blobClient.UploadAsync(mediaFile.GetStream(), httpHeaders: new BlobHttpHeaders { ContentType = $"video/mp4" }),
                            // Save message locally
                            _messaging.AddMessageAsync(message)
                    }).ConfigureAwait(false);

                    mediaFile.Dispose();
                    // Send Video message

                    IsBusy = false;

                    await _messaging.SendMessageToRoomAsync(message.RoomId, message.Sender, message.Body).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    MessagingCenter.Send(this, "CameraActionCompleted", "completed");
                    App.IsCameraActivated = false;
                }
            }
        }
        private async Task GalleryVideoAsync()
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
                    App.IsCameraActivated = true;

                    var mediaFile = await CrossMedia.Current.PickVideoAsync();

                    if (mediaFile == null)
                        return;

                    IsBusy = true;
                    var fileName = $"video-{App.UserId}-{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")}.mp4";
                    BlobClient blobClient = _blobContainer.GetBlobClient(fileName);
                    var message = new MessageModel
                    {
                        RoomId = App.RoomId,
                        ServerId = -1,
                        Body = blobClient.Uri.ToString(),
                        Sender = App.UserName,
                        SenderId = App.UserId,
                        CreatedAt = DateTime.UtcNow,
                        ReceivedAt = DateTime.UtcNow,
                        MessageTypes = MessageTypes.Outgoing | MessageTypes.Video | MessageTypes.Leading | MessageTypes.Trailing,
                    };

                    await Task.WhenAll(new Task[]
                    {
                            // upload to azure blob storage
                            blobClient.UploadAsync(mediaFile.GetStream(), httpHeaders: new BlobHttpHeaders { ContentType = $"video/mp4" }),
                            // Save message locally
                            _messaging.AddMessageAsync(message)

                    }).ConfigureAwait(false);

                    mediaFile.Dispose();

                    IsBusy = false;


                    await _messaging.SendMessageToRoomAsync(message.RoomId, message.Sender, message.Body).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    MessagingCenter.Send(this, "CameraActionCompleted", "completed");
                    App.IsCameraActivated = false;
                }
            }
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
        #endregion

        #region Public Methods
        public async Task ExcuteLogInProcessAsync()
        {
            if (App.IsLoggedIn)
                return;

            switch (_userInfoToQuery)
            {
                case UserInfoType.None:

                    // await Task.Delay(1000);
                    Messages.Add(new MessageModel { Body = "안녕하세요? 건강한 여행의 동반자 링거입니다.", Sender = Constants.System, MessageTypes = MessageTypes.Incomming | MessageTypes.Text | MessageTypes.Leading });
                    // await Task.Delay(1500);
                    Messages.Add(new MessageModel { Body = "정확한 상담을 위해 이름, 나이, 성별을 알려주세요.", Sender = Constants.System, MessageTypes = MessageTypes.Incomming | MessageTypes.Text });
                    // await Task.Delay(1500);
                    Messages.Add(new MessageModel { Body = "한 번만 입력하면 다음부터는 링거 상담팀과 곧바로 대화할 수 있습니다. 정보 입력은 세 가지 질문에 답하는 형식으로 진행됩니다.", Sender = Constants.System, MessageTypes = MessageTypes.Incomming | MessageTypes.Text });
                    // await Task.Delay(2000);
                    Messages.Add(new MessageModel { Body = "그럼 정보 입력을 시작하겠습니다.", Sender = Constants.System, MessageTypes = MessageTypes.Incomming | MessageTypes.Text });
                    // await Task.Delay(2500);
                    Messages.Add(new MessageModel { Body = "이름을 입력하세요.", Sender = Constants.System, MessageTypes = MessageTypes.Incomming | MessageTypes.Text | MessageTypes.Trailing, CreatedAt = DateTime.UtcNow });

                    _userInfoToQuery = UserInfoType.Name;

                    break;

                case UserInfoType.Name:


                    App.UserName = TextToSend;
                    Messages.Add(new MessageModel { Body = TextToSend, Sender = App.UserName, MessageTypes = MessageTypes.Text | MessageTypes.Leading | MessageTypes.Trailing | MessageTypes.Outgoing, CreatedAt = DateTime.UtcNow });
                    TextToSend = string.Empty;

                    // TODO: name validation here
                    // 여기서 1차로 서버에 이름을 조회
                    // 신모벙은 가입되지 않은 이름입니다. 다시 한 번 이름을 입력하세요.

                    // if name validation passed

                    // await Task.Delay(1000);

                    Messages.Add(new MessageModel { Body = "생년월일 6자리와, 주민등록번호 뒷자리 1개를 입력해주세요.", Sender = Constants.System, MessageTypes = MessageTypes.Incomming | MessageTypes.Text | MessageTypes.Leading });
                    // await Task.Delay(600);

                    Keyboard = Keyboard.Numeric;
                    Messages.Add(new MessageModel { Body = "예를 들어 1999년 3월 20일에 태어난 여자라면 993202라고 입력하시면 됩니다.", Sender = Constants.System, MessageTypes = MessageTypes.Incomming | MessageTypes.Text | MessageTypes.Trailing, CreatedAt = DateTime.UtcNow });

                    _userInfoToQuery = UserInfoType.BirthDate;
                    break;

                case UserInfoType.BirthDate:

                    // TODO: birth date and sex validation here
                    // 형식 조회 등
                    // 신모벙은 가입되지 않은 이름입니다. 다시 한 번 이름을 입력하세요.

                    var numeric = TextToSend;
                    TextToSend = string.Empty;

                    // process gender
                    string gender = numeric.Substring(6, 1);
                    if (int.TryParse(gender, out var parsedGenderInt))
                        Debug.WriteLine(parsedGenderInt % 2 == 0 ? GenderType.Female : GenderType.Male);
                    else
                        Debug.WriteLine("Wrong Gender format");

                    _genderType = parsedGenderInt % 2 == 0 ? GenderType.Female : GenderType.Male;

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

                    _birthDate = parsedBirthdate;

                    Messages.Add(new MessageModel { Body = $"{year}년 {month}월 {day}일 {_genderType}", Sender = App.UserName });

                    // await Task.Delay(500);

                    Keyboard = Keyboard.Chat;

                    Messages.Add(new MessageModel { Body = "조회 중입니다. 잠시만 기다려주세요.", Sender = Constants.System });

                    // Log in and get Token
                    // TODO: Add user's Location data to validate ticket
                    await _restService.LogInAsync(App.UserName, _birthDate, _genderType);

                    if (App.IsLoggedIn)
                    {
                        // TODO: _messageRepository.LoadMessageAsync, _messagingService.ConnectAsync를 await WhenAll()로 처리한다. 

                        //_messageRepository.Messages.Clear();
                        //await _messageRepository.LoadMessagesAsync(reset: true).ConfigureAwait(false);

                        //Messages = _messageRepository.Messages;

                        Debug.WriteLine("--------------------------LoadMessage Finished------------------------------");
                        Debug.WriteLine("--------------------------LoadMessage------------------------------");
                        Debug.WriteLine("--------------------------Connect------------------------------");
                        Debug.WriteLine("--------------------------Connect Finished------------------------------");

                        App.LastConnectionId = await _messaging.InitAsync(Constants.HubUrl, App.Token);
                    }

                    break;
            }
        }
        public void ResetMessages()
        {
            _messaging.InitMessagesAsync();
        }
        #endregion

        #region public Commands
        public ICommand SendMessageCommand { get; }
        public ICommand ResetConnectionCommand { get; }
        public ICommand TakingPhotoCommand { get; }
        public ICommand TakingVideoCommand { get; }
        public ICommand GalleryPhotoCommand { get; }
        public ICommand GalleryVideoCommand { get; }
        public ICommand LoadBufferCommand { get; }
        public ICommand RefreshCommand { get; }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}