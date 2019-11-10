using Ringer.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Ringer.Views.Partials
{
    public partial class ChatInputBarView : ContentView
    {
        ChatPageViewModel vm;

        public ChatInputBarView()
        {
            InitializeComponent();

            //if (Device.RuntimePlatform == Device.iOS)
            //{
            //    this.SetBinding(HeightRequestProperty, new Binding("Height", BindingMode.OneWay, null, null, null, chatTextInput));
            //}
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext is ChatPageViewModel)
                vm = BindingContext as ChatPageViewModel;
        }

        public void SendMessage()
        {   
            //(this.Parent.Parent.BindingContext as ChatPageViewModel).SendMessageCommand.Execute(null);

            vm?.SendMessageCommand.Execute(null);
        }

        private void SendButton_Tapped(object sender, EventArgs e)
        {
            chatTextInput.Focus();
            SendMessage();
        }

        private async void CameraButton_Clicked(object sender, EventArgs e)
        {
            chatTextInput.Focus();

            string action = await Shell.Current.DisplayActionSheet(
                null,
                vm.CameraAction.Cancle,
                null,
                vm.CameraAction.TakingPhoto,
                vm.CameraAction.AttachingPhoto,
                vm.CameraAction.TakingVideo,
                Device.RuntimePlatform == Device.iOS ? null : vm.CameraAction.AttachingVideo,
                "설정 열기");

            vm?.CameraCommand.Execute(action);
        }

        ChatPage _page;
        double _paddingBottom;

        protected void ChatTextInput_Focused(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform != Device.iOS)
                return;

            _page = Parent.Parent as ChatPage;

            var safeInsets = _page.On<iOS>().SafeAreaInsets();
            _paddingBottom = safeInsets.Bottom;
            safeInsets.Bottom = 0;

            //_page.Padding = safeInsets;

            //Console.WriteLine($"ChatTextInput_Focused: {safeInsets.Left}, {safeInsets.Top}, {safeInsets.Right}, {safeInsets.Bottom}");
        }

        protected void ChatTextInput_Unfocused(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform != Device.iOS)
                return;

            var safeInsets = _page.On<iOS>().SafeAreaInsets();
            safeInsets.Bottom = _paddingBottom;

            //_page.Padding = safeInsets;


            //Console.WriteLine($"ChatTextInput_Unfocused: {safeInsets.Left}, {safeInsets.Top}, {safeInsets.Right}, {safeInsets.Bottom}");
        }
    }
}