using Xamarin.Forms;
using Ringer.Core;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Push;
using Device = Xamarin.Forms.Device;
using Xamarin.Essentials;

namespace Ringer
{
    public partial class App : Application
    {
        #region private members
        string user;
        string group;
        string chatUrl;
        SignalRService signalR;
        #endregion

        #region Constructor
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            Properties["User"] = user = Device.RuntimePlatform + "-" + new Random().Next(1, 100).ToString();
            Properties["Group"] = group = "Xamarin";
            //Properties["ChatURL"] = chatUrl = "ringerchat.azurewebsites.net"; // debug: localhost:5001

#if DEBUG
            Properties["ChatURL"] = chatUrl = DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
            var https = false;
#else
            Properties["ChatURL"] = chatUrl = "ringerchat.azurewebsites.net";
            var https = true;
#endif

            #region prepare signalR
            DependencyService.Register<SignalRService>();

            signalR = DependencyService.Resolve<SignalRService>();

            signalR.Init(chatUrl, user, group, https);

            // Connection events
            signalR.Closed += (s, e) => signalR.AddLocalMessage(e.Message, e.User);
            signalR.Reconnecting += (s, e) => signalR.AddLocalMessage(e.Message, e.User);
            signalR.Reconnected += (s, e) => signalR.AddLocalMessage(e.Message, e.User);

            // Message events
            signalR.OnEntered += (s, e) => signalR.AddLocalMessage(e.Message, e.User);
            signalR.OnLeft += (s, e) => signalR.AddLocalMessage(e.Message, e.User);
            signalR.OnReceivedMessage += (s, e) => signalR.AddLocalMessage(e.Message, e.User);
            #endregion
        }
        #endregion

        #region Life Cycle Methods
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Potential Code Quality Issues", "RECS0165:Asynchronous methods should return a Task instead of void", Justification = "<Pending>")]
        protected override async void OnStart()
        {
            base.OnStart();

            #region AppCenter
            AppCenter.Start(
            "android=776e5a61-2f89-48c3-95b6-5fa3dde1c708;" +
            "ios=b1b4c859-3d1a-4f7c-bf34-b4e45a2aad65",
            typeof(Analytics), typeof(Crashes), typeof(Push));

            if (await Push.IsEnabledAsync())
            {
                Guid? id = await AppCenter.GetInstallIdAsync();
                Debug.WriteLine("-------------------------");
                Debug.WriteLine(id);
                Debug.WriteLine("-------------------------");
            }
            #endregion

            Debug.WriteLine("App.OnStart");

            if (DesignMode.IsDesignModeEnabled)
                return;

            await signalR.ConnectAsync();
            await signalR.JoinChannelAsync(group, user);
        }

        protected override void OnSleep()
        {
            base.OnSleep();

            //Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            //{
            //    Debug.WriteLine(signalR.HubConnection.State);
            //    return true;
            //});

            Debug.WriteLine("OnSleep");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Potential Code Quality Issues", "RECS0165:Asynchronous methods should return a Task instead of void", Justification = "<Pending>")]
        protected override async void OnResume()
        {

            base.OnResume();

            Debug.WriteLine("OnResume");

            if (DesignMode.IsDesignModeEnabled)
                return;

            await signalR.ConnectAsync();
            await signalR.JoinChannelAsync(group, user);

        }
        #endregion
    }
}
