using Xamarin.Forms;
using RingerStaff.Services;
using Plugin.LocalNotification;
using System.Diagnostics;
using RingerStaff.Views;
using System.Linq;

namespace RingerStaff
{
    public partial class App : Application
    {
        public static string Token = string.Empty;
        public static bool IsLoggedIn => !string.IsNullOrEmpty(Token);

        public App()
        {
            InitializeComponent();

            NotificationCenter.Current.NotificationTapped += OnLocalNotificationTapped;

            DependencyService.Register<MockDataStore>();

            MainPage = new AppShell();

            PageAppearing += App_PageAppearing;
        }

        private void App_PageAppearing(object sender, Page page)
        {
            if (page.Navigation.ModalStack.Count > 0 && page.Navigation.ModalStack.Last() is LoginPage)
                return;

            if (!App.IsLoggedIn)
                page.Navigation.PushModalAsync(new LoginPage());
            //if (IsLoggedIn || page is LoginPage)
            //    return;

            //page.Navigation.PushModalAsync(new LoginPage());
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
