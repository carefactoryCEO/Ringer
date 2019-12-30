using System.ComponentModel;
using Android.Content;
using RingerStaff.Droid.Renderers;
using RingerStaff.Views.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(RingerListView), typeof(RingerListViewRenderer))]
namespace RingerStaff.Droid.Renderers
{
    public class RingerListViewRenderer : ListViewRenderer
    {
        Context _context;

        public RingerListViewRenderer(Context context) : base(context)
        {
            _context = context;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control != null)
                {
                    // TODO: Disable list view selection highlight
                }
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var ringerListView = (RingerListView)Element;

            if (e.PropertyName == VisualElement.HeightProperty.PropertyName)
            {
                Control.ScrollTo(Control.ScrollX, Control.ScrollY);
            }
        }
    }
}
