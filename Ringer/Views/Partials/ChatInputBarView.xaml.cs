using Ringer.Helpers;
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
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext is ChatPageViewModel)
                vm = BindingContext as ChatPageViewModel;
        }

        private void SendButton_Tapped(object sender, EventArgs e)
        {
            chatTextInput.Focus();
            vm?.SendMessageCommand.Execute(null);
        }

        private async void CameraButton_Clicked(object sender, EventArgs e)
        {
            string action = await Shell.Current.DisplayActionSheet(
                null,
                Constants.Cancle,
                null,
                Constants.TakingPhoto,
                Constants.TakingVideo,
                Constants.AttachingPhoto,
                Constants.AttachingVideo,
                "설정 열기");

            //vm?.CameraCommand.Execute(action);
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

        double _keyboardHeight;

        void ChatTextInput_Focused(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform != Device.iOS || _insets.Bottom == 0)
                return;

            _page.Padding = new Thickness(0);

            _keyboardHeight = Margin.Bottom;

            Padding = new Thickness(0);
        }

        void ChatTextInput_Unfocused(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform != Device.iOS || _insets.Bottom == 0)
                return;

            //_page.Padding = new Thickness(0, 0, 0, _insets.Bottom);
            Padding = new Thickness(0, 0, 0, _keyboardHeight);
        }

        public EventHandler EditorFocused;
        public EventHandler EditorUnfocused;
    }
}