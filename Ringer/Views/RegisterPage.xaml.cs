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
        string _lastElement;
        bool _isCircuitCompleted;
        bool _isBottomSet = false;

        public RegisterPage()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<RegisterPageViewModel>(this, "ShowTermsView", async sender => await ShowTermsViewAsync());
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Task.Delay(100);

            NameEntry.Focus();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (!_isBottomSet)
            {
                _isBottomSet = true;

                var insets = On<iOS>().SafeAreaInsets();

                if (insets != default)
                {
                    var pad = MainContainer.Padding;
                    pad.Top += insets.Top;

                    MainContainer.Padding = pad;
                }
            }
        }

        private Task<bool> ShowTermsViewAsync()
        {
            return TermsView.TranslateTo(0, 0, 250);
        }

        void Entry_Focused(object sender, FocusEventArgs e)
        {
            var entry = sender as Xamarin.Forms.Entry;

            _lastElement = entry.ClassId;

            ConfirmButton.IsVisible =
                _isCircuitCompleted ?
                    entry.Text?.Length > 0 :
                    entry.ClassId == "BirthDateEntry" || entry.ClassId == "SexEntry" ?
                        false :
                        entry.Text?.Length > 0;

            if (_lastElement == "SexEntry")
                entry.Text = string.Empty;

            ContinueButton.IsVisible = false;
        }

        void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            ConfirmButton.IsVisible = false;

            if (_isCircuitCompleted)
                ContinueButton.IsVisible = true;
        }

        void ConfirmButton_Cllicked(object sender, EventArgs e)
        {
            ConfirmButton.IsVisible = false;

            if (_isCircuitCompleted)
                return;

            switch (_lastElement)
            {
                case "NameEntry":
                    {
                        BioStackLayout.IsVisible = true;
                        BirthDateEntry.Focus();
                        InstructionLabel.Text = "주민등록번호를 입력해주세요.";
                        break;
                    }

                case "EmailEntry":
                    {
                        PasswordEntry.IsVisible = true;
                        PasswordEntry.Focus();
                        InstructionLabel.Text = "비밀번호를 입력해주세요.";
                        break;
                    }

                case "PasswordEntry":
                    {
                        _isCircuitCompleted = true;
                        ContinueButton.IsVisible = true;
                        InstructionLabel.Text = "입력 내용을 확인해주세요.";
                        break;
                    }

                default:
                    break;
            }
        }

        async void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Xamarin.Forms.Entry;

            if (_isCircuitCompleted)
            {
                ConfirmButton.IsVisible = entry.Text?.Length > 0;
            }
            else
            {
                if (entry.ClassId == "BirthDateEntry")
                {
                    if (e.NewTextValue?.Length == 6)
                        SexEntry.Focus();
                }
                else if (entry.ClassId == "SexEntry")
                {
                    if (e.NewTextValue?.Length == 1)
                    {
                        EmailEntry.IsVisible = true;
                        await Task.Delay(100);
                        EmailEntry.Focus();

                        InstructionLabel.Text = "이메일을 입력해주세요.";
                    }
                }
                else
                {
                    ConfirmButton.IsVisible = entry.Text?.Length > 0;
                }
            }
        }

        void Entry_Completed(object sender, EventArgs e)
        {
            ConfirmButton_Cllicked(sender, e);
        }
    }
}
