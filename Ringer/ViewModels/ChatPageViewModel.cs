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
using Ringer.Helpers;
using Ringer.Models;
using Ringer.Services;
using Ringer.Types;
using Ringer.Views;
using Xamarin.Forms;

namespace Ringer.ViewModels
{
    [QueryProperty("From", "from")]
    public class ChatPageViewModel : INotifyPropertyChanged
    {
        #region private members
        private readonly IMessaging _messaging;
        private readonly BlobContainerClient _blobContainer;
        #endregion

        public string From
        {
            set
            {
                if (value == Constants.PushNotificationString || value == Constants.LocalNotificationString)
                {
                    Device.InvokeOnMainThreadAsync(async () =>
                    {
                        await EnsureMessageLoaded();
                    });
                }
            }
        }

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

        #region Public Properties
        public string TextToSend { get; set; } = string.Empty;
        public double NavBarHeight { get; set; }
        public Thickness BottomPadding { get; set; }
        public Keyboard Keyboard { get; set; } = Keyboard.Chat;
        public ObservableCollection<MessageModel> Messages { get; set; }
        public bool IsBusy { get; set; }
        #endregion

        #region Constructor
        public ChatPageViewModel()
        {
            _blobContainer = new BlobContainerClient(Constants.BlobStorageConnectionString, Constants.BlobContainerName);

            _messaging = DependencyService.Resolve<IMessaging>();
            _messaging.MessageAdded += OnMessageAdded;
            _messaging.MessageUpdated += OnMessageUpdated;
            _messaging.FetchingStateChanged += OnFetchingStateChanged;
            _messaging.MessagesFetched += OnMessagesFetched;

            RefreshCommand = new Command(async () => await RefreshMessageAsync());
            GalleryVideoCommand = new Command(async () => await GalleryVideoAsync());
            TakingVideoCommand = new Command(async () => await TakeVideoAsync());
            GalleryPhotoCommand = new Command(async () => await GalleryPhotoAsync());
            TakingPhotoCommand = new Command(async () => await TakePhotoAsync());
            ResetCommand = new Command(async () => await Reset());
            SendMessageCommand = new Command(async () => await SendMessageAsync());
            LoadBufferCommand = new Command(() => LoadBuffer());

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
                //MessagingCenter.Send(this, "MessageAdded", (object)targetMessage);
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
            MessagingCenter.Send(this, "ShowOrHideKeyboard", false);

            App.Token = null;
            App.RoomId = null;
            App.UserName = null;
            App.LastServerMessageId = 0;

            await _messaging.Clear();
            await _messaging.DisconnectAsync();
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

                    var fileName = $"image-{App.UserId}-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.jpg";
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
        private async Task GalleryVideoAsync()
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
            {
                return true;
            }
            else
            {
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                    await Shell.Current.DisplayAlert("저장소 접근 권한 요청", "사진, 동영상을 저장하고 불러오려면 저장소 권한을 허용해야 합니다.", "확인");

                status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();

                return status == PermissionStatus.Granted;
            }
        }
        private async Task<bool> CheckCameraPermissionAsync()
        {
            if (!CrossMedia.Current.IsCameraAvailable)
            {
                await Shell.Current.DisplayAlert("카메라 사용 불가", "사용 가능한 카메라가 없습니다 :(", "확인");
                return false;
            }

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
            {
                return true;
            }

            var status = await CrossPermissions.Current.CheckPermissionStatusAsync<MicrophonePermission>();

            Debug.WriteLine(status.ToString());

            if (status == PermissionStatus.Granted)
            {
                return true;
            }
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
                {
                    return true;
                }
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
        public async Task RefreshMessageAsync()
        {
            IsBusy = true;
            await _messaging.InitMessagesAsync();
            Messages = new ObservableCollection<MessageModel>(_messaging.Messages);
            _messaging.BufferMessages();
            MessagingCenter.Send(this, "MessageAdded", (object)Messages.Last());
            IsBusy = false;
        }
        #endregion

    }
}