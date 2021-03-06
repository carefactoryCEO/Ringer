﻿using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.OS;
using Android;
using Microsoft.AppCenter.Push;
using Plugin.LocalNotification;

namespace Ringer.Droid
{
    [Activity(Label = "링거",
        Icon = "@drawable/icon",
        Theme = "@style/RingerTheme.Splash",//"@style/MainTheme",
        LaunchMode = LaunchMode.SingleInstance,
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        WindowSoftInputMode = SoftInput.AdjustNothing)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            // TODO check and remove Shell, Visual, CollectionView experimental
            global::Xamarin.Forms.Forms.SetFlags("MediaElement_Experimental", "FastRenderers_Experimental");

            // essentials
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);


            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true);
            //FFImageLoading.Forms.Platform.CachedImageRenderer.InitImageViewHandler();

            // Local Notification
            NotificationCenter.CreateNotificationChannel(new Plugin.LocalNotification.Platform.Droid.NotificationChannelRequest
            {
                //Sound = Resource.Raw.filling_your_inbox.ToString(),
                Importance = NotificationImportance.Max
            });
            NotificationCenter.NotifyNotificationTapped(Intent);

            // Map
            Xamarin.FormsMaps.Init(this, savedInstanceState);

            // status bar color
            Window.SetStatusBarColor(Android.Graphics.Color.Rgb(56, 79, 129));

            LoadApplication(new App());
        }

        protected override void OnStart()
        {
            base.OnStart();

            //if (CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != (int)Permission.Granted)
            //{
            //    RequestPermissions(new string[] { Manifest.Permission.AccessCoarseLocation, Manifest.Permission.AccessFineLocation }, 1);
            //}

        }

        protected override void OnNewIntent(Android.Content.Intent intent)
        {
            Push.CheckLaunchedFromNotification(this, intent);
            NotificationCenter.NotifyNotificationTapped(intent);
            base.OnNewIntent(intent);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            // camera
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}