using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Ringer.ViewModels;
using Xamarin.Forms;

namespace Ringer.Views.Partials
{
    public partial class InputBarView : ContentView
    {
        #region Constructor
        public InputBarView()
        {
            InitializeComponent();

            //if (Device.RuntimePlatform == Device.iOS)
            //{
            SizeChanged += (s, e) =>
            {
                if (Height > _previousHeight)
                {
                    Debug.WriteLine("--------------------Editor expanded-----------------------");
                    NotifyListScroll();
                }

                _previousHeight = Height;
            };
            //}

            MessagingCenter.Subscribe<ChatPageViewModel, string>(this, "CameraActionCompleted", (s, e) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ActionsGrid.IsVisible = false;

                    SendButton.IsEnabled = ActionsButton.IsEnabled = true;
                });
            });

            MessagingCenter.Subscribe<ChatPageViewModel, bool>(this, "ShowOrHideKeyboard", (sender, showing) =>
            {
                if (!showing)
                    RingerEditor.Unfocus();

                if (showing)
                    RingerEditor.Focus();
            });
        }

        #endregion

        #region Custom Events
        public event EventHandler ListShouldBeScrolled;
        public event EventHandler<bool> KeyboardShuldBeShownChanged;
        public void NotifyListScroll() => ListShouldBeScrolled?.Invoke(this, new EventArgs());
        #endregion

        #region Bindable Properties
        public static BindableProperty PagePaddingProperty
            = BindableProperty.Create(nameof(PagePadding), typeof(Thickness), typeof(InputBarView));
        private double _previousHeight;

        public Thickness PagePadding
        {
            get => (Thickness)GetValue(PagePaddingProperty);
            set => SetValue(PagePaddingProperty, value);
        }
        #endregion

        void ActionsButton_Clicked(object sender, EventArgs e)
        {
            ActionsGrid.IsVisible = true;

            SendButton.IsEnabled = ActionsButton.IsEnabled = false;
        }

        void RingerEditor_Focused(object sender, EventArgs e)
        {
            ActionsGrid.IsVisible = false;

            SendButton.IsEnabled = ActionsButton.IsEnabled = true;
        }

        void SendButton_Tapped(object sender, EventArgs e)
        {
            RingerEditor.Focus();
        }
    }
}
