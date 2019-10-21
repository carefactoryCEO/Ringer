using Xamarin.Forms;
using Ringer.Core;
using System.Collections.ObjectModel;
using Ringer.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ringer.Views;

namespace Ringer
{
    public partial class App : Application
    {
        //TODO: Replace with *.azurewebsites.net url after deploying backend to Azure
        //To debug on Android emulators run the web backend against .NET Core not IIS
        //If using other emulators besides stock Google images you may need to adjust the IP address
        //public static string AzureBackendUrl =
        //    DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5000" : "http://localhost:5000";
        //public static bool UseMockDataStore = true;

        public static string User = "김영미";
        public static string Group = "Xamarin";
        public static string ChatURL = "ringerchat.azurewebsites.net";
        public static List<Message> Repository = new List<Message>();

        public App()
        {
            InitializeComponent();

            DependencyService.Register<ChatService>();

            //if (UseMockDataStore)
            //    DependencyService.Register<MockDataStore>();
            //else
            //    DependencyService.Register<AzureDataStore>();

            MainPage = new AppShell();


            //MainPage = new AboutPage();

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
