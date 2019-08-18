using Ringer.Models;
using Ringer.ViewModels;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Ringer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        ChatPageViewModel vm;

        public ChatPage()
        {
            InitializeComponent();
            BindingContext = vm = new ChatPageViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (!DesignMode.IsDesignModeEnabled)
                vm.ConnectCommand.Execute(null);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (!DesignMode.IsDesignModeEnabled)
                vm.DisconnectCommand.Execute(null);
        }


        public void OnListTapped(object sender, ItemTappedEventArgs e)
        {
            chatInput.UnFocusEntry();
            Debug.WriteLine("OnListTapped");
        }

        private void ChatList_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            Debug.WriteLine($"{e.ItemIndex} {(e.Item as Message).Text}");
        }
    }
}