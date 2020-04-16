using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ringer.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Ringer.Views
{
    public partial class RegisterPage : ContentPage
    {

        public RegisterPage()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<RegisterPageViewModel, string>(this, "FocusEntry", async (vm, entryName) =>
            {
                var entry = this.FindByName<Xamarin.Forms.Entry>(entryName);

                await Task.Delay(100);

                entry.Focus();
            });

            MessagingCenter.Subscribe<RegisterPageViewModel, string>(this, "UnfocusEntry", (vm, entryName) =>
            {
                var entry = this.FindByName<Xamarin.Forms.Entry>(entryName);
                entry.Unfocus();
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Task.Delay(100);

            NameEntry.Focus();
        }

        bool _isBottomSetted = false;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (!_isBottomSetted)
            {
                _isBottomSetted = true;

                var insets = On<iOS>().SafeAreaInsets();

                if (insets != default)
                {
                    var pad = MainContainer.Padding;
                    pad.Top += insets.Top;

                    MainContainer.Padding = pad;
                }
            }
        }

        async void ContinueButton_Clicked(object sender, EventArgs e)
        {
            await PermissionView.TranslateTo(0, 0, 250);
        }

        async void Button_Clicked_1(object sender, EventArgs e)
        {
            await PermissionView.TranslateTo(0, 1000, 250);
        }

        void Entry_Focused(object sender, FocusEventArgs e)
        {
            // hide Continue button

            // show confirm button if text length is greater then 0
            // var entry = sender as Xamarin.Forms.Entry;
        }

        void Entry_Unfocused(object sender, FocusEventArgs e)
        {
        }

        void ConfirmButton_Cllicked(object sender, EventArgs e)
        {
            ConfirmButton.IsVisible = false;
        }

        void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Xamarin.Forms.Entry;

            if (entry.ClassId == "BirthDate")
            {
                if (e.NewTextValue.Length == 6)
                    SexEntry.Focus();
            }
            else if (entry.ClassId == "Sex")
            {
                if (e.NewTextValue.Length == 1)
                    EmailEntry.Focus();
            }
            else
            {
                ConfirmButton.IsVisible = entry.Text.Length > 0;
            }

        }
    }
}
