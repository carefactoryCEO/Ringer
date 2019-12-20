using System;
using System.ComponentModel;
using System.Diagnostics;
using CoreGraphics;
using Foundation;
using RingerStaff.iOS.Renderers;
using RingerStaff.Views.Controls;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(RingerEditor), typeof(RingerEditorRenderer))]
namespace RingerStaff.iOS.Renderers
{
    public class RingerEditorRenderer : EditorRenderer
    {
        UILabel _placeholderLabel;
        double _previousHeight = -1;
        int _previousLines;

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                //Control.TextContainer.LineBreakMode = UILineBreakMode.CharacterWrap;

                if (_placeholderLabel == null)
                    CreatePlaceholder();
            }

            if (e.NewElement != null)
            {
                var ringerEditor = (RingerEditor)e.NewElement;
                ringerEditor.HeightRequest = -1;

                Control.ScrollEnabled = !ringerEditor.IsExpandable;
                //Control.TextContainer.LineBreakMode = UILineBreakMode.CharacterWrap;


                if (ringerEditor.HasRoundedCorner)
                {
                    Control.Layer.CornerRadius = ringerEditor.CornerRadius;
                    Control.Layer.BorderColor = Color.LightGray.ToCGColor();
                    Control.Layer.BorderWidth = 1;

                    Control.TextContainerInset = new UIEdgeInsets(
                        (nfloat)(Control.TextContainerInset.Top + 0.5),
                        Control.TextContainerInset.Left + 8,
                        (nfloat)(Control.TextContainerInset.Bottom - 0.5),
                        Control.TextContainerInset.Right + 35);
                }
                else
                    Control.Layer.CornerRadius = 0;

                Control.InputAccessoryView = new UIView(CGRect.Empty);
                Control.ReloadInputViews();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var ringerEditor = (RingerEditor)Element;

            if (e.PropertyName == Editor.TextProperty.PropertyName)
            {
                if (ringerEditor.IsExpandable)
                {
                    CGSize size = Control.Text.StringSize(Control.Font, Control.TextContainer.Size, UILineBreakMode.CharacterWrap);

                    int numLines = (int)(size.Height / Control.Font.LineHeight);

                    Debug.WriteLine($"{Control.TextContainer.Size.Width}, {Control.TextContainer.Size.Height}, {size.Width}, {size.Height}, {numLines}");

                    if (_previousLines > numLines)
                        ringerEditor.HeightRequest = -1;

                    else if (string.IsNullOrEmpty(Control.Text))
                        ringerEditor.HeightRequest = -1;

                    _previousLines = numLines;
                }

                _placeholderLabel.Hidden = !string.IsNullOrEmpty(Control.Text);
            }
            else if (e.PropertyName == VisualElement.HeightProperty.PropertyName)
            {
                if (ringerEditor.IsExpandable)
                {
                    CGSize size = Control.Text.StringSize(Control.Font, Control.TextContainer.Size, UILineBreakMode.CharacterWrap);

                    int numLines = (int)(size.Height / Control.Font.LineHeight);

                    if (numLines > 4)
                    {
                        ringerEditor.HeightRequest = _previousHeight;
                        Control.ScrollEnabled = true;
                    }
                    else
                    {
                        _previousHeight = ringerEditor.Height;
                        Control.ScrollEnabled = false;
                    }

                }
            }
            else if (e.PropertyName == RingerEditor.PlaceholderProperty.PropertyName)
            {
                _placeholderLabel.Text = ringerEditor.Placeholder;

            }
            else if (e.PropertyName == RingerEditor.PlaceholderColorProperty.PropertyName)
            {

                _placeholderLabel.TextColor = ringerEditor.PlaceholderColor.ToUIColor();
            }
            else if (e.PropertyName == RingerEditor.HasRoundedCornerProperty.PropertyName)
            {
                if (ringerEditor.HasRoundedCorner)
                {
                    Control.Layer.CornerRadius = ringerEditor.CornerRadius;
                    Control.Layer.BorderColor = Color.Gray.ToCGColor();
                    Control.Layer.BorderWidth = 1;

                    Control.TextContainerInset = new UIEdgeInsets(
                        Control.TextContainerInset.Top,
                        Control.TextContainerInset.Left + 6,
                        Control.TextContainerInset.Bottom,
                        Control.TextContainerInset.Right + 30);
                }
                else
                    Control.Layer.CornerRadius = 0;
            }
            else if (e.PropertyName == RingerEditor.IsExpandableProperty.PropertyName)
            {
                Control.ScrollEnabled = !ringerEditor.IsExpandable;
            }
            else if (e.PropertyName == RingerEditor.CornerRadiusProperty.PropertyName)
            {
                Control.Layer.CornerRadius = ringerEditor.CornerRadius;
            }
        }

        private void CreatePlaceholder()
        {
            var element = Element as RingerEditor;

            _placeholderLabel = new UILabel
            {
                Text = element?.Placeholder,
                TextColor = element.PlaceholderColor.ToUIColor(),
                BackgroundColor = UIColor.Clear
            };

            var edgeInsets = Control.TextContainerInset;
            var lineFragmentPadding = Control.TextContainer.LineFragmentPadding;

            Control.AddSubview(_placeholderLabel);

            var vConstraints = NSLayoutConstraint.FromVisualFormat(
                "V:|-" + edgeInsets.Top + "-[PlaceholderLabel]-" + edgeInsets.Bottom + "-|", 0, new NSDictionary(),
                NSDictionary.FromObjectsAndKeys(
                    new NSObject[] { _placeholderLabel }, new NSObject[] { new NSString("PlaceholderLabel") })
            );

            var hConstraints = NSLayoutConstraint.FromVisualFormat(
                "H:|-" + lineFragmentPadding + "-[PlaceholderLabel]-" + lineFragmentPadding + "-|",
                0, new NSDictionary(),
                NSDictionary.FromObjectsAndKeys(
                    new NSObject[] { _placeholderLabel }, new NSObject[] { new NSString("PlaceholderLabel") })
            );

            _placeholderLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            Control.AddConstraints(hConstraints);
            Control.AddConstraints(vConstraints);
        }
    }
}
