using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Ringer.Helpers;
using Ringer.Models;
using Xamarin.Forms;

namespace Ringer.Views.Controls
{
    [DesignTimeVisible(true)]
    public class RingerListView : ListView
    {
        int lastAppearedItemIndex = -1;
        private double _previousHeight;

        #region Bindable Properties
        public static readonly BindableProperty LoadCommandProperty =
            BindableProperty.Create(nameof(LoadCommand), typeof(ICommand), typeof(RingerListView), default(ICommand));

        public ICommand LoadCommand
        {
            get { return (ICommand)GetValue(LoadCommandProperty); }
            set { SetValue(LoadCommandProperty, value); }
        }

        public static readonly BindableProperty IsLoadingProperty =
            BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(RingerListView), default(bool));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }
        #endregion

        public RingerListView() : this(ListViewCachingStrategy.RetainElement)
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

            ItemAppearing += RingerListView_ItemAppearing;
        }

        private void RingerListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if (e.ItemIndex == 0 && lastAppearedItemIndex > e.ItemIndex && !IsLoading)
                LoadCommand?.Execute(e.Item);

            var direction = lastAppearedItemIndex > e.ItemIndex ? "upward" : "downward";
            Utilities.Trace($"last index: {lastAppearedItemIndex}, index: {e.ItemIndex}, direction: {direction}");

            lastAppearedItemIndex = (IsLoading) ? -1 : e.ItemIndex;
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
                    Debug.WriteLine(ex.ToString());
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
