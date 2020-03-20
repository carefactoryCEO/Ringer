using Ringer.iOS.Renderers;
using Ringer.Views.Controls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(RingerListView), typeof(RingerListViewRenderer))]
namespace Ringer.iOS.Renderers
{
    public class RingerListViewRenderer : ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);

            //((UITableViewController)ViewController).RefreshControl.TintColor = UIColor.White;

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
