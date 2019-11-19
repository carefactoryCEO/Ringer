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
            Properties["ChatURL"] = chatUrl = "ringerchat.azurewebsites.net"; // debug: localhost:5001

            #region prepare signalR
            DependencyService.Register<SignalRService>();

            signalR = DependencyService.Resolve<SignalRService>();

            signalR.Init(chatUrl, user, group);

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

            Debug.WriteLine("OnSleep");

            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                Debug.WriteLine("-----------" + resumed + "------------");

                if (resumed)
                {
                    resumed = false;
                    return false;
                }
                else
                {
                    Task.Run(async () =>
                    {
                        await signalR.LeaveChannelAsync(group, user);
                        await signalR.DisconnectAsync();

                        Debug.WriteLine("excuted");
                    });
                }

                return false;
            });

        }

        bool resumed = false;

        protected override async void OnResume()
        {
            resumed = true;

            base.OnResume();

            Debug.WriteLine("OnResume");

            if (DesignMode.IsDesignModeEnabled)
                return;

            await signalR.ConnectAsync();
            resumed = true;

            await signalR.JoinChannelAsync(group, user);
            resumed = true;

        }
        #endregion
    }
}
