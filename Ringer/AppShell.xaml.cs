﻿using Ringer.Views;
using Xamarin.Forms;

namespace Ringer
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("chatpage", typeof(ChatPage));
            Routing.RegisterRoute("mappage", typeof(MapPage));
            Routing.RegisterRoute("vidyopage", typeof(VidyoPage));
        }
    }
}
