using System;
using System.Collections.Generic;
using System.Diagnostics;
using RingerStaff.Models;
using RingerStaff.ViewModels;
using Xamarin.Forms;

namespace RingerStaff.Views
{
    public partial class ChatPage : ContentPage
    {
        private ChatPageViewModel vm;

        public ChatPage()
        {
            InitializeComponent();

            BindingContext = vm = new ChatPageViewModel();

            MessagingCenter.Subscribe<ChatPageViewModel, MessageModel>(this, "MessageAdded", (sender, message) =>
            {
                MessageFeed.ScrollTo(message, ScrollToPosition.End, true);
            });
        }

        void Label_Tapped(object sender, EventArgs e)
        {
            Debug.WriteLine("tapped(behind)");
        }

        void ListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            var messageModel = e.Item as MessageModel;

            Debug.WriteLine($"appearing {messageModel.Body}");
        }

        void ListView_ItemDisappearing(object sender, ItemVisibilityEventArgs e)
        {
            var messageModel = e.Item as MessageModel;
            Debug.WriteLine($"disappearing {messageModel.Body}");
        }
    }
}
