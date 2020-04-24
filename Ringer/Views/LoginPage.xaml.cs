using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ringer.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Ringer.Views
{
    public partial class LoginPage : ContentPage
    {
        private LoginPageViewModel _vm;
        private string _lastElement;
        private bool _isBottomSet;

        public LoginPage()
        {
            InitializeComponent();

            _vm = BindingContext as LoginPageViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Task.Delay(100);

            if (_vm.IsEmailEntryEnabled)
                EmailEntry.Focus();
            else
                PasswordEntry.Focus();
        }

        protected override async void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (!_isBottomSet)
            {
                _isBottomSet = true;

                await Task.Delay(1);

                var insets = On<iOS>().SafeAreaInsets();

                if (insets != default)
                {
                    var pad = MainContainer.Padding;
                    pad.Top += insets.Top;

                    MainContainer.Padding = pad;
                }
            }
        }

        void Entry_Completed(object sender, EventArgs e)
        {
            ConfirmButton_Clicked(sender, e);
        }

        void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Xamarin.Forms.Entry;

            ConfirmButton.IsVisible = (_lastElement == EmailEntry.ClassId) ? entry.Text?.Length > 0 : false;
        }

        void Entry_Focused(object sender, FocusEventArgs e)
        {
            var entry = sender as Xamarin.Forms.Entry;
            _lastElement = entry.ClassId;

            if (_lastElement == PasswordEntry.ClassId)
                entry.Text = string.Empty;

            ConfirmButton.IsVisible = (_lastElement == EmailEntry.ClassId) ? entry.Text?.Length > 0 : false;
        }

        void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            ConfirmButton_Clicked(sender, e);
        }

        void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            ConfirmButton.IsVisible = false;

            if (_lastElement == EmailEntry.ClassId)
                PasswordEntry.Focus();
        }
    }
}
