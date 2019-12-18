using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using RingerStaff.Services;
using RingerStaff.Views;
using Plugin.LocalNotification;
using System.Diagnostics;

namespace RingerStaff
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            NotificationCenter.Current.NotificationTapped += OnLocalNotificationTapped;

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        private void OnLocalNotificationTapped(NotificationTappedEventArgs e)
        {
            Debug.WriteLine($"noti data: {e.Data}");
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
