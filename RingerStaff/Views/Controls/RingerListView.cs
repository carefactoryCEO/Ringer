using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace RingerStaff.Views.Controls
{
    [DesignTimeVisible(true)]
    public class RingerListView : ListView
    {
        public RingerListView() : this(ListViewCachingStrategy.RecycleElement)
        {
        }

        public RingerListView(ListViewCachingStrategy cachingStrategy) : base(cachingStrategy)
        {
        }
    }
}
