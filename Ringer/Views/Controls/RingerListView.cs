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
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }

            });
        }
    }
}
