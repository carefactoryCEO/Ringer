using System;
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
using Ringer.Core.Data;
using Ringer.Extensions;
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
        private string _name;
        #endregion

        #region Public Properties
        public string TextToSend { get; set; } = string.Empty;
        public string NavBarTitle { get; set; } = App.IsLoggedIn ? App.UserName : "로그인";
        public string Title { get; set; } = "링거 상담실";
        public double NavBarHeight { get; set; } = 0;
        public Keyboard Keyboard { get; set; } = Keyboard.Chat;
        public Thickness BottomPadding { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; }
        public bool IsBusy { get; set; }
        public bool KeyboardShouldBeShown { get; set; }
        public bool IsProcessingLogin { get; set; }
        public bool Retrying { get; set; }
        #endregion

        #region Constructor
        public ChatPageViewModel()
        {
            _blobContainer = new BlobContainerClient(Constants.BlobStorageConnectionString, Constants.BlobContainerName);

            _messaging = DependencyService.Resolve<IMessaging>();
            _restService = DependencyService.Resolve<IRESTService>();

            _messaging.MessageAdded += OnMessageAdded;
            _messaging.MessageUpdated += OnMessageUpdated;
            _messaging.FetchingStateChanged += OnFetchingStateChanged;
            _messaging.MessagesFetched += OnMessagesFetched;

            SendMessageCommand = new Command(async () => await SendMessageAsync());
            TakingPhotoCommand = new Command(async () => await TakePhotoAsync());
            TakingVideoCommand = new Command(async () => await TakeVideoAsync());
            GalleryPhotoCommand = new Command(async () => await GalleryPhotoAsync());
            GalleryVideoCommand = new Command(async () => await GalleryVideoAsync());
            ResetCommand = new Command(async () => await Reset());
            LoadBufferCommand = new Command(() => LoadBuffer());
            RefreshCommand = new Command(() =>
            {
                Messages = new ObservableCollection<MessageModel>(_messaging.Messages);
                MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());
            });

            Messages = new ObservableCollection<MessageModel>(_messaging.Messages);
            _messaging.BufferMessages();
        }
        #endregion

        #region Private Methods
        private void OnMessagesFetched(object sender, MessageModel[] fetchedMessages)
        {
            if (fetchedMessages.Any())
            {
                foreach (var message in fetchedMessages)
                    Messages.Add(message);

                MessagingCenter.Send(this, "MessageAdded", (object)fetchedMessages.Last());
            }
        }
        private void OnFetchingStateChanged(object sender, FetchingState state)
        {
            IsBusy = (state == FetchingState.Fetching) ? true : false;
        }
        private void OnMessageUpdated(object sender, MessageModel updatedMessage)
        {
            var targetMessage = Messages.LastOrDefault(t => t.Id == updatedMessage.Id);

            if (targetMessage != null)
            {
                targetMessage.MessageTypes = updatedMessage.MessageTypes;
                MessagingCenter.Send(this, "MessageAdded", (object)targetMessage);
            }
        }
        private void OnMessageAdded(object sender, MessageModel newMessage)
        {
            Messages.Add(newMessage);
            MessagingCenter.Send(this, "MessageAdded", (object)newMessage);
        }
        private void LoadBuffer()
        {
            if (!_messaging.Messages.Any() || Messages.Count == _messaging.Messages.Count)
                return;

            IsBusy = true;

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
                await LogInProcessAsync();
                return;
            }

            TextToSend = TextToSend.Trim();

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

            await _messaging.AddMessageAsync(message).ConfigureAwait(false);
            await _messaging.SendMessageToRoomAsync(message.RoomId, message.Sender, message.Body).ConfigureAwait(false);
        }
        private async Task Reset()
        {
            App.Token = null;
            App.RoomId = null;
            App.UserName = null;
            App.LastServerMessageId = 0;
            _userInfoToQuery = UserInfoType.None;
            NavBarTitle = "로그인";
            Retrying = false;

            await _messaging.Clear();
            await _messaging.DisconnectAsync();
            await LogInProcessAsync();
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

                    MessagingCenter.Send(this, "CameraActionCompleted", "completed");

                    await blobClient.UploadAsync(mediaFile.GetStream(), httpHeaders: new BlobHttpHeaders { ContentType = "image/jpeg" });
                    await _messaging.AddMessageAsync(message);

                    //await Task.WhenAll(new Task[]
                    //{
                    //    // Upload to azure blob storage
                    //    blobClient.UploadAsync(mediaFile.GetStream(), httpHeaders: new BlobHttpHeaders { ContentType = "image/jpeg" }),
                    //    // Display image message to view locally
                    //    _messaging.AddMessageAsync(message)
                    //});

                    mediaFile.Dispose();

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
                        CompressionQuality = 75,
                        CustomPhotoSize = 50,
                        PhotoSize = PhotoSize.MaxWidthHeight,
                        MaxWidthHeight = 1000
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

                    MessagingCenter.Send(this, "CameraActionCompleted", "completed");

                    await blobClient.UploadAsync(mediaFile.GetStream(), httpHeaders: new BlobHttpHeaders { ContentType = "image/jpeg" });
                    await _messaging.AddMessageAsync(message);

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
                        DesiredSize = 10 * 1024 * 1024, // android 10MB
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

                    MessagingCenter.Send(this, "CameraActionCompleted", "completed");

                    await blobClient.UploadAsync(mediaFile.GetStream(), httpHeaders: new BlobHttpHeaders { ContentType = $"video/mp4" });
                    await _messaging.AddMessageAsync(message);

                    await Task.WhenAll(new Task[]
                    {

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

                    MessagingCenter.Send(this, "CameraActionCompleted", "completed");

                    await blobClient.UploadAsync(mediaFile.GetStream(), httpHeaders: new BlobHttpHeaders { ContentType = $"video/mp4" });
                    await _messaging.AddMessageAsync(message);

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
        public async Task EnsureMessageLoaded()
        {
            IsBusy = true;
            await _messaging.InitMessagesAsync();
            Messages = new ObservableCollection<MessageModel>(_messaging.Messages);
            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());
            IsBusy = false;
        }
        public async Task LogInProcessAsync()
        {
            if (App.IsLoggedIn)
                return;

            TextToSend = TextToSend.RemoveWhiteSpaces();



            //    else
            //    {
            //        await Task.Delay(2000);
            //        Messages.Add(new MessageModel
            //        {
            //            Body = $"다시 입력하시려면 10초 안에 \"다시\"라고 입력하세요.",
            //            Sender = Constants.System,
            //            MessageTypes = MessageTypes.Text | MessageTypes.Incomming | MessageTypes.Trailing,
            //            CreatedAt = DateTime.UtcNow
            //        });
            //        MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

            //        await Task.Delay(1000);
            //        ChangeKeyboardStatus(show: true, keyboard: Keyboard.Chat);

            //        await Task.Delay(9000);

            //        if (Retrying)
            //            return;

            //        Messages.Add(new MessageModel
            //        {
            //            Body = "곧 링거 서포트팀에서 다음 절차를 안내해드리겠습니다. 잠시만 기다리세요.",
            //            Sender = Constants.System,
            //            MessageTypes = MessageTypes.Text | MessageTypes.Incomming | MessageTypes.Leading | MessageTypes.Trailing,
            //            CreatedAt = DateTime.UtcNow
            //        });
            //        MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

            //        await Task.Delay(1000);
            //        ChangeKeyboardStatus(show: false);
            //    }

            //    return;
            //}

            switch (_userInfoToQuery)
            {
                case UserInfoType.None:
                    {
                        if (!Retrying)
                        {
                            Messages.Clear();

                            await Task.Delay(1000);
                            ChangeKeyboardStatus(show: false);

                            await Task.Delay(1000);
                            Messages.Add(new MessageModel
                            {
                                Body = "안녕하세요? 건강한 여행의 동반자 링거입니다.",
                                Sender = Constants.System,
                                MessageTypes = MessageTypes.Incomming | MessageTypes.Text | MessageTypes.Leading
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                            await Task.Delay(1500);
                            Messages.Add(new MessageModel
                            {
                                Body = "정확한 상담을 위해 이름, 생년월일, 성별을 알려주세요.",
                                Sender = Constants.System,
                                MessageTypes = MessageTypes.Incomming | MessageTypes.Text
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                            await Task.Delay(1500);
                            Messages.Add(new MessageModel
                            {
                                Body = "한 번만 입력하면 다음부터는 링거 상담팀과 곧바로 대화할 수 있습니다. 정보 입력은 두 가지 질문에 답하는 형식으로 진행됩니다.",
                                Sender = Constants.System,
                                MessageTypes = MessageTypes.Incomming | MessageTypes.Text
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                            await Task.Delay(2000);
                            Messages.Add(new MessageModel
                            {
                                Body = "그럼 입력을 시작하겠습니다.",
                                Sender = Constants.System,
                                MessageTypes = MessageTypes.Incomming | MessageTypes.Text
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());
                        }

                        await Task.Delay(1500);
                        Messages.Add(new MessageModel
                        {
                            Body = "이름을 입력하세요.",
                            Sender = Constants.System,
                            MessageTypes = MessageTypes.Incomming | MessageTypes.Text | MessageTypes.Trailing,
                            CreatedAt = DateTime.UtcNow
                        });
                        MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                        await Task.Delay(1000);
                        ChangeKeyboardStatus(show: true, keyboard: Keyboard.Chat);

                        _userInfoToQuery = UserInfoType.Name;

                        break;
                    }
                case UserInfoType.Name:
                    {
                        _name = TextToSend;
                        TextToSend = string.Empty;

                        Messages.Add(new MessageModel
                        {
                            Body = _name,
                            MessageTypes = MessageTypes.Text | MessageTypes.Outgoing | MessageTypes.Leading | MessageTypes.Trailing,
                            CreatedAt = DateTime.UtcNow
                        });
                        MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                        #region validate name input

                        // 이름 포맷 체크
                        if (!_name.IsKoreanOnly())
                        {
                            await Task.Delay(1000);
                            Messages.Add(new MessageModel
                            {
                                Body = $"입력하신 \"{_name}\"은 한글 이름이 아닌 것 같습니다. 다시 입력해주세요.",
                                MessageTypes = MessageTypes.Text | MessageTypes.Incomming | MessageTypes.Leading | MessageTypes.Trailing,
                                CreatedAt = DateTime.UtcNow
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                            return;
                        }

                        // 이름 체크 통과

                        #endregion

                        if (!Retrying)
                        {
                            await Task.Delay(1000);
                            ChangeKeyboardStatus(show: false);

                            await Task.Delay(1000);
                            Messages.Add(new MessageModel
                            {
                                Body = $"{_name}님 반갑습니다.",
                                Sender = Constants.System,
                                MessageTypes = MessageTypes.Incomming | MessageTypes.Text | MessageTypes.Leading,
                                CreatedAt = DateTime.UtcNow
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());
                        }

                        await Task.Delay(1000);
                        Messages.Add(new MessageModel
                        {
                            Body = "생년월일 6자리와 성별 번호 1자리를 연속해서 입력해주세요.",
                            Sender = Constants.System,
                            MessageTypes = MessageTypes.Incomming | MessageTypes.Text,
                            CreatedAt = DateTime.UtcNow
                        });
                        MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                        await Task.Delay(1500);
                        Messages.Add(new MessageModel
                        {
                            Body = $"성별 번호는\n2000년 이전에 태어난\n  남자는 1, 여자는 2\n2000년 이후에 태어난\n  남자는 3, 여자는 4\n입니다.",
                            Sender = Constants.System,
                            MessageTypes = MessageTypes.Incomming | MessageTypes.Text,
                            CreatedAt = DateTime.UtcNow
                        });
                        MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                        if (!Retrying)
                        {
                            await Task.Delay(2000);
                            Messages.Add(new MessageModel
                            {
                                Body = "예를 들어 1999년 3월 20일에 태어난 여자라면 9903202라고 입력하시면 됩니다.",
                                Sender = Constants.System,
                                MessageTypes = MessageTypes.Incomming | MessageTypes.Text,
                                CreatedAt = DateTime.UtcNow
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                            await Task.Delay(1500);
                            Messages.Add(new MessageModel
                            {
                                Body = "7자리 숫자를 입력하세요.",
                                Sender = Constants.System,
                                MessageTypes = MessageTypes.Incomming | MessageTypes.Text | MessageTypes.Trailing,
                                CreatedAt = DateTime.UtcNow
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());
                        }

                        await Task.Delay(1500);
                        ChangeKeyboardStatus(show: true, keyboard: Keyboard.Numeric);

                        _userInfoToQuery = UserInfoType.BirthDate;

                        break;
                    }
                case UserInfoType.BirthDate:
                    {
                        string numericInput = TextToSend;
                        TextToSend = string.Empty;

                        Messages.Add(new MessageModel
                        {
                            Body = numericInput,
                            MessageTypes = MessageTypes.Text | MessageTypes.Outgoing | MessageTypes.Leading | MessageTypes.Trailing,
                            CreatedAt = DateTime.UtcNow
                        });
                        MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                        // validate numeric input

                        // confirm the input has exactly 7 numeric digit
                        if (!numericInput.IsSevenNumericDigit())
                        {
                            await Task.Delay(1000);
                            Messages.Add(new MessageModel
                            {
                                Body = $"숫자 7개만 입력하셔야 해요.",
                                MessageTypes = MessageTypes.Text | MessageTypes.Incomming | MessageTypes.Leading | MessageTypes.Trailing,
                                CreatedAt = DateTime.UtcNow
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                            return;
                        }

                        // 3. confirm birth date and sex format
                        if (!numericInput.IsValidBirthDateAndSex(out var birthDate, out GenderType gender))
                        {
                            await Task.Delay(1000);
                            Messages.Add(new MessageModel
                            {
                                Body = $"생년월일과 성별을 나타내는 숫자 7자리를 정확하게 입력하셔야 합니다.",
                                MessageTypes = MessageTypes.Text | MessageTypes.Incomming | MessageTypes.Leading | MessageTypes.Trailing,
                                CreatedAt = DateTime.UtcNow
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                            return;
                        }

                        // 생년월일, 성별 체크 통과

                        await Task.Delay(1000);
                        ChangeKeyboardStatus(show: false);

                        //DateTime birthDate = new DateTime(year, month, day);
                        string genderString = gender == GenderType.Female ? "여자" : "남자";

                        await Task.Delay(1000);
                        Messages.Add(new MessageModel
                        {
                            Body = $"생일이 {birthDate.Year}년 {birthDate.Month}월 {birthDate.Day}일인 {genderString} {_name}님으로 조회합니다.",
                            Sender = App.UserName,
                            MessageTypes = MessageTypes.Text | MessageTypes.Incomming | MessageTypes.Leading,
                            CreatedAt = DateTime.UtcNow
                        });
                        MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                        await Task.Delay(1000);
                        Messages.Add(new MessageModel
                        {
                            Body = "잠시만 기다려주세요.",
                            Sender = Constants.System,
                            MessageTypes = MessageTypes.Text | MessageTypes.Incomming | MessageTypes.Trailing,
                            CreatedAt = DateTime.UtcNow
                        });
                        MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                        if (await _restService.LogInAsync(_name, birthDate, gender))
                        {
                            await Task.Delay(1000);
                            IsProcessingLogin = false;

                            App.LastConnectionId = await _messaging.InitAsync(Constants.HubUrl, App.Token);
                            NavBarTitle = App.UserName;

                            Messages = new ObservableCollection<MessageModel>(_messaging.Messages);
                            _messaging.BufferMessages();

                            if (Messages.Any())
                                MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                            await Task.Delay(1000);
                            ChangeKeyboardStatus(show: true, keyboard: Keyboard.Chat);
                        }
                        else
                        {
                            Retrying = false;

                            await Task.Delay(1000);
                            Messages.Add(new MessageModel
                            {
                                Body = "입력하신 정보는 링거에 등록되지 않은 정보입니다. 잘못 입력하셨나요?",
                                Sender = Constants.System,
                                MessageTypes = MessageTypes.Text | MessageTypes.Incomming | MessageTypes.Leading,
                                CreatedAt = DateTime.UtcNow
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                            await Task.Delay(2000);
                            Messages.Add(new MessageModel
                            {
                                Body = $"다시 입력하시려면 15초 안에 \"다시\"라고 입력하세요.",
                                Sender = Constants.System,
                                MessageTypes = MessageTypes.Text | MessageTypes.Incomming | MessageTypes.Trailing,
                                CreatedAt = DateTime.UtcNow
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                            await Task.Delay(1000);
                            ChangeKeyboardStatus(show: true, keyboard: Keyboard.Chat);

                            _userInfoToQuery = UserInfoType.Retry;

                            // 최종 실패
                            await Task.Delay(15000);

                            if (!Retrying)
                            {
                                Messages.Add(new MessageModel
                                {
                                    Body = "곧 링거 서포트팀에서 다음 절차를 안내해드리겠습니다. 잠시만 기다리세요.",
                                    Sender = Constants.System,
                                    MessageTypes = MessageTypes.Text | MessageTypes.Incomming | MessageTypes.Leading | MessageTypes.Trailing,
                                    CreatedAt = DateTime.UtcNow
                                });
                                MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());
                                ChangeKeyboardStatus(show: false);
                            }
                        }
                        break;
                    }
                case UserInfoType.Retry:
                    {
                        var retryString = TextToSend;
                        TextToSend = string.Empty;

                        Messages.Add(new MessageModel
                        {
                            Body = retryString,
                            MessageTypes = MessageTypes.Text | MessageTypes.Outgoing | MessageTypes.Leading | MessageTypes.Trailing,
                            CreatedAt = DateTime.UtcNow
                        });
                        MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                        if (retryString == "다시")
                        {
                            Retrying = true;

                            await Task.Delay(1500);
                            Messages.Add(new MessageModel
                            {
                                Body = "다시 시도하겠습니다. 차분히 입력해주세요.",
                                MessageTypes = MessageTypes.Text | MessageTypes.Incomming | MessageTypes.Leading,
                                CreatedAt = DateTime.UtcNow
                            });
                            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());

                            _userInfoToQuery = UserInfoType.None;

                            LogInProcessAsync();

                        }

                        break;
                    }
            }
        }

        private void ChangeKeyboardStatus(bool show, Keyboard keyboard = default)
        {
            IsProcessingLogin = !show;
            IsBusy = !show;
            MessagingCenter.Send(this, "KeyboardShow", show);

            if (show)
                Keyboard = keyboard;
        }

        public void InitializeMessages()
        {
            _messaging.InitMessagesAsync();
        }
        #endregion

        #region public Commands
        public ICommand SendMessageCommand { get; }
        public ICommand ResetCommand { get; }
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