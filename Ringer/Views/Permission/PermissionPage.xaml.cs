using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Ringer.Views.Permission
{
    public partial class PermissionPage : ContentPage
    {
        private bool insetCaptured = false;

        public PermissionPage()
        {
            InitializeComponent();
            On<iOS>().SetUseSafeArea(true);
        }

        protected override void OnSizeAllocated(double width, double height)
        {

            //if (insetCaptured)
            //    return;

            //insetCaptured = true;

            //var insets = On<iOS>().SafeAreaInsets();

            //InstructionLabel.Text += $" {insets.Left},{insets.Top},{insets.Right},{insets.Bottom}";
            base.OnSizeAllocated(width, height);
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            Xamarin.Forms.Application.Current.MainPage = new AppShell();
        }

        void Button_Clicked_1(System.Object sender, System.EventArgs e)
        {
            Xamarin.Forms.Application.Current.MainPage = new AppShell();
        }

        void CollectionView_SelectionChanged(System.Object sender, Xamarin.Forms.SelectionChangedEventArgs e)
        {
            var collectionView = sender as CollectionView;
            collectionView.SelectedItem = null;

        }
        //async void Button_Clicked(System.Object sender, System.EventArgs e)
        //{
        //    On<iOS>().SetUseSafeArea(false);
        //    await PermissionView.TranslateTo(0, 0, 250);
        //}

        //async void Button_Clicked_1(System.Object sender, System.EventArgs e)
        //{
        //    On<iOS>().SetUseSafeArea(true);
        //    await PermissionView.TranslateTo(0, 1000, 250);
        //}
    }
}
