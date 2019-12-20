using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace RingerStaff.Views.Partials
{
    public partial class ChatInputBarView : ContentView
    {
        public void OnKeyboardActivated()
        {
            KeyboardActivated?.Invoke(this, new EventArgs());
        }

        public static BindableProperty PagePaddingProperty
            = BindableProperty.Create(nameof(PagePadding), typeof(Thickness), typeof(ChatInputBarView));

        public Thickness PagePadding
        {
            get => (Thickness)GetValue(PagePaddingProperty);
            set => SetValue(PagePaddingProperty, value);
        }

        public event EventHandler KeyboardActivated;

        public ChatInputBarView()
        {
            InitializeComponent();
        }

        void PlusButton_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("hehehe");
        }


    }
}
