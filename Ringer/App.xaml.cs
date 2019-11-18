using Xamarin.Forms;
using Ringer.Core;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

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
                Debug.WriteLine("-----------"+resumed+"------------");

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
