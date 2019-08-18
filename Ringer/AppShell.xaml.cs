using Ringer.Views;
using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Ringer
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("chatpage", typeof(ChatPage));
        }
    }
}
