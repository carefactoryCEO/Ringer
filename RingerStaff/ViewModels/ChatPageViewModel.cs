﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using Ringer.Core.EventArgs;
using RingerStaff.Helpers;
using RingerStaff.Models;
using RingerStaff.Services;
using RingerStaff.Types;
using RingerStaff.Views;
using Xamarin.Forms;

namespace RingerStaff.ViewModels
{
    [QueryProperty("RoomId", "RoomId")]
    public class ChatPageViewModel : BaseViewModel
    {
        public string RoomId
        {
            get => _roomId;
            set
            {
                App.RoomId = value;
                _roomId = value;
                App.UnreadCounts[_roomId] = 0;

                Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(async () => await OnAppearingAsync());
            }
        }

        #region Public Commands
        public ICommand SendCommand { get; set; }
        public ICommand StopCommand { get; set; }
        public ICommand GoBackCommand { get; set; }
        public ICommand OpenSessionsPageCommand { get; set; }
        public ICommand OpenProfilePageCommand { get; set; }
        public ICommand TakePhotoCommand { get; set; }
        public ICommand PickPhotoCommand { get; set; }
        public ICommand TakeVideoCommand { get; set; }
        public ICommand PickVideoCommand { get; set; }
        public ICommand InviteCommand { get; set; }

        public ICommand LoadBufferCommand { get; set; }
        #endregion

        private ObservableCollection<MessageModel> messages;
        private readonly BlobContainerClient _blobContainer;
        private string textToSend;
        private double navBarHeight;
        private Thickness bottomPadding;
        private string _roomId;

        public string TextToSend { get => textToSend; set => SetProperty(ref textToSend, value); }
        public ObservableCollection<MessageModel> Messages { get => messages; set => SetProperty(ref messages, value); }
        public double NavBarHeight { get => navBarHeight; set => SetProperty(ref navBarHeight, value); }
        public Thickness BottomPadding { get => bottomPadding; set => SetProperty(ref bottomPadding, value); }

        public ChatPageViewModel()
        {
            messages = new ObservableCollection<MessageModel>();
            _blobContainer = new BlobContainerClient(Constant.BlobStorageConnectionString, Constant.BlobContainerName);

            SendCommand = new Command(async () => await SendMessageAsync());
            GoBackCommand = new Command(async () => await ExcuteGoBackCommand());
            OpenSessionsPageCommand = new Command(async () => await ExcuteOpenSessionsPageCommand());
            OpenProfilePageCommand = new Command(async () => await ExcuteOpenProfilePageCommand());

            TakePhotoCommand = new Command(async () => await TakePhotoAsync());
            PickPhotoCommand = new Command(async () => await PickPhotoAsync());
            TakeVideoCommand = new Command(async () => await TakeVideoAsync());
            PickVideoCommand = new Command(async () => await PickVideoAsync());
            InviteCommand = new Command(async () => await InviteAsync());
            LoadBufferCommand = new Command(async () => await LoadBufferAsync());

            RealTimeService.MessageReceived += RealTimeService_MessageReceived;
            RealTimeService.Reconnecting += RealTimeService_Reconnecting;
            RealTimeService.Reconnected += RealTimeService_Reconnected;
            RealTimeService.SomeoneEntered += RealTimeService_SomeoneEntered;
            RealTimeService.SomeoneLeft += RealTimeService_SomeoneLeft;

            messages.CollectionChanged += Messages_CollectionChanged;
        }

        private async Task LoadBufferAsync()
        {
            IsBusy = true;

            int skip = Messages.Count;
            int take = 50;

            List<PendingMessage> pendingMessages = await ApiService.GetSegmentedMessages(_roomId, skip, take);

            if (!pendingMessages.Any())
            {
                IsBusy = false;
                return;
            }

            var newMessages = new ObservableCollection<MessageModel>();

            MessageModel[] messages = pendingMessages
                .OrderBy(pm => pm.CreatedAt)
                .Select(pm => new MessageModel
                {
                    ServerId = pm.Id,
                    Body = pm.Body,
                    Sender = pm.SenderName,
                    SenderId = pm.SenderId,
                    CreatedAt = pm.CreatedAt,
                    UnreadCount = 0
                })
                .ToArray();

            var scrollTarget = Messages.First();

            for (int i = 0; i < messages.Length; i++)
            {
                MessageModel lastMessage = i > 0 ? messages[i - 1] : null;
                Utility.SetMessageTypes(ref messages[i], ref lastMessage, App.UserId);
                Messages.Insert(i, messages[i]);
            }

            MessagingCenter.Send(this, "MessageLoaded", scrollTarget);
            //var mergedMessages = newMessages.Concat(Messages);

            //Debug.WriteLine(mergedMessages.Count());

            //Messages = mergedMessages as ObservableCollection<MessageModel>;

            // skip : 현재 메시지 개수
            // take : 50

            IsBusy = false;
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
                    };

                    MessagingCenter.Send(this, "CameraActionCompleted", "completed");

                    await blobClient.UploadAsync(mediaFile.GetStream(), httpHeaders: new BlobHttpHeaders { ContentType = "image/jpeg" });

                    var lastMessage = messages.LastOrDefault();
                    Utility.SetMessageTypes(ref message, ref lastMessage, App.UserId);

                    Messages.Add(message);

                    mediaFile.Dispose();

                    IsBusy = false;

                    //// Send image message to server
                    await RealTimeService.SendMessageAsync(message, message.RoomId).ConfigureAwait(false);
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
        private async Task PickPhotoAsync()
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
                };

                MessagingCenter.Send(this, "CameraActionCompleted", "completed");

                await blobClient.UploadAsync(mediaFile.GetStream(), httpHeaders: new BlobHttpHeaders { ContentType = "image/jpeg" });

                var lastMessage = messages.LastOrDefault();
                Utility.SetMessageTypes(ref message, ref lastMessage, App.UserId);
                Messages.Add(message);

                mediaFile.Dispose();

                IsBusy = false;

                await RealTimeService.SendMessageAsync(message, message.RoomId).ConfigureAwait(false);

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
                    });

                    if (mediaFile == null)
                        return;

                    IsBusy = true;

                    var fileName = $"video-{App.UserId}-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.mp4";
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
                    };

                    MessagingCenter.Send(this, "CameraActionCompleted", "completed");

                    await blobClient.UploadAsync(mediaFile.GetStream(), httpHeaders: new BlobHttpHeaders { ContentType = $"video/mp4" });

                    var lastMessage = messages.LastOrDefault();
                    Utility.SetMessageTypes(ref message, ref lastMessage, App.UserId);
                    Messages.Add(message);

                    mediaFile.Dispose();

                    IsBusy = false;

                    await RealTimeService.SendMessageAsync(message, message.RoomId).ConfigureAwait(false);
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
        private async Task PickVideoAsync()
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
                var fileName = $"video-{App.UserId}-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.mp4";
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

                var lastMessage = messages.LastOrDefault();
                Utility.SetMessageTypes(ref message, ref lastMessage, App.UserId);
                Messages.Add(message);

                mediaFile.Dispose();

                IsBusy = false;

                await RealTimeService.SendMessageAsync(message, message.RoomId).ConfigureAwait(false);
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
            return Device.RuntimePlatform == Xamarin.Forms.Device.iOS
                ? await CheckCameraPermissionAsync() && await CheckPhotosPermissionsAsync() && await CheckMicPermissionAsync()
                : Device.RuntimePlatform == Device.Android
                ? await CheckCameraPermissionAsync() && await CheckStoragePermissionAsync()
                : false;
        }
        #endregion

        private async Task InviteAsync()
        {

        }

        private void RealTimeService_SomeoneLeft(object sender, SignalREventArgs e)
        {
            if (e.Sender == App.UserName)
                return;

            MessageModel message = new MessageModel
            {
                Body = $"{e.Sender}가 나갔습니다.",
                MessageTypes = MessageTypes.EntranceNotice | MessageTypes.Leading | MessageTypes.Trailing
            };

            messages.Add(message);
        }
        private void RealTimeService_SomeoneEntered(object sender, SignalREventArgs e)
        {
            if (e.Sender == App.UserName)
                return;

            MessageModel message = new MessageModel
            {
                Body = $"{e.Sender}가 들어왔습니다.",
                MessageTypes = MessageTypes.EntranceNotice | MessageTypes.Leading | MessageTypes.Trailing

            };

            Messages.Add(message);
        }
        private void RealTimeService_Reconnecting(object sender, ConnectionEventArgs e)
        {
            Debug.WriteLine(e.Message);
            MessagingCenter.Send(this, "ConnectionEvent", e.Message);
        }
        private void RealTimeService_Reconnected(object sender, ConnectionEventArgs e)
        {
            Debug.WriteLine(e.Message);
        }

        private void RealTimeService_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.RoomId != _roomId)
            {
                return;
            }

            var lastMessage = Messages.LastOrDefault();

            // 내가 방금 보낸 메시지면 바디가 같음.
            if (lastMessage.Body == e.Body && lastMessage.SenderId == e.SenderId)
                return;

            MessageModel message = new MessageModel
            {
                Body = e.Body,
                Sender = e.SenderName,
                CreatedAt = e.CreatedAt,
                SenderId = e.SenderId
            };

            Utility.SetMessageTypes(ref message, ref lastMessage, App.UserId);

            Messages.Add(message);
        }

        private async Task SendMessageAsync()
        {
            var body = TextToSend.Trim();

            if (string.IsNullOrEmpty(body))
                return;

            TextToSend = string.Empty;

            var lastMessage = messages.LastOrDefault();
            var message = new MessageModel
            {
                Body = body,
                SenderId = App.UserId,
                Sender = App.UserName,
                CreatedAt = DateTime.UtcNow
            };

            Utility.SetMessageTypes(ref message, ref lastMessage, App.UserId);

            Messages.Add(message);

            await RealTimeService.SendMessageAsync(message, _roomId);
        }

        private void Messages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                MessagingCenter.Send(this, "MessageAdded", messages.Last());
            }
        }

        private Task ExcuteOpenProfilePageCommand()
        {
            return Task.CompletedTask;
            //return Shell.Current.GoToAsync("profilepage");
            //return Shell.Current.Navigation.PushModalAsync(new ProfilePage());
        }
        private Task ExcuteOpenSessionsPageCommand()
        {
            return Task.CompletedTask;
            //return Shell.Current.Navigation.PushModalAsync(new SessionsPage());
        }
        private Task ExcuteGoBackCommand()
        {
            return Shell.Current.Navigation.PopAsync();
        }

        public async Task OnAppearingAsync()
        {
            IsBusy = true;
            Title = await ApiService.GetRoomNameByIdAsync(_roomId);

            await Task.WhenAll(new Task[]
            {
                RealTimeService.EnterRoomAsync(_roomId, App.UserName),
                LoadMessagesAsync()
            }).ConfigureAwait(false);

            IsBusy = false;
        }
        public async Task LoadMessagesAsync()
        {
            Messages.Clear();

            List<PendingMessage> pendingMessages = await ApiService.GetSegmentedMessages(_roomId, 0, 50);

            if (pendingMessages == null || !pendingMessages.Any())
                return;

            MessageModel[] messages = pendingMessages
                .OrderBy(p => p.CreatedAt)
                .Select(pm => new MessageModel
                {
                    ServerId = pm.Id,
                    Body = pm.Body,
                    Sender = pm.SenderName,
                    SenderId = pm.SenderId,
                    CreatedAt = pm.CreatedAt,
                    UnreadCount = 0
                })
                .ToArray();

            for (int i = 0; i < messages.Length; i++)
            {
                MessageModel lastMessage = i > 0 ? messages[i - 1] : null;
                Utility.SetMessageTypes(ref messages[i], ref lastMessage, App.UserId);
                Messages.Add(messages[i]);
            }

            if (Messages.Any())
                MessagingCenter.Send(this, "MessageAdded", Messages.Last());
        }


    }
}
