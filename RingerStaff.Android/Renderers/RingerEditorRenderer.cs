using System;
using RingerStaff.Views.Controls;
using Ringerstaff.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Widget;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(RingerEditor), typeof(RingerEditorRenderer))]
namespace Ringerstaff.Droid.Renderers
{
    public class RingerEditorRenderer : EditorRenderer
    {
        bool initial = true;
        Drawable originalBackground;

        public RingerEditorRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {

                // hide android default underline
                // https://stackoverflow.com/questions/48003093/xamarin-forms-hide-editor-underline
                Control.SetBackgroundColor(Android.Graphics.Color.Transparent);

                // remove android default cursor color
                // https://stackoverflow.com/questions/45916849/how-to-change-entry-cursor-color-in-xamarin-android
                IntPtr IntPtrtextViewClass = JNIEnv.FindClass(typeof(TextView));
                IntPtr mCursorDrawableResProperty = JNIEnv.GetFieldID(IntPtrtextViewClass, "mCursorDrawableRes", "I");
                JNIEnv.SetField(Control.Handle, mCursorDrawableResProperty, 0); // replace 0 with a Resource.Drawable.my_cursor

                if (initial)
                {
                    originalBackground = Control.Background;
                    initial = false;
                }

                // expend limitation to 5 lines
                Control.SetMaxLines(5);

                //Control.SetPadding(Control.PaddingLeft + 25, Control.PaddingTop, Control.PaddingRight + 90, Control.PaddingBottom);
                Control.SetPaddingRelative(Control.PaddingLeft + 20, Control.PaddingTop - 10, Control.PaddingRight + 95, Control.PaddingBottom - 10);
            }

            if (e.NewElement != null)
            {
                var customControl = (RingerEditor)Element;
                if (customControl.HasRoundedCorner)
                {
                    ApplyBorder();
                }

                if (!string.IsNullOrEmpty(customControl.Placeholder))
                {
                    Control.Hint = customControl.Placeholder;
                    Control.SetHintTextColor(customControl.PlaceholderColor.ToAndroid());

                }
            }


        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var customControl = (RingerEditor)Element;

            if (RingerEditor.PlaceholderProperty.PropertyName == e.PropertyName)
            {
                Control.Hint = customControl.Placeholder;

            }
            else if (RingerEditor.PlaceholderColorProperty.PropertyName == e.PropertyName)
            {

                Control.SetHintTextColor(customControl.PlaceholderColor.ToAndroid());

            }
            else if (RingerEditor.HasRoundedCornerProperty.PropertyName == e.PropertyName)
            {
                if (customControl.HasRoundedCorner)
                {
                    ApplyBorder();

                }
                else
                {
                    this.Control.Background = originalBackground;
                }
            }
        }

        void ApplyBorder()
        {
            GradientDrawable gd = new GradientDrawable();
            gd.SetCornerRadius(45);
            gd.SetStroke(3, Color.LightGray.ToAndroid());
            this.Control.Background = gd;
        }
    }
}
