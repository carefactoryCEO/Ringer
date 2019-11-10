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
        Thickness _insets;
        bool initial = true;

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (Device.RuntimePlatform != Device.iOS || !initial)
                return base.OnMeasure(widthConstraint, heightConstraint);

            initial = false;

            _page = Parent.Parent as ChatPage;

            _insets = _page.On<iOS>().SafeAreaInsets();

            _page.Padding = new Thickness(0, 0, 0, _insets.Bottom);

            return base.OnMeasure(widthConstraint, heightConstraint);
        }

        protected void ChatTextInput_Focused(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform != Device.iOS || _insets.Bottom == 0)
                return;

            _page.Padding = new Thickness(0);
        }

        protected void ChatTextInput_Unfocused(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform != Device.iOS || _insets.Bottom == 0)
                return;

            _page.Padding = new Thickness(0, 0, 0, _insets.Bottom);
        }
    }
}