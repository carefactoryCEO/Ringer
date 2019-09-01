using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Ringer.Droid.Renderers;
using Ringer.Views.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtendedEditorControl), typeof(CustomEditorRenderer))]
namespace Ringer.Droid.Renderers
{
    public class CustomEditorRenderer : EditorRenderer
    {
        bool initial = true;
        Drawable originalBackground;

        public CustomEditorRenderer(Context context) : base(context)
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
                //IntPtr IntPtrtextViewClass = JNIEnv.FindClass(typeof(TextView));
                //IntPtr mCursorDrawableResProperty = JNIEnv.GetFieldID(IntPtrtextViewClass, "mCursorDrawableRes", "I");
                //JNIEnv.SetField(Control.Handle, mCursorDrawableResProperty, 0); // replace 0 with a Resource.Drawable.my_cursor

                if (initial)
                {
                    originalBackground = Control.Background;
                    initial = false;
                }

                // expend limitation to 5 lines
                Control.SetMaxLines(5);
                
                //Control.SetPadding(Control.PaddingLeft + 25, Control.PaddingTop, Control.PaddingRight + 90, Control.PaddingBottom);
                Control.SetPaddingRelative(Control.PaddingLeft + 25, Control.PaddingTop, Control.PaddingRight + 90, Control.PaddingBottom);
            }

            if (e.NewElement != null)
            {
                var customControl = (ExtendedEditorControl)Element;
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

            var customControl = (ExtendedEditorControl)Element;

            if (ExtendedEditorControl.PlaceholderProperty.PropertyName == e.PropertyName)
            {
                Control.Hint = customControl.Placeholder;

            }
            else if (ExtendedEditorControl.PlaceholderColorProperty.PropertyName == e.PropertyName)
            {

                Control.SetHintTextColor(customControl.PlaceholderColor.ToAndroid());

            }
            else if (ExtendedEditorControl.HasRoundedCornerProperty.PropertyName == e.PropertyName)
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
            gd.SetCornerRadius(50);
            gd.SetStroke(2, Color.Gray.ToAndroid());
            this.Control.Background = gd;
        }
    }
}