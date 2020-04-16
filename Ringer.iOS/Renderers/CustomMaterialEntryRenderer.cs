using System;
using CoreGraphics;
using Ringer.iOS.Renderers;
using Ringer.Views.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Material.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomMaterialEntry), typeof(CustomMaterialEntryRenderer))]
namespace Ringer.iOS.Renderers
{
    public class CustomMaterialEntryRenderer : MaterialEntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.LeftView = new UIKit.UIView(new CGRect(0, 0, 0, 0));
                Control.RightView = new UIKit.UIView(new CGRect(0, 0, 0, 0));
            }
        }
    }
}
