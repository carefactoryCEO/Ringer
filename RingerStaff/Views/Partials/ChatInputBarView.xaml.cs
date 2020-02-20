using System;
using System.Collections.Generic;
using System.Diagnostics;
using RingerStaff.ViewModels;
using Xamarin.Forms;

namespace RingerStaff.Views.Partials
{
    public partial class ChatInputBarView : ContentView
    {
        #region Constructor
        public ChatInputBarView()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                SizeChanged += (s, e) =>
                {
                    if (Height > _previousHeight)
                    {
                        Debug.WriteLine("--------------------Editor expanded-----------------------");
                        NotifyListScroll();
                    }

                    _previousHeight = Height;
                };
            }
        }
        #endregion

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            vm = BindingContext as ChatPageViewModel;
        }

        #region Custom Events
        public event EventHandler ListShouldBeScrolled;
        public void NotifyListScroll() => ListShouldBeScrolled?.Invoke(this, new EventArgs());
        #endregion

        #region Bindable Properties
        public static BindableProperty PagePaddingProperty
            = BindableProperty.Create(nameof(PagePadding), typeof(Thickness), typeof(ChatInputBarView));
        private double _previousHeight;
        private ChatPageViewModel vm;

        public Thickness PagePadding
        {
            get => (Thickness)GetValue(PagePaddingProperty);
            set => SetValue(PagePaddingProperty, value);
        }
        #endregion


        void ActionsButton_Clicked(object sender, EventArgs e)
        {
            ActionsGrid.IsVisible = true;
            SendButton.IsEnabled = false;
            ActionsButton.IsEnabled = false;
        }

        void RingerEditor_Focused(object sender, EventArgs e)
        {
            ActionsGrid.IsVisible = false;
            SendButton.IsEnabled = true;
            ActionsButton.IsEnabled = true;
        }

        void SendButton_Tapped(object sender, EventArgs e)
        {
            RingerEditor.Focus();
        }
    }
}
