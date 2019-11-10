using Ringer.ViewModels;
using System;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
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
            chatInput.BindingContext = vm;

            
        }

        bool initial = true;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (!initial)
                return;

            initial = false;

            var topInset = (Device.RuntimePlatform == Device.iOS) ? On<iOS>().SafeAreaInsets().Top : 0;

            vm.NavBarHeight = topInset + 40;
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            vm.SendLocalMessage("chatpage.OnAppearing", "test");
            if (!DesignMode.IsDesignModeEnabled)
                vm.ConnectCommand.Execute(null);

         
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            vm.SendLocalMessage("chatpage.OnDiappearing", "test");
            if (!DesignMode.IsDesignModeEnabled)
                vm.DisconnectCommand.Execute(null);
        }

        protected async void BackButton_Tapped(object sender, EventArgs e)
        {
            //await Shell.Current.Navigation.PopModalAsync();

            var state = Shell.Current.CurrentState;

            await Shell.Current.Navigation.PopAsync();
        }

        protected async void MenuButton_Tapped(object sender, EventArgs e)
        {
            await Shell.Current.Navigation.PushModalAsync(new VidyoPage());
        }
    }
}