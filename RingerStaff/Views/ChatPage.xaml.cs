using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RingerStaff.Models;
using RingerStaff.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace RingerStaff.Views
{
    public partial class ChatPage : ContentPage
    {
        private ChatPageViewModel vm;
        private Thickness _insets;

        public ChatPage()
        {
            InitializeComponent();

            BindingContext = vm = new ChatPageViewModel();

            MessagingCenter.Subscribe<ChatPageViewModel, MessageModel>(this, "MessageAdded", (sender, message) =>
            {
                MessageFeed.ScrollTo(message, ScrollToPosition.End, false);
            });
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            if (Device.RuntimePlatform == Device.iOS)
                _insets = On<iOS>().SafeAreaInsets();

            vm.NavBarHeight = _insets.Top + 44;
            vm.BottomPadding = new Thickness(0, 0, 0, _insets.Bottom);
            //Debug.WriteLine($"-----------{_insets.Left}, {_insets.Top}, {_insets.Right}, {_insets.Bottom}-------------");

            base.OnSizeAllocated(width, height);
        }

        private void OnListShouldBeScrolled(object sender, EventArgs e)
        {
            if (Device.RuntimePlatform == Device.iOS)
                MessageFeed.ScrollToLast();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            vm.LoadMessagesCommand.Execute(null);


        }

        void Label_Tapped(object sender, EventArgs e)
        {
            Debug.WriteLine("tapped(behind)");
        }

        void ListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            //var messageModel = e.Item as MessageModel;
            //Debug.WriteLine($"appearing {messageModel.Body}");
        }

        void ListView_ItemDisappearing(object sender, ItemVisibilityEventArgs e)
        {
            //var messageModel = e.Item as MessageModel;
            //Debug.WriteLine($"disappearing {messageModel.Body}");
        }
    }
}
