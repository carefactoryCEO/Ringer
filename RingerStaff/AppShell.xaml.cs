﻿using RingerStaff.Views;
using Xamarin.Forms;

namespace RingerStaff
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("loginpage", typeof(LoginPage));
            Routing.RegisterRoute("chatpage", typeof(ChatPage));
        }
    }
}