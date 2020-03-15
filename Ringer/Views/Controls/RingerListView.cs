using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace Ringer.Views.Controls
{
    [DesignTimeVisible(true)]
    public class RingerListView : ListView
    {
        private double _previousHeight;

        public RingerListView() : this(ListViewCachingStrategy.RecycleElement)
        {
            if (Device.RuntimePlatform == Device.Android)
            {
                SizeChanged += (s, e) =>
                {
                    if (Height < _previousHeight)
                    {
                        Debug.WriteLine("--------------------Keyboard Shown or Editor expanded-----------------------");

                        if (Device.RuntimePlatform == Device.Android)
                            ScrollToLast();
                    }

                    _previousHeight = Height;
                };
            }

            Scrolled += RingerListView_Scrolled;
            ItemAppearing += RingerListView_ItemAppearing;
        }

        int lastAppearedItemIndex = -1;

        private void RingerListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            Debug.WriteLine($"{e.ItemIndex}");

            if (e.ItemIndex == 0 && lastAppearedItemIndex > e.ItemIndex)
                Debug.WriteLine("refresh!");

            lastAppearedItemIndex = e.ItemIndex;
        }

        private void RingerListView_Scrolled(object sender, ScrolledEventArgs e)
        {
            //Debug.WriteLine($"height:{Height}, scrollY: {e.ScrollY}");
        }

        public RingerListView(ListViewCachingStrategy cachingStrategy) : base(cachingStrategy)
        {
        }

        public void ScrollToFirst()
        {

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (ItemsSource != null && ItemsSource.Cast<object>().Count() > 0)
                    {
                        var msg = ItemsSource.Cast<object>().FirstOrDefault();
                        if (msg != null)
                        {
                            ScrollTo(msg, ScrollToPosition.Start, false);
                        }

                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }

            });
        }

        public void ScrollToLast()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (ItemsSource != null && ItemsSource.Cast<object>().Count() > 0)
                    {
                        var msg = ItemsSource.Cast<object>().LastOrDefault();
                        if (msg != null)
                        {
                            ScrollTo(msg, ScrollToPosition.End, false);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }

            });
        }
    }
}
