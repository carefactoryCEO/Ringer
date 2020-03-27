using System;
using Foundation;
using UIKit;
using UserNotifications;

namespace Ringer.iOS
{
    // TODO: [production][ios] info.plist 에서 NSAppTransportSecurity 제거
    // https://docs.microsoft.com/en-us/xamarin/ios/app-fundamentals/ats#opting-out-of-atsThe

    // UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // TODO check and remove Shell, Visual, CollectionView experimental
            //global::Xamarin.Forms.Forms.SetFlags("Shell_Experimental", "Visual_Experimental", "CollectionView_Experimental", "FastRenderers_Experimental", "MediaElement_Experimental");
            global::Xamarin.Forms.Forms.SetFlags("MediaElement_Experimental");

            global::Xamarin.Forms.Forms.Init();

            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();

            Xamarin.FormsMaps.Init();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        public override void WillEnterForeground(UIApplication uiApplication)
        {
            Plugin.LocalNotification.NotificationCenter.ResetApplicationIconBadgeNumber(uiApplication);

            base.WillEnterForeground(uiApplication);
        }
    }
}
