using RingerStaff.iOS.Renderers;
using RingerStaff.Views.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(RingerListView), typeof(RingerListViewRenderer))]
namespace RingerStaff.iOS.Renderers
{
    public class RingerListViewRenderer : ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control != null)
                {
                    Control.AllowsSelection = false;
                    //Control.AlwaysBounceVertical = false;
                    //Control.Bounces = false;
                }
            }
        }
    }
}
