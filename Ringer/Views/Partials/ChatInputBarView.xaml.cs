using Ringer.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Ringer.Views.Partials
{
    public partial class ChatInputBarView : ContentView
    {
        public ChatInputBarView()
        {
            InitializeComponent();

            //if (Device.RuntimePlatform == Device.iOS)
            //{
            //    this.SetBinding(HeightRequestProperty, new Binding("Height", BindingMode.OneWay, null, null, null, chatTextInput));
            //}
        }
        public void SendMessage()
        {   
            (this.Parent.Parent.BindingContext as ChatPageViewModel).SendMessageCommand.Execute(null);
        }

        public void UnFocusEntry()
        {
            chatTextInput?.Unfocus();
        }

        DateTime _lastTime = DateTime.UtcNow;

        void Handle_Focused(object sender, FocusEventArgs e)
        {
            //var now = DateTime.UtcNow;
            //var diff = now - _lastTime;
            //_lastTime = now;

            //Console.WriteLine($"{diff}");

            //SendMessage();

            //chatTextInput.Focus();

        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            chatTextInput.Focus();
            SendMessage();
        }
    }
}